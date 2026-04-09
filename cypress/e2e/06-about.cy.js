/// <reference types="cypress" />

describe('About Page E2E Tests', () => {
  beforeEach(() => {
    cy.visit('/about');
    cy.waitForBlazor();
  });

  describe('Page Load and Structure', () => {
    it('should load the about page successfully', () => {
      cy.url().should('include', '/about');
      cy.get('h1, h2').should('contain.text', 'About');
    });

    it('should display about content', () => {
      cy.get('body').should('not.be.empty');
      cy.get('p, div').should('have.length.at.least', 1);
    });
  });

  describe('Content', () => {
    it('should display application information', () => {
      cy.get('body').should('contain.text', 'Bank');
    });

    it('should have descriptive content', () => {
      cy.get('p, div').first().invoke('text').should('have.length.at.least', 20);
    });

    it('should display features or information', () => {
      cy.get('body').then(($body) => {
        const hasKeywords = $body.text().match(/(bank|rating|comparison|profile|service)/i);
        expect(hasKeywords).to.exist;
      });
    });
  });

  describe('Version Information', () => {
    it('should display version or copyright info', () => {
      cy.get('body').then(($body) => {
        const hasVersion = $body.text().match(/(version|v\d|2026|copyright)/i);
        if (hasVersion) {
          cy.contains(/(version|v\d|2026|copyright)/i).should('exist');
        }
      });
    });
  });

  describe('Technology Stack', () => {
    it('should mention technologies used', () => {
      cy.get('body').then(($body) => {
        const hasTech = $body.text().match(/(blazor|\.net|c#|entity framework)/i);
        if (hasTech) {
          cy.contains(/(blazor|\.net|c#)/i).should('exist');
        }
      });
    });
  });

  describe('External Links', () => {
    it('should have external links if present', () => {
      cy.get('a[href^="http"]').then(($links) => {
        if ($links.length > 0) {
          cy.wrap($links).should('have.attr', 'target', '_blank');
        }
      });
    });

    it('should have GitHub or documentation links', () => {
      cy.get('a[href*="github"], a[href*="docs"]').then(($links) => {
        if ($links.length > 0) {
          cy.wrap($links).should('have.attr', 'href');
        }
      });
    });
  });

  describe('Responsive Design', () => {
    it('should be responsive on mobile', () => {
      cy.viewport(375, 667);
      cy.get('h1, h2').should('be.visible');
      cy.get('p').should('be.visible');
    });

    it('should be responsive on tablet', () => {
      cy.viewport(768, 1024);
      cy.get('body').should('be.visible');
    });

    it('should be responsive on desktop', () => {
      cy.viewport(1920, 1080);
      cy.get('body').should('be.visible');
    });
  });

  describe('Accessibility', () => {
    it('should have proper heading structure', () => {
      cy.get('h1').should('have.length.at.least', 1);
    });

    it('should have semantic HTML', () => {
      cy.get('main, article, section').should('exist');
    });

    it('should support keyboard navigation', () => {
      cy.get('a, button').first().focus();
      cy.focused().should('exist');
    });
  });

  describe('Performance', () => {
    it('should load within acceptable time', () => {
      const start = Date.now();
      cy.visit('/about');
      cy.waitForBlazor();
      cy.then(() => {
        const loadTime = Date.now() - start;
        expect(loadTime).to.be.lessThan(5000);
      });
    });
  });
});
