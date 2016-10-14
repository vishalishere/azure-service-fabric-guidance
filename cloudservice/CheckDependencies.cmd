@echo off

ver| find "6.2"
if %ERRORLEVEL% == 0 goto win_8
DEL .\Setup\DependencyChecker\DependencyChecker.exe.config"
COPY ".\Setup\DependencyChecker\Config\Win7\DependencyChecker.exe.config" ".\Setup\DependencyChecker\DependencyChecker.exe.config" /Y
start "DependencyChecker" /D"Setup\DependencyChecker" "Setup\DependencyChecker\DependencyChecker.exe"
goto exit

:win_8
DEL .\Setup\DependencyChecker\DependencyChecker.exe.config"
COPY ".\Setup\DependencyChecker\Config\Win8\DependencyChecker.exe.config" ".\Setup\DependencyChecker\DependencyChecker.exe.config" /Y
start "DependencyChecker" /D"Setup\DependencyChecker" "Setup\DependencyChecker\DependencyChecker.exe"
goto exit

:exit