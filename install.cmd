@echo off
cls

.paket\paket.bootstrapper.exe 2.62.8
if errorlevel 1 (
    exit /b %errorlevel%
)

.paket\paket.exe restore
if errorlevel 1 (
    exit /b %errorlevel%
)

cd src\Queue
npm install