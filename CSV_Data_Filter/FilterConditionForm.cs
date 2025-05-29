using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CSV_Data_Filter.Models;

namespace CSV_Data_Filter
{
    public class FilterConditionForm : Form
    {
        private readonly List<string> _columns;
        public Models.FilterCondition Condition { get; private set; }
        
        private ComboBox cboColumns;
        private ComboBox cboOperator;
        private TextBox txtValue;
        private Panel pnlDate;
        private TextBox txtDateFormat;
        
        public FilterConditionForm(List<string> columns)
        {
            _columns = columns ?? throw new ArgumentNullException(nameof(columns));
            Condition = new FilterCondition
            {
                ColumnName = columns.Count > 0 ? columns[0] : string.Empty,
                Operator = FilterOperator.Equals,
                Value = string.Empty,
                DateFormat = "yyyy-MM-dd"
            };
            
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.Text = "新增篩選條件";
            this.Size = new System.Drawing.Size(450, 250);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            
            var lblColumn = new Label
            {
                Text = "欄位:",
                Location = new System.Drawing.Point(20, 20),
                AutoSize = true
            };
            
            cboColumns = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(100, 18),
                Size = new System.Drawing.Size(200, 23)
            };
            cboColumns.Items.AddRange(_columns.ToArray());
            if (_columns.Count > 0)
            {
                cboColumns.SelectedIndex = 0;
            }
            
            var lblOperator = new Label
            {
                Text = "運算子:",
                Location = new System.Drawing.Point(20, 50),
                AutoSize = true
            };
            
            cboOperator = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(100, 48),
                Size = new System.Drawing.Size(200, 23)
            };            cboOperator.Items.AddRange(new object[] { 
                "等於",
                "不等於",
                "包含",
                "不包含",
                "開頭為",
                "結尾為",
                "大於",
                "小於",
                "大於或等於",
                "小於或等於",
                "日期大於",
                "日期小於",
                "日期大於或等於",
                "日期小於或等於"
            });
            cboOperator.SelectedIndex = 0;
            cboOperator.SelectedIndexChanged += CboOperator_SelectedIndexChanged;
            
            var lblValue = new Label
            {
                Text = "值:",
                Location = new System.Drawing.Point(20, 80),
                AutoSize = true
            };
            
            txtValue = new TextBox
            {
                Location = new System.Drawing.Point(100, 78),
                Size = new System.Drawing.Size(320, 23)
            };
            
            pnlDate = new Panel
            {
                Location = new System.Drawing.Point(20, 110),
                Size = new System.Drawing.Size(400, 40),
                Visible = false
            };
            
            var lblDateFormat = new Label
            {
                Text = "日期格式:",
                Location = new System.Drawing.Point(0, 5),
                AutoSize = true
            };
            
            txtDateFormat = new TextBox
            {
                Location = new System.Drawing.Point(80, 3),
                Size = new System.Drawing.Size(150, 23),
                Text = "yyyy-MM-dd"
            };
            
            pnlDate.Controls.AddRange(new Control[] { lblDateFormat, txtDateFormat });
            
            // 按鈕
            var btnOK = new Button
            {
                Text = "確定",
                DialogResult = DialogResult.OK,
                Location = new System.Drawing.Point(260, 170),
                Size = new System.Drawing.Size(80, 30)
            };
            btnOK.Click += BtnOK_Click;
            
            var btnCancel = new Button
            {
                Text = "取消",
                DialogResult = DialogResult.Cancel,
                Location = new System.Drawing.Point(350, 170),
                Size = new System.Drawing.Size(80, 30)
            };
            
            this.Controls.AddRange(new Control[] {
                lblColumn,
                cboColumns,
                lblOperator,
                cboOperator,
                lblValue,
                txtValue,
                pnlDate,
                btnOK,
                btnCancel
            });
            
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
        }        private void CboOperator_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // 只有日期運算子才顯示日期格式面板
            int idx = cboOperator.SelectedIndex;
            pnlDate.Visible = idx == 10 || idx == 11 || idx == 12 || idx == 13; // 日期相關的運算符
        }        private void BtnOK_Click(object? sender, EventArgs e)
        {
            if (cboColumns.SelectedIndex == -1)
            {
                MessageBox.Show("請選擇欄位", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            
            if (string.IsNullOrWhiteSpace(txtValue.Text))
            {
                MessageBox.Show("請輸入篩選值", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.None;
                return;
            }
            
            // 設定篩選條件
            if (cboColumns.SelectedItem != null)
            {
                // 以最後一個 " (" 為分隔，並保留括號內容
                string display = cboColumns.SelectedItem.ToString() ?? string.Empty;
                string columnName = display;
                int idx = display.LastIndexOf(" (");
                if (idx >= 0 && display.EndsWith(")"))
                {
                    columnName = display.Substring(idx + 2, display.Length - idx - 3);
                }
                Condition.ColumnName = columnName;
            }
            
            switch (cboOperator.SelectedIndex)
            {
                case 0: Condition.Operator = FilterOperator.Equals; break;
                case 1: Condition.Operator = FilterOperator.NotEquals; break;
                case 2: Condition.Operator = FilterOperator.Contains; break;
                case 3: Condition.Operator = FilterOperator.NotContains; break;
                case 4: Condition.Operator = FilterOperator.StartsWith; break;
                case 5: Condition.Operator = FilterOperator.EndsWith; break;
                case 6: Condition.Operator = FilterOperator.GreaterThan; break;
                case 7: Condition.Operator = FilterOperator.LessThan; break;
                case 8: Condition.Operator = FilterOperator.GreaterThanOrEqual; break;
                case 9: Condition.Operator = FilterOperator.LessThanOrEqual; break;
                case 10: Condition.Operator = FilterOperator.DateGreaterThan; break;
                case 11: Condition.Operator = FilterOperator.DateLessThan; break;
                case 12: Condition.Operator = FilterOperator.DateGreaterThanOrEqual; break;
                case 13: Condition.Operator = FilterOperator.DateLessThanOrEqual; break;
            }
            
            Condition.Value = txtValue.Text;
            
            if (pnlDate.Visible)
            {
                Condition.DateFormat = txtDateFormat.Text;
            }
        }
    }
}
