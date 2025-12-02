Imports Microsoft.Win32
Imports System.IO

Public Class TestForm
    ' ===== Registry 路徑常數 =====
    Const REG_BASE_PATH As String = "Software\ZHAOI\VALUE"
    Const REG_MACHINE_CONFIG As String = "Software\ZHAOI\VALUE\MachineConfig"
    Const REG_CURRENT_TASK As String = "Software\ZHAOI\VALUE\CurrentTask"
    Const REG_HEARTBEAT As String = "Software\ZHAOI\VALUE\Heartbeat"

    ' ===== 全域變數 =====
    Dim machineName As String = ""
    Dim machineDisplayName As String = ""
    Dim machineIP As String = ""
    Dim currentTaskId As Integer = 0
    Dim totalImages As Integer = 350
    Dim logFilePath As String = "C:\Temp\CameraTestApp_Log.txt"

    ' ===== Form 載入事件 =====
    Private Sub TestForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' 建立 Log 檔案目錄
        Directory.CreateDirectory("C:\Temp")
        WriteLog("========== 程式啟動 ==========")

        Try
            ' 從 Registry 讀取機台設定
            If Not LoadMachineConfigFromRegistry() Then
                lblStatus.Text = "錯誤：無法讀取機台設定！請執行設定檔。"
                lblStatus.ForeColor = Color.Red
                btnStart.Enabled = False
                WriteLog("✗ 無法讀取機台設定")
                MessageBox.Show("請先執行機台設定檔 (.reg) 來設定機台編號和名稱！", "設定錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            lblStatus.Text = $"機台：{machineDisplayName} ({machineName})"
            lblStatus.ForeColor = Color.Green
            WriteLog($"✓ 機台設定載入成功：{machineName} - {machineDisplayName}")

            ' 初始化 Registry 狀態
            InitializeRegistryStatus()

            ' 啟動心跳 Timer (每 15 秒)
            TimerHeartbeat.Interval = 15000
            TimerHeartbeat.Enabled = True

        Catch ex As Exception
            WriteLog($"✗ 初始化錯誤: {ex.Message}")
            MessageBox.Show($"初始化失敗！{vbCrLf}{ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error)
            btnStart.Enabled = False
        End Try
    End Sub

    ' ===== Form 關閉事件 =====
    Private Sub TestForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            ' 更新機台狀態為閒置
            UpdateRegistryStatus("idle")
            WriteLog("✓ 程式正常關閉")
        Catch ex As Exception
            WriteLog($"關閉時錯誤: {ex.Message}")
        End Try
    End Sub

    ' ===== 心跳 Timer =====
    Private Sub TimerHeartbeat_Tick(sender As Object, e As EventArgs) Handles TimerHeartbeat.Tick
        UpdateHeartbeat()
    End Sub

    ' ===== 開始按鈕點擊事件 =====
    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        ' 檢查是否輸入生產批號
        If String.IsNullOrWhiteSpace(txtBatchNumber.Text) Then
            MessageBox.Show("請輸入生產批號！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtBatchNumber.Focus()
            Return
        End If

        ' 重置計數器
        currentTaskId = Environment.TickCount ' 使用時間戳作為暫時的 TaskID
        ProgressBar1.Value = 0
        lblProgress.Text = "0 / 350"

        ' 使用使用者輸入的生產批號
        Dim taskName As String = txtBatchNumber.Text.Trim()
        WriteLog($"========== 開始新任務：{taskName} ==========")

        ' 寫入 Registry
        If CreateNewTaskInRegistry(taskName) Then
            lblStatus.Text = "任務建立成功！開始拍攝..."
            lblStatus.ForeColor = Color.Blue
            btnStart.Enabled = False
            WriteLog($"任務建立成功！Task ID: {currentTaskId}")

            ' 更新機台狀態為工作中
            UpdateRegistryStatus("working")

            ' 模擬你的實際拍照流程
            StartMyPhotoProcess()
        Else
            lblStatus.Text = "任務建立失敗！"
            lblStatus.ForeColor = Color.Red
            WriteLog("任務建立失敗！")
        End If
    End Sub

    ' ===== 你的實際拍照流程（在這裡整合你的程式碼） =====
    Private Sub StartMyPhotoProcess()
        WriteLog("開始拍照流程")

        ' 這是你實際的 FOR 迴圈
        For i As Integer = 1 To 350
            ' ===== 在這裡放你的拍照程式碼 =====
            ' 例如：
            ' Camera.Capture()
            ' SaveImage(i)
            ' ... 你的其他程式碼 ...

            ' 模擬拍照延遲（實際使用時可以移除這行）
            System.Threading.Thread.Sleep(100)

            ' ===== 呼叫這個函數更新前端進度 =====
            UpdateProgress(i)

            ' 讓 UI 保持回應
            Application.DoEvents()
        Next

        ' 拍攝完成
        OnPhotoProcessCompleted()
    End Sub

    ' ===== 更新進度（在你的 FOR 迴圈中呼叫這個函數） =====
    Private Sub UpdateProgress(currentCount As Integer)
        ' 更新本地 UI
        lblProgress.Text = currentCount & " / " & totalImages
        ProgressBar1.Value = CInt((currentCount / totalImages) * 100)

        ' 每 10 張才更新一次 Registry（減少寫入次數）
        If currentCount Mod 10 = 0 OrElse currentCount >= totalImages Then
            WriteLog($"更新進度到 Registry: {currentCount}/{totalImages}")
            UpdateTaskProgressInRegistry(currentCount)
        End If
    End Sub

    ' ===== 拍攝完成 =====
    Private Sub OnPhotoProcessCompleted()
        ' 確保最後一次進度更新到 350 並標記為完成
        UpdateTaskProgressInRegistry(totalImages)

        ' 等待 1 秒確保 Python 有時間同步
        System.Threading.Thread.Sleep(1000)

        lblStatus.Text = "拍攝完成！"
        lblStatus.ForeColor = Color.Green
        btnStart.Enabled = True
        txtBatchNumber.Clear()

        ' 更新機台狀態為閒置
        UpdateRegistryStatus("idle")

        WriteLog($"========== 拍攝完成！Task ID: {currentTaskId} ==========")
        MessageBox.Show("拍攝完成！已完成 " & totalImages & " 張照片", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    ' ========================================
    ' ===== Registry 操作函數 =====
    ' ========================================

    ' ===== 從 Registry 讀取機台設定 =====
    Private Function LoadMachineConfigFromRegistry() As Boolean
        Try
            Dim key As RegistryKey = Registry.CurrentUser.OpenSubKey(REG_MACHINE_CONFIG, False)
            If key Is Nothing Then
                WriteLog("Registry 機台設定不存在")
                Return False
            End If

            machineName = CStr(key.GetValue("MachineID", ""))
            machineDisplayName = CStr(key.GetValue("MachineName", ""))
            machineIP = CStr(key.GetValue("MachineIP", GetLocalIPAddress()))

            key.Close()

            If String.IsNullOrEmpty(machineName) Then
                WriteLog("機台編號未設定")
                Return False
            End If

            If String.IsNullOrEmpty(machineDisplayName) Then
                machineDisplayName = machineName
            End If

            Return True
        Catch ex As Exception
            WriteLog($"讀取 Registry 錯誤: {ex.Message}")
            Return False
        End Try
    End Function

    ' ===== 初始化 Registry 狀態 =====
    Private Sub InitializeRegistryStatus()
        Try
            ' 建立或開啟 CurrentTask 鍵
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(REG_CURRENT_TASK)
            key.SetValue("TaskID", 0, RegistryValueKind.DWord)
            key.SetValue("TaskName", "")
            key.SetValue("TotalImages", totalImages, RegistryValueKind.DWord)
            key.SetValue("Progress", 0, RegistryValueKind.DWord)
            key.SetValue("Status", "idle")
            key.SetValue("StartTime", "")
            key.SetValue("LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            key.Close()

            WriteLog("Registry 狀態初始化完成")
        Catch ex As Exception
            WriteLog($"初始化 Registry 狀態錯誤: {ex.Message}")
        End Try
    End Sub

    ' ===== 更新心跳 =====
    Private Sub UpdateHeartbeat()
        Try
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(REG_HEARTBEAT)
            key.SetValue("LastHeartbeat", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            key.Close()
            WriteLog("心跳發送成功")
        Catch ex As Exception
            WriteLog($"心跳錯誤: {ex.Message}")
        End Try
    End Sub

    ' ===== 更新機台狀態 =====
    Private Sub UpdateRegistryStatus(status As String)
        Try
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(REG_CURRENT_TASK)
            key.SetValue("Status", status)
            key.SetValue("LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            key.Close()
            WriteLog($"機台狀態更新: {status}")
        Catch ex As Exception
            WriteLog($"更新機台狀態錯誤: {ex.Message}")
        End Try
    End Sub

    ' ===== 建立新任務 =====
    Private Function CreateNewTaskInRegistry(taskName As String) As Boolean
        Try
            WriteLog($"建立新任務: 任務名稱={taskName}, 總數={totalImages}")

            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(REG_CURRENT_TASK)
            key.SetValue("TaskID", currentTaskId, RegistryValueKind.DWord)
            key.SetValue("TaskName", taskName)
            key.SetValue("TotalImages", totalImages, RegistryValueKind.DWord)
            key.SetValue("Progress", 0, RegistryValueKind.DWord)
            key.SetValue("Status", "in_progress")
            key.SetValue("StartTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            key.SetValue("LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            key.Close()

            WriteLog($"任務建立成功，Task ID: {currentTaskId}")
            Return True

        Catch ex As Exception
            WriteLog($"建立任務錯誤: {ex.Message}")
            Return False
        End Try
    End Function

    ' ===== 更新任務進度 =====
    Private Sub UpdateTaskProgressInRegistry(currentCount As Integer)
        Try
            Dim status As String = If(currentCount >= totalImages, "completed", "in_progress")

            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(REG_CURRENT_TASK)
            key.SetValue("Progress", currentCount, RegistryValueKind.DWord)
            key.SetValue("Status", status)
            key.SetValue("LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            key.Close()

            WriteLog($"✓ 進度更新成功: {currentCount}/{totalImages}")

        Catch ex As Exception
            WriteLog($"更新進度錯誤 ({currentCount}): {ex.Message}")
        End Try
    End Sub

    ' ===== 自動取得本機 IP 位址 =====
    Private Function GetLocalIPAddress() As String
        Try
            Dim host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
            For Each ip In host.AddressList
                If ip.AddressFamily = System.Net.Sockets.AddressFamily.InterNetwork Then
                    Return ip.ToString()
                End If
            Next
            Return "127.0.0.1"  ' 如果找不到，使用 localhost
        Catch ex As Exception
            Return "Unknown"
        End Try
    End Function

    ' ===== Log 寫入函數 =====
    Private Sub WriteLog(message As String)
        Try
            Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            Dim logMessage As String = $"[{timestamp}] {message}"

            ' 寫入檔案
            File.AppendAllText(logFilePath, logMessage & Environment.NewLine)

            ' 也輸出到 Console
            Console.WriteLine(logMessage)
        Catch ex As Exception
            Console.WriteLine("Log 寫入失敗: " & ex.Message)
        End Try
    End Sub

End Class
