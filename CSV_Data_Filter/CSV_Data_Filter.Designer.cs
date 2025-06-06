namespace CSV_Data_Filter
{
    partial class CSV_Data_Filter
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // 控制項成員變量
        private System.Windows.Forms.Label lblSourcePaths;
        private System.Windows.Forms.ListBox lstSourcePaths;
        private System.Windows.Forms.Button btnAddPath;
        private System.Windows.Forms.Button btnRemovePath;
        private System.Windows.Forms.GroupBox gbFolderFilter;
        private System.Windows.Forms.Label lblFolderInclude;
        private System.Windows.Forms.TextBox txtFolderInclude;
        private System.Windows.Forms.Label lblFolderExclude;
        private System.Windows.Forms.TextBox txtFolderExclude;
        private System.Windows.Forms.CheckBox chkFolderDate;
        private System.Windows.Forms.Label lblFolderDateFormat;
        private System.Windows.Forms.TextBox txtFolderDateFormat;
        private System.Windows.Forms.Label lblFolderDateOp;
        private System.Windows.Forms.ComboBox cboFolderDateOp;
        private System.Windows.Forms.Label lblFolderDateValue;
        private System.Windows.Forms.DateTimePicker dtpFolderDateValue;
        private System.Windows.Forms.GroupBox gbFileFilter;
        private System.Windows.Forms.Label lblFileInclude;
        private System.Windows.Forms.TextBox txtFileInclude;
        private System.Windows.Forms.Label lblFileExclude;
        private System.Windows.Forms.TextBox txtFileExclude;
        private System.Windows.Forms.CheckBox chkFileDate;
        private System.Windows.Forms.Label lblFileDateFormat;
        private System.Windows.Forms.TextBox txtFileDateFormat;
        private System.Windows.Forms.Label lblFileDateOp;
        private System.Windows.Forms.ComboBox cboFileDateOp;
        private System.Windows.Forms.Label lblFileDateValue;
        private System.Windows.Forms.DateTimePicker dtpFileDateValue;
        private System.Windows.Forms.GroupBox gbColumns;
        private System.Windows.Forms.Label lblAvailColumns;
        private System.Windows.Forms.ListBox lstAvailColumns;
        private System.Windows.Forms.Button btnGetColumns;
        private System.Windows.Forms.Label lblSelectedColumns;
        private System.Windows.Forms.ListBox lstSelectedColumns;
        private System.Windows.Forms.Button btnAddColumn;
        private System.Windows.Forms.Button btnRemoveColumn;
        private System.Windows.Forms.Button btnConfigColumn;
        private System.Windows.Forms.GroupBox gbFilter;
        private System.Windows.Forms.ListBox lstFilters;
        private System.Windows.Forms.Button btnAddFilter;
        private System.Windows.Forms.Button btnRemoveFilter;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.CheckBox chkAddFileName;
        private System.Windows.Forms.CheckBox chkAddDirectoryName;
        private System.Windows.Forms.Label lblThreads;
        private System.Windows.Forms.NumericUpDown nudThreads;
        private System.Windows.Forms.GroupBox gbTarget;
        private System.Windows.Forms.Label lblTargetPath;
        private System.Windows.Forms.TextBox txtTargetPath;
        private System.Windows.Forms.Button btnBrowseTarget;        
        private System.Windows.Forms.Button btnLoadConfig;
        private System.Windows.Forms.Button btnSaveConfig;
        private System.Windows.Forms.Label lblCurrentConfig;
        private System.Windows.Forms.Label lblOutputFileName;
        private System.Windows.Forms.TextBox txtOutputFileName;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkSkipIncompleteFiles;
        private System.Windows.Forms.CheckBox chkKeepTempFiles;
        private System.Windows.Forms.CheckBox chkDebugLog;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblSourcePaths = new Label();
            lstSourcePaths = new ListBox();
            btnAddPath = new Button();
            btnRemovePath = new Button();
            gbFolderFilter = new GroupBox();
            lblFolderInclude = new Label();
            txtFolderInclude = new TextBox();
            lblFolderExclude = new Label();
            txtFolderExclude = new TextBox();
            chkFolderDate = new CheckBox();
            lblFolderDateFormat = new Label();
            txtFolderDateFormat = new TextBox();
            lblFolderDateOp = new Label();
            cboFolderDateOp = new ComboBox();
            lblFolderDateValue = new Label();
            dtpFolderDateValue = new DateTimePicker();
            gbFileFilter = new GroupBox();
            lblFileInclude = new Label();
            txtFileInclude = new TextBox();
            lblFileExclude = new Label();
            txtFileExclude = new TextBox();
            chkFileDate = new CheckBox();
            lblFileDateFormat = new Label();
            txtFileDateFormat = new TextBox();
            lblFileDateOp = new Label();
            cboFileDateOp = new ComboBox();
            lblFileDateValue = new Label();
            dtpFileDateValue = new DateTimePicker();
            gbColumns = new GroupBox();
            lblAvailColumns = new Label();
            lstAvailColumns = new ListBox();
            btnGetColumns = new Button();
            lblSelectedColumns = new Label();
            lstSelectedColumns = new ListBox();
            btnAddColumn = new Button();
            btnRemoveColumn = new Button();
            btnConfigColumn = new Button();
            chkDebugLog = new CheckBox();
            gbFilter = new GroupBox();
            lstFilters = new ListBox();
            btnAddFilter = new Button();
            btnRemoveFilter = new Button();
            gbOptions = new GroupBox();
            chkAddFileName = new CheckBox();
            chkAddDirectoryName = new CheckBox();
            lblThreads = new Label();
            nudThreads = new NumericUpDown();
            chkSkipIncompleteFiles = new CheckBox();
            chkKeepTempFiles = new CheckBox();
            gbTarget = new GroupBox();
            lblTargetPath = new Label();
            txtTargetPath = new TextBox();
            btnBrowseTarget = new Button();
            lblOutputFileName = new Label();
            txtOutputFileName = new TextBox();
            progressBar = new ProgressBar();
            lstLog = new ListBox();
            btnExecute = new Button();
            btnCancel = new Button();
            btnLoadConfig = new Button();
            btnSaveConfig = new Button();
            lblCurrentConfig = new Label();
            gbFolderFilter.SuspendLayout();
            gbFileFilter.SuspendLayout();
            gbColumns.SuspendLayout();
            gbFilter.SuspendLayout();
            gbOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudThreads).BeginInit();
            gbTarget.SuspendLayout();
            SuspendLayout();
            // 
            // lblSourcePaths
            // 
            lblSourcePaths.AutoSize = true;
            lblSourcePaths.Location = new Point(10, 18);
            lblSourcePaths.Name = "lblSourcePaths";
            lblSourcePaths.Size = new Size(82, 15);
            lblSourcePaths.TabIndex = 0;
            lblSourcePaths.Text = "資料來源路徑:";
            // 
            // lstSourcePaths
            // 
            lstSourcePaths.FormattingEnabled = true;
            lstSourcePaths.ItemHeight = 15;
            lstSourcePaths.Location = new Point(10, 35);
            lstSourcePaths.Name = "lstSourcePaths";
            lstSourcePaths.Size = new Size(450, 79);
            lstSourcePaths.TabIndex = 1;
            // 
            // btnAddPath
            // 
            btnAddPath.Location = new Point(470, 35);
            btnAddPath.Name = "btnAddPath";
            btnAddPath.Size = new Size(100, 26);
            btnAddPath.TabIndex = 2;
            btnAddPath.Text = "新增路徑";
            btnAddPath.UseVisualStyleBackColor = true;
            btnAddPath.Click += btnAddPath_Click;
            // 
            // btnRemovePath
            // 
            btnRemovePath.Location = new Point(470, 71);
            btnRemovePath.Name = "btnRemovePath";
            btnRemovePath.Size = new Size(100, 26);
            btnRemovePath.TabIndex = 3;
            btnRemovePath.Text = "移除路徑";
            btnRemovePath.UseVisualStyleBackColor = true;
            btnRemovePath.Click += btnRemovePath_Click;
            // 
            // gbFolderFilter
            // 
            gbFolderFilter.Controls.Add(lblFolderInclude);
            gbFolderFilter.Controls.Add(txtFolderInclude);
            gbFolderFilter.Controls.Add(lblFolderExclude);
            gbFolderFilter.Controls.Add(txtFolderExclude);
            gbFolderFilter.Controls.Add(chkFolderDate);
            gbFolderFilter.Controls.Add(lblFolderDateFormat);
            gbFolderFilter.Controls.Add(txtFolderDateFormat);
            gbFolderFilter.Controls.Add(lblFolderDateOp);
            gbFolderFilter.Controls.Add(cboFolderDateOp);
            gbFolderFilter.Controls.Add(lblFolderDateValue);
            gbFolderFilter.Controls.Add(dtpFolderDateValue);
            gbFolderFilter.Location = new Point(10, 132);
            gbFolderFilter.Name = "gbFolderFilter";
            gbFolderFilter.Size = new Size(580, 124);
            gbFolderFilter.TabIndex = 4;
            gbFolderFilter.TabStop = false;
            gbFolderFilter.Text = "資料夾篩選";
            // 
            // lblFolderInclude
            // 
            lblFolderInclude.AutoSize = true;
            lblFolderInclude.Location = new Point(10, 18);
            lblFolderInclude.Name = "lblFolderInclude";
            lblFolderInclude.Size = new Size(34, 15);
            lblFolderInclude.TabIndex = 0;
            lblFolderInclude.Text = "包含:";
            // 
            // txtFolderInclude
            // 
            txtFolderInclude.Location = new Point(60, 16);
            txtFolderInclude.Name = "txtFolderInclude";
            txtFolderInclude.Size = new Size(200, 23);
            txtFolderInclude.TabIndex = 1;
            // 
            // lblFolderExclude
            // 
            lblFolderExclude.AutoSize = true;
            lblFolderExclude.Location = new Point(280, 18);
            lblFolderExclude.Name = "lblFolderExclude";
            lblFolderExclude.Size = new Size(34, 15);
            lblFolderExclude.TabIndex = 2;
            lblFolderExclude.Text = "排除:";
            // 
            // txtFolderExclude
            // 
            txtFolderExclude.Location = new Point(330, 16);
            txtFolderExclude.Name = "txtFolderExclude";
            txtFolderExclude.Size = new Size(200, 23);
            txtFolderExclude.TabIndex = 3;
            // 
            // chkFolderDate
            // 
            chkFolderDate.AutoSize = true;
            chkFolderDate.Location = new Point(10, 44);
            chkFolderDate.Name = "chkFolderDate";
            chkFolderDate.Size = new Size(74, 19);
            chkFolderDate.TabIndex = 4;
            chkFolderDate.Text = "日期篩選";
            chkFolderDate.UseVisualStyleBackColor = true;
            // 
            // lblFolderDateFormat
            // 
            lblFolderDateFormat.AutoSize = true;
            lblFolderDateFormat.Location = new Point(100, 46);
            lblFolderDateFormat.Name = "lblFolderDateFormat";
            lblFolderDateFormat.Size = new Size(58, 15);
            lblFolderDateFormat.TabIndex = 5;
            lblFolderDateFormat.Text = "日期格式:";
            // 
            // txtFolderDateFormat
            // 
            txtFolderDateFormat.Location = new Point(170, 44);
            txtFolderDateFormat.Name = "txtFolderDateFormat";
            txtFolderDateFormat.Size = new Size(100, 23);
            txtFolderDateFormat.TabIndex = 6;
            txtFolderDateFormat.Text = "yyyy-MM-dd";
            // 
            // lblFolderDateOp
            // 
            lblFolderDateOp.AutoSize = true;
            lblFolderDateOp.Location = new Point(10, 71);
            lblFolderDateOp.Name = "lblFolderDateOp";
            lblFolderDateOp.Size = new Size(58, 15);
            lblFolderDateOp.TabIndex = 7;
            lblFolderDateOp.Text = "比較運算:";
            // 
            // cboFolderDateOp
            // 
            cboFolderDateOp.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFolderDateOp.FormattingEnabled = true;
            cboFolderDateOp.Items.AddRange(new object[] { ">", ">=", "<", "<=", "=" });
            cboFolderDateOp.Location = new Point(100, 69);
            cboFolderDateOp.Name = "cboFolderDateOp";
            cboFolderDateOp.Size = new Size(100, 23);
            cboFolderDateOp.TabIndex = 8;
            // 
            // lblFolderDateValue
            // 
            lblFolderDateValue.AutoSize = true;
            lblFolderDateValue.Location = new Point(210, 71);
            lblFolderDateValue.Name = "lblFolderDateValue";
            lblFolderDateValue.Size = new Size(58, 15);
            lblFolderDateValue.TabIndex = 9;
            lblFolderDateValue.Text = "比較日期:";
            // 
            // dtpFolderDateValue
            // 
            dtpFolderDateValue.Format = DateTimePickerFormat.Short;
            dtpFolderDateValue.Location = new Point(280, 69);
            dtpFolderDateValue.Name = "dtpFolderDateValue";
            dtpFolderDateValue.Size = new Size(150, 23);
            dtpFolderDateValue.TabIndex = 10;
            // 
            // gbFileFilter
            // 
            gbFileFilter.Controls.Add(lblFileInclude);
            gbFileFilter.Controls.Add(txtFileInclude);
            gbFileFilter.Controls.Add(lblFileExclude);
            gbFileFilter.Controls.Add(txtFileExclude);
            gbFileFilter.Controls.Add(chkFileDate);
            gbFileFilter.Controls.Add(lblFileDateFormat);
            gbFileFilter.Controls.Add(txtFileDateFormat);
            gbFileFilter.Controls.Add(lblFileDateOp);
            gbFileFilter.Controls.Add(cboFileDateOp);
            gbFileFilter.Controls.Add(lblFileDateValue);
            gbFileFilter.Controls.Add(dtpFileDateValue);
            gbFileFilter.Location = new Point(10, 265);
            gbFileFilter.Name = "gbFileFilter";
            gbFileFilter.Size = new Size(580, 124);
            gbFileFilter.TabIndex = 5;
            gbFileFilter.TabStop = false;
            gbFileFilter.Text = "檔案篩選";
            // 
            // lblFileInclude
            // 
            lblFileInclude.AutoSize = true;
            lblFileInclude.Location = new Point(10, 18);
            lblFileInclude.Name = "lblFileInclude";
            lblFileInclude.Size = new Size(34, 15);
            lblFileInclude.TabIndex = 0;
            lblFileInclude.Text = "包含:";
            // 
            // txtFileInclude
            // 
            txtFileInclude.Location = new Point(60, 16);
            txtFileInclude.Name = "txtFileInclude";
            txtFileInclude.Size = new Size(200, 23);
            txtFileInclude.TabIndex = 1;
            // 
            // lblFileExclude
            // 
            lblFileExclude.AutoSize = true;
            lblFileExclude.Location = new Point(280, 18);
            lblFileExclude.Name = "lblFileExclude";
            lblFileExclude.Size = new Size(34, 15);
            lblFileExclude.TabIndex = 2;
            lblFileExclude.Text = "排除:";
            // 
            // txtFileExclude
            // 
            txtFileExclude.Location = new Point(330, 16);
            txtFileExclude.Name = "txtFileExclude";
            txtFileExclude.Size = new Size(200, 23);
            txtFileExclude.TabIndex = 3;
            // 
            // chkFileDate
            // 
            chkFileDate.AutoSize = true;
            chkFileDate.Location = new Point(10, 44);
            chkFileDate.Name = "chkFileDate";
            chkFileDate.Size = new Size(74, 19);
            chkFileDate.TabIndex = 4;
            chkFileDate.Text = "日期篩選";
            chkFileDate.UseVisualStyleBackColor = true;
            // 
            // lblFileDateFormat
            // 
            lblFileDateFormat.AutoSize = true;
            lblFileDateFormat.Location = new Point(100, 46);
            lblFileDateFormat.Name = "lblFileDateFormat";
            lblFileDateFormat.Size = new Size(58, 15);
            lblFileDateFormat.TabIndex = 5;
            lblFileDateFormat.Text = "日期格式:";
            // 
            // txtFileDateFormat
            // 
            txtFileDateFormat.Location = new Point(170, 44);
            txtFileDateFormat.Name = "txtFileDateFormat";
            txtFileDateFormat.Size = new Size(100, 23);
            txtFileDateFormat.TabIndex = 6;
            txtFileDateFormat.Text = "yyyy-MM-dd";
            // 
            // lblFileDateOp
            // 
            lblFileDateOp.AutoSize = true;
            lblFileDateOp.Location = new Point(10, 71);
            lblFileDateOp.Name = "lblFileDateOp";
            lblFileDateOp.Size = new Size(58, 15);
            lblFileDateOp.TabIndex = 7;
            lblFileDateOp.Text = "比較運算:";
            // 
            // cboFileDateOp
            // 
            cboFileDateOp.DropDownStyle = ComboBoxStyle.DropDownList;
            cboFileDateOp.FormattingEnabled = true;
            cboFileDateOp.Items.AddRange(new object[] { ">", ">=", "<", "<=", "=" });
            cboFileDateOp.Location = new Point(100, 69);
            cboFileDateOp.Name = "cboFileDateOp";
            cboFileDateOp.Size = new Size(100, 23);
            cboFileDateOp.TabIndex = 8;
            // 
            // lblFileDateValue
            // 
            lblFileDateValue.AutoSize = true;
            lblFileDateValue.Location = new Point(210, 71);
            lblFileDateValue.Name = "lblFileDateValue";
            lblFileDateValue.Size = new Size(58, 15);
            lblFileDateValue.TabIndex = 9;
            lblFileDateValue.Text = "比較日期:";
            // 
            // dtpFileDateValue
            // 
            dtpFileDateValue.Format = DateTimePickerFormat.Short;
            dtpFileDateValue.Location = new Point(280, 69);
            dtpFileDateValue.Name = "dtpFileDateValue";
            dtpFileDateValue.Size = new Size(150, 23);
            dtpFileDateValue.TabIndex = 10;
            // 
            // gbColumns
            // 
            gbColumns.Controls.Add(lblAvailColumns);
            gbColumns.Controls.Add(lstAvailColumns);
            gbColumns.Controls.Add(btnGetColumns);
            gbColumns.Controls.Add(lblSelectedColumns);
            gbColumns.Controls.Add(lstSelectedColumns);
            gbColumns.Controls.Add(btnAddColumn);
            gbColumns.Controls.Add(btnRemoveColumn);
            gbColumns.Controls.Add(btnConfigColumn);
            gbColumns.Controls.Add(chkDebugLog);
            gbColumns.Location = new Point(10, 397);
            gbColumns.Name = "gbColumns";
            gbColumns.Size = new Size(580, 168);
            gbColumns.TabIndex = 6;
            gbColumns.TabStop = false;
            gbColumns.Text = "欄位配置";
            // 
            // lblAvailColumns
            // 
            lblAvailColumns.AutoSize = true;
            lblAvailColumns.Location = new Point(10, 18);
            lblAvailColumns.Name = "lblAvailColumns";
            lblAvailColumns.Size = new Size(58, 15);
            lblAvailColumns.TabIndex = 0;
            lblAvailColumns.Text = "所有欄位:";
            // 
            // lstAvailColumns
            // 
            lstAvailColumns.FormattingEnabled = true;
            lstAvailColumns.ItemHeight = 15;
            lstAvailColumns.Location = new Point(10, 35);
            lstAvailColumns.Name = "lstAvailColumns";
            lstAvailColumns.Size = new Size(200, 79);
            lstAvailColumns.TabIndex = 1;
            // 
            // btnGetColumns
            // 
            btnGetColumns.Location = new Point(10, 124);
            btnGetColumns.Name = "btnGetColumns";
            btnGetColumns.Size = new Size(100, 26);
            btnGetColumns.TabIndex = 2;
            btnGetColumns.Text = "獲取欄位";
            btnGetColumns.UseVisualStyleBackColor = true;
            btnGetColumns.Click += btnGetColumns_Click;
            // 
            // lblSelectedColumns
            // 
            lblSelectedColumns.AutoSize = true;
            lblSelectedColumns.Location = new Point(220, 18);
            lblSelectedColumns.Name = "lblSelectedColumns";
            lblSelectedColumns.Size = new Size(70, 15);
            lblSelectedColumns.TabIndex = 3;
            lblSelectedColumns.Text = "選擇的欄位:";
            // 
            // lstSelectedColumns
            // 
            lstSelectedColumns.FormattingEnabled = true;
            lstSelectedColumns.ItemHeight = 15;
            lstSelectedColumns.Location = new Point(220, 35);
            lstSelectedColumns.Name = "lstSelectedColumns";
            lstSelectedColumns.Size = new Size(200, 79);
            lstSelectedColumns.TabIndex = 4;
            // 
            // btnAddColumn
            // 
            btnAddColumn.Location = new Point(430, 44);
            btnAddColumn.Name = "btnAddColumn";
            btnAddColumn.Size = new Size(40, 26);
            btnAddColumn.TabIndex = 5;
            btnAddColumn.Text = ">";
            btnAddColumn.UseVisualStyleBackColor = true;
            btnAddColumn.Click += btnAddColumn_Click;
            // 
            // btnRemoveColumn
            // 
            btnRemoveColumn.Location = new Point(430, 79);
            btnRemoveColumn.Name = "btnRemoveColumn";
            btnRemoveColumn.Size = new Size(40, 26);
            btnRemoveColumn.TabIndex = 6;
            btnRemoveColumn.Text = "<";
            btnRemoveColumn.UseVisualStyleBackColor = true;
            btnRemoveColumn.Click += btnRemoveColumn_Click;
            // 
            // btnConfigColumn
            // 
            btnConfigColumn.Location = new Point(480, 44);
            btnConfigColumn.Name = "btnConfigColumn";
            btnConfigColumn.Size = new Size(90, 26);
            btnConfigColumn.TabIndex = 7;
            btnConfigColumn.Text = "欄位設置";
            btnConfigColumn.UseVisualStyleBackColor = true;
            btnConfigColumn.Click += btnConfigColumn_Click;
            // 
            // chkDebugLog
            // 
            chkDebugLog.AutoSize = true;
            chkDebugLog.Location = new Point(10, 150);
            chkDebugLog.Name = "chkDebugLog";
            chkDebugLog.Size = new Size(114, 19);
            chkDebugLog.TabIndex = 8;
            chkDebugLog.Text = "啟用Debug Log";
            chkDebugLog.UseVisualStyleBackColor = true;
            // 
            // gbFilter
            // 
            gbFilter.Controls.Add(lstFilters);
            gbFilter.Controls.Add(btnAddFilter);
            gbFilter.Controls.Add(btnRemoveFilter);
            gbFilter.Location = new Point(600, 35);
            gbFilter.Name = "gbFilter";
            gbFilter.Size = new Size(380, 194);
            gbFilter.TabIndex = 7;
            gbFilter.TabStop = false;
            gbFilter.Text = "篩選條件";
            // 
            // lstFilters
            // 
            lstFilters.FormattingEnabled = true;
            lstFilters.ItemHeight = 15;
            lstFilters.Location = new Point(10, 18);
            lstFilters.Name = "lstFilters";
            lstFilters.Size = new Size(360, 139);
            lstFilters.TabIndex = 0;
            // 
            // btnAddFilter
            // 
            btnAddFilter.Location = new Point(10, 163);
            btnAddFilter.Name = "btnAddFilter";
            btnAddFilter.Size = new Size(100, 26);
            btnAddFilter.TabIndex = 1;
            btnAddFilter.Text = "新增條件";
            btnAddFilter.UseVisualStyleBackColor = true;
            btnAddFilter.Click += btnAddFilter_Click;
            // 
            // btnRemoveFilter
            // 
            btnRemoveFilter.Location = new Point(120, 163);
            btnRemoveFilter.Name = "btnRemoveFilter";
            btnRemoveFilter.Size = new Size(100, 26);
            btnRemoveFilter.TabIndex = 2;
            btnRemoveFilter.Text = "移除條件";
            btnRemoveFilter.UseVisualStyleBackColor = true;
            btnRemoveFilter.Click += btnRemoveFilter_Click;
            // 
            // gbOptions
            // 
            gbOptions.Controls.Add(chkAddFileName);
            gbOptions.Controls.Add(chkAddDirectoryName);
            gbOptions.Controls.Add(lblThreads);
            gbOptions.Controls.Add(nudThreads);
            gbOptions.Controls.Add(chkSkipIncompleteFiles);
            gbOptions.Controls.Add(chkKeepTempFiles);
            gbOptions.Location = new Point(600, 238);
            gbOptions.Name = "gbOptions";
            gbOptions.Size = new Size(380, 97);
            gbOptions.TabIndex = 8;
            gbOptions.TabStop = false;
            gbOptions.Text = "其他選項";
            // 
            // chkAddFileName
            // 
            chkAddFileName.AutoSize = true;
            chkAddFileName.Location = new Point(10, 18);
            chkAddFileName.Name = "chkAddFileName";
            chkAddFileName.Size = new Size(158, 19);
            chkAddFileName.TabIndex = 0;
            chkAddFileName.Text = "在最後一欄新增檔案名稱";
            chkAddFileName.UseVisualStyleBackColor = true;
            // 
            // chkAddDirectoryName
            // 
            chkAddDirectoryName.AutoSize = true;
            chkAddDirectoryName.Location = new Point(10, 44);
            chkAddDirectoryName.Name = "chkAddDirectoryName";
            chkAddDirectoryName.Size = new Size(158, 19);
            chkAddDirectoryName.TabIndex = 1;
            chkAddDirectoryName.Text = "在最後一欄新增目錄名稱";
            chkAddDirectoryName.UseVisualStyleBackColor = true;
            // 
            // lblThreads
            // 
            lblThreads.AutoSize = true;
            lblThreads.Location = new Point(200, 19);
            lblThreads.Name = "lblThreads";
            lblThreads.Size = new Size(82, 15);
            lblThreads.TabIndex = 2;
            lblThreads.Text = "處理執行緒數:";
            // 
            // nudThreads
            // 
            nudThreads.Location = new Point(300, 18);
            nudThreads.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
            nudThreads.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudThreads.Name = "nudThreads";
            nudThreads.Size = new Size(60, 23);
            nudThreads.TabIndex = 3;
            nudThreads.Value = new decimal(new int[] { 8, 0, 0, 0 });
            // 
            // chkSkipIncompleteFiles
            // 
            chkSkipIncompleteFiles.AutoSize = true;
            chkSkipIncompleteFiles.Checked = true;
            chkSkipIncompleteFiles.CheckState = CheckState.Checked;
            chkSkipIncompleteFiles.Location = new Point(10, 44);
            chkSkipIncompleteFiles.Name = "chkSkipIncompleteFiles";
            chkSkipIncompleteFiles.Size = new Size(122, 19);
            chkSkipIncompleteFiles.TabIndex = 4;
            chkSkipIncompleteFiles.Text = "跳過不完整的檔案";
            chkSkipIncompleteFiles.UseVisualStyleBackColor = true;
            // 
            // chkKeepTempFiles
            // 
            chkKeepTempFiles.AutoSize = true;
            chkKeepTempFiles.Location = new Point(10, 62);
            chkKeepTempFiles.Name = "chkKeepTempFiles";
            chkKeepTempFiles.Size = new Size(98, 19);
            chkKeepTempFiles.TabIndex = 5;
            chkKeepTempFiles.Text = "保留暫存檔案";
            chkKeepTempFiles.UseVisualStyleBackColor = true;
            // 
            // gbTarget
            // 
            gbTarget.Controls.Add(lblTargetPath);
            gbTarget.Controls.Add(txtTargetPath);
            gbTarget.Controls.Add(btnBrowseTarget);
            gbTarget.Controls.Add(lblOutputFileName);
            gbTarget.Controls.Add(txtOutputFileName);
            gbTarget.Location = new Point(600, 344);
            gbTarget.Name = "gbTarget";
            gbTarget.Size = new Size(380, 141);
            gbTarget.TabIndex = 9;
            gbTarget.TabStop = false;
            gbTarget.Text = "目標設置";
            // 
            // lblTargetPath
            // 
            lblTargetPath.AutoSize = true;
            lblTargetPath.Location = new Point(10, 18);
            lblTargetPath.Name = "lblTargetPath";
            lblTargetPath.Size = new Size(70, 15);
            lblTargetPath.TabIndex = 0;
            lblTargetPath.Text = "目標資料夾:";
            // 
            // txtTargetPath
            // 
            txtTargetPath.Location = new Point(10, 40);
            txtTargetPath.Name = "txtTargetPath";
            txtTargetPath.Size = new Size(280, 23);
            txtTargetPath.TabIndex = 1;
            // 
            // btnBrowseTarget
            // 
            btnBrowseTarget.Location = new Point(300, 40);
            btnBrowseTarget.Name = "btnBrowseTarget";
            btnBrowseTarget.Size = new Size(70, 20);
            btnBrowseTarget.TabIndex = 2;
            btnBrowseTarget.Text = "瀏覽...";
            btnBrowseTarget.UseVisualStyleBackColor = true;
            btnBrowseTarget.Click += btnBrowseTarget_Click;
            // 
            // lblOutputFileName
            // 
            lblOutputFileName.AutoSize = true;
            lblOutputFileName.Location = new Point(10, 71);
            lblOutputFileName.Name = "lblOutputFileName";
            lblOutputFileName.Size = new Size(82, 15);
            lblOutputFileName.TabIndex = 3;
            lblOutputFileName.Text = "自訂檔案名稱:";
            // 
            // txtOutputFileName
            // 
            txtOutputFileName.Location = new Point(110, 69);
            txtOutputFileName.Name = "txtOutputFileName";
            txtOutputFileName.Size = new Size(200, 23);
            txtOutputFileName.TabIndex = 4;
            txtOutputFileName.Text = "Merged_CSV";
            // 
            // progressBar
            // 
            progressBar.Location = new Point(10, 574);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(970, 20);
            progressBar.TabIndex = 10;
            // 
            // lstLog
            // 
            lstLog.FormattingEnabled = true;
            lstLog.HorizontalScrollbar = true;
            lstLog.ItemHeight = 15;
            lstLog.Location = new Point(10, 600);
            lstLog.Name = "lstLog";
            lstLog.SelectionMode = SelectionMode.MultiExtended;
            lstLog.Size = new Size(970, 109);
            lstLog.TabIndex = 11;
            lstLog.KeyDown += lstLog_KeyDown;
            // 
            // btnExecute
            // 
            btnExecute.Location = new Point(810, 494);
            btnExecute.Name = "btnExecute";
            btnExecute.Size = new Size(80, 26);
            btnExecute.TabIndex = 12;
            btnExecute.Text = "執行";
            btnExecute.UseVisualStyleBackColor = true;
            btnExecute.Click += btnExecute_Click;
            // 
            // btnCancel
            // 
            btnCancel.Enabled = false;
            btnCancel.Location = new Point(900, 494);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 26);
            btnCancel.TabIndex = 13;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnLoadConfig
            // 
            btnLoadConfig.Location = new Point(600, 494);
            btnLoadConfig.Name = "btnLoadConfig";
            btnLoadConfig.Size = new Size(80, 26);
            btnLoadConfig.TabIndex = 14;
            btnLoadConfig.Text = "載入設定";
            btnLoadConfig.UseVisualStyleBackColor = true;
            btnLoadConfig.Click += btnLoadConfig_Click;
            // 
            // btnSaveConfig
            // 
            btnSaveConfig.Location = new Point(688, 494);
            btnSaveConfig.Name = "btnSaveConfig";
            btnSaveConfig.Size = new Size(80, 26);
            btnSaveConfig.TabIndex = 15;
            btnSaveConfig.Text = "儲存設定";
            btnSaveConfig.UseVisualStyleBackColor = true;
            btnSaveConfig.Click += btnSaveConfig_Click;
            // 
            // lblCurrentConfig
            // 
            lblCurrentConfig.AutoSize = true;
            lblCurrentConfig.Location = new Point(597, 527);
            lblCurrentConfig.Name = "lblCurrentConfig";
            lblCurrentConfig.Size = new Size(0, 15);
            lblCurrentConfig.TabIndex = 16;
            // 
            // CSV_Data_Filter
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            ClientSize = new Size(1000, 724);
            Controls.Add(lblSourcePaths);
            Controls.Add(lstSourcePaths);
            Controls.Add(btnAddPath);
            Controls.Add(btnRemovePath);
            Controls.Add(gbFolderFilter);
            Controls.Add(gbFileFilter);
            Controls.Add(gbColumns);
            Controls.Add(gbFilter);
            Controls.Add(gbOptions);
            Controls.Add(gbTarget);
            Controls.Add(progressBar);
            Controls.Add(lstLog);
            Controls.Add(btnExecute);
            Controls.Add(btnCancel);
            Controls.Add(btnLoadConfig);
            Controls.Add(btnSaveConfig);
            Controls.Add(lblCurrentConfig);
            MinimumSize = new Size(800, 534);
            Name = "CSV_Data_Filter";
            Text = "CSV 資料整理工具";
            gbFolderFilter.ResumeLayout(false);
            gbFolderFilter.PerformLayout();
            gbFileFilter.ResumeLayout(false);
            gbFileFilter.PerformLayout();
            gbColumns.ResumeLayout(false);
            gbColumns.PerformLayout();
            gbFilter.ResumeLayout(false);
            gbOptions.ResumeLayout(false);
            gbOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudThreads).EndInit();
            gbTarget.ResumeLayout(false);
            gbTarget.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
