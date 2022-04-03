# Script to start SWA CLI in order to test authentication/authorization




$FeUrl = "http://localhost:5260"
$BeUrl = "http://localhost:7071"
$SwaUrl = "http://localhost:4280"
$ProjectPath = "C:\Users\esz42romdigr\source\repos\rmdg82\SWA_TodoList"

$StartFe = { dotnet run watch --project $ProjectPath\Client\Client.csproj }
$StartBe = { func host start --script-root $ProjectPath\Api\ }
$StartSwa = { swa start $FeUrl --api-location $BeUrl }


# I'd like to launch fe, be and swa in 3 differents tabs in windows terminal
function Start-InWindowsTerminalTabs {
	# Start fe
	wt nt -p Powershell $StartFe

	# Start be
	wt nt -p Powershell $StartBe

	# Start swa
	wt nt -p Powershell $StartSwa
}

# Start-InWindowsTerminalTabs

wt -p "Powershell" dotnet run watch --project $ProjectPath\Client\Client.csproj `; new-tab -p "Powershell" func host start --script-root $ProjectPath\Api `; new-tab -p "Powershell" swa start $FeUrl --api-location $BeUrl

# Sleep 3 sec
Start-Sleep 3

# Lauch browser
Start-Process microsoft-edge:$SwaUrl