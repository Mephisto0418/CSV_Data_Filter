namespace CSV_Data_Filter
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new CSV_Data_Filter());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "啟動錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}