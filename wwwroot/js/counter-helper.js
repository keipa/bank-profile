/**
 * Counter animation helper for metric tiles.
 * Animates numeric text from start to end using ease-out timing.
 */
(function () {
  'use strict';

  function easeOutCubic(t) {
    return 1 - Math.pow(1 - t, 3);
  }

  function formatNumber(value, decimals, useGrouping) {
    const formatter = new Intl.NumberFormat(undefined, {
      minimumFractionDigits: decimals,
      maximumFractionDigits: decimals,
      useGrouping: useGrouping
    });

    return formatter.format(value);
  }

  function formatCounterValue(value, options) {
    const prefix = options.prefix ?? '';
    const suffix = options.suffix ?? '';
    const decimals = Number.isFinite(options.decimals) ? options.decimals : 0;
    const useGrouping = options.useGrouping === true;
    const numberPart = formatNumber(value, decimals, useGrouping);

    return `${prefix}${numberPart}${suffix}`;
  }

  function animateValue(element, options) {
    if (!element || !options) {
      return;
    }

    const start = Number(options.start);
    const end = Number(options.end);
    const duration = Number(options.duration);

    if (!Number.isFinite(start) || !Number.isFinite(end)) {
      return;
    }

    const reduceMotion = window.matchMedia &&
      window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    if (reduceMotion || !Number.isFinite(duration) || duration <= 0) {
      element.textContent = formatCounterValue(end, options);
      return;
    }

    if (element.__counterAnimationFrame) {
      cancelAnimationFrame(element.__counterAnimationFrame);
    }

    const startedAt = performance.now();

    const tick = (now) => {
      const elapsed = now - startedAt;
      const progress = Math.min(elapsed / duration, 1);
      const eased = easeOutCubic(progress);
      const current = start + (end - start) * eased;

      element.textContent = formatCounterValue(current, options);

      if (progress < 1) {
        element.__counterAnimationFrame = requestAnimationFrame(tick);
        return;
      }

      element.textContent = formatCounterValue(end, options);
      element.__counterAnimationFrame = null;
    };

    element.__counterAnimationFrame = requestAnimationFrame(tick);
  }

  function stopAnimation(element) {
    if (element && element.__counterAnimationFrame) {
      cancelAnimationFrame(element.__counterAnimationFrame);
      element.__counterAnimationFrame = null;
    }
  }

  window.counterHelper = {
    animateValue,
    stopAnimation
  };
})();
