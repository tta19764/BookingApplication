# Ініціалізація SQL Server та наповнення даними

DAL використовує ADO.NET (`Microsoft.Data.SqlClient`) і процедури `booking_api` замість EF/PostgreSQL. Застосунок підключається до налаштованої бази SQL Server; контейнери SQL Server використовуються лише в інтеграційних тестах.

## Порядок запуску

Common визначає `IDatabaseSeeder`; DAL реалізує його, а Services реєструє як scoped. Якщо `DatabaseSeeding:Enabled=true`, `StartupDataSeeder` послідовно очікує `InitializeAsync`, `SeedReferenceDataAsync` і, за `IncludeDemoData=true`, `SeedDemoDataAsync`. Це відбувається до HTTP-запитів і фонових завдань. Помилка зупиняє запуск. За вимкненого прапорця startup не отримує seeder з DI.

## Конфігурація та права

Обидва прапорці в базовій конфігурації мають значення false. `appsettings.Development.json` є локальним файлом, ігнорується Git; його прапорці налаштовуйте явно. `ConnectionStrings:Database` використовується і для DAL, і для seeding. Необов’язковий `ConnectionStrings:Seeding` перевизначає підключення для setup; порожнє значення використовує `Database`.

Для нових скриптів потрібні права створення таблиць, процедур, ролей і надання дозволів; для даних — SELECT/INSERT. Окрема setup identity дозволяє залишити runtime лише EXECUTE. Скрипт прав не створює login і не призначає користувача до ролі.

Environment variables використовують подвійне підкреслення: `DatabaseSeeding__Enabled`, `DatabaseSeeding__IncludeDemoData`, `ConnectionStrings__Database`, `ConnectionStrings__Seeding`. Compose не читає appsettings для `${...}`: підключення треба передати через shell або `.env`; обидва прапорці Compose за замовчуванням false.

## Скрипти та повторний запуск

`Initialization/Scripts` містить `001_schema.sql`, `002_stored_procedures.sql` і `003_permissions.sql`. `DatabaseInitializer` застосовує їх по черзі з окремими транзакціями та записує версії й SHA256 у `dbo.booking_schema_versions`. Незмінені застосовані скрипти пропускаються; зміна checksum зупиняє запуск. Для змін додавайте новий versioned script.

База має існувати до запуску. Системні бази заборонені. Початковий скрипт відхиляє наявні application tables без journal; для зовнішньої code-first схеми потрібен перевірений baseline. `Database/` призначено для особистих файлів, ігнорується Git і виключено з ресурсів застосунку.

Reference seeding додає відсутні ролі, дозволи, зв’язки й тимчасового API-користувача. Конфлікт зарезервованих ідентичностей спричиняє rollback без перезапису. Demo додає Hall A/B/C лише за порожнього каталогу залів; будь-який наявний зал пропускає всю операцію.

Data methods використовують спільне application lock та serializable transactions. Скрипти й кожне наповнення комітяться окремо; помилка пізнішого кроку не скасовує попередні кроки. `InspectAsync` повертає row counts і `HasData`, а не перевіряє metadata схеми. Перед inspection/data methods викликайте `InitializeAsync`. Прапорці керують startup, але не забороняють явні виклики через scoped `IDatabaseSeeder`; HTTP endpoint для seeding немає.

Детальні приклади, посилання на скрипти та troubleshooting: [English initialization guide](../en/database-initialization.md). Архітектура: [рефакторинг](refactoring.md). Тести: [тестування](testing.md).
