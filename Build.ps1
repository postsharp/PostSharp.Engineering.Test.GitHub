(& dotnet nuget locals http-cache -c) | Out-Null
& dotnet run --project "$PSScriptRoot\eng\src\BuildGitHubTestProduct.csproj" -- $args
exit $LASTEXITCODE

