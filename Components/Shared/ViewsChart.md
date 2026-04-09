# ViewsChart Component

## Overview
The `ViewsChart` component displays view count history for a bank using an area chart with Chart.js.

## Features
- **Area Chart**: Displays view counts over time with a gradient fill
- **Time Range Selector**: Switch between 7, 30, and 90 day ranges
- **Statistics Display**:
  - Total views in the selected period
  - Average views per day
  - Trend indicator (↑ increasing, ↓ decreasing, → stable)
- **Responsive Design**: Glass-morphism styling consistent with the application theme

## Usage

### Basic Usage
```razor
<ViewsChart BankCode="BankA" />
```

### With Custom Time Range
```razor
<ViewsChart BankCode="BankA" TimeRange="90" />
```

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `BankCode` | `string` | (required) | The bank code to display view history for |
| `TimeRange` | `int` | `30` | Number of days to display (7, 30, or 90) |

## Dependencies

- **Chart.js**: Loaded via CDN (`chart.js@4.4.0`)
- **chart-helper.js**: Custom JavaScript helper for Chart.js integration
- **IChartDataService**: Service for retrieving view history data

## Data Source

The component retrieves data from the `ViewHistory` table via the `IChartDataService.GetViewHistoryDataAsync()` method, which:
- Filters by bank and date range
- Groups view counts by date
- Fills gaps with zeros for missing dates
- Returns properly formatted chart data

## Styling

The component uses:
- Glass-morphism effects with backdrop blur
- Responsive grid layout for statistics
- Smooth animations and transitions
- Color scheme consistent with Bootstrap primary colors

## Example Output

The chart displays:
- X-axis: Dates (formatted as "MM/dd")
- Y-axis: View counts (starting from 0)
- Data points with hover effects
- Filled area under the line with transparency
- Interactive tooltips showing exact values

## Implementation Notes

- Uses Chart.js directly via JavaScript interop instead of ChartJs.Blazor for better compatibility with .NET 10
- Automatically destroys and recreates chart when data changes
- Calculates trend by comparing first half vs second half of the period
- Trend threshold: ±10% change to show increasing/decreasing arrows
