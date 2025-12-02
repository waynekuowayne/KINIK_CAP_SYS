Imports Microsoft.Win32
Imports System.IO

Public Class TestForm
    ' ===== 設定區 =====
    ' 您可以根據實際需求修改這些參數
    Private Const TOTAL_IMAGES As Integer = 350        ' 總拍照張數
    Private Const UPDATE_INTERVAL As Integer = 10      ' 每幾張更新一次進度到 Registry
    Private Const LOG_PATH As String = "C:\Temp\CameraTestApp_Log.txt"

    ' ===== 全域變數 =====
    Private machineName As String = ""
    Private machineDisplayName As String = ""
    Private currentBatchNumber As String = ""
    Private isCapturing As Boolean = False

    ' ==========================================================================
    ' Form 載入事件
    ' ==========================================================================
    Private Sub TestForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        InitializeApplication()
    End Sub

    Private Sub InitializeApplication()
        Try
            ' 建立 Log 目錄
            Directory.CreateDirectory(Path.GetDirectoryName(LOG_PATH))
            WriteLog("========== 程式啟動 ==========")

            ' 從 Registry 讀取機台設定
            If Not LoadMachineConfig() Then
                ShowError("無法讀取機台設定！", "請先執行機台設定檔 (.reg)")
                btnStart.Enabled = False
                Return
            End If

            ' 顯示機台資訊
            UpdateStatusLabel($"機台：{machineDisplayName} ({machineName})", Color.Green)
            WriteLog($"✓ 機台設定載入成功：{machineName} - {machineDisplayName}")

            ' 初始化為閒置狀態
            RegistryHelper.SetIdle()

            ' ===== 在此初始化您的設備（相機、PLC 等）=====
            ' 例如：
            ' InitializeCamera()
            ' InitializePLC()
            ' ConnectToDevices()
            ' =================================================

        Catch ex As Exception
            WriteLog($"✗ 初始化錯誤: {ex.Message}")
            ShowError("初始化失敗！", ex.Message)
            btnStart.Enabled = False
        End Try
    End Sub

    ' ==========================================================================
    ' Form 關閉事件
    ' ==========================================================================
    Private Sub TestForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        Try
            ' 設定為閒置狀態
            RegistryHelper.SetIdle()
            WriteLog("✓ 程式正常關閉")

            ' ===== 在此釋放您的設備資源 =====
            ' 例如：
            ' Camera.Disconnect()
            ' PLC.Disconnect()
            ' =================================

        Catch ex As Exception
            WriteLog($"關閉時錯誤: {ex.Message}")
        End Try
    End Sub

    ' ==========================================================================
    ' 開始拍攝按鈕
    ' ==========================================================================
    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        ' 驗證輸入
        If String.IsNullOrWhiteSpace(txtBatchNumber.Text) Then
            MessageBox.Show("請輸入生產批號！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtBatchNumber.Focus()
            Return
        End If

        ' 防止重複點擊
        If isCapturing Then
            MessageBox.Show("正在拍攝中，請稍候...", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return
        End If

        ' 開始拍攝流程
        StartCaptureProcess()
    End Sub

    ' ==========================================================================
    ' 拍攝流程（主要邏輯）
    ' ==========================================================================
    Private Sub StartCaptureProcess()
        Try
            ' 準備工作
            currentBatchNumber = txtBatchNumber.Text.Trim()
            isCapturing = True
            btnStart.Enabled = False
            ProgressBar1.Value = 0
            lblProgress.Text = $"0 / {TOTAL_IMAGES}"

            WriteLog($"========== 開始新任務：{currentBatchNumber} ==========")

            ' ===== 步驟 1：建立任務（Registry） =====
            RegistryHelper.CreateTask(currentBatchNumber, TOTAL_IMAGES)
            UpdateStatusLabel("任務建立成功，準備拍攝...", Color.Blue)

            ' ===== 步驟 2：拍攝前準備 =====
            ' 在此加入您的拍攝前準備工作
            ' 例如：
            ' If Not PrepareCameraSettings() Then
            '     ShowError("相機準備失敗", "請檢查相機連線")
            '     ResetCaptureState()
            '     Return
            ' End If
            '
            ' If Not CheckPLCStatus() Then
            '     ShowError("PLC 狀態異常", "請檢查 PLC 連線")
            '     ResetCaptureState()
            '     Return
            ' End If

            ' ===== 步驟 3：開始拍攝迴圈 =====
            PerformCapture()

        Catch ex As Exception
            WriteLog($"✗ 拍攝流程錯誤: {ex.Message}")
            ShowError("拍攝失敗", ex.Message)
            ResetCaptureState()
        End Try
    End Sub

    ' ==========================================================================
    ' 執行拍攝（核心迴圈）
    ' ==========================================================================
    Private Sub PerformCapture()
        Dim successCount As Integer = 0
        Dim failCount As Integer = 0

        UpdateStatusLabel("拍攝中...", Color.Blue)

        For i As Integer = 1 To TOTAL_IMAGES
            Try
                ' ╔═══════════════════════════════════════════════════════════╗
                ' ║  在此放入您的實際拍照程式碼                                 ║
                ' ╚═══════════════════════════════════════════════════════════╝

                ' 範例 1：單張拍照流程
                ' -------------------
                ' Dim image As Bitmap = Camera.Capture()
                ' Dim fileName As String = GenerateFileName(currentBatchNumber, i)
                ' SaveImage(image, fileName)
                ' ProcessImage(image)  ' 影像處理、檢測等

                ' 範例 2：與 PLC 互動
                ' -------------------
                ' PLC.TriggerCapture()
                ' System.Threading.Thread.Sleep(100)  ' 等待 PLC 信號
                ' While Not PLC.IsCaptureReady()
                '     System.Threading.Thread.Sleep(10)
                ' End While
                ' Dim image As Bitmap = Camera.Capture()

                ' 範例 3：多相機拍照
                ' -------------------
                ' For Each cam In CameraList
                '     Dim img As Bitmap = cam.Capture()
                '     SaveImage(img, $"{currentBatchNumber}_{i}_{cam.ID}.jpg")
                ' Next

                ' === 模擬拍照（實際使用時請刪除）===
                System.Threading.Thread.Sleep(100)  ' 模擬拍照耗時
                successCount += 1

                ' ╔═══════════════════════════════════════════════════════════╗
                ' ║  拍照程式碼結束                                             ║
                ' ╚═══════════════════════════════════════════════════════════╝

                ' 更新本地 UI
                UpdateProgressUI(i)

                ' 定期更新 Registry（每 UPDATE_INTERVAL 張或最後一張）
                If i Mod UPDATE_INTERVAL = 0 OrElse i >= TOTAL_IMAGES Then
                    RegistryHelper.UpdateProgress(i, TOTAL_IMAGES)
                    WriteLog($"進度: {i}/{TOTAL_IMAGES}")
                End If

                ' 讓 UI 保持回應
                Application.DoEvents()

                ' 檢查是否需要中斷（可選）
                If Not isCapturing Then
                    WriteLog($"任務被中斷於第 {i} 張")
                    Exit For
                End If

            Catch ex As Exception
                ' 單張拍照失敗處理
                failCount += 1
                WriteLog($"✗ 第 {i} 張拍照失敗: {ex.Message}")

                ' 決定是否繼續
                ' Option 1: 繼續拍攝
                Continue For

                ' Option 2: 詢問使用者
                ' Dim result = MessageBox.Show($"第 {i} 張拍照失敗！{vbCrLf}是否繼續？", "錯誤", MessageBoxButtons.YesNo, MessageBoxIcon.Error)
                ' If result = DialogResult.No Then Exit For

                ' Option 3: 立即中止
                ' Throw
            End Try
        Next

        ' 拍攝完成
        OnCaptureCompleted(successCount, failCount)
    End Sub

    ' ==========================================================================
    ' 拍攝完成處理
    ' ==========================================================================
    Private Sub OnCaptureCompleted(successCount As Integer, failCount As Integer)
        Try
            ' 確保最後一次進度更新
            RegistryHelper.UpdateProgress(TOTAL_IMAGES, TOTAL_IMAGES)
            System.Threading.Thread.Sleep(1000)  ' 等待 Python 同步

            ' 設定為閒置狀態
            RegistryHelper.SetIdle()

            ' 更新 UI
            UpdateStatusLabel("拍攝完成！", Color.Green)
            ResetCaptureState()

            ' Log 記錄
            WriteLog($"========== 拍攝完成 ==========")
            WriteLog($"成功: {successCount} 張")
            WriteLog($"失敗: {failCount} 張")

            ' 顯示結果
            Dim message As String = $"拍攝完成！{vbCrLf}{vbCrLf}" &
                                   $"成功：{successCount} 張{vbCrLf}" &
                                   $"失敗：{failCount} 張"

            If failCount > 0 Then
                MessageBox.Show(message, "完成（有失敗）", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Else
                MessageBox.Show(message, "完成", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            ' ===== 拍攝後處理 =====
            ' 在此加入拍攝完成後的工作
            ' 例如：
            ' UploadImagesToServer()
            ' GenerateReport()
            ' SendNotification()
            ' =======================

        Catch ex As Exception
            WriteLog($"✗ 完成處理錯誤: {ex.Message}")
            ShowError("完成處理失敗", ex.Message)
        End Try
    End Sub

    ' ==========================================================================
    ' 輔助函數
    ' ==========================================================================

    ''' <summary>從 Registry 讀取機台設定</summary>
    Private Function LoadMachineConfig() As Boolean
        Try
            Dim key As RegistryKey = Registry.CurrentUser.OpenSubKey("Software\ZHAOI\VALUE\MachineConfig", False)
            If key Is Nothing Then
                WriteLog("Registry 機台設定不存在")
                Return False
            End If

            machineName = CStr(key.GetValue("MachineID", ""))
            machineDisplayName = CStr(key.GetValue("MachineName", ""))
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

    ''' <summary>更新進度 UI</summary>
    Private Sub UpdateProgressUI(currentCount As Integer)
        lblProgress.Text = $"{currentCount} / {TOTAL_IMAGES}"
        ProgressBar1.Value = CInt((currentCount / TOTAL_IMAGES) * 100)
    End Sub

    ''' <summary>更新狀態標籤</summary>
    Private Sub UpdateStatusLabel(text As String, color As Color)
        lblStatus.Text = text
        lblStatus.ForeColor = color
    End Sub

    ''' <summary>重置拍攝狀態</summary>
    Private Sub ResetCaptureState()
        isCapturing = False
        btnStart.Enabled = True
        txtBatchNumber.Clear()
        txtBatchNumber.Focus()
    End Sub

    ''' <summary>顯示錯誤訊息</summary>
    Private Sub ShowError(title As String, message As String)
        UpdateStatusLabel($"錯誤：{title}", Color.Red)
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    ''' <summary>寫入 Log</summary>
    Private Sub WriteLog(message As String)
        Try
            Dim timestamp As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
            Dim logMessage As String = $"[{timestamp}] {message}"

            File.AppendAllText(LOG_PATH, logMessage & Environment.NewLine)
            Console.WriteLine(logMessage)
        Catch ex As Exception
            Console.WriteLine($"Log 寫入失敗: {ex.Message}")
        End Try
    End Sub

    ' ==========================================================================
    ' 您可以在下方加入其他功能函數
    ' ==========================================================================

    ' 例如：
    ' Private Function InitializeCamera() As Boolean
    '     ' 初始化相機
    ' End Function
    '
    ' Private Function GenerateFileName(batchNumber As String, index As Integer) As String
    '     ' 產生檔案名稱
    ' End Function
    '
    ' Private Sub SaveImage(image As Bitmap, fileName As String)
    '     ' 儲存影像
    ' End Sub

End Class
