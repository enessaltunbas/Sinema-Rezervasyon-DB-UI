@echo off
chcp 65001 >nul
cd /d "%~dp0"
set MYSQL="C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe"

echo Sinema veritabani kuruluyor (root sifresi sorulacak)...
%MYSQL% -u root -p --default-character-set=utf8mb4 -e "source sinema_db.sql;"

if errorlevel 1 (
    echo.
    echo HATA olustu, yukaridaki mesaja bakin.
) else (
    echo.
    echo Kurulum tamam.
)
pause
