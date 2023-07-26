# Set the path to your ASP.NET app project file (csproj)
$webAppProjectPath = "C:\Users\PC\source\repos\WebApplication1\ConsoleRun\ConsoleRun.csproj"

# Set the path to your integration tests project file (csproj)
$integrationTestsProjectPath = "C:\Users\PC\source\repos\WebApplication1\IntegrationTests\IntegrationTests.csproj"

# Start the ASP.NET app server
Write-Host "Starting the app server..."
Start-Process "dotnet" -ArgumentList "run --project $webAppProjectPath" -NoNewWindow -Wait

# Wait for the server to be ready (adjust the waiting condition as needed)
Write-Host "Waiting for the server to be ready..."
do {
    $response = Invoke-WebRequest -Uri "https://localhost:6941/healthcheck" -UseBasicParsing
    Start-Sleep -Seconds 2
} while ($response.StatusCode -ne 200)

# Run integration tests
Write-Host "Running integration tests..."
dotnet test $integrationTestsProjectPath

# Clean up: Stop the server
Write-Host "Stopping the app server..."
Stop-Process -Name "dotnet"
