(function () {
    var zonesCache = [];
    var matrixCache = [];

    function alertBox(id, ok, msg) {
        $('#' + id).html('<div class="alert alert-' + (ok ? 'success' : 'danger') + '">' + msg + '</div>');
    }

    function zonePayload() {
        return {
            name: $('#zoneName').val(),
            geoJson: $('#zoneGeo').val(),
            isActive: $('#zoneActive').is(':checked'),
            baseDeliveryPrice: parseFloat($('#zoneBasePrice').val()) || null,
            lzaKm: parseFloat($('#zoneLza').val()) || 3,
            ecaPricePerKm: parseFloat($('#zoneEcaPrice').val()) || 250,
            maxEcaFee: parseFloat($('#zoneMaxEca').val()) || 2500,
            maxTotalDeliveryFee: parseFloat($('#zoneMaxTotal').val()) || null,
            nearRestaurantPrice: parseFloat($('#zoneNearPrice').val()) || null,
            nearRestaurantKm: parseFloat($('#zoneNearKm').val()) || 1
        };
    }

    function fillSelects(rows) {
        var opts = '<option value="">—</option>';
        rows.forEach(function (z) {
            opts += '<option value="' + (z.zoneId || z.ZoneId) + '">' + (z.name || z.Name) + '</option>';
        });
        $('#matrixFromZone, #matrixToZone').html(opts);
    }

    function openZoneMap(geo, name) {
        var text = (geo || '').trim();
        if (!text) {
            alertBox('zoneFormResult', false, 'أدخل GeoJSON أولاً');
            return;
        }
        window.ZonesMaps.showPreview(text, name || '');
    }

    function actionButtons(type, id) {
        if (type === 'zone') {
            return '<div class="btn-action-group">' +
                '<button type="button" class="btn btn-sm btn-info btn-map-row" data-id="' + id + '" title="خريطة"><i class="material-icons" style="font-size:16px">map</i></button>' +
                '<button type="button" class="btn btn-sm btn-primary btn-edit-row" data-id="' + id + '" title="تعديل"><i class="material-icons" style="font-size:16px">edit</i></button>' +
                '<button type="button" class="btn btn-sm btn-danger btn-delete-row" data-id="' + id + '" title="تعطيل"><i class="material-icons" style="font-size:16px">delete</i></button>' +
                '</div>';
        }
        return '<div class="btn-action-group">' +
            '<button type="button" class="btn btn-sm btn-primary btn-edit-matrix" data-id="' + id + '" title="تعديل"><i class="material-icons" style="font-size:16px">edit</i></button>' +
            '<button type="button" class="btn btn-sm btn-danger btn-delete-matrix" data-id="' + id + '" title="حذف"><i class="material-icons" style="font-size:16px">delete</i></button>' +
            '</div>';
    }

    function renderZonesTable(rows) {
        var html = '';
        rows.forEach(function (z) {
            var id = z.zoneId || z.ZoneId;
            html += '<tr>' +
                '<td>' + id + '</td>' +
                '<td>' + (z.name || z.Name) + '</td>' +
                '<td>' + (z.lzaKm || z.LzaKm || '-') + '</td>' +
                '<td>' + (z.ecaPricePerKm || z.EcaPricePerKm || '-') + '</td>' +
                '<td>' + (z.baseDeliveryPrice || z.BaseDeliveryPrice || '-') + '</td>' +
                '<td>' + (z.maxTotalDeliveryFee || z.MaxTotalDeliveryFee || '-') + '</td>' +
                '<td>' + ((z.isActive || z.IsActive) ? '✔' : '✖') + '</td>' +
                '<td>' + actionButtons('zone', id) + '</td></tr>';
        });
        $('#zonesTableBody').html(html || '<tr><td colspan="8" class="text-center">لا يوجد</td></tr>');
    }

    function renderMatrixTable(rows) {
        var html = '';
        rows.forEach(function (m) {
            var id = m.zonePriceId || m.ZonePriceId;
            html += '<tr>' +
                '<td>' + id + '</td>' +
                '<td>' + (m.fromZoneName || m.FromZoneName) + '</td>' +
                '<td>' + (m.toZoneName || m.ToZoneName) + '</td>' +
                '<td>' + (m.price || m.Price) + '</td>' +
                '<td>' + actionButtons('matrix', id) + '</td></tr>';
        });
        $('#matrixTableBody').html(html || '<tr><td colspan="5" class="text-center">لا يوجد</td></tr>');
    }

    function refreshMaps() {
        if (window.ZonesMaps) {
            ZonesMaps.setZones(zonesCache);
        }
    }

    function loadZones() {
        call_ajax('GET', 'zones', null, function (rows) {
            rows = rows || [];
            zonesCache = rows;
            $('#zonesCount').text(rows.length);
            fillSelects(rows);
            renderZonesTable(rows);
            refreshMaps();
        });
    }

    function loadMatrix() {
        call_ajax('GET', 'zones/matrix', null, function (rows) {
            rows = rows || [];
            matrixCache = rows;
            $('#matrixCount').text(rows.length);
            renderMatrixTable(rows);
        });
    }

    function resetZoneForm() {
        $('#zoneEditId').val('');
        $('#zoneFormTitle').text('إضافة منطقة');
        $('#zoneName, #zoneGeo').val('');
        $('#zoneBasePrice').val(3000);
        $('#zoneLza').val(3);
        $('#zoneEcaPrice').val(250);
        $('#zoneMaxEca').val(2500);
        $('#zoneMaxTotal').val('');
        $('#zoneNearPrice').val(1500);
        $('#zoneNearKm').val(1);
        $('#zoneActive').prop('checked', true);
    }

    function resetMatrixForm() {
        $('#matrixEditId').val('');
        $('#matrixFormTitle').text('إضافة سعر');
        $('#matrixFromZone, #matrixToZone').val('');
        $('#matrixPriceManual').val('');
    }

    function getSimPoints() {
        return {
            pickup: {
                lat: parseFloat($('#simPickupLat').val()),
                lng: parseFloat($('#simPickupLng').val())
            },
            dropoff: {
                lat: parseFloat($('#simDropLat').val()),
                lng: parseFloat($('#simDropLng').val())
            }
        };
    }

    function updateSimZoneBadge(elId, zoneName) {
        var el = $('#' + elId);
        if (zoneName) {
            el.text('الزون: ' + zoneName).show();
        } else {
            el.text('خارج أي زون').show();
        }
    }

    function onSimPointChange(type, lat, lng, zoneName) {
        if (type === 'pickup') {
            $('#simPickupLat').val(lat.toFixed(6));
            $('#simPickupLng').val(lng.toFixed(6));
            updateSimZoneBadge('simPickupZone', zoneName);
        } else {
            $('#simDropLat').val(lat.toFixed(6));
            $('#simDropLng').val(lng.toFixed(6));
            updateSimZoneBadge('simDropZone', zoneName);
        }
        if (window.ZonesMaps) ZonesMaps.clearRoute();
        $('#simRouteInfo, #simResult').hide();
    }

    function syncSimMarkersFromInputs() {
        var pts = getSimPoints();
        if (window.ZonesMaps) {
            ZonesMaps.syncSimMarkers(pts.pickup, pts.dropoff);
        }
    }

    function renderSimResult(q, expected) {
        var total = q.total || q.Total || 0;
        var ok = !expected || Math.abs(total - expected) < 1;
        var cls = ok ? 'sim-result-ok' : 'sim-result-bad';
        var caps = '';
        if (q.ecaCapApplied || q.EcaCapApplied)
            caps += '<div class="text-warning small">⚠ تم تطبيق <strong>سقف ECA</strong> (أقصى رسوم المسافة الإضافية)</div>';
        if (q.maxTotalCapApplied || q.MaxTotalCapApplied)
            caps += '<div class="text-danger small">⚠ تم تطبيق <strong>سقف التوصيل الإجمالي</strong>' +
                (q.maxTotalDeliveryFee || q.MaxTotalDeliveryFee ? ' = ' + (q.maxTotalDeliveryFee || q.MaxTotalDeliveryFee) + ' د.ع' : '') + '</div>';
        var html = '<div class="' + cls + ' p-3 pricing-breakdown">' + caps +
            '<dl class="row mb-0">' +
            '<dt class="col-5">Zone Fee (المصفوفة)</dt><dd class="col-7">' + (q.zoneFee || q.ZoneFee || 0) + ' د.ع</dd>' +
            '<dt class="col-5">مسافة الطريق</dt><dd class="col-7">' + (q.routeDistanceKm || q.RouteDistanceKm || '-') + ' km (' + (q.routeSource || q.RouteSource || '-') + ')</dd>' +
            '<dt class="col-5">LZA</dt><dd class="col-7">' + (q.lzaKm || q.LzaKm || '-') + ' km</dd>' +
            '<dt class="col-5">ECA</dt><dd class="col-7">' + (q.ecaKm || q.EcaKm || 0) + ' km → ' + (q.ecaFee || q.EcaFee || 0) + ' د.ع</dd>' +
            '<dt class="col-5">From → To</dt><dd class="col-7">' + (q.fromZone || q.FromZone || '-') + ' → ' + (q.toZone || q.ToZone || '-') + '</dd>' +
            '<dt class="col-5">المصدر</dt><dd class="col-7">' + (q.pricingSource || q.PricingSource) + '</dd>' +
            '</dl>' +
            '<hr><div style="font-size:1.25em;font-weight:700">FINAL: ' + total.toLocaleString() + ' د.ع ' + (ok ? '✔' : '✖') + '</div></div>';
        $('#simResult').html(html).show();
    }

    function fetchRouteAndPrice() {
        var pts = getSimPoints();
        if (isNaN(pts.pickup.lat) || isNaN(pts.pickup.lng) || isNaN(pts.dropoff.lat) || isNaN(pts.dropoff.lng)) {
            md.showNotification('أدخل إحداثيات صالحة للمطعم والزبون');
            return;
        }

        var body = {
            pickupLat: pts.pickup.lat,
            pickupLng: pts.pickup.lng,
            dropoffLat: pts.dropoff.lat,
            dropoffLng: pts.dropoff.lng,
            forceZonePricing: true
        };
        var dKm = $('#simDistanceKm').val();
        if (dKm) body.distanceKm = parseFloat(dKm);

        call_ajax_json('POST', 'zones/route', {
            fromLat: pts.pickup.lat,
            fromLng: pts.pickup.lng,
            toLat: pts.dropoff.lat,
            toLng: pts.dropoff.lng
        }, function (route) {
            if (window.ZonesMaps && route) {
                ZonesMaps.drawRoute(route.path, route.distanceKm, route.source);
                $('#simRouteInfo').html(
                    '<strong>مسافة الطريق:</strong> ' + (route.distanceKm || '-') + ' km' +
                    ' | <strong>المصدر:</strong> ' + (route.source || '-')
                ).show();
            }

            call_ajax_json('POST', 'zones/simulate', body, function (q) {
                renderSimResult(q, parseFloat($('#simExpected').val()));
            });
        });
    }

    // --- Events ---
    $('#btnSaveZone').on('click', function () {
        var payload = zonePayload();
        var editId = $('#zoneEditId').val();
        var url = editId ? ('zones/' + editId) : 'zones/create';
        var method = editId ? 'PUT' : 'POST';
        call_ajax_json(method, url, payload, function () {
            alertBox('zoneFormResult', true, 'تم الحفظ');
            resetZoneForm();
            loadZones();
        });
    });

    $('#btnResetZoneForm').on('click', resetZoneForm);

    $('#btnPreviewMap').on('click', function (e) {
        e.preventDefault();
        openZoneMap($('#zoneGeo').val(), $('#zoneName').val());
    });

    $('#zonesTableBody').on('click', '.btn-map-row', function (e) {
        e.preventDefault();
        var id = $(this).data('id');
        var z = zonesCache.find(function (x) { return (x.zoneId || x.ZoneId) == id; });
        if (z) openZoneMap(z.geoJson || z.GeoJson, z.name || z.Name);
    });

    $('#zonesTableBody').on('click', '.btn-edit-row', function () {
        var id = $(this).data('id');
        var z = zonesCache.find(function (x) { return (x.zoneId || x.ZoneId) == id; });
        if (!z) return;
        $('#zoneEditId').val(id);
        $('#zoneFormTitle').text('تعديل: ' + (z.name || z.Name));
        $('#zoneName').val(z.name || z.Name);
        $('#zoneGeo').val(z.geoJson || z.GeoJson);
        $('#zoneBasePrice').val(z.baseDeliveryPrice || z.BaseDeliveryPrice || '');
        $('#zoneLza').val(z.lzaKm || z.LzaKm || 3);
        $('#zoneEcaPrice').val(z.ecaPricePerKm || z.EcaPricePerKm || 250);
        $('#zoneMaxEca').val(z.maxEcaFee || z.MaxEcaFee || 2500);
        $('#zoneMaxTotal').val(z.maxTotalDeliveryFee || z.MaxTotalDeliveryFee || '');
        $('#zoneNearPrice').val(z.nearRestaurantPrice || z.NearRestaurantPrice || '');
        $('#zoneNearKm').val(z.nearRestaurantKm || z.NearRestaurantKm || 1);
        $('#zoneActive').prop('checked', !!(z.isActive || z.IsActive));
        $('a[href="#tabZones"]').tab('show');
        $('html, body').animate({ scrollTop: 0 }, 300);
    });

    $('#zonesTableBody').on('click', '.btn-delete-row', function () {
        var id = $(this).data('id');
        var z = zonesCache.find(function (x) { return (x.zoneId || x.ZoneId) == id; });
        if (!z) return;
        if (!confirm('تعطيل المنطقة «' + (z.name || z.Name) + '»؟')) return;
        call_ajax('DELETE', 'zones/' + id, null, function () {
            loadZones();
        });
    });

    $('#btnLoadSampleGeo').on('click', function () {
        $('#zoneGeo').val('{"type":"Polygon","coordinates":[[[47.75,30.48],[47.82,30.48],[47.82,30.52],[47.75,30.52],[47.75,30.48]]]}');
    });

    $('#uploadZonesBtn').on('click', function () {
        var f = $('#zonesFile')[0].files[0];
        if (!f) { alertBox('zonesUploadResult', false, 'اختر ملف'); return; }
        var fd = new FormData(); fd.append('file', f);
        call_ajax_withfile('POST', 'zones/upload', fd, function (msg) {
            alertBox('zonesUploadResult', true, msg); loadZones();
        });
    });

    $('#uploadMatrixBtn').on('click', function () {
        var f = $('#matrixFile')[0].files[0];
        if (!f) { alertBox('matrixUploadResult', false, 'اختر ملف'); return; }
        var fd = new FormData(); fd.append('file', f);
        call_ajax_withfile('POST', 'zones/matrix/upload', fd, function (msg) {
            alertBox('matrixUploadResult', true, msg); loadMatrix();
        });
    });

    $('#saveMatrixManualBtn').on('click', function () {
        var fromId = parseInt($('#matrixFromZone').val(), 10);
        var toId = parseInt($('#matrixToZone').val(), 10);
        var price = parseFloat($('#matrixPriceManual').val());
        if (!fromId || !toId) { alertBox('matrixManualResult', false, 'اختر المنطقتين'); return; }
        if (isNaN(price) || price < 0) { alertBox('matrixManualResult', false, 'السعر غير صالح'); return; }
        call_ajax_json('POST', 'zones/matrix/create', {
            fromZoneId: fromId,
            toZoneId: toId,
            price: price
        }, function () {
            alertBox('matrixManualResult', true, 'تم الحفظ');
            resetMatrixForm();
            loadMatrix();
        });
    });

    $('#btnResetMatrixForm').on('click', resetMatrixForm);

    $('#matrixTableBody').on('click', '.btn-edit-matrix', function () {
        var id = $(this).data('id');
        var m = matrixCache.find(function (x) { return (x.zonePriceId || x.ZonePriceId) == id; });
        if (!m) return;
        $('#matrixEditId').val(id);
        $('#matrixFormTitle').text('تعديل سعر: ' + (m.fromZoneName || m.FromZoneName) + ' → ' + (m.toZoneName || m.ToZoneName));
        $('#matrixFromZone').val(m.fromZoneId || m.FromZoneId);
        $('#matrixToZone').val(m.toZoneId || m.ToZoneId);
        $('#matrixPriceManual').val(m.price || m.Price);
        $('a[href="#tabMatrix"]').tab('show');
    });

    $('#matrixTableBody').on('click', '.btn-delete-matrix', function () {
        var id = $(this).data('id');
        var m = matrixCache.find(function (x) { return (x.zonePriceId || x.ZonePriceId) == id; });
        if (!m) return;
        if (!confirm('حذف السعر: ' + (m.fromZoneName || m.FromZoneName) + ' → ' + (m.toZoneName || m.ToZoneName) + '؟')) return;
        call_ajax('DELETE', 'zones/matrix/' + id, null, function () {
            loadMatrix();
        });
    });

    $('.sim-map-mode').on('click', function () {
        if (window.ZonesMaps) ZonesMaps.setSimMode($(this).data('mode'));
    });

    $('.sim-field').on('change', function () {
        syncSimMarkersFromInputs();
        if (window.ZonesMaps) ZonesMaps.clearRoute();
    });

    $('.sim-preset').on('click', function () {
        var p = $(this).data('preset');
        if (p === 'pdf4') {
            $('#simPickupLat').val('30.50'); $('#simPickupLng').val('47.78');
            $('#simDropLat').val('30.50'); $('#simDropLng').val('47.86');
            $('#simDistanceKm').val('4'); $('#simExpected').val('3250');
        } else if (p === 'pdf52') {
            $('#simPickupLat').val('30.50'); $('#simPickupLng').val('47.78');
            $('#simDropLat').val('30.50'); $('#simDropLng').val('47.88');
            $('#simDistanceKm').val('5.2'); $('#simExpected').val('3500');
        } else if (p === 'near') {
            $('#simPickupLat').val('30.500'); $('#simPickupLng').val('47.780');
            $('#simDropLat').val('30.501'); $('#simDropLng').val('47.781');
            $('#simDistanceKm').val(''); $('#simExpected').val('');
        }
        syncSimMarkersFromInputs();
        if (window.ZonesMaps) ZonesMaps.clearRoute();
        $('#simRouteInfo, #simResult').hide();
    });

    $('#btnRunSimulator').on('click', fetchRouteAndPrice);

    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        var target = $(e.target).attr('href');
        if (target === '#tabMatrix' && window.ZonesMaps) {
            ZonesMaps.initMatrixMap();
            ZonesMaps.refreshMatrix();
        }
        if (target === '#tabSimulator' && window.ZonesMaps) {
            ZonesMaps.initSimulatorMap(onSimPointChange);
            ZonesMaps.setSimMode('pickup');
            syncSimMarkersFromInputs();
            ZonesMaps.refreshSimulator();
        }
    });

    loadZones();
    loadMatrix();
    loadSystemSettings();

    $('#btnSaveSystemSettings').on('click', function () {
        call_ajax_json('PUT', 'zones/system-settings', {
            iqdRoundingStep: parseInt($('#sysIqdStep').val(), 10) || 250,
            allowBusyDriverDispatch: $('#sysAllowBusyDispatch').is(':checked'),
            pricePerKm: parseFloat($('#sysPricePerKm').val()) || 500,
            minChargeKmThreshold: parseFloat($('#sysMinKm').val()) || 1.5,
            minChargeAmount: parseFloat($('#sysMinAmount').val()) || 500,
            roundingMode: $('#sysRoundingMode').val() || 'Ceil'
        }, function () {
            alertBox('systemSettingsResult', true, 'تم حفظ إعدادات النظام');
        });
    });

    function loadSystemSettings() {
        call_ajax('GET', 'zones/system-settings', null, function (s) {
            if (!s) return;
            $('#sysIqdStep').val(s.iqdRoundingStep || s.IqdRoundingStep || 250);
            $('#sysPricePerKm').val(s.pricePerKm || s.PricePerKm || 500);
            $('#sysMinKm').val(s.minChargeKmThreshold || s.MinChargeKmThreshold || 1.5);
            $('#sysMinAmount').val(s.minChargeAmount || s.MinChargeAmount || 500);
            $('#sysRoundingMode').val(s.roundingMode || s.RoundingMode || 'Ceil');
            $('#sysAllowBusyDispatch').prop('checked', !!(s.allowBusyDriverDispatch || s.AllowBusyDriverDispatch));
        });
    }
})();
