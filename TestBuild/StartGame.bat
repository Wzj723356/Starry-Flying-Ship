@echo off
REM 星际战舰 - 快速启动脚本
REM Star Ship - Quick Launch Script

setlocal

echo ========================================
echo 星际战舰 - 快速启动
echo Star Ship - Quick Launch
echo ========================================
echo.

set GAME_DIR=%~dp0
set EXE_NAME=StarryFlyingShip.exe
set CONFIG_DIR=%GAME_DIR%Configs

if not exist "%GAME_DIR%%EXE_NAME%" (
    echo [错误] 找不到游戏可执行文件: %EXE_NAME%
    echo.
    echo 请先运行 BuildScript.bat 构建游戏
    echo Please run BuildScript.bat first to build the game
    echo.
    pause
    exit /b 1
)

echo 正在启动游戏...
echo 游戏目录: %GAME_DIR%
echo 配置文件: %CONFIG_DIR%
echo.

cd /d "%GAME_DIR%"

start "" "%EXE_NAME%" -configdir "%CONFIG_DIR%" -logfile "Logs\game.log" -force-d3d12

echo 游戏已启动
echo.
echo 快捷键:
echo   F1 - 切换调试信息
echo   F2 - 截图
echo   F3 - 重置视角
echo   F4 - 切换HUD
echo   ESC - 暂停菜单
echo   ~  - 控制台
echo.
echo 按任意键退出此窗口...
pause >nul

endlocal
