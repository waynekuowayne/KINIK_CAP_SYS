Imports Microsoft.Win32

''' <summary>
''' Registry 輔助類別 - 用於寫入機台狀態到 Registry
''' 使用方法：在您的主程式中只需呼叫這個類別的靜態方法即可
''' </summary>
Public Class RegistryHelper
    ' Registry 路徑
    Private Const REG_CURRENT_TASK As String = "Software\ZHAOI\VALUE\CurrentTask"

    ''' <summary>
    ''' 建立新任務
    ''' </summary>
    ''' <param name="taskName">任務名稱（生產批號）</param>
    ''' <param name="totalImages">總張數</param>
    Public Shared Sub CreateTask(taskName As String, totalImages As Integer)
        Try
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(REG_CURRENT_TASK)
            key.SetValue("TaskName", taskName)
            key.SetValue("TotalImages", totalImages, RegistryValueKind.DWord)
            key.SetValue("Progress", 0, RegistryValueKind.DWord)
            key.SetValue("Status", "in_progress")
            key.SetValue("StartTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            key.SetValue("LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            key.Close()
        Catch ex As Exception
            ' 寫入失敗也不影響主程式運作
            Console.WriteLine("Registry 寫入失敗: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' 更新進度
    ''' </summary>
    ''' <param name="currentCount">當前張數</param>
    ''' <param name="totalImages">總張數</param>
    Public Shared Sub UpdateProgress(currentCount As Integer, totalImages As Integer)
        Try
            Dim status As String = If(currentCount >= totalImages, "completed", "in_progress")

            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(REG_CURRENT_TASK)
            key.SetValue("Progress", currentCount, RegistryValueKind.DWord)
            key.SetValue("Status", status)
            key.SetValue("LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            key.Close()
        Catch ex As Exception
            ' 寫入失敗也不影響主程式運作
            Console.WriteLine("Registry 更新失敗: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' 標記任務完成
    ''' </summary>
    Public Shared Sub CompleteTask()
        Try
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(REG_CURRENT_TASK)
            key.SetValue("Status", "completed")
            key.SetValue("LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            key.Close()
        Catch ex As Exception
            Console.WriteLine("Registry 更新失敗: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' 設定機台為閒置狀態
    ''' </summary>
    Public Shared Sub SetIdle()
        Try
            Dim key As RegistryKey = Registry.CurrentUser.CreateSubKey(REG_CURRENT_TASK)
            key.SetValue("Status", "idle")
            key.SetValue("TaskName", "")
            key.SetValue("Progress", 0, RegistryValueKind.DWord)
            key.SetValue("LastUpdate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
            key.Close()
        Catch ex As Exception
            Console.WriteLine("Registry 更新失敗: " & ex.Message)
        End Try
    End Sub
End Class
