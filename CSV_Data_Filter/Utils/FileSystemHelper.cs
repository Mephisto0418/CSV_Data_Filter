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
        /// 若遇到網路目錄，僅當該目錄下有符合條件檔案時才robocopy整個目錄到本地暫存，並回傳本地檔案路徑。
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
                    catch (Exception ex) 
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
                catch (Exception ex) 
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
        /// 使用robocopy將整個資料夾複製到本地暫存目錄，回傳本地資料夾路徑
        /// </summary>
        private string BulkCopyNetworkFolder(string sourcePath, string tempDir)
        {
            string folderName = Path.GetFileName(sourcePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string localFolder = Path.Combine(tempDir, folderName);
            try
            {
                Directory.CreateDirectory(localFolder);
                var psi = new ProcessStartInfo
                {
                    FileName = "robocopy",
                    Arguments = $"\"{sourcePath}\" \"{localFolder}\" /E /NFL /NDL /NJH /NJS /NC /NS /NP",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (var proc = Process.Start(psi))
                {
                    proc.WaitForExit();
                }
                return localFolder;
            }
            catch (Exception ex)
            {
                _logAction($"robocopy複製網路資料夾失敗: {ex.Message}");
                return sourcePath;
            }
        }

        /// <summary>
        /// 查找符合條件的CSV文件（大量目錄/檔案時效能佳，網路路徑自動複製到本地暫存）
        /// 多執行緒安全：如需共用暫存檔案清單，請使用ConcurrentBag等執行緒安全集合。
        /// </summary>
        public List<string> FindCsvFiles(
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
            // 首先，過濾掉子目錄
            var filteredPaths = FilterOutSubdirectories(sourcePaths);
            _logAction($"原始路徑數量: {sourcePaths.Count}, 過濾後路徑數量: {filteredPaths.Count}");
            
            var results = new ConcurrentBag<string>();
            var totalPaths = filteredPaths.Count;
            int processedPaths = 0;
            
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
                    
                    // 判斷是否為網路路徑
                    bool isNetworkPath = sourcePath.StartsWith("\\\\");
                    
                    foreach (var file in EnumerateCsvFiles(
                        sourcePath, 
                        folderInclude, folderExclude, useFolderDateFilter, folderDateFormat, folderDateOp, folderDateValue,
                        fileInclude, fileExclude, useFileDateFilter, fileDateFormat, fileDateOp, fileDateValue,
                        tempDir))
                    {
                        if (_cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        
                        results.Add(file);
                        fileCount++;
                        
                        // 每處理500個檔案輸出一次進度
                        if (fileCount % 500 == 0)
                        {
                            _logAction($"在 {Path.GetFileName(sourcePath)} 中已找到 {fileCount} 個符合條件的檔案");
                        }
                    }
                    
                    Interlocked.Increment(ref processedPaths);
                    _logAction($"完成搜尋路徑: {Path.GetFileName(sourcePath)} (找到 {fileCount} 個檔案) - 進度: {processedPaths}/{totalPaths}");
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
            _logAction($"搜尋完成，總共找到 {finalResults.Count} 個符合條件的CSV檔案");
            return finalResults;
        }

        /// <summary>
        /// 查找符合條件的CSV文件（異步版本，避免UI執行緒阻塞）
        /// </summary>
        public async Task<List<string>> FindCsvFilesAsync(
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
            return await Task.Run(() => FindCsvFiles(
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
        /// 獲取符合條件的子目錄
        /// </summary>
        private List<string> GetFilteredSubdirectories(
            string sourcePath, 
            string folderInclude, 
            string folderExclude, 
            bool useFolderDateFilter,
            string folderDateFormat, 
            string folderDateOp, 
            DateTime folderDateValue)
        {
            var results = new List<string>();
            try
            {
                // 添加根目錄
                results.Add(sourcePath);
                
                // 遍歷所有子目錄
                var allDirs = Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories);
                
                foreach (var dir in allDirs)
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var dirName = new DirectoryInfo(dir).Name;
                    
                    // 檢查包含條件
                    if (!string.IsNullOrEmpty(folderInclude) && !dirName.Contains(folderInclude))
                    {
                        continue;
                    }
                    
                    // 檢查排除條件
                    if (!string.IsNullOrEmpty(folderExclude) && dirName.Contains(folderExclude))
                    {
                        continue;
                    }
                    
                    // 檢查日期條件
                    if (useFolderDateFilter)
                    {
                        if (!CompareDates(dirName, folderDateFormat, folderDateValue, folderDateOp))
                        {
                            continue;
                        }
                    }
                    
                    results.Add(dir);
                }
            }
            catch (Exception ex)
            {
                _logAction($"過濾目錄時出錯: {ex.Message}");
            }
            
            return results;
        }

        /// <summary>
        /// 獲取符合條件的CSV文件
        /// </summary>
        private List<string> GetFilteredCsvFiles(
            string directory, 
            string fileInclude, 
            string fileExclude, 
            bool useFileDateFilter,
            string fileDateFormat, 
            string fileDateOp, 
            DateTime fileDateValue)
        {
            var results = new List<string>();
            
            try
            {
                var files = Directory.GetFiles(directory, "*.csv", SearchOption.TopDirectoryOnly);
                
                foreach (var file in files)
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var fileName = Path.GetFileName(file);
                    
                    // 檢查包含條件
                    if (!string.IsNullOrEmpty(fileInclude) && !fileName.Contains(fileInclude))
                    {
                        continue;
                    }
                    
                    // 檢查排除條件
                    if (!string.IsNullOrEmpty(fileExclude) && fileName.Contains(fileExclude))
                    {
                        continue;
                    }
                    
                    // 檢查日期條件
                    if (useFileDateFilter)
                    {
                        if (!CompareDates(fileName, fileDateFormat, fileDateValue, fileDateOp))
                        {
                            continue;
                        }
                    }
                    
                    results.Add(file);
                }
            }
            catch (Exception ex)
            {
                _logAction($"過濾文件時出錯: {ex.Message}");
            }
            
            return results;
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
