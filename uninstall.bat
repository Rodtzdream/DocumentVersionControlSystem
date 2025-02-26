@echo off
set "mainDir=%LocalAppData%\DocumentVersionControlSystem"

if exist "%mainDir%" (
    rmdir /S /Q "%mainDir%"
    if not exist "%mainDir%" (
        echo All application data removed.
    ) else (
        echo Failed to remove application data.
    )
) else (
    echo No application data found.
)

echo.
pause