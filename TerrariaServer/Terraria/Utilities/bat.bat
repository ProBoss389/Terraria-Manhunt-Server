@echo off
setlocal enabledelayedexpansion

echo Scanning for namespace-prefixed folders...
echo ------------------------------------------

:: Loop through all directories in the current folder
for /d %%D in (*) do (
    set "folderName=%%D"
    
    :: Remove all dots from the name to see if any existed
    set "strippedName=!folderName:.=!"
    
    if not "!folderName!"=="!strippedName!" (
        
        :: Split at the first dot found
        for /f "tokens=1* delims=." %%a in ("!folderName!") do (
            set "parentPart=%%a"
            set "childPart=%%b"
            
            echo Fixing: !folderName! -^> !parentPart!\!childPart!
            
            :: Create the parent directory (e.g., "ReLogic")
            if not exist "!parentPart!" mkdir "!parentPart!"
            
            :: Move contents and remove the old prefixed folder
            xcopy "%%D\*" "!parentPart!\!childPart!\" /s /e /y /i >nul
            rd /s /q "%%D"
        )
    )
)

echo ------------------------------------------
echo Done! Run this again if you have folders with multiple dots (e.g. A.B.C).
pause