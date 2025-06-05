using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CSV_Data_Filter.Utils
{
    /// <summary>
    /// 文件搜尋結果類別，包含搜索到的文件列表和網路路徑映射
    /// </summary>
    public class FindCsvFilesResult
    {
        /// <summary>
        /// 搜尋到的CSV文件列表
        /// </summary>
        public List<string> Files { get; set; } = new List<string>();
        
        /// <summary>
        /// 網路路徑到本地暫存路徑的映射字典
        /// </summary>
        public ConcurrentDictionary<string, string> NetworkPathMappings { get; set; } = new ConcurrentDictionary<string, string>();
    }
    
    /// <summary>
    /// 文件系統處理的輔助類
    /// </summary>    
    public class FileSystemHelper
    {

        private readonly Action<string> _logAction;
        private readonly CancellationToken _cancellationToken;
        private readonly string _dateFormat;

        /// <summary>
        /// 初始化文件系統助手
        /// </summary>
        /// <param name="logAction">日誌記錄的動作</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="dateFormat">日期格式</param>
        public FileSystemHelper(Action<string> logAction, CancellationToken cancellationToken, string dateFormat = "yyyy-MM-dd")
        {
            _logAction = logAction;
            _cancellationToken = cancellationToken;
            _dateFormat = dateFormat;
        }

        /// <summary>
        /// 高效列舉所有符合條件的CSV檔案（支援大量目錄/檔案，避免記憶體爆炸）
        /// 若遇到網路目錄，先使用原始路徑搜尋檔案，僅當找到符合條件的檔案時，才將這些檔案複製到本地暫存，並回傳本地檔案路徑。
        /// </summary>
        private IEnumerable<string> EnumerateCsvFiles(
            string rootPath,
            string folderInclude,
            string folderExclude,
            bool useFolderDateFilter,
            string folderDateFormat,
            string folderDateOp,
            DateTime folderDateValue,
            string fileInclude,
            string fileExclude,
            bool useFileDateFilter,
            string fileDateFormat,
            string fileDateOp,
            DateTime fileDateValue,
            string tempDir
        )
        {
            var dirs = new Stack<string>();
            dirs.Push(rootPath);
            int processedDirs = 0;
            int processedBatch = 0;
            // 使用較小的批次大小以避免記憶體問題
            const int batchSize = 500;
            
            while (dirs.Count > 0)
            {
                if (_cancellationToken.IsCancellationRequested) yield break;
                
                var current = dirs.Pop();
                processedDirs++;
                
                // 每處理1000個目錄報告一次進度（減少頻繁輸出）
                if (processedDirs % 1000 == 0)
                {
                    _logAction($"已掃描 {processedDirs} 個目錄，待處理: {dirs.Count}");
                    
                    // 每處理一批次，執行一次GC回收並短暫暫停，避免記憶體持續增長
                    processedBatch++;
                    if (processedBatch % 5 == 0)
                    {
                        GC.Collect(0, GCCollectionMode.Optimized, false);
                        Task.Delay(1).Wait(); // 極短暫暫停，讓系統有機會回收資源
                    }
                }
                
                var dirName = Path.GetFileName(current);
                
                // 目錄篩選 - 只對最終的目錄名稱應用篩選條件，而不是整個路徑
                // 如果不符合包含條件或符合排除條件，仍然處理其子目錄，但跳過當前目錄的檔案
                bool skipCurrentDirFiles = false;
                if (!string.IsNullOrEmpty(folderInclude) && !dirName.Contains(folderInclude))
                    skipCurrentDirFiles = true;
                if (!string.IsNullOrEmpty(folderExclude) && dirName.Contains(folderExclude))
                    skipCurrentDirFiles = true;
                if (useFolderDateFilter && !CompareDates(dirName, folderDateFormat, folderDateValue, folderDateOp))
                    skipCurrentDirFiles = true;
                
                // 即使跳過當前目錄，也繼續處理子目錄
                if (!skipCurrentDirFiles)
                {
                    string[] files = Array.Empty<string>();
                    try
                    {
                        // 使用更高效的方式枚舉文件
                        files = Directory.GetFiles(current, "*.csv");
                    }
                    catch (Exception) 
                    { 
                        // 只記錄重要的存取錯誤，避免過多log
                        if (processedDirs % 5000 == 0)
                            _logAction($"無法存取部分目錄，繼續搜尋中...");
                    }
                    
                    // 在try-catch區塊外處理檔案，避免yield在try區塊內的問題
                    foreach (var file in files)
                    {
                        if (_cancellationToken.IsCancellationRequested) yield break;
                        
                        var fileName = Path.GetFileName(file);
                        // 檔案篩選
                        if (!string.IsNullOrEmpty(fileInclude) && !fileName.Contains(fileInclude)) continue;
                        if (!string.IsNullOrEmpty(fileExclude) && fileName.Contains(fileExclude)) continue;
                        if (useFileDateFilter && !CompareDates(fileName, fileDateFormat, fileDateValue, fileDateOp)) continue;
                        
                        yield return file; // 直接返回檔案，避免建立中間列表
                    }
                }
                
                // 添加子目錄到處理佇列 - 無論當前目錄是否符合篩選條件，都需要處理子目錄
                string[] subDirs = Array.Empty<string>();
                try
                {
                    // 使用固定大小的數組暫存目錄清單，避免不必要的記憶體分配
                    subDirs = Directory.GetDirectories(current);
                }
                catch (Exception) 
                { 
                    // 減少錯誤log的頻率
                }
                
                // 從後往前添加，保持深度優先的順序
                for (int i = subDirs.Length - 1; i >= 0; i--)
                {
                    dirs.Push(subDirs[i]);
                }
                
                // 如果目錄堆疊變得太大，執行一次記憶體回收
                if (dirs.Count > 10000 && processedDirs % 1000 == 0)
                {
                    GC.Collect(1, GCCollectionMode.Optimized, false);
                }
                
                // 每處理一定數量的目錄後釋放一下批次處理結果
                if (processedDirs % batchSize == 0)
                {
                    // 允許其他線程執行
                    Task.Delay(1).Wait();
                }
            }
            
            _logAction($"目錄掃描完成，總共處理了 {processedDirs} 個目錄");
        }

        /// <summary>
        /// 使用 .NET File.Copy 將整個資料夾複製到本地暫存目錄，回傳本地資料夾路徑
        /// </summary>
        private string BulkCopyNetworkFolder(string sourcePath, string tempDir)
        {
            string folderName = Path.GetFileName(sourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string localFolder = Path.Combine(tempDir, folderName);
            try
            {
                // 建立目標目錄
                Directory.CreateDirectory(localFolder);
                int fileCount = 0;
                int errorCount = 0;
                
                // 先遞迴找出所有的 CSV 檔案
                var filesToCopy = new List<string>();
                try
                {
                    // 使用 SearchOption.AllDirectories 遞迴搜尋
                    filesToCopy.AddRange(Directory.GetFiles(sourcePath, "*.csv", SearchOption.AllDirectories));
                    _logAction($"在 {sourcePath} 中找到 {filesToCopy.Count} 個 CSV 檔案準備複製");
                }
                catch (Exception ex)
                {
                    _logAction($"在 {sourcePath} 中搜尋 CSV 檔案時發生錯誤：{ex.Message}");
                    return sourcePath;
                }
                
                // 使用並行處理來加速複製
                Parallel.ForEach(
                    filesToCopy,
                    new ParallelOptions { MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8) },
                    (file) => {
                        try
                        {
                            string relativePath = file.Substring(sourcePath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                            string targetPath = Path.Combine(localFolder, relativePath);
                            // 確保目標目錄存在
                            string? targetDir = Path.GetDirectoryName(targetPath);
                            if (!string.IsNullOrEmpty(targetDir))
                                Directory.CreateDirectory(targetDir);
                                
                            // 使用 .NET File.Copy 進行文件複製，效能高且穩定
                            File.Copy(file, targetPath, true);
                            
                            Interlocked.Increment(ref fileCount);
                            
                            // 每複製 100 個檔案報告一次進度
                            if (fileCount % 100 == 0)
                            {
                                _logAction($"已複製 {fileCount} 個檔案，失敗 {errorCount} 個");
                            }
                        }
                        catch (Exception ex)
                        {
                            Interlocked.Increment(ref errorCount);
                            // 避免大量錯誤訊息，僅在較大間隔報告錯誤
                            if (errorCount % 50 == 0)
                            {
                                _logAction($"複製檔案時發生錯誤，已失敗 {errorCount} 個：{ex.Message}");
                            }
                        }
                    }
                );
                
                _logAction($"已將網路目錄 {sourcePath} 複製到本地暫存資料夾 {localFolder}，共 {fileCount} 個檔案成功，{errorCount} 個失敗");
                return localFolder;
            }
            catch (Exception ex)
            {
                _logAction($"複製網路資料夾失敗：{ex.Message}，將使用原始路徑 {sourcePath}");
                return sourcePath;
            }
        }

        /// <summary>
        /// 複製已經找到的CSV檔案到本地暫存目錄，回傳暫存檔案路徑
        /// </summary>
        /// <param name="sourcePath">來源目錄路徑</param>
        /// <param name="filesToCopy">需要複製的檔案列表</param>
        /// <param name="tempDir">暫存目錄</param>
        /// <returns>暫存檔案路徑列表</returns>
        private List<string> CopyFilesToTemp(string sourcePath, List<string> filesToCopy, string tempDir)
        {
            var result = new List<string>();
            int fileCount = 0;
            int errorCount = 0;
            
            string folderName = Path.GetFileName(sourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string localFolder = Path.Combine(tempDir, folderName);
            
            // 建立目標目錄
            Directory.CreateDirectory(localFolder);
            
            _logAction($"準備將 {filesToCopy.Count} 個檔案從網路路徑 {sourcePath} 複製到本地暫存目錄");
            
            // 使用並行處理來加速複製
            Parallel.ForEach(
                filesToCopy,
                new ParallelOptions { MaxDegreeOfParallelism = Math.Min(Environment.ProcessorCount, 8) },
                (file) => {
                    try
                    {
                        if (_cancellationToken.IsCancellationRequested)
                            return;
                            
                        // 計算相對路徑，保持原始目錄結構
                        string relativePath = file.Substring(sourcePath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        string targetPath = Path.Combine(localFolder, relativePath);
                        
                        // 確保目標目錄存在
                        string? targetDir = Path.GetDirectoryName(targetPath);
                        if (!string.IsNullOrEmpty(targetDir))
                            Directory.CreateDirectory(targetDir);
                            
                        // 使用 .NET File.Copy 進行檔案複製
                        File.Copy(file, targetPath, true);
                        
                        // 將複製後的檔案路徑加入結果集
                        lock (result)
                        {
                            result.Add(targetPath);
                        }
                        
                        int currentFileCount = Interlocked.Increment(ref fileCount);
                        
                        // 每複製 50 個檔案報告一次進度
                        if (currentFileCount % 50 == 0)
                        {
                            _logAction($"已複製 {currentFileCount} 個檔案，失敗 {errorCount} 個");
                        }
                    }
                    catch (Exception ex)
                    {
                        int currentErrorCount = Interlocked.Increment(ref errorCount);
                        // 避免大量錯誤訊息，僅在較大間隔報告錯誤
                        if (currentErrorCount % 25 == 0)
                        {
                            _logAction($"複製檔案時發生錯誤，已失敗 {currentErrorCount} 個：{ex.Message}");
                        }
                    }
                }
            );
            
            _logAction($"檔案複製完成，共 {fileCount} 個成功，{errorCount} 個失敗");
            return result;
        }

        /// <summary>
        /// 查找符合條件的CSV文件（大量目錄/檔案時效能佳，網路路徑自動複製到本地暫存）
        /// 多執行緒安全：如需共用暫存檔案清單，請使用ConcurrentBag等執行緒安全集合。
        /// </summary>
        public FindCsvFilesResult FindCsvFiles(
            List<string> sourcePaths,
            string folderInclude,
            string folderExclude,
            bool useFolderDateFilter,
            string folderDateFormat,
            string folderDateOp,
            DateTime folderDateValue,
            string fileInclude,
            string fileExclude,
            bool useFileDateFilter,
            string fileDateFormat,
            string fileDateOp,
            DateTime fileDateValue,
            string tempDir
        )
        {
            // 準備結果物件
            var result = new FindCsvFilesResult();
            
            // 首先，過濾掉子目錄
            var filteredPaths = FilterOutSubdirectories(sourcePaths);
            _logAction($"原始路徑數量: {sourcePaths.Count}, 過濾後路徑數量: {filteredPaths.Count}");
            
            var results = new ConcurrentBag<string>();
            var totalPaths = filteredPaths.Count;
            int processedPaths = 0;
            
            // 暫存目錄對應表，用於記錄網路路徑與本地暫存路徑的映射關係
            var networkPathMapping = new ConcurrentDictionary<string, string>();
            
            // 限制最大並行搜尋數量，避免系統資源耗盡
            int maxConcurrency = Math.Min(Environment.ProcessorCount * 2, 8);
            using var semaphore = new SemaphoreSlim(maxConcurrency);
            
            var tasks = filteredPaths.Select(async sourcePath =>
            {
                await semaphore.WaitAsync();
                try
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    
                    int fileCount = 0;
                    var matchedFiles = new List<string>();
                    
                    // 判斷是否為網路路徑
                    bool isNetworkPath = sourcePath.StartsWith("\\\\");
                    
                    // 使用原始路徑進行搜尋，無論是否為網路路徑
                    string pathToSearch = sourcePath;
                    
                    if (isNetworkPath)
                    {
                        _logAction($"檢測到網路路徑: {sourcePath}，先進行檔案搜尋，之後再決定是否需要複製");
                    }
                    
                    // 對任何路徑都使用原始路徑搜尋
                    foreach (var file in EnumerateCsvFiles(
                        pathToSearch, 
                        folderInclude, folderExclude, useFolderDateFilter, folderDateFormat, folderDateOp, folderDateValue,
                        fileInclude, fileExclude, useFileDateFilter, fileDateFormat, fileDateOp, fileDateValue,
                        tempDir))
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        
                        // 記錄符合條件的檔案路徑
                        matchedFiles.Add(file);
                        fileCount++;
                        
                        // 每處理500個檔案輸出一次進度
                        if (fileCount % 500 == 0)
                        {
                            _logAction($"在 {Path.GetFileName(pathToSearch)} 中已找到 {fileCount} 個符合條件的檔案");
                        }
                    }
                    
                    // 只有在是網路路徑且找到檔案時，才複製檔案到暫存目錄
                    if (isNetworkPath && matchedFiles.Count > 0)
                    {
                        _logAction($"網路路徑 {sourcePath} 中找到 {matchedFiles.Count} 個符合條件的檔案，準備複製到本地暫存目錄");
                        
                        // 複製找到的檔案到暫存目錄
                        var localFiles = CopyFilesToTemp(sourcePath, matchedFiles, tempDir);
                        
                        // 記錄映射關係
                        networkPathMapping[sourcePath] = tempDir;
                        
                        // 將本地暫存檔案路徑加入結果
                        foreach (var localFile in localFiles)
                        {
                            results.Add(localFile);
                        }
                    }
                    else
                    {
                        // 非網路路徑或網路路徑但沒有符合條件的檔案，直接加入結果
                        foreach (var file in matchedFiles)
                        {
                            results.Add(file);
                        }
                    }
                    
                    Interlocked.Increment(ref processedPaths);
                    _logAction($"完成搜尋路徑: {Path.GetFileName(pathToSearch)} (找到 {fileCount} 個檔案) - 進度: {processedPaths}/{totalPaths}");
                }
                finally
                {
                    // 確保信號量被釋放
                    semaphore.Release();
                }
            }).ToList(); // 轉換為列表以便實際創建任務
            
            // 等待所有任務完成
            Task.WhenAll(tasks).GetAwaiter().GetResult();
            
            var finalResults = results.ToList();
            
            // 記錄網路路徑映射，這對後續處理很重要
            if (networkPathMapping.Count > 0)
            {
                _logAction($"共有 {networkPathMapping.Count} 個網路路徑被複製到本地暫存目錄");
                foreach (var mapping in networkPathMapping)
                {
                    _logAction($"網路路徑映射: {mapping.Key} -> {mapping.Value}");
                }
            }
            
            _logAction($"搜尋完成，總共找到 {finalResults.Count} 個符合條件的CSV檔案");
            
            // 填充結果
            result.Files = finalResults;
            result.NetworkPathMappings = networkPathMapping;
            
            return result;
        }

        /// <summary>
        /// 非同步查找CSV文件
        /// </summary>
        public Task<FindCsvFilesResult> FindCsvFilesAsync(
            List<string> sourcePaths,
            string folderInclude,
            string folderExclude,
            bool useFolderDateFilter,
            string folderDateFormat,
            string folderDateOp,
            DateTime folderDateValue,
            string fileInclude,
            string fileExclude,
            bool useFileDateFilter,
            string fileDateFormat,
            string fileDateOp,
            DateTime fileDateValue,
            string tempDir
        )
        {
            return Task.Run(() => FindCsvFiles(
                sourcePaths, folderInclude, folderExclude, useFolderDateFilter, folderDateFormat, folderDateOp, folderDateValue,
                fileInclude, fileExclude, useFileDateFilter, fileDateFormat, fileDateOp, fileDateValue, tempDir));
        }
        
        /// <summary>
        /// 過濾掉路徑列表中的子目錄，如果某個路徑是另一個路徑的子目錄，則將其排除
        /// 注意：此方法僅用於搜尋時跳過子目錄，不會從介面移除子目錄
        /// </summary>
        private List<string> FilterOutSubdirectories(List<string> paths)
        {
            if (paths == null || paths.Count <= 1)
            {
                return paths?.ToList() ?? new List<string>();
            }
            
            var result = new List<string>();
            
            // 首先對路徑進行排序，讓上層目錄在前面（長度較短的路徑通常是上層目錄）
            var sortedPaths = paths.OrderBy(p => p.Length).ToList();
            
            foreach (var path in sortedPaths)
            {
                bool isSubdirectory = false;
                
                // 檢查當前路徑是否是已加入結果中任何路徑的子目錄
                foreach (var existingPath in result)
                {
                    // 檢查是否為子目錄（確保路徑後面是目錄分隔符號，避免部分匹配）
                    // 例如：D:\Test 不應該被視為 D:\TestFolder 的上層目錄
                    string existingPathWithSeparator = existingPath.EndsWith(Path.DirectorySeparatorChar.ToString()) || 
                                                       existingPath.EndsWith(Path.AltDirectorySeparatorChar.ToString()) 
                                                     ? existingPath 
                                                     : existingPath + Path.DirectorySeparatorChar;
                    
                    if (path.StartsWith(existingPathWithSeparator, StringComparison.OrdinalIgnoreCase))
                    {
                        isSubdirectory = true;
                        _logAction($"[搜尋時跳過] {path} 是 {existingPath} 的子目錄，上層目錄檢索已包含此目錄");
                        break;
                    }
                }
                
                if (!isSubdirectory)
                {
                    result.Add(path);
                }
            }
            
            if (result.Count < paths.Count)
            {
                _logAction($"搜尋時跳過了 {paths.Count - result.Count} 個子目錄，因為上層目錄搜尋已包含這些目錄");
            }
            
            return result;
        }

        /// <summary>
        /// 從文字中比較日期
        /// </summary>
        public bool CompareDates(string? textWithDate, string format, DateTime compareDate, string compareOperator)
        {
            if (string.IsNullOrEmpty(textWithDate) || string.IsNullOrEmpty(format))
                return false;

            try
            {
                // 嘗試從文字中提取日期
                var dateRegex = new Regex(@"\d{4}[-/\.年]\d{1,2}[-/\.月]\d{1,2}日?|\d{1,2}[-/\.月]\d{1,2}[-/\.日]?\s*,?\s*\d{4}年?|\d{1,2}[-/\.]\d{1,2}[-/\.]\d{4}|\d{4}\d{2}\d{2}");
                var match = dateRegex.Match(textWithDate);

                if (!match.Success)
                    return false;

                var dateStr = match.Value;
                
                // 嘗試解析日期字符串
                var formats = new[] 
                { 
                    "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd", "yyyy年MM月dd日", 
                    "MM-dd-yyyy", "MM/dd/yyyy", "dd-MM-yyyy", "dd/MM/yyyy",
                    "yyyyMMdd", format 
                };
                
                DateTime fileDate = DateTime.MinValue;
                foreach (var fmt in formats)
                {
                    if (DateTime.TryParseExact(dateStr, fmt, System.Globalization.CultureInfo.InvariantCulture, 
                                              System.Globalization.DateTimeStyles.None, out fileDate))
                        break;
                }
                
                if (fileDate == DateTime.MinValue)
                    return false;
                
                // 根據指定的運算符進行比較
                switch (compareOperator)
                {
                    case ">":
                        return fileDate > compareDate;
                    case ">=":
                        return fileDate >= compareDate;
                    case "<":
                        return fileDate < compareDate;
                    case "<=":
                        return fileDate <= compareDate;
                    case "=":
                        return fileDate.Date == compareDate.Date;
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 創建臨時目錄
        /// </summary>
        public string CreateTempDirectory()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), $"CSVFilter_{DateTime.Now:yyyyMMddHHmmss}");
            Directory.CreateDirectory(tempDir);
            return tempDir;
        }

        /// <summary>
        /// 獲取網路檔案的本地映射路徑，如果不是網路檔案則返回原路徑
        /// </summary>
        /// <param name="filePath">文件路徑</param>
        /// <param name="networkMappings">網路路徑映射字典</param>
        /// <returns>本地映射路徑或原路徑</returns>
        public string GetNetworkFileMappingPath(string filePath, ConcurrentDictionary<string, string>? networkMappings = null)
        {
            // 使用提供的映射字典或新建一個空的字典
            var mappings = networkMappings ?? new ConcurrentDictionary<string, string>();
            
            // 判斷文件是否來自網路路徑
            bool isNetworkFile = filePath.StartsWith("\\\\");
            if (!isNetworkFile)
                return filePath;
            
            // 檢查是否有直接的路徑映射
            if (mappings.TryGetValue(filePath, out string? localPath))
                return localPath;
            
            // 嘗試查找父目錄的映射
            foreach (var mapping in mappings)
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
        /// 清理臨時文件和目錄
        /// </summary>
        public void CleanupTempFiles(List<string> tempFiles, string tempDir)
        {
            foreach (var tempFile in tempFiles)
            {
                try { File.Delete(tempFile); } catch { }
            }
            
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }
}
