/// <reference types="cypress" />

describe('Metric Suggestions Page E2E Tests', () => {
  it('should be reachable from navigation', () => {
    cy.visit('/');
    cy.waitForBlazor();

    cy.get('nav a[href*="metric-suggestions"]').first().click();
    cy.waitForBlazor();

    cy.url().should('include', '/metric-suggestions');
  });

  it('should load the metric suggestion form', () => {
    cy.visit('/metric-suggestions');
    cy.waitForBlazor();

    cy.url().should('include', '/metric-suggestions');
    cy.get('#categorySelector').should('exist');
    cy.get('#metricSelector').should('exist').and('be.disabled');
    cy.get('#explanation').should('exist');
    cy.get('button[type="submit"]').should('exist');
  });

  it('should load metric options when category is selected', () => {
    cy.visit('/metric-suggestions');
    cy.waitForBlazor();

    cy.get('#categorySelector option').should('have.length.greaterThan', 1);
    cy.get('#categorySelector').select(1, { force: true }).trigger('change');
    cy.wait(250);
    cy.get('#metricSelector option').should('have.length.greaterThan', 1);
  });
});
