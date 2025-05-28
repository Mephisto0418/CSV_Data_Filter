using System;
using System.Windows.Forms;
using CSV_Data_Filter.Models;

namespace CSV_Data_Filter
{
    public class ColumnConfigForm : Form
    {
        private readonly Models.ColumnConfig _config;
        private TextBox txtCustomName;
        private ComboBox cboProcessType;
        private Panel pnlSubstring;
        private Panel pnlMath;
        private Panel pnlReplace;
        private Panel pnlRegex;
        
        public ColumnConfigForm(ColumnConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            InitializeComponent();
            LoadConfig();
        }
        
        private void InitializeComponent()
        {
            this.Text = $"配置欄位: {_config.Name}";
            this.Size = new System.Drawing.Size(450, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            
            // 自訂欄位名稱
            var lblCustomName = new Label
            {
                Text = "自訂欄位名稱:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            
            txtCustomName = new TextBox
            {
                Location = new System.Drawing.Point(120, 18),
                Size = new System.Drawing.Size(200, 23),
                Text = _config.CustomName
            };
            
            var lblProcessType = new Label
            {
                Text = "處理方式:",
                Location = new System.Drawing.Point(20, 55),
                AutoSize = true
            };
            
            cboProcessType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(120, 53),
                Size = new System.Drawing.Size(200, 23)
            };
            cboProcessType.Items.AddRange(new object[] { 
                "不處理", 
                "取子字串",
                "數值運算",
                "字串替換",
                "正規表達式"
            });
            cboProcessType.SelectedIndexChanged += CboProcessType_SelectedIndexChanged;
            
            // 取子字串面板
            pnlSubstring = new Panel
            {
                Location = new System.Drawing.Point(20, 95),
                Size = new System.Drawing.Size(400, 150),
                Visible = false
            };
            
            var lblStartIndex = new Label
            {
                Text = "起始位置:",
                Location = new System.Drawing.Point(0, 10),
                AutoSize = true
            };
            
            var nudStartIndex = new NumericUpDown
            {
                Name = "nudStartIndex",
                Location = new System.Drawing.Point(100, 8),
                Size = new System.Drawing.Size(80, 23),
                Minimum = 0,
                Maximum = 1000,
                Value = _config.StartIndex
            };
            
            var lblLength = new Label
            {
                Text = "長度:",
                Location = new System.Drawing.Point(0, 40),
                AutoSize = true
            };
            
            var nudLength = new NumericUpDown
            {
                Name = "nudLength",
                Location = new System.Drawing.Point(100, 38),
                Size = new System.Drawing.Size(80, 23),
                Minimum = 0,
                Maximum = 1000,
                Value = _config.SubstringLength
            };
            
            pnlSubstring.Controls.AddRange(new Control[] { lblStartIndex, nudStartIndex, lblLength, nudLength });
            
            // 數值運算面板
            pnlMath = new Panel
            {
                Location = new System.Drawing.Point(20, 95),
                Size = new System.Drawing.Size(400, 150),
                Visible = false
            };
            
            var lblMultiply = new Label
            {
                Text = "乘以:",
                Location = new System.Drawing.Point(0, 10),
                AutoSize = true
            };
            
            var nudMultiply = new NumericUpDown
            {
                Name = "nudMultiply",
                Location = new System.Drawing.Point(100, 8),
                Size = new System.Drawing.Size(120, 23),
                DecimalPlaces = 3,
                Minimum = -1000,
                Maximum = 1000,
                Value = (decimal)_config.MultiplyBy,
                Increment = 0.1m
            };
            
            var lblAdd = new Label
            {
                Text = "加上:",
                Location = new System.Drawing.Point(0, 40),
                AutoSize = true
            };
            
            var nudAdd = new NumericUpDown
            {
                Name = "nudAdd",
                Location = new System.Drawing.Point(100, 38),
                Size = new System.Drawing.Size(120, 23),
                DecimalPlaces = 3,
                Minimum = -1000,
                Maximum = 1000,
                Value = (decimal)_config.AddValue,
                Increment = 0.1m
            };
            
            pnlMath.Controls.AddRange(new Control[] { lblMultiply, nudMultiply, lblAdd, nudAdd });
            
            // 字串替換面板
            pnlReplace = new Panel
            {
                Location = new System.Drawing.Point(20, 95),
                Size = new System.Drawing.Size(400, 150),
                Visible = false
            };
            
            var lblFind = new Label
            {
                Text = "尋找:",
                Location = new System.Drawing.Point(0, 10),
                AutoSize = true
            };
            
            var txtFind = new TextBox
            {
                Name = "txtFind",
                Location = new System.Drawing.Point(100, 8),
                Size = new System.Drawing.Size(280, 23),
                Text = _config.FindText ?? string.Empty
            };
            
            var lblReplace = new Label
            {
                Text = "替換為:",
                Location = new System.Drawing.Point(0, 40),
                AutoSize = true
            };
            
            var txtReplace = new TextBox
            {
                Name = "txtReplace",
                Location = new System.Drawing.Point(100, 38),
                Size = new System.Drawing.Size(280, 23),
                Text = _config.ReplaceText ?? string.Empty
            };
            
            pnlReplace.Controls.AddRange(new Control[] { lblFind, txtFind, lblReplace, txtReplace });
            
            // 正規表達式面板
            pnlRegex = new Panel
            {
                Location = new System.Drawing.Point(20, 95),
                Size = new System.Drawing.Size(400, 150),
                Visible = false
            };
            
            var lblRegex = new Label
            {
                Text = "正規表達式:",
                Location = new System.Drawing.Point(0, 10),
                AutoSize = true
            };
            
            var txtRegex = new TextBox
            {
                Name = "txtRegex",
                Location = new System.Drawing.Point(0, 40),
                Size = new System.Drawing.Size(380, 23),
                Text = _config.RegexPattern ?? string.Empty
            };
            
            pnlRegex.Controls.AddRange(new Control[] { lblRegex, txtRegex });
            
            // 按鈕
            var btnOK = new Button
            {
                Text = "確定",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(260, 270),
                Size = new System.Drawing.Size(80, 30)
            };
            btnOK.Click += BtnOK_Click;
            
            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(350, 270),
                Size = new System.Drawing.Size(80, 30)
            };
            
            this.Controls.AddRange(new Control[] {
                lblCustomName,
                txtCustomName,
                lblProcessType,
                cboProcessType,
                pnlSubstring,
                pnlMath,
                pnlReplace,
                pnlRegex,
                btnOK,
                btnCancel
            });
            
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }

        private void LoadConfig()
        {
            switch (_config.ProcessType)
            {
                case ProcessType.None:
                    cboProcessType.SelectedIndex = 0;
                    break;
                case ProcessType.Substring:
                    cboProcessType.SelectedIndex = 1;
                    break;
                case ProcessType.Math:
                    cboProcessType.SelectedIndex = 2;
                    break;
                case ProcessType.Replace:
                    cboProcessType.SelectedIndex = 3;
                    break;
                case ProcessType.Regex:
                    cboProcessType.SelectedIndex = 4;
                    break;
            }
        }

        private void CboProcessType_SelectedIndexChanged(object sender, EventArgs e)
        {
            HideAllPanels();
            
            switch (cboProcessType.SelectedIndex)
            {
                case 1: // 取子字串
                    pnlSubstring.Visible = true;
                    break;
                case 2: // 數值運算
                    pnlMath.Visible = true;
                    break;
                case 3: // 字串替換
                    pnlReplace.Visible = true;
                    break;
                case 4: // 正規表達式
                    pnlRegex.Visible = true;
                    break;
            }
        }

        private void HideAllPanels()
        {
            pnlSubstring.Visible = false;
            pnlMath.Visible = false;
            pnlReplace.Visible = false;
            pnlRegex.Visible = false;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            // 儲存自訂欄位名稱
            _config.CustomName = txtCustomName.Text.Trim();
            if (string.IsNullOrEmpty(_config.CustomName))
            {
                _config.CustomName = _config.Name; // 如果為空，使用原始名稱
            }
            
            switch (cboProcessType.SelectedIndex)
            {
                case 0:
                    _config.ProcessType = ProcessType.None;
                    break;
                case 1:
                    _config.ProcessType = ProcessType.Substring;
                    _config.StartIndex = (int)((NumericUpDown)pnlSubstring.Controls["nudStartIndex"]).Value;
                    _config.SubstringLength = (int)((NumericUpDown)pnlSubstring.Controls["nudLength"]).Value;
                    break;
                case 2:
                    _config.ProcessType = ProcessType.Math;
                    _config.MultiplyBy = (decimal)((NumericUpDown)pnlMath.Controls["nudMultiply"]).Value;
                    _config.AddValue = (decimal)((NumericUpDown)pnlMath.Controls["nudAdd"]).Value;
                    break;
                case 3:
                    _config.ProcessType = ProcessType.Replace;
                    _config.FindText = ((TextBox)pnlReplace.Controls["txtFind"]).Text;
                    _config.ReplaceText = ((TextBox)pnlReplace.Controls["txtReplace"]).Text;
                    break;
                case 4:
                    _config.ProcessType = ProcessType.Regex;
                    _config.RegexPattern = ((TextBox)pnlRegex.Controls["txtRegex"]).Text;
                    break;
            }
        }
    }
}
