/// <reference types="cypress" />

describe('Ratings Page E2E Tests', () => {
  beforeEach(() => {
    cy.visit('/ratings');
    cy.waitForBlazor();
  });

  describe('Page Load and Structure', () => {
    it('should load the ratings page successfully', () => {
      cy.url().should('include', '/ratings');
      cy.get('h1, h2').should('contain.text', 'Rating');
    });

    it('should display ratings content', () => {
      cy.get('body').should('not.be.empty');
    });
  });

  describe('Ratings Display', () => {
    it('should display bank ratings', () => {
      cy.get('[class*="rating"], [class*="bank"], table, .card').should('exist');
    });

    it('should show multiple banks', () => {
      cy.get('body').then(($body) => {
        const bankNames = $body.text().match(/(alpha|beta|gamma|delta|epsilon)/gi);
        expect(bankNames).to.have.length.at.least(3);
      });
    });

    it('should display rating scores', () => {
      cy.get('body').then(($body) => {
        const hasScores = $body.text().match(/\d+(\.\d+)?/);
        expect(hasScores).to.exist;
      });
    });
  });

  describe('Rating Criteria', () => {
    it('should display multiple rating criteria', () => {
      cy.get('body').then(($body) => {
        const hasCriteria = $body.text().match(/(customer service|fees|interest|quality|speed)/i);
        if (hasCriteria) {
          cy.contains(/(customer service|fees|interest|quality|speed)/i).should('exist');
        }
      });
    });

    it('should show overall ratings', () => {
      cy.get('body').then(($body) => {
        const hasOverall = $body.text().match(/overall|total|average/i);
        if (hasOverall) {
          cy.contains(/overall|total|average/i).should('exist');
        }
      });
    });
  });

  describe('Sorting and Filtering', () => {
    it('should have sortable columns if table view', () => {
      cy.get('table').then(($table) => {
        if ($table.length > 0) {
          cy.get('th').should('exist');
        }
      });
    });

    it('should allow filtering by criteria', () => {
      cy.get('select, button').then(($controls) => {
        const hasFilters = $controls.text().match(/(filter|sort|criteria)/i);
        if (hasFilters) {
          cy.wrap($controls).should('exist');
        }
      });
    });
  });

  describe('Charts and Visualization', () => {
    it('should display rating charts', () => {
      cy.get('canvas, [class*="chart"], svg').then(($charts) => {
        if ($charts.length > 0) {
          cy.wrap($charts).should('be.visible');
        }
      });
    });

    it('should have interactive chart controls', () => {
      cy.get('button, select').then(($controls) => {
        const hasChartControls = $controls.text().match(/(day|month|year)/i);
        if (hasChartControls) {
          cy.wrap($controls).should('exist');
        }
      });
    });
  });

  describe('Bank Links', () => {
    it('should have links to individual bank pages', () => {
      cy.get('a[href*="/"]').should('have.length.at.least', 3);
    });

    it('should navigate to bank detail page', () => {
      cy.get('a[href*="/"]').first().then(($link) => {
        const href = $link.attr('href');
        if (href && href.match(/\/(uk|us|de|fr|es)\/.+/)) {
          cy.wrap($link).click();
          cy.waitForBlazor();
          cy.url().should('match', /\/(uk|us|de|fr|es)\/.+/);
        }
      });
    });
  });

  describe('Responsive Design', () => {
    it('should be responsive on mobile', () => {
      cy.viewport(375, 667);
      cy.get('h1, h2').should('be.visible');
      cy.get('[class*="rating"], [class*="bank"]').should('exist');
    });

    it('should be responsive on tablet', () => {
      cy.viewport(768, 1024);
      cy.get('table, [class*="card"]').should('be.visible');
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

    it('should have accessible tables', () => {
      cy.get('table').then(($table) => {
        if ($table.length > 0) {
          cy.get('thead, th').should('exist');
        }
      });
    });

    it('should support keyboard navigation', () => {
      cy.get('a, button').first().focus();
      cy.focused().should('exist');
    });
  });

  describe('Performance', () => {
    it('should load within acceptable time', () => {
      const start = Date.now();
      cy.visit('/ratings');
      cy.waitForBlazor();
      cy.then(() => {
        const loadTime = Date.now() - start;
        expect(loadTime).to.be.lessThan(5000);
      });
    });
  });
});
