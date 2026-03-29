@echo off
title BorsaApp - Hot Reload (Development)
echo Starting BorsaApp WPF Application with Hot Reload...
echo Save your C# or XAML files to see the UI update automatically!
cd "%~dp0BorsaApp.Wpf"
dotnet watch run
pause
