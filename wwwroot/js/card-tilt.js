/**
 * ═══════════════════════════════════════════════════════════
 * 3D TILT EFFECT FOR GLASS CARDS
 * Extracted from pure-css-effects-showcase
 * Adapted for Blazor with dynamic element support
 * ═══════════════════════════════════════════════════════════
 */

(function() {
  'use strict';

  /**
   * Initialize 3D tilt effect on a card element
   * @param {HTMLElement} card - The card element to apply tilt to
   */
  function initTilt(card) {
    // Skip if already initialized
    if (card.dataset.tiltInitialized === 'true') {
      return;
    }

    card.dataset.tiltInitialized = 'true';

    // Mouse move handler for 3D rotation
    const handleMouseMove = (e) => {
      const r = card.getBoundingClientRect();
      const x = (e.clientX - r.left) / r.width - 0.5;
      const y = (e.clientY - r.top) / r.height - 0.5;
      
      // Apply 3D tilt with scale
      card.style.transform = `perspective(700px) rotateY(${x * 14}deg) rotateX(${-y * 14}deg) translateY(-4px) scale(1.02)`;
    };

    // Mouse leave handler for smooth reset
    const handleMouseLeave = () => {
      // Smooth transition back to normal state
      card.style.transition = 'transform 0.6s cubic-bezier(0.23, 1, 0.32, 1), box-shadow 0.4s cubic-bezier(0.23, 1, 0.32, 1), background 0.3s ease, border-color 0.3s ease';
      card.style.transform = 'translateY(0) scale(1)';
      
      // Remove transition override after animation completes
      setTimeout(() => {
        card.style.transition = '';
      }, 600);
    };

    // Mouse enter handler for immediate response
    const handleMouseEnter = () => {
      card.style.transition = 'transform 0.12s ease, box-shadow 0.4s cubic-bezier(0.23, 1, 0.32, 1), background 0.3s ease, border-color 0.3s ease';
    };

    // Attach event listeners
    card.addEventListener('mousemove', handleMouseMove);
    card.addEventListener('mouseleave', handleMouseLeave);
    card.addEventListener('mouseenter', handleMouseEnter);

    // Store cleanup function for potential removal
    card._tiltCleanup = () => {
      card.removeEventListener('mousemove', handleMouseMove);
      card.removeEventListener('mouseleave', handleMouseLeave);
      card.removeEventListener('mouseenter', handleMouseEnter);
      // Reset inline styles
      card.style.transform = '';
      card.style.transition = '';
      delete card.dataset.tiltInitialized;
      delete card._tiltCleanup;
    };
  }

  /**
   * Initialize tilt on all cards with data-tilt attribute
   */
  function initAllTiltCards() {
    const cards = document.querySelectorAll('[data-tilt]');
    cards.forEach(initTilt);
  }

  /**
   * Mutation observer to handle dynamically added cards (Blazor)
   */
  const observer = new MutationObserver((mutations) => {
    mutations.forEach((mutation) => {
      mutation.addedNodes.forEach((node) => {
        // Check if the added node itself has data-tilt
        if (node.nodeType === Node.ELEMENT_NODE) {
          if (node.hasAttribute('data-tilt')) {
            initTilt(node);
          }
          // Check for descendants with data-tilt
          const tiltCards = node.querySelectorAll('[data-tilt]');
          tiltCards.forEach(initTilt);
        }
      });
    });
  });

  /**
   * Start observing for dynamic content
   */
  function startObserving() {
    observer.observe(document.body, {
      childList: true,
      subtree: true
    });
  }

  /**
   * Stop observing and cleanup
   */
  function stopObserving() {
    observer.disconnect();
  }

  /**
   * Initialize on DOM ready
   */
  function init() {
    if (document.readyState === 'loading') {
      document.addEventListener('DOMContentLoaded', () => {
        initAllTiltCards();
        startObserving();
      });
    } else {
      initAllTiltCards();
      startObserving();
    }
  }

  /**
   * Cleanup all tilt effects
   */
  function cleanup() {
    stopObserving();
    const cards = document.querySelectorAll('[data-tilt]');
    cards.forEach(card => {
      if (card._tiltCleanup) {
        card._tiltCleanup();
      }
    });
  }

  // Export to global scope for Blazor interop if needed
  window.CardTilt = {
    init,
    cleanup,
    initCard: initTilt,
    initAll: initAllTiltCards
  };

  // Auto-initialize
  init();
})();
