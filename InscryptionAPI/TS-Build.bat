@echo off
SETLOCAL ENABLEDELAYEDEXPANSION

REM -------------------------------
REM THIS SCRIPT IS PROVIDED BY CHAOSYR
REM THIS IS LICENSED UNDER SML-1.0.0
REM IT CAN BE FOUND HERE: https://stoatgames.icu/license-repository/licenses/sml-1-0-0/#content
REM IF YOU REUSE THIS FOR YOUR PROJECT, THIS COMMENT BLOCK MUST REMAIN IN TACT FOR LEGAL REASONS!!
REM -------------------------------

REM -------------------------------
REM Config
REM -------------------------------
set "PrimaryCSProj=%~dp0InscryptionAPI.CSProj"

if not exist "%PrimaryCSProj%" (
    echo ERROR: Project file not found: "%PrimaryCSProj%"
    exit /b 1
)

REM -------------------------------
REM Read Secondary CSProj's
REM -------------------------------
set "SCPRJs="
for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<SecondaryCSProjLocations>"`) do (
    set "line=%%A"
    set "line=!line:<SecondaryCSProjLocations>=!"
    set "line=!line:</SecondaryCSProjLocations>=!"
    set "line=!line:;= !"
    for %%B in (!line!) do set "SCPRJs=!SCPRJs! %%B"
)
set "SCPRJs=!SCPRJs:~1!"

REM -------------------------------
REM Read TargetFrameworks
REM -------------------------------
set "TFs="
for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<TargetFrameworks>"`) do (
    set "line=%%A"
    set "line=!line:<TargetFrameworks>=!"
    set "line=!line:</TargetFrameworks>=!"
    set "line=!line:;= !"
    for %%B in (!line!) do set "TFs=!TFs! %%B"
)
set "TFs=!TFs:~1!"

REM -------------------------------
REM Read RuntimeIdentifiers
REM -------------------------------
set "RIDs="
for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<RuntimeIdentifiers>"`) do (
    set "line=%%A"
    set "line=!line:<RuntimeIdentifiers>=!"
    set "line=!line:</RuntimeIdentifiers>=!"
    set "line=!line:;= !"
    for %%B in (!line!) do set "RIDs=!RIDs! %%B"
)
set "RIDs=!RIDs:~1!"

REM -------------------------------
REM Read Copy Locations
REM -------------------------------
set "CLs="
for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<BuildCopyLocations>"`) do (
    set "line=%%A"
    set "line=!line:<BuildCopyLocations>=!"
    set "line=!line:</BuildCopyLocations>=!"
    set "line=!line:;= !"
    for %%B in (!line!) do set "CLs=!CLs! %%B"
)
set "CLs=!CLs:~1!"

REM -------------------------------
REM Read Dependency Locations
REM -------------------------------
set "DPLs="
for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<DependenciesCopyLocations>"`) do (
    set "line=%%A"
    set "line=!line:<DependenciesCopyLocations>=!"
    set "line=!line:</DependenciesCopyLocations>=!"
    set "line=!line:;= !"
    for %%B in (!line!) do set "DPLs=!DPLs! %%B"
)
set "DPLs=!DPLs:~1!"

REM -------------------------------
REM Read Build Locations
REM -------------------------------
set "BPLs="
for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<BuildOutputLocations>"`) do (
    set "line=%%A"
    set "line=!line:<BuildOutputLocations>=!"
    set "line=!line:</BuildOutputLocations>=!"
    set "line=!line:;= !"
    for %%B in (!line!) do set "BPLs=!BPLs! %%B"
)
set "BPLs=!BPLs:~1!"

REM -------------------------------
REM Read Markdown File Locations
REM -------------------------------
set "CHANGELOG="
set "README="
set "LICENSE="

for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<CHANGELOGCopyLocation>"`) do (
    set "line=%%A"
    set "line=!line:<CHANGELOGCopyLocation>=!"
    set "line=!line:</CHANGELOGCopyLocation>=!"
    set "CHANGELOG=!line!"
)

for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<READMECopyLocation>"`) do (
    set "line=%%A"
    set "line=!line:<READMECopyLocation>=!"
    set "line=!line:</READMECopyLocation>=!"
    set "README=!line!"
)

for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<LICENSECopyLocation>"`) do (
    set "line=%%A"
    set "line=!line:<LICENSECopyLocation>=!"
    set "line=!line:</LICENSECopyLocation>=!"
    set "LICENSE=!line!"
)

REM -------------------------------
REM Read Release Mode
REM -------------------------------

set "RELEASEMODE="
for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<ReleaseMode>"`) do (
    set "line=%%A"
    set "line=!line:<ReleaseMode>=!"
    set "line=!line:</ReleaseMode>=!"
    set "RELEASEMODE=!line!"
)

if "!RELEASEMODE!"=="" set "RELEASEMODE=Debug"

if /I "!RELEASEMODE!"=="Release" (
    set "OUTPUT_DIR=%~dp0GITHUB RELEASE"
) else (
    if /I "!RELEASEMODE!"=="Debug" (
        set "OUTPUT_DIR=%~dp0GITHUB PRERELEASE"
    ) else (
        set "RELEASEMODE=Debug"
        set "OUTPUT_DIR=%~dp0GITHUB PRERELEASE"
    )
)

REM Normalize full path
for %%I in ("%OUTPUT_DIR%") do set "OUTPUT_DIR=%%~fI"

REM -------------------------------
REM Clean output root safely
REM -------------------------------
if exist "%OUTPUT_DIR%" rd /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

mkdir "%OUTPUT_DIR%\NET"

set "TMP_ROOT=%OUTPUT_DIR%\_tmp"
mkdir "%TMP_ROOT%"

set "MANIFEST_ROOT=%OUTPUT_DIR%\_manifest"
mkdir "%MANIFEST_ROOT%"

REM -------------------------------
REM Tell User the Settings
REM -------------------------------

if "!TFs!"=="~1" set "TFs=net6.0"
if "!RIDs!"=="~1" set "RIDs=win-x64"
if "!CLs!"=="~1" set "CLs=Thunderstore-Package-Elements"
if "!DPLs!"=="~1" set "DPLs=exclude"
if "!BPLs!"=="~1" set "BPLs=root"

echo CONFIGURATION
echo SecondaryCSProjs: !SCPRJs!
echo TargetFrameworks: !TFs!
echo RuntimeIdentifiers: !RIDs!
echo CopyLocations: !CLs!
echo DependencyLocations: !DPLs!
echo CHANGELOGCopyLocation: !CHANGELOG!
echo READMECopyLocation: !README!
echo LICENSECopyLocation: !LICENSE!
echo BuildOutputLocations: !BPLs!
echo ReleaseMode: !RELEASEMODE!

REM -------------------------------
REM Fetch Manifest Properties
REM -------------------------------
set "NAME="
set "VERSION="
set "WEBSITEURL="
set "DESCRIPTION="
set "DEPENDENCIES="

for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<ThunderstorePackageName>"`) do (
    set "line=%%A"
    set "line=!line:<ThunderstorePackageName>=!"
    set "line=!line:</ThunderstorePackageName>=!"
    set "NAME=!line!"
)

for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<ThunderstorePackageVersion>"`) do (
    set "line=%%A"
    set "line=!line:<ThunderstorePackageVersion>=!"
    set "line=!line:</ThunderstorePackageVersion>=!"
    set "VERSION=!line!"
)

for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<ThunderstoreWebsiteURL>"`) do (
    set "line=%%A"
    set "line=!line:<ThunderstoreWebsiteURL>=!"
    set "line=!line:</ThunderstoreWebsiteURL>=!"
    set "WEBSITEURL=!line!"
)

for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<ThunderstorePackageDescription>"`) do (
    set "line=%%A"
    set "line=!line:<ThunderstorePackageDescription>=!"
    set "line=!line:</ThunderstorePackageDescription>=!"
    set "DESCRIPTION=!line!"
)

for /f "usebackq tokens=*" %%A in (`type "%PrimaryCSProj%" ^| findstr "<ThunderstorePackageDependencyStrings>"`) do (
    set "line=%%A"
    set "line=!line:<ThunderstorePackageDependencyStrings>=!"
    set "line=!line:</ThunderstorePackageDependencyStrings>=!"
    set "line=!line:;= !"
    for %%B in (!line!) do set "DEPENDENCIES=!DEPENDENCIES! %%B"
)
set "DEPENDENCIES=!DEPENDENCIES:~1!"

if "!DEPENDENCIES!"=="~1" set "DEPENDENCIES="

set "DEPENDENCYCOUNT=0"

for %%D in (!DEPENDENCIES!) do (
    set /a DEPENDENCYCOUNT+=1
)

echo MANIFEST PROPERTIES
echo NAME: !NAME!
echo VERSION: !VERSION!
echo WEBSITEURL: !WEBSITEURL!
echo DESCRIPTION: !DESCRIPTION!
echo DEPENDENCIES: !DEPENDENCIES!
echo DEPENDENCYCOUNT: !DEPENDENCYCOUNT!

REM -------------------------------
REM Build Manifest File
REM -------------------------------

set "OUT=%MANIFEST_ROOT%\manifest.json"

(
    echo {
    echo    "name": "!NAME!",
    echo    "version_number": "!VERSION!",
    echo    "website_url": "!WEBSITEURL!",
    echo    "description": "!DESCRIPTION!",
    echo    "dependencies": [
) > "!OUT!"

if "!DEPENDENCYCOUNT!"=="0" (
    echo No Dependencies Skipping
) else (
    set "INDEX=0"
    
    for %%D in (!DEPENDENCIES!) do (
        set /a INDEX+=1
    
        if "!INDEX!"=="!DEPENDENCYCOUNT!" (
            echo         "%%D"
        ) else (
            echo         "%%D",
        )
    ) >> "!OUT!"
)

(
    echo    ]
    echo }
) >> "!OUT!"

REM -------------------------------
REM Build Loop
REM -------------------------------
for %%F in (!TFs!) do (
    for %%R in (!RIDs!) do (

        set "TF_NAME=%%F"
        set "RID_FLAT=%%R"
        set "OUT=!TMP_ROOT!\!TF_NAME!-!RID_FLAT!"

        if exist "!OUT!" rd /s /q "!OUT!"
        mkdir "!OUT!"
        if "!BPLs!" == "root" (
            echo Told to Export to root Location we are not making any Directories. Skipping.
        ) else (
            for %%B in (!BPLs!) do (
                mkdir "!OUT!\%%B"
            )
        )

        if "!BPLs!"=="root" (
            echo Publishing NET: %%F / %%R
            dotnet publish "%PrimaryCSProj%" -c "!RELEASEMODE!" -f "%%F" -r "%%R" -o "!OUT!" || echo Failed %%F/%%R
            for %%P in (!SCPRJs!) do (
                echo Publishing NET: %%P
                dotnet publish %%P -c "!RELEASEMODE!" -f "%%F" -r "%%R" -o "!OUT!" || echo Failed %%F/%%R
            )
        ) else (
            for %%B in (!BPLs!) do (
                echo Publishing NET: %%F / %%R
                dotnet publish "%PrimaryCSProj%" -c "!RELEASEMODE!" -f "%%F" -r "%%R" -o "!OUT!\%%B" || echo Failed %%F/%%R
                for %%P in (!SCPRJs!) do (
                    echo Publishing NET: %%P
                    dotnet publish %%P -c "!RELEASEMODE!" -f "%%F" -r "%%R" -o "!OUT!\%%B" || echo Failed %%F/%%R
                )
            )
        )
        copy /Y "%~dp0!CHANGELOG!" "!OUT!"
        copy /Y "%~dp0!README!" "!OUT!"
        copy /Y "%~dp0!LICENSE!" "!OUT!"
        copy /Y "%MANIFEST_ROOT%\manifest.json" "!OUT!"
        if "!DPLs!"=="exclude" (
            echo Told to Exclude on Dependency Locations as the field became a Empty field. Skipping.
        ) else (
            mkdir "!OUT!\plugins"
            mkdir "!OUT!\plugins\Dependencies"
            for %%L in (!DPLs!) do (
                xcopy /E /I /Y "%%L" "!OUT!\plugins\Dependencies"
            )
        )
        for %%L in (!CLs!) do (
            xcopy /E /I /Y "%%L" "!OUT!"
        )
        set "ZIP_OUT=%OUTPUT_DIR%\NET\!TF_NAME!-!RID_FLAT!-!RELEASEMODE!.zip"

        powershell -NoProfile -Command ^ "Compress-Archive -Path '!OUT!\*' -DestinationPath '!ZIP_OUT!' -Force"

        rd /s /q "!OUT!"
    )
)

REM -------------------------------
REM Remove temp root completely
REM -------------------------------
rd /s /q "%TMP_ROOT%"
rd /s /q "%MANIFEST_ROOT%"

echo.
echo Build completed! Artifacts in:
echo %OUTPUT_DIR%

ENDLOCAL