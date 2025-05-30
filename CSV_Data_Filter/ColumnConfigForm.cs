using CSV_Data_Filter.Models;
using System;
using System.Windows.Forms;

namespace CSV_Data_Filter
{
    public partial class ColumnConfigForm : Form
    {
        private readonly Models.ColumnConfig _config;

        public ColumnConfigForm(ColumnConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            InitializeComponent();
            this.Text = $"配置欄位: {_config.Name}";
            LoadConfig();
        }

        private void LoadConfig()
        {
            txtCustomName.Text = _config.CustomName;

            switch (_config.ProcessType)
            {
                case ProcessType.None:
                    cboProcessType.SelectedIndex = 0;
                    break;

                case ProcessType.Substring:
                    cboProcessType.SelectedIndex = 1;
                    nudStartIndex.Value = _config.StartIndex;
                    nudLength.Value = _config.SubstringLength;
                    break;

                case ProcessType.Math:
                    cboProcessType.SelectedIndex = 2;
                    nudMultiply.Value = (decimal)_config.MultiplyBy;
                    nudAdd.Value = (decimal)_config.AddValue;
                    break;

                case ProcessType.Replace:
                    cboProcessType.SelectedIndex = 3;
                    txtFind.Text = _config.FindText ?? string.Empty;
                    txtReplace.Text = _config.ReplaceText ?? string.Empty;
                    break;

                case ProcessType.Regex:
                    cboProcessType.SelectedIndex = 4;
                    txtRegex.Text = _config.RegexPattern ?? string.Empty;
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
                    _config.StartIndex = (int)nudStartIndex.Value;
                    _config.SubstringLength = (int)nudLength.Value;
                    break;

                case 2:
                    _config.ProcessType = ProcessType.Math;
                    _config.MultiplyBy = nudMultiply.Value;
                    _config.AddValue = nudAdd.Value;
                    break;

                case 3:
                    _config.ProcessType = ProcessType.Replace;
                    _config.FindText = txtFind.Text;
                    _config.ReplaceText = txtReplace.Text;
                    break;

                case 4:
                    _config.ProcessType = ProcessType.Regex;
                    _config.RegexPattern = txtRegex.Text;
                    break;
            }
        }
    }
}