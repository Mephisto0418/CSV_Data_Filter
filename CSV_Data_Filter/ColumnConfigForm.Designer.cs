using System;
using System.Drawing;
using System.Windows.Forms;

namespace CSV_Data_Filter
{
    partial class ColumnConfigForm
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
            txtCustomName = new TextBox();
            cboProcessType = new ComboBox();
            pnlSubstring = new Panel();
            nudStartIndex = new NumericUpDown();
            nudLength = new NumericUpDown();
            lblStartIndex = new Label();
            lblLength = new Label();
            pnlMath = new Panel();
            nudMultiply = new NumericUpDown();
            nudAdd = new NumericUpDown();
            lblMultiply = new Label();
            lblAdd = new Label();
            pnlReplace = new Panel();
            txtFind = new TextBox();
            txtReplace = new TextBox();
            lblFind = new Label();
            lblReplace = new Label();
            pnlRegex = new Panel();
            txtRegex = new TextBox();
            lblRegex = new Label();
            lblCustomName = new Label();
            lblProcessType = new Label();
            btnOK = new Button();
            btnCancel = new Button();
            pnlSubstring.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudStartIndex).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudLength).BeginInit();
            pnlMath.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudMultiply).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudAdd).BeginInit();
            pnlReplace.SuspendLayout();
            pnlRegex.SuspendLayout();
            SuspendLayout();
            
            // txtCustomName
            // 
            txtCustomName.Location = new Point(120, 18);
            txtCustomName.Name = "txtCustomName";
            txtCustomName.Size = new Size(200, 23);
            txtCustomName.TabIndex = 1;
            
            // cboProcessType
            // 
            cboProcessType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboProcessType.Location = new Point(120, 53);
            cboProcessType.Name = "cboProcessType";
            cboProcessType.Size = new Size(200, 23);
            cboProcessType.TabIndex = 3;
            cboProcessType.Items.AddRange(new object[] {
                "不處理",
                "取子字串",
                "數值運算",
                "字串替換",
                "正規表達式"
            });
            cboProcessType.SelectedIndexChanged += CboProcessType_SelectedIndexChanged;
            
            // pnlSubstring
            // 
            pnlSubstring.Location = new Point(20, 95);
            pnlSubstring.Name = "pnlSubstring";
            pnlSubstring.Size = new Size(400, 150);
            pnlSubstring.TabIndex = 4;
            pnlSubstring.Visible = false;
            pnlSubstring.Controls.AddRange(new Control[] { 
                lblStartIndex, 
                nudStartIndex, 
                lblLength, 
                nudLength 
            });
            
            // nudStartIndex
            // 
            nudStartIndex.Location = new Point(100, 8);
            nudStartIndex.Name = "nudStartIndex";
            nudStartIndex.Size = new Size(80, 23);
            nudStartIndex.TabIndex = 1;
            nudStartIndex.Minimum = 0;
            nudStartIndex.Maximum = 1000;
            
            // nudLength
            // 
            nudLength.Location = new Point(100, 38);
            nudLength.Name = "nudLength";
            nudLength.Size = new Size(80, 23);
            nudLength.TabIndex = 3;
            nudLength.Minimum = 0;
            nudLength.Maximum = 1000;
            
            // lblStartIndex
            // 
            lblStartIndex.AutoSize = true;
            lblStartIndex.Location = new Point(0, 10);
            lblStartIndex.Name = "lblStartIndex";
            lblStartIndex.Size = new Size(61, 15);
            lblStartIndex.TabIndex = 0;
            lblStartIndex.Text = "起始位置:";
            
            // lblLength
            // 
            lblLength.AutoSize = true;
            lblLength.Location = new Point(0, 40);
            lblLength.Name = "lblLength";
            lblLength.Size = new Size(35, 15);
            lblLength.TabIndex = 2;
            lblLength.Text = "長度:";
            
            // pnlMath
            // 
            pnlMath.Location = new Point(20, 95);
            pnlMath.Name = "pnlMath";
            pnlMath.Size = new Size(400, 150);
            pnlMath.TabIndex = 5;
            pnlMath.Visible = false;
            pnlMath.Controls.AddRange(new Control[] { 
                lblMultiply, 
                nudMultiply, 
                lblAdd, 
                nudAdd 
            });
            
            // nudMultiply
            // 
            nudMultiply.DecimalPlaces = 3;
            nudMultiply.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            nudMultiply.Location = new Point(100, 8);
            nudMultiply.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            nudMultiply.Maximum = 1000M;
            nudMultiply.Name = "nudMultiply";
            nudMultiply.Size = new Size(120, 23);
            nudMultiply.TabIndex = 1;
            nudMultiply.Value = 1M;
            
            // nudAdd
            // 
            nudAdd.DecimalPlaces = 3;
            nudAdd.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            nudAdd.Location = new Point(100, 38);
            nudAdd.Minimum = new decimal(new int[] { 1000, 0, 0, int.MinValue });
            nudAdd.Maximum = 1000M;
            nudAdd.Name = "nudAdd";
            nudAdd.Size = new Size(120, 23);
            nudAdd.TabIndex = 3;
            nudAdd.Value = 0M;
            
            // lblMultiply
            // 
            lblMultiply.AutoSize = true;
            lblMultiply.Location = new Point(0, 10);
            lblMultiply.Name = "lblMultiply";
            lblMultiply.Size = new Size(35, 15);
            lblMultiply.TabIndex = 0;
            lblMultiply.Text = "乘以:";
            
            // lblAdd
            // 
            lblAdd.AutoSize = true;
            lblAdd.Location = new Point(0, 40);
            lblAdd.Name = "lblAdd";
            lblAdd.Size = new Size(35, 15);
            lblAdd.TabIndex = 2;
            lblAdd.Text = "加上:";
            
            // pnlReplace
            // 
            pnlReplace.Location = new Point(20, 95);
            pnlReplace.Name = "pnlReplace";
            pnlReplace.Size = new Size(400, 150);
            pnlReplace.TabIndex = 6;
            pnlReplace.Visible = false;
            pnlReplace.Controls.AddRange(new Control[] { 
                lblFind, 
                txtFind, 
                lblReplace, 
                txtReplace 
            });
            
            // txtFind
            // 
            txtFind.Location = new Point(100, 8);
            txtFind.Name = "txtFind";
            txtFind.Size = new Size(280, 23);
            txtFind.TabIndex = 1;
            
            // txtReplace
            // 
            txtReplace.Location = new Point(100, 38);
            txtReplace.Name = "txtReplace";
            txtReplace.Size = new Size(280, 23);
            txtReplace.TabIndex = 3;
            
            // lblFind
            // 
            lblFind.AutoSize = true;
            lblFind.Location = new Point(0, 10);
            lblFind.Name = "lblFind";
            lblFind.Size = new Size(35, 15);
            lblFind.TabIndex = 0;
            lblFind.Text = "尋找:";
            
            // lblReplace
            // 
            lblReplace.AutoSize = true;
            lblReplace.Location = new Point(0, 40);
            lblReplace.Name = "lblReplace";
            lblReplace.Size = new Size(50, 15);
            lblReplace.TabIndex = 2;
            lblReplace.Text = "替換為:";
            
            // pnlRegex
            // 
            pnlRegex.Location = new Point(20, 95);
            pnlRegex.Name = "pnlRegex";
            pnlRegex.Size = new Size(400, 150);
            pnlRegex.TabIndex = 7;
            pnlRegex.Visible = false;
            pnlRegex.Controls.AddRange(new Control[] { 
                lblRegex, 
                txtRegex 
            });
            
            // txtRegex
            // 
            txtRegex.Location = new Point(0, 40);
            txtRegex.Name = "txtRegex";
            txtRegex.Size = new Size(380, 23);
            txtRegex.TabIndex = 1;
            
            // lblRegex
            // 
            lblRegex.AutoSize = true;
            lblRegex.Location = new Point(0, 10);
            lblRegex.Name = "lblRegex";
            lblRegex.Size = new Size(71, 15);
            lblRegex.TabIndex = 0;
            lblRegex.Text = "正規表達式:";
            
            // lblCustomName
            // 
            lblCustomName.AutoSize = true;
            lblCustomName.Location = new Point(20, 20);
            lblCustomName.Name = "lblCustomName";
            lblCustomName.Size = new Size(83, 15);
            lblCustomName.TabIndex = 0;
            lblCustomName.Text = "自訂欄位名稱:";
            
            // lblProcessType
            // 
            lblProcessType.AutoSize = true;
            lblProcessType.Location = new Point(20, 55);
            lblProcessType.Name = "lblProcessType";
            lblProcessType.Size = new Size(59, 15);
            lblProcessType.TabIndex = 2;
            lblProcessType.Text = "處理方式:";
            
            // btnOK
            // 
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(260, 270);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(80, 30);
            btnOK.TabIndex = 8;
            btnOK.Text = "確定";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += BtnOK_Click;
            
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(350, 270);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 30);
            btnCancel.TabIndex = 9;
            btnCancel.Text = "取消";
            btnCancel.UseVisualStyleBackColor = true;
            
            // ColumnConfigForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(450, 350);
            Controls.Add(lblCustomName);
            Controls.Add(txtCustomName);
            Controls.Add(lblProcessType);
            Controls.Add(cboProcessType);
            Controls.Add(pnlSubstring);
            Controls.Add(pnlMath);
            Controls.Add(pnlReplace);
            Controls.Add(pnlRegex);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ColumnConfigForm";
            StartPosition = FormStartPosition.CenterParent;
            pnlSubstring.ResumeLayout(false);
            pnlSubstring.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudStartIndex).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudLength).EndInit();
            pnlMath.ResumeLayout(false);
            pnlMath.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudMultiply).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudAdd).EndInit();
            pnlReplace.ResumeLayout(false);
            pnlReplace.PerformLayout();
            pnlRegex.ResumeLayout(false);
            pnlRegex.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtCustomName;
        private ComboBox cboProcessType;
        private Panel pnlSubstring;
        private Panel pnlMath;
        private Panel pnlReplace;
        private Panel pnlRegex;
        private NumericUpDown nudStartIndex;
        private NumericUpDown nudLength;
        private Label lblStartIndex;
        private Label lblLength;
        private NumericUpDown nudMultiply;
        private NumericUpDown nudAdd;
        private Label lblMultiply;
        private Label lblAdd;
        private TextBox txtFind;
        private TextBox txtReplace;
        private Label lblFind;
        private Label lblReplace;
        private TextBox txtRegex;
        private Label lblRegex;
        private Label lblCustomName;
        private Label lblProcessType;
        private Button btnOK;
        private Button btnCancel;
    }
}
