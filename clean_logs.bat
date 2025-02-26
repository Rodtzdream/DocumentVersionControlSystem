@echo off
set "logFile=%LocalAppData%\DocumentVersionControlSystem\log.txt"

if exist "%logFile%" (
    del /Q "%logFile%"
    echo Log file removed.
) else (
    echo No log file found to remove.
)

echo.
pause
