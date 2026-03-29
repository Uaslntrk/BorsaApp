$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

$DesktopPath = [Environment]::GetFolderPath("Desktop")
$ShortcutPath = Join-Path -Path $DesktopPath -ChildPath "BorsaApp.lnk"

$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut($ShortcutPath)

# Point shortcut to start.bat
$Shortcut.TargetPath = Join-Path -Path $ScriptDir -ChildPath "start.bat"
$Shortcut.WorkingDirectory = $ScriptDir
$Shortcut.Description = "Launch BorsaApp"

# Provide C# App icon if it exists, otherwise it will use default bat icon
$IconPath = Join-Path -Path $ScriptDir -ChildPath "BorsaApp.Wpf\Assets\logo.ico"
if (Test-Path $IconPath) {
    $Shortcut.IconLocation = $IconPath
} else {
    # Fallback to dotnet icon or similar if needed, or leave default
}

$Shortcut.Save()

Write-Host "Desktop shortcut created successfully at: $ShortcutPath" -ForegroundColor Green
Read-Host "Press Enter to exit..."
