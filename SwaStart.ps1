# Script to start SWA CLI in order to test authentication/authorization

$SwaUrl = "http://localhost:5260"

# Go to project
Set-Location "C:\Users\esz42romdigr\source\repos\rmdg82\SWA_TodoList"

# Start swa
swa start $SwaUrl --api-location .\Api &

# Start fe
dotnet run watch --project .\Client\Client.csproj &

# Start be
#func host start --script-root .\Api\ &

# Sleep 3 sec
Start-Sleep 3

# Lauch browser
Start-Process microsoft-edge:$SwaUrl