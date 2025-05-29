using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CSV_Data_Filter.Models;

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
                // 定義輸入 CSV 配置
                var inputConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null
                };
                string outputPath = Path.Combine(tempDir, Path.GetFileName(filePath));
                var outputConfig = new CsvConfiguration(CultureInfo.InvariantCulture) 
                {
                    HasHeaderRecord = true
                };
                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, inputConfig))
                using (var writer = new StreamWriter(outputPath))
                using (var csvWriter = new CsvWriter(writer, outputConfig))
                {
                    if (!csv.Read())
                        return null; // 沒有資料
                    csv.ReadHeader();
                    var headers = csv.HeaderRecord;
                    if (headers == null)
                    {
                        _logAction($"無法讀取文件標頭: {Path.GetFileName(filePath)}");
                        return null;
                    }
                    
                    // 檢查欄位完整性
                    var missingColumns = new List<string>();
                    foreach (var config in columnConfigs)
                    {
                        if (!headers.Contains(config.Name))
                        {
                            missingColumns.Add(config.Name);
                        }
                    }
                    
                    if (missingColumns.Count > 0)
                    {
                        string missingList = string.Join(", ", missingColumns);
                        _logAction($"檔案 {Path.GetFileName(filePath)} 缺少欄位: {missingList}");
                        
                        if (skipIncompleteFiles)
                        {
                            _logAction($"跳過檔案 {Path.GetFileName(filePath)}（缺少必要欄位）");
                            return null;
                        }
                        else
                        {
                            _logAction($"繼續處理檔案 {Path.GetFileName(filePath)}（缺少的欄位將顯示為空值）");
                        }
                    }
                    
                    // 寫入表頭（使用自訂名稱）
                    foreach (var config in columnConfigs)
                    {
                        csvWriter.WriteField(config.CustomName);
                    }
                    if (addFileName)
                    {
                        csvWriter.WriteField("FileName");
                    }
                    if (addDirectoryName)
                    {
                        csvWriter.WriteField("DirectoryName");
                    }
                    await csvWriter.NextRecordAsync();
                    // 開始讀取和處理數據
                    while (await csv.ReadAsync())
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            return null;
                        }
                        // 檢查過濾條件
                        bool includeRecord = true;
                        foreach (var condition in filterConditions)
                        {
                            if (headers.Contains(condition.ColumnName))
                            {
                                string value = csv.GetField(condition.ColumnName) ?? "";
                                _logAction($"[Debug] 欄位: {condition.ColumnName}, 值: {value}, 條件: {condition.Operator}, 篩選值: {condition.Value}");
                                if (!EvaluateCondition(value, condition))
                                {
                                    includeRecord = false;
                                    break;
                                }
                            }
                            else
                            {
                                _logAction($"[Debug] 找不到欄位: {condition.ColumnName}");
                            }
                        }
                        if (!includeRecord) continue;
                        // 寫入選擇的列（使用原始名稱讀取，但已用自訂名稱作為標頭）
                        foreach (var config in columnConfigs)
                        {
                            string value = headers.Contains(config.Name) ? csv.GetField(config.Name) ?? "" : "";
                            
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
                return outputPath;
            }
            catch (Exception ex)
            {
                _logAction($"處理檔案 {Path.GetFileName(filePath)} 時出錯: {ex.Message}");
                return null;
            }
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
                using var outputWriter = new StreamWriter(outputPath);
                bool isFirstFile = true;
                int processedCount = 0;

                foreach (var tempFile in tempFiles)
                {
                    if (_cancellationToken.IsCancellationRequested)
                        break;

                    using var inputReader = new StreamReader(tempFile);
                    
                    if (isFirstFile)
                    {
                        // 第一個文件：複製包含標頭的所有內容
                        await outputWriter.WriteAsync(await inputReader.ReadToEndAsync());
                        isFirstFile = false;
                    }
                    else
                    {
                        // 其他文件：跳過標頭行，只複製資料行
                        await inputReader.ReadLineAsync(); // 跳過標頭行
                        
                        string? line;
                        while ((line = await inputReader.ReadLineAsync()) != null)
                        {
                            if (_cancellationToken.IsCancellationRequested)
                                break;
                                
                            await outputWriter.WriteLineAsync(line);
                        }
                    }

                    processedCount++;
                    progressCallback?.Invoke(processedCount, tempFiles.Count);
                    
                    _logAction($"合併進度: {processedCount}/{tempFiles.Count}");
                }

                await outputWriter.FlushAsync();
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
