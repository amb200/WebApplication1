$projectPath = "C:\Users\PC\source\repos\WebApplication1\ConsoleRun\ConsoleRun.csproj"
$envVariables = @{
    ASPNETCORE_ENVIRONMENT = "IntegrationTest"
    Port = "6941"
}

$envVariablesString = $envVariables.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" } -join ";"

Start-Process "dotnet" -ArgumentList "run", "--project", $projectPath, "--", "--urls", "http://localhost:$($envVariables['Port'])" -NoNewWindow -Wait -Env $envVariablesString
