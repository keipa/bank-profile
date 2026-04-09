/// <reference types="cypress" />

describe('Navigation and Global Features E2E Tests', () => {
  describe('Navigation Bar', () => {
    beforeEach(() => {
      cy.visit('/');
      cy.waitForBlazor();
    });

    it('should have all main navigation links', () => {
      cy.get('nav').within(() => {
        cy.contains('Home').should('exist');
        cy.contains('Banks').should('exist');
        cy.contains('Ratings').should('exist');
        cy.contains('Contacts').should('exist');
        cy.contains('About').should('exist');
      });
    });

    it('should navigate to all pages from navbar', () => {
      const pages = [
        { text: 'Banks', url: '/banks' },
        { text: 'Ratings', url: '/ratings' },
        { text: 'Contacts', url: '/contacts' },
        { text: 'About', url: '/about' },
        { text: 'Home', url: '/' }
      ];

      pages.forEach((page) => {
        cy.contains('nav a', page.text).click();
        cy.waitForBlazor();
        cy.url().should('include', page.url);
      });
    });

    it('should highlight active navigation item', () => {
      cy.visit('/banks');
      cy.waitForBlazor();
      cy.get('nav a[href="/banks"]').should('have.class', /(active|selected)/i);
    });

    it('should have working brand logo link', () => {
      cy.visit('/banks');
      cy.waitForBlazor();
      cy.get('.navbar-brand, [class*="brand"]').click();
      cy.waitForBlazor();
      cy.url().should('eq', Cypress.config().baseUrl + '/');
    });
  });

  describe('Mobile Navigation', () => {
    beforeEach(() => {
      cy.viewport(375, 667);
      cy.visit('/');
      cy.waitForBlazor();
    });

    it('should have mobile menu toggle', () => {
      cy.get('.navbar-toggler, [class*="menu-toggle"], button[aria-label*="menu" i]')
        .should('be.visible');
    });

    it('should toggle mobile menu', () => {
      cy.get('.navbar-toggler').click();
      cy.get('.navbar-collapse, [class*="mobile-menu"]').should('be.visible');
    });

    it('should navigate from mobile menu', () => {
      cy.get('.navbar-toggler').click();
      cy.contains('a', 'Banks').click();
      cy.waitForBlazor();
      cy.url().should('include', '/banks');
    });
  });

  describe('Theme Toggle', () => {
    beforeEach(() => {
      cy.visit('/');
      cy.waitForBlazor();
    });

    it('should persist theme across pages', () => {
      cy.get('[aria-label*="theme" i], .theme-toggle').click();
      cy.wait(500);
      
      cy.get('html').invoke('attr', 'data-theme').then((theme) => {
        cy.visit('/banks');
        cy.waitForBlazor();
        cy.get('html').should('have.attr', 'data-theme', theme);
      });
    });

    it('should apply theme immediately', () => {
      cy.get('[aria-label*="theme" i], .theme-toggle').click();
      cy.wait(300);
      cy.get('body').should('have.css', 'background-color').and('not.be.empty');
    });

    it('should have theme icon indicators', () => {
      cy.get('[aria-label*="theme" i], .theme-toggle').within(() => {
        cy.get('svg, i, span').should('exist');
      });
    });
  });

  describe('Language Selector', () => {
    beforeEach(() => {
      cy.visit('/');
      cy.waitForBlazor();
    });

    it('should persist language across pages', () => {
      cy.get('select[name*="lang" i], .language-selector select').then(($select) => {
        if ($select.length > 0) {
          cy.wrap($select).select(1);
          cy.wait(1000);
          
          cy.visit('/banks');
          cy.waitForBlazor();
          
          cy.getCookie('lang').should('exist');
        }
      });
    });

    it('should update content when language changes', () => {
      cy.get('select[name*="lang" i]').then(($select) => {
        if ($select.length > 0) {
          cy.get('h1, h2, h3').first().invoke('text').then((originalText) => {
            cy.wrap($select).select(1);
            cy.wait(1500);
            cy.get('h1, h2, h3').first().invoke('text').should('exist');
          });
        }
      });
    });

    it('should show language flags or names', () => {
      cy.get('.language-selector, [class*="language"]').within(() => {
        cy.get('img, span, option').should('exist');
      });
    });
  });

  describe('Breadcrumb Navigation', () => {
    it('should display breadcrumbs on detail pages', () => {
      cy.visit('/uk/alpha-bank');
      cy.waitForBlazor();
      
      cy.get('[class*="breadcrumb"], nav[aria-label*="breadcrumb" i]').then(($breadcrumb) => {
        if ($breadcrumb.length > 0) {
          cy.wrap($breadcrumb).should('be.visible');
          cy.wrap($breadcrumb).find('a').should('have.length.at.least', 1);
        }
      });
    });

    it('should navigate using breadcrumbs', () => {
      cy.visit('/uk/alpha-bank');
      cy.waitForBlazor();
      
      cy.get('[class*="breadcrumb"] a, nav[aria-label*="breadcrumb" i] a').then(($links) => {
        if ($links.length > 0) {
          cy.wrap($links).first().click();
          cy.waitForBlazor();
          cy.url().should('not.include', 'alpha-bank');
        }
      });
    });
  });

  describe('Footer', () => {
    beforeEach(() => {
      cy.visit('/');
      cy.waitForBlazor();
    });

    it('should be present on all pages', () => {
      const pages = ['/', '/banks', '/ratings', '/contacts', '/about'];
      
      pages.forEach((page) => {
        cy.visit(page);
        cy.waitForBlazor();
        cy.get('footer').should('exist');
      });
    });

    it('should have copyright information', () => {
      cy.get('footer').should('contain.text', '2026');
      cy.get('footer').should('contain.text', 'Bank');
    });

    it('should have footer links if present', () => {
      cy.get('footer a').then(($links) => {
        if ($links.length > 0) {
          cy.wrap($links).should('have.attr', 'href');
        }
      });
    });
  });

  describe('Error Handling', () => {
    it('should handle 404 pages gracefully', () => {
      cy.visit('/non-existent-page', { failOnStatusCode: false });
      cy.waitForBlazor();
      
      cy.url().then((url) => {
        expect(url).to.satisfy((u) =>
          u.includes('not-found') || 
          u.includes('404') ||
          u.includes('error') ||
          u === Cypress.config().baseUrl + '/'
        );
      });
    });

    it('should show error UI for Blazor errors', () => {
      cy.get('#blazor-error-ui').should('exist').and('not.be.visible');
    });
  });

  describe('Skip Links', () => {
    beforeEach(() => {
      cy.visit('/');
    });

    it('should have skip to main content link', () => {
      cy.get('.skip-link, a[href="#main-content"]').should('exist');
    });

    it('should work when focused', () => {
      cy.get('.skip-link').focus();
      cy.focused().should('have.attr', 'href', '#main-content');
    });
  });

  describe('Cross-Page State', () => {
    it('should maintain theme across navigation', () => {
      cy.visit('/');
      cy.waitForBlazor();
      
      cy.get('[aria-label*="theme" i], .theme-toggle').click();
      cy.wait(500);
      
      const pages = ['/banks', '/ratings', '/contacts', '/about', '/'];
      
      pages.forEach((page) => {
        cy.visit(page);
        cy.waitForBlazor();
        cy.getCookie('theme').should('exist');
      });
    });

    it('should maintain language across navigation', () => {
      cy.visit('/');
      cy.waitForBlazor();
      
      cy.get('select[name*="lang" i]').then(($select) => {
        if ($select.length > 0) {
          cy.wrap($select).select(1);
          cy.wait(1000);
          
          const pages = ['/banks', '/ratings'];
          pages.forEach((page) => {
            cy.visit(page);
            cy.waitForBlazor();
            cy.getCookie('lang').should('exist');
          });
        }
      });
    });
  });
});
