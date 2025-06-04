cd CSV_Data_Filter
REM 原始發布指令
REM dotnet publish -c public -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false /p:PublishReadyToRun=true /p:IncludeNativeLibrariesForSelfExtract=true

REM 替代發布指令 - 可能更不容易觸發防毒軟體
dotnet publish -c public -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false /p:IncludeNativeLibrariesForSelfExtract=true