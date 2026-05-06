// Card Holo Effect — vanilla JS port of pokemon-card-holo-effect
// Uses animated gradient + sparkle GIF overlay + color-dodge blend mode
// Blazor JS interop: cardHolo.initialize(selector) / cardHolo.destroy(selector)

window.cardHolo = {
    _cards: new Map(),

    initialize(selector) {
        this.destroy(selector);
        const container = document.querySelector(selector);
        if (!container) return;

        const cards = container.querySelectorAll('.holo-card');
        if (cards.length === 0) return;

        // Create shared <style> element for pseudo-element overrides
        let styleEl = container.querySelector('.holo-hover-style');
        if (!styleEl) {
            styleEl = document.createElement('style');
            styleEl.className = 'holo-hover-style';
            container.appendChild(styleEl);
        }

        const state = { cards: [], styleEl, timers: new Map() };

        cards.forEach((card, idx) => {
            const cardId = card.dataset.holoId || `holo-${idx}-${Date.now()}`;
            card.dataset.holoId = cardId;

            const onMove = (e) => this._handleMove(e, card, cardId, styleEl);
            const onLeave = (e) => this._handleLeave(e, card, cardId, styleEl, state.timers);

            card.addEventListener('mousemove', onMove);
            card.addEventListener('touchmove', onMove, { passive: false });
            card.addEventListener('mouseout', onLeave);
            card.addEventListener('touchend', onLeave);
            card.addEventListener('touchcancel', onLeave);

            state.cards.push({ el: card, cardId, onMove, onLeave });
        });

        this._cards.set(selector, state);
    },

    _handleMove(e, card, cardId, styleEl) {
        let pos;
        if (e.type === 'touchmove') {
            e.preventDefault();
            const rect = card.getBoundingClientRect();
            pos = [e.touches[0].clientX - rect.left, e.touches[0].clientY - rect.top];
        } else {
            pos = [e.offsetX, e.offsetY];
        }

        const l = pos[0];
        const t = pos[1];
        const h = card.offsetHeight;
        const w = card.offsetWidth;
        const px = Math.abs(Math.floor((100 / w) * l) - 100);
        const py = Math.abs(Math.floor((100 / h) * t) - 100);
        const pa = (50 - px) + (50 - py);

        const lp = 50 + (px - 50) / 1.5;
        const tp = 50 + (py - 50) / 1.5;
        const pxSpark = 50 + (px - 50) / 7;
        const pySpark = 50 + (py - 50) / 7;
        const pOpc = 20 + Math.abs(pa) * 1.5;
        const ty = ((tp - 50) / 2) * -1;
        const tx = ((lp - 50) / 1.5) * 0.5;

        const tf = `rotateX(${ty}deg) rotateY(${tx}deg)`;

        card.classList.remove('holo-animated');
        card.style.transform = tf;

        // Use data attribute scoped styles for pseudo-elements
        const style = `
            .holo-card[data-holo-id="${cardId}"]:before { background-position: ${lp}% ${tp}%; }
            .holo-card[data-holo-id="${cardId}"]:after { background-position: ${pxSpark}% ${pySpark}%; opacity: ${pOpc / 100}; }
        `;
        styleEl.textContent = style;

        if (e.type === 'touchmove') {
            e.preventDefault();
        }
    },

    _handleLeave(e, card, cardId, styleEl, timers) {
        styleEl.textContent = '';
        card.style.transform = '';

        if (timers.has(cardId)) {
            clearTimeout(timers.get(cardId));
        }
        timers.set(cardId, setTimeout(() => {
            card.classList.add('holo-animated');
        }, 2500));
    },

    destroy(selector) {
        const state = this._cards.get(selector);
        if (!state) return;

        state.cards.forEach(({ el, onMove, onLeave }) => {
            el.removeEventListener('mousemove', onMove);
            el.removeEventListener('touchmove', onMove);
            el.removeEventListener('mouseout', onLeave);
            el.removeEventListener('touchend', onLeave);
            el.removeEventListener('touchcancel', onLeave);
            el.style.transform = '';
            el.classList.remove('holo-animated');
        });

        if (state.styleEl) {
            state.styleEl.textContent = '';
        }

        state.timers.forEach(t => clearTimeout(t));
        this._cards.delete(selector);
    }
};
