# SOCpit v2026.10.11 — Инструкция по установке / Installation Guide

**Версия / Version**: `2026.10.11`  
**Дата релиза / Release Date**: `2026-10-11`  
**Платформа / Platform**: Windows 10 (1809+) / Windows 11 (x64)  
**Тип сборки / Build Type**: Self-Contained Single-File EXE (.NET 10.0 включён / .NET 10.0 runtime bundled)

---

## Быстрый старт (RU)

### 1. Скачивание
Скачайте архив релиза со страницы GitHub Releases:
- **Страница релиза**: [SOCpit v2026.10.11 - Production Ready](https://github.com/aifirsts/01/releases/tag/v2026.10.11)
- **Прямая ссылка на архив**: `SOCpit-2026.10.11-RELEASE.zip`

### 2. Распаковка
Распакуйте архив `SOCpit-2026.10.11-RELEASE.zip` в любую удобную папку (рекомендуется `C:\SOCpit2026\bin` или `C:\Program Files\SOCpit`):

```powershell
Expand-Archive -Path .\SOCpit-2026.10.11-RELEASE.zip -DestinationPath C:\SOCpit2026\App
```

### 3. Запуск
Запустите исполняемый файл:
```powershell
C:\SOCpit2026\App\SOCpit.App.exe
```
> **Примечание**: Дополнительная установка .NET Runtime **не требуется** — все необходимые библиотеки упакованы внутрь `SOCpit.App.exe`.

### 4. Хранение данных и логов
При первом запуске приложение автоматически инициализирует рабочую директорию `C:\SOCpit2026`:
- **База данных SQLite**: `C:\SOCpit2026\socpit.db` (автоматически создаётся с базовой схемой и начальной конфигурацией)
- **Логи приложения**: `C:\SOCpit2026\logs\socpit-YYYYMMDD.log` (ротация по дням, хранение 30 дней)

---

## Quick Start (EN)

### 1. Download
Download the release archive from GitHub Releases:
- **Release Page**: [SOCpit v2026.10.11 - Production Ready](https://github.com/aifirsts/01/releases/tag/v2026.10.11)
- **Archive**: `SOCpit-2026.10.11-RELEASE.zip`

### 2. Extract
Extract `SOCpit-2026.10.11-RELEASE.zip` to a folder of your choice (e.g., `C:\SOCpit2026\App`):

```powershell
Expand-Archive -Path .\SOCpit-2026.10.11-RELEASE.zip -DestinationPath C:\SOCpit2026\App
```

### 3. Run
Launch the executable:
```powershell
C:\SOCpit2026\App\SOCpit.App.exe
```
> **Note**: No separate .NET installation is required. `SOCpit.App.exe` is a self-contained single-file executable with .NET 10.0 embedded.

### 4. Data & Logs Location
On first startup, SOCpit automatically initializes its data directory at `C:\SOCpit2026`:
- **SQLite Database**: `C:\SOCpit2026\socpit.db`
- **Application Logs**: `C:\SOCpit2026\logs\socpit-YYYYMMDD.log`

---

## Системные требования / System Requirements

| Параметр / Component | Минимум / Minimum | Рекомендуется / Recommended |
|---|---|---|
| **OS** | Windows 10 x64 (1809+) | Windows 11 x64 |
| **CPU** | 2 cores (x64) | 4+ cores (x64) |
| **RAM** | 4 GB | 8+ GB |
| **Disk** | 500 MB free on `C:\` | 2+ GB SSD |
| **Display** | 1366×768 | 1920×1080 (Full HD) |

---

## Проверка целостности / Checksum Verification

Для проверки контрольной суммы SHA-256 выполните в PowerShell:
```powershell
Get-FileHash .\SOCpit-2026.10.11-RELEASE.zip -Algorithm SHA256
Get-FileHash .\SOCpit.App.exe -Algorithm SHA256
```
Сравните полученное значение с контрольными суммами, указанными в описании релиза и в отчёте `audit/FINAL-VALIDATION-2026-10-11.md`.
