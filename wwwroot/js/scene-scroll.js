// Scene-based scroll tracking for bank detail page
// Uses IntersectionObserver for efficient scene change detection

window.sceneScroll = {
    _observer: null,
    _dotNetRef: null,
    _scenes: [],
    _activeIndex: 0,
    _scrollHandler: null,
    _resizeHandler: null,

    initialize(dotNetRef) {
        this.dispose();
        this._dotNetRef = dotNetRef;
        this._scenes = Array.from(document.querySelectorAll('.scene'));

        if (this._scenes.length === 0) return;

        // IntersectionObserver for active scene detection
        this._observer = new IntersectionObserver(
            (entries) => this._handleIntersection(entries),
            { threshold: [0, 0.25, 0.5, 0.75, 1], rootMargin: '-20% 0px -20% 0px' }
        );

        this._scenes.forEach(scene => this._observer.observe(scene));

        // Scroll handler for progress tracking (throttled)
        let ticking = false;
        this._scrollHandler = () => {
            if (!ticking) {
                requestAnimationFrame(() => {
                    this._updateProgress();
                    ticking = false;
                });
                ticking = true;
            }
        };
        window.addEventListener('scroll', this._scrollHandler, { passive: true });

        // Initial update
        this._updateProgress();
    },

    _handleIntersection(entries) {
        // Find the scene with the highest intersection ratio
        let bestScene = null;
        let bestRatio = 0;

        for (const scene of this._scenes) {
            const rect = scene.getBoundingClientRect();
            const viewportHeight = window.innerHeight;
            const visibleTop = Math.max(0, rect.top);
            const visibleBottom = Math.min(viewportHeight, rect.bottom);
            const visibleHeight = Math.max(0, visibleBottom - visibleTop);
            const ratio = visibleHeight / viewportHeight;

            if (ratio > bestRatio) {
                bestRatio = ratio;
                bestScene = scene;
            }
        }

        if (bestScene) {
            const newIndex = this._scenes.indexOf(bestScene);
            if (newIndex !== this._activeIndex) {
                this._activeIndex = newIndex;
                this._notifyActiveScene(newIndex);
            }
        }
    },

    _updateProgress() {
        const scrollTop = window.scrollY;
        const docHeight = document.documentElement.scrollHeight - window.innerHeight;
        const overallProgress = docHeight > 0 ? Math.min(scrollTop / docHeight, 1) : 0;

        // Determine active scene from scroll position
        let activeIdx = 0;
        for (let i = 0; i < this._scenes.length; i++) {
            const rect = this._scenes[i].getBoundingClientRect();
            if (rect.top <= window.innerHeight * 0.5) {
                activeIdx = i;
            }
        }

        if (activeIdx !== this._activeIndex) {
            this._activeIndex = activeIdx;
            this._notifyActiveScene(activeIdx);
        }

        // Update HUD progress bar directly (avoid round-trip to Blazor for smooth animation)
        const hudBar = document.getElementById('scene-hud-bar');
        if (hudBar) {
            hudBar.style.width = (overallProgress * 100) + '%';
        }

        const hudText = document.getElementById('scene-hud-text');
        if (hudText) {
            hudText.textContent = `${activeIdx + 1} / ${this._scenes.length}`;
        }

        // Update scene dots active state directly for responsiveness
        document.querySelectorAll('.scene-dot').forEach((dot, i) => {
            dot.classList.toggle('active', i === activeIdx);
        });

        // Update navbar bank name / scene title
        this._updateNavbarHeader(activeIdx);
    },

    _updateNavbarHeader(activeIndex) {
        const bankNameEl = document.querySelector('.navbar-bank-name');
        const sceneTitleEl = document.querySelector('.navbar-scene-title');
        if (!bankNameEl || !sceneTitleEl) return;

        if (activeIndex === 0) {
            bankNameEl.classList.remove('shrunk');
            sceneTitleEl.classList.remove('visible');
        } else {
            bankNameEl.classList.add('shrunk');
            sceneTitleEl.classList.add('visible');
            const activeScene = this._scenes[activeIndex];
            if (activeScene) {
                const title = activeScene.getAttribute('data-scene-title') || '';
                sceneTitleEl.textContent = title;
            }
        }
    },

    _notifyActiveScene(index) {
        if (this._dotNetRef) {
            this._dotNetRef.invokeMethodAsync('OnActiveSceneChanged', index);
        }
    },

    scrollToScene(index) {
        if (index >= 0 && index < this._scenes.length) {
            this._scenes[index].scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    },

    dispose() {
        if (this._observer) {
            this._observer.disconnect();
            this._observer = null;
        }
        if (this._scrollHandler) {
            window.removeEventListener('scroll', this._scrollHandler);
            this._scrollHandler = null;
        }
        this._dotNetRef = null;
        this._scenes = [];
        this._activeIndex = 0;
    }
};
