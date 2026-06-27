@echo off
chcp 65001 >nul
echo ==================================
echo   Industrial Toolkit 一键部署
echo ==================================
echo.

echo [1/4] 编译项目...
cd /d "C:\Users\WuRongXin\Desktop\Industrial-Toolkit"
if exist publish\backend rmdir /s /q publish\backend
dotnet publish "backend\IndustrialToolkit\IndustrialToolkit.Api\IndustrialToolkit.Api.csproj" -c Release -o "publish\backend"
if errorlevel 1 (
    echo 编译失败！
    pause
    exit /b 1
)
echo 编译成功！
echo.

echo [2/4] 复制前端文件...
xcopy "frontend\*" "publish\backend\wwwroot\" /s /e /y /q
echo 前端文件复制完成！
echo.

echo [3/4] 上传到服务器...
if exist "\\TOOLKIT\website\backend.zip" del /f /q "\\TOOLKIT\website\backend.zip"
powershell -Command "Compress-Archive -Path 'publish\backend\*' -DestinationPath 'publish\backend.zip' -Force"
copy /y "publish\backend.zip" "\\TOOLKIT\website\backend.zip"
echo 上传完成！
echo.

echo [4/4] 服务器上手动执行 update.bat 更新
echo.
echo ==================================
echo   部署完成！
echo   请在服务器上运行 C:\website\update.bat
echo ==================================
pause
