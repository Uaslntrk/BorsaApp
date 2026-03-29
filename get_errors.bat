@echo off
dotnet build BorsaApp.Wpf\BorsaApp.Wpf.csproj -v n -clp:ErrorsOnly --no-incremental > errors.log 2>&1
