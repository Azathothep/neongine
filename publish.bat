dotnet publish -c Release -r win-x64 -p:PublishReadyToRun=false -p:TieredCompilation=false --self-contained /p:DefineConstants=NEONGINE_BUILD
explorer bin\Release\net8.0\win-x64\publish\