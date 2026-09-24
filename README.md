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
- `src/Cli` — консольний застосунок, що взаємодіє з бібліотекою `Core` через `ProjectReference` та відповідає за вивід інформації (текст/JSON).

### Структура каталогу Core
- `Core/Dto/` — record-типи для передачі даних (лабораторна 3).
- `Core/Domain/` — бізнес-моделі предметної області "Склад" (лабораторна 4).
- `Core/Storage/` — сховища та сервіси доступу до даних (лабораторна 5).

### Порівняння режимів публікації

| RID | Режим | Розмір publish | Потрібен runtime | Призначення |
| :--- | :--- | :--- | :--- | :--- |
| **win-x64** | **Self-contained** | 78 МБ | ні | Повна автономність для систем без встановленого .NET. |
| **win-x64** | **Framework-dependent** | 213 КБ | так (.NET 10) | Мінімальний розмір завдяки використанню системного рантайму. |
| **win-x64** | **Single-file** | 71 МБ | ні | Зручність розповсюдження: один монолітний файл `Cli.exe`. |
| **win-x64** | **Single-file + Trimmed** | 13 МБ | ні | Максимальна оптимізація розміру (видалено невикористовуваний BCL). |

### Команди для збірки та запуску
- Запуск CLI: `dotnet run --project src/Cli`
- Публікація Self-contained: `dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained true -o publish/self-contained`
- Публікація Framework-dependent: `dotnet publish src/Cli -c Release -r win-x64 -f net10.0 --self-contained false -o publish/framework-dependent`
- Прямий запуск скомпільованого файлу: `./publish/self-contained/Cli.exe`

### Додаткові завдання lab2
* **Single-file:** прапорець `-p:PublishSingleFile=true` упакував усі залежності та runtime в один файл `Cli.exe` (розмір папки — 71 МБ). Програма успішно запускається автономно.
* **Trimming:** прапорець `-p:PublishTrimmed=true` зменшив розмір з 71 МБ до 13 МБ. Отримано warning `IL2026` (`JsonSerializer.Serialize`). Trimming небезпечний для коду з рефлексією, оскільки IL Linker видаляє типи й методи без явних статичних посилань, що призводить до збоїв у runtime під час динамічного виклику.
* **Multi-targeting та умовна компіляція:** у `src/Core` реалізовано властивість `BuildNote` через препроцесорні директиви `#if NET10_0_OR_GREATER` та `#else`:
  * `net10.0`: виводить `збірка під net10.0` (Runtime: .NET 10.0.12)
  * `net8.0`: виводить `збірка під net8.0` (Runtime: .NET 8.0.16)



  ## Лабораторна робота 4: Доменна модель, інваріанти, інкапсуляція

### Доменні сутності
* **Product** — сутність товару зі складом та операціями зміни балансу (прихід, видача).

### Таблиця інваріантів
| Інваріант / Бізнес-правило | Тип винятку | Метод перевірки |
| :--- | :--- | :--- |
| Ідентифікатор, SKU, назва та одиниця вимірювання не можуть бути порожніми | `ArgumentException` | `Product.Create` |
| Початковий залишок не може бути від'ємним (`< 0`) | `ArgumentOutOfRangeException` | `Product.Create` |
| Кількість приходу товару має бути строго більшою за нуль (`<= 0`) | `ArgumentOutOfRangeException` | `Product.RegisterArrival` |
| Кількість видачі товару має бути строго більшою за нуль (`<= 0`) | `ArgumentOutOfRangeException` | `Product.Issue` |
| Не можна видати товару більше, ніж є на залишку (захист від перевитрати) | `InvalidOperationException` | `Product.Issue` |