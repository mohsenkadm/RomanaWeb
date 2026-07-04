// Leaflet maps for Zones admin — overview, simulator, preview modal
window.ZonesMaps = (function () {
    var ZONE_COLORS = [
        '#E53935', '#1E88E5', '#43A047', '#FB8C00', '#8E24AA',
        '#00ACC1', '#F4511E', '#3949AB', '#C0CA33', '#6D4C41',
        '#D81B60', '#00897B', '#5E35B1', '#FDD835', '#546E7A'
    ];

    var zonesData = [];
    var matrixMap = null, matrixLayerGroup = null;
    var simMap = null, simLayerGroup = null, simRouteLayer = null;
    var simPickupMarker = null, simDropMarker = null;
    var simMode = 'pickup';
    var simOnPointChange = null;

    var previewMap = null, previewPolygon = null, previewTestMarker = null, previewRing = null;

    function parseGeoJson(text) {
        if (!text || !String(text).trim()) return null;
        try {
            var gj = JSON.parse(String(text).trim());
            if (gj.type === 'Feature' && gj.geometry) gj = gj.geometry;
            if (gj.type === 'FeatureCollection' && gj.features && gj.features.length > 0) {
                var f = gj.features[0];
                gj = f.geometry || f;
            }
            if (!gj.coordinates) return null;
            var ring = null;
            if (gj.type === 'Polygon') ring = gj.coordinates[0];
            else if (gj.type === 'MultiPolygon' && gj.coordinates[0]) ring = gj.coordinates[0][0];
            if (!ring || ring.length < 3) return null;
            return ring.map(function (p) { return [p[1], p[0]]; });
        } catch (e) {
            return null;
        }
    }

    function pointInPolygon(polygonLatLng, lat, lng) {
        if (!polygonLatLng || polygonLatLng.length < 3) return false;
        var inside = false;
        for (var i = 0, j = polygonLatLng.length - 1; i < polygonLatLng.length; j = i++) {
            var pi = polygonLatLng[i], pj = polygonLatLng[j];
            var intersect = ((pi[0] > lat) !== (pj[0] > lat)) &&
                (lng < (pj[1] - pi[1]) * (lat - pi[0]) / ((pj[0] - pi[0]) || 1e-12) + pi[1]);
            if (intersect) inside = !inside;
        }
        return inside;
    }

    function zoneColor(index) {
        return ZONE_COLORS[index % ZONE_COLORS.length];
    }

    function zoneId(z) { return z.zoneId || z.ZoneId; }
    function zoneName(z) { return z.name || z.Name || '-'; }
    function zoneGeo(z) { return z.geoJson || z.GeoJson; }

    function createBaseMap(containerId, center, zoom) {
        var el = document.getElementById(containerId);
        if (!el) return null;
        if (el._leaflet_id) return el._leaflet_map || null;
        var map = L.map(containerId, { zoomControl: true }).setView(center || [30.5, 47.8], zoom || 11);
        el._leaflet_map = map;
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; OpenStreetMap'
        }).addTo(map);
        return map;
    }

    function makePinIcon(label, bg) {
        return L.divIcon({
            className: 'zone-map-pin',
            html: '<div class="zone-pin-inner" style="background:' + bg + '">' + label + '</div>',
            iconSize: [34, 34],
            iconAnchor: [17, 34]
        });
    }

    function renderLegend(containerId, zones) {
        var html = '';
        zones.forEach(function (z, i) {
            var active = !!(z.isActive || z.IsActive);
            html += '<span class="zone-legend-item' + (active ? '' : ' zone-legend-inactive') + '">' +
                '<i style="background:' + zoneColor(i) + '"></i>' + zoneName(z) + '</span>';
        });
        $('#' + containerId).html(html || '<span class="text-muted">لا توجد مناطق</span>');
    }

    function drawZonesOnMap(map, layerGroup, zones, options) {
        options = options || {};
        layerGroup.clearLayers();
        var bounds = [];
        zones.forEach(function (z, i) {
            var ring = parseGeoJson(zoneGeo(z));
            if (!ring) return;
            var color = zoneColor(i);
            var poly = L.polygon(ring, {
                color: color,
                weight: 2,
                fillColor: color,
                fillOpacity: (z.isActive || z.IsActive) !== false ? 0.22 : 0.08,
                opacity: (z.isActive || z.IsActive) !== false ? 0.9 : 0.4
            });
            poly.bindTooltip(zoneName(z), { sticky: true, direction: 'top' });
            if (options.simulatorMode) {
                poly.on('click', function (e) {
                    L.DomEvent.stopPropagation(e);
                    placeSimPoint(simMode, e.latlng.lat, e.latlng.lng);
                });
            } else {
                poly.bindPopup(
                    '<strong>' + zoneName(z) + '</strong><br>LZA: ' + (z.lzaKm || z.LzaKm || '-') +
                    ' | ECA: ' + (z.ecaPricePerKm || z.EcaPricePerKm || '-') + ' د.ع/كم'
                );
            }
            if (options.onZoneClick) {
                poly.on('click', function (e) {
                    L.DomEvent.stopPropagation(e);
                    options.onZoneClick(z);
                });
            }
            layerGroup.addLayer(poly);
            bounds.push(poly.getBounds());
        });
        if (bounds.length && options.fitBounds !== false) {
            var combined = bounds[0];
            for (var b = 1; b < bounds.length; b++) combined = combined.extend(bounds[b]);
            map.fitBounds(combined, { padding: [24, 24], maxZoom: 14 });
        }
    }

    function setZones(zones) {
        zonesData = zones || [];
        if (matrixMap && matrixLayerGroup) {
            drawZonesOnMap(matrixMap, matrixLayerGroup, zonesData);
            renderLegend('matrixLegend', zonesData);
            matrixMap.invalidateSize();
        }
        if (simMap && simLayerGroup) {
            drawZonesOnMap(simMap, simLayerGroup, zonesData, { simulatorMode: true, fitBounds: false });
            renderLegend('simLegend', zonesData);
            simMap.invalidateSize();
        }
    }

    function initMatrixMap() {
        if (matrixMap) return;
        matrixMap = createBaseMap('matrixOverviewMap');
        if (!matrixMap) return;
        matrixLayerGroup = L.layerGroup().addTo(matrixMap);
        drawZonesOnMap(matrixMap, matrixLayerGroup, zonesData);
        renderLegend('matrixLegend', zonesData);
    }

    function initSimulatorMap(onPointChange) {
        simOnPointChange = onPointChange;
        if (simMap) return;
        simMap = createBaseMap('simulatorMap');
        if (!simMap) return;
        simLayerGroup = L.layerGroup().addTo(simMap);
        simRouteLayer = L.layerGroup().addTo(simMap);
        drawZonesOnMap(simMap, simLayerGroup, zonesData, { simulatorMode: true });

        simMap.on('click', function (e) {
            placeSimPoint(simMode, e.latlng.lat, e.latlng.lng);
        });

        renderLegend('simLegend', zonesData);
    }

    function findZoneAt(lat, lng) {
        for (var i = 0; i < zonesData.length; i++) {
            var ring = parseGeoJson(zoneGeo(zonesData[i]));
            if (ring && pointInPolygon(ring, lat, lng)) return zonesData[i];
        }
        return null;
    }

    function placeSimPoint(type, lat, lng) {
        if (!simMap) return;
        var icon = type === 'pickup'
            ? makePinIcon('🍽', '#FF9800')
            : makePinIcon('👤', '#4CAF50');

        if (type === 'pickup') {
            if (simPickupMarker) simMap.removeLayer(simPickupMarker);
            simPickupMarker = L.marker([lat, lng], { icon: icon, draggable: true }).addTo(simMap);
            simPickupMarker.on('dragend', function (ev) {
                var p = ev.target.getLatLng();
                notifyPoint('pickup', p.lat, p.lng);
            });
        } else {
            if (simDropMarker) simMap.removeLayer(simDropMarker);
            simDropMarker = L.marker([lat, lng], { icon: icon, draggable: true }).addTo(simMap);
            simDropMarker.on('dragend', function (ev) {
                var p = ev.target.getLatLng();
                notifyPoint('dropoff', p.lat, p.lng);
            });
        }
        notifyPoint(type, lat, lng);
    }

    function notifyPoint(type, lat, lng) {
        var z = findZoneAt(lat, lng);
        if (typeof simOnPointChange === 'function') {
            simOnPointChange(type, lat, lng, z ? zoneName(z) : null);
        }
    }

    function setSimMode(mode) {
        simMode = mode === 'dropoff' ? 'dropoff' : 'pickup';
        $('.sim-map-mode').removeClass('active');
        $('.sim-map-mode[data-mode="' + simMode + '"]').addClass('active');
        if (simMap) {
            simMap.getContainer().style.cursor = simMode === 'pickup' ? 'crosshair' : 'cell';
        }
    }

    function syncSimMarkers(pickup, dropoff) {
        if (!simMap) return;
        if (pickup && pickup.lat && pickup.lng) {
            if (simPickupMarker) simMap.removeLayer(simPickupMarker);
            simPickupMarker = L.marker([pickup.lat, pickup.lng], {
                icon: makePinIcon('🍽', '#FF9800'), draggable: true
            }).addTo(simMap);
            simPickupMarker.on('dragend', function (ev) {
                var p = ev.target.getLatLng();
                notifyPoint('pickup', p.lat, p.lng);
            });
        }
        if (dropoff && dropoff.lat && dropoff.lng) {
            if (simDropMarker) simMap.removeLayer(simDropMarker);
            simDropMarker = L.marker([dropoff.lat, dropoff.lng], {
                icon: makePinIcon('👤', '#4CAF50'), draggable: true
            }).addTo(simMap);
            simDropMarker.on('dragend', function (ev) {
                var p = ev.target.getLatLng();
                notifyPoint('dropoff', p.lat, p.lng);
            });
        }
    }

    function drawRoute(path, distanceKm, source) {
        if (!simMap || !simRouteLayer) return;
        simRouteLayer.clearLayers();
        if (!path || path.length < 2) return;

        var latlngs = path.map(function (p) {
            return [p.lat != null ? p.lat : p.Lat, p.lng != null ? p.lng : p.Lng];
        });
        var line = L.polyline(latlngs, {
            color: '#1565C0',
            weight: 5,
            opacity: 0.85,
            dashArray: source === 'haversine_fallback' ? '8,8' : null
        }).addTo(simRouteLayer);

        var mid = latlngs[Math.floor(latlngs.length / 2)];
        L.marker(mid, {
            icon: L.divIcon({
                className: 'zone-route-label',
                html: '<div class="route-km-badge">' + (distanceKm || '-') + ' km<br><small>' + (source || '') + '</small></div>',
                iconSize: [80, 40],
                iconAnchor: [40, 20]
            })
        }).addTo(simRouteLayer);

        simMap.fitBounds(line.getBounds().pad(0.2));
    }

    function clearRoute() {
        if (simRouteLayer) simRouteLayer.clearLayers();
    }

    function refreshMatrix() {
        initMatrixMap();
        if (matrixMap) setTimeout(function () { matrixMap.invalidateSize(); }, 200);
    }

    function refreshSimulator() {
        initSimulatorMap(simOnPointChange);
        if (simMap) setTimeout(function () { simMap.invalidateSize(); }, 200);
    }

    // --- Preview modal (single zone) ---
    function showPreviewError(msg) {
        previewRing = null;
        if (previewMap && previewPolygon) {
            previewMap.removeLayer(previewPolygon);
            previewPolygon = null;
        }
        $('#mapModalZoneName').text('خطأ');
        $('#mapModalMeta').html('<span class="text-danger">' + msg + '</span>');
        $('#mapClickResult').html('');
        $('#zoneMapModal').modal('show');
    }

    function showPreview(geoJsonText, name) {
        var latlngs = parseGeoJson(geoJsonText);
        if (!latlngs) {
            showPreviewError('GeoJSON غير صالح — تأكد من Polygon وترتيب [lng, lat]');
            return false;
        }
        previewRing = latlngs;
        $('#mapModalZoneName').text(name || '-');
        var closed = latlngs.length >= 4 &&
            Math.abs(latlngs[0][0] - latlngs[latlngs.length - 1][0]) < 1e-6 &&
            Math.abs(latlngs[0][1] - latlngs[latlngs.length - 1][1]) < 1e-6;
        $('#mapModalMeta').html(
            (closed ? '✔ Polygon مغلق' : '⚠ Polygon قد لا يكون مغلقاً') +
            ' | ' + latlngs.length + ' نقطة | انقر لاختبار داخل/خارج الزون');
        $('#mapClickResult').html('');
        $('#zoneMapModal').modal('show');

        setTimeout(function () {
            if (!previewMap) {
                previewMap = L.map('zonePreviewMap').setView([30.5, 47.8], 11);
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    maxZoom: 19, attribution: '&copy; OpenStreetMap'
                }).addTo(previewMap);
                previewMap.on('click', function (e) {
                    if (!previewRing) return;
                    var inside = pointInPolygon(previewRing, e.latlng.lat, e.latlng.lng);
                    if (previewTestMarker) previewMap.removeLayer(previewTestMarker);
                    previewTestMarker = L.marker(e.latlng).addTo(previewMap);
                    $('#mapClickResult').html(inside
                        ? '<span class="text-success">✔ داخل الزون</span>'
                        : '<span class="text-danger">✖ خارج الزون</span>');
                });
            }
            if (previewPolygon) previewMap.removeLayer(previewPolygon);
            previewPolygon = L.polygon(latlngs, { color: '#2196F3', fillColor: '#2196F3', fillOpacity: 0.25 }).addTo(previewMap);
            previewMap.fitBounds(previewPolygon.getBounds(), { padding: [20, 20] });
            previewMap.invalidateSize();
        }, 350);
        return true;
    }

    return {
        setZones: setZones,
        initMatrixMap: initMatrixMap,
        initSimulatorMap: initSimulatorMap,
        refreshMatrix: refreshMatrix,
        refreshSimulator: refreshSimulator,
        setSimMode: setSimMode,
        syncSimMarkers: syncSimMarkers,
        drawRoute: drawRoute,
        clearRoute: clearRoute,
        showPreview: showPreview,
        parseGeoJson: parseGeoJson
    };
})();

window.ZonesMapPreview = { show: function (geo, name) { return window.ZonesMaps.showPreview(geo, name); } };
