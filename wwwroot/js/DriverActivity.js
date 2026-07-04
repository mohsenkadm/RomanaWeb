(function () {
    function fmtDate(d) {
        return d.getFullYear() + '-' +
            String(d.getMonth() + 1).padStart(2, '0') + '-' +
            String(d.getDate()).padStart(2, '0');
    }

    function queryParams() {
        return {
            dateFrom: $('#actDateFrom').val(),
            dateTo: $('#actDateTo').val(),
            name: $('#actDriverName').val() || ''
        };
    }

    function renderRows(rows) {
        rows = rows || [];
        if (!rows.length) {
            $('#driverActivityBody').html('<tr><td colspan="8" class="text-center">لا توجد بيانات</td></tr>');
            return;
        }
        var html = '';
        rows.forEach(function (r) {
            var avail = (r.isAvailable !== undefined ? r.isAvailable : r.IsAvailable);
            var badge = avail
                ? '<span class="badge" style="background:#4caf50;color:#fff">يعمل</span>'
                : '<span class="badge" style="background:#9e9e9e;color:#fff">متوقف</span>';
            html += '<tr>' +
                '<td>' + (r.driverName || r.DriverName || '-') + '</td>' +
                '<td>' + (r.phone || r.Phone || '-') + '</td>' +
                '<td>' + badge + '</td>' +
                '<td>' + (r.assignedOrders || r.AssignedOrders || 0) + '</td>' +
                '<td>' + (r.deliveredOrders || r.DeliveredOrders || 0) + '</td>' +
                '<td>' + (r.cancelledOrders || r.CancelledOrders || 0) + '</td>' +
                '<td>' + (r.totalDeliveryFees || r.TotalDeliveryFees || 0) + '</td>' +
                '<td>' + (r.totalRouteKm || r.TotalRouteKm || 0) + '</td>' +
                '</tr>';
        });
        $('#driverActivityBody').html(html);
    }

    function loadReport() {
        var p = queryParams();
        var url = 'driver-activity?dateFrom=' + encodeURIComponent(p.dateFrom) +
            '&dateTo=' + encodeURIComponent(p.dateTo) +
            '&name=' + encodeURIComponent(p.name);
        call_ajax('GET', url, null, renderRows);
    }

    $('#btnActSearch').on('click', loadReport);

    $('#btnActExcel').on('click', function () {
        var p = queryParams();
        window.open('/driver-activity/excel?dateFrom=' + encodeURIComponent(p.dateFrom) +
            '&dateTo=' + encodeURIComponent(p.dateTo) +
            '&name=' + encodeURIComponent(p.name));
    });

    var today = new Date();
    var monthAgo = new Date();
    monthAgo.setDate(today.getDate() - 30);
    $('#actDateFrom').val(fmtDate(monthAgo));
    $('#actDateTo').val(fmtDate(today));
    loadReport();
})();
