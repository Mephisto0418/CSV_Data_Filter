using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CSV_Data_Filter.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace CSV_Data_Filter.Utils
{
    /// <summary>
    /// CSV 文件處理的輔助類
    /// </summary>
    public class CsvProcessor
    {
        private readonly Action<string> _logAction;
        private readonly CancellationToken _cancellationToken;
        private string _dateFormat;
        private readonly FileSystemHelper _fileSystemHelper;

        // 網路路徑映射字典，記錄網路路徑到本地暫存路徑的映射關係
        private ConcurrentDictionary<string, string> _networkPathMappings = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// 初始化 CSV 處理器
        /// </summary>
        /// <param name="logAction">日誌記錄的動作</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="dateFormat">日期格式</param>
        /// <param name="fileSystemHelper">文件系統助手，若為null則自動創建</param>
        public CsvProcessor(Action<string> logAction, CancellationToken cancellationToken, string dateFormat = "yyyy-MM-dd", FileSystemHelper? fileSystemHelper = null)
        {
            _logAction = logAction;
            _cancellationToken = cancellationToken;
            _dateFormat = dateFormat;
            _fileSystemHelper = fileSystemHelper ?? new FileSystemHelper(logAction, cancellationToken, dateFormat);
        }

        /// <summary>
        /// 添加網路路徑映射，記錄網路路徑與本地暫存路徑的對應關係
        /// </summary>
        public void AddNetworkPathMapping(string networkPath, string localPath)
        {
            if (!string.IsNullOrEmpty(networkPath) && !string.IsNullOrEmpty(localPath))
            {
                _networkPathMappings[networkPath] = localPath;
                _logAction($"已記錄網路路徑映射: {networkPath} -> {localPath}");
            }
        }

        /// <summary>
        /// 添加多個網路路徑映射
        /// </summary>
        public void AddNetworkPathMappings(ConcurrentDictionary<string, string> mappings)
        {
            if (mappings != null && mappings.Count > 0)
            {
                foreach (var mapping in mappings)
                {
                    _networkPathMappings[mapping.Key] = mapping.Value;
                }
            }
        }

        /// <summary>
        /// 獲取網路檔案的本地映射路徑，如果不是網路檔案則返回原路徑
        /// </summary>
        private string GetNetworkFileMappingPath(string filePath)
        {
            // 使用 FileSystemHelper 中的方法處理網路路徑映射
            return _fileSystemHelper.GetNetworkFileMappingPath(filePath, _networkPathMappings);
        }

        /// <summary>
        /// 從CSV文件中獲取列標題
        /// </summary>
        /// <param name="filePath">CSV文件路徑</param>
        /// <returns>列標題數組</returns>
        public string[]? GetCsvHeaders(string filePath)
        {
            try
            {
                var inputConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null
                };

                using var reader = new StreamReader(filePath);
                using var csv = new CsvReader(reader, inputConfig);

                csv.Read();
                csv.ReadHeader();
                return csv.HeaderRecord;
            }
            catch (Exception ex)
            {
                _logAction($"獲取CSV標頭時出錯: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 處理單個CSV文件
        /// </summary>
        /// <param name="filePath">CSV文件路徑</param>
        /// <param name="columnConfigs">欄位配置列表（包含自訂名稱）</param>
        /// <param name="filterConditions">過濾條件</param>
        /// <param name="addFileName">是否添加文件名列</param>
        /// <param name="addDirectoryName">是否添加目錄名稱列</param>
        /// <param name="tempDir">臨時目錄</param>
        /// <param name="skipIncompleteFiles">是否跳過缺少欄位的檔案</param>
        /// <returns>臨時文件路徑，處理失敗則返回null</returns>
        public async Task<string?> ProcessCsvFileAsync(string filePath, List<ColumnConfig> columnConfigs,
            List<FilterCondition> filterConditions, bool addFileName, bool addDirectoryName, string tempDir, bool skipIncompleteFiles = false)
        {
            try
            {
                // 檢查文件是否來自網路路徑，如果是則使用映射後的本地暫存路徑
                string actualFilePath = GetNetworkFileMappingPath(filePath);
                string originalFilePath = filePath; // 保留原始路徑用於顯示和輸出

                if (actualFilePath != filePath)
                {
                    _logAction($"使用本地暫存路徑處理檔案: {actualFilePath}");
                }

                // 檢查文件是否存在
                if (!File.Exists(actualFilePath))
                {
                    _logAction($"找不到檔案: {filePath}");
                    return null;
                }                
                // 尋找標題行（最多檢查前100行，必須完全匹配所有必要欄位）
                var (hasValidHeader, headerFields) = await FindValidHeaderRowAsync(actualFilePath, columnConfigs, 100);
                if (!hasValidHeader || headerFields == null)
                {
                    _logAction($"跳過檔案: {Path.GetFileName(filePath)} - 未找到包含所有必要欄位的標題行");
                    return null;
                }
                
                // 標題檢查已經確認包含所有需要的欄位，不需要二次檢查

                // 建立臨時輸出檔案路徑
                string outputPath = Path.Combine(tempDir, $"temp_{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.Now.Ticks}.csv");

                // 記錄處理進度
                long totalProcessed = 0;
                const int batchSize = 10000; // 每次處理10000行，避免記憶體問題

                using (var csvWriter = new CsvWriter(new StreamWriter(outputPath), new CsvConfiguration(CultureInfo.InvariantCulture)))
                {                // 寫入標頭行（使用原始欄位名稱，不使用自訂名稱）
                    foreach (var column in columnConfigs)
                    {
                        csvWriter.WriteField(column.Name);
                    }

                    // 添加文件名列標題
                    if (addFileName)
                    {
                        csvWriter.WriteField("FileName");
                    }

                    // 添加目錄名稱列標題
                    if (addDirectoryName)
                    {
                        csvWriter.WriteField("DirectoryName");
                    }

                    await csvWriter.NextRecordAsync();

                    // 讀取並處理數據
                    using (var reader = new StreamReader(actualFilePath))
                    {
                        // 分批讀取並處理
                        List<string> lines = new List<string>(batchSize);
                        string? line;

                        // 讀取每一行並處理
                        bool headerFound = false;
                        int lineNumber = 0;

                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            lineNumber++;

                            // 如果還沒找到標題行，檢查當前行是否是標題行
                            if (!headerFound)
                            {
                                var parsedValues = ParseCsvLine(line);
                                if (IsHeaderRow(parsedValues, columnConfigs))
                                {
                                    headerFound = true;
                                    continue; // 跳過標題行
                                }
                                // 如果不是標題行並且還在標題搜尋階段，繼續讀取下一行
                                if (lineNumber <= 100)
                                {
                                    continue;
                                }
                                // 如果超過100行還沒找到標題行，使用之前找到的有效標題
                                headerFound = true;
                            }

                            // 已找到標題行，開始處理實際數據
                            lines.Add(line);

                            if (lines.Count >= batchSize)
                            {
                                await ProcessBatchAsync(lines, headerFields, columnConfigs, filterConditions,
                                    addFileName, addDirectoryName, originalFilePath, csvWriter);                                totalProcessed += lines.Count;
                                if (totalProcessed % 100000 == 0)
                                {
                                    _logAction($"處理 {Path.GetFileName(filePath)}: 已處理 {totalProcessed/1000}K 行");
                                    // 這裡可以添加進度回調
                                    // progressCallback?.Invoke(current, total);
                                    
                                    // 定期暫停，減少連續大量I/O操作
                                    await Task.Delay(20); // 短暫暫停20毫秒
                                }

                                lines.Clear();
                            }

                            if (_cancellationToken.IsCancellationRequested)
                            {
                                return null;
                            }
                        }

                        // 處理最後一批
                        if (lines.Count > 0)
                        {
                            await ProcessBatchAsync(lines, headerFields, columnConfigs, filterConditions,
                                addFileName, addDirectoryName, originalFilePath, csvWriter);
                            totalProcessed += lines.Count;
                        }
                    }                
                }

                return outputPath;
            }
            catch (Exception ex)
            {
                _logAction($"處理檔案 {Path.GetFileName(filePath)} 時出錯: {ex.Message}");
                return null;
            }
        }        /// <summary>
        /// 檢查前N行，尋找包含所有必要欄位的標題行（必須完全匹配）
        /// </summary>
        private async Task<(bool success, List<string>? headerFields)> FindValidHeaderRowAsync(string filePath, List<ColumnConfig> requiredColumns, int maxLinesToCheck)
        {
            try
            {
                using var reader = new StreamReader(filePath);

                // 第一次掃描：尋找完全匹配的標題行
                for (int i = 0; i < maxLinesToCheck; i++)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;

                    var values = ParseCsvLine(line);
                    if (IsHeaderRow(values, requiredColumns))
                    {
                        return (true, values);
                    }
                }

                // 如果找不到完整的標題行，再次嘗試尋找最接近的一行（僅用於日誌報告，不會使用部分匹配的標題）
                reader.BaseStream.Position = 0;
                reader.DiscardBufferedData();

                int bestMatchCount = 0;
                int bestMatchLineNumber = -1;
                
                for (int i = 0; i < maxLinesToCheck && i < 10; i++)  // 只檢查前10行就足夠了
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;

                    var values = ParseCsvLine(line);
                    int matchCount = CountMatchingColumns(values, requiredColumns);

                    if (matchCount > bestMatchCount)
                    {
                        bestMatchCount = matchCount;
                        bestMatchLineNumber = i + 1;
                    }
                }
                
                // 只記錄最佳匹配的結果，但仍然拒絕不完全匹配的檔案
                if (bestMatchCount > 0)
                {
                    _logAction($"找到最佳匹配 {bestMatchCount}/{requiredColumns.Count} 個欄位，但不符合完全匹配要求");
                }
                else
                {
                    _logAction($"該檔案不包含任何所需的欄位");
                }
                
                // 返回失敗
                return (false, null);
            }
            catch (Exception ex)
            {
                _logAction($"檢查標題行時出錯: {ex.Message}");
                return (false, null);
            }
        }

        /// <summary>
        /// 檢查給定的值列表是否包含所有必要的欄位
        /// </summary>
        private bool IsHeaderRow(List<string> values, List<ColumnConfig> requiredColumns)
        {
            // 檢查是否所有必要欄位都存在於values中
            foreach (var column in requiredColumns)
            {
                bool found = false;
                foreach (var value in values)
                {
                    // 不區分大小寫比較，使用原始欄位名稱而非自訂名稱
                    if (string.Equals(value.Trim(), column.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 計算匹配的欄位數量
        /// </summary>
        private int CountMatchingColumns(List<string> values, List<ColumnConfig> requiredColumns)
        {
            int count = 0;
            foreach (var column in requiredColumns)
            {
                foreach (var value in values)
                {
                    // 不區分大小寫比較，使用原始欄位名稱而非自訂名稱
                    if (string.Equals(value.Trim(), column.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        count++;
                        break;
                    }
                }
            }
            return count;
        }

        /// <summary>
        /// 批次處理CSV行
        /// </summary>
        private async Task ProcessBatchAsync(List<string> lines, List<string> headers, List<ColumnConfig> columnConfigs,
            List<FilterCondition> filterConditions, bool addFileName, bool addDirectoryName, string filePath, CsvWriter csvWriter)
        {
            if (headers == null || headers.Count == 0)
            {
                _logAction("警告：沒有有效的標題行，無法正確對應欄位。");
                return;
            }

            foreach (var line in lines)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                // 解析CSV行
                var values = ParseCsvLine(line);
                if (values.Count == 0)
                {
                    continue; // 跳過空行
                }                

                // 創建欄位字典，用於方便地按名稱查找值
                var fieldDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                  // 更精確地處理標題和值的映射
                // 如果值的數量小於標題數量，只映射可用的值
                // 如果值的數量大於標題數量，考慮可能的欄位偏移情況
                
                int validFieldCount = Math.Min(headers.Count, values.Count);
                for (int i = 0; i < validFieldCount; i++)
                {
                    if (!string.IsNullOrEmpty(headers[i])) // 只處理有效的標題名
                    {
                        var headerName = headers[i].Trim();
                        var value = values[i];
                        
                        // 檢查此欄位名稱是否已在字典中
                        if (fieldDict.ContainsKey(headerName))
                        {
                            // 不需要每行都記錄重複欄位警告
                        }
                        
                        fieldDict[headerName] = value;
                    }
                }
                
                // 如果欄位數量不同，嘗試使用欄位名稱進行匹配
                // 這有助於處理CSV文件中欄位順序與預期不同的情況
                if (values.Count > headers.Count)
                {
                    // 檢查額外的欄位是否與已知的欄位名稱匹配
                    for (int i = headers.Count; i < values.Count; i++)
                    {
                        // 這裡保留欄位，但不進行額外的映射，因為我們不確定這些額外欄位的名稱
                    }
                }

                // 檢查過濾條件（使用原始欄位名稱）
                bool includeRecord = true;
                foreach (var condition in filterConditions)
                {
                    // 使用原始欄位名稱查找
                    if (fieldDict.TryGetValue(condition.ColumnName, out string? value))
                    {
                        if (!EvaluateCondition(value ?? "", condition))
                        {
                            includeRecord = false;
                            break;
                        }
                    }                }
                if (!includeRecord) continue;
                
                // 寫入選擇的列（按配置輸出，使用原始名稱查找值，使用自訂名稱或原始名稱作為輸出標頭）
                foreach (var config in columnConfigs)
                {
                    string value = "";                    // 使用原始欄位名稱查找資料，忽略大小寫
                    
                    // 嘗試直接匹配
                    if (fieldDict.TryGetValue(config.Name, out string? fieldValue))
                    {
                        value = fieldValue ?? "";
                    }
                    else
                    {
                        // 嘗試忽略大小寫匹配
                        foreach (var key in fieldDict.Keys)
                        {
                            if (string.Equals(key.Trim(), config.Name.Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                value = fieldDict[key] ?? "";
                                break;
                            }
                        }
                    }
                    
                    // 根據配置處理欄位值
                    string processedValue = ProcessColumnValue(value, config);
                    
                    csvWriter.WriteField(processedValue);
                }

                // 添加文件名列
                if (addFileName)
                {
                    csvWriter.WriteField(Path.GetFileName(filePath));
                }

                // 添加目錄名稱列
                if (addDirectoryName)
                {
                    csvWriter.WriteField(Path.GetFileName(Path.GetDirectoryName(filePath) ?? ""));
                }

                await csvWriter.NextRecordAsync();
            }
        }        /// <summary>
        /// 解析CSV行（考慮引號、逗號、轉義字符和不同的行尾）
        /// </summary>
        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(line))
                return result;

            bool inQuotes = false;
            var currentValue = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 轉義的引號
                        currentValue.Append('"');
                        i++;
                    }
                    else
                    {
                        // 切換引號狀態
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // 欄位分隔符
                    result.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else if ((c == '\r' || c == '\n') && !inQuotes)
                {
                    // 行尾符號，如果不在引號內，可能是錯誤的格式，但我們會嘗試處理
                    if (c == '\r' && i + 1 < line.Length && line[i + 1] == '\n')
                    {
                        i++; // 跳過 \r\n 的 \n 部分
                    }
                    // 在這裡不添加分隔符，因為這是行尾
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            // 添加最後一個欄位
            result.Add(currentValue.ToString());
            
            // 檢查是否有空欄位需要修復
            for (int i = 0; i < result.Count; i++)
            {
                // 移除引號包圍的欄位中的引號
                string value = result[i];
                if (value.Length >= 2 && value.StartsWith("\"") && value.EndsWith("\""))
                {
                    result[i] = value.Substring(1, value.Length - 2);
                }
            }

            return result;
        }

        /// <summary>
        /// 評估過濾條件
        /// </summary>
        private bool EvaluateCondition(string value, FilterCondition condition)
        {
            var val = value?.Trim() ?? "";
            var condVal = condition.Value?.Trim() ?? "";
            var comparison = StringComparison.OrdinalIgnoreCase;

            switch (condition.Operator)
            {
                case FilterOperator.Contains:
                    return val.IndexOf(condVal, comparison) >= 0;
                case FilterOperator.NotContains:
                    return val.IndexOf(condVal, comparison) < 0;
                case FilterOperator.Equals:
                    return val.Equals(condVal, comparison);
                case FilterOperator.NotEquals:
                    return !val.Equals(condVal, comparison);
                case FilterOperator.StartsWith:
                    return val.StartsWith(condVal, comparison);
                case FilterOperator.EndsWith:
                    return val.EndsWith(condVal, comparison);
                case FilterOperator.GreaterThan:
                    if (decimal.TryParse(val, out decimal decValue) &&
                        decimal.TryParse(condVal, out decimal decCondition))
                        return decValue > decCondition;
                    return false;
                case FilterOperator.LessThan:
                    if (decimal.TryParse(val, out decimal decValue2) &&
                        decimal.TryParse(condVal, out decimal decCondition2))
                        return decValue2 < decCondition2;
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// 根據欄位配置處理欄位值
        /// </summary>
        private string ProcessColumnValue(string value, ColumnConfig config)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            switch (config.ProcessType)
            {
                case ProcessType.Substring:
                    if (config.StartIndex < value.Length)
                    {
                        int length = config.SubstringLength > 0 ?
                            Math.Min(config.SubstringLength, value.Length - config.StartIndex) :
                            value.Length - config.StartIndex;
                        return value.Substring(config.StartIndex, length);
                    }
                    return "";

                case ProcessType.Math:
                    if (decimal.TryParse(value, out decimal numValue))
                    {
                        return (numValue * config.MultiplyBy + config.AddValue).ToString();
                    }
                    return value;

                case ProcessType.Replace:
                    if (!string.IsNullOrEmpty(config.FindText))
                    {
                        return value.Replace(config.FindText, config.ReplaceText ?? "");
                    }
                    return value;

                case ProcessType.Regex:
                    if (!string.IsNullOrEmpty(config.RegexPattern))
                    {
                        try
                        {
                            return System.Text.RegularExpressions.Regex.Replace(value, config.RegexPattern, "");
                        }
                        catch
                        {
                            return value;
                        }
                    }
                    return value;

                default:
                    return value;
            }
        }

        /// <summary>
        /// 生成唯一的檔案名稱，如果檔案已存在則添加時間戳記
        /// </summary>
        public string GenerateUniqueFileName(string targetPath, string baseFileName)
        {
            string fullPath = Path.Combine(targetPath, baseFileName);

            if (!File.Exists(fullPath))
            {
                return fullPath;
            }

            // 檔案已存在，添加時間戳記
            string nameWithoutExt = Path.GetFileNameWithoutExtension(baseFileName);
            string extension = Path.GetExtension(baseFileName);
            string timestamp = DateTime.Now.ToString("_yyyyMMdd_HHmmss");
            string newFileName = $"{nameWithoutExt}{timestamp}{extension}";

            return Path.Combine(targetPath, newFileName);
        }        /// <summary>
                 /// 合併多個CSV文件
                 /// </summary>
                 /// <param name="tempFiles">要合併的臨時文件列表</param>
                 /// <param name="outputPath">輸出路徑</param>
                 /// <param name="columnConfigs">欄位配置列表（包含自訂名稱）</param>
                 /// <returns>輸出文件的路徑</returns>
        public async Task<string> MergeCsvFilesAsync(List<string> tempFiles, string outputPath, List<ColumnConfig>? columnConfigs = null)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true
                };

                bool isFirstFile = true;
                using (var writer = new StreamWriter(outputPath))
                using (var csvWriter = new CsvWriter(writer, config))
                {
                    foreach (var tempFile in tempFiles)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        using var reader = new StreamReader(tempFile);
                        using var csvReader = new CsvReader(reader, config);
                        // 讀取表頭
                        if (!await csvReader.ReadAsync())
                        {
                            _logAction($"警告：檔案 {tempFile} 為空，已跳過");
                            continue;
                        }

                        csvReader.ReadHeader();

                        // 確保 HeaderRecord 有效
                        if (csvReader.HeaderRecord == null || csvReader.HeaderRecord.Length == 0)
                        {
                            _logAction($"警告：檔案 {tempFile} 的標頭無效，已跳過");
                            continue;
                        }                        // 寫入第一個文件的表頭（使用自訂欄位名稱）
                        if (isFirstFile)
                        {
                            // 獲取原始標題與列的對應
                            var headers = csvReader.HeaderRecord;

                            // 尋找對應的自訂欄位名稱並輸出
                            foreach (var header in headers)
                            {
                                // 尋找這個欄位是否有自訂名稱配置
                                var matchingConfig = columnConfigs?.FirstOrDefault(c =>
                                    string.Equals(c.Name.Trim(), header.Trim(), StringComparison.OrdinalIgnoreCase));

                                // 如果找到配置且有自訂名稱，則使用自訂名稱；否則使用原始名稱
                                string outputName = matchingConfig?.CustomName ?? "";
                                if (string.IsNullOrEmpty(outputName))
                                {
                                    outputName = header; // 沒有自訂名稱，使用原始名稱
                                }

                                csvWriter.WriteField(outputName);
                            }

                            await csvWriter.NextRecordAsync();
                            isFirstFile = false;
                        }

                        // 複製數據行
                        while (await csvReader.ReadAsync())
                        {
                            if (_cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            if (csvReader.HeaderRecord != null)
                            {
                                foreach (var header in csvReader.HeaderRecord)
                                {
                                    csvWriter.WriteField(csvReader.GetField(header));
                                }
                                await csvWriter.NextRecordAsync();
                            }
                        }
                    }
                }

                return outputPath;
            }
            catch (Exception ex)
            {
                _logAction($"合併CSV文件時出錯: {ex.Message}");
                throw;
            }
        }        /// <summary>
                 /// 高效合併多個CSV文件（使用直接文件流複製，避免逐行CSV解析）
                 /// </summary>
                 /// <param name="tempFiles">要合併的臨時文件清單</param>
                 /// <param name="outputPath">輸出文件路徑</param>
                 /// <param name="columnConfigs">欄位配置列表（包含自訂名稱）</param>
                 /// <param name="progressCallback">進度回調函數</param>
                 /// <returns>合併後的文件路徑</returns>
        public async Task<string> MergeCsvFilesOptimizedAsync(List<string> tempFiles, string outputPath,
            List<ColumnConfig>? columnConfigs = null, Action<int, int>? progressCallback = null)
        {
            if (tempFiles.Count == 0)
                throw new ArgumentException("沒有要合併的文件");

            try
            {
                // 建立一個較大的緩衝區來提高I/O效率
                const int bufferSize = 81920; // 80KB緩衝區
                using var outputWriter = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8, bufferSize);
                bool isFirstFile = true;
                int processedCount = 0;

                // 預估總檔案大小，以便更合理地報告進度
                long totalBytes = 0;
                long processedBytes = 0;

                foreach (var tempFile in tempFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(tempFile);
                        if (fileInfo.Exists)
                        {
                            totalBytes += fileInfo.Length;
                        }
                    }
                    catch { /* 忽略檔案大小計算錯誤 */ }
                }

                _logAction($"開始合併 {tempFiles.Count} 個檔案，總計約 {totalBytes / (1024 * 1024)} MB");

                // 限制每批處理的檔案數
                const int batchSize = 100; // 每批處理100個檔案
                int batchCount = (int)Math.Ceiling(tempFiles.Count / (double)batchSize);

                for (int batchIndex = 0; batchIndex < batchCount; batchIndex++)
                {
                    // 取得當前批次的檔案
                    var batchFiles = tempFiles.Skip(batchIndex * batchSize).Take(batchSize).ToList();

                    // 處理每個批次的檔案
                    foreach (var tempFile in batchFiles)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                            break;

                        if (tempFile == null || !File.Exists(tempFile))
                        {
                            _logAction($"警告: 找不到暫存檔案 {(tempFile == null ? "null" : Path.GetFileName(tempFile))}，已跳過");
                            continue;
                        }

                        // 使用緩衝區讀取檔案
                        using var inputReader = new StreamReader(tempFile, System.Text.Encoding.UTF8, true, bufferSize);
                        if (isFirstFile)
                        {
                            // 第一個文件：讀取標頭行，應用自訂欄位名稱，然後讀取剩餘行
                            string? headerLine = await inputReader.ReadLineAsync();
                            if (headerLine != null)
                            {
                                // 如果有自訂欄位名稱配置，則進行欄位名稱映射
                                if (columnConfigs != null && columnConfigs.Count > 0)
                                {
                                    var headers = headerLine.Split(',').Select(h => h.Trim('"')).ToList();
                                    var mappedHeaders = new List<string>();

                                    foreach (var header in headers)
                                    {
                                        // 尋找這個欄位是否有自訂名稱配置
                                        var matchingConfig = columnConfigs.FirstOrDefault(c =>
                                            string.Equals(c.Name.Trim(), header.Trim(), StringComparison.OrdinalIgnoreCase));

                                        // 如果找到配置且有自訂名稱，則使用自訂名稱；否則使用原始名稱
                                        string outputName = matchingConfig?.CustomName ?? "";
                                        if (string.IsNullOrEmpty(outputName))
                                        {
                                            outputName = header; // 沒有自訂名稱，使用原始名稱
                                        }

                                        mappedHeaders.Add($"\"{outputName}\"");
                                    }

                                    // 寫入映射後的標頭
                                    string newHeaderLine = string.Join(",", mappedHeaders);
                                    await outputWriter.WriteLineAsync(newHeaderLine);
                                    processedBytes += newHeaderLine.Length + 2; // +2 for newline chars
                                }
                                else
                                {
                                    // 如果沒有自訂欄位名稱，直接寫入原始標頭
                                    await outputWriter.WriteLineAsync(headerLine);
                                    processedBytes += headerLine.Length + 2;
                                }
                            }                            // 複製剩餘的資料行
                            string? line;
                            int lineCount = 0;
                            int logInterval = 100000; // 每處理10萬行記錄一次日誌
                            
                            while ((line = await inputReader.ReadLineAsync()) != null)
                            {
                                lineCount++;
                                
                                // 處理資料行
                                try {
                                    // 只解析需要檢查的行
                                    if (columnConfigs != null && columnConfigs.Count > 0 && (lineCount % 100000 == 0))
                                    {
                                        var values = ParseCsvLine(line);
                                        if (values.Count == 0)
                                        {
                                            continue; // 跳過空行
                                        }
                                    }
                                    
                                    // 寫入行
                                    await outputWriter.WriteLineAsync(line);
                                    processedBytes += line.Length + 2; // +2 for newline chars
                                    
                                    // 定期記錄處理進度
                                    if (lineCount % logInterval == 0)
                                    {
                                        _logAction($"處理 {Path.GetFileName(tempFile)} 中: 已處理 {lineCount/1000}K 行");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // 只記錄重大錯誤
                                    if (lineCount % 10000 == 0 || lineCount < 10)
                                    {
                                        _logAction($"處理 {Path.GetFileName(tempFile)} 第 {lineCount} 行時出錯: {ex.Message}");
                                    }
                                    // 仍然寫入該行以確保連續性
                                    await outputWriter.WriteLineAsync(line);
                                    processedBytes += line.Length + 2;
                                }
                            }
                            isFirstFile = false;
                        }
                        else
                        {
                            // 其他文件：跳過標頭行，只複製資料行
                            await inputReader.ReadLineAsync(); // 跳過標頭行

                            // 使用更有效率的方式複製檔案內容
                            string? line;
                            while ((line = await inputReader.ReadLineAsync()) != null)
                            {
                                if (_cancellationToken.IsCancellationRequested)
                                    break;

                                await outputWriter.WriteLineAsync(line);
                                processedBytes += line.Length + 2; // +2 for newline chars
                            }
                        }

                        processedCount++;                        // 每處理一定數量的檔案，更新進度並暫停一下以減少連續I/O操作
                        if (processedCount % 10 == 0 || processedCount == tempFiles.Count)
                        {
                            // 報告進度
                            double percentComplete = totalBytes > 0 ?
                                Math.Min(100, processedBytes * 100.0 / totalBytes) :
                                (processedCount * 100.0 / tempFiles.Count);

                            _logAction($"合併進度: {processedCount}/{tempFiles.Count} 檔案 ({percentComplete:F1}%)");
                            progressCallback?.Invoke(processedCount, tempFiles.Count);

                            // 短暫暫停，減少連續I/O操作
                            await Task.Delay(20);

                            // 定期刷新資料到磁盤
                            await outputWriter.FlushAsync();
                        }
                    }

                    // 每批檔案處理完後，暫停一下並強制GC回收
                    if (batchIndex < batchCount - 1)
                    {
                        await Task.Delay(100);
                        GC.Collect(2, GCCollectionMode.Forced, false);
                    }
                }

                await outputWriter.FlushAsync();
                _logAction($"合併完成: 總共合併了 {processedCount} 個檔案，約 {processedBytes / (1024 * 1024)} MB");
                return outputPath;
            }
            catch (Exception ex)
            {
                _logAction($"合併CSV文件時出錯: {ex.Message}");
                throw;
            }
        }
    }
}
