# Kinik CAP Machine System - 專案文檔

## 專案概述

基尼克CAP機台監控系統 - 用於監控多台相機檢測機台的生產狀態、任務進度和產能分析的完整監控解決方案。

**版本**: 1.0
**最後更新**: 2025-01
**公司**: 基尼克精密工業

---

## 專案架構

```
kinik_cap_machine_system/
├── backend/                    # 後端 API (FastAPI)
│   └── simple_api.py          # 主要 API 服務
├── frontend/                   # 前端 (React + Vite + TypeScript)
│   ├── src/
│   │   ├── components/        # React 元件
│   │   │   └── TaskProgress.tsx
│   │   ├── hooks/             # 自定義 Hooks
│   │   ├── pages/             # 頁面元件
│   │   │   └── UnifiedDashboard.tsx  # 主儀表板
│   │   ├── services/          # API 服務層
│   │   └── types/             # TypeScript 型別定義
│   └── package.json
├── registry_version/           # 機台端程式 (Windows Registry 同步)
│   ├── registry_sync.py       # Registry 同步主程式
│   ├── config.yaml            # 機台設定檔
│   └── 啟動機台同步.bat
├── 1_監控室部署包/            # 監控室部署檔案
│   ├── backend/
│   ├── frontend/
│   └── 啟動監控室系統.bat
├── 2_機台部署包/              # 機台部署檔案
│   └── 啟動機台同步.bat
└── PROJECT_INFO.md            # 本文檔

```

---

## 技術棧

### 後端
- **框架**: FastAPI 0.104+
- **語言**: Python 3.8+
- **資料庫**: MySQL 8.0
- **資料庫驅動**: PyMySQL
- **CORS**: 允許所有來源 (內網環境)

### 前端
- **框架**: React 18
- **建構工具**: Vite 4
- **語言**: TypeScript 4.9+
- **UI 框架**: Tailwind CSS 3
- **圖表庫**: Recharts 2.5+
- **Excel 匯出**: ExcelJS + file-saver
- **HTTP 客戶端**: Fetch API

### 機台端
- **語言**: Python 3.8+
- **Windows Registry**: winreg (標準庫)
- **設定格式**: YAML

---

## 資料庫連線資訊

### 連線設定
```python
DB_CONFIG = {
    'host': '122.100.99.161',
    'port': 43306,
    'user': 'A999',
    'password': '1023',
    'database': 'db_camera',
    'charset': 'utf8mb4'
}
```

### 資料表結構

#### `machines` 表
```sql
CREATE TABLE machines (
    id INT AUTO_INCREMENT PRIMARY KEY,
    machine_id VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(100),
    status VARCHAR(20),
    last_heartbeat DATETIME,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

#### `tasks` 表
```sql
CREATE TABLE tasks (
    id INT AUTO_INCREMENT PRIMARY KEY,
    machine_id INT,
    task_name VARCHAR(200),
    total_images INT,
    progress INT DEFAULT 0,
    status VARCHAR(20),  -- 'in_progress', 'completed', 'failed'
    start_time DATETIME,
    expected_completion_time DATETIME,
    actual_completion_time DATETIME,
    duration_minutes FLOAT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (machine_id) REFERENCES machines(id) ON DELETE CASCADE
);
```

---

## 主要功能清單

### 1. 監控室儀表板 (UnifiedDashboard)

#### 產出摘要卡片
- **今日產出**: 當日完成顆數、早晚班統計、稼動率
- **本周產出**: 本週完成顆數、早晚班統計、稼動率
- **本月產出**: 本月完成顆數、早晚班統計、稼動率

#### 機台監控
- 即時機台列表顯示
- 機台狀態 (運行中/閒置/離線)
- 當前任務進度條 (每 3 秒更新)
- 最後心跳時間

#### 統計圖表
- **每日生產趨勢** (折線圖): 最近 7 天的生產數量和總時長
- **產能分析** (長條圖): 最近 7 天的每日產量趨勢

#### 生產紀錄
- 顯示最近 10 筆記錄 (可載入更多,每次 +10 筆)
- 自動過濾失敗任務 (不顯示但保留在資料庫)
- 搜尋功能 (依任務名稱、狀態、時間)
- Excel 匯出功能

#### 管理功能
- 管理員密碼登入 (預設: admin123)
- 刪除機台功能 (級聯刪除所有相關任務)

### 2. 智能刷新機制

- **檢查頻率**: 每 10 秒
- **更新策略**: 只在資料真正變化時才重新渲染圖表
- **版本追蹤**: 使用資料 hash 比對 (總筆數 + 最新更新時間)
- **效能優化**:
  - 使用 `useMemo` 快取圖表資料
  - 避免不必要的重新計算和重繪

### 3. 機台端 Registry 同步

- **監控 Registry**: `HKEY_CURRENT_USER\Software\ZHAOI\VALUE\CurrentTask`
- **同步資料**:
  - TaskName (任務名稱)
  - TotalImages (總圖片數)
  - Progress (當前進度)
  - Status (任務狀態)
- **同步頻率**: 每 3 秒
- **心跳機制**: 每次同步更新 `last_heartbeat`

---

## 重要的設計決策

### 1. 資料寫入架構
**決策**: 機台端直接寫入資料庫,前端只讀取

**原因**:
- VB.NET 機台程式直接控制 Windows Registry
- Python 同步程式讀取 Registry 並寫入 MySQL
- 前端 API 純粹作為讀取層,簡化架構
- 避免前端和機台端的寫入衝突

### 2. 稼動率計算方式
**公式**: `稼動率 = (工作時數) / (24 × 天數 × 機台數) × 100%`

**重要修正** (2025-01):
- 原本使用 `COALESCE(actual_completion_time, NOW())` 會計入進行中任務
- 修改為只計算 `status = 'COMPLETED'` 的任務
- 確保稼動率反映實際完成的工作時間

### 3. 班別定義
- **早班**: 08:00 - 20:00 (12 小時)
- **晚班**: 20:00 - 次日 08:00 (12 小時)
- **判斷依據**: 任務的 `start_time` 的小時數

### 4. 失敗任務處理
**決策**: 隱藏而非刪除

**原因**:
- 保留完整歷史記錄供事後分析
- 前端過濾 `status = 'FAILED'` 任務
- 資料庫仍保存所有資料

### 5. 進度條更新機制
**決策**: 獨立於整體資料刷新

**頻率**: 每 3 秒
**原因**:
- 進度需要即時更新
- 不受整體頁面刷新影響
- 提供流暢的用戶體驗

### 6. 圖表閃爍問題解決
**問題**: 圖表每 10 秒重新計算導致閃爍

**解決方案**:
1. 使用 `useMemo` 快取所有圖表資料
2. 依賴陣列設定為實際資料變化指標
3. 移除 JSX 中的 IIFE (立即執行函數)
4. 實作智能刷新,只在有新資料時更新

---

## API 端點列表

### 機台管理
- `GET /api/v1/machines` - 取得所有機台
- `GET /api/v1/machines/{machine_id}` - 取得單一機台

### 任務管理
- `GET /api/v1/machines/{machine_id}/tasks` - 取得機台的所有任務
- `GET /api/v1/machines/{machine_id}/tasks?status={status}` - 依狀態篩選任務

### 統計資料
- `GET /api/v1/machines/{machine_id}/statistics` - 取得機台統計資料
- `GET /api/v1/production-summary` - 取得產出摘要 (日/周/月 + 班別)

### 智能刷新
- `GET /api/v1/data-version` - 取得資料版本 hash (用於智能刷新)

### 管理功能
- `POST /api/v1/admin/verify?password={pwd}` - 驗證管理員密碼
- `DELETE /api/v1/admin/machines/{machine_id}?password={pwd}` - 刪除機台

---

## 部署說明

### 監控室部署

1. **複製部署包**:
   ```
   複製 1_監控室部署包/ 到監控室電腦
   ```

2. **安裝 Python 相依套件**:
   ```bash
   cd backend
   pip install fastapi uvicorn pymysql python-multipart
   ```

3. **安裝 Node.js 相依套件**:
   ```bash
   cd frontend
   npm install
   ```

4. **啟動系統**:
   ```
   雙擊 啟動監控室系統.bat
   ```

5. **訪問網頁**:
   ```
   http://localhost:5173
   ```

### 機台端部署

1. **複製部署包**:
   ```
   複製 2_機台部署包/ 到每台機台電腦
   ```

2. **修改設定檔** (`config.yaml`):
   ```yaml
   machine_id: "CAM-001"  # 修改為實際機台 ID
   machine_name: "相機檢測機 #1"
   ```

3. **安裝 Python 相依套件**:
   ```bash
   pip install pymysql pyyaml
   ```

4. **啟動同步程式**:
   ```
   雙擊 啟動機台同步.bat
   ```

---

## 系統埠號

- **後端 API**: `8000`
- **前端網頁**: `5173`
- **MySQL 資料庫**: `43306`

---

## 管理員密碼

**預設密碼**: `admin123`

**修改位置**: `backend/simple_api.py` 第 23 行
```python
ADMIN_PASSWORD = "admin123"
```

---

## 常見問題 (FAQ)

### Q1: 前端顯示「無法連線」
**A**:
1. 確認後端 API 是否啟動 (http://localhost:8000)
2. 檢查防火牆設定
3. 確認資料庫連線正常

### Q2: 機台狀態顯示「離線」
**A**:
1. 檢查機台端同步程式是否運行
2. 確認 `config.yaml` 中的 machine_id 是否正確
3. 檢查資料庫連線

### Q3: 圖表一直閃爍
**A**:
已在最新版本修復,確保使用智能刷新機制的版本

### Q4: 進度條不更新
**A**:
1. 確認 VB.NET 程式有寫入 Registry
2. 檢查 Registry 路徑: `HKEY_CURRENT_USER\Software\ZHAOI\VALUE\CurrentTask`
3. 確認機台同步程式正在運行

---

## 版本歷史

### v1.0 (2025-01)
- ✅ 完整的監控室儀表板
- ✅ 機台端 Registry 同步
- ✅ 智能刷新機制
- ✅ 早晚班產出統計
- ✅ Excel 匯出功能
- ✅ 管理員刪除機台功能
- ✅ 修復圖表閃爍問題
- ✅ 修復稼動率計算邏輯
- ✅ 生產紀錄過濾失敗任務

---

## 聯絡資訊

**開發者**: Claude AI
**專案路徑**: `C:/github/kinik_cap_machine_system/`
**最後更新**: 2025-01-12

---

## 授權

內部使用專案 - 基尼克精密工業股份有限公司
