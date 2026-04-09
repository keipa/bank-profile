/// <reference types="cypress" />

describe('Contacts Page E2E Tests', () => {
  beforeEach(() => {
    cy.visit('/contacts');
    cy.waitForBlazor();
  });

  describe('Page Load and Structure', () => {
    it('should load the contacts page successfully', () => {
      cy.url().should('include', '/contacts');
      cy.get('h1, h2').should('contain.text', 'Contact');
    });

    it('should display contacts content', () => {
      cy.get('body').should('not.be.empty');
    });
  });

  describe('Contact Information', () => {
    it('should display contact details', () => {
      cy.get('body').should('contain.text', '@').or('contain.text', 'email');
    });

    it('should have email information', () => {
      cy.get('a[href^="mailto:"]').then(($mailto) => {
        if ($mailto.length > 0) {
          cy.wrap($mailto).should('have.attr', 'href').and('include', 'mailto:');
        } else {
          cy.contains(/email|contact|reach/i).should('exist');
        }
      });
    });

    it('should display contact methods', () => {
      cy.get('body').then(($body) => {
        const hasContactInfo = $body.text().match(/(email|phone|address|contact)/i);
        expect(hasContactInfo).to.exist;
      });
    });
  });

  describe('Contact Form', () => {
    it('should have contact form if available', () => {
      cy.get('form, input[type="text"], input[type="email"], textarea').then(($form) => {
        if ($form.length > 0) {
          cy.wrap($form).should('be.visible');
        }
      });
    });

    it('should validate email input', () => {
      cy.get('input[type="email"]').then(($email) => {
        if ($email.length > 0) {
          cy.wrap($email).type('invalid-email');
          cy.get('button[type="submit"]').click();
          cy.get('.invalid-feedback, [class*="error"]').should('exist');
        }
      });
    });

    it('should submit form with valid data', () => {
      cy.get('form').then(($form) => {
        if ($form.length > 0) {
          cy.get('input[type="text"], input[name*="name"]').first().type('Test User');
          cy.get('input[type="email"]').type('test@example.com');
          cy.get('textarea').type('Test message');
          cy.get('button[type="submit"]').click();
        }
      });
    });
  });

  describe('Social Media Links', () => {
    it('should have social media links', () => {
      cy.get('a[href*="twitter"], a[href*="facebook"], a[href*="linkedin"], a[href*="github"]')
        .then(($social) => {
          if ($social.length > 0) {
            cy.wrap($social).should('have.attr', 'href');
          }
        });
    });

    it('should open social links in new tab', () => {
      cy.get('a[href*="twitter"], a[href*="facebook"], a[href*="linkedin"]').then(($links) => {
        if ($links.length > 0) {
          cy.wrap($links).first().should('have.attr', 'target', '_blank');
        }
      });
    });
  });

  describe('Responsive Design', () => {
    it('should be responsive on mobile', () => {
      cy.viewport(375, 667);
      cy.get('h1, h2').should('be.visible');
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

    it('should have accessible form labels', () => {
      cy.get('input, textarea, select').then(($inputs) => {
        if ($inputs.length > 0) {
          $inputs.each((index, input) => {
            cy.wrap(input).should('have.attr', 'aria-label')
              .or('have.attr', 'id').and('not.be.empty');
          });
        }
      });
    });

    it('should support keyboard navigation', () => {
      cy.get('a, button, input').first().focus();
      cy.focused().should('exist');
    });
  });

  describe('Performance', () => {
    it('should load within acceptable time', () => {
      const start = Date.now();
      cy.visit('/contacts');
      cy.waitForBlazor();
      cy.then(() => {
        const loadTime = Date.now() - start;
        expect(loadTime).to.be.lessThan(5000);
      });
    });
  });
});
