(function () {
    "use strict";

    const previewRoots = new Map();
    const roadmapRoots = new Map();
    const amChartsScriptUrls = [
        "/js/vendor/amcharts/index.js",
        "/js/vendor/amcharts/map.js",
        "/js/vendor/amcharts/geodata/worldLow.js",
        "/js/vendor/amcharts/themes/Animated.js"
    ];
    let amChartsLoadPromise = null;

    function hasAmCharts() {
        return typeof am5 !== "undefined"
            && typeof am5map !== "undefined"
            && typeof am5geodata_worldLow !== "undefined"
            && typeof am5themes_Animated !== "undefined";
    }

    function loadScript(url) {
        const absoluteUrl = new URL(url, document.baseURI).href;
        const existingScript = Array.from(document.scripts).find(function (scriptTag) {
            return scriptTag.src === absoluteUrl;
        });

        if (existingScript) {
            if (existingScript.dataset.loaded === "true") {
                return Promise.resolve();
            }

            return new Promise(function (resolve, reject) {
                existingScript.addEventListener("load", function () {
                    existingScript.dataset.loaded = "true";
                    resolve();
                }, { once: true });
                existingScript.addEventListener("error", function () {
                    reject(new Error("Failed to load script: " + url));
                }, { once: true });
            });
        }

        return new Promise(function (resolve, reject) {
            const script = document.createElement("script");
            script.src = url;
            script.async = true;
            script.addEventListener("load", function () {
                script.dataset.loaded = "true";
                resolve();
            }, { once: true });
            script.addEventListener("error", function () {
                reject(new Error("Failed to load script: " + url));
            }, { once: true });
            document.head.appendChild(script);
        });
    }

    async function ensureAmChartsLoaded() {
        if (hasAmCharts()) {
            return true;
        }

        if (!amChartsLoadPromise) {
            amChartsLoadPromise = Promise.all(amChartsScriptUrls.map(loadScript))
                .then(function () {
                    return hasAmCharts();
                })
                .catch(function (error) {
                    console.error("Failed to load amCharts dependencies.", error);
                    return false;
                })
                .finally(function () {
                    if (!hasAmCharts()) {
                        amChartsLoadPromise = null;
                    }
                });
        }

        return amChartsLoadPromise;
    }

    function disposeChart(store, containerId) {
        const chartEntry = store.get(containerId);
        if (!chartEntry) {
            return;
        }

        if (chartEntry.rotationAnimation) {
            chartEntry.rotationAnimation.stop();
        }

        chartEntry.root.dispose();
        store.delete(containerId);
    }

    function configureBaseMap(root) {
        root.setThemes([am5themes_Animated.new(root)]);

        const chart = root.container.children.push(am5map.MapChart.new(root, {
            panX: "rotateX",
            panY: "rotateY",
            projection: am5map.geoOrthographic(),
            rotationX: -15,
            rotationY: -25,
            minZoomLevel: 0.9,
            zoomLevel: 0.9
        }));

        const graticuleSeries = chart.series.push(am5map.GraticuleSeries.new(root, {}));
        graticuleSeries.mapLines.template.setAll({
            stroke: am5.color(0x5c6bc0),
            strokeOpacity: 0.12
        });

        const polygonSeries = chart.series.push(am5map.MapPolygonSeries.new(root, {
            geoJSON: am5geodata_worldLow
        }));

        polygonSeries.mapPolygons.template.setAll({
            fill: am5.color(0xdde3ff),
            stroke: am5.color(0xffffff),
            strokeWidth: 0.8,
            strokeOpacity: 0.65
        });

        const rotationAnimation = chart.animate({
            key: "rotationX",
            from: -15,
            to: -15 + 360,
            duration: 120000,
            loops: Infinity,
            easing: am5.ease.linear
        });

        chart.chartContainer.events.on("pointerdown", function () {
            if (rotationAnimation) {
                rotationAnimation.stop();
            }
        });

        return { chart, polygonSeries, rotationAnimation };
    }

    function normalizeDestinations(destinations) {
        if (!Array.isArray(destinations)) {
            return [];
        }

        const uniqueByCode = new Map();
        destinations.forEach(function (item) {
            if (!item || typeof item.code !== "string" || typeof item.name !== "string") {
                return;
            }

            const code = item.code.trim().toUpperCase();
            if (!code || uniqueByCode.has(code)) {
                return;
            }

            uniqueByCode.set(code, {
                code,
                name: item.name.trim()
            });
        });

        return Array.from(uniqueByCode.values());
    }

    async function renderPreview(containerId) {
        disposeChart(previewRoots, containerId);

        if (!await ensureAmChartsLoaded()) {
            console.error("amCharts libraries are not available for transaction roadmap preview.");
            return;
        }

        const container = document.getElementById(containerId);
        if (!container) {
            return;
        }

        container.innerHTML = "";

        const root = am5.Root.new(containerId);
        const baseMap = configureBaseMap(root);

        baseMap.chart.appear(800, 100);
        previewRoots.set(containerId, {
            root,
            rotationAnimation: baseMap.rotationAnimation
        });
    }

    async function renderRoadmap(containerId, originCode, destinations) {
        disposeChart(roadmapRoots, containerId);

        if (!await ensureAmChartsLoaded()) {
            console.error("amCharts libraries are not available for transaction roadmap.");
            return;
        }

        const normalizedOrigin = typeof originCode === "string" ? originCode.trim().toUpperCase() : "";
        const normalizedDestinations = normalizeDestinations(destinations)
            .filter(function (destination) { return destination.code !== normalizedOrigin; });

        if (!normalizedOrigin || normalizedDestinations.length === 0) {
            return;
        }

        const container = document.getElementById(containerId);
        if (!container) {
            return;
        }

        container.innerHTML = "";

        const root = am5.Root.new(containerId);
        const baseMap = configureBaseMap(root);
        const chart = baseMap.chart;
        const polygonSeries = baseMap.polygonSeries;

        const destinationCodes = normalizedDestinations.map(function (destination) { return destination.code; });
        polygonSeries.events.on("datavalidated", function () {
            am5.array.each(polygonSeries.dataItems, function (dataItem) {
                const id = dataItem.get("id");
                const polygon = dataItem.get("mapPolygon");
                if (!id || !polygon) {
                    return;
                }

                const uppercaseId = id.toUpperCase();
                if (uppercaseId === normalizedOrigin) {
                    polygon.setAll({
                        fill: am5.color(0x2e7d32)
                    });
                    return;
                }

                if (destinationCodes.includes(uppercaseId)) {
                    polygon.setAll({
                        fill: am5.color(0x1565c0)
                    });
                }
            });
        });

        const sankeySeries = chart.series.push(am5map.MapSankeySeries.new(root, {
            polygonSeries,
            maxWidth: 3,
            controlPointDistance: 0.5,
            nodePadding: 0.4,
            resolution: 64
        }));

        sankeySeries.mapPolygons.template.setAll({
            fill: am5.color(0x4f46e5),
            fillOpacity: 0.6,
            strokeOpacity: 0,
            tooltipText: "{sourceNode.name} -> {targetNode.name}"
        });

        sankeySeries.nodes.mapPolygons.template.setAll({
            fill: am5.color(0x1f2937),
            stroke: am5.color(0xf8fafc),
            strokeWidth: 1.4,
            fillOpacity: 0.92
        });

        sankeySeries.bullets.push(function () {
            return am5.Bullet.new(root, {
                locationX: 0,
                autoRotate: true,
                sprite: am5.Circle.new(root, {
                    radius: 2.8,
                    fill: am5.color(0x93c5fd),
                    stroke: am5.color(0xffffff),
                    strokeWidth: 0.8
                })
            });
        });

        const countryNames = {};
        normalizedDestinations.forEach(function (destination) {
            countryNames[destination.code] = destination.name;
        });
        countryNames[normalizedOrigin] = countryNames[normalizedOrigin] || normalizedOrigin;

        sankeySeries.data.setAll(
            normalizedDestinations.map(function (destination) {
                return {
                    sourceId: normalizedOrigin,
                    targetId: destination.code,
                    value: 1
                };
            })
        );

        sankeySeries.events.on("datavalidated", function () {
            am5.array.each(sankeySeries.nodes.dataItems, function (dataItem) {
                const id = dataItem.get("id");
                if (id && countryNames[id]) {
                    dataItem.set("name", countryNames[id]);
                }
            });

            am5.array.each(sankeySeries.dataItems, function (dataItem) {
                const bullets = dataItem.bullets;
                if (!bullets) {
                    return;
                }

                am5.array.each(bullets, function (bullet) {
                    bullet.animate({
                        key: "locationX",
                        from: 0,
                        to: 1,
                        duration: 3600 + (Math.random() * 1400),
                        easing: am5.ease.linear,
                        loops: Infinity
                    });
                });
            });
        });

        chart.set("zoomControl", am5map.ZoomControl.new(root, {}));
        chart.appear(900, 100);

        roadmapRoots.set(containerId, {
            root,
            rotationAnimation: baseMap.rotationAnimation
        });
    }

    window.transactionRoadmap = {
        renderPreview: renderPreview,
        destroyPreview: function (containerId) {
            disposeChart(previewRoots, containerId);
        },
        renderRoadmap: renderRoadmap,
        destroyRoadmap: function (containerId) {
            disposeChart(roadmapRoots, containerId);
        }
    };
})();
