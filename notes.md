### To build

dotnet publish .\ArcdpsLogManager.Avalonia\ArcdpsLogManager.Avalonia.csproj -c Release -r win-x64 `
>>   --self-contained false `
>>   -p:PublishSingleFile=true `
>>   -p:IncludeNativeLibrariesForSelfExtract=true `
>>   -p:IncludeAllContentForSelfExtract=true `
>>   -o publish/win-x64

