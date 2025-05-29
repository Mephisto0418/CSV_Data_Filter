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
        private System.Windows.Forms.ListBox lstSourcePaths;        private System.Windows.Forms.Button btnAddPath;
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
        private System.Windows.Forms.Label lblOutputFileName;
        private System.Windows.Forms.TextBox txtOutputFileName;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.ListBox lstLog;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkSkipIncompleteFiles;
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
            this.lblSourcePaths = new System.Windows.Forms.Label();
            this.lstSourcePaths = new System.Windows.Forms.ListBox();
            this.btnAddPath = new System.Windows.Forms.Button();
            this.btnRemovePath = new System.Windows.Forms.Button();
            this.gbFolderFilter = new System.Windows.Forms.GroupBox();
            this.lblFolderInclude = new System.Windows.Forms.Label();
            this.txtFolderInclude = new System.Windows.Forms.TextBox();
            this.lblFolderExclude = new System.Windows.Forms.Label();
            this.txtFolderExclude = new System.Windows.Forms.TextBox();
            this.chkFolderDate = new System.Windows.Forms.CheckBox();
            this.lblFolderDateFormat = new System.Windows.Forms.Label();
            this.txtFolderDateFormat = new System.Windows.Forms.TextBox();
            this.lblFolderDateOp = new System.Windows.Forms.Label();
            this.cboFolderDateOp = new System.Windows.Forms.ComboBox();
            this.lblFolderDateValue = new System.Windows.Forms.Label();
            this.dtpFolderDateValue = new System.Windows.Forms.DateTimePicker();
            this.gbFileFilter = new System.Windows.Forms.GroupBox();
            this.lblFileInclude = new System.Windows.Forms.Label();
            this.txtFileInclude = new System.Windows.Forms.TextBox();
            this.lblFileExclude = new System.Windows.Forms.Label();
            this.txtFileExclude = new System.Windows.Forms.TextBox();
            this.chkFileDate = new System.Windows.Forms.CheckBox();
            this.lblFileDateFormat = new System.Windows.Forms.Label();
            this.txtFileDateFormat = new System.Windows.Forms.TextBox();
            this.lblFileDateOp = new System.Windows.Forms.Label();
            this.cboFileDateOp = new System.Windows.Forms.ComboBox();
            this.lblFileDateValue = new System.Windows.Forms.Label();
            this.dtpFileDateValue = new System.Windows.Forms.DateTimePicker();
            this.gbColumns = new System.Windows.Forms.GroupBox();
            this.lblAvailColumns = new System.Windows.Forms.Label();
            this.lstAvailColumns = new System.Windows.Forms.ListBox();
            this.btnGetColumns = new System.Windows.Forms.Button();
            this.lblSelectedColumns = new System.Windows.Forms.Label();
            this.lstSelectedColumns = new System.Windows.Forms.ListBox();
            this.btnAddColumn = new System.Windows.Forms.Button();
            this.btnRemoveColumn = new System.Windows.Forms.Button();
            this.btnConfigColumn = new System.Windows.Forms.Button();
            this.gbFilter = new System.Windows.Forms.GroupBox();
            this.lstFilters = new System.Windows.Forms.ListBox();
            this.btnAddFilter = new System.Windows.Forms.Button();
            this.btnRemoveFilter = new System.Windows.Forms.Button();
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.chkAddFileName = new System.Windows.Forms.CheckBox();
            this.chkAddDirectoryName = new System.Windows.Forms.CheckBox();
            this.lblThreads = new System.Windows.Forms.Label();
            this.nudThreads = new System.Windows.Forms.NumericUpDown();
            this.chkSkipIncompleteFiles = new System.Windows.Forms.CheckBox();
            this.chkDebugLog = new System.Windows.Forms.CheckBox();
            this.gbTarget = new System.Windows.Forms.GroupBox();
            this.lblTargetPath = new System.Windows.Forms.Label();
            this.txtTargetPath = new System.Windows.Forms.TextBox();
            this.btnBrowseTarget = new System.Windows.Forms.Button();
            this.lblOutputFileName = new System.Windows.Forms.Label();
            this.txtOutputFileName = new System.Windows.Forms.TextBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.lstLog = new System.Windows.Forms.ListBox();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.gbFolderFilter.SuspendLayout();
            this.gbFileFilter.SuspendLayout();
            this.gbColumns.SuspendLayout();
            this.gbFilter.SuspendLayout();
            this.gbOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreads)).BeginInit();
            this.gbTarget.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSourcePaths
            // 
            this.lblSourcePaths.AutoSize = true;
            this.lblSourcePaths.Location = new System.Drawing.Point(10, 20);
            this.lblSourcePaths.Name = "lblSourcePaths";
            this.lblSourcePaths.Size = new System.Drawing.Size(97, 19);
            this.lblSourcePaths.TabIndex = 0;
            this.lblSourcePaths.Text = "資料來源路徑:";
            // 
            // lstSourcePaths
            // 
            this.lstSourcePaths.FormattingEnabled = true;
            this.lstSourcePaths.ItemHeight = 19;
            this.lstSourcePaths.Location = new System.Drawing.Point(10, 40);
            this.lstSourcePaths.Name = "lstSourcePaths";
            this.lstSourcePaths.Size = new System.Drawing.Size(450, 100);
            this.lstSourcePaths.TabIndex = 1;
            // 
            // btnAddPath
            // 
            this.btnAddPath.Location = new System.Drawing.Point(470, 40);
            this.btnAddPath.Name = "btnAddPath";            this.btnAddPath.Size = new System.Drawing.Size(100, 30);
            this.btnAddPath.TabIndex = 2;
            this.btnAddPath.Text = "新增路徑";
            this.btnAddPath.UseVisualStyleBackColor = true;
            this.btnAddPath.Click += new System.EventHandler(this.btnAddPath_Click);
            // 
            // btnRemovePath
            // 
            this.btnRemovePath.Location = new System.Drawing.Point(470, 80);            this.btnRemovePath.Name = "btnRemovePath";
            this.btnRemovePath.Size = new System.Drawing.Size(100, 30);
            this.btnRemovePath.TabIndex = 3;
            this.btnRemovePath.Text = "移除路徑";
            this.btnRemovePath.UseVisualStyleBackColor = true;
            this.btnRemovePath.Click += new System.EventHandler(this.btnRemovePath_Click);
            // 
            // gbFolderFilter
            // 
            this.gbFolderFilter.Controls.Add(this.lblFolderInclude);
            this.gbFolderFilter.Controls.Add(this.txtFolderInclude);
            this.gbFolderFilter.Controls.Add(this.lblFolderExclude);
            this.gbFolderFilter.Controls.Add(this.txtFolderExclude);
            this.gbFolderFilter.Controls.Add(this.chkFolderDate);
            this.gbFolderFilter.Controls.Add(this.lblFolderDateFormat);
            this.gbFolderFilter.Controls.Add(this.txtFolderDateFormat);
            this.gbFolderFilter.Controls.Add(this.lblFolderDateOp);
            this.gbFolderFilter.Controls.Add(this.cboFolderDateOp);
            this.gbFolderFilter.Controls.Add(this.lblFolderDateValue);
            this.gbFolderFilter.Controls.Add(this.dtpFolderDateValue);
            this.gbFolderFilter.Location = new System.Drawing.Point(10, 150);
            this.gbFolderFilter.Name = "gbFolderFilter";
            this.gbFolderFilter.Size = new System.Drawing.Size(580, 140);
            this.gbFolderFilter.TabIndex = 4;
            this.gbFolderFilter.TabStop = false;
            this.gbFolderFilter.Text = "資料夾篩選";
            // 
            // lblFolderInclude
            // 
            this.lblFolderInclude.AutoSize = true;
            this.lblFolderInclude.Location = new System.Drawing.Point(10, 20);
            this.lblFolderInclude.Name = "lblFolderInclude";
            this.lblFolderInclude.Size = new System.Drawing.Size(42, 19);
            this.lblFolderInclude.TabIndex = 0;
            this.lblFolderInclude.Text = "包含:";
            // 
            // txtFolderInclude
            // 
            this.txtFolderInclude.Location = new System.Drawing.Point(60, 18);
            this.txtFolderInclude.Name = "txtFolderInclude";
            this.txtFolderInclude.Size = new System.Drawing.Size(200, 23);
            this.txtFolderInclude.TabIndex = 1;
            // 
            // lblFolderExclude
            // 
            this.lblFolderExclude.AutoSize = true;
            this.lblFolderExclude.Location = new System.Drawing.Point(280, 20);
            this.lblFolderExclude.Name = "lblFolderExclude";
            this.lblFolderExclude.Size = new System.Drawing.Size(42, 19);
            this.lblFolderExclude.TabIndex = 2;
            this.lblFolderExclude.Text = "排除:";
            // 
            // txtFolderExclude
            // 
            this.txtFolderExclude.Location = new System.Drawing.Point(330, 18);
            this.txtFolderExclude.Name = "txtFolderExclude";
            this.txtFolderExclude.Size = new System.Drawing.Size(200, 23);
            this.txtFolderExclude.TabIndex = 3;
            // 
            // chkFolderDate
            // 
            this.chkFolderDate.AutoSize = true;
            this.chkFolderDate.Location = new System.Drawing.Point(10, 50);
            this.chkFolderDate.Name = "chkFolderDate";
            this.chkFolderDate.Size = new System.Drawing.Size(86, 23);
            this.chkFolderDate.TabIndex = 4;
            this.chkFolderDate.Text = "日期篩選";
            this.chkFolderDate.UseVisualStyleBackColor = true;
            // 
            // lblFolderDateFormat
            // 
            this.lblFolderDateFormat.AutoSize = true;
            this.lblFolderDateFormat.Location = new System.Drawing.Point(100, 52);
            this.lblFolderDateFormat.Name = "lblFolderDateFormat";
            this.lblFolderDateFormat.Size = new System.Drawing.Size(67, 19);
            this.lblFolderDateFormat.TabIndex = 5;
            this.lblFolderDateFormat.Text = "日期格式:";
            // 
            // txtFolderDateFormat
            // 
            this.txtFolderDateFormat.Location = new System.Drawing.Point(170, 50);
            this.txtFolderDateFormat.Name = "txtFolderDateFormat";
            this.txtFolderDateFormat.Size = new System.Drawing.Size(100, 23);
            this.txtFolderDateFormat.TabIndex = 6;
            this.txtFolderDateFormat.Text = "yyyy-MM-dd";
            // 
            // lblFolderDateOp
            // 
            this.lblFolderDateOp.AutoSize = true;
            this.lblFolderDateOp.Location = new System.Drawing.Point(10, 80);
            this.lblFolderDateOp.Name = "lblFolderDateOp";
            this.lblFolderDateOp.Size = new System.Drawing.Size(67, 19);
            this.lblFolderDateOp.TabIndex = 7;
            this.lblFolderDateOp.Text = "比較運算:";
            // 
            // cboFolderDateOp
            // 
            this.cboFolderDateOp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFolderDateOp.FormattingEnabled = true;
            this.cboFolderDateOp.Items.AddRange(new object[] {
            ">",
            ">=",
            "<",
            "<=",
            "="});
            this.cboFolderDateOp.Location = new System.Drawing.Point(100, 78);
            this.cboFolderDateOp.Name = "cboFolderDateOp";
            this.cboFolderDateOp.Size = new System.Drawing.Size(100, 27);
            this.cboFolderDateOp.TabIndex = 8;
            // 
            // lblFolderDateValue
            // 
            this.lblFolderDateValue.AutoSize = true;
            this.lblFolderDateValue.Location = new System.Drawing.Point(210, 80);
            this.lblFolderDateValue.Name = "lblFolderDateValue";
            this.lblFolderDateValue.Size = new System.Drawing.Size(67, 19);
            this.lblFolderDateValue.TabIndex = 9;
            this.lblFolderDateValue.Text = "比較日期:";
            // 
            // dtpFolderDateValue
            // 
            this.dtpFolderDateValue.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFolderDateValue.Location = new System.Drawing.Point(280, 78);
            this.dtpFolderDateValue.Name = "dtpFolderDateValue";
            this.dtpFolderDateValue.Size = new System.Drawing.Size(150, 23);
            this.dtpFolderDateValue.TabIndex = 10;
            // 
            // gbFileFilter
            // 
            this.gbFileFilter.Controls.Add(this.lblFileInclude);
            this.gbFileFilter.Controls.Add(this.txtFileInclude);
            this.gbFileFilter.Controls.Add(this.lblFileExclude);
            this.gbFileFilter.Controls.Add(this.txtFileExclude);
            this.gbFileFilter.Controls.Add(this.chkFileDate);
            this.gbFileFilter.Controls.Add(this.lblFileDateFormat);
            this.gbFileFilter.Controls.Add(this.txtFileDateFormat);
            this.gbFileFilter.Controls.Add(this.lblFileDateOp);
            this.gbFileFilter.Controls.Add(this.cboFileDateOp);
            this.gbFileFilter.Controls.Add(this.lblFileDateValue);
            this.gbFileFilter.Controls.Add(this.dtpFileDateValue);
            this.gbFileFilter.Location = new System.Drawing.Point(10, 300);
            this.gbFileFilter.Name = "gbFileFilter";
            this.gbFileFilter.Size = new System.Drawing.Size(580, 140);
            this.gbFileFilter.TabIndex = 5;
            this.gbFileFilter.TabStop = false;
            this.gbFileFilter.Text = "檔案篩選";
            // 
            // lblFileInclude
            // 
            this.lblFileInclude.AutoSize = true;
            this.lblFileInclude.Location = new System.Drawing.Point(10, 20);
            this.lblFileInclude.Name = "lblFileInclude";
            this.lblFileInclude.Size = new System.Drawing.Size(42, 19);
            this.lblFileInclude.TabIndex = 0;
            this.lblFileInclude.Text = "包含:";
            // 
            // txtFileInclude
            // 
            this.txtFileInclude.Location = new System.Drawing.Point(60, 18);
            this.txtFileInclude.Name = "txtFileInclude";
            this.txtFileInclude.Size = new System.Drawing.Size(200, 23);
            this.txtFileInclude.TabIndex = 1;
            // 
            // lblFileExclude
            // 
            this.lblFileExclude.AutoSize = true;
            this.lblFileExclude.Location = new System.Drawing.Point(280, 20);
            this.lblFileExclude.Name = "lblFileExclude";
            this.lblFileExclude.Size = new System.Drawing.Size(42, 19);
            this.lblFileExclude.TabIndex = 2;
            this.lblFileExclude.Text = "排除:";
            // 
            // txtFileExclude
            // 
            this.txtFileExclude.Location = new System.Drawing.Point(330, 18);
            this.txtFileExclude.Name = "txtFileExclude";
            this.txtFileExclude.Size = new System.Drawing.Size(200, 23);
            this.txtFileExclude.TabIndex = 3;
            // 
            // chkFileDate
            // 
            this.chkFileDate.AutoSize = true;
            this.chkFileDate.Location = new System.Drawing.Point(10, 50);
            this.chkFileDate.Name = "chkFileDate";
            this.chkFileDate.Size = new System.Drawing.Size(86, 23);
            this.chkFileDate.TabIndex = 4;
            this.chkFileDate.Text = "日期篩選";
            this.chkFileDate.UseVisualStyleBackColor = true;
            // 
            // lblFileDateFormat
            // 
            this.lblFileDateFormat.AutoSize = true;
            this.lblFileDateFormat.Location = new System.Drawing.Point(100, 52);
            this.lblFileDateFormat.Name = "lblFileDateFormat";
            this.lblFileDateFormat.Size = new System.Drawing.Size(67, 19);
            this.lblFileDateFormat.TabIndex = 5;
            this.lblFileDateFormat.Text = "日期格式:";
            // 
            // txtFileDateFormat
            // 
            this.txtFileDateFormat.Location = new System.Drawing.Point(170, 50);
            this.txtFileDateFormat.Name = "txtFileDateFormat";
            this.txtFileDateFormat.Size = new System.Drawing.Size(100, 23);
            this.txtFileDateFormat.TabIndex = 6;
            this.txtFileDateFormat.Text = "yyyy-MM-dd";
            // 
            // lblFileDateOp
            // 
            this.lblFileDateOp.AutoSize = true;
            this.lblFileDateOp.Location = new System.Drawing.Point(10, 80);
            this.lblFileDateOp.Name = "lblFileDateOp";
            this.lblFileDateOp.Size = new System.Drawing.Size(67, 19);
            this.lblFileDateOp.TabIndex = 7;
            this.lblFileDateOp.Text = "比較運算:";
            // 
            // cboFileDateOp
            // 
            this.cboFileDateOp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboFileDateOp.FormattingEnabled = true;
            this.cboFileDateOp.Items.AddRange(new object[] {
            ">",
            ">=",
            "<",
            "<=",
            "="});
            this.cboFileDateOp.Location = new System.Drawing.Point(100, 78);
            this.cboFileDateOp.Name = "cboFileDateOp";
            this.cboFileDateOp.Size = new System.Drawing.Size(100, 27);
            this.cboFileDateOp.TabIndex = 8;
            // 
            // lblFileDateValue
            // 
            this.lblFileDateValue.AutoSize = true;
            this.lblFileDateValue.Location = new System.Drawing.Point(210, 80);
            this.lblFileDateValue.Name = "lblFileDateValue";
            this.lblFileDateValue.Size = new System.Drawing.Size(67, 19);
            this.lblFileDateValue.TabIndex = 9;
            this.lblFileDateValue.Text = "比較日期:";
            // 
            // dtpFileDateValue
            // 
            this.dtpFileDateValue.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFileDateValue.Location = new System.Drawing.Point(280, 78);
            this.dtpFileDateValue.Name = "dtpFileDateValue";
            this.dtpFileDateValue.Size = new System.Drawing.Size(150, 23);
            this.dtpFileDateValue.TabIndex = 10;
            // 
            // gbColumns
            // 
            this.gbColumns.Controls.Add(this.lblAvailColumns);
            this.gbColumns.Controls.Add(this.lstAvailColumns);
            this.gbColumns.Controls.Add(this.btnGetColumns);
            this.gbColumns.Controls.Add(this.lblSelectedColumns);
            this.gbColumns.Controls.Add(this.lstSelectedColumns);
            this.gbColumns.Controls.Add(this.btnAddColumn);
            this.gbColumns.Controls.Add(this.btnRemoveColumn);
            this.gbColumns.Controls.Add(this.btnConfigColumn);
            this.gbColumns.Controls.Add(this.chkDebugLog);
            this.gbColumns.Location = new System.Drawing.Point(10, 450);
            this.gbColumns.Name = "gbColumns";
            this.gbColumns.Size = new System.Drawing.Size(580, 190);
            this.gbColumns.TabIndex = 6;
            this.gbColumns.TabStop = false;
            this.gbColumns.Text = "欄位配置";
            // 
            // lblAvailColumns
            // 
            this.lblAvailColumns.AutoSize = true;
            this.lblAvailColumns.Location = new System.Drawing.Point(10, 20);
            this.lblAvailColumns.Name = "lblAvailColumns";
            this.lblAvailColumns.Size = new System.Drawing.Size(67, 19);
            this.lblAvailColumns.TabIndex = 0;
            this.lblAvailColumns.Text = "所有欄位:";
            // 
            // lstAvailColumns
            // 
            this.lstAvailColumns.FormattingEnabled = true;
            this.lstAvailColumns.ItemHeight = 19;
            this.lstAvailColumns.Location = new System.Drawing.Point(10, 40);
            this.lstAvailColumns.Name = "lstAvailColumns";
            this.lstAvailColumns.Size = new System.Drawing.Size(200, 100);
            this.lstAvailColumns.TabIndex = 1;
            // 
            // btnGetColumns
            // 
            this.btnGetColumns.Location = new System.Drawing.Point(10, 145);
            this.btnGetColumns.Name = "btnGetColumns";
            this.btnGetColumns.Size = new System.Drawing.Size(100, 30);
            this.btnGetColumns.TabIndex = 2;
            this.btnGetColumns.Text = "獲取欄位";
            this.btnGetColumns.UseVisualStyleBackColor = true;
            this.btnGetColumns.Click += new System.EventHandler(this.btnGetColumns_Click);
            // 
            // lblSelectedColumns
            // 
            this.lblSelectedColumns.AutoSize = true;
            this.lblSelectedColumns.Location = new System.Drawing.Point(220, 20);
            this.lblSelectedColumns.Name = "lblSelectedColumns";
            this.lblSelectedColumns.Size = new System.Drawing.Size(82, 19);
            this.lblSelectedColumns.TabIndex = 3;
            this.lblSelectedColumns.Text = "選擇的欄位:";
            // 
            // lstSelectedColumns
            // 
            this.lstSelectedColumns.FormattingEnabled = true;
            this.lstSelectedColumns.ItemHeight = 19;
            this.lstSelectedColumns.Location = new System.Drawing.Point(220, 40);
            this.lstSelectedColumns.Name = "lstSelectedColumns";
            this.lstSelectedColumns.Size = new System.Drawing.Size(200, 100);
            this.lstSelectedColumns.TabIndex = 4;
            // 
            // btnAddColumn
            // 
            this.btnAddColumn.Location = new System.Drawing.Point(430, 50);
            this.btnAddColumn.Name = "btnAddColumn";
            this.btnAddColumn.Size = new System.Drawing.Size(40, 30);
            this.btnAddColumn.TabIndex = 5;
            this.btnAddColumn.Text = ">";
            this.btnAddColumn.UseVisualStyleBackColor = true;
            this.btnAddColumn.Click += new System.EventHandler(this.btnAddColumn_Click);
            // 
            // btnRemoveColumn
            // 
            this.btnRemoveColumn.Location = new System.Drawing.Point(430, 90);
            this.btnRemoveColumn.Name = "btnRemoveColumn";
            this.btnRemoveColumn.Size = new System.Drawing.Size(40, 30);
            this.btnRemoveColumn.TabIndex = 6;
            this.btnRemoveColumn.Text = "<";
            this.btnRemoveColumn.UseVisualStyleBackColor = true;
            this.btnRemoveColumn.Click += new System.EventHandler(this.btnRemoveColumn_Click);
            // 
            // btnConfigColumn
            // 
            this.btnConfigColumn.Location = new System.Drawing.Point(480, 50);
            this.btnConfigColumn.Name = "btnConfigColumn";
            this.btnConfigColumn.Size = new System.Drawing.Size(90, 30);
            this.btnConfigColumn.TabIndex = 7;
            this.btnConfigColumn.Text = "欄位設置";
            this.btnConfigColumn.UseVisualStyleBackColor = true;
            this.btnConfigColumn.Click += new System.EventHandler(this.btnConfigColumn_Click);
            // 
            // chkDebugLog
            // 
            this.chkDebugLog.AutoSize = true;
            this.chkDebugLog.Location = new System.Drawing.Point(10, 170);
            this.chkDebugLog.Name = "chkDebugLog";
            this.chkDebugLog.Size = new System.Drawing.Size(120, 23);
            this.chkDebugLog.TabIndex = 8;
            this.chkDebugLog.Text = "啟用Debug Log";
            this.chkDebugLog.UseVisualStyleBackColor = true;
            // 
            // gbFilter
            // 
            this.gbFilter.Controls.Add(this.lstFilters);
            this.gbFilter.Controls.Add(this.btnAddFilter);
            this.gbFilter.Controls.Add(this.btnRemoveFilter);
            this.gbFilter.Location = new System.Drawing.Point(600, 40);
            this.gbFilter.Name = "gbFilter";
            this.gbFilter.Size = new System.Drawing.Size(380, 220);
            this.gbFilter.TabIndex = 7;
            this.gbFilter.TabStop = false;
            this.gbFilter.Text = "篩選條件";
            // 
            // lstFilters
            // 
            this.lstFilters.FormattingEnabled = true;
            this.lstFilters.ItemHeight = 19;
            this.lstFilters.Location = new System.Drawing.Point(10, 20);
            this.lstFilters.Name = "lstFilters";
            this.lstFilters.Size = new System.Drawing.Size(360, 160);
            this.lstFilters.TabIndex = 0;
            // 
            // btnAddFilter
            // 
            this.btnAddFilter.Location = new System.Drawing.Point(10, 185);
            this.btnAddFilter.Name = "btnAddFilter";
            this.btnAddFilter.Size = new System.Drawing.Size(100, 30);
            this.btnAddFilter.TabIndex = 1;
            this.btnAddFilter.Text = "新增條件";
            this.btnAddFilter.UseVisualStyleBackColor = true;
            this.btnAddFilter.Click += new System.EventHandler(this.btnAddFilter_Click);
            // 
            // btnRemoveFilter
            // 
            this.btnRemoveFilter.Location = new System.Drawing.Point(120, 185);
            this.btnRemoveFilter.Name = "btnRemoveFilter";
            this.btnRemoveFilter.Size = new System.Drawing.Size(100, 30);
            this.btnRemoveFilter.TabIndex = 2;
            this.btnRemoveFilter.Text = "移除條件";
            this.btnRemoveFilter.UseVisualStyleBackColor = true;
            this.btnRemoveFilter.Click += new System.EventHandler(this.btnRemoveFilter_Click);
            // 
            // gbOptions
            // 
            this.gbOptions.Controls.Add(this.chkAddFileName);
            this.gbOptions.Controls.Add(this.chkAddDirectoryName);
            this.gbOptions.Controls.Add(this.lblThreads);
            this.gbOptions.Controls.Add(this.nudThreads);
            this.gbOptions.Controls.Add(this.chkSkipIncompleteFiles);
            this.gbOptions.Location = new System.Drawing.Point(600, 270);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(380, 110);
            this.gbOptions.TabIndex = 8;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "其他選項";
            // 
            // chkAddFileName
            // 
            this.chkAddFileName.AutoSize = true;
            this.chkAddFileName.Location = new System.Drawing.Point(10, 20);
            this.chkAddFileName.Name = "chkAddFileName";
            this.chkAddFileName.Size = new System.Drawing.Size(176, 23);
            this.chkAddFileName.TabIndex = 0;
            this.chkAddFileName.Text = "在最後一欄新增檔案名稱";
            this.chkAddFileName.UseVisualStyleBackColor = true;
            // 
            // chkAddDirectoryName
            // 
            this.chkAddDirectoryName.AutoSize = true;
            this.chkAddDirectoryName.Location = new System.Drawing.Point(10, 50);
            this.chkAddDirectoryName.Name = "chkAddDirectoryName";
            this.chkAddDirectoryName.Size = new System.Drawing.Size(180, 23);
            this.chkAddDirectoryName.TabIndex = 1;
            this.chkAddDirectoryName.Text = "在最後一欄新增目錄名稱";
            this.chkAddDirectoryName.UseVisualStyleBackColor = true;
            // 
            // lblThreads
            // 
            this.lblThreads.AutoSize = true;
            this.lblThreads.Location = new System.Drawing.Point(200, 22);
            this.lblThreads.Name = "lblThreads";
            this.lblThreads.Size = new System.Drawing.Size(97, 19);
            this.lblThreads.TabIndex = 2;
            this.lblThreads.Text = "處理執行緒數:";
            // 
            // nudThreads
            // 
            this.nudThreads.Location = new System.Drawing.Point(300, 20);
            this.nudThreads.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.nudThreads.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudThreads.Name = "nudThreads";
            this.nudThreads.Size = new System.Drawing.Size(60, 23);
            this.nudThreads.TabIndex = 3;
            this.nudThreads.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // chkSkipIncompleteFiles
            // 
            this.chkSkipIncompleteFiles.AutoSize = true;
            this.chkSkipIncompleteFiles.Location = new System.Drawing.Point(10, 50);
            this.chkSkipIncompleteFiles.Name = "chkSkipIncompleteFiles";
            this.chkSkipIncompleteFiles.Size = new System.Drawing.Size(180, 23);
            this.chkSkipIncompleteFiles.TabIndex = 4;
            this.chkSkipIncompleteFiles.Text = "跳過不完整的檔案";
            this.chkSkipIncompleteFiles.Checked = true;
            this.chkSkipIncompleteFiles.UseVisualStyleBackColor = true;
            // 
            // gbTarget
            // 
            this.gbTarget.Controls.Add(this.lblTargetPath);
            this.gbTarget.Controls.Add(this.txtTargetPath);
            this.gbTarget.Controls.Add(this.btnBrowseTarget);
            this.gbTarget.Controls.Add(this.lblOutputFileName);
            this.gbTarget.Controls.Add(this.txtOutputFileName);
            this.gbTarget.Location = new System.Drawing.Point(600, 390);
            this.gbTarget.Name = "gbTarget";
            this.gbTarget.Size = new System.Drawing.Size(380, 160);
            this.gbTarget.TabIndex = 9;
            this.gbTarget.TabStop = false;
            this.gbTarget.Text = "目標設置";
            // 
            // lblTargetPath
            // 
            this.lblTargetPath.AutoSize = true;
            this.lblTargetPath.Location = new System.Drawing.Point(10, 20);
            this.lblTargetPath.Name = "lblTargetPath";
            this.lblTargetPath.Size = new System.Drawing.Size(67, 19);
            this.lblTargetPath.TabIndex = 0;
            this.lblTargetPath.Text = "目標資料夾:";
            // 
            // txtTargetPath
            // 
            this.txtTargetPath.Location = new System.Drawing.Point(10, 45);
            this.txtTargetPath.Name = "txtTargetPath";
            this.txtTargetPath.Size = new System.Drawing.Size(280, 23);
            this.txtTargetPath.TabIndex = 1;
            // 
            // btnBrowseTarget
            // 
            this.btnBrowseTarget.Location = new System.Drawing.Point(300, 45);
            this.btnBrowseTarget.Name = "btnBrowseTarget";
            this.btnBrowseTarget.Size = new System.Drawing.Size(70, 23);
            this.btnBrowseTarget.TabIndex = 2;
            this.btnBrowseTarget.Text = "瀏覽...";
            this.btnBrowseTarget.UseVisualStyleBackColor = true;
            this.btnBrowseTarget.Click += new System.EventHandler(this.btnBrowseTarget_Click);
            // 
            // lblOutputFileName
            // 
            this.lblOutputFileName.AutoSize = true;
            this.lblOutputFileName.Location = new System.Drawing.Point(10, 80);
            this.lblOutputFileName.Name = "lblOutputFileName";
            this.lblOutputFileName.Size = new System.Drawing.Size(102, 19);
            this.lblOutputFileName.TabIndex = 3;
            this.lblOutputFileName.Text = "自訂檔案名稱:";
            // 
            // txtOutputFileName
            // 
            this.txtOutputFileName.Location = new System.Drawing.Point(110, 78);
            this.txtOutputFileName.Name = "txtOutputFileName";
            this.txtOutputFileName.Size = new System.Drawing.Size(200, 23);
            this.txtOutputFileName.TabIndex = 4;
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(10, 650);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(970, 23);
            this.progressBar.TabIndex = 10;
            // 
            // lstLog
            // 
            this.lstLog.FormattingEnabled = true;
            this.lstLog.ItemHeight = 19;
            this.lstLog.Location = new System.Drawing.Point(10, 680);
            this.lstLog.Name = "lstLog";
            this.lstLog.Size = new System.Drawing.Size(970, 130);
            this.lstLog.TabIndex = 11;
            this.lstLog.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstLog.HorizontalScrollbar = true;
            this.lstLog.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstLog_KeyDown);
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(810, 560);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(80, 30);
            this.btnExecute.TabIndex = 12;
            this.btnExecute.Text = "執行";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Enabled = false;
            this.btnCancel.Location = new System.Drawing.Point(900, 560);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 30);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // CSV_Data_Filter
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 820);
            this.Controls.Add(this.lblSourcePaths);
            this.Controls.Add(this.lstSourcePaths);
            this.Controls.Add(this.btnAddPath);
            this.Controls.Add(this.btnRemovePath);
            this.Controls.Add(this.gbFolderFilter);
            this.Controls.Add(this.gbFileFilter);
            this.Controls.Add(this.gbColumns);
            this.Controls.Add(this.gbFilter);
            this.Controls.Add(this.gbOptions);
            this.Controls.Add(this.gbTarget);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.lstLog);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.btnCancel);
            this.Name = "CSV_Data_Filter";
            this.Text = "CSV 資料整理工具";
            this.gbFolderFilter.ResumeLayout(false);
            this.gbFolderFilter.PerformLayout();
            this.gbFileFilter.ResumeLayout(false);
            this.gbFileFilter.PerformLayout();
            this.gbColumns.ResumeLayout(false);
            this.gbColumns.PerformLayout();
            this.gbFilter.ResumeLayout(false);
            this.gbOptions.ResumeLayout(false);
            this.gbOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudThreads)).EndInit();
            this.gbTarget.ResumeLayout(false);
            this.gbTarget.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

            // 設置初始值
            cboFolderDateOp.SelectedIndex = 0;
            cboFileDateOp.SelectedIndex = 0;
            nudThreads.Value = Environment.ProcessorCount / 2;
            nudThreads.Maximum = Environment.ProcessorCount;
            txtFolderDateFormat.Text = _dateFormat;
            txtFileDateFormat.Text = _dateFormat;
            txtOutputFileName.Text = "Merged_CSV";
            
            // 初始日誌
            AddLog(lstLog, "應用程式已啟動，請設定參數");
        }

        #endregion
    }
}
