cargo-nuget pack --cargo-dir .\native\ --nupkg-dir .\feed\
dotnet restore .\dotnet\dotnet.csproj --configfile .\Nuget.Config 