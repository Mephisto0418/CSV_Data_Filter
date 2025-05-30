using System;
using System.Drawing;
using System.Windows.Forms;

namespace CSV_Data_Filter
{
    partial class FilterConditionForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected new void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cboColumns = new ComboBox();
            cboOperator = new ComboBox();
            txtValue = new TextBox();
            pnlDate = new Panel();
            txtDateFormat = new TextBox();
            lblDateFormat = new Label();
            lblColumn = new Label();
            lblOperator = new Label();
            lblValue = new Label();
            btnOK = new Button();
            btnCancel = new Button();
            pnlDate.SuspendLayout();
            SuspendLayout();
            
            // cboColumns
            // 
            cboColumns.DropDownStyle = ComboBoxStyle.DropDownList;
            cboColumns.Location = new Point(100, 18);
            cboColumns.Name = "cboColumns";
            cboColumns.Size = new Size(200, 23);
            cboColumns.TabIndex = 1;
            
            // cboOperator
            // 
            cboOperator.DropDownStyle = ComboBoxStyle.DropDownList;
            cboOperator.Location = new Point(100, 48);
            cboOperator.Name = "cboOperator";
            cboOperator.Size = new Size(200, 23);
            cboOperator.TabIndex = 3;
            cboOperator.Items.AddRange(new object[] {
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
            cboOperator.SelectedIndexChanged += CboOperator_SelectedIndexChanged;
            
            // txtValue
            // 
            txtValue.Location = new Point(100, 78);
            txtValue.Name = "txtValue";
            txtValue.Size = new Size(320, 23);
            txtValue.TabIndex = 5;
            
            // pnlDate
            // 
            pnlDate.Location = new Point(20, 110);
            pnlDate.Name = "pnlDate";
            pnlDate.Size = new Size(400, 40);
            pnlDate.TabIndex = 6;
            pnlDate.Visible = false;
            pnlDate.Controls.Add(lblDateFormat);
            pnlDate.Controls.Add(txtDateFormat);
            
            // txtDateFormat
            // 
            txtDateFormat.Location = new Point(80, 3);
            txtDateFormat.Name = "txtDateFormat";
            txtDateFormat.Size = new Size(150, 23);
            txtDateFormat.TabIndex = 1;
            txtDateFormat.Text = "yyyy-MM-dd";
            
            // lblDateFormat
            // 
            lblDateFormat.AutoSize = true;
            lblDateFormat.Location = new Point(0, 5);
            lblDateFormat.Name = "lblDateFormat";
            lblDateFormat.Size = new Size(59, 15);
            lblDateFormat.TabIndex = 0;
            lblDateFormat.Text = "日期格式:";
            
            // lblColumn
            // 
            lblColumn.AutoSize = true;
            lblColumn.Location = new Point(20, 20);
            lblColumn.Name = "lblColumn";
            lblColumn.Size = new Size(35, 15);
            lblColumn.TabIndex = 0;
            lblColumn.Text = "欄位:";
            
            // lblOperator
            // 
            lblOperator.AutoSize = true;
            lblOperator.Location = new Point(20, 50);
            lblOperator.Name = "lblOperator";
            lblOperator.Size = new Size(47, 15);
            lblOperator.TabIndex = 2;
            lblOperator.Text = "運算子:";
            
            // lblValue
            // 
            lblValue.AutoSize = true;
            lblValue.Location = new Point(20, 80);
            lblValue.Name = "lblValue";
            lblValue.Size = new Size(23, 15);
            lblValue.TabIndex = 4;
            lblValue.Text = "值:";
            
            // btnOK
            // 
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(260, 170);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(80, 30);
            btnOK.TabIndex = 7;
            btnOK.Text = "確定";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(350, 170);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 30);
            btnCancel.TabIndex = 8;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            
            // FilterConditionForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(450, 250);
            Controls.Add(lblColumn);
            Controls.Add(cboColumns);
            Controls.Add(lblOperator);
            Controls.Add(cboOperator);
            Controls.Add(lblValue);
            Controls.Add(txtValue);
            Controls.Add(pnlDate);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FilterConditionForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "新增篩選條件";
            pnlDate.ResumeLayout(false);
            pnlDate.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ComboBox cboColumns;
        private ComboBox cboOperator;
        private TextBox txtValue;
        private Panel pnlDate;
        private TextBox txtDateFormat;
        private Label lblDateFormat;
        private Label lblColumn;
        private Label lblOperator;
        private Label lblValue;
        private Button btnOK;
        private Button btnCancel;
    }
}
