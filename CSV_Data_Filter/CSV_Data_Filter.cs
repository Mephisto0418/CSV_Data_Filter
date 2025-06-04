using CSV_Data_Filter.Models;
using CSV_Data_Filter.Utils;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Collections.Concurrent;

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

        #endregion 專案資訊

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
        /// 是否在輸出檔案最後一欄加入來源目錄名稱
        /// </summary>
        private bool _addDirectoryNameColumn = false;

        /// <summary>
        /// 是否跳過缺少欄位的檔案
        /// </summary>
        private bool _skipIncompleteFiles = false;

        /// <summary>
        /// 是否保留暫存檔案
        /// </summary>
        private bool _keepTempFiles = false;

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

        private bool EnableDebugLog => chkDebugLog != null && chkDebugLog.Checked;

        #endregion 私有欄位

        #region 建構子

        /// <summary>
        /// 建構子，初始化UI元件
        /// </summary>
        public CSV_Data_Filter()
        {
            InitializeComponent();
            this.Text = $"{PROJECT_NAME} v{PROJECT_VERSION}";
            
            // 設置拖放功能
            lstAvailColumns.AllowDrop = true;
            lstAvailColumns.DragEnter += LstAvailColumns_DragEnter;
            lstAvailColumns.DragDrop += LstAvailColumns_DragDrop;
            
            // 設置默認執行緒數
            nudThreads.Value = Math.Max(1, Environment.ProcessorCount - 1);
            
            // 設置日期操作下拉選項
            cboFolderDateOp.Items.AddRange(new string[] { "等於", "大於", "小於", "不等於" });
            cboFileDateOp.Items.AddRange(new string[] { "等於", "大於", "小於", "不等於" });
            cboFolderDateOp.SelectedIndex = 0;
            cboFileDateOp.SelectedIndex = 0;
            
            // 設置日期格式和日期值
            txtFolderDateFormat.Text = _dateFormat;
            txtFileDateFormat.Text = _dateFormat;
            dtpFolderDateValue.Format = DateTimePickerFormat.Custom;
            dtpFolderDateValue.CustomFormat = _dateFormat;
            dtpFileDateValue.Format = DateTimePickerFormat.Custom;
            dtpFileDateValue.CustomFormat = _dateFormat;
            
            // 日期過濾條件初始禁用
            txtFolderDateFormat.Enabled = false;
            cboFolderDateOp.Enabled = false;
            dtpFolderDateValue.Enabled = false;
            txtFileDateFormat.Enabled = false;
            cboFileDateOp.Enabled = false;
            dtpFileDateValue.Enabled = false;
            
            // 綁定事件
            chkFolderDate.CheckedChanged += (s, e) => {
                txtFolderDateFormat.Enabled = chkFolderDate.Checked;
                cboFolderDateOp.Enabled = chkFolderDate.Checked;
                dtpFolderDateValue.Enabled = chkFolderDate.Checked;
            };
            chkFileDate.CheckedChanged += (s, e) => {
                txtFileDateFormat.Enabled = chkFileDate.Checked;
                cboFileDateOp.Enabled = chkFileDate.Checked;
                dtpFileDateValue.Enabled = chkFileDate.Checked;
            };
            
            // 設置雙擊事件
            lstAvailColumns.DoubleClick += (s, e) => { btnAddColumn_Click(btnAddColumn, EventArgs.Empty); };
            lstSelectedColumns.DoubleClick += (s, e) => { btnRemoveColumn_Click(btnRemoveColumn, EventArgs.Empty); };
        }

        /// <summary>
        /// 處理拖放進入事件
        /// </summary>
        private void LstAvailColumns_DragEnter(object? sender, DragEventArgs e)
        {
            // 檢查是否有文件被拖放
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && files[0].EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }
        
        /// <summary>
        /// 處理拖放CSV檔案的事件
        /// </summary>
        private void LstAvailColumns_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0 && files[0].EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    string csvFile = files[0];
                    AddLog(lstLog, $"拖放CSV檔案: {csvFile}");
                    
                    try
                    {
                        // 讀取CSV標頭
                        var csvProcessor = new Utils.CsvProcessor(msg => AddLog(lstLog, msg), CancellationToken.None, _dateFormat);
                        string[]? headers = csvProcessor.GetCsvHeaders(csvFile);
                        
                        if (headers != null && headers.Length > 0)
                        {
                            lstAvailColumns.Items.Clear();
                            foreach (var header in headers)
                            {
                                lstAvailColumns.Items.Add(header);
                            }
                            AddLog(lstLog, $"從 {Path.GetFileName(csvFile)} 中讀取了 {headers.Length} 個欄位");
                            
                            // 如果來源路徑列表中沒有這個檔案所在的目錄，自動添加
                            string directory = Path.GetDirectoryName(csvFile) ?? "";
                            if (!string.IsNullOrEmpty(directory) && !_sourcePaths.Contains(directory))
                            {
                                _sourcePaths.Add(directory);
                                lstSourcePaths.Items.Add(directory);
                                AddLog(lstLog, $"已自動新增來源路徑: {directory}");
                            }
                        }
                        else
                        {
                            MessageBox.Show("無法從檔案讀取欄位標題", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"讀取CSV檔案時出錯: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        AddLog(lstLog, $"錯誤: {ex.Message}");
                    }
                }
            }
        }

        #endregion 建構子

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
            // 使用檔案選擇對話框讓使用者選擇CSV檔案
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "CSV Files (*.csv)|*.csv";
                dialog.Title = "選擇一個CSV檔案來獲取欄位名稱";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string firstCsvFile = dialog.FileName;
                    AddLog(lstLog, $"已選擇檔案: {firstCsvFile}");
                    
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
            }
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
                    // 以最後一個 " (" 為分隔，並保留括號內容
                    int idx = selectedItem.LastIndexOf(" (");
                    string originalName = selectedItem;
                    if (idx >= 0 && selectedItem.EndsWith(")"))
                    {
                        originalName = selectedItem.Substring(idx + 2, selectedItem.Length - idx - 3);
                    }
                    selectedColumnsListBox.Items.RemoveAt(selectedColumnsListBox.SelectedIndex);
                    _columnConfigs.RemoveAll(c => c.Name == originalName);
                    AddLog(lstLog, $"已移除欄位: {originalName}");
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

            // 解析顯示字串，取得原始欄位名稱清單
            var columnNames = selectedColumnsListBox.Items.Cast<string>().Select(item =>
            {
                int idx = item.LastIndexOf(" (");
                if (idx >= 0 && item.EndsWith(")"))
                    return item.Substring(idx + 2, item.Length - idx - 3);
                return item;
            }).ToList();

            using (var filterForm = new FilterConditionForm(columnNames))
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
            if (lstSelectedColumns.Items.Count == 0)
            {
                MessageBox.Show("請至少選擇一個輸出欄位", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_sourcePaths.Count == 0)
            {
                MessageBox.Show("請至少新增一個來源路徑", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // 檢查目標路徑，如果未設定則使用「文件」資料夾
                if (string.IsNullOrWhiteSpace(_targetPath))
                {
                    _targetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                    txtTargetPath.Text = _targetPath;
                    SafeAddLog(lstLog, $"未指定目標資料夾，使用預設「文件」資料夾: {_targetPath}");
                }

                // 1. 確認要處理的欄位清單
                var columnConfigs = new List<Models.ColumnConfig>(_columnConfigs);
                
                // 如果沒有選擇任何欄位，自動選擇所有欄位
                if (columnConfigs.Count == 0)
                {
                    SafeAddLog(lstLog, "未選擇任何欄位，將自動使用檔案中的所有欄位");
                    
                    // 使用輔助類尋找第一個符合條件的CSV檔案
                    var tempFileHelper = new Utils.FileSystemHelper(msg => SafeAddLog(lstLog, msg), _cts.Token, _dateFormat);
                    string? firstCsvFile = tempFileHelper.FindFirstCsvFile(_sourcePaths);
                    
                    if (firstCsvFile != null)
                    {
                        var tempCsvProcessor = new Utils.CsvProcessor(msg => SafeAddLog(lstLog, msg), _cts.Token, _dateFormat);
                        string[]? headers = tempCsvProcessor.GetCsvHeaders(firstCsvFile);
                        
                        if (headers != null && headers.Length > 0)
                        {
                            foreach (var header in headers)
                            {
                                var config = new Models.ColumnConfig(header);
                                columnConfigs.Add(config);
                                // 同步更新UI
                                this.Invoke((Action)(() => {
                                    lstSelectedColumns.Items.Add($"{config.CustomName} ({config.Name})");
                                }));
                            }
                            SafeAddLog(lstLog, $"已自動新增 {headers.Length} 個欄位");
                        }
                        else
                        {
                            MessageBox.Show("無法從檔案獲取欄位，請手動選擇欄位", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            btnExecute.Enabled = true;
                            btnCancel.Enabled = false;
                            return;
                        }
                    }
                    else
                    {
                        MessageBox.Show("找不到任何CSV檔案，請確認路徑和篩選條件", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        btnExecute.Enabled = true;
                        btnCancel.Enabled = false;
                        return;
                    }
                }
                
                // 2. 確認過濾條件
                var filterConditions = new List<Models.FilterCondition>(_filterConditions);
                
                // 3. 建立進度報告和取消令牌
                _cts = new CancellationTokenSource();
                
                // 4. 更新UI
                btnExecute.Enabled = false;
                btnCancel.Enabled = true;
                progressBar.Value = 0;
                SafeAddLog(lstLog, "開始處理...");
                
                // 5. 建立臨時目錄
                var fileHelper = new Utils.FileSystemHelper(
                    message => SafeAddLog(lstLog, message),
                    _cts.Token, 
                    _dateFormat
                );
                string tempDir = fileHelper.CreateTempDirectory();
                
                // 紀錄開始時間
                var startTime = DateTime.Now;
                
                // 找出符合條件的所有CSV檔案
                SafeAddLog(lstLog, "正在搜尋符合條件的CSV檔案...");
                var files = await fileHelper.FindCsvFilesAsync(
                    _sourcePaths,
                    txtFolderInclude.Text, 
                    txtFolderExclude.Text, 
                    chkFolderDate.Checked, 
                    _dateFormat, 
                    cboFolderDateOp.Text, 
                    dtpFolderDateValue.Value,
                    txtFileInclude.Text, 
                    txtFileExclude.Text, 
                    chkFileDate.Checked, 
                    _dateFormat, 
                    cboFileDateOp.Text, 
                    dtpFileDateValue.Value, 
                    tempDir
                );
                
                // 更新總檔案數
                _totalFiles = files.Count;
                _processedFiles = 0;
                SafeAddLog(lstLog, $"找到 {_totalFiles} 個符合條件的CSV檔案");
                
                if (_totalFiles == 0)
                {
                    SafeAddLog(lstLog, "沒有找到符合條件的檔案，處理結束");
                    btnExecute.Enabled = true;
                    btnCancel.Enabled = false;
                    _cts.Dispose();
                    _cts = null;
                    return;
                }
                
                // 處理每個檔案並收集結果（臨時文件路徑）
                var csvProcessor = new Utils.CsvProcessor(
                    message => {
                        if (EnableDebugLog || !message.StartsWith("[Debug]"))
                            SafeAddLog(lstLog, message);
                    }, 
                    _cts.Token, 
                    _dateFormat
                );
                
                var tempFiles = new ConcurrentBag<string>();
                var parallelOptions = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = _cts.Token
                };
                
                await Task.Run(() =>
                {
                    Parallel.ForEach(files, parallelOptions, async (file) =>
                    {
                        var tempFile = await csvProcessor.ProcessCsvFileAsync(
                            file, 
                            columnConfigs, 
                            filterConditions, 
                            _addFileNameColumn, 
                            _addDirectoryNameColumn, 
                            tempDir,
                            _skipIncompleteFiles
                        );
                        
                        if (tempFile != null)
                        {
                            tempFiles.Add(tempFile);
                        }
                        
                        Interlocked.Increment(ref _processedFiles);
                        
                        // 更新進度條
                        this.Invoke((Action)(() =>
                        {
                            progressBar.Value = (int)((_processedFiles / (double)_totalFiles) * 100);
                            // 使用進度訊息更新日誌
                            SafeAddLog(lstLog, $"進度: {_processedFiles} / {_totalFiles}");
                        }));
                    });
                }, _cts.Token);
                
                if (_cts.Token.IsCancellationRequested)
                {
                    SafeAddLog(lstLog, "處理已取消");
                    btnExecute.Enabled = true;
                    btnCancel.Enabled = false;
                    fileHelper.CleanupTempFiles(tempFiles.ToList(), tempDir);
                    return;
                }
                
                SafeAddLog(lstLog, $"處理完成，共處理 {_processedFiles} 個檔案，耗時 {(DateTime.Now - startTime).TotalSeconds:F1} 秒");
                
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
                
                // 生成目標檔案路徑
                string targetFilePath = Path.Combine(_targetPath, baseFileName);
                
                // 檢查檔案是否已存在，如果存在則使用不重複的檔名
                if (File.Exists(targetFilePath))
                {
                    targetFilePath = csvProcessor.GenerateUniqueFileName(_targetPath, baseFileName);
                    SafeAddLog(lstLog, $"檔案已存在，使用新檔名: {Path.GetFileName(targetFilePath)}");
                }
                
                // 執行合併
                var mergedFilePath = await csvProcessor.MergeCsvFilesOptimizedAsync(
                    tempFiles.ToList(), 
                    targetFilePath, 
                    (current, total) => this.Invoke((Action)(() =>
                    {
                        progressBar.Value = (int)((current / (double)total) * 100);
                        // 更新日誌
                        SafeAddLog(lstLog, $"合併進度: {current} / {total}");
                    }))
                );
                
                SafeAddLog(lstLog, $"合併完成，輸出文件: {mergedFilePath}");
                
                // 7. 清理暫存檔案（除非使用者選擇保留）
                if (!_keepTempFiles)
                {
                    SafeAddLog(lstLog, "正在清理暫存檔案...");
                    fileHelper.CleanupTempFiles(tempFiles.ToList(), tempDir);
                }
                else
                {
                    SafeAddLog(lstLog, $"已保留暫存檔案於: {tempDir}");
                }
                
                SafeAddLog(lstLog, "處理完成!");
                
                // 顯示完成訊息
                MessageBox.Show($"CSV檔案處理完成!\n輸出檔案: {mergedFilePath}", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // 恢復UI狀態
                btnExecute.Enabled = true;
                btnCancel.Enabled = false;
                progressBar.Value = 100;
                // 更新最終狀態到日誌
                SafeAddLog(lstLog, "完成");
                
                // 釋放資源
                _cts.Dispose();
                _cts = null;
            }
            catch (OperationCanceledException)
            {
                SafeAddLog(lstLog, "處理已取消");
                btnExecute.Enabled = true;
                btnCancel.Enabled = false;
                progressBar.Value = 0;
                // 更新取消狀態到日誌
                SafeAddLog(lstLog, "已取消");
            }
            catch (Exception ex)
            {
                SafeAddLog(lstLog, $"處理時發生錯誤: {ex.Message}");
                if (EnableDebugLog)
                    SafeAddLog(lstLog, $"詳細錯誤: {ex}");
                    
                MessageBox.Show($"處理時發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                btnExecute.Enabled = true;
                btnCancel.Enabled = false;
                progressBar.Value = 0;
                // 更新錯誤狀態到日誌
                SafeAddLog(lstLog, "錯誤");
            }
        }

        /// <summary>
        /// 執行緒安全地新增Log（UI執行緒同步），自動添加時間戳記並滾動到最新訊息
        /// </summary>
        private void SafeAddLog(ListBox logList, string message)
        {
            // 僅在啟用DebugLog時顯示[Debug]訊息
            if (message.StartsWith("[Debug]") && !EnableDebugLog)
                return;
            if (logList.InvokeRequired)
            {
                logList.Invoke(new Action(() =>
                {
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
            if (message.StartsWith("[Debug]") && !EnableDebugLog)
                return;
            if (lstLog != null)
            {
                // LogHelper.AddLog 已經會添加時間戳記，所以這裡不需要重複添加
                LogHelper.AddLog(this, lstLog, message);
                // 自動滾動到最新訊息並取消選取
                lstLog.TopIndex = lstLog.Items.Count - 1;
                lstLog.ClearSelected();
            }
        }

        #endregion 私有方法

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

        #endregion 事件處理方法
    }
}