# Script to start SWA CLI in order to test authentication/authorization

$FeUrl = "http://localhost:5260"
$BeUrl = "http://localhost:7071"
$SwaUrl = "http://localhost:4280"
$ProjectPath = "C:\Users\esz42romdigr\source\repos\rmdg82\SWA_TodoList"

# Go to project
# Set-Location "C:\Users\esz42romdigr\source\repos\rmdg82\SWA_TodoList"

# Start fe
wt -p Powershell "dotnet run watch --project $ProjectPath\Client\Client.csproj"

# Start be
wt -p Powershell "func host start --script-root $ProjectPath\Api\"

# Start swa
wt -p Powershell "swa start $FeUrl --api-location $BeUrl"

# Sleep 3 sec
Start-Sleep 3

# Lauch browser
Start-Process microsoft-edge:$SwaUrl