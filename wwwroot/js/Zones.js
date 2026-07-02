// Section 2.2 - Zones admin UI.
// IMPORTANT: this app stores the JWT in cookie "token2" and uses the global
// call_ajax / call_ajax_withfile helpers (see site.js) to attach it as
// "Authorization: Bearer <token>". Using $.ajax directly here would send no
// token and the server returns 401. Always go through call_ajax*.
(function () {

    function alertBox(targetId, success, msg) {
        var cls = success ? 'alert-success' : 'alert-danger';
        $('#' + targetId).html('<div class="alert ' + cls + '">' + msg + '</div>');
    }

    function fillZoneSelects(rows) {
        var options = '<option value="">اختر المنطقة</option>';
        rows.forEach(function (z) {
            var id = z.zoneId || z.ZoneId;
            var name = z.name || z.Name;
            options += '<option value="' + id + '">' + name + '</option>';
        });
        $('#matrixFromZone, #matrixToZone').html(options);
    }

    function loadZones() {
        call_ajax('GET', 'zones', null, function (rows) {
            rows = rows || [];
            $('#zonesCount').text(rows.length);
            fillZoneSelects(rows);

            var html = '';
            rows.forEach(function (z) {
                html += '<tr>' +
                    '<td>' + (z.zoneId || z.ZoneId) + '</td>' +
                    '<td>' + (z.name || z.Name) + '</td>' +
                    '<td>' + ((z.isActive || z.IsActive) ? '✔' : '✖') + '</td>' +
                    '<td style="direction:ltr;text-align:left;font-size:11px;max-width:600px;word-break:break-all;">' +
                    (z.geoJson || z.GeoJson || '') + '</td>' +
                    '</tr>';
            });
            $('#zonesTableBody').html(html || '<tr><td colspan="4" class="text-center">لا يوجد بيانات</td></tr>');
        });
    }

    function loadMatrix() {
        call_ajax('GET', 'zones/matrix', null, function (rows) {
            rows = rows || [];
            $('#matrixCount').text(rows.length);
            var html = '';
            rows.forEach(function (m) {
                var fromName = m.fromZoneName || m.FromZoneName || m.fromZoneId || m.FromZoneId;
                var toName = m.toZoneName || m.ToZoneName || m.toZoneId || m.ToZoneId;
                html += '<tr>' +
                    '<td>' + (m.zonePriceId || m.ZonePriceId) + '</td>' +
                    '<td>' + fromName + '</td>' +
                    '<td>' + toName + '</td>' +
                    '<td>' + (m.price || m.Price) + '</td>' +
                    '</tr>';
            });
            $('#matrixTableBody').html(html || '<tr><td colspan="4" class="text-center">لا يوجد بيانات</td></tr>');
        });
    }

    function uploadFile(url, inputId, resultId, onDone) {
        var $input = $('#' + inputId);
        var fileInput = $input.length ? $input[0] : document.getElementById(inputId);
        if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
            alertBox(resultId, false, 'يرجى اختيار ملف Excel أولاً');
            return;
        }
        var fd = new FormData();
        fd.append('file', fileInput.files[0]);

        call_ajax_withfile('POST', url, fd, function (data) {
            alertBox(resultId, true, 'تم رفع الملف بنجاح');
            if (onDone) onDone();
            try { fileInput.value = ''; } catch (e) { }
        });
    }

    function saveZoneManual() {
        var name = ($('#zoneNameManual').val() || '').trim();
        var geo = ($('#zoneGeoManual').val() || '').trim();
        if (!name) {
            alertBox('zoneManualResult', false, 'رجاءا ادخل اسم المنطقة');
            return;
        }
        if (!geo) {
            alertBox('zoneManualResult', false, 'رجاءا ادخل GeoJSON للمنطقة');
            return;
        }

        call_ajax_json('POST', 'zones/create', {
            name: name,
            geoJson: geo,
            isActive: $('#zoneActiveManual').is(':checked')
        }, function () {
            alertBox('zoneManualResult', true, 'تم حفظ المنطقة بنجاح');
            $('#zoneNameManual').val('');
            $('#zoneGeoManual').val('');
            $('#zoneActiveManual').prop('checked', true);
            loadZones();
        });
    }

    function saveMatrixManual() {
        var fromId = parseInt($('#matrixFromZone').val(), 10);
        var toId = parseInt($('#matrixToZone').val(), 10);
        var price = parseFloat($('#matrixPriceManual').val());
        if (!fromId || !toId) {
            alertBox('matrixManualResult', false, 'رجاءا اختر منطقة المصدر والوجهة');
            return;
        }
        if (isNaN(price) || price < 0) {
            alertBox('matrixManualResult', false, 'رجاءا ادخل سعرا صالحا');
            return;
        }

        call_ajax_json('POST', 'zones/matrix/create', {
            fromZoneId: fromId,
            toZoneId: toId,
            price: price
        }, function () {
            alertBox('matrixManualResult', true, 'تم حفظ السعر بنجاح');
            $('#matrixFromZone').val('');
            $('#matrixToZone').val('');
            $('#matrixPriceManual').val('');
            loadMatrix();
        });
    }

    $(function () {
        if ($('#zonesTableBody').length === 0) return;

        $('#uploadZonesBtn').on('click', function () {
            uploadFile('zones/upload', 'zonesFile', 'zonesResult', loadZones);
        });
        $('#uploadMatrixBtn').on('click', function () {
            uploadFile('zones/matrix/upload', 'matrixFile', 'matrixResult', loadMatrix);
        });
        $('#saveZoneManualBtn').on('click', saveZoneManual);
        $('#saveMatrixManualBtn').on('click', saveMatrixManual);

        loadZones();
        loadMatrix();
    });
})();
