## Лабораторна робота 8: Асинхронний імпорт, винятки та прогрес
- Реалізовано асинхронний зчитувач даних `JsonCatalogImporter` (`ICatalogImporter`) із використанням `async/await` та `CancellationToken`.
- Впроваджено кастомний тип винятку `DomainException` для обробки помилок валідації доменних правил під час імпорту.
- Реалізовано відстеження та відображення прогресу обробки файлу у реальному часі за допомогою стандартного інтерфейсу `IProgress<int>`.
- Написано модульні тести (`Core.Tests`) для перевірки коректності асинхронного імпорту, обробки скасування та валідації вхідних даних.

## Лабораторна робота 9: Конфігурація, Secrets та атомарний JSON
- Налаштовано збирання конфігурації через `ConfigurationBuilder` (файли `appsettings.json`, `appsettings.Development.json`, User Secrets, змінні середовища та аргументи CLI).
- Забезпечено кросплатформність: усунуто жорстко прописані абсолютні шляхи Windows за допомогою `AppContext.BaseDirectory` та `Path.Combine`.
- Безпечне збереження секретів: ключ `Integration:ApiKey` зберігається поза репозиторієм за допомогою механізму `user-secrets`.
- Впроваджено атомарний запис сховища `FileCatalogStore` через тимчасові файли (`.tmp + File.Move`) та відключено екранування кирилиці (`JavaScriptEncoder.UnsafeRelaxedJsonEscaping`).

### Таблиця ключів конфігурації
| Ключ | Значення за замовчуванням | Опис |
| :--- | :--- | :--- |
| `EnvironmentName` | `Development` | Поточне середовище виконання (`Development` / `Production`) |
| `Storage:DataPath` | `data` | Відносна або абсолютна директорія для збереження JSON |
| `Storage:FileName` | `items.json` | Назва файлу каталогу |
| `Integration:ApiKey` | *(відсутнє)* | Секретний ключ доступу (збережено у User Secrets) |
| `ConnectionStrings:AppDb` | `Data Source=data/app.db` | Рядок підключення до бази даних SQLite для `Api` |

### Приклади перевизначення конфігурації
* **PowerShell:** `$env:CROSSAPP_DATA_PATH = "C:\custom_data"`
* **Bash / Linux:** `export CROSSAPP_DATA_PATH="/tmp/data"`
* **User Secrets (локально):** `dotnet user-secrets set "Integration:ApiKey" "local-dev-key" --project src/Cli`

## Запуск HTTP API (Лабораторна №10)
* Запуск сервера: `dotnet run --project src/Api` (порт за замовчуванням: `http://localhost:5080`)
* Документація Scalar UI (Development): `http://localhost:5080/scalar/v1`
* Створення товару:
  `curl.exe -i -X POST http://localhost:5080/api/products -H "Content-Type: application/json" -d '{\"sku\":\"A-100\",\"name\":\"Cement-M400\",\"unit\":\"bag\",\"quantity\":40}'`

## База даних SQLite та міграції EF Core (Лабораторна №11)
* **Створення нової міграції:**
  `dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/Api`
* **Застосування міграцій до бази даних:**
  `dotnet ef database update --project src/Infrastructure --startup-project src/Api`
* **Перегляд списку міграцій або SQL-скрипту:**
  `dotnet ef migrations list --project src/Infrastructure --startup-project src/Api`
  `dotnet ef migrations script --project src/Infrastructure --startup-project src/Api`
* **Скидання бази даних (видалення файлу БД та повторне створення зі схеми міграцій):**
  `dotnet ef database drop --force --project src/Infrastructure --startup-project src/Api`
  `dotnet ef database update --project src/Infrastructure --startup-project src/Api`
  ## Архітектура та залежності шарів (Лабораторна №13)

* `Core` — доменна модель, інваріанти, винятки (`DomainException`) та інтерфейси сховищ (не залежить від жодних внутрішніх проєктів чи зовнішніх пакетів).
* `Infrastructure` -> `Core` (EF Core, `AppDbContext`, міграції та реалізації репозиторіїв SQLite).
* `Api` -> `Core`, `Infrastructure` (Minimal API, DTO, мапінг, обробка помилок `ProblemDetails`, конфігурація DI).
* `Cli` -> `Core`, `Infrastructure` (консольний клієнт).
* `Web` -> лише HTTP-контракт `Api` (Blazor Web App `InteractiveServer`).
* `Core.Tests` -> `Core` (модульні тести доменної логіки).

## Вебклієнт Blazor (Лабораторна №12)
* Запуск API: `dotnet run --project src/Api --urls http://localhost:5080`
* Запуск Web: `dotnet run --project src/Web --urls http://localhost:5090` (маршрут: `/products`)