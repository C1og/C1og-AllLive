# ================= 配置区域 =================
$workDir = "D:\D-Software\source\C1og-AllLive"
$uwpProcessName = "AllLive.UWP"
$uwpAppId = "5421.24244EC421563_a5x6jjv384fej!App" 
# 强制控制台使用 UTF8，防止日志乱码
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
# ===========================================

# 定义 Win32 API
$code = @"
    [DllImport("user32.dll")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")]
    public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
"@
try {
    Add-Type -MemberDefinition $code -Name "Win32SetForegroundWindow" -Namespace Win32Functions -ErrorAction SilentlyContinue
} catch {}

# 1. 初始化变量
$serviceProc = $null
$uwpProc = Get-Process -Name $uwpProcessName -ErrorAction SilentlyContinue

try {
    if ($uwpProc) {
        Write-Host "[系统] 程序已在运行，正在切换窗口..." -ForegroundColor Cyan
        $handle = $uwpProc.MainWindowHandle
        if ($handle -ne [IntPtr]::Zero) {
            [Win32Functions.Win32SetForegroundWindow]::ShowWindow($handle, 9)
            [Win32Functions.Win32SetForegroundWindow]::SetForegroundWindow($handle)
        }
    } else {
        Write-Host "[系统] 正在启动服务与主程序..." -ForegroundColor Green
        
        # 启动后台服务 (dotnet) 并将日志输出到当前窗口
        if (Test-Path $workDir) {
            Push-Location $workDir
            # --- 关键修改：使用 -NoNewWindow 展示日志 ---
            $serviceProc = Start-Process dotnet -ArgumentList "run --project AllLive.SignService --no-build" -NoNewWindow -PassThru
            Pop-Location
        }

        # 启动 UWP
        Start-Process "shell:AppsFolder\$uwpAppId"
    }

    # 2. 等待 UWP 进程出现
    $retry = 0
    while (-not $uwpProc -and $retry -lt 40) {
        Start-Sleep -Milliseconds 500
        $uwpProc = Get-Process -Name $uwpProcessName -ErrorAction SilentlyContinue
        $retry++
    }

    # 3. 监控状态
      if ($uwpProc) {
        Write-Host "[系统] 正在监控。程序退出后将自动关闭 dotnet 服务。" -ForegroundColor Green
        Write-Host "---------------- dotnet 日志输出 ----------------" -ForegroundColor Gray
        
        # 阻塞式等待
        $uwpProc.WaitForExit()
    }
}
catch {
    Write-Error "脚本运行出错: $_"
}
finally {
    Write-Host "`n[系统] 检测到主程序关闭，正在清理并退出..." -ForegroundColor Yellow
    
    # 停止 service 进程
    if ($serviceProc -and -not $serviceProc.HasExited) {
        Stop-Process -Id $serviceProc.Id -Force -ErrorAction SilentlyContinue
    }

    # 清理残余 dotnet 进程
    Get-CimInstance Win32_Process -Filter "Name LIKE 'dotnet%'" | 
        Where-Object { $_.CommandLine -like "*AllLive.SignService*" } | 
        ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }

    Write-Host "[系统] 清理完成，窗口即将关闭。" -ForegroundColor White
    Start-Sleep -Seconds 0 # 给用户 1 秒时间看最后一眼日志
    
    # 彻底退出
    [System.Environment]::Exit(0)
}

