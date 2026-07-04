(function () {
    function fmtDate(v) {
        if (!v) return '-';
        var d = new Date(v);
        if (isNaN(d.getTime())) return v;
        return d.toLocaleString('ar-IQ');
    }

    function mapLink(lat, lng) {
        if (!lat || !lng) return '-';
        var url = 'https://www.google.com/maps?q=' + lat + ',' + lng;
        return '<a href="' + url + '" target="_blank" rel="noopener">عرض الخريطة</a>';
    }

    function updateStats(rows) {
        rows = rows || [];
        var pending = rows.filter(function (r) { return !(r.isProcessed || r.IsProcessed); }).length;
        $('#covTotal').text(rows.length);
        $('#covPending').text(pending);
    }

    function renderRows(rows) {
        rows = rows || [];
        updateStats(rows);

        if (!rows.length) {
            $('#covTableBody').html('<tr><td colspan="8" class="text-center text-muted">لا توجد طلبات</td></tr>');
            return;
        }

        var html = '';
        rows.forEach(function (r) {
            var id = r.serviceCoverageRequestId || r.ServiceCoverageRequestId;
            var isProcessed = !!(r.isProcessed || r.IsProcessed);
            var lat = r.lat || r.Lat;
            var lng = r.lng || r.Lng;
            html += '<tr>' +
                '<td>' + id + '</td>' +
                '<td>' + (r.name || r.Name || '-') + '</td>' +
                '<td>' + (r.phone || r.Phone || '-') + '</td>' +
                '<td>' + (r.address || r.Address || '-') + '</td>' +
                '<td style="direction:ltr;text-align:left;font-size:12px;">' +
                    lat + ', ' + lng + '<br/>' + mapLink(lat, lng) +
                '</td>' +
                '<td>' + fmtDate(r.createdAt || r.CreatedAt) + '</td>' +
                '<td>' + (isProcessed
                    ? '<span class="badge badge-success">تمت المعالجة</span>'
                    : '<span class="badge badge-warning">بانتظار</span>') + '</td>' +
                '<td>' +
                (isProcessed
                    ? '<button class="btn btn-sm btn-secondary btn-cov-toggle" data-id="' + id + '" data-done="0">إرجاع</button>'
                    : '<button class="btn btn-sm btn-success btn-cov-toggle" data-id="' + id + '" data-done="1">تمت المعالجة</button>') +
                '</td></tr>';
        });
        $('#covTableBody').html(html);
    }

    function loadList() {
        var status = $('#covFilterStatus').val();
        var params = {
            name: ($('#covFilterName').val() || '').trim(),
            phone: ($('#covFilterPhone').val() || '').trim(),
            take: 200
        };
        if (status !== '') params.processed = status;

        call_ajax('GET', 'coverage-requests', params, renderRows);
    }

    $('#covSearchBtn, #covRefreshBtn').on('click', function (e) {
        e.preventDefault();
        loadList();
    });

    $('#covFilterName, #covFilterPhone').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            loadList();
        }
    });

    $(document).on('click', '.btn-cov-toggle', function () {
        var id = $(this).data('id');
        var done = $(this).data('done') === 1 || $(this).data('done') === '1';
        call_ajax_json('POST', 'coverage-requests/' + id + '/processed', { isProcessed: done }, function () {
            loadList();
        });
    });

    $(function () {
        if ($('#covTableBody').length === 0) return;
        loadList();
    });
})();
