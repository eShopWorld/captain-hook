$ErrorActionPreference = "Stop"

function Generate-CSharp-Client
{
	<#
    .SYNOPSIS
        Generates a C# Client from OpenAPI for a provided compiled webservice.

    .PARAMETER Configuration
        Cofiguration to use in order to retrieve the binary of the compiled webservice. By default it is set to Debug.

    .PARAMETER ProjectFolder
        Relative location of the project folder

	.PARAMETER DllToGenerateFrom
		Full name of the binary to generate the client from, e.g. Tahoe.dll

	.PARAMETER ClientFolder
		Relative location to the destination folder to output the generated client.

	.PARAMETER ClientName
		Name of C# class with the client code.

	.PARAMETER ClientNamespace
		Namespace of the C# class with the client code.
    #>
	param (
		[string] $Configuration = "Debug",
		[string] $ProjectFolder,
		[string] $DllToGenerateFrom,
		[string] $ClientFolder,
		[string] $ClientName,
		[string] $ClientNamespace)

	$toolsPath = "..\tools"
	Install-Tooling -ToolsPath $toolsPath

	$apiDllPath = "$ProjectFolder\bin\$Configuration\netcoreapp3.1\win-x64\$DllToGenerateFrom"
	$apiDll = Split-Path $apiDllPath -Leaf
	$apiFolder = Split-Path $apiDllPath
	$apiFolderParent = Split-Path $apiFolder
	$tempApiFolder = Join-Path $apiFolderParent 'autorest-temp'
	$tempApiDllPath = Join-Path $tempApiFolder $apiDll	

	Remove-Item $tempApiFolder -Recurse -ErrorAction Ignore | Out-Null
	New-Item $tempApiFolder -ItemType Directory | Out-Null
	Copy-Item "$apiFolder\*" $tempApiFolder -Recurse
	Copy-Item "$ProjectFolder\appsettings.*" $tempApiFolder

	$swaggerPath = Join-Path (Resolve-Path $toolsPath) swagger.exe
	$autorestPath = Join-Path (Resolve-Path $toolsPath) autorest.cmd
	$clientOutputFolder = Resolve-Path $ClientFolder
	Push-Location -Path $tempApiFolder
	
	Set-Item -Path Env:Logging:LogLevel:Microsoft -Value "Warning"
	Set-Item -Path Env:Logging:LogLevel:System -Value "Warning"
	Set-Item -Path Env:ASPNETCORE_ENVIRONMENT -Value "Development"

	Try
	{
		Write-Output "Generating 'swagger.json' from $(Resolve-Path $apiDll) ..."
		$env:KEYVAULT_BASE_URI = "https://esw-tooling-ci-we.vault.azure.net/"
		& $swaggerPath tofile --output swagger.json --serializeasv2 $apiDll v1
		
		& $autorestPath --input-file="swagger.json" --sync-methods=all --add-credentials=true --override-client-name=$ClientName --clear-output-folder --namespace=$ClientNamespace --csharp --output-folder=$clientOutputFolder --client-side-validation=$false
		Write-Output "C# Client '$ClientName' generated in $(Resolve-Path $clientOutputFolder)"
	}
	Finally
	{
		Pop-Location
		Remove-Item $tempApiFolder -Recurse -ErrorAction Ignore
	}
}

function Install-Tooling
{
	param([string] $ToolsPath)

	Write-Output "Installing or updating Swashbuckle CLI"
	dotnet tool update swashbuckle.aspnetcore.cli --version 5.4.1 --tool-path $ToolsPath

	Write-Output "Installing or updating AutoRest"
	npm install autorest@3.0.6187 --no-audit --prefix $ToolsPath
}

Generate-CSharp-Client -ProjectFolder "..\CaptainHook.Api" -DllToGenerateFrom "CaptainHook.Api.dll" -ClientFolder "ApiClient" -ClientName "CaptainHookClient" -ClientNamespace "CaptainHook.Api.Client"

exit 0