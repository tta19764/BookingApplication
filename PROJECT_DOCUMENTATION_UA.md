# Документація проєкту Booking Application

## Посилання на репозиторій

```text
https://github.com/tta19764/BookingApplication
```

## Опис бізнес-задачі

Booking Application - це API для управління бронюванням і орендою конференц-залів. Система дозволяє клієнтам переглядати зали, шукати доступні варіанти за датою, часом і місткістю, створювати бронювання з додатковими послугами та отримувати розрахунок вартості оренди.

Проєкт орієнтований на подальше розширення: додавання авторизації, адміністрування, нових звітів, додаткових правил ціноутворення та інтеграцій із зовнішніми сервісами.

## Основні можливості

- Створення конференц-залів із назвою, місткістю, погодинною ціною, валютою та списком доступних послуг.
- Редагування інформації про конференц-зали.
- Видалення конференц-залів.
- Пошук доступних залів за датою, початковим часом, кінцевим часом і потрібною місткістю.
- Створення бронювання для системного seeded-користувача.
- Автоматичний розрахунок вартості бронювання з урахуванням часу оренди та додаткових послуг.
- Автоматичне завершення бронювань після закінчення їхнього періоду.
- Звіт з аналітикою бронювань і доходу.

## Бізнес-правила ціноутворення

Бронювання має бути в межах одного календарного дня та в межах дозволених годин оренди. Запити на бронювання та пошук доступних залів використовують UTC-час у форматі `HH:mm`, наприклад `10:40`.

| Період | Правило |
| --- | --- |
| 06:00-09:00 | Знижка 10% |
| 09:00-12:00 | Базова погодинна ціна |
| 12:00-14:00 | Націнка 15% |
| 14:00-18:00 | Базова погодинна ціна |
| 18:00-23:00 | Знижка 20% |

Неповні години оплачуються пропорційно. Наприклад, 20 хвилин рахуються як третина погодинної ставки. Якщо бронювання перетинає кілька цінових періодів, система ділить його на частини та рахує кожну частину окремо.

Додаткові послуги оплачуються один раз за бронювання:

| Послуга | Значення enum | Ціна |
| --- | ---: | ---: |
| Проєктор | 1 | 500 UAH |
| Wi-Fi | 2 | 300 UAH |
| Звукова система | 3 | 700 UAH |

## Архітектура

Проєкт побудований у стилі Clean Architecture. Основна ідея - відокремити бізнес-логіку від API, бази даних та інфраструктурних деталей.

| Проєкт | Відповідальність |
| --- | --- |
| `BookingApp.Domain` | Сутності, value objects, доменні події, бізнес-правила, правила ціноутворення та контракти репозиторіїв. |
| `BookingApp.Application` | Commands, queries, handlers, validators, pipeline behaviors, mapping та абстракції прикладного рівня. |
| `BookingApp.Infrastructure` | EF Core, PostgreSQL, реалізації репозиторіїв, конфігурації сутностей, Unit of Work, Quartz jobs, date/time provider. |
| `BookingApp.Api` | Minimal APIs, версіонування API, Swagger, middleware, логування, DI, міграції та seed data. |
| `BookingApp.Domain.UnitTests` | Unit-тести доменного рівня. |
| `test/BookingApp.Application.UnitTests` | Unit-тести application handlers з mocked-залежностями. |
| `test/BookingApp.Application.IntegrationTests` | Integration-тести application-рівня з реальним API host, EF Core та PostgreSQL Testcontainer. |

## Технічні рішення

- **Minimal APIs** використовуються для компактного опису HTTP endpoints.
- **API Versioning** реалізовано через URL-сегменти, поточна версія: `/api/v1`.
- **MediatR** використовується для commands, queries та handlers.
- **FluentValidation** використовується для перевірки вхідних команд і запитів.
- **EF Core + PostgreSQL** використовуються для persistence-рівня.
- **Quartz.NET** використовується для background job, яка завершує бронювання після закінчення періоду.
- **Serilog + Seq** використовуються для структурованого логування.
- **Docker Compose** запускає API, PostgreSQL і Seq у development-середовищі.
- **Testcontainers** використовується в integration-тестах для запуску тимчасової PostgreSQL бази.

## Seeded data

Поки реєстрація, логін і авторизація не входять у поточний scope, API використовує одного seeded-користувача з усіма потрібними ролями та permissions.

Seeded user ID:

```text
aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa
```

Початкові зали:

| Зал | Місткість | Базова ціна |
| --- | ---: | ---: |
| Hall A | 50 | 2000 UAH |
| Hall B | 100 | 3500 UAH |
| Hall C | 30 | 1500 UAH |

ID залів генеруються під час seeding. Перед створенням бронювання потрібно отримати актуальний ID через endpoint списку конференц-залів.

## API

Усі endpoints знаходяться під префіксом:

```text
/api/v1
```

Conference halls:

```http
POST   /api/v1/conference-halls
GET    /api/v1/conference-halls?page=1&pageSize=20
GET    /api/v1/conference-halls/{hallId}
PUT    /api/v1/conference-halls/{hallId}
DELETE /api/v1/conference-halls/{hallId}
GET    /api/v1/conference-halls/available?date=2026-09-01&startTime=10:40&endTime=14:00&capacity=50
```

Bookings:

```http
GET  /api/v1/bookings?page=1&pageSize=20
POST /api/v1/bookings
```

Reports:

```http
GET /api/v1/reports/bookings-summary
```

Приклад створення залу:

```json
{
  "name": "Hall D",
  "capacity": 80,
  "hourlyRate": 2800,
  "currencyCode": "UAH",
  "amenities": [1, 2, 3]
}
```

Приклад створення бронювання:

```json
{
  "hallId": "<conference-hall-id-from-get-conference-halls>",
  "date": "2026-09-01",
  "startTime": "10:40",
  "endTime": "14:00",
  "amenities": [1, 2]
}
```

Успішна відповідь на створення бронювання містить:

- ID бронювання;
- ID залу;
- початок і кінець бронювання;
- вартість оренди за період;
- вартість додаткових послуг;
- загальну суму;
- валюту.

## Звіти та аналітика

Endpoint:

```http
GET /api/v1/reports/bookings-summary
```

Повертає:

- загальну кількість бронювань;
- загальний дохід;
- валюту;
- статистику по кожному залу.

Обробка звіту реалізована посторінково, щоб не завантажувати всі бронювання в пам'ять одночасно.

## Запуск через Docker Compose

Передумови:

- Docker Desktop

Команда запуску:

```powershell
docker compose up --build
```

Після запуску доступні:

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`
- PostgreSQL: `localhost:5432`
- Seq UI: `http://localhost:8081`
- Seq ingestion endpoint: `http://localhost:5341`

Зупинити контейнери:

```powershell
docker compose down
```

Зупинити контейнери та видалити persisted data:

```powershell
docker compose down -v
```

## Локальний запуск без Docker

Передумови:

- .NET 10 SDK
- PostgreSQL на `localhost:5432`
- опціонально Seq на `http://localhost:5341`

Очікуваний connection string для local development:

```text
Host=localhost;Port=5432;Database=booking_app;Username=postgres;Password=postgres
```

Запуск API:

```powershell
dotnet run --project BookingApp.Api
```

API застосовує EF Core migrations та виконує seeding при старті в development-середовищі.

## Тестування

Запуск усіх тестів:

```powershell
dotnet test BookingApplicationSolution.sln
```

Тестове покриття включає:

- domain unit tests для сутностей, value objects, доменних подій та pricing rules;
- application unit tests для command/query handlers;
- application integration tests з `WebApplicationFactory<Program>` і тимчасовою PostgreSQL базою через Testcontainers.

Для integration-тестів має бути запущений Docker Desktop.

## Postman

Postman collection та environment знаходяться в папці:

```text
Postman
```

Файли:

- `BookingApp.Api.postman_collection.json`
- `BookingApp.Local.postman_environment.json`

Колекція дозволяє перевірити створення залу, створення бронювання, overlap validation, paginated endpoints та booking summary report.

## Поточні обмеження

- Реєстрація, логін і повноцінна авторизація не входять у поточний scope.
- Запити на бронювання виконуються від seeded-користувача.
- Система налаштована для development/test сценаріїв.
- Для production потрібно додати authentication, authorization policies, secret management, deployment-specific configuration та додатковий security hardening.

