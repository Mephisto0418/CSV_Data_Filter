namespace CSV_Data_Filter.Models
{
    public class ColumnConfig
    {
        public ColumnConfig(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CustomName = name; // 預設自訂名稱與原始名稱相同
            ProcessType = ProcessType.None;
            FindText = string.Empty;
            ReplaceText = string.Empty;
            RegexPattern = string.Empty;
        }

        /// <summary>
        /// 原始欄位名稱
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 用戶自訂的欄位名稱（用於輸出檔案）
        /// </summary>
        public string CustomName { get; set; }

        public ProcessType ProcessType { get; set; }

        // Substring 參數
        public int StartIndex { get; set; } = 0;

        public int SubstringLength { get; set; } = 0;

        // Math 參數
        public decimal MultiplyBy { get; set; } = 1;

        public decimal AddValue { get; set; } = 0;

        // Replace 參數
        public string? FindText { get; set; }

        public string? ReplaceText { get; set; }

        // Regex 參數
        public string? RegexPattern { get; set; }
    }

    public enum ProcessType
    {
        None,
        Substring,
        Math,
        Replace,
        Regex
    }
}