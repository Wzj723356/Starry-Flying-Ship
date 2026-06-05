@echo off
REM 星际战舰 - 测试构建脚本
REM Star Ship - Test Build Script

echo ========================================
echo 星际战舰 - 构建工具 v1.0
echo Star Ship - Build Tool v1.0
echo ========================================
echo.

set BUILD_MODE=%1
if "%BUILD_MODE%"=="" set BUILD_MODE=Development

set UNITY_PATH=C:\Program Files\Unity\Hub\Editor\2022.3.23f1\Editor\Unity.exe
set PROJECT_PATH=%~dp0..
set BUILD_PATH=%~dp0Build
set LOG_PATH=%~dp0Logs

echo 项目路径: %PROJECT_PATH%
echo 构建路径: %BUILD_PATH%
echo 构建模式: %BUILD_MODE%
echo.

if not exist "%BUILD_PATH%" (
    mkdir "%BUILD_PATH%"
)

if not exist "%LOG_PATH%" (
    mkdir "%LOG_PATH%"
)

echo [1/3] 验证项目结构...
if not exist "%PROJECT_PATH%\Assets" (
    echo [错误] 项目 Assets 文件夹不存在
    exit /b 1
)

if not exist "%PROJECT_PATH%\Packages\manifest.json" (
    echo [错误] 项目 Packages 文件夹不存在
    exit /b 1
)

echo [完成] 项目结构验证通过
echo.

echo [2/3] 编译项目脚本...
echo 注意: 请使用 Unity Editor 打开项目进行编译
echo 或者使用 Unity BatchMode 进行命令行构建
echo.

echo [3/3] 生成可执行文件配置...
echo.

set EXE_NAME=StarryFlyingShip_%BUILD_MODE%.exe
set CONFIG_FILE=%BUILD_PATH%\Configs

if not exist "%CONFIG_FILE%" (
    mkdir "%CONFIG_FILE%"
)

copy "%~dp0Configs\*.ini" "%CONFIG_FILE%\" >nul 2>&1
copy "%~dp0Configs\*.json" "%CONFIG_FILE%\" >nul 2>&1

echo 构建配置已复制到: %CONFIG_FILE%
echo.

echo ========================================
echo 构建准备完成
echo ========================================
echo.
echo 后续步骤:
echo 1. 使用 Unity Editor 打开项目
echo 2. 选择 Build Settings
echo 3. 选择 %BUILD_MODE% 模式
echo 4. 点击 Build 选择输出目录
echo.
echo 或者使用命令行:
echo "%UNITY_PATH%" -quit -batchmode -projectPath "%PROJECT_PATH%" -buildWindows64Player "%BUILD_PATH%\%EXE_NAME%"
echo.
pause
