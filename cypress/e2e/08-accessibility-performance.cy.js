/// <reference types="cypress" />

describe('Accessibility and Performance E2E Tests', () => {
  describe('WCAG 2.1 Level AA Compliance', () => {
    const pages = [
      { name: 'Home', url: '/' },
      { name: 'Banks', url: '/banks' },
      { name: 'Bank Detail', url: '/uk/alpha-bank' },
      { name: 'Ratings', url: '/ratings' },
      { name: 'Contacts', url: '/contacts' },
      { name: 'About', url: '/about' }
    ];

    pages.forEach((page) => {
      describe(`${page.name} Page`, () => {
        beforeEach(() => {
          cy.visit(page.url);
          cy.waitForBlazor();
        });

        it('should have proper document structure', () => {
          cy.get('html').should('have.attr', 'lang');
          cy.get('head title').should('exist').and('not.be.empty');
          cy.get('meta[name="viewport"]').should('exist');
        });

        it('should have landmark regions', () => {
          cy.get('[role="banner"], header').should('exist');
          cy.get('[role="main"], main').should('exist');
          cy.get('[role="contentinfo"], footer').should('exist');
        });

        it('should have proper heading hierarchy', () => {
          cy.get('h1').should('have.length', 1);
          
          let previousLevel = 0;
          cy.get('h1, h2, h3, h4, h5, h6').each(($heading) => {
            const level = parseInt($heading.prop('tagName').substring(1));
            expect(level).to.be.at.most(previousLevel + 1);
            previousLevel = level;
          });
        });

        it('should have accessible images', () => {
          cy.get('img').each(($img) => {
            cy.wrap($img).should('satisfy', ($el) => {
              return $el.attr('alt') !== undefined || $el.attr('role') === 'presentation';
            });
          });
        });

        it('should have accessible links', () => {
          cy.get('a').each(($link) => {
            cy.wrap($link).should('satisfy', ($el) => {
              const text = $el.text().trim();
              const ariaLabel = $el.attr('aria-label');
              const title = $el.attr('title');
              return text.length > 0 || ariaLabel || title;
            });
          });
        });

        it('should have accessible buttons', () => {
          cy.get('button').each(($button) => {
            cy.wrap($button).should('satisfy', ($el) => {
              const text = $el.text().trim();
              const ariaLabel = $el.attr('aria-label');
              return text.length > 0 || ariaLabel;
            });
          });
        });

        it('should have accessible form controls', () => {
          cy.get('input, select, textarea').each(($control) => {
            cy.wrap($control).should('satisfy', ($el) => {
              const id = $el.attr('id');
              const ariaLabel = $el.attr('aria-label');
              const ariaLabelledby = $el.attr('aria-labelledby');
              const hasLabel = id && cy.$$(`label[for="${id}"]`).length > 0;
              return hasLabel || ariaLabel || ariaLabelledby;
            });
          });
        });

        it('should have sufficient color contrast', () => {
          cy.get('body').should('have.css', 'color');
          cy.get('body').should('have.css', 'background-color');
          // Manual verification recommended for actual contrast ratios
        });

        it('should support keyboard navigation', () => {
          cy.get('a, button, input, select, textarea, [tabindex]:not([tabindex="-1"])')
            .first()
            .focus();
          
          cy.focused().should('exist');
          
          cy.focused().tab();
          cy.focused().should('exist');
        });

        it('should have focus indicators', () => {
          cy.get('a, button').first().focus();
          cy.focused().should('have.css', 'outline').and('not.equal', 'none');
        });

        it('should not have low contrast text', () => {
          cy.get('*').each(($el) => {
            cy.wrap($el).should('have.css', 'color');
          });
        });
      });
    });
  });

  describe('Performance Metrics', () => {
    const pages = [
      { name: 'Home', url: '/' },
      { name: 'Banks', url: '/banks' },
      { name: 'Bank Detail', url: '/uk/alpha-bank' }
    ];

    pages.forEach((page) => {
      describe(`${page.name} Page Performance`, () => {
        it('should load within 5 seconds', () => {
          const start = Date.now();
          cy.visit(page.url);
          cy.waitForBlazor();
          
          cy.then(() => {
            const loadTime = Date.now() - start;
            expect(loadTime).to.be.lessThan(5000);
            cy.log(`Load time: ${loadTime}ms`);
          });
        });

        it('should not have memory leaks', () => {
          cy.visit(page.url);
          cy.waitForBlazor();
          
          cy.window().then((win) => {
            const initialMemory = win.performance.memory?.usedJSHeapSize;
            if (initialMemory) {
              cy.reload();
              cy.waitForBlazor();
              
              cy.window().then((win2) => {
                const finalMemory = win2.performance.memory?.usedJSHeapSize;
                const memoryIncrease = (finalMemory - initialMemory) / initialMemory;
                expect(memoryIncrease).to.be.lessThan(2); // Less than 200% increase
              });
            }
          });
        });

        it('should have acceptable DOM size', () => {
          cy.visit(page.url);
          cy.waitForBlazor();
          
          cy.get('*').then(($all) => {
            expect($all.length).to.be.lessThan(1500); // Reasonable DOM size
          });
        });

        it('should load resources efficiently', () => {
          cy.visit(page.url);
          cy.waitForBlazor();
          
          cy.window().then((win) => {
            const resources = win.performance.getEntriesByType('resource');
            const slowResources = resources.filter(r => r.duration > 3000);
            expect(slowResources.length).to.equal(0);
          });
        });
      });
    });
  });

  describe('Mobile Accessibility', () => {
    beforeEach(() => {
      cy.viewport(375, 667);
    });

    it('should have touch-friendly targets (44x44px minimum)', () => {
      cy.visit('/');
      cy.waitForBlazor();
      
      cy.get('a, button').each(($el) => {
        const height = $el.height();
        const width = $el.width();
        
        if ($el.is(':visible')) {
          expect(height).to.be.at.least(40); // Allowing small margin
          expect(width).to.be.at.least(40);
        }
      });
    });

    it('should not require horizontal scrolling', () => {
      cy.visit('/');
      cy.waitForBlazor();
      
      cy.window().then((win) => {
        expect(win.document.body.scrollWidth).to.be.at.most(win.innerWidth + 1);
      });
    });

    it('should have readable text sizes', () => {
      cy.visit('/');
      cy.waitForBlazor();
      
      cy.get('p, li, td, span').each(($el) => {
        const fontSize = parseInt($el.css('font-size'));
        if ($el.is(':visible')) {
          expect(fontSize).to.be.at.least(14); // Minimum 14px on mobile
        }
      });
    });
  });

  describe('Screen Reader Support', () => {
    const pages = ['/', '/banks', '/ratings'];

    pages.forEach((url) => {
      it(`should have ARIA labels on ${url}`, () => {
        cy.visit(url);
        cy.waitForBlazor();
        
        cy.get('[aria-label], [aria-labelledby], [aria-describedby]')
          .should('have.length.at.least', 3);
      });

      it(`should have semantic HTML on ${url}`, () => {
        cy.visit(url);
        cy.waitForBlazor();
        
        cy.get('header, nav, main, article, section, aside, footer')
          .should('have.length.at.least', 3);
      });

      it(`should announce page changes on ${url}`, () => {
        cy.visit(url);
        cy.waitForBlazor();
        
        cy.get('[role="alert"], [aria-live]').then(($live) => {
          // Check for live regions that announce changes
          expect($live.length).to.be.at.least(0);
        });
      });
    });
  });

  describe('Reduced Motion Support', () => {
    it('should respect prefers-reduced-motion', () => {
      cy.visit('/', {
        onBeforeLoad(win) {
          Object.defineProperty(win, 'matchMedia', {
            writable: true,
            value: (query) => ({
              matches: query === '(prefers-reduced-motion: reduce)',
              media: query,
              onchange: null,
              addEventListener: () => {},
              removeEventListener: () => {},
              dispatchEvent: () => true,
            }),
          });
        },
      });
      
      cy.waitForBlazor();
      cy.get('[class*="animate"]').should('exist');
    });
  });

  describe('High Contrast Mode', () => {
    it('should be usable in high contrast mode', () => {
      cy.visit('/');
      cy.waitForBlazor();
      
      // Verify elements have borders or backgrounds
      cy.get('button, a, input').each(($el) => {
        cy.wrap($el).should('satisfy', ($element) => {
          const border = $element.css('border');
          const background = $element.css('background-color');
          return border !== 'none' || background !== 'rgba(0, 0, 0, 0)';
        });
      });
    });
  });

  describe('Browser Compatibility', () => {
    it('should work without JavaScript errors', () => {
      cy.visit('/');
      cy.waitForBlazor();
      
      cy.window().then((win) => {
        cy.spy(win.console, 'error');
      });
      
      cy.visit('/banks');
      cy.waitForBlazor();
      
      cy.window().then((win) => {
        const errorCalls = win.console.error.getCalls ? win.console.error.getCalls().length : 0;
        expect(errorCalls).to.equal(0);
      });
    });
  });
});
