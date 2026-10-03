# Layered architecture refactoring

The project was migrated from its earlier Clean Architecture/CQRS-oriented structure to an explicit layered architecture. The current solution contains four production projects and four test projects; obsolete `Api`, `Application`, `Domain`, and `Infrastructure` project directories are no longer part of the repository.

## Current project mapping

| Project | Layer | Responsibility |
| --- | --- | --- |
| `BookingApp.Services.Web` | Service | MVC controllers, HTTP DTOs, middleware, configuration, dependency injection, Swagger, background jobs, seed data, and domain-event handlers. |
| `BookingApp.Bll.Common` | Common | Business models, value objects, manager interfaces, repository interfaces, errors, results, and domain-event contracts. It has no dependency on ASP.NET Core or EF Core. |
| `BookingApp.Bll` | BLL | Service-style managers, pricing, booking-period construction, FluentValidation validators, and mappings from business models to BLL read models. |
| `BookingApp.Dal.PostgreSQLRepositories` | DAL | EF Core context, PostgreSQL repositories, persistence entities, entity configurations, AutoMapper mappings, and migrations. |

The compile-time dependency direction is:

```text
Services.Web ──> Bll ──> Bll.Common
      │                     ▲
      └──> Dal.PostgreSQLRepositories ──┘
```

`Services.Web` is the composition root and is the only project that registers concrete implementations.

## Main changes

### Managers instead of request handlers

MediatR, Commands, Queries, and request handlers were removed. Controllers call `IBookingManager`, `IConferenceHallManager`, and `IReportManager`. Managers expose cohesive business operations and use repository abstractions and other business services through dependency injection.

### Separate representations at each boundary

- Common contains anemic business models and operation input models.
- DAL contains EF Core entities that inherit the persistence-only `Entity` base class.
- Services contains HTTP request and response DTOs.
- AutoMapper profiles are owned by the layer performing each conversion: Services maps HTTP DTOs, BLL builds read models, and DAL maps business models to persistence entities.

### MVC controllers

Minimal API endpoint groups were replaced with versioned ASP.NET Core MVC controllers. Controllers depend on manager interfaces and `IMapper`; they do not access repositories or EF Core directly. Existing `/api/v1` routes and response envelopes were preserved.

### Validation

The static `ManagerInputValidator` was replaced with operation-specific FluentValidation validators in BLL. Common input models use `DateOnly` and `TimeOnly` instead of passing time strings into managers. Services discovers all public BLL validators by assembly. Managers retain rules that require application state, such as hall existence, booking overlap, supported amenities, and past-time checks.

### Explicit persistence

The former save-time `EntityChangeTracker` copied changed Common models back into tracked EF entities implicitly. It was removed. Repositories now expose explicit `Update` operations, map changes into entities found by EF Core, and persist them through `IUnitOfWork`. The generic repository orders pages directly by the shared DAL entity ID and has no redundant ordering or model-ID hooks.

### Mappings and dependency injection

Manual `ToModel` projections in managers were replaced with the BLL AutoMapper profile. Services performs the single AutoMapper registration for the Services, BLL, and DAL profiles. FluentValidation uses assembly discovery, while managers, repositories, event dispatchers, and jobs remain explicitly registered at the composition root.

### Tests and repository layout

All test projects and the Postman assets are under `test/`. The maintained suites are `BookingApp.Bll.Common.UnitTests`, `BookingApp.Bll.UnitTests`, `BookingApp.Bll.IntegrationTests`, and `BookingApp.Services.Web.IntegrationTests`. Architecture tests verify AutoMapper configuration, DAL entity rules, controller dependencies, request serialization, and event dispatch. Obsolete build directories from the former projects were removed.

### Database migration impact

The migration history was consolidated into the current initial migration. Existing local databases created by the old migration chain are not compatible with that new history. Disposable development databases must be recreated; production data would require a deliberate incremental migration or migration-history reconciliation instead of deleting the database.

## Result

The refactored solution has explicit layer boundaries, controller-to-manager flow, visible persistence operations, centralized composition, independently testable business logic, and no dependency on a mediator pipeline. The API behavior and required business capabilities remain intact.
