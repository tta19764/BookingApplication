# Layered architecture та міграція ADO.NET

Попередній рефакторинг замінив mediator handlers на managers і MVC-контролери. Поточна міграція замінює EF/PostgreSQL на ADO.NET та процедури віддаленого SQL Server.

Services.Web відповідає за HTTP, композицію, події та Quartz. Bll.Common містить бізнес-моделі й незалежні контракти. BLL виконує валідацію, ціноутворення та сценарії використання. Dal.SqlServerRepositories містить SqlClient, readers і SQL-скрипти. Початкову базу створюють через code first, потім генерують setup SQL; проєкт deployment tool видалено.

Контролери залежать від managers, BLL — від контрактів Common. Типи провайдера та SQL залишаються в DAL. DAL entities та AutoMapper ізолюють представлення даних від бізнес-моделей Common. AutoMapper також використовується в BLL/Services. EF tracking, generic repository та IUnitOfWork видалено.

Асинхронні записи одразу викликають процедури. Бронювання та час залу зберігаються атомарно, а блокування залу захищає перевірку перетину. Лише Reserved блокує зал; суміжні періоди дозволені. Редагування не перезаписує час бронювання; видалення залу з бронюваннями повертає HTTP 409. Завершення виконується обмеженими пакетами.

Startup не змінює схему та не заповнює віддалену базу. Deployment використовує окремі права; runtime має лише EXECUTE. Дані PostgreSQL потребують окремого перевіреного перенесення. Див. [SQL Server setup](../../BookingApp.Dal.SqlServerRepositories/Database/README.md).

Інтеграційні тести використовують SQL Server Testcontainers із production-скриптами й обмеженим runtime-користувачем. Вони перевіряють конкурентність, rollback, завершення та права. Застосунок підключається до віддаленого SQL Server; контейнери бази використовуються лише для тестів.
