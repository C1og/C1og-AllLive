# ================= 配置区域 =================
$workDir = "D:\D-Software\C1og-AllLive"
$uwpProcessName = "AllLive.uwp" # UWP 进程名
# ===========================================

if (Test-Path $workDir) {
    Push-Location $workDir

    Write-Host "正在静默启动服务并监听 $uwpProcessName..." -ForegroundColor Cyan

    # 1. 启动 dotnet 服务
    # 使用 -WindowStyle Hidden 隐藏窗口
    # 使用 -PassThru 获取进程对象
    $serviceProc = Start-Process dotnet -ArgumentList "run --project AllLive.SignService --no-build" -PassThru -WindowStyle Hidden

    # 检查进程是否成功启动
    if ($null -eq $serviceProc) {
        Write-Error "服务启动失败！"
        exit
    }

    # 2. 性能最优的等待方案
    # 给 UWP 程序一点启动时间，防止脚本运行太快没抓到进程
    Start-Sleep -Seconds 2 

    try {
        # 持续等待直到 UWP 进程退出 (0% CPU 占用)
        # 如果 UWP 没运行，这里会直接进入 catch
        Wait-Process -Name $uwpProcessName -ErrorAction Stop
        Write-Host "检测到主程序已退出。" -ForegroundColor Yellow
    }
    catch {
        Write-Warning "未找到正在运行的 $uwpProcessName，服务即将关闭。"
    }

    # 3. 释放资源
    # 强制结束服务进程及其所有子进程
    if ($null -ne $serviceProc) {
        Stop-Process -Id $serviceProc.Id -Force -ErrorAction SilentlyContinue
    }
    
    Pop-Location
    Write-Host "已安全退出。" -ForegroundColor Green
    # 这里的退出是为了关闭当前的脚本黑窗口
    exit
}
else {
    Write-Error "路径未找到: $workDir"
    pause
}