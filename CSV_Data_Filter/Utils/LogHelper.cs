using System;
using System.Windows.Forms;

namespace CSV_Data_Filter.Utils
{
    /// <summary>
    /// 提供日誌記錄的輔助功能
    /// </summary>
    public static class LogHelper
    {
        /// <summary>
        /// 將日誌消息添加到ListBox控件中
        /// </summary>
        /// <param name="form">當前窗體</param>
        /// <param name="lstLog">日誌控件</param>
        /// <param name="message">日誌消息</param>
        public static void AddLog(Form form, ListBox? lstLog, string message)
        {
            if (lstLog == null) return;
            
            if (form.InvokeRequired)
            {
                form.Invoke((Action)(() => AddLog(form, lstLog, message)));
                return;
            }
            
            lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            lstLog.SelectedIndex = lstLog.Items.Count - 1;
        }
    }
}
