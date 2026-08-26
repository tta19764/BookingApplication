# Тестування

Усі тестові методи мають явні секції `Arrange`, `Act` і `Assert`. Запуск повного набору:

```powershell
dotnet test BookingApplicationSolution.sln
```

Для integration-тестів потрібен Docker Desktop або сумісний Docker engine.

## BookingApp.Domain.UnitTests

Швидкі тести переходів статусів, доменних подій, дозволених годин, неповних годин, тарифних меж, знижок, пікової націнки, вартості послуг і невалідних періодів. Вони не використовують mocks, HTTP або базу даних.

## BookingApp.Application.UnitTests

Тести handlers із NSubstitute. Перевіряють створення залу, пошук доступності, успішні й неуспішні бронювання, overlap, непідтримувані послуги, минулий час, виклики repositories та Unit of Work, а також посторінкову агрегацію звіту.

## BookingApp.Application.IntegrationTests

Application-level тести отримують MediatR та EF Core з реального host, виконують commands/queries без HTTP і перевіряють PostgreSQL persistence, mappings, seed data, UTC timestamps, доступність і деталізацію ціни.

## BookingApp.Api.IntegrationTests

End-to-end тести викликають in-memory ASP.NET Core host через `HttpClient` і використовують реальну тимчасову PostgreSQL базу. Вони покривають п'ять обов'язкових операцій: створення, оновлення, видалення, пошук доступності та бронювання. Assertions перевіряють HTTP status codes, routing/model binding, JSON contracts, persistence, виключення перетинів і точну ціну пікового часу та послуг.

Кожен integration test project має власні `WebApplicationFactory` і PostgreSQL container, тому API та application test concerns залишаються розділеними.

