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

Обидва проєкти використовують спільний `test/Shared/SqlServerWebAppFactory.cs`. Fixture застосовує ті самі SQL-скрипти, що й deployment, створює runtime-користувача лише з EXECUTE та звільняє контейнер після тестів. BLL-тести мають спільну непаралельну collection і окремі ідентифікатори даних. Планувальник у звичайних API-тестах вимкнено. Віддалена база застосунку не використовується.

Docker потрібен для integration tests. Образ SQL Server 2022 зафіксовано digest; `BOOKINGAPP_TEST_SQL_IMAGE` дозволяє узгодити його з віддаленим сервером. Перше завантаження потребує часу та ресурсів. TLS, мережу та автентифікацію віддаленого deployment перевіряють окремо. Див. [SQL Server setup](../../BookingApp.Dal.SqlServerRepositories/Database/README.md).

Поточний набір має 64 тести: Common 1, BLL unit 27, BLL integration 22 та Web integration 14. Архітектурні перевірки entities перевіряють Guid inheritance та відсутність бізнес/provider типів у властивостях DAL. Database artifacts навмисно не включено до migration commits; їх потрібно відновити або підготувати локально перед збиранням integration-test bootstrap із такого checkout.
