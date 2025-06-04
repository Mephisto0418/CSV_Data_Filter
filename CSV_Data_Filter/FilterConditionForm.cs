using CSV_Data_Filter.Models;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CSV_Data_Filter
{
    public partial class FilterConditionForm : Form
    {
        private readonly List<string> _columns;
        public Models.FilterCondition Condition { get; private set; }

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
            
            // 初始化欄位選擇器
            cboColumns.Items.AddRange(_columns.ToArray());
            if (_columns.Count > 0)
            {
                cboColumns.SelectedIndex = 0;
            }
            
            // 初始化運算子選擇
            cboOperator.SelectedIndex = 0;
        }

        private void CboOperator_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 只有日期運算子才顯示日期格式面板
            int idx = cboOperator.SelectedIndex;
            pnlDate.Visible = idx == 10 || idx == 11 || idx == 12 || idx == 13; // 日期相關的運算符
        }

        private void BtnOK_Click(object sender, EventArgs e)
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