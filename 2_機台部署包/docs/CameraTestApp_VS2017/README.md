# CameraTestApp - Registry 版本

## 🎯 專案特色

這是一個**超簡潔**的 VB.NET 拍照機台監控程式，使用 Registry 架構。

### ✅ 核心優勢

- **只加 3 行程式碼**：整合到您現有的主程式只需要 3 行
- **統一 exe**：所有機台使用同一個執行檔
- **快速部署**：每台機台 2 分鐘完成部署
- **不需資料庫連線**：VB.NET 只寫 Registry，由 Python 背景程式同步到資料庫

---

## 📂 專案結構

```
CameraTestApp_VS2017/
├── CameraTestApp.sln           ← Visual Studio 2017 解決方案檔
├── CameraTestApp.vbproj        ← 專案檔
├── RegistryHelper.vb           ← 核心：Registry 輔助類別（只有 80 行）
├── TestForm.vb                 ← 主程式（示範如何使用 RegistryHelper）
├── TestForm.Designer.vb        ← Form 設計器
├── TestForm.resx               ← Form 資源檔
├── App.config                  ← 設定檔
└── My Project/                 ← VB.NET 專案檔案
    ├── AssemblyInfo.vb
    ├── Application.myapp
    ├── Application.Designer.vb
    ├── Resources.resx
    └── Resources.Designer.vb
```

---

## 🚀 如何使用

### 方法 1：直接使用這個測試程式

1. 用 Visual Studio 2017 開啟 `CameraTestApp.sln`
2. 按 F5 執行
3. 輸入生產批號，按「開始拍攝」
4. 觀察進度即時同步到前端

### 方法 2：整合到您現有的主程式（推薦）

#### 步驟 1：加入 RegistryHelper.vb

在您的專案中：
- 方案總管 → 右鍵「加入」→「現有項目」
- 選擇 `RegistryHelper.vb`

#### 步驟 2：修改您的拍照程式碼

只需要在 3 個位置各加 1 行：

```vb
' 在您的拍照按鈕事件中：
Private Sub btnStart_Click(...) Handles btnStart.Click
    Dim batchNumber As String = txtBatchNumber.Text

    ' ===== 加這 1 行 =====
    RegistryHelper.CreateTask(batchNumber, 350)

    ' 您原本的 FOR 迴圈
    For i = 1 To 350
        ' 您原本的拍照程式碼
        YourCamera.Capture()
        YourSaveImage(i)

        ' ===== 每 10 張加這 1 行 =====
        If i Mod 10 = 0 Then
            RegistryHelper.UpdateProgress(i, 350)
        End If
    Next

    MessageBox.Show("完成！")

    ' ===== 加這 1 行 =====
    RegistryHelper.SetIdle()
End Sub
```

#### 步驟 3：編譯

F6 編譯 → 產生的 exe 就可以用在所有機台！

---

## 📋 RegistryHelper API 說明

### CreateTask(taskName, totalImages)
建立新任務
- `taskName`: 任務名稱（生產批號）
- `totalImages`: 總張數

### UpdateProgress(currentCount, totalImages)
更新進度
- `currentCount`: 當前張數
- `totalImages`: 總張數

### CompleteTask()
標記任務完成

### SetIdle()
設定機台為閒置狀態

---

## 🔧 編譯要求

- Visual Studio 2017
- .NET Framework 4.7.2
- Windows 7 或更新版本

---

## 📦 部署流程

### 編譯後的檔案

編譯完成後，到 `bin\Release\` 資料夾，您會找到：
- `CameraTestApp.exe` ← 這就是要部署到機台的檔案

### 在機台上部署

1. 複製 `CameraTestApp.exe` 到機台
2. 雙擊執行對應的 `.reg` 檔案（例如：CAM-001.reg）
3. 啟動 Python 同步程式：`啟動Python同步程式.bat`
4. 執行 `CameraTestApp.exe`
5. 完成！

---

## 🎯 與舊版本的差異

| 項目 | 舊版本（直接連資料庫） | 新版本（Registry） |
|------|---------------------|-------------------|
| 程式碼改動 | 200+ 行 | **3 行** |
| 需要資料庫知識 | 是 | **否** |
| 網路斷線影響主程式 | 是 | **否** |
| 每台機台重新編譯 | 是 | **否** |
| 部署時間 | 15 分鐘 | **2 分鐘** |

---

## ⚠️ 重要提醒

1. **機台必須先匯入 .reg 檔案**
   - 路徑：`HKEY_CURRENT_USER\Software\ZHAOI\VALUE\MachineConfig`
   - 包含：MachineID, MachineName

2. **Python 同步程式必須執行**
   - VB.NET 只寫 Registry
   - Python 負責同步到資料庫
   - 如果 Python 沒執行，前端看不到資料

3. **網路連線**
   - VB.NET 不需要網路
   - Python 需要連到資料庫：`122.100.99.161:43306`

---

## 📝 Log 檔案

- VB.NET Log: `C:\Temp\CameraTestApp_Log.txt`
- Python Log: `C:\Temp\RegistrySync_Log.txt`

---

## 版本資訊

- **版本**: 3.0.0 (Registry 版本)
- **建立日期**: 2025-11-10
- **相容性**: Visual Studio 2017, .NET Framework 4.7.2
