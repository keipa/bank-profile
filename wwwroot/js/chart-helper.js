// Chart.js helper functions for Blazor components

let chartInstances = {};

window.renderChart = function (canvasId, config) {
    // Destroy existing chart if it exists
    if (chartInstances[canvasId]) {
        chartInstances[canvasId].destroy();
    }

    // Get canvas element
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.error(`Canvas element with id '${canvasId}' not found`);
        return;
    }

    // Create new chart
    const ctx = canvas.getContext('2d');
    chartInstances[canvasId] = new Chart(ctx, config);
};

window.destroyChart = function (canvasId) {
    if (chartInstances[canvasId]) {
        chartInstances[canvasId].destroy();
        delete chartInstances[canvasId];
    }
};

window.updateChartData = function (canvasId, labels, data) {
    const chart = chartInstances[canvasId];
    if (chart) {
        chart.data.labels = labels;
        chart.data.datasets[0].data = data;
        chart.update();
    }
};
