// Modal helper for keyboard navigation
window.modalHelper = {
    escapeHandler: null,
    
    registerEscapeHandler: function(dotNetRef) {
        this.escapeHandler = function(e) {
            if (e.key === 'Escape') {
                e.preventDefault();
                dotNetRef.invokeMethodAsync('HandleEscapeKey');
            }
        };
        document.addEventListener('keydown', this.escapeHandler);
    },
    
    unregisterEscapeHandler: function() {
        if (this.escapeHandler) {
            document.removeEventListener('keydown', this.escapeHandler);
            this.escapeHandler = null;
        }
    }
};
