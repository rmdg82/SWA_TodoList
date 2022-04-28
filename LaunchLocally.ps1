<#
.SYNOPSIS
Script to start SWA CLI in order to test locally authentication/authorization
#>

param (
	[Parameter(Mandatory = $true)]
	[ValidateSet('be', 'fe', 'swa')]
	[string]
	$Mode
)

$FeUrl = "http://localhost:5260"
$BeUrl = "http://localhost:7071"
$SwaUrl = "http://localhost:4280"
$ProjectPath = "C:\Users\esz42romdigr\source\repos\rmdg82\SWA_TodoList"

function Start-Fe {
	dotnet run watch --project $ProjectPath\Client\Client.csproj
}

function Start-Be {
	func host start --script-root $ProjectPath\Api\ --verbose
}

function Start-Swa {
	swa start $FeUrl --api-location $BeUrl
	Start-Sleep 3
	Start-Process microsoft-edge:$SwaUrl
}

switch ($Mode) {
	'fe' { Start-Fe }
	'be' { Start-Be }
	'swa' { Start-Swa }
}

# I'd like to launch fe, be and swa in 3 differents tabs in windows terminal but it doesn't work'
# wt -p "Powershell" dotnet run watch --project $ProjectPath\Client\Client.csproj `; new-tab -p "Powershell" func host start --script-root $ProjectPath\Api `; new-tab -p "Powershell" swa start $FeUrl --api-location $BeUrl
