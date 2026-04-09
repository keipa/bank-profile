# GlassWindow Component

A reusable glass morphism panel component with blur effects and transparency that adapts to light/dark themes.

## Usage

```razor
@using BankProfiles.Web.Components.Shared

<!-- Basic usage -->
<GlassWindow>
    <p>Your content here</p>
</GlassWindow>

<!-- With title -->
<GlassWindow Title="Dashboard">
    <p>Your content here</p>
</GlassWindow>

<!-- With different elevation levels -->
<GlassWindow Elevation="subtle">
    <p>Subtle glass effect</p>
</GlassWindow>

<GlassWindow Elevation="medium">
    <p>Medium glass effect (default)</p>
</GlassWindow>

<GlassWindow Elevation="strong">
    <p>Strong glass effect</p>
</GlassWindow>

<!-- With custom CSS classes -->
<GlassWindow CssClass="my-3 p-4" Title="Custom Styled">
    <p>Your content here</p>
</GlassWindow>
```

## Parameters

- **ChildContent** (`RenderFragment?`) - Content to display inside the glass window
- **Title** (`string?`) - Optional header title. When provided, displays a header bar
- **Elevation** (`string`) - Glass effect intensity: "subtle", "medium" (default), "strong"
- **CssClass** (`string?`) - Additional CSS classes to apply to the component

## Elevation Levels

### Subtle
- Blur: 5px
- Lighter border
- Smaller shadow
- Best for nested or secondary elements

### Medium (Default)
- Blur: 10px
- Standard border
- Medium shadow
- Best for primary content areas

### Strong
- Blur: 20px
- Stronger border
- Larger shadow
- Best for modal overlays or hero sections

## Theme Support

The component automatically adapts to light and dark themes:

**Light Theme:**
- Background: `rgba(255, 255, 255, 0.1)` to `0.15`
- Border: Semi-transparent white
- Shadow: Light blue-gray

**Dark Theme:**
- Background: `rgba(0, 0, 0, 0.1)` to `0.3`
- Border: Semi-transparent white
- Shadow: Black

## Browser Support

- Modern browsers with `backdrop-filter` support (Chrome, Edge, Safari, Firefox)
- Automatic fallback for older browsers (uses solid background instead of blur)

## CSS Files

The component requires:
- `wwwroot/css/glass-effects.css` - Glass morphism styles
- `wwwroot/css/theme-variables.css` - Theme color variables

These are automatically imported via `wwwroot/app.css`.
