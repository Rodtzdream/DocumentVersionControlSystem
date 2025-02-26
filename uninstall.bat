@echo off
set "mainDir=%LocalAppData%\DocumentVersionControlSystem"

if exist "%mainDir%" (
    rmdir /S /Q "%mainDir%"
    echo All application data removed.
) else (
    echo No application data found.
)

echo.
pause