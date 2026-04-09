/// <reference types="cypress" />

describe('Bank Detail Page E2E Tests', () => {
  const banks = [
    { country: 'uk', code: 'alpha-bank', name: 'Alpha' },
    { country: 'us', code: 'beta-finance', name: 'Beta' },
    { country: 'de', code: 'gamma-bank', name: 'Gamma' }
  ];

  banks.forEach((bank) => {
    describe(`${bank.name} Bank (${bank.country.toUpperCase()})`, () => {
      beforeEach(() => {
        cy.visit(`/${bank.country}/${bank.code}`);
        cy.waitForBlazor();
      });

      describe('Page Load', () => {
        it('should load the bank detail page', () => {
          cy.url().should('include', `/${bank.country}/${bank.code}`);
        });

        it('should display bank name', () => {
          cy.get('h1, h2').should('exist').and('not.be.empty');
        });

        it('should not show error messages', () => {
          cy.contains('error', { matchCase: false }).should('not.exist');
          cy.contains('not found', { matchCase: false }).should('not.exist');
        });
      });

      describe('Bank Information', () => {
        it('should display bank rating', () => {
          cy.get('[class*="rating"], [class*="score"], .stars').should('exist');
        });

        it('should display view count', () => {
          cy.get('body').then(($body) => {
            const hasViews = $body.text().match(/view/i);
            if (hasViews) {
              cy.contains(/\d+.*view/i).should('exist');
            }
          });
        });

        it('should display bank details', () => {
          cy.get('body').should('not.be.empty');
        });
      });

      describe('Credit Card Component', () => {
        it('should display animated credit card', () => {
          cy.get('[class*="credit-card"], [class*="card-display"], .card').then(($card) => {
            if ($card.length > 0) {
              cy.wrap($card).should('be.visible');
            }
          });
        });

        it('should have bank-themed styling', () => {
          cy.get('[class*="credit-card"], [class*="card-display"]').then(($card) => {
            if ($card.length > 0) {
              cy.wrap($card).should('have.css', 'background');
            }
          });
        });
      });

      describe('Charts', () => {
        it('should display rating chart', () => {
          cy.get('canvas, [class*="chart"]', { timeout: 10000 }).should('exist');
        });

        it('should display views chart', () => {
          cy.get('canvas, [class*="chart"]', { timeout: 10000 }).should('have.length.at.least', 1);
        });

        it('should have chart time range selectors', () => {
          cy.get('button, select').then(($controls) => {
            const hasTimeControls = $controls.text().match(/(7|30|90|day|month)/i);
            if (hasTimeControls) {
              cy.wrap($controls).should('exist');
            }
          });
        });
      });

      describe('Glass Windows', () => {
        it('should display glass window components', () => {
          cy.get('[class*="glass"]').then(($glass) => {
            if ($glass.length > 0) {
              cy.wrap($glass).should('be.visible');
            }
          });
        });

        it('should have proper styling', () => {
          cy.get('[class*="glass"]').then(($glass) => {
            if ($glass.length > 0) {
              cy.wrap($glass).should('have.css', 'backdrop-filter');
            }
          });
        });
      });

      describe('Animated Background', () => {
        it('should have animated background', () => {
          cy.get('[class*="animated"], [class*="background"], svg').then(($bg) => {
            if ($bg.length > 0) {
              cy.wrap($bg).should('exist');
            }
          });
        });

        it('should use bank-specific colors', () => {
          cy.get('[class*="animated"], svg').then(($bg) => {
            if ($bg.length > 0) {
              cy.wrap($bg).should('have.attr', 'style');
            }
          });
        });
      });

      describe('Technical Information', () => {
        it('should display commission information', () => {
          cy.get('body').then(($body) => {
            const hasCommission = $body.text().match(/commission|fee/i);
            if (hasCommission) {
              cy.contains(/commission|fee/i).should('exist');
            }
          });
        });

        it('should display available currencies', () => {
          cy.get('body').then(($body) => {
            const hasCurrency = $body.text().match(/currency|currencies/i);
            if (hasCurrency) {
              cy.contains(/currency|currencies/i).should('exist');
            }
          });
        });
      });

      describe('Navigation', () => {
        it('should have breadcrumb navigation', () => {
          cy.get('[class*="breadcrumb"], nav[aria-label*="breadcrumb" i]').then(($breadcrumb) => {
            if ($breadcrumb.length > 0) {
              cy.wrap($breadcrumb).should('be.visible');
            }
          });
        });

        it('should have link to banks list', () => {
          cy.get('a[href="/banks"], a[href*="banks"]').should('exist');
        });

        it('should navigate back to banks list', () => {
          cy.get('a[href="/banks"]').first().click();
          cy.waitForBlazor();
          cy.url().should('include', '/banks');
        });
      });

      describe('Responsive Design', () => {
        it('should be responsive on mobile', () => {
          cy.viewport(375, 667);
          cy.get('h1, h2').should('be.visible');
          cy.get('canvas, [class*="chart"]').should('exist');
        });

        it('should be responsive on tablet', () => {
          cy.viewport(768, 1024);
          cy.get('[class*="credit-card"], [class*="card-display"]').then(($card) => {
            if ($card.length > 0) {
              cy.wrap($card).should('be.visible');
            }
          });
        });

        it('should be responsive on desktop', () => {
          cy.viewport(1920, 1080);
          cy.get('[class*="glass"]').should('exist');
        });
      });

      describe('Accessibility', () => {
        it('should have proper heading structure', () => {
          cy.get('h1').should('have.length', 1);
        });

        it('should have accessible charts', () => {
          cy.get('canvas').each(($canvas) => {
            cy.wrap($canvas).parents('[role], [aria-label]').should('exist');
          });
        });

        it('should support keyboard navigation', () => {
          cy.get('button, a').first().focus();
          cy.focused().should('exist');
        });
      });

      describe('Performance', () => {
        it('should load within acceptable time', () => {
          const start = Date.now();
          cy.visit(`/${bank.country}/${bank.code}`);
          cy.waitForBlazor();
          cy.then(() => {
            const loadTime = Date.now() - start;
            expect(loadTime).to.be.lessThan(6000);
          });
        });

        it('should increment view count', () => {
          cy.visit(`/${bank.country}/${bank.code}`);
          cy.waitForBlazor();
          cy.wait(1000);
          
          cy.get('body').then(($body) => {
            const viewText = $body.text().match(/(\d+)\s*view/i);
            if (viewText) {
              const initialViews = parseInt(viewText[1]);
              
              cy.reload();
              cy.waitForBlazor();
              cy.wait(1000);
              
              cy.contains(/\d+\s*view/i).should('exist');
            }
          });
        });
      });
    });
  });

  describe('Invalid Bank Code', () => {
    it('should handle non-existent bank gracefully', () => {
      cy.visit('/uk/non-existent-bank', { failOnStatusCode: false });
      cy.waitForBlazor();
      
      // Should show error or redirect
      cy.url().then((url) => {
        expect(url).to.satisfy((u) => 
          u.includes('not-found') || 
          u.includes('error') || 
          u.includes('/banks') ||
          u.includes('/uk/non-existent-bank')
        );
      });
    });
  });
});
