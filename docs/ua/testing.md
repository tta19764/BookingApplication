# Тестування

Запуск повного набору:

```powershell
dotnet test BookingApplicationSolution.sln
```

Для integration-тестів потрібен Docker Desktop або сумісний рушій Docker.

## BookingApp.Bll.Common.UnitTests

Швидкі тести об'єктів-значень і поведінки бізнес-моделей без імітацій залежностей, HTTP або бази даних.

## BookingApp.Bll.UnitTests

Тести managers і валідаторів із NSubstitute та FluentAssertions перевіряють асинхронні записи, результати атомарного бронювання, валідацію, ціноутворення, події після запису та звіти.

## BookingApp.Bll.IntegrationTests

Тести BLL викликають managers і SqlClient-репозиторії з реальним SQL Server Testcontainers. Перевіряються UTC, Unicode, ціни, початкові дані, доступність, атомарність, конкурентне бронювання, завершення пакетів, ролі та обмеження прав.

## BookingApp.Services.Web.IntegrationTests

HTTP-тести використовують HttpClient та тимчасовий контейнер SQL Server. Вони перевіряють створення, редагування, видалення, доступність і бронювання, HTTP-коди, JSON та точну вартість.

Архітектурні тести перевіряють ізоляцію провайдера, AutoMapper, межу контролерів/managers, серіалізацію TimeOnly, події та пакетне завершення.

Обидва проєкти використовують спільний `test/Shared/SqlServerWebAppFactory.cs`. Fixture застосовує Initialization scripts, викликає reference/demo seed methods і створює runtime-користувача лише з EXECUTE та звільняє контейнер після тестів. BLL-тести мають спільну непаралельну collection і окремі ідентифікатори даних. Планувальник у звичайних API-тестах вимкнено. Віддалена база застосунку не використовується.

Docker потрібен для integration tests. Образ SQL Server 2022 зафіксовано digest; `BOOKINGAPP_TEST_SQL_IMAGE` дозволяє узгодити його з віддаленим сервером. Перше завантаження потребує часу та ресурсів. TLS, мережу та автентифікацію віддаленого deployment перевіряють окремо. Див. [SQL Server setup](../en/database-initialization.md).

Поточний набір містить 107 тестів: Common 1, BLL unit 27, BLL integration 49 та Web integration 30. Архітектурні перевірки entities перевіряють Guid inheritance та відсутність бізнес/provider типів у властивостях DAL. Database містить лише персональні файли поза application scope. Test bootstrap використовує Initialization scripts і data seeder methods.

`DatabaseSeederTests` створює окрему schema-only базу для кожного тесту у спільному SQL Server container. Перевіряються порожній стан даних, SQL-помилки відсутніх таблиць, збереження наявних даних, відновлення пропущених reference rows, конфлікти IDs, rollback, конкурентна ідемпотентність та відмова для runtime credentials. Спільна application-test схема й віддалена база не змінюються.

`RepositoryExceptionTests` містить 18 перевірок для hall, booking і user repositories: read/write SQL failures, відсутня схема, timeout, помилка підключення та cancellation. Перевіряються sanitized PersistenceException, incident ID і один error log. Кожен тест використовує окрему disposable базу.
