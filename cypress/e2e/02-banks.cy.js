/// <reference types="cypress" />

describe('Banks List Page E2E Tests', () => {
  beforeEach(() => {
    cy.visit('/banks');
    cy.waitForBlazor();
  });

  describe('Page Load and Structure', () => {
    it('should load the banks page successfully', () => {
      cy.url().should('include', '/banks');
      cy.get('h1, h2').should('contain.text', 'Banks');
    });

    it('should display bank cards', () => {
      cy.get('[class*="bank"], [class*="card"]').should('have.length.at.least', 3);
    });
  });

  describe('Bank Cards Display', () => {
    it('should display bank information', () => {
      cy.get('[class*="bank"], [class*="card"]').first().within(() => {
        cy.get('h3, h4, h5').should('exist'); // Bank name
      });
    });

    it('should have clickable bank cards', () => {
      cy.get('[class*="bank"], [class*="card"]').first().within(() => {
        cy.get('a').should('have.attr', 'href').and('include', '/');
      });
    });

    it('should display ratings if available', () => {
      cy.get('[class*="bank"], [class*="card"]').first().then(($card) => {
        // Check if rating exists
        cy.wrap($card).find('[class*="rating"], .stars, [class*="score"]').then(($rating) => {
          if ($rating.length > 0) {
            cy.wrap($rating).should('be.visible');
          }
        });
      });
    });
  });

  describe('Country Filtering', () => {
    it('should have country filter or grouping', () => {
      cy.get('body').then(($body) => {
        const hasCountryFilter = $body.find('[class*="country"], [class*="filter"], select').length > 0;
        const hasCountryGroup = $body.find('h2, h3, h4').text().match(/(UK|US|DE|FR|ES)/i);
        expect(hasCountryFilter || hasCountryGroup).to.be.true;
      });
    });

    it('should display banks grouped by country', () => {
      cy.get('h2, h3, h4').then(($headings) => {
        const text = $headings.text();
        const hasCountryHeadings = /UK|US|DE|FR|ES|United Kingdom|United States|Germany|France|Spain/i.test(text);
        expect(hasCountryHeadings).to.be.true;
      });
    });
  });

  describe('Search and Filter', () => {
    it('should have search or filter functionality', () => {
      cy.get('input[type="search"], input[type="text"], input[placeholder*="search" i]')
        .then(($input) => {
          if ($input.length > 0) {
            cy.wrap($input).should('be.visible');
          }
        });
    });
  });

  describe('Navigation', () => {
    it('should navigate to individual bank pages', () => {
      cy.get('[class*="bank"], [class*="card"]').first().within(() => {
        cy.get('a').first().click();
      });
      
      cy.waitForBlazor();
      cy.url().should('match', /\/(uk|us|de|fr|es)\/[a-z-]+/);
    });

    it('should have back navigation capability', () => {
      cy.get('[class*="bank"], [class*="card"]').first().find('a').first().click();
      cy.waitForBlazor();
      cy.go('back');
      cy.url().should('include', '/banks');
    });
  });

  describe('Responsive Design', () => {
    it('should display cards in grid on desktop', () => {
      cy.viewport(1280, 720);
      cy.get('[class*="bank"], [class*="card"]').should('be.visible');
    });

    it('should stack cards on mobile', () => {
      cy.viewport(375, 667);
      cy.get('[class*="bank"], [class*="card"]').should('be.visible');
    });

    it('should have responsive layout on tablet', () => {
      cy.viewport(768, 1024);
      cy.get('[class*="bank"], [class*="card"]').should('be.visible');
    });
  });

  describe('Accessibility', () => {
    it('should have proper heading structure', () => {
      cy.get('h1').should('have.length.at.least', 1);
      cy.get('h2, h3, h4').should('exist');
    });

    it('should have accessible links', () => {
      cy.get('a').each(($link) => {
        cy.wrap($link).should('have.attr', 'href');
      });
    });

    it('should support keyboard navigation', () => {
      cy.get('a').first().focus();
      cy.focused().type('{enter}');
      cy.waitForBlazor();
      cy.url().should('not.include', '/banks');
    });
  });

  describe('Content Validation', () => {
    it('should display at least 3 banks', () => {
      cy.get('[class*="bank"], [class*="card"]').should('have.length.at.least', 3);
    });

    it('should display bank names', () => {
      cy.get('[class*="bank"], [class*="card"]').first().within(() => {
        cy.get('h3, h4, h5').should('not.be.empty');
      });
    });
  });

  describe('Performance', () => {
    it('should load within acceptable time', () => {
      const start = Date.now();
      cy.visit('/banks');
      cy.waitForBlazor();
      cy.then(() => {
        const loadTime = Date.now() - start;
        expect(loadTime).to.be.lessThan(5000);
      });
    });
  });
});
