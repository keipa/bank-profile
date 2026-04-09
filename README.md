# Bank Profiles - .NET Blazor Application

A comprehensive bank comparison platform built with .NET 10 Blazor Server, featuring intelligent caching, multi-criteria ratings, detailed technical specifications, and multi-country support with internationalization.

## Features

### Core Features
- **5 Banks** with comprehensive profiles
- **Multi-criteria Rating System** (10-point scale)
  - Service Quality
  - Fees & Commissions
  - Convenience
  - Digital Services
  - Customer Support
- **Advanced Caching** - 500MB LRU cache with absolute expiration
- **Dynamic View Tracking** - Real-time view count updates
- **Search & Filter** - Find banks by name, currency, premium banking
- **Responsive Design** - Mobile-friendly Bootstrap 5 interface
- **Rating History** - Track rating changes over time

### Phase 2 Enhancements 🆕

#### 📊 Data Visualization
- **Rating Charts** - Interactive Chart.js-powered line charts showing rating trends over time
- **View Count History** - Track bank profile views with statistics (total, average, trend indicators)
- **Time Range Filters** - View data for 7, 30, or 90 days (charts), 30, 90, or 365 days (ratings)
- **Beautiful Skeletons** - Loading states with animated skeletons for better UX

#### 🎨 Theming System
- **Custom Bank Themes** - Each bank can define colors, fonts, and accents
- **Dynamic CSS Variables** - Theme applied automatically per bank profile
- **Light/Dark Mode Toggle** - Theme switcher with persistent preference
- **Animated Backgrounds** - SVG animations with gradients and floating elements

#### 🌍 Internationalization (i18n)
- **5 Languages Supported**: 
  - 🇺🇸 English (US) - en-US
  - 🇬🇧 English (UK) - en-GB
  - 🇩🇪 German - de-DE
  - 🇫🇷 French - fr-FR
  - 🇪🇸 Spanish - es-ES
- **Language Selector** - Dropdown component with flag icons
- **Resource-Based Translation** - JSON resource files for easy content management
- **Cookie-Based Persistence** - Language preference saved across sessions

#### 🗺️ Country-Based Routing
- **New URL Pattern**: `/{countryCode}/{bankCode}` (e.g., `/uk/bank-alpha`, `/us/bank-beta`)
- **Backward Compatibility**: Legacy `/bank/{bankCode}` URLs auto-redirect to country URLs
- **Country Metadata**: Flag, currency, and language info displayed on bank profiles
- **Breadcrumb Navigation**: Shows country context in navigation path
- **Automatic Language Detection**: Bank's default language set based on country

## Technology Stack

- **.NET 10** - Latest .NET framework
- **Blazor Server** - Interactive server-side rendering with InteractiveServer rendermode
- **SQL Server** - Entity Framework Core with migrations
- **Bootstrap 5** - Responsive UI framework
- **Font Awesome** - Icon library
- **Chart.js** - Interactive charts via PSC.Blazor.Components.Chartjs
- **SVG Animations** - Custom animated backgrounds with CSS keyframes

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server or LocalDB
- Modern web browser

## Getting Started

### 1. Clone the Repository

\\\ash
git clone <repository-url>
cd bank
\\\

### 2. Configure Database

Update the connection string in \ppsettings.json\ if needed:

\\\json
{
  "ConnectionStrings": {
    "BankDatabase": "Server=(localdb)\\mssqllocaldb;Database=BankProfiles;Trusted_Connection=True;..."
  }
}
\\\

### 3. Apply Migrations

\\\ash
dotnet ef database update
\\\

### 4. Seed Sample Data

The database is automatically seeded with:
- 5 rating criteria (Service, Fees, Convenience, Digital Services, Customer Support)

To add sample bank ratings, run the seed script or add them manually.

### 5. Run the Application

\\\ash
dotnet run
\\\

Navigate to \https://localhost:5001\ or \http://localhost:5000\

## Project Structure

```
BankProfiles.Web/
├── Components/
│   ├── Layout/          # MainLayout, NavMenu
│   ├── Pages/           # All application pages
│   │   ├── BankDetail.razor    # Main bank profile (/{country}/{bank})
│   │   ├── BankRedirect.razor  # Legacy URL redirector
│   │   ├── Banks.razor         # Bank listing
│   │   ├── Home.razor          # Homepage
│   │   └── Ratings.razor       # Rating comparison
│   └── Shared/          # Reusable components
│       ├── AnimatedBackground.razor  # SVG animations
│       ├── BankCard.razor            # Bank card display
│       ├── ChartSkeleton.razor       # Loading skeleton
│       ├── CreditCard.razor          # 3D credit card
│       ├── GlassWindow.razor         # Glass morphism panels
│       ├── LanguageSelector.razor    # Language switcher
│       ├── RatingChart.razor         # Rating trend chart
│       ├── ThemeToggle.razor         # Light/dark mode
│       ├── ViewsChart.razor          # View history chart
│       └── ... (skeletons and displays)
├── Data/
│   ├── Entities/        # EF Core entities
│   └── BankDbContext.cs # Database context
├── Models/              # JSON models
│   ├── BankProfile.cs   # Bank data model (with CountryCode, DefaultLanguage)
│   ├── AnimationConfig.cs  # Animation configurations
│   ├── BankTheme.cs     # Theme definitions
│   └── CountryInfo.cs   # Country metadata
├── Services/            # Business logic services
│   ├── BankDataService.cs      # Bank data loading
│   ├── CacheManager.cs         # LRU cache
│   ├── ChartDataService.cs     # Chart data preparation
│   ├── CountryService.cs       # Country metadata
│   ├── LocalizationService.cs  # i18n service
│   ├── RatingService.cs        # Rating management
│   ├── ThemeService.cs         # Theme management
│   └── ViewCountService.cs     # View tracking
├── Resources/           # i18n resource files
│   ├── Strings.en-US.json
│   ├── Strings.en-GB.json
│   ├── Strings.de-DE.json
│   ├── Strings.fr-FR.json
│   └── Strings.es-ES.json
├── wwwroot/
│   ├── css/
│   │   └── site.css     # CSS with theme variables
│   ├── data/banks/      # Bank JSON files
│   └── images/          # Bank logos and assets
├── Program.cs           # Application entry point
└── appsettings.json     # Configuration
```

## Configuration

### Cache Settings

\\\json
"CacheSettings": {
  "SizeLimitMB": 500,
  "AbsoluteExpirationMinutes": 60,
  "EnableSizeLimit": true
}
\\\

### Rating Settings

\\\json
"RatingSettings": {
  "MinRating": 0,
  "MaxRating": 10,
  "EnableHistory": true,
  "HistorySnapshotIntervalHours": 24
}
\\\

## Adding a New Bank

1. Create a JSON file in \wwwroot/data/banks/\ (e.g., \ank-newbank.json\)
2. Follow the JSON schema (see existing files)
3. Add a corresponding Bank entry in the database:

\\\sql
INSERT INTO Banks (BankCode, ViewCount, CreatedDate)
VALUES ('bank-newbank', 0, GETUTCDATE())
\\\

4. Add ratings for the bank across all criteria

## Features Overview

### Pages

- **Home** - Featured banks and quick access
- **Banks** - Browse all banks with search/filter/sort
- **Bank Detail** - Comprehensive bank profile with ratings
- **Ratings** - Compare all banks side-by-side
- **Contacts** - Contact information
- **About** - Application information

### Services

- **BankDataService** - Loads and caches bank JSON data
- **RatingService** - Manages ratings and history
- **RatingHistoryService** - Tracks and retrieves historical ratings
- **ViewCountService** - Tracks profile views
- **CacheManager** - Custom 500MB LRU cache
- **ThemeService** - Manages theme variables and dark mode
- **LocalizationService** - Handles multi-language support
- **CountryService** - Provides country metadata
- **ChartDataService** - Prepares data for Chart.js visualizations

## Performance

- **Caching** reduces JSON file reads by 95%+
- **LRU eviction** keeps memory under 500MB
- **Absolute expiration** ensures fresh data hourly
- **Indexed queries** for fast database lookups

## Troubleshooting

### Database Connection Issues

Ensure SQL Server LocalDB is running:
\\\ash
sqllocaldb start mssqllocaldb
\\\

### Cache Not Working

Check \CacheSettings\ in \ppsettings.json\ and ensure \EnableSizeLimit\ is true.

### Banks Not Loading

Verify JSON files exist in \wwwroot/data/banks/\ and are valid JSON.

## License

This project is for demonstration purposes.

## Phase 2 Feature Details

### Country-Based Routing

Banks are now accessed via country-specific URLs:
- **New Format**: `/{countryCode}/{bankCode}` (e.g., `/uk/bank-alpha`, `/us/bank-beta`)
- **Legacy Format**: `/bank/{bankCode}` (auto-redirects to country URL)

Benefits:
- Clearer geographic context
- Automatic language detection based on country
- Better SEO with country-specific URLs
- Breadcrumb navigation shows country information

### Supported Languages

| Language | Code | Flag |
|----------|------|------|
| English (US) | en-US | 🇺🇸 |
| English (UK) | en-GB | 🇬🇧 |
| German | de-DE | 🇩🇪 |
| French | fr-FR | 🇫🇷 |
| Spanish | es-ES | 🇪🇸 |

Add new languages by:
1. Creating `Resources/Strings.{lang}.json` (e.g., `Strings.it-IT.json`)
2. Adding the language code to `SupportedLanguages` in `LocalizationService.cs`
3. Optionally adding country mapping in `CountryToLanguageMap`

### Theme System

Each bank can define custom theming:

```json
"theme": {
  "primaryColor": "#1a237e",
  "secondaryColor": "#3949ab",
  "fontFamily": "Roboto, sans-serif",
  "accentColor": "#ff6f00"
}
```

Themes are applied via CSS variables:
- `--primary-color`
- `--secondary-color`
- `--accent-color`
- `--font-family`

The ThemeToggle component allows switching between light and dark modes, with preference saved in localStorage.

### Chart Visualization

Two chart types are available:

1. **Rating Charts** (`RatingChart.razor`)
   - Shows rating trends over time for all criteria
   - Interactive time range filters (30/90/365 days)
   - Uses Chart.js line charts
   - Smooth animations and tooltips

2. **View Count Charts** (`ViewsChart.razor`)
   - Displays view history with statistics
   - Total views, average views per day, trend indicator
   - Time range filters (7/30/90 days)
   - Bar chart visualization

Both charts include skeleton loaders for better UX during data fetching.

### Animation System

Banks can define custom SVG animations:

```json
"animationConfig": {
  "animationType": "float",
  "gradientStops": [
    { "offset": "0%", "color": "#1a237e" },
    { "offset": "100%", "color": "#3949ab" }
  ],
  "balls": [
    { "centerX": 150, "centerY": 150, "radius": 100, "color": "#ff6f00" }
  ]
}
```

Animation types:
- `pulse` - Pulsing effect
- `wave` - Wave motion
- `rotate` - Rotation animation
- `float` - Floating effect

## Migration Guide

### Updating from Phase 1 to Phase 2

**1. Update Bank JSON Files**

Add new required fields to all bank JSON files:

```json
{
  "bankCode": "bank-alpha",
  "countryCode": "uk",           // NEW: Required
  "defaultLanguage": "en-GB",    // NEW: Optional
  "theme": { ... },              // NEW: Optional
  "animationConfig": { ... }     // NEW: Optional
}
```

**2. Update Links**

Old links using `/bank/{code}` will auto-redirect, but update your links to use the new format:

```razor
<!-- Old -->
<a href="/bank/bank-alpha">Alpha Bank</a>

<!-- New -->
<a href="/uk/bank-alpha">Alpha Bank</a>
```

**3. Install Chart.js Package**

If deploying to a new environment:

```bash
dotnet add package PSC.Blazor.Components.Chartjs
```

**4. No Database Changes Required**

Phase 2 doesn't change the database schema. Existing data works without migration.

## Support

For issues or questions, please contact: info@bankprofiles.com
