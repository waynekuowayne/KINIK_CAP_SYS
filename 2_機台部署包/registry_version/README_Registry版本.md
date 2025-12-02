# Registry 版本 - 快速部署方案

## 🎯 新架構優勢

### 傳統方式（舊版）
❌ 每台機台都要修改 VB.NET 程式碼中的機台編號
❌ 需要重新編譯不同版本的 exe
❌ 部署麻煩，容易出錯

### Registry 方式（新版）
✅ **所有機台使用同一個 exe 檔案**
✅ 機台設定存在 Registry 中，雙擊 .reg 檔即可設定
✅ VB.NET 不直接連資料庫，只寫 Registry
✅ Python 程式負責同步 Registry 到資料庫
✅ 部署超快速，不用編譯

---

## 📋 新架構流程

```
VB.NET 主程式
    ↓ 寫入
Windows Registry (HKEY_CURRENT_USER\SOFTWARE\CameraTestApp)
    ↓ 每 3 秒讀取
Python 同步程式 (registry_sync.py)
    ↓ 上傳
MySQL 資料庫
    ↓ 查詢
前端網頁
```

---

## 🚀 快速部署步驟（5 台機台）

### 步驟 1：準備統一的檔案包

將以下檔案複製到一個資料夾（例如 `部署包`）：

```
部署包/
├── CameraTestApp.exe          # VB.NET 程式（所有機台都一樣）
├── registry_sync.py           # Python 同步程式
├── 啟動Python同步程式.bat      # 啟動腳本
├── 機台設定範本.reg            # 完整設定範本
└── 各機台設定/
    ├── CAM-001.reg
    ├── CAM-002.reg
    ├── CAM-003.reg
    ├── CAM-004.reg
    └── CAM-005.reg
```

### 步驟 2：在每台機台上部署

#### 機台 1 (CAM-001)
1. 複製整個「部署包」資料夾到機台
2. **雙擊執行**：`各機台設定\CAM-001.reg` → 確定匯入
3. **雙擊執行**：`啟動Python同步程式.bat`
4. **執行**：`CameraTestApp.exe`
5. 完成！

#### 機台 2 (CAM-002)
1. 複製整個「部署包」資料夾到機台
2. **雙擊執行**：`各機台設定\CAM-002.reg` → 確定匯入
3. **雙擊執行**：`啟動Python同步程式.bat`
4. **執行**：`CameraTestApp.exe`
5. 完成！

#### 機台 3-5
依此類推，只需要匯入對應的 .reg 檔案即可！

---

## 📂 Registry 結構

```
HKEY_CURRENT_USER\Software\ZHAOI\VALUE\
│
├── MachineConfig\              # 機台設定（手動設定）
│   ├── MachineID = "CAM-001"
│   ├── MachineName = "拍照機A"
│   └── MachineIP = ""
│
├── CurrentTask\                # 當前任務（VB.NET 寫入，Python 讀取）
│   ├── TaskID = 123
│   ├── TaskName = "BATCH-001"
│   ├── TotalImages = 350
│   ├── Progress = 150
│   ├── Status = "working"
│   ├── StartTime = "2025-11-10 14:30:00"
│   └── LastUpdate = "2025-11-10 14:35:00"
│
├── Heartbeat\                  # 心跳（VB.NET 寫入）
│   └── LastHeartbeat = "2025-11-10 14:35:30"
│
└── DatabaseConfig\             # 資料庫設定（可選）
    ├── Server = "122.100.99.161"
    ├── Port = 43306
    ├── Database = "db_camera"
    ├── Username = "A999"
    └── Password = "1023"
```

---

## 🔧 使用說明

### 1. 首次設定機台

#### 方法 A：使用提供的 .reg 檔案
```
雙擊：各機台設定\CAM-001.reg
點選「是」確認匯入
```

#### 方法 B：手動修改範本
1. 複製 `機台設定範本.reg`
2. 用記事本開啟
3. 修改以下內容：
   ```
   "MachineID"="CAM-006"        ← 改成你的機台編號
   "MachineName"="拍照機F"      ← 改成你的機台名稱
   ```
4. 儲存
5. 雙擊執行

### 2. 啟動 Python 同步程式

#### 方法 A：使用批次檔（推薦）
```
雙擊：啟動Python同步程式.bat
```

#### 方法 B：命令列執行
```batch
cd C:\部署包
python registry_sync.py
```

#### 方法 C：設定為開機自動執行
1. 按 Win+R，輸入 `shell:startup`
2. 建立「啟動Python同步程式.bat」的捷徑到這個資料夾

### 3. 執行 VB.NET 程式

```
執行：CameraTestApp.exe
```

程式會自動從 Registry 讀取機台設定，不需要修改程式碼！

---

## ✅ 驗證測試

### 測試 Registry 是否正確設定

1. 按 Win+R，輸入 `regedit`
2. 瀏覽到：`HKEY_CURRENT_USER\Software\ZHAOI\VALUE\MachineConfig`
3. 確認有以下值：
   - MachineID = "CAM-001"（或你的機台編號）
   - MachineName = "拍照機A"（或你的機台名稱）

### 測試 Python 同步程式

1. 執行 `啟動Python同步程式.bat`
2. 應該看到：
   ```
   ========== Registry Sync Service 啟動 ==========
   ✓ 從 Registry 載入資料庫設定: 122.100.99.161:43306/db_camera
   ✓ 資料庫連線成功
   ✓ 機台註冊成功，DB ID: 1
   ```
3. 檢查 Log 檔案：`C:\Temp\RegistrySync_Log.txt`

### 測試 VB.NET 程式

1. 執行 `CameraTestApp.exe`
2. 應該顯示：機台：拍照機A (CAM-001)
3. 輸入生產批號，按「開始拍攝」
4. 檢查 Python 同步程式視窗，應該看到：
   ```
   ✓ 建立新任務，Task ID: 17
   ✓ 心跳發送 (循環: 5)
   ```

### 測試前端網頁

1. 開啟瀏覽器：`http://localhost:5173`
2. 應該看到機台卡片
3. 進度應該即時更新

---

## 🎯 優勢總結

### 部署速度
- 傳統方式：每台機台 15 分鐘（修改程式碼、編譯、測試）
- Registry 方式：每台機台 **2 分鐘**（複製檔案、匯入 .reg、執行）

### 維護成本
- 傳統方式：修改程式碼 → 所有機台都要重新部署
- Registry 方式：修改 .reg 檔 → **雙擊即可**

### 錯誤率
- 傳統方式：容易改錯機台編號、忘記編譯
- Registry 方式：**統一 exe，設定檔明確**

---

## ⚠️ 注意事項

### 1. Python 環境
每台機台需要安裝 Python 和 pymysql：
```batch
python -m pip install pymysql
```

### 2. 權限問題
- Registry 使用 `HKEY_CURRENT_USER`，不需要管理員權限
- VB.NET 和 Python 程式都以一般使用者執行即可

### 3. 同步程式必須執行
- VB.NET 只寫 Registry，不會直接寫資料庫
- **必須確保 Python 同步程式在執行**
- 建議設定為開機自動啟動

### 4. 網路中斷處理
- Python 程式會自動重試連線
- 前端會顯示「斷線中」
- Registry 資料不會遺失，重新連線後會同步

---

## 📞 故障排除

### 問題 1：VB.NET 顯示「無法讀取機台設定」
**原因**：尚未匯入 .reg 檔案
**解決**：雙擊執行對應的 .reg 檔案（例如 CAM-001.reg）

### 問題 2：Python 程式顯示「資料庫連線失敗」
**原因**：網路無法連到資料庫或設定錯誤
**解決**：
1. 檢查網路：`ping 122.100.99.161`
2. 檢查防火牆 Port 43306
3. 確認 Registry 中的 DatabaseConfig 設定正確

### 問題 3：前端看不到機台
**原因**：Python 同步程式未執行
**解決**：執行 `啟動Python同步程式.bat`

### 問題 4：進度不更新
**原因**：Python 同步程式掛掉或資料庫連線中斷
**解決**：
1. 檢查 Python 程式是否還在執行
2. 查看 `C:\Temp\RegistrySync_Log.txt` Log 檔
3. 重新啟動 Python 同步程式

---

## 📝 檔案清單

### 必要檔案
- ✅ `CameraTestApp.exe` - VB.NET 主程式
- ✅ `registry_sync.py` - Python 同步程式
- ✅ `啟動Python同步程式.bat` - 啟動腳本

### 設定檔案
- ✅ `機台設定範本.reg` - 完整設定範本
- ✅ `各機台設定/CAM-001.reg` - 機台 1 設定
- ✅ `各機台設定/CAM-002.reg` - 機台 2 設定
- ✅ `各機台設定/CAM-003.reg` - 機台 3 設定
- ✅ `各機台設定/CAM-004.reg` - 機台 4 設定
- ✅ `各機台設定/CAM-005.reg` - 機台 5 設定

### Log 檔案
- `C:\Temp\CameraTestApp_Log.txt` - VB.NET 程式 Log
- `C:\Temp\RegistrySync_Log.txt` - Python 同步程式 Log

---

**版本**：Registry 版本 v1.0
**建立日期**：2025-11-10
**適用於**：快速部署多台機台
