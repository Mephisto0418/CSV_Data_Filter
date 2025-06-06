namespace CSV_Data_Filter
{
    public partial class ConfigManagerForm : Form
    {
        private List<string> _availableConfigs = new List<string>();
        private bool _isLoading = false;
        private Action<Models.UserConfig> _onConfigLoadedCallback;

        public ConfigManagerForm(Action<Models.UserConfig> onConfigLoadedCallback)
        {
            InitializeComponent();
            _onConfigLoadedCallback = onConfigLoadedCallback;
            LoadAvailableConfigs();
        }

        private void LoadAvailableConfigs()
        {
            _isLoading = true;
            
            try
            {
                // 清除現有項目
                lstConfigs.Items.Clear();
                _availableConfigs = Models.UserConfig.GetAvailableConfigs();
                
                // 添加設定到列表
                foreach (var config in _availableConfigs)
                {
                    lstConfigs.Items.Add(config);
                }
                
                if (lstConfigs.Items.Count > 0)
                {
                    btnLoad.Enabled = true;
                    btnDelete.Enabled = true;
                }
                else
                {
                    btnLoad.Enabled = false;
                    btnDelete.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"載入設定時發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            _isLoading = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtConfigName.Text))
                {
                    MessageBox.Show("請輸入設定名稱", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // 創建新的設定
                var config = (Models.UserConfig)Tag;
                config.Name = txtConfigName.Text.Trim();
                
                // 如果設定已存在，詢問是否覆蓋
                if (_availableConfigs.Contains(config.Name) && 
                    MessageBox.Show($"設定 '{config.Name}' 已存在，確定要覆蓋嗎？", 
                                  "確認覆蓋", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
                
                // 保存設定
                if (config.SaveConfig())
                {
                    MessageBox.Show($"設定 '{config.Name}' 已成功儲存", "儲存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAvailableConfigs();
                    
                    // 選中新建的設定
                    for (int i = 0; i < lstConfigs.Items.Count; i++)
                    {
                        if (lstConfigs.Items[i].ToString() == config.Name)
                        {
                            lstConfigs.SelectedIndex = i;
                            break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show($"儲存設定 '{config.Name}' 時發生錯誤", "儲存失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"儲存設定時發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstConfigs.SelectedIndex == -1)
                {
                    MessageBox.Show("請選擇要載入的設定", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                string configName = lstConfigs.SelectedItem.ToString() ?? "";
                var config = Models.UserConfig.LoadConfig(configName);
                
                if (config != null)
                {
                    _onConfigLoadedCallback?.Invoke(config);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"載入設定 '{configName}' 時發生錯誤", "載入失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"載入設定時發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstConfigs.SelectedIndex == -1)
                {
                    MessageBox.Show("請選擇要刪除的設定", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                string configName = lstConfigs.SelectedItem.ToString() ?? "";
                
                if (MessageBox.Show($"確定要刪除設定 '{configName}' 嗎？此操作無法復原。", 
                                   "確認刪除", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (Models.UserConfig.DeleteConfig(configName))
                    {
                        MessageBox.Show($"設定 '{configName}' 已成功刪除", "刪除成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAvailableConfigs();
                    }
                    else
                    {
                        MessageBox.Show($"刪除設定 '{configName}' 時發生錯誤", "刪除失敗", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"刪除設定時發生錯誤: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void lstConfigs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_isLoading && lstConfigs.SelectedIndex != -1)
            {
                txtConfigName.Text = lstConfigs.SelectedItem.ToString() ?? "";
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
