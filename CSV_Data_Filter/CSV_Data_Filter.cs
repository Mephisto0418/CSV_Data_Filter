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
        public const string PROJECT_VERSION = "1.0.4";
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
        #endregion

        #region 建構子
        /// <summary>
        /// 建構子，初始化UI元件
        /// </summary>
        public CSV_Data_Filter()
        {
            InitializeComponent();
            this.Text = $"{PROJECT_NAME} v{PROJECT_VERSION}";

            // 新增：雙擊所有欄位加入選擇欄位
            lstAvailColumns.MouseDoubleClick += (s, e) => {
                AddSelectedColumn(lstAvailColumns, lstSelectedColumns);
            };
            // 新增：雙擊選擇欄位移除
            lstSelectedColumns.MouseDoubleClick += (s, e) => {
                RemoveSelectedColumn(lstSelectedColumns);
            };
            
            // 啟用拖放功能
            lstAvailColumns.AllowDrop = true;
            lstSourcePaths.AllowDrop = true;
            
            // 註冊拖放事件處理函數
            lstAvailColumns.DragEnter += ListBox_DragEnter;
            lstAvailColumns.DragDrop += AvailColumns_DragDrop;
            
            lstSourcePaths.DragEnter += ListBox_DragEnter;
            lstSourcePaths.DragDrop += SourcePaths_DragDrop;
        }
        #endregion

        #region 拖放功能處理
        /// <summary>
        /// 處理拖放進入事件
        /// </summary>
        private void ListBox_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }
        
        /// <summary>
        /// 處理將CSV檔案拖放到欄位區域的事件
        /// </summary>
        private void AvailColumns_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            
            string[]? files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length == 0) return;
            
            // 尋找第一個CSV檔案
            string? csvFile = files.FirstOrDefault(f => f.EndsWith(".csv", StringComparison.OrdinalIgnoreCase));
            if (csvFile == null)
            {
                MessageBox.Show("請拖放CSV檔案以獲取欄位", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // 從CSV檔案獲取欄位
            GetColumnsFromFile(csvFile);
        }
        
        /// <summary>
        /// 處理將文件或資料夾拖放到資料來源路徑的事件
        /// </summary>
        private void SourcePaths_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            
            string[]? items = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (items == null || items.Length == 0) return;
            
            foreach (string item in items)
            {
                if (Directory.Exists(item))
                {
                    // 添加資料夾路徑
                    AddPathIfNotSubdirectory(item);
                }
                else if (File.Exists(item) && item.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    // 如果是CSV檔案，添加其所在的資料夾
                    string? directory = Path.GetDirectoryName(item);
                    if (!string.IsNullOrEmpty(directory))
                    {
                        AddPathIfNotSubdirectory(directory);
                    }
                }
            }
        }
        
        /// <summary>
        /// 添加路徑到資料來源路徑列表，檢查是否已存在相同路徑
        /// </summary>
        private void AddPathIfNotSubdirectory(string path)
        {
            // 檢查路徑是否已存在
            if (_sourcePaths.Contains(path))
            {
                AddLog(lstLog, $"路徑已存在: {path}");
                return;
            }
            
            // 添加新路徑
            _sourcePaths.Add(path);
            lstSourcePaths.Items.Add(path);
            AddLog(lstLog, $"已新增來源路徑: {path}");
            
            // 提示使用者如果存在子目錄與上層目錄的情況
            CheckAndLogPathRelationships();
        }
        
        /// <summary>
        /// 檢查資料來源路徑中是否存在子目錄與上層目錄的關係，並提示使用者
        /// </summary>
        private void CheckAndLogPathRelationships()
        {
            if (_sourcePaths.Count <= 1) return;
            
            // 查找所有的子目錄-上層目錄關係
            var relationshipFound = false;
            
            foreach (var path1 in _sourcePaths)
            {
                foreach (var path2 in _sourcePaths)
                {
                    if (path1 == path2) continue;
                    
                    // 檢查path1是否是path2的子目錄
                    string path2WithSep = path2.EndsWith(Path.DirectorySeparatorChar.ToString()) || 
                                         path2.EndsWith(Path.AltDirectorySeparatorChar.ToString())
                                        ? path2
                                        : path2 + Path.DirectorySeparatorChar;
                    
                    if (path1.StartsWith(path2WithSep, StringComparison.OrdinalIgnoreCase))
                    {
                        AddLog(lstLog, $"提示: 路徑 '{path1}' 是 '{path2}' 的子目錄");
                        AddLog(lstLog, $"      搜尋時將自動跳過子目錄以避免重複處理");
                        relationshipFound = true;
                    }
                }
            }
            
            if (relationshipFound)
            {
                AddLog(lstLog, "注意: 搜尋時只會處理包含所有子目錄的上層目錄，子目錄只會在UI中保留顯示");
            }
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

        /// <summary>
        /// 從指定的CSV檔案中獲取欄位
        /// </summary>
        private void GetColumnsFromFile(string filePath)
        {
            AddLog(lstLog, $"從檔案中讀取欄位: {Path.GetFileName(filePath)}");
            
            try
            {
                // 清空現有欄位
                lstAvailColumns.Items.Clear();
                
                // 讀取 CSV 標頭
                using (var reader = new StreamReader(filePath))
                {
                    string? headerLine = reader.ReadLine();
                    if (headerLine != null)
                    {
                        // 解析標頭行以獲取欄位名稱
                        var headers = headerLine.Split(',');
                        foreach (var header in headers)
                        {
                            // 移除引號並清理欄位名稱
                            string fieldName = header.Trim().Trim('"', '\'');
                            if (!string.IsNullOrWhiteSpace(fieldName))
                            {
                                lstAvailColumns.Items.Add(fieldName);
                            }
                        }
                        
                        AddLog(lstLog, $"成功讀取 {lstAvailColumns.Items.Count} 個欄位");
                    }
                    else
                    {
                        AddLog(lstLog, "CSV檔案標頭為空");
                    }
                }
            }
            catch (Exception ex)
            {
                AddLog(lstLog, $"讀取欄位時出錯: {ex.Message}");
                MessageBox.Show($"無法從檔案讀取欄位: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 遞迴尋找第一個符合條件的CSV檔案（副檔名.csv，可擴充為名稱包含/排除等）
        /// </summary>
        private string? FindFirstCsvFile(string dir)
        {
            try
            {
                if (EnableDebugLog)
                {
                    AddLog(lstLog, $"[Debug] 搜尋目錄: {dir}");
                }
                
                // 先檢查當前目錄中的文件
                var files = Directory.GetFiles(dir, "*.csv", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    if (EnableDebugLog)
                    {
                        AddLog(lstLog, $"[Debug] 在 {dir} 中找到 {files.Length} 個CSV檔案，返回第一個: {Path.GetFileName(files[0])}");
                    }
                    return files[0];
                }
                
                // 如果當前目錄沒有符合條件的文件，則檢查所有子目錄
                var subDirs = Directory.GetDirectories(dir);
                if (EnableDebugLog && subDirs.Length > 0)
                {
                    AddLog(lstLog, $"[Debug] 目錄 {dir} 沒有找到CSV檔案，將搜尋 {subDirs.Length} 個子目錄");
                }
                
                foreach (var sub in subDirs)
                {
                    var found = FindFirstCsvFile(sub);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            catch (Exception ex)
            {
                if (EnableDebugLog)
                {
                    AddLog(lstLog, $"[Debug] 搜尋目錄 {dir} 時出錯: {ex.Message}");
                }
            }
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
                    // 以最後一個 " (" 為分隔，並保留括號內容
                    int idx = selectedItem.LastIndexOf(" (");
                    if (idx >= 0 && selectedItem.EndsWith(")"))
                    {
                        var originalName = selectedItem.Substring(idx + 2, selectedItem.Length - idx - 3);
                        var config = _columnConfigs.Find(c => c.Name == originalName);

                        if (config != null)
                        {
                            if (EnableDebugLog)
                            {
                                AddLog(lstLog, $"[Debug] 設定欄位: {originalName}, CustomName: {config.CustomName}");
                            }

                            using (var configForm = new ColumnConfigForm(config))
                            {
                                if (configForm.ShowDialog() == DialogResult.OK)
                                {
                                    // 更新ListBox中的顯示文字
                                    selectedColumnsListBox.Items[selectedColumnsListBox.SelectedIndex] =
                                        $"{config.CustomName} ({config.Name})";
                                    AddLog(lstLog, $"已設定欄位 '{originalName}' 的處理方式");
                                }
                            }
                        }
                        else
                        {
                            AddLog(lstLog, $"警告: 找不到欄位配置: {originalName}");
                        }
                    }
                    else
                    {
                        // 處理舊格式或未含括號的項目
                        var config = _columnConfigs.Find(c => c.Name == selectedItem);
                        if (config != null)
                        {
                            using (var configForm = new ColumnConfigForm(config))
                            {
                                if (configForm.ShowDialog() == DialogResult.OK)
                                {
                                    // 更新ListBox中的顯示文字
                                    selectedColumnsListBox.Items[selectedColumnsListBox.SelectedIndex] =
                                        $"{config.CustomName} ({config.Name})";
                                    AddLog(lstLog, $"已設定欄位 '{selectedItem}' 的處理方式");
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
            var columnNames = selectedColumnsListBox.Items.Cast<string>().Select(item => {
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
            if (_cts != null)
            {
                SafeAddLog(lstLog, "已有任務執行中");
                return;
            }
            _cts = new CancellationTokenSource();
            try
            {
                // 設置按鈕狀態
                this.Invoke((Action)(() => {
                    btnExecute.Enabled = false;
                    btnCancel.Enabled = true;
                }));
                
                SafeAddLog(lstLog, "開始處理...");

                // 讀取自訂檔案名稱
                this.Invoke((Action)(() => {
                    _customFileName = txtOutputFileName.Text.Trim();
                    _addFileNameColumn = chkAddFileName.Checked;
                    _addDirectoryNameColumn = chkAddDirectoryName.Checked;
                    _skipIncompleteFiles = chkSkipIncompleteFiles.Checked;
                    _keepTempFiles = chkKeepTempFiles.Checked;
                }));

                // 1. 處理欄位選擇，若未選擇則自動選擇所有欄位
                var columnConfigs = new List<ColumnConfig>();
                bool configOk = false;
                this.Invoke((Action)(() => {
                    // 使用已配置的欄位配置
                    if (_columnConfigs.Count > 0)
                    {
                        columnConfigs = new List<ColumnConfig>(_columnConfigs);
                        configOk = true;
                    }
                    else
                    {
                        // 使用"所有欄位"清單中的項目建立預設配置
                        if (lstAvailColumns.Items.Count > 0)
                        {
                            foreach (var item in lstAvailColumns.Items)
                            {
                                if (item is string headerName && !string.IsNullOrEmpty(headerName))
                                {
                                    columnConfigs.Add(new ColumnConfig(headerName));
                                }
                            }
                            configOk = columnConfigs.Count > 0;
                        }
                        else
                        {
                            // 如果"所有欄位"是空的，顯示訊息
                            MessageBox.Show("請先選取CSV檔案或將檔案拖曳至「所有欄位」區域以獲取欄位配置。", 
                                "缺少欄位配置", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            configOk = false;
                            SafeAddLog(lstLog, "操作中斷：缺少欄位配置，請先取得欄位清單");
                        }
                    }
                }));
                
                // 如果沒有欄位配置，則中止處理
                if (!configOk)
                {
                    btnExecute.Enabled = true;
                    btnCancel.Enabled = false;
                    return;
                }                // 2. 建立輔助類別
                var fileHelper = new Utils.FileSystemHelper(msg => SafeAddLog(lstLog, msg), _cts.Token, _dateFormat);
                var csvProcessor = new Utils.CsvProcessor(msg => SafeAddLog(lstLog, msg), _cts.Token, _dateFormat, fileHelper);

                // 3. 建立唯一暫存目錄
                var tempDir = fileHelper.CreateTempDirectory();

                // 4. 異步尋找所有符合條件的 CSV 檔案（避免UI鎖死）
                SafeAddLog(lstLog, "正在搜尋符合條件的CSV檔案...");
                
                if (EnableDebugLog)
                {
                    foreach (var sourcePath in _sourcePaths)
                    {
                        SafeAddLog(lstLog, $"[Debug] 搜尋源路徑: {sourcePath}");
                        var testDirs = Directory.GetDirectories(sourcePath);
                        foreach (var testDir in testDirs)
                        {
                            SafeAddLog(lstLog, $"[Debug] 發現子目錄: {Path.GetFileName(testDir)}");
                        }
                    }
                }
                
                // 執行檔案搜尋，這會自動處理網路路徑複製到本地暫存
                var result = await fileHelper.FindCsvFilesAsync(
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
                
                var matchingFiles = result.Files;
                var networkPathMappings = result.NetworkPathMappings;
                
                // 將網路路徑映射傳遞給CSV處理器
                if (networkPathMappings != null && networkPathMappings.Count > 0)
                {
                    csvProcessor.AddNetworkPathMappings(networkPathMappings);
                }
                
                _totalFiles = matchingFiles.Count;
                _processedFiles = 0;
                
                if (_totalFiles == 0)
                {
                    SafeAddLog(lstLog, "找不到符合條件的檔案");
                    
                    // 輸出更多調試信息以定位問題
                    if (EnableDebugLog)
                    {
                        SafeAddLog(lstLog, $"[Debug] 資料夾包含條件: \"{txtFolderInclude.Text}\"");
                        SafeAddLog(lstLog, $"[Debug] 資料夾排除條件: \"{txtFolderExclude.Text}\"");
                        SafeAddLog(lstLog, $"[Debug] 檔案包含條件: \"{txtFileInclude.Text}\"");
                        SafeAddLog(lstLog, $"[Debug] 檔案排除條件: \"{txtFileExclude.Text}\"");
                        
                        // 手動檢查test_dir1和test_dir2目錄
                        foreach (var sourcePath in _sourcePaths)
                        {
                            var testDir1 = Path.Combine(sourcePath, "test_dir1");
                            var testDir2 = Path.Combine(sourcePath, "test_dir2");
                            
                            if (Directory.Exists(testDir1))
                            {
                                SafeAddLog(lstLog, $"[Debug] 檢查 test_dir1: {testDir1}");
                                var files = Directory.GetFiles(testDir1, "*.csv", SearchOption.AllDirectories);
                                SafeAddLog(lstLog, $"[Debug] test_dir1 中找到 {files.Length} 個 CSV 檔案");
                                foreach (var file in files.Take(5)) // 只顯示前5個檔案
                                {
                                    SafeAddLog(lstLog, $"[Debug] 檔案: {Path.GetFileName(file)}");
                                }
                            }
                            
                            if (Directory.Exists(testDir2))
                            {
                                SafeAddLog(lstLog, $"[Debug] 檢查 test_dir2: {testDir2}");
                                var files = Directory.GetFiles(testDir2, "*.csv", SearchOption.AllDirectories);
                                SafeAddLog(lstLog, $"[Debug] test_dir2 中找到 {files.Length} 個 CSV 檔案");
                                foreach (var file in files.Take(5)) // 只顯示前5個檔案
                                {
                                    SafeAddLog(lstLog, $"[Debug] 檔案: {Path.GetFileName(file)}");
                                }
                            }
                        }
                    }
                    
                    btnExecute.Enabled = true;
                    btnCancel.Enabled = false;
                    return;
                }
                
                SafeAddLog(lstLog, $"找到 {_totalFiles} 個符合條件的 CSV 檔案");
                
                // 在調試模式下顯示找到的檔案分佈
                if (EnableDebugLog)
                {
                    var filesByDir = matchingFiles.GroupBy(f => Path.GetDirectoryName(f) ?? string.Empty)
                                                .ToDictionary(g => g.Key, g => g.Count());
                    foreach (var dir in filesByDir)
                    {
                        SafeAddLog(lstLog, $"[Debug] 目錄 {dir.Key} 中找到 {dir.Value} 個符合條件的檔案");
                    }
                }
                
                // 設置進度條範圍
                this.Invoke((Action)(() => {
                progressBar.Maximum = _totalFiles;
                progressBar.Value = 0;
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
                                _addDirectoryNameColumn,
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
                    
                    // 檢查目標路徑，如果未設定則使用「文件」資料夾
                    if (string.IsNullOrWhiteSpace(_targetPath))
                    {
                        string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        _targetPath = documentsPath;
                        SafeAddLog(lstLog, $"未指定目標資料夾，預設使用「文件」資料夾: {documentsPath}");
                        
                        // 更新UI顯示
                        this.Invoke((Action)(() => {
                            txtTargetPath.Text = documentsPath;
                        }));
                    }
                      string finalFile = csvProcessor.GenerateUniqueFileName(_targetPath, baseFileName);
                    
                    await csvProcessor.MergeCsvFilesOptimizedAsync(tempFiles, finalFile, columnConfigs, (processed, total) => {
                        this.Invoke((Action)(() => {
                            progressBar.Value = Math.Min(progressBar.Maximum, processed);
                        }));
                    });
                    
                    SafeAddLog(lstLog, $"處理完成，結果已保存至: {finalFile}");
                    
                    // 7. 根據設定決定是否清理臨時檔案
                    if (_keepTempFiles)
                    {
                        SafeAddLog(lstLog, $"暫存檔案已保留於: {tempDir}");
                        SafeAddLog(lstLog, $"暫存檔案數量: {tempFiles.Count}");
                    }
                    else
                    {
                        fileHelper.CleanupTempFiles(tempFiles, tempDir);
                        SafeAddLog(lstLog, "已清理暫存檔案");
                    }
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
            // 僅在啟用DebugLog時顯示[Debug]訊息
            if (message.StartsWith("[Debug]") && !EnableDebugLog)
                return;
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

        private void GetCsvColumns(ListBox sourcePathsListBox, ListBox availColumnsListBox)
        {
            // 使用檔案選擇對話框讓使用者選擇CSV檔案
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "CSV檔案 (*.csv)|*.csv|所有檔案 (*.*)|*.*";
                dialog.Title = "選擇CSV檔案以取得欄位";
                
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFile = dialog.FileName;
                    GetColumnsFromFile(selectedFile);
                }
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
