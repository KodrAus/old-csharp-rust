cargo-nuget pack --cargo-dir .\native\ --nupkg-dir .\feed\
dotnet restore .\dotnet\src\ByteArray.csproj --configfile .\Nuget.Config
dotnet restore .\dotnet\test\ByteArray.Tests.csproj --configfile .\Nuget.Config