# Script to start SWA CLI in order to test authentication/authorization

$FeUrl = "http://localhost:5260"
$SwaUrl = "http://localhost:4280"

# Go to project
Set-Location "C:\Users\esz42romdigr\source\repos\rmdg82\SWA_TodoList"

# Start fe
dotnet run watch --project .\Client\Client.csproj &

# Start swa
swa start $FeUrl --api-location .\Api &

# Start be
#func host start --script-root .\Api\ &

# Sleep 3 sec
Start-Sleep 3

# Lauch browser
Start-Process microsoft-edge:$SwaUrl