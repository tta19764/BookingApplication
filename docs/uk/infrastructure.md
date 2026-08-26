# Інфраструктурний рівень

`BookingApp.Infrastructure` реалізує зовнішні та persistence-компоненти, потрібні внутрішнім рівням.

## Persistence

- `ApplicationDbContext` є EF Core Unit of Work.
- PostgreSQL repositories реалізують контракти залів, бронювань і користувачів.
- Entity configurations маплять aggregates і value objects: точність грошей, UTC timestamps, список послуг, зв'язки й правила видалення.
- Migrations версіонують реляційну схему.
- Перевірки доступності та перетину виконуються запитами до бази.
- Дані для звіту читаються детерміновано й посторінково.

## Операційні сервіси

- `DateTimeProvider` надає UTC-час через application-абстракцію.
- Quartz запускає `CompleteBookingsJob`, яка обробляє обмежені пакети завершених у часі бронювань.
- Реєстрація dependency injection зосереджена в `DependencyInjection` цього рівня.

## Конфігурація

Development connection string і параметри job знаходяться в `BookingApp.Api/appsettings.Development.json`; Docker Compose перевизначає значення, залежні від середовища. У production secrets мають надходити з конфігурації deployment, а не з committed-файлів.

Мапінги репозиторіїв і поведінку бази перевіряють обидва integration test projects із тимчасовими PostgreSQL Testcontainers.

