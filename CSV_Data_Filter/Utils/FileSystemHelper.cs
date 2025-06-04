using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;

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
                if (_cancellationToken.IsCancellationRequested)
                    break;

                var current = dirs.Pop();
                processedDirs++;

                // 每處理1000個目錄報告一次進度（減少頻繁輸出）
                if (processedDirs % 1000 == 0)
                {
                    _logAction($"已掃描 {processedDirs} 個目錄，待處理: {dirs.Count}");
                    // 每批次處理結束後，強制記憶體回收
                    processedBatch++;
                    if (processedBatch % 10 == 0)
                    {
                        GC.Collect(2, GCCollectionMode.Forced);
                        GC.WaitForPendingFinalizers();
                    }
                }

                var dirName = Path.GetFileName(current);
                // 目錄篩選
                if (!string.IsNullOrEmpty(folderInclude) && !dirName.Contains(folderInclude)) continue;
                if (!string.IsNullOrEmpty(folderExclude) && dirName.Contains(folderExclude)) continue;
                if (useFolderDateFilter && !CompareDates(dirName, folderDateFormat, folderDateValue, folderDateOp)) continue;

                // 使用局部變數避免大量資料佔用記憶體
                List<string> tempFiles = new();
                IEnumerable<string>? files = null;
                try
                {
                    // 使用EnumerateFiles而非GetFiles以提高效能
                    files = Directory.EnumerateFiles(current, "*.csv", SearchOption.TopDirectoryOnly);
                }
                catch (Exception)
                {
                    // 存取錯誤處理
                    files = null;
                }

                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                            yield break;

                        var fileName = Path.GetFileName(file);
                        // 檔案篩選
                        if (!string.IsNullOrEmpty(fileInclude) && !fileName.Contains(fileInclude)) continue;
                        if (!string.IsNullOrEmpty(fileExclude) && fileName.Contains(fileExclude)) continue;
                        if (useFileDateFilter && !CompareDates(fileName, fileDateFormat, fileDateValue, fileDateOp)) continue;
                        tempFiles.Add(file);
                    }
                }

                // 若為網路目錄且有符合條件檔案，robocopy整個目錄到本地暫存
                if (current.StartsWith("\\\\") && tempFiles.Count > 0)
                {
                    string localDir = BulkCopyNetworkFolder(current, tempDir);
                    IEnumerable<string>? localFiles = null;
                    try
                    {
                        localFiles = Directory.EnumerateFiles(localDir, "*.csv", SearchOption.TopDirectoryOnly)
                            .Where(f => tempFiles.Any(ff => Path.GetFileName(ff) == Path.GetFileName(f)));
                    }
                    catch { localFiles = null; }
                    foreach (var lf in localFiles ?? Array.Empty<string>())
                        yield return lf;
                }
                else
                {
                    foreach (var f in tempFiles)
                        yield return f;
                }

                // 釋放臨時檔案清單以減少記憶體使用
                tempFiles.Clear();
                tempFiles = null!;

                // 添加子目錄到處理佇列，使用批次處理避免記憶體爆炸
                try
                {
                    // 子目錄也使用枚舉方式，減少記憶體使用
                    foreach (var dir in Directory.EnumerateDirectories(current, "*", SearchOption.TopDirectoryOnly))
                    {
                        dirs.Push(dir);
                        
                        // 如果佇列太長，優先處理一些目錄以避免記憶體過度使用
                        if (dirs.Count > batchSize * 2)
                            break;
                    }
                }
                catch (Exception)
                {
                    // 處理存取錯誤，但不中斷處理
                }
                
                // 如果處理了足夠多的目錄，則強制進行記憶體回收
                if (processedDirs % batchSize == 0)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
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
                    if (proc is not null)
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
            var results = new ConcurrentBag<string>(); // 執行緒安全
            int totalPaths = sourcePaths.Count;
            int processedPaths = 0;

            _logAction($"開始搜尋 {totalPaths} 個來源路徑...");

            Parallel.ForEach(sourcePaths, sourcePath =>
            {
                if (_cancellationToken.IsCancellationRequested) return;

                _logAction($"正在搜尋路徑: {sourcePath}");
                int fileCount = 0;

                foreach (var file in EnumerateCsvFiles(
                    sourcePath, folderInclude, folderExclude, useFolderDateFilter, folderDateFormat, folderDateOp, folderDateValue,
                    fileInclude, fileExclude, useFileDateFilter, fileDateFormat, fileDateOp, fileDateValue, tempDir))
                {
                    if (_cancellationToken.IsCancellationRequested) return;
                    results.Add(file);
                    fileCount++;

                    // 每找到100個檔案報告一次進度（減少頻繁輸出）
                    if (fileCount % 100 == 0)
                    {
                        _logAction($"在 {Path.GetFileName(sourcePath)} 中已找到 {fileCount} 個符合條件的檔案");
                    }
                }

                Interlocked.Increment(ref processedPaths);
                _logAction($"完成搜尋路徑: {Path.GetFileName(sourcePath)} (找到 {fileCount} 個檔案) - 進度: {processedPaths}/{totalPaths}");
            });

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

        /// <summary>
        /// 從給定的路徑列表中尋找第一個CSV檔案
        /// </summary>
        /// <param name="sourcePaths">要搜尋的路徑列表</param>
        /// <returns>找到的第一個CSV檔案路徑，如果沒有找到則返回null</returns>
        public string? FindFirstCsvFile(List<string> sourcePaths)
        {
            foreach (var path in sourcePaths.Where(p => !string.IsNullOrEmpty(p)))
            {
                string? result = FindFirstCsvFileInDirectory(path);
                if (result != null)
                    return result;
            }
            return null;
        }
        
        /// <summary>
        /// 在指定目錄中尋找第一個CSV檔案（包括子目錄）
        /// </summary>
        /// <param name="directory">要搜索的目錄</param>
        /// <returns>找到的第一個CSV檔案路徑，如果沒有找到則返回null</returns>
        private string? FindFirstCsvFileInDirectory(string directory)
        {
            try
            {
                // 先檢查當前目錄中的檔案
                var files = Directory.GetFiles(directory, "*.csv", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                    return files[0];
                
                // 如果當前目錄沒有找到，則檢查子目錄
                foreach (var subDir in Directory.GetDirectories(directory))
                {
                    var found = FindFirstCsvFileInDirectory(subDir);
                    if (found != null)
                        return found;
                }
            }
            catch (Exception ex)
            {
                _logAction($"搜尋CSV檔案時出錯: {ex.Message}");
            }
            return null;
        }
    }
}