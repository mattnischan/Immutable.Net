param
(
	[string]$ProjectFolder,
	[string]$Configuration
)

cd $ProjectFolder

& ./PostBuildScript/nuget.exe pack "$ProjectFolder\Immutable.Net.csproj" -Prop Configuration=$Configuration