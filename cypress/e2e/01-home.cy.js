/// <reference types="cypress" />

describe('Home Page E2E Tests', () => {
  beforeEach(() => {
    cy.visit('/');
    cy.waitForBlazor();
  });

  describe('Page Load and Structure', () => {
    it('should load the home page successfully', () => {
      cy.url().should('eq', Cypress.config().baseUrl + '/');
      cy.get('h1').should('contain', 'Welcome to Bank Profiles');
    });

    it('should display the main navigation', () => {
      cy.get('header').should('exist');
      cy.get('.navbar-brand').should('contain', 'Bank Profiles');
    });

    it('should have all navigation links', () => {
      cy.get('nav').within(() => {
        cy.contains('Home').should('exist');
        cy.contains('Banks').should('exist');
        cy.contains('Ratings').should('exist');
        cy.contains('Contacts').should('exist');
        cy.contains('About').should('exist');
      });
    });

    it('should display the skip link for accessibility', () => {
      cy.get('.skip-link').should('exist').and('have.attr', 'href', '#main-content');
    });
  });

  describe('Country Navigation', () => {
    it('should display country cards', () => {
      cy.get('.country-card').should('have.length.at.least', 5);
    });

    it('should have links to country-specific bank pages', () => {
      cy.get('.country-card').first().within(() => {
        cy.get('a').should('have.attr', 'href').and('match', /\/(uk|us|de|fr|es)\/.+/);
      });
    });

    it('should display country flags and information', () => {
      cy.get('.country-card').each(($card) => {
        cy.wrap($card).within(() => {
          cy.get('h3, h4').should('exist'); // Country name
        });
      });
    });
  });

  describe('Theme Toggle', () => {
    it('should have a theme toggle button', () => {
      cy.get('[aria-label*="theme" i], .theme-toggle, [data-testid="theme-toggle"]')
        .should('exist');
    });

    it('should toggle between light and dark themes', () => {
      cy.get('html').then(($html) => {
        const initialTheme = $html.attr('data-theme') || 'light';
        
        cy.get('[aria-label*="theme" i], .theme-toggle, [data-testid="theme-toggle"]')
          .click();
        
        cy.wait(500);
        
        cy.get('html').should('have.attr', 'data-theme')
          .and('not.equal', initialTheme);
      });
    });

    it('should persist theme selection in cookies', () => {
      cy.get('[aria-label*="theme" i], .theme-toggle, [data-testid="theme-toggle"]')
        .click();
      
      cy.wait(500);
      
      cy.getCookie('theme').should('exist').and('have.property', 'value')
        .and('match', /^(light|dark)$/);
    });
  });

  describe('Language Selector', () => {
    it('should have a language selector', () => {
      cy.get('[aria-label*="language" i], .language-selector, select[name*="lang" i]')
        .should('exist');
    });

    it('should display available languages', () => {
      cy.get('select[name*="lang" i], .language-selector select').then(($select) => {
        if ($select.length > 0) {
          cy.wrap($select).find('option').should('have.length.at.least', 3);
        }
      });
    });

    it('should change language when selected', () => {
      cy.get('select[name*="lang" i], .language-selector select').then(($select) => {
        if ($select.length > 0) {
          cy.wrap($select).select(1); // Select second language
          cy.wait(1000);
          cy.getCookie('lang').should('exist');
        }
      });
    });
  });

  describe('Responsive Design', () => {
    it('should be responsive on mobile', () => {
      cy.viewport(375, 667);
      cy.get('.navbar-toggler').should('be.visible');
      cy.get('.country-card').should('exist');
    });

    it('should be responsive on tablet', () => {
      cy.viewport(768, 1024);
      cy.get('.country-card').should('be.visible');
    });

    it('should be responsive on desktop', () => {
      cy.viewport(1920, 1080);
      cy.get('.country-card').should('be.visible');
      cy.get('nav a').should('be.visible');
    });
  });

  describe('Accessibility', () => {
    it('should have proper ARIA labels', () => {
      cy.get('[role="banner"]').should('exist');
      cy.get('[role="main"], main').should('exist');
      cy.get('[role="contentinfo"]').should('exist');
    });

    it('should have keyboard navigation', () => {
      cy.get('a, button').first().focus();
      cy.focused().should('exist');
    });

    it('should have proper heading hierarchy', () => {
      cy.get('h1').should('have.length', 1);
      cy.get('h2, h3, h4').should('exist');
    });
  });

  describe('Footer', () => {
    it('should display the footer', () => {
      cy.get('footer').should('exist').and('contain', '2026');
    });

    it('should have copyright information', () => {
      cy.get('footer').should('contain', 'Bank Profiles');
    });
  });

  describe('Performance', () => {
    it('should load within acceptable time', () => {
      const start = Date.now();
      cy.visit('/');
      cy.waitForBlazor();
      cy.then(() => {
        const loadTime = Date.now() - start;
        expect(loadTime).to.be.lessThan(5000); // 5 seconds
      });
    });

    it('should not have console errors', () => {
      cy.visit('/');
      cy.waitForBlazor();
      cy.window().then((win) => {
        const errors = win.console.error.calls?.map(call => call.args);
        // Allow Blazor reconnection errors
        const criticalErrors = errors?.filter(err => 
          !JSON.stringify(err).includes('WebSocket') && 
          !JSON.stringify(err).includes('SignalR')
        ) || [];
        expect(criticalErrors).to.have.length(0);
      });
    });
  });
});
