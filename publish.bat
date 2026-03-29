@echo off
title BorsaApp - Publish release
echo Publishing BorsaApp for Windows x64...
cd "%~dp0BorsaApp.Wpf"
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "%~dp0dist"
echo.
echo ==========================================================
echo Application successfully published to the 'dist' folder.
echo You can run the portable BorsaApp.Wpf.exe from there.
echo ==========================================================
pause
