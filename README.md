CrossApp-наскрізний проєкт з крос-платформного програмування.

## Предметна область: Склад
- **Сутності:** 
  - `Product` (товар) — найменування, характеристики та облік номенклатури.
  - `StockBatch` (партія) — інформація про конкретну партію товару, дату надходження та термін придатності.
  - `Warehouse` (склад) — адреса та характеристики приміщення зберігання.
  - `Movement` (переміщення) — фіксація внутрішніх та зовнішніх переміщень партій між складами.
- **Призначення:** облік залишків товарів на складах у розрізі партій та відстеження їх переміщень.



## Середовище розробки
.NET SDK: 9.0.100 (x64)
Цільова платформа (TFM):** `net9.0`
RID: `win-x64`
Операційна система: Windows 11 Pro x64 (Build 26200)
Інструменти: Visual Studio Code, Git Bash, Docker Desktop



## Додаткове завдання 1: Порівняння розмірів self-contained публікацій

Було виконано автономну (self-contained) публікацію консольного застосунку під дві різні цільові платформи (RID):

`win-x64`: 75 MB  
  Команда: `dotnet publish src/Cli -c Release -r win-x64 --self-contained true`
`linux-x64`: 77 MB  
  Команда: `dotnet publish src/Cli -c Release -r linux-x64 --self-contained true`


## Додаткове завдання 3: Запуск у Docker-контейнері

Проєкт змонтовано та запущено в офіційному SDK-образі .NET без зміни вихідного коду:
 bash
MSYS_NO_PATHCONV=1 docker run --rm -v "${PWD}:/src" -w /src [mcr.microsoft.com/dotnet/sdk:9.0](https://mcr.microsoft.com/dotnet/sdk:9.0) dotnet run --project src/Cli


## Запуск застосунку
  bash
`dotnet build`
`dotnet run --project src/Cli`




## Лабораторна робота 2: Бібліотека Core та режими публікації

### Структура рішення
- `src/Core` — class library (Multi-targeting: `net8.0`, `net10.0`), містить логіку збору інформації про середовище (`EnvironmentInfo`, `EnvironmentReport`).
- `src/Cli` — консольний застосунок, що взаємодіє з бібліотекою `Core` через `ProjectReference` та відповідає лише за вивід інформації (текст/JSON).

### Структура каталогу Core
- `Core/Dto/` — record-типи для передачі даних (лабораторна 3).
- `Core/Domain/` — бізнес-моделі предметної області "Склад" (лабораторна 4).
- `Core/Storage/` — сховища та сервіси доступу до даних (лабораторна 5).

### Порівняння режимів публікації (RID: win-x64)

| Режим | Розмір каталогу | Наявність .NET Runtime | Призначення |
| :--- | :--- | :--- | :--- |
| **Self-contained** | 78 МБ | Не потрібен (вбудований у пакет) | Повна автономність для клієнтських машин без встановленого .NET. |
| **Framework-dependent** | 213 КБ | Потрібен встановлений .NET 10 | Мінімальний розмір артефакту за рахунок системного рантайму. |

### Команди для збірки та запуску
- Запуск CLI: `dotnet run --project src/Cli`
- Публікація Self-contained: `dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -o publish/self-contained`
- Публікація Framework-dependent: `dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained false -o publish/framework-dependent` `./publish/self-contained/Cli.exe`


 
### Додаткові завдання lab2
* **Trimming:** розмір зменшено з 71 MB до 13 MB. Отримано warning `IL2026` (`JsonSerializer.Serialize`). Trimming небезпечний для рефлексії, бо видаляє типи й методи, що не мають статичних викликів у коді, що веде до помилок у runtime.
* **Multi-targeting та умовна компіляція:** реалізовано константу `BuildNote` через `#if NET10_0_OR_GREATER`.
  * `net10.0`: виводить `збірка під net10.0` (CLR 10.0.12)
  * `net8.0`: виводить `збірка під net8.0` (CLR 8.0.16)