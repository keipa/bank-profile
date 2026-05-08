# Copilot instructions for this repository

## Build, test, and lint commands

```bash
# .NET app
dotnet restore
dotnet build BankProfiles.Web.csproj
dotnet run --project BankProfiles.Web.csproj

# Database migrations
dotnet ef database update

# Seed historical data (ratings, views, metric events)
dotnet run --project BankProfiles.Web.csproj -- --seed-historical-data
dotnet run --project BankProfiles.Web.csproj -- --reseed-historical-data  # force re-seed
```

```bash
# Unit/integration tests (xUnit)
dotnet test Tests/BankProfiles.Tests.csproj

# Run a single test
dotnet test Tests/BankProfiles.Tests.csproj --filter "FullyQualifiedName~EventStoreServiceTests.AppendEventAsync_AssignsSequenceAndCreatedDate"
```

```bash
# Cypress E2E (app must be running at http://localhost:5194)
npm install
npm run test:e2e

# Run a single Cypress spec
npx cypress run --spec "cypress/e2e/01-home.cy.js"

# Responsive viewport presets
npm run test:e2e:mobile   # 375x667
npm run test:e2e:tablet   # 768x1024
npm run test:e2e:desktop  # 1920x1080
```

```bash
# CSS linting
npx stylelint "wwwroot/css/**/*.css"
```

## High-level architecture

- **Clean Architecture layers:** `Domain/` (models, value objects, `ValidationHelper`) → `Application/` (interfaces + feature services) → `Infrastructure/` (EF Core persistence, repositories, seeders) → `Presentation/` (DI registration, middleware). All services are registered in `Presentation/DependencyInjection/ServiceCollectionExtensions.cs`.
- **Blazor Server app composition:** `Program.cs` wires interactive server components, pooled EF Core `BankDbContext` factory, memory cache, app services, a hosted snapshot service (`RatingHistoryService`), ASP.NET rate limiting, and `CircuitRateLimitingMiddleware`.
- **Event-sourced metric model:** `IEventStoreService` appends ordered events per bank (`EventSequence`), and `IEventProjectionService` reconstructs `BankProfile` from latest metric values where **last event wins**. `EventModelTraversal` walks the domain model's `JsonPropertyName` attributes to build dot-notation paths.
- **Relational + event persistence:** `Infrastructure/Persistence/DbContext/BankDbContext.cs` stores classic entities (`Banks`, `BankRatings`, `RatingHistories`, `ViewHistory`) and event/migration entities (`MetricEvents`, `BankSnapshots`, feedback tables).
- **Routing model:** canonical bank route is `/{countryCode}/{bankCode}` (`Components/Pages/BankDetail.razor`); legacy `/bank/{bankCode}` is redirected by `BankRedirect.razor`.
- **UI composition:** pages consume service interfaces; metric tiles are produced by `IBankMetricsExtractorService`, and chart components use `IChartDataService` / `IMetricChartService` over historical DB tables.
- **Localization and theming:** translations come from `Resources/Strings.{lang}.json` via `LocalizationService` (`lang` cookie), and theme state uses `ThemeService` + JS helpers (`theme` cookie).
- **Bank onboarding pipeline:** public `/submit-bank` → admin `/admin/bank-submissions` review → `IBankOnboardingService` publishes approved banks to event store, seeds DB rows, and refreshes cache.

## Key conventions in this codebase

- **Validate all bank codes with `ValidationHelper.IsValidBankCode`** (and asset paths with `IsValidBankAssetPath`) before using them in file paths, queries, or service logic. The regex allows only `[a-zA-Z0-9\-_]`.
- **JSON bank data follows `schema.json` and `Domain/BankProfiles/BankProfile.cs`** (`bankId`, `name`, nested sections). UI compatibility properties like `BankCode`, `BankName`, and `CountryCode` are derived `[JsonIgnore]` accessors, not stored fields.
- **Event metric names must stay aligned with model JSON property paths** (dot notation such as `fees.commissions.incomingDomesticPercent`), because migration/projection logic builds paths from `JsonPropertyName` metadata via `EventModelTraversal`.
- **Country behavior depends on multiple mappings:** if you add a country/language, update `BankProfile.CountryCode` switch expression, `CountryService`, `CountryCodeMapperService`, and `LocalizationService.CountryToLanguageMap`.
- **Blazor data services use `IDbContextFactory<BankDbContext>` per operation** (create context inside each method), not a long-lived shared `DbContext`.
- **Service registration:** all new services must be added to `Presentation/DependencyInjection/ServiceCollectionExtensions.cs` with the appropriate lifetime (`Scoped` for DB-dependent services, `Singleton` for stateless mappers like `CountryService`).
- **UI text is resource-key driven** (`Localization.GetString("...")`); when adding keys, keep all five `Resources/Strings.*.json` files (`en-US`, `en-GB`, `de-DE`, `fr-FR`, `es-ES`) in sync.
- **Tests mirror feature structure:** `Tests/Features/{FeatureName}/` and `Tests/Domain/` for domain unit tests. Use xUnit with service mocking.
- **Cypress tests assume Blazor initialization wait:** custom command `cy.waitForBlazor()` in `cypress/support/commands.js` should be used after `cy.visit(...)`. Specs are numbered for execution order.
