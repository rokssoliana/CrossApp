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

### Приклади перевизначення конфігурації
* **PowerShell:** `$env:CROSSAPP_DATA_PATH = "C:\custom_data"`
* **Bash / Linux:** `export CROSSAPP_DATA_PATH="/tmp/data"`
* **User Secrets (локально):** `dotnet user-secrets set "Integration:ApiKey" "local-dev-key" --project src/Cli`