// Section 2.2 - Zones admin UI.
// IMPORTANT: this app stores the JWT in cookie "token2" and uses the global
// call_ajax / call_ajax_withfile helpers (see site.js) to attach it as
// "Authorization: Bearer <token>". Using $.ajax directly here would send no
// token and the server returns 401. Always go through call_ajax*.
(function () {

    function getCookieSafe(name) {
        try { return getCookie(name); } catch (e) { return ''; }
    }

    function alertBox(targetId, success, msg) {
        var cls = success ? 'alert-success' : 'alert-danger';
        $('#' + targetId).html('<div class="alert ' + cls + '">' + msg + '</div>');
    }

    function loadZones() {
        call_ajax('GET', 'zones', null, function (rows) {
            rows = rows || [];
            $('#zonesCount').text(rows.length);
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
                html += '<tr>' +
                    '<td>' + (m.zonePriceId || m.ZonePriceId) + '</td>' +
                    '<td>' + (m.fromZoneId || m.FromZoneId) + '</td>' +
                    '<td>' + (m.toZoneId || m.ToZoneId) + '</td>' +
                    '<td>' + (m.price || m.Price) + '</td>' +
                    '</tr>';
            });
            $('#matrixTableBody').html(html || '<tr><td colspan="4" class="text-center">لا يوجد بيانات</td></tr>');
        });
    }

    function uploadFile(url, inputId, resultId, onDone) {
        // Read the input via jQuery to be resilient to any DOM replacement
        // done by Material Dashboard's bmd-form-group transform.
        var $input = $('#' + inputId);
        var fileInput = $input.length ? $input[0] : document.getElementById(inputId);
        if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
            alertBox(resultId, false, 'يرجى اختيار ملف Excel أولاً');
            return;
        }
        var fd = new FormData();
        fd.append('file', fileInput.files[0]);

        // call_ajax_withfile already wires the JWT from cookie "token2".
        call_ajax_withfile('POST', url, fd, function (data) {
            alertBox(resultId, true, 'تم رفع الملف بنجاح');
            if (onDone) onDone();
            // Reset so the same file can be re-uploaded.
            try { fileInput.value = ''; } catch (e) { }
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

        loadZones();
        loadMatrix();
    });
})();
