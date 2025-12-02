# VB.NET 機台監控程式 - Registry 版本

## 📂 資料夾結構

```
docs/
└── CameraTestApp_VS2017/       ← VB.NET 專案（VS2017）
    ├── CameraTestApp.sln       ← 雙擊開啟專案
    ├── RegistryHelper.vb       ← 核心檔案（只有 80 行）
    ├── TestForm.vb             ← 測試程式（示範用法）
    └── README.md               ← 詳細說明文件
```

---

## 🚀 快速開始

### 方法 1：測試範例程式

1. 開啟 `CameraTestApp_VS2017\CameraTestApp.sln`
2. 按 F5 執行
3. 觀察如何使用 RegistryHelper

### 方法 2：整合到您的主程式（推薦）

#### 只需要做 2 件事：

1. **加入 RegistryHelper.vb 到您的專案**
2. **在您的程式中加 3 行程式碼**

```vb
' 1. 開始拍照時
RegistryHelper.CreateTask(batchNumber, 350)

' 2. 進度更新時（在 FOR 迴圈內）
If i Mod 10 = 0 Then
    RegistryHelper.UpdateProgress(i, 350)
End If

' 3. 拍照完成時
RegistryHelper.SetIdle()
```

就這樣！**您的主程式其他部分完全不用動**。

---

## ✅ 優勢

| 項目 | 傳統方法 | Registry 方法 |
|------|---------|--------------|
| 需要修改的程式碼 | 200+ 行 | **3 行** |
| 需要資料庫連線 | 是 | **否** |
| 每台機台重新編譯 | 是 | **否** |
| 部署時間 | 15 分鐘 | **2 分鐘** |

---

## 📋 架構說明

```
您的 VB.NET 主程式
    ↓ 只寫 Registry（3 行程式碼）
Windows Registry
    ↓ Python 自動同步（不用管）
MySQL 資料庫
    ↓ 自動查詢（不用管）
前端網頁顯示
```

**您只需要專注在拍照邏輯，其他都自動處理！**

---

## 📖 詳細文件

請參閱：`CameraTestApp_VS2017\README.md`

---

## 版本

- **版本**: 3.0.0 (Registry 版本)
- **建立日期**: 2025-11-10
- **VS 版本**: Visual Studio 2017
- **框架**: .NET Framework 4.7.2
