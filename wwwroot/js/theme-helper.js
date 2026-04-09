// Theme helper functions for light/dark mode
// Can be called from Blazor via JS interop or directly from JavaScript

export function applyTheme(theme) {
    if (theme !== 'light' && theme !== 'dark') {
        console.warn('Invalid theme value:', theme);
        return false;
    }
    document.documentElement.setAttribute('data-theme', theme);
    return true;
}

export function readThemeFromCookie() {
    const cookies = document.cookie.split(';');
    for (let cookie of cookies) {
        const [name, value] = cookie.trim().split('=');
        if (name === 'theme') {
            return value;
        }
    }
    return null;
}

export function applyThemeFromCookie() {
    const theme = readThemeFromCookie();
    if (theme) {
        applyTheme(theme);
        return theme;
    }
    // Default to light theme
    applyTheme('light');
    return 'light';
}

export function getCurrentTheme() {
    return document.documentElement.getAttribute('data-theme') || 'light';
}

export function detectSystemPreference() {
    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        return 'dark';
    }
    return 'light';
}

// Apply theme immediately on page load to prevent flash
applyThemeFromCookie();

// Make functions available globally for Blazor JS interop
window.themeHelper = {
    applyTheme,
    readThemeFromCookie,
    applyThemeFromCookie,
    getCurrentTheme,
    detectSystemPreference
};
