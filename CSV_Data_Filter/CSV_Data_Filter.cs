using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsvHelper;
using CsvHelper.Configuration;
using CSV_Data_Filter.Models;
using CSV_Data_Filter.Utils;

namespace CSV_Data_Filter
{
    /// <summary>
    /// 主視窗表單，負責CSV資料過濾與合併的主要流程與UI互動。
    /// </summary>
    public partial class CSV_Data_Filter : Form
    {
        #region 專案資訊
        /// <summary>
        /// 專案名稱
        /// </summary>
        public const string PROJECT_NAME = "CSV Data Filter";
        /// <summary>
        /// 專案版本
        /// </summary>
        public const string PROJECT_VERSION = "1.0.0";
        /// <summary>
        /// 專案描述
        /// </summary>
        public const string PROJECT_DESCRIPTION = "CSV資料過濾與合併工具";
        #endregion

        #region 私有欄位
        /// <summary>
        /// 使用者選擇的來源資料夾路徑清單
        /// </summary>
        private List<string> _sourcePaths = new List<string>();
        /// <summary>
        /// 目標輸出資料夾
        /// </summary>
        private string _targetPath = "";
        /// <summary>
        /// 自訂輸出檔案名稱
        /// </summary>
        private string _customFileName = "";
        /// <summary>
        /// 欄位處理設定清單，對應每個選取欄位的處理方式
        /// </summary>
        private List<Models.ColumnConfig> _columnConfigs = new List<Models.ColumnConfig>();
        /// <summary>
        /// 篩選條件清單
        /// </summary>
        private List<Models.FilterCondition> _filterConditions = new List<Models.FilterCondition>();
        /// <summary>
        /// 是否在輸出檔案最後一欄加入來源檔案名稱
        /// </summary>
        private bool _addFileNameColumn = false;
        /// <summary>
        /// 是否跳過缺少欄位的檔案
        /// </summary>
        private bool _skipIncompleteFiles = false;
        /// <summary>
        /// 日期格式字串
        /// </summary>
        private string _dateFormat = "yyyy-MM-dd";
        /// <summary>
        /// 總檔案數
        /// </summary>
        private int _totalFiles = 0;
        /// <summary>
        /// 已處理檔案數
        /// </summary>
        private int _processedFiles = 0;
        /// <summary>
        /// 取消令牌來源
        /// </summary>
        private CancellationTokenSource? _cts;
        #endregion

        #region 建構子
        /// <summary>
        /// 建構子，初始化UI元件
        /// </summary>
        public CSV_Data_Filter()
        {
            InitializeComponent();
            this.Text = $"{PROJECT_NAME} v{PROJECT_VERSION}";
        }
        #endregion

        #region 私有方法
        private void AddSourcePath()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    lstSourcePaths.Items.Add(dialog.SelectedPath);
                    _sourcePaths.Add(dialog.SelectedPath);
                    AddLog(lstLog, $"已新增來源路徑: {dialog.SelectedPath}");
                }
            }
        }

        private void RemoveSourcePath(ListBox sourcePathsListBox)
        {
            if (sourcePathsListBox.SelectedIndex != -1)
            {
                var path = sourcePathsListBox.SelectedItem?.ToString();
                if (path != null)
                {
                    sourcePathsListBox.Items.RemoveAt(sourcePathsListBox.SelectedIndex);
                    _sourcePaths.Remove(path);
                    AddLog(lstLog, $"已移除來源路徑: {path}");
                }
            }
        }

        private void BrowseTargetFolder(TextBox targetPathTextBox)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    targetPathTextBox.Text = dialog.SelectedPath;
                    _targetPath = dialog.SelectedPath;
                    AddLog(lstLog, $"已選擇目標資料夾: {dialog.SelectedPath}");
                }
            }
        }

        private void GetCsvColumns(ListBox sourcePathsListBox, ListBox availColumnsListBox)
        {
            if (sourcePathsListBox.Items.Count == 0)
            {
                MessageBox.Show("請先新增至少一個來源路徑", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            AddLog(lstLog, "正在搜尋 CSV 檔案以獲取欄位...");
            
            // 只要找到第一個符合條件的CSV檔案就停止
            string? firstCsvFile = null;
            foreach (var path in _sourcePaths.Where(p => !string.IsNullOrEmpty(p)))
            {
                firstCsvFile = FindFirstCsvFile(path);
                if (firstCsvFile != null)
                    break;
            }

            if (firstCsvFile == null)
            {
                MessageBox.Show("在指定的路徑中找不到 CSV 檔案", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 讀取 CSV 標頭
                using (var reader = new StreamReader(firstCsvFile))
                {
                    string? headerLine = reader.ReadLine();
                    if (headerLine != null)
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = true
                        };
                        using (var csv = new CsvReader(new StringReader(headerLine + "\r\n"), config))
                        {
                            csv.Read();
                            csv.ReadHeader();
                            if (lstAvailColumns != null)
                            {
                                lstAvailColumns.Items.Clear();
                                foreach (var header in csv.HeaderRecord ?? Array.Empty<string>())
                                {
                                    lstAvailColumns.Items.Add(header);
                                }
                                AddLog(lstLog, $"從 {Path.GetFileName(firstCsvFile)} 中讀取了 {csv.HeaderRecord?.Length ?? 0} 個欄位");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"讀取 CSV 標頭時出錯: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                AddLog(lstLog, $"錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 遞迴尋找第一個符合條件的CSV檔案（副檔名.csv，可擴充為名稱包含/排除等）
        /// </summary>
        private string? FindFirstCsvFile(string dir)
        {
            try
            {
                var files = Directory.GetFiles(dir, "*.csv", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                    return files[0];
                foreach (var sub in Directory.GetDirectories(dir))
                {
                    var found = FindFirstCsvFile(sub);
                    if (found != null)
                        return found;
                }
            }
            catch { }
            return null;
        }

        private void AddSelectedColumn(ListBox availColumnsListBox, ListBox selectedColumnsListBox)
        {
            if (availColumnsListBox.SelectedIndex != -1)
            {
                var columnName = availColumnsListBox.SelectedItem?.ToString();
                if (columnName != null && !_columnConfigs.Any(c => c.Name == columnName))
                {
                    var config = new Models.ColumnConfig(columnName);
                    _columnConfigs.Add(config);
                    
                    // 顯示自訂名稱（初始時與原始名稱相同）
                    selectedColumnsListBox.Items.Add($"{config.CustomName} ({config.Name})");
                    AddLog(lstLog, $"已新增欄位: {columnName}");
                }
            }
        }

        private void RemoveSelectedColumn(ListBox selectedColumnsListBox)
        {
            if (selectedColumnsListBox.SelectedIndex != -1)
            {
                var selectedItem = selectedColumnsListBox.SelectedItem?.ToString();
                if (selectedItem != null)
                {
                    // 從顯示文字中提取原始欄位名稱
                    var match = System.Text.RegularExpressions.Regex.Match(selectedItem, @"\(([^)]+)\)$");
                    if (match.Success)
                    {
                        var originalName = match.Groups[1].Value;
                    selectedColumnsListBox.Items.RemoveAt(selectedColumnsListBox.SelectedIndex);
                        _columnConfigs.RemoveAll(c => c.Name == originalName);
                        AddLog(lstLog, $"已移除欄位: {originalName}");
                    }
                }
            }
        }

        private void ConfigureSelectedColumn(ListBox selectedColumnsListBox)
        {
            if (selectedColumnsListBox.SelectedIndex != -1)
            {
                var selectedItem = selectedColumnsListBox.SelectedItem?.ToString();
                if (selectedItem != null)
                {
                    // 從顯示文字中提取原始欄位名稱
                    var match = System.Text.RegularExpressions.Regex.Match(selectedItem, @"\(([^)]+)\)$");
                    if (match.Success)
                    {
                        var originalName = match.Groups[1].Value;
                        var config = _columnConfigs.Find(c => c.Name == originalName);
                    
                    if (config != null)
                    {
                        using (var configForm = new ColumnConfigForm(config))
                        {
                            if (configForm.ShowDialog() == DialogResult.OK)
                            {
                                    // 更新ListBox中的顯示文字
                                    selectedColumnsListBox.Items[selectedColumnsListBox.SelectedIndex] = 
                                        $"{config.CustomName} ({config.Name})";
                                    AddLog(lstLog, $"已設定欄位 {originalName} 的處理方式");
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("請先選擇一個欄位", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AddFilterCondition(ListBox filtersListBox, ListBox selectedColumnsListBox)
        {
            if (selectedColumnsListBox.Items.Count == 0)
            {
                MessageBox.Show("請先選擇要使用的欄位", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var filterForm = new FilterConditionForm(selectedColumnsListBox.Items.Cast<string>().ToList()))
            {
                if (filterForm.ShowDialog() == DialogResult.OK)
                {
                    var condition = filterForm.Condition;
                    _filterConditions.Add(condition);
                    filtersListBox.Items.Add(condition.ToString());
                    
                    AddLog(lstLog, $"已新增篩選條件: {condition}");
                }
            }
        }

        private void RemoveFilterCondition(ListBox filtersListBox)
        {
            if (filtersListBox.SelectedIndex != -1)
            {
                var conditionStr = filtersListBox.SelectedItem?.ToString() ?? "";
                var index = filtersListBox.SelectedIndex;
                filtersListBox.Items.RemoveAt(index);
                
                if (index < _filterConditions.Count)
                {
                    _filterConditions.RemoveAt(index);
                }
                
                AddLog(lstLog, $"已移除篩選條件: {conditionStr}");
            }
        }

        /// <summary>
        /// 執行主流程，確保同時僅有一個任務執行，並保證Log與UI操作執行緒安全。
        /// </summary>
        private async void ExecuteProcess()
        {
            if (_cts != null)
            {
                SafeAddLog(lstLog, "已有任務執行中");
                return;
            }
            _cts = new CancellationTokenSource();
            try
            {
                SafeAddLog(lstLog, "開始處理...");

                // 讀取自訂檔案名稱
                this.Invoke((Action)(() => {
                    _customFileName = txtOutputFileName.Text.Trim();
                    _addFileNameColumn = chkAddFileName.Checked;
                    _skipIncompleteFiles = chkSkipIncompleteFiles.Checked;
                }));

                // 1. 建立輔助類別
                var fileHelper = new Utils.FileSystemHelper(msg => SafeAddLog(lstLog, msg), _cts.Token, _dateFormat);
                var csvProcessor = new Utils.CsvProcessor(msg => SafeAddLog(lstLog, msg), _cts.Token, _dateFormat);

                // 2. 建立唯一暫存目錄
                var tempDir = fileHelper.CreateTempDirectory();

                // 3. 異步尋找所有符合條件的 CSV 檔案（避免UI鎖死）
                SafeAddLog(lstLog, "正在搜尋符合條件的CSV檔案...");
                var matchingFiles = await fileHelper.FindCsvFilesAsync(
                    _sourcePaths,
                    txtFolderInclude.Text,
                    txtFolderExclude.Text,
                    chkFolderDate.Checked,
                    txtFolderDateFormat.Text,
                    cboFolderDateOp.Text,
                    dtpFolderDateValue.Value,
                    txtFileInclude.Text,
                    txtFileExclude.Text,
                    chkFileDate.Checked,
                    txtFileDateFormat.Text,
                    cboFileDateOp.Text,
                    dtpFileDateValue.Value,
                    tempDir
                );
                
                _totalFiles = matchingFiles.Count;
                _processedFiles = 0;
                
                if (_totalFiles == 0)
                {
                    SafeAddLog(lstLog, "找不到符合條件的檔案");
                    btnExecute.Enabled = true;
                    btnCancel.Enabled = false;
                    return;
                }
                
                SafeAddLog(lstLog, $"找到 {_totalFiles} 個符合條件的 CSV 檔案");
                
                // 設置進度條範圍
                this.Invoke((Action)(() => {
                progressBar.Maximum = _totalFiles;
                progressBar.Value = 0;
                }));

                // 4. 處理欄位選擇，若未選擇則自動選擇所有欄位
                var columnConfigs = new List<ColumnConfig>();
                this.Invoke((Action)(() => {
                    // 使用已配置的欄位配置
                    if (_columnConfigs.Count > 0)
                    {
                        columnConfigs = new List<ColumnConfig>(_columnConfigs);
                    }
                    else
                    {
                        // 從第一個檔案取得所有欄位，建立預設配置
                        var allHeaders = csvProcessor.GetCsvHeaders(matchingFiles[0]);
                        if (allHeaders != null)
                        {
                            foreach (var header in allHeaders)
                            {
                                columnConfigs.Add(new ColumnConfig(header));
                            }
                        }
                    }
                }));

                // 5. 開始處理檔案
                var tempFiles = new List<string>();
                var semaphore = new SemaphoreSlim((int)nudThreads.Value);
                var tasks = new List<Task>();
                foreach (var file in matchingFiles)
                {
                    await semaphore.WaitAsync();
                    tasks.Add(Task.Run(async () => {
                        try
                        {
                            if (_cts.Token.IsCancellationRequested)
                                return;
                            var tempFile = await csvProcessor.ProcessCsvFileAsync(
                                file,
                                columnConfigs,
                                _filterConditions,
                                _addFileNameColumn,
                                tempDir,
                                _skipIncompleteFiles
                            );
                            if (!string.IsNullOrEmpty(tempFile))
                            {
                                lock (tempFiles)
                                {
                                    tempFiles.Add(tempFile);
                                }
                            }
                            lock (this)
                            {
                                _processedFiles++;
                                this.Invoke((Action)(() => {
                                    progressBar.Value = _processedFiles;
                                    SafeAddLog(lstLog, $"已處理: {Path.GetFileName(file)} ({_processedFiles}/{_totalFiles})");
                                }));
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Invoke((Action)(() => {
                                SafeAddLog(lstLog, $"處理檔案 {Path.GetFileName(file)} 時出錯: {ex.Message}");
                            }));
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, _cts.Token));
                }
                await Task.WhenAll(tasks);
                
                if (_cts.Token.IsCancellationRequested)
                {
                    SafeAddLog(lstLog, "操作已取消");
                }
                else
                {
                    // 6. 使用優化的合併方法（大幅提升效率）並檢查檔案名稱重複
                    SafeAddLog(lstLog, "正在合併處理結果...");
                    
                    // 使用自訂檔案名稱或預設名稱
                    string baseFileName;
                    if (!string.IsNullOrWhiteSpace(_customFileName))
                    {
                        // 確保檔案名稱有.csv副檔名
                        baseFileName = _customFileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) 
                            ? _customFileName 
                            : $"{_customFileName}.csv";
                    }
                    else
                    {
                        baseFileName = $"Merged_CSV_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    }
                    
                    string finalFile = csvProcessor.GenerateUniqueFileName(_targetPath, baseFileName);
                    
                    await csvProcessor.MergeCsvFilesOptimizedAsync(tempFiles, finalFile, (processed, total) => {
                        this.Invoke((Action)(() => {
                            progressBar.Value = Math.Min(progressBar.Maximum, processed);
                        }));
                    });
                    
                    SafeAddLog(lstLog, $"處理完成，結果已保存至: {finalFile}");
                    // 7. 清理臨時檔案
                    fileHelper.CleanupTempFiles(tempFiles, tempDir);
                }
            }
            catch (Exception ex)
            {
                SafeAddLog(lstLog, $"發生錯誤: {ex.Message}");
                MessageBox.Show($"執行過程中發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _cts = null;
                this.Invoke((Action)(() => {
                btnExecute.Enabled = true;
                btnCancel.Enabled = false;
                }));
            }
        }

        /// <summary>
        /// 執行緒安全地新增Log（UI執行緒同步），自動添加時間戳記並滾動到最新訊息
        /// </summary>
        private void SafeAddLog(ListBox logList, string message)
        {
            if (logList.InvokeRequired)
            {
                logList.Invoke(new Action(() => {
                    string timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
                    logList.Items.Add(timestampedMessage);
                    // 自動滾動到最新訊息並取消選取
                    logList.TopIndex = logList.Items.Count - 1;
                    logList.ClearSelected();
                }));
            }
            else
            {
                string timestampedMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
                logList.Items.Add(timestampedMessage);
                // 自動滾動到最新訊息並取消選取
                logList.TopIndex = logList.Items.Count - 1;
                logList.ClearSelected();
            }
        }

        private void CancelProcess(Button executeButton, Button cancelButton)
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                AddLog(lstLog, "正在取消處理...");
                cancelButton.Enabled = false;
            }
        }

        // 使用 LogHelper 工具類代替此方法，但保留向後相容性
        private void AddLog(ListBox? lstLog, string message)
        {
            if (lstLog != null)
            {
                // LogHelper.AddLog 已經會添加時間戳記，所以這裡不需要重複添加
                LogHelper.AddLog(this, lstLog, message);
                // 自動滾動到最新訊息並取消選取
                lstLog.TopIndex = lstLog.Items.Count - 1;
                lstLog.ClearSelected();
            }
        }
        #endregion

        #region 事件處理方法
        // 按鈕事件處理方法
        private void btnAddPath_Click(object sender, EventArgs e)
        {
            AddSourcePath();
        }

        private void btnRemovePath_Click(object sender, EventArgs e)
        {
            RemoveSourcePath(lstSourcePaths);
        }

        private void btnBrowseTarget_Click(object sender, EventArgs e)
        {
            BrowseTargetFolder(txtTargetPath);
        }

        private void btnGetColumns_Click(object sender, EventArgs e)
        {
            GetCsvColumns(lstSourcePaths, lstAvailColumns);
        }

        private void btnAddColumn_Click(object sender, EventArgs e)
        {
            AddSelectedColumn(lstAvailColumns, lstSelectedColumns);
        }

        private void btnRemoveColumn_Click(object sender, EventArgs e)
        {
            RemoveSelectedColumn(lstSelectedColumns);
        }

        private void btnConfigColumn_Click(object sender, EventArgs e)
        {
            ConfigureSelectedColumn(lstSelectedColumns);
        }

        private void btnAddFilter_Click(object sender, EventArgs e)
        {
            AddFilterCondition(lstFilters, lstSelectedColumns);
        }

        private void btnRemoveFilter_Click(object sender, EventArgs e)
        {
            RemoveFilterCondition(lstFilters);
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            ExecuteProcess();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CancelProcess(btnExecute, btnCancel);
        }

        // 支援log面板Ctrl+C複製
        private void lstLog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.C && lstLog.SelectedItems.Count > 0)
            {
                var lines = lstLog.SelectedItems.Cast<object>().Select(item => item.ToString());
                Clipboard.SetText(string.Join("\r\n", lines));
                e.Handled = true;
            }
        }
        #endregion
    }
}
