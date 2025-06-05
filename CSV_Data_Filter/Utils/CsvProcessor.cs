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
        
        // 網路路徑映射字典，記錄網路路徑到本地暫存路徑的映射關係
        private readonly ConcurrentDictionary<string, string> _networkPathMappings = new ConcurrentDictionary<string, string>();
        
        /// <summary>
        /// 初始化 CSV 處理器
        /// </summary>
        /// <param name="logAction">日誌記錄的動作</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="dateFormat">日期格式</param>
        public CsvProcessor(Action<string> logAction, CancellationToken cancellationToken, string dateFormat = "yyyy-MM-dd")
        {
            _logAction = logAction;
            _cancellationToken = cancellationToken;
            _dateFormat = dateFormat;
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
                _logAction($"已添加 {mappings.Count} 個網路路徑映射");
            }
        }
        
        /// <summary>
        /// 獲取網路檔案的本地映射路徑，如果不是網路檔案則返回原路徑
        /// </summary>
        private string GetNetworkFileMappingPath(string filePath)
        {
            // 判斷文件是否來自網路路徑
            bool isNetworkFile = filePath.StartsWith("\\\\");
            if (!isNetworkFile)
                return filePath;
            
            // 檢查是否有直接的路徑映射
            if (_networkPathMappings.TryGetValue(filePath, out string? localPath))
                return localPath;
            
            // 嘗試查找父目錄的映射
            foreach (var mapping in _networkPathMappings)
            {
                string networkParentPath = mapping.Key;
                string localParentPath = mapping.Value;
                
                // 如果網路檔案路徑是某個已映射網路目錄的子路徑
                if (filePath.StartsWith(networkParentPath, StringComparison.OrdinalIgnoreCase))
                {
                    // 計算相對路徑並組合成本地暫存路徑
                    string relativePath = filePath.Substring(networkParentPath.Length).TrimStart('\\', '/');
                    string mappedLocalPath = Path.Combine(localParentPath, relativePath);
                    
                    _logAction($"找到網路檔案的本地映射: {filePath} -> {mappedLocalPath}");
                    return mappedLocalPath;
                }
            }
            
            // 如果找不到映射，返回原始路徑
            return filePath;
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

                // 尋找標題行（最多檢查前100行，找到包含所有必要欄位的行）
                var (hasValidHeader, headerFields) = await FindValidHeaderRowAsync(actualFilePath, columnConfigs, 100);
                if (!hasValidHeader || headerFields == null)
                {
                    if (skipIncompleteFiles)
                    {
                        _logAction($"跳過不完整檔案: {Path.GetFileName(filePath)} - 找不到完整的標題行");
                        return null;
                    }
                    
                    _logAction($"警告: 檔案 {Path.GetFileName(filePath)} 找不到完整的標題行，將使用最佳匹配行");
                    // 如果找不到標題行但選擇繼續處理，則使用欄位配置名稱作為標題
                    headerFields = columnConfigs.Select(c => c.Name).ToList();
                }
                
                // 建立臨時輸出檔案路徑
                string outputPath = Path.Combine(tempDir, $"temp_{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.Now.Ticks}.csv");
                
                // 記錄處理進度
                long totalProcessed = 0;
                const int batchSize = 10000; // 每次處理10000行，避免記憶體問題
                
                using (var csvWriter = new CsvWriter(new StreamWriter(outputPath), new CsvConfiguration(CultureInfo.InvariantCulture)))
                {
                    // 寫入標頭行
                    foreach (var column in columnConfigs)
                    {
                        csvWriter.WriteField(column.CustomName ?? column.Name);
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
                                    _logAction($"在第 {lineNumber} 行找到標題行");
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
                                    addFileName, addDirectoryName, originalFilePath, csvWriter);
                                
                                totalProcessed += lines.Count;
                                if (totalProcessed % 50000 == 0)
                                {
                                    _logAction($"已處理 {totalProcessed} 行 - {Path.GetFileName(filePath)}");
                                }
                                
                                lines.Clear();
                                
                                // 定期暫停，減少連續大量I/O操作
                                if (totalProcessed % 100000 == 0)
                                {
                                    await Task.Delay(50); // 短暫暫停50毫秒
                                }
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
                
                _logAction($"完成處理檔案: {Path.GetFileName(filePath)} - 共 {totalProcessed} 行");
                return outputPath;
            }
            catch (Exception ex)
            {
                _logAction($"處理檔案 {Path.GetFileName(filePath)} 時出錯: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 檢查前N行，尋找包含所有必要欄位的標題行
        /// </summary>
        private async Task<(bool success, List<string>? headerFields)> FindValidHeaderRowAsync(string filePath, List<ColumnConfig> requiredColumns, int maxLinesToCheck)
        {
            try
            {
                using var reader = new StreamReader(filePath);
                
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
                
                // 如果找不到完整的標題行，再次嘗試尋找最接近的一行
                reader.BaseStream.Position = 0;
                reader.DiscardBufferedData();
                
                int bestMatchCount = 0;
                List<string>? bestMatchHeader = null;
                
                for (int i = 0; i < maxLinesToCheck; i++)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;
                    
                    var values = ParseCsvLine(line);
                    int matchCount = CountMatchingColumns(values, requiredColumns);
                    
                    if (matchCount > bestMatchCount)
                    {
                        bestMatchCount = matchCount;
                        bestMatchHeader = values;
                    }
                    
                    // 如果找到足夠匹配的欄位（例如，超過80%的必要欄位），就使用該行
                    if (bestMatchCount >= requiredColumns.Count * 0.8)
                    {
                        return (true, bestMatchHeader);
                    }
                }
                
                // 如果找到的標題行包含一半以上的必要欄位，也接受使用
                if (bestMatchHeader != null && bestMatchCount >= requiredColumns.Count * 0.5)
                {
                    return (true, bestMatchHeader);
                }
                
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
                    // 不區分大小寫比較
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
                    // 不區分大小寫比較
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
            foreach (var line in lines)
            {
                if (_cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                
                // 解析CSV行
                var values = ParseCsvLine(line);
                if (values.Count == 0 || values.Count != headers.Count)
                {
                    continue; // 跳過格式不正確的行
                }
                
                // 創建欄位字典，用於方便地按名稱查找值
                var fieldDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < Math.Min(headers.Count, values.Count); i++)
                {
                    fieldDict[headers[i]] = values[i];
                }
                
                // 檢查過濾條件
                bool includeRecord = true;
                foreach (var condition in filterConditions)
                {
                    if (fieldDict.TryGetValue(condition.ColumnName, out string? value))
                    {
                        if (!EvaluateCondition(value ?? "", condition))
                        {
                            includeRecord = false;
                            break;
                        }
                    }
                }
                if (!includeRecord) continue;
                
                // 寫入選擇的列
                foreach (var config in columnConfigs)
                {
                    string value = "";
                    if (fieldDict.TryGetValue(config.Name, out string? fieldValue))
                    {
                        value = fieldValue ?? "";
                    }
                    
                    // 根據配置處理欄位值
                    value = ProcessColumnValue(value, config);
                    
                    csvWriter.WriteField(value);
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
        }
        
        /// <summary>
        /// 簡單解析CSV行（考慮引號和逗號）
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
                else
                {
                    currentValue.Append(c);
                }
            }
            
            // 添加最後一個欄位
            result.Add(currentValue.ToString());
            
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
        }

        /// <summary>
        /// 合併多個CSV文件
        /// </summary>
        public async Task<string> MergeCsvFilesAsync(List<string> tempFiles, string outputPath)
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
                        await csvReader.ReadAsync();
                        csvReader.ReadHeader();

                        // 寫入第一個文件的表頭
                        if (isFirstFile)
                        {
                            foreach (var header in csvReader.HeaderRecord)
                            {
                                csvWriter.WriteField(header);
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

                            foreach (var header in csvReader.HeaderRecord)
                            {
                                csvWriter.WriteField(csvReader.GetField(header));
                            }
                            await csvWriter.NextRecordAsync();
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
        }

        /// <summary>
        /// 高效合併多個CSV文件（使用直接文件流複製，避免逐行CSV解析）
        /// </summary>
        /// <param name="tempFiles">要合併的臨時文件清單</param>
        /// <param name="outputPath">輸出文件路徑</param>
        /// <param name="progressCallback">進度回調函數</param>
        /// <returns>合併後的文件路徑</returns>
        public async Task<string> MergeCsvFilesOptimizedAsync(List<string> tempFiles, string outputPath, 
            Action<int, int>? progressCallback = null)
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
                            // 第一個文件：複製包含標頭的所有內容
                            string? line;
                            while ((line = await inputReader.ReadLineAsync()) != null)
                            {
                                await outputWriter.WriteLineAsync(line);
                                processedBytes += line.Length + 2; // +2 for newline chars
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

                        processedCount++;
                        
                        // 每處理一定數量的檔案，暫停一下以減少連續I/O操作
                        if (processedCount % 50 == 0)
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
                        _logAction($"已完成第 {batchIndex + 1}/{batchCount} 批次合併，短暫暫停...");
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
