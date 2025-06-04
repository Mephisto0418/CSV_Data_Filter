cd CSV_Data_Filter
dotnet publish -c public -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false /p:PublishReadyToRun=true /p:IncludeNativeLibrariesForSelfExtract=true