#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Registry to Database Sync Service
將 Windows Registry 中的機台資料同步到 MySQL 資料庫

使用方法：
    python registry_sync.py

建議設定為開機自動執行或排程任務
"""

import winreg
import pymysql
import time
import sys
import os
from datetime import datetime

# 設定 Windows 終端機使用 UTF-8 編碼
if sys.platform == 'win32':
    os.system('chcp 65001 >nul 2>&1')
    sys.stdout.reconfigure(encoding='utf-8', errors='replace')
    sys.stderr.reconfigure(encoding='utf-8', errors='replace')

# ========================================
# 設定區
# ========================================

# Registry 路徑
REG_BASE = r"Software\ZHAOI\VALUE"
REG_MACHINE_CONFIG = r"Software\ZHAOI\VALUE\MachineConfig"
REG_CURRENT_TASK = r"Software\ZHAOI\VALUE\CurrentTask"
REG_HEARTBEAT = r"Software\ZHAOI\VALUE\Heartbeat"
REG_DB_CONFIG = r"Software\ZHAOI\VALUE\DatabaseConfig"

# 資料庫連線設定（預設值，優先從 Registry 讀取）
DB_CONFIG = {
    'host': '122.100.99.161',
    'port': 43306,
    'user': 'A999',
    'password': '1023',
    'database': 'db_camera',
    'charset': 'utf8mb4'
}

# 同步間隔（秒）
SYNC_INTERVAL = 3

# Log 檔案
LOG_FILE = r"C:\Temp\RegistrySync_Log.txt"

# ========================================
# 全域變數
# ========================================

db_connection = None
machine_db_id = 0
current_task_db_id = 0

# ========================================
# 工具函數
# ========================================

def write_log(message):
    """寫入 Log"""
    timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S.%f")[:-3]
    log_message = f"[{timestamp}] {message}"
    print(log_message)

    try:
        with open(LOG_FILE, 'a', encoding='utf-8') as f:
            f.write(log_message + '\n')
    except Exception as e:
        print(f"Log 寫入失敗: {e}")

def read_registry_value(key_path, value_name, default=""):
    """讀取 Registry 值"""
    try:
        key = winreg.OpenKey(winreg.HKEY_CURRENT_USER, key_path, 0, winreg.KEY_READ)
        value, _ = winreg.QueryValueEx(key, value_name)
        winreg.CloseKey(key)
        return value
    except FileNotFoundError:
        return default
    except Exception as e:
        write_log(f"讀取 Registry 錯誤 ({key_path}\\{value_name}): {e}")
        return default

def load_db_config_from_registry():
    """從 Registry 載入資料庫設定"""
    global DB_CONFIG
    try:
        server = read_registry_value(REG_DB_CONFIG, "Server", DB_CONFIG['host'])
        port = read_registry_value(REG_DB_CONFIG, "Port", DB_CONFIG['port'])
        database = read_registry_value(REG_DB_CONFIG, "Database", DB_CONFIG['database'])
        username = read_registry_value(REG_DB_CONFIG, "Username", DB_CONFIG['user'])
        password = read_registry_value(REG_DB_CONFIG, "Password", DB_CONFIG['password'])

        DB_CONFIG['host'] = server
        DB_CONFIG['port'] = int(port) if isinstance(port, int) else DB_CONFIG['port']
        DB_CONFIG['database'] = database
        DB_CONFIG['user'] = username
        DB_CONFIG['password'] = password

        write_log(f"✓ 從 Registry 載入資料庫設定: {DB_CONFIG['host']}:{DB_CONFIG['port']}/{DB_CONFIG['database']}")
    except Exception as e:
        write_log(f"⚠ 無法從 Registry 載入資料庫設定，使用預設值: {e}")

# ========================================
# 資料庫操作
# ========================================

def connect_database():
    """連接資料庫"""
    global db_connection
    try:
        db_connection = pymysql.connect(**DB_CONFIG)
        write_log("✓ 資料庫連線成功")
        return True
    except Exception as e:
        write_log(f"✗ 資料庫連線失敗: {e}")
        return False

def register_or_update_machine():
    """註冊或更新機台"""
    global machine_db_id

    try:
        machine_id = read_registry_value(REG_MACHINE_CONFIG, "MachineID")
        machine_name = read_registry_value(REG_MACHINE_CONFIG, "MachineName")
        machine_ip = read_registry_value(REG_MACHINE_CONFIG, "MachineIP")

        if not machine_id:
            write_log("⚠ Registry 中沒有機台設定，跳過註冊")
            return False

        cursor = db_connection.cursor()

        # 檢查機台是否已存在
        sql_check = "SELECT id FROM machines WHERE machine_id = %s"
        cursor.execute(sql_check, (machine_id,))
        result = cursor.fetchone()

        if result:
            # 機台已存在，更新資料
            machine_db_id = result[0]
            sql_update = """
                UPDATE machines
                SET name = %s, ip_address = %s, status = 'idle', last_heartbeat = NOW(), updated_at = NOW()
                WHERE id = %s
            """
            cursor.execute(sql_update, (machine_name, machine_ip, machine_db_id))
            db_connection.commit()
            write_log(f"✓ 機台已存在，更新資料，DB ID: {machine_db_id}")
        else:
            # 機台不存在，新增
            sql_insert = """
                INSERT INTO machines (machine_id, name, ip_address, status, last_heartbeat, created_at, updated_at)
                VALUES (%s, %s, %s, 'idle', NOW(), NOW(), NOW())
            """
            cursor.execute(sql_insert, (machine_id, machine_name, machine_ip))
            db_connection.commit()
            machine_db_id = cursor.lastrowid
            write_log(f"✓ 機台註冊成功，DB ID: {machine_db_id}")

        cursor.close()
        return True

    except Exception as e:
        write_log(f"✗ 註冊機台錯誤: {e}")
        return False

def update_heartbeat():
    """更新心跳"""
    try:
        if machine_db_id == 0:
            return

        cursor = db_connection.cursor()
        sql = "UPDATE machines SET last_heartbeat = NOW() WHERE id = %s"
        cursor.execute(sql, (machine_db_id,))
        db_connection.commit()
        cursor.close()

    except Exception as e:
        write_log(f"✗ 心跳更新錯誤: {e}")

def update_machine_status(status):
    """更新機台狀態"""
    if machine_db_id == 0:
        return

    # 轉換 Registry 狀態值為資料庫 ENUM 值（必須大寫）
    status_map = {
        'in_progress': 'WORKING',  # 拍攝中 → WORKING
        'completed': 'IDLE',       # 完成後自動變回待機
        'idle': 'IDLE',            # 待機 → IDLE
    }
    db_status = status_map.get(status, 'IDLE')  # 預設為 IDLE

    cursor = db_connection.cursor()
    sql = "UPDATE machines SET status = %s, updated_at = NOW() WHERE id = %s"
    cursor.execute(sql, (db_status, machine_db_id))
    db_connection.commit()
    cursor.close()

def sync_task_to_database():
    """同步任務到資料庫"""
    global current_task_db_id

    try:
        if machine_db_id == 0:
            return

        # 從 Registry 讀取任務資料
        task_name = read_registry_value(REG_CURRENT_TASK, "TaskName", "")
        progress = read_registry_value(REG_CURRENT_TASK, "Progress", 0)
        total_images = read_registry_value(REG_CURRENT_TASK, "TotalImages", 350)
        status = read_registry_value(REG_CURRENT_TASK, "Status", "idle")
        start_time = read_registry_value(REG_CURRENT_TASK, "StartTime", "")

        # 如果沒有任務，跳過
        if not task_name:
            current_task_db_id = 0
            return

        cursor = db_connection.cursor()

        # 檢查是否有進行中的任務
        if current_task_db_id == 0:
            # 查詢是否有同名任務在進行中
            sql_check = """
                SELECT id FROM tasks
                WHERE machine_id = %s AND task_name = %s AND status = 'in_progress'
                ORDER BY id DESC LIMIT 1
            """
            cursor.execute(sql_check, (machine_db_id, task_name))
            result = cursor.fetchone()

            if result:
                current_task_db_id = result[0]
            else:
                # 建立新任務
                sql_insert = """
                    INSERT INTO tasks (machine_id, task_name, total_images, progress, status, start_time, created_at, updated_at)
                    VALUES (%s, %s, %s, %s, %s, %s, NOW(), NOW())
                """
                cursor.execute(sql_insert, (machine_db_id, task_name, total_images, progress, status, start_time))
                db_connection.commit()
                current_task_db_id = cursor.lastrowid
                write_log(f"✓ 建立新任務，Task ID: {current_task_db_id}")

        # 更新任務進度
        completion_time_sql = "NOW()" if status == "completed" else "NULL"
        sql_update = f"""
            UPDATE tasks
            SET progress = %s, status = %s, actual_completion_time = {completion_time_sql}, updated_at = NOW()
            WHERE id = %s
        """
        cursor.execute(sql_update, (progress, status, current_task_db_id))
        db_connection.commit()

        cursor.close()

        # 如果任務完成，重置 task_db_id
        if status == "completed":
            write_log(f"✓ 任務完成: {task_name} ({progress}/{total_images})")
            current_task_db_id = 0

    except Exception as e:
        write_log(f"✗ 同步任務錯誤: {e}")

# ========================================
# 主程式
# ========================================

def main():
    """主程式循環"""
    global db_connection, machine_db_id

    write_log("========== Registry Sync Service 啟動 ==========")

    # 載入資料庫設定
    load_db_config_from_registry()

    # 連接資料庫
    if not connect_database():
        write_log("✗ 無法連接資料庫，程式結束")
        return

    # 註冊機台
    if not register_or_update_machine():
        write_log("⚠ 機台註冊失敗，將持續嘗試...")

    # 主循環
    loop_count = 0
    heartbeat_count = 0

    try:
        while True:
            loop_count += 1
            heartbeat_count += 1

            # 每 3 秒同步一次
            time.sleep(SYNC_INTERVAL)

            try:
                # 讀取 Registry 狀態
                status = read_registry_value(REG_CURRENT_TASK, "Status", "idle")

                # 更新機台狀態
                update_machine_status(status)

                # 同步任務
                sync_task_to_database()

                # 每 15 秒 (5次循環) 更新一次心跳
                if heartbeat_count >= 5:
                    update_heartbeat()
                    write_log(f"✓ 心跳發送 (循環: {loop_count})")
                    heartbeat_count = 0

            except pymysql.err.OperationalError as e:
                write_log(f"✗ 資料庫連線中斷: {e}")
                write_log("⚠ 嘗試重新連線...")
                if connect_database():
                    register_or_update_machine()

            except Exception as e:
                write_log(f"✗ 同步循環錯誤: {e}")

    except KeyboardInterrupt:
        write_log("========== 程式手動停止 ==========")

    finally:
        if db_connection:
            db_connection.close()
            write_log("✓ 資料庫連線已關閉")

if __name__ == "__main__":
    main()
