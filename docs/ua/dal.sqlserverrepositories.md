# DAL для SQL Server

`BookingApp.Dal.SqlServerRepositories` реалізує контракти Bll.Common через Microsoft.Data.SqlClient та збережені процедури. BLL не залежить від типів SQL Server. Кожна операція відкриває короткочасне з'єднання з пулу; команди та readers звільняються асинхронно. Параметри мають явні типи, розміри й точність.

Readers заповнюють DAL entities та відновлюють UTC. DAL AutoMapper перетворює entities на бізнес-моделі й об'єкти-значення та виконує зворотне перетворення для запису. Ролі й дозволи користувачів завантажуються явно. EF і tracking видалено; entities та mappings залишаються в DAL. Асинхронні записи одразу зберігають дані; IUnitOfWork більше не потрібен.

Лише Reserved блокує зал. Процедура бронювання під блокуванням залу атомарно перевіряє перетин, додає бронювання та оновлює час останнього бронювання. Суміжні інтервали дозволені. Редагування залу не перезаписує цей час; видалення залу з бронюваннями повертає конфлікт. Завершення виконується обмеженими атомарними пакетами.

Застосунок підключається до віддаленого SQL Server. Початкову базу створюють через code first, потім генерують SQL setup із процедурами та reference data; deployment tool видалено. [Інструкції](../../BookingApp.Dal.SqlServerRepositories/Database/README.md). Startup не змінює базу. Runtime має лише EXECUTE; integration tests зберігають SQL Server Testcontainers.

Quartz і TimeProvider залишаються в Services, ціноутворення та валідація — у BLL. Див. [Тестування](testing.md).

## Сутності та межа зіставлення

`Entity` надає `Guid Id` для `BookingEntity`, `ConferenceHallEntity` та `UserEntity`. `RoleEntity` і `PermissionEntity` мають цілочислові ключі. `UserRoleEntity` та `RolePermissionEntity` визначаються складеним ключем із двох зовнішніх ключів і не мають додаткового Guid.

Entities зберігають примітиви: decimal суми, трилітерні коди валют, назви статусів і перелік числових значень amenities через кому. Таблицю валют не додано. `RowMapper` читає іменовані колонки, а DAL `AutoMapperConfig` відновлює об’єкти-значення Common і виконує зворотне зіставлення для запису. AutoMapper не завантажує дані. Hall та booking reads не завантажують навігаційні колекції або пов’язані halls/users.

`user_get` повертає три result sets: користувача, ролі, потім дозволи з `role_id`. `RowMapper.User`, `Role` і `Permission` заповнюють entities; репозиторій приєднує дозволи до ролей перед AutoMapper. Створення користувача використовує унікальні IDs наявних ролей; некоректна роль відкочує весь запис.

## Контракти операцій

| Операція | Процедура у схемі `booking_api` | Результат |
| --- | --- | --- |
| Hall get / list / availability | `hall_get`, `hall_list`, `hall_available` | Зал або null, матеріалізована сторінка, доступні зали за місткістю. |
| Hall create / update | `hall_create`, `hall_update` | Негайний запис; update повертає false для відсутнього залу й зберігає last-booked time. |
| Hall removal | `hall_delete` | `Removed=0`, `NotFound=1`, `HasBookings=2`; останній результат відповідає HTTP 409. |
| Booking get / list / reports | `booking_get`, `booking_list` | Бронювання або null; матеріалізовані сторінки, впорядковані за ID. |
| Overlap / reserve | `booking_has_overlap`, `booking_reserve` | Попередня перевірка; атомарний запис повертає `Created=0`, `Overlap=1`, `HallNotFound=2`, `UserNotFound=3`. |
| Due read / complete | `booking_due`, `booking_complete_due` | Обмежене читання без захоплення рядків; атомарне завершення повертає фактичну кількість змін. |
| User get / create | `user_get`, `user_create` | Користувач із ролями/дозволами; атомарний запис користувача та зв’язків ролей. |

Нумерація сторінок починається з 1; розміри сторінки й пакета мають бути додатними. `ListAsync` відкриває окреме з’єднання для кожної сторінки та не гарантує спільний snapshot під час конкурентних записів. Параметри часу мають UTC kind і тип datetime2(7); суми — decimal(18,2). Відомі outcomes повертаються output-параметрами, невідомі значення спричиняють помилку. SQL-помилки передаються до централізованої обробки API; автоматичні повтори та перетворення SqlException на бізнес-помилки не реалізовані.

## Реєстрація та конфігурація

Services реєструє DAL AutoMapper profile разом із BLL/HTTP profiles і викликає `AddSqlServerDataAccess(connectionString, commandTimeoutSeconds)`. Factory є singleton лише для конфігурації; репозиторії — scoped. Кожна операція відкриває та звільняє власне pooled connection. Процедури керують атомарними транзакціями; caller не обгортає багатокрокові процедури в ambient transaction.

Обов’язковий connection string: `ConnectionStrings:Database`, через `ConnectionStrings__Database` або user secrets. `Database:CommandTimeoutSeconds` має додатне значення, типово 30 секунд; connection timeout задається у connection string. `BackgroundJobs:CompleteBookings:Enabled` типово true; `IntervalSeconds` та `PageSize` задають розклад і розмір пакета у web appsettings.

Реєстрація перевіряє формат connection string та timeout, але не відкриває з’єднання й не застосовує SQL. Схему, права й доступність віддаленого сервера перевіряють окремо. Integration fixtures використовують локальні embedded SQL artifacts та EXECUTE-only identity, без віддалених credentials.
