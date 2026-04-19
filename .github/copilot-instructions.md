# Copilot instructions for this repository

## Build, test, and lint commands

```bash
# .NET app
dotnet restore
dotnet build BankProfiles.Web.csproj
dotnet run --project BankProfiles.Web.csproj

# Database migrations
dotnet ef database update
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
```

```bash
# CSS linting (documented in docs/RESPONSIVE_DESIGN_TESTING_GUIDE.md)
npx stylelint "wwwroot/css/**/*.css"
```

## High-level architecture

- **Blazor Server app composition:** `Program.cs` wires interactive server components, pooled EF Core `BankDbContext` factory, memory cache, app services, a hosted snapshot service, ASP.NET rate limiting, and `CircuitRateLimitingMiddleware`.
- **Event-sourced metric model:** `IEventStoreService` appends ordered events per bank (`EventSequence`), and `IEventProjectionService` reconstructs `BankProfile` from latest metric values where **last event wins**.
- **Relational + event persistence:** `Data/BankDbContext.cs` stores classic entities (`Banks`, `BankRatings`, `RatingHistories`, `ViewHistory`) and event/migration entities (`MetricEvents`, `BankSnapshots`, feedback tables).
- **Routing model:** canonical bank route is `/{countryCode}/{bankCode}` (`Components/Pages/BankDetail.razor`); legacy `/bank/{bankCode}` is redirected by `BankRedirect.razor`.
- **UI composition:** pages consume service interfaces; metric tiles are produced by `IBankMetricsExtractorService`, and chart components use `IChartDataService` over historical DB tables.
- **Localization and theming:** translations come from `Resources/Strings.{lang}.json` via `LocalizationService` (`lang` cookie), and theme state uses `ThemeService` + JS helpers (`theme` cookie).

## Key conventions in this codebase

- **Validate all bank codes with `ValidationHelper.IsValidBankCode`** before using them in file paths, queries, or service logic.
- **JSON bank data follows `schema.json` and `Models/BankProfile*.cs`** (`bankId`, `name`, nested sections). UI compatibility properties like `BankCode`, `BankName`, and `CountryCode` are derived `[JsonIgnore]` accessors, not stored fields.
- **Event metric names must stay aligned with model JSON property paths** (dot notation such as `fees.commissions.incomingDomesticPercent`), because migration/projection logic builds paths from `JsonPropertyName` metadata.
- **Country behavior depends on multiple mappings:** if you add a country/language, update `BankProfile.CountryCode`, `CountryService`, and `LocalizationService.CountryToLanguageMap`.
- **Blazor data services use `IDbContextFactory<BankDbContext>` per operation** (create context inside each method), not a long-lived shared `DbContext`.
- **UI text is resource-key driven** (`Localization.GetString("...")`); when adding keys, keep all `Resources/Strings.*.json` files in sync.
- **Cypress tests assume Blazor initialization wait:** custom command `cy.waitForBlazor()` in `cypress/support/commands.js` should be used after `cy.visit(...)`.
