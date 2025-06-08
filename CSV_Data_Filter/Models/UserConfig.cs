using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CSV_Data_Filter.Models
{
    /// <summary>
    /// 使用者配置設定，用於保存和載入應用程式設定
    /// </summary>
    public class UserConfig
    {
        // 基本屬性
        public string Name { get; set; } = "預設設定";
        public List<string> SourcePaths { get; set; } = new List<string>();
        public string TargetPath { get; set; } = "";
        public string OutputFileName { get; set; } = "";
        
        // 資料夾篩選
        public string FolderInclude { get; set; } = "";
        public string FolderExclude { get; set; } = "";
        public bool UseFolderDateFilter { get; set; } = false;
        public string FolderDateFormat { get; set; } = "yyyy-MM-dd";
        public string FolderDateOp { get; set; } = "";
        public DateTime FolderDateValue { get; set; } = DateTime.Now;
        
        // 檔案篩選
        public string FileInclude { get; set; } = "";
        public string FileExclude { get; set; } = "";
        public bool UseFileDateFilter { get; set; } = false;
        public string FileDateFormat { get; set; } = "yyyy-MM-dd";
        public string FileDateOp { get; set; } = "";
        public DateTime FileDateValue { get; set; } = DateTime.Now;
        
        // 欄位設定
        public List<ColumnConfig> ColumnConfigs { get; set; } = new List<ColumnConfig>();
        public List<FilterCondition> FilterConditions { get; set; } = new List<FilterCondition>();
        
        // 選項設定
        public bool AddFileNameColumn { get; set; } = false;
        public bool AddDirectoryNameColumn { get; set; } = false;
        public bool SkipIncompleteFiles { get; set; } = false;
        public bool KeepTempFiles { get; set; } = false;
        public int ThreadCount { get; set; } = Environment.ProcessorCount / 2;
        
        /// <summary>
        /// 取得設定檔案儲存路徑的基礎目錄
        /// </summary>
        public static string GetConfigBasePath()
        {
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CSV_Data_Filter");
            
            // 確保目錄存在
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);
                
            return appDataPath;
        }
        
        /// <summary>
        /// 取得指定名稱的設定檔案完整路徑
        /// </summary>
        public static string GetConfigFilePath(string configName)
        {
            // 清理非法字符，防止路徑注入
            string safeConfigName = string.Join("_", configName.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(GetConfigBasePath(), $"{safeConfigName}.json");
        }
        
        /// <summary>
        /// 取得所有可用的設定檔案名稱
        /// </summary>
        public static List<string> GetAvailableConfigs()
        {
            var results = new List<string>();
            var basePath = GetConfigBasePath();
            
            if (Directory.Exists(basePath))
            {
                foreach (var file in Directory.GetFiles(basePath, "*.json"))
                {
                    results.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// 儲存設定到指定名稱的檔案
        /// </summary>
        public bool SaveConfig()
        {
            if (string.IsNullOrEmpty(Name))
                Name = $"設定_{DateTime.Now:yyyyMMdd_HHmmss}";
                
            try
            {
                var filePath = GetConfigFilePath(Name);
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                string jsonString = JsonSerializer.Serialize(this, options);
                File.WriteAllText(filePath, jsonString);
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// 從指定名稱的檔案載入設定
        /// </summary>
        public static UserConfig? LoadConfig(string configName)
        {
            try
            {
                var filePath = GetConfigFilePath(configName);
                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    var config = JsonSerializer.Deserialize<UserConfig>(jsonString);
                    return config;
                }
            }
            catch
            {
                // 忽略載入錯誤
            }
            
            return null;
        }
        
        /// <summary>
        /// 刪除指定名稱的設定檔案
        /// </summary>
        public static bool DeleteConfig(string configName)
        {
            try
            {
                var filePath = GetConfigFilePath(configName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
            }
            catch
            {
                // 忽略刪除錯誤
            }
            
            return false;
        }
    }
}
