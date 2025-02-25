@echo off

REM Removing program files
set "targetDir=%LocalAppData%\DocumentVersionControlSystem\Documents"
if exist "%targetDir%" (
    rmdir /S /Q "%targetDir%"
    echo Document files removed.
) else (
    echo No document files found to remove.
)

REM Removing database file
set "dbFile=%LocalAppData%\DocumentVersionControlSystem\database.db"
if exist "%dbFile%" (
    del /Q "%dbFile%"
    echo Database removed.
) else (
    echo No database file found to remove.
)

echo.
pause