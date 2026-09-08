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