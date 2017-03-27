cargo-nuget pack --cargo-dir .\native\ --nupkg-dir .\feed\
dotnet restore .\dotnet\src\dotnet.csproj --configfile .\Nuget.Config 