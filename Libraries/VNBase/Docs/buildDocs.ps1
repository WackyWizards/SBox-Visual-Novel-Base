$DoxyVer = "1.9.8"
$DoxyFile = "docs\doxygen\doxyfile"

# Internal Variables
$FriendlyVersion = $($DoxyVer -replace '\.', '_')
$DownloadUrl = "https://github.com/doxygen/doxygen/releases/download/Release_$FriendlyVersion/doxygen-$DoxyVer.windows.x64.bin.zip"

# Check if doxygen exists, if not, install it
If(!(Test-Path -Path ".\doxygen\bin\doxygen.exe" -PathType Leaf))
{
    # Check if folder exists, if not create it
    If(!(Test-Path -PathType Container "doxygen/bin"))
    {
        New-Item -Path 'doxygen/bin' -ItemType Directory
    }

    # Download doxygen
    Invoke-WebRequest -Uri $DownloadUrl -OutFile "doxygen/doxygen.zip"
    # Unzip doxygen
    Expand-Archive -Path "doxygen/doxygen.zip" -DestinationPath "doxygen/bin"
    # Remove zip file
    Remove-Item -Path "doxygen/doxygen.zip"
}

## Change Working Directory to project root
Set-Location ".\..\"
## Make Docs
$env:PROJECT_NUMBER = 'LOCAL'; &".\docs\doxygen\bin\doxygen.exe" $DoxyFile
## Wait for user responds
Write-Host -NoNewLine 'Press any key to continue...';
$null = $Host.UI.RawUI.ReadKey('NoEcho, IncludeKeyDown');