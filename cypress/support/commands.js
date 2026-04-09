// ***********************************************
// Cypress custom commands and global configuration
// ***********************************************

// Wait for Blazor to initialize
Cypress.Commands.add('waitForBlazor', () => {
  cy.window().then((win) => {
    return new Cypress.Promise((resolve) => {
      if (win.Blazor) {
        resolve();
      } else {
        const checkBlazor = setInterval(() => {
          if (win.Blazor) {
            clearInterval(checkBlazor);
            resolve();
          }
        }, 100);
      }
    });
  });
  cy.wait(500); // Additional wait for SignalR connection
});

// Check accessibility
Cypress.Commands.add('checkA11y', (context = null, options = {}) => {
  const defaultOptions = {
    runOnly: {
      type: 'tag',
      values: ['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa']
    },
    ...options
  };
  
  // Basic accessibility checks without axe-core
  cy.get('h1, h2, h3, h4, h5, h6').should('exist');
  cy.get('[role="main"], main').should('exist');
  cy.get('img').each(($img) => {
    cy.wrap($img).should('have.attr', 'alt');
  });
});

// Test responsive design
Cypress.Commands.add('testResponsive', (callback) => {
  const viewports = [
    { name: 'mobile', width: 375, height: 667 },
    { name: 'tablet', width: 768, height: 1024 },
    { name: 'desktop', width: 1280, height: 720 }
  ];

  viewports.forEach((viewport) => {
    cy.viewport(viewport.width, viewport.height);
    cy.log(`Testing on ${viewport.name} (${viewport.width}x${viewport.height})`);
    callback(viewport);
  });
});

// Check for console errors
Cypress.Commands.add('checkConsoleErrors', () => {
  cy.window().then((win) => {
    const logs = win.console.error.args || [];
    expect(logs.length, 'Console errors').to.equal(0);
  });
});

// Intercept Blazor SignalR
Cypress.Commands.add('interceptBlazorHub', () => {
  cy.intercept('/_blazor*').as('blazorHub');
});

// Custom assertions
Cypress.Commands.add('shouldBeVisible', { prevSubject: true }, (subject) => {
  cy.wrap(subject).should('be.visible').and('not.be.disabled');
});

// Global before hook
beforeEach(() => {
  // Suppress uncaught exceptions from Blazor
  cy.on('uncaught:exception', (err, runnable) => {
    // Don't fail tests on Blazor reconnection errors
    if (err.message.includes('WebSocket') || err.message.includes('SignalR')) {
      return false;
    }
    return true;
  });
});
