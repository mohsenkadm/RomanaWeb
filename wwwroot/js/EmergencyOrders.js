(function () {
    var driversCache = [];
    var refreshTimer = null;

    function fmtIqd(n) {
        n = Math.round(n || 0);
        return n.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',') + ' د.ع';
    }

    function escapeHtml(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function renderIdleDrivers(rows) {
        rows = rows || [];
        $('#idleDriversCount').text(rows.length);
        if (!rows.length) {
            $('#idleDriversBody').html('<tr><td colspan="4" class="text-center text-muted">لا يوجد مندوبون فارغون حالياً</td></tr>');
            return;
        }
        var html = '';
        rows.forEach(function (d) {
            var name = d.name || d.Name || '-';
            var phone = d.phone || d.Phone || '-';
            html += '<tr>' +
                '<td>' + escapeHtml(name) + '</td>' +
                '<td>' + escapeHtml(phone) + '</td>' +
                '<td><span class="badge badge-success">فارغ</span></td>' +
                '<td>0</td>' +
                '</tr>';
        });
        $('#idleDriversBody').html(html);
    }

    function loadIdleDrivers() {
        call_ajax('GET', 'emergency-orders/idle-drivers', null, renderIdleDrivers);
    }

    function renderRows(rows) {
        rows = rows || [];
        $('#emergencyCount').text(rows.length ? ('عدد الطلبات: ' + rows.length) : 'لا توجد طلبات طوارئ');
        if (!rows.length) {
            $('#emergencyBody').html('<tr><td colspan="8" class="text-center text-success">لا توجد طلبات بدون مندوب</td></tr>');
            return;
        }
        var html = '';
        rows.forEach(function (r) {
            var orderId = r.orderId || r.OrderId;
            var orderNo = r.orderNo || r.OrderNo;
            html += '<tr>' +
                '<td><strong>' + orderNo + '</strong></td>' +
                '<td>' + (r.restaurantName || r.RestaurantName || '-') + '</td>' +
                '<td>' + (r.userName || r.UserName || '-') + '</td>' +
                '<td>' + (r.phone || r.Phone || '-') + '</td>' +
                '<td>' + fmtIqd(r.netAmount || r.NetAmount) + '</td>' +
                '<td>' + fmtIqd(r.costDelivery || r.CostDelivery) + '</td>' +
                '<td><span class="badge badge-warning">' + (r.waitingMinutes || r.WaitingMinutes || 0) + '</span></td>' +
                '<td>' +
                '<button class="btn btn-sm btn-primary btn-redispatch" data-id="' + orderId + '" title="إعادة إرسال"><i class="material-icons" style="font-size:16px">send</i></button> ' +
                '<button class="btn btn-sm btn-success btn-assign" data-id="' + orderId + '" title="تعيين مندوب"><i class="material-icons" style="font-size:16px">person_add</i></button>' +
                '</td></tr>';
        });
        $('#emergencyBody').html(html);
    }

    function loadList() {
        call_ajax('GET', 'emergency-orders', null, renderRows);
        loadIdleDrivers();
    }

    function loadDrivers(cb) {
        call_ajax('GET', 'emergency-orders/drivers', null, function (data) {
            driversCache = data || [];
            var opts = '<option value="">— اختر —</option>';
            driversCache.forEach(function (d) {
                var id = d.saleManId || d.SaleManId;
                var name = d.name || d.Name || '-';
                var phone = d.phone || d.Phone || '';
                var avail = (d.isAvailable !== undefined ? d.isAvailable : d.IsAvailable);
                var idle = (d.isIdle !== undefined ? d.isIdle : d.IsIdle);
                var active = d.activeOrders != null ? d.activeOrders : (d.ActiveOrders || 0);
                var tag = !avail ? ' (متوقف)' : (idle ? ' — فارغ' : ' — مشغول (' + active + ')');
                opts += '<option value="' + id + '"' + (!avail ? ' disabled' : '') + '>' +
                    escapeHtml(name) + (phone ? ' | ' + escapeHtml(phone) : '') + tag + '</option>';
            });
            $('#assignDriverSelect').html(opts);
            if (cb) cb();
        });
    }

    $('#btnEmergencyRefresh').on('click', loadList);

    $(document).on('click', '.btn-redispatch', function () {
        var id = $(this).data('id');
        if (!confirm('إعادة إرسال الطلب #' + id + ' لأقرب المندوبين؟')) return;
        call_ajax('POST', 'emergency-orders/' + id + '/redispatch?radius_km=5', null, function () {
            alert('تم إرسال الطلب للمندوبين');
            loadList();
        });
    });

    $(document).on('click', '.btn-assign', function () {
        var id = $(this).data('id');
        $('#assignOrderId').val(id);
        loadDrivers(function () {
            $('#assignDriverModal').modal('show');
        });
    });

    $('#btnConfirmAssign').on('click', function () {
        var orderId = $('#assignOrderId').val();
        var saleManId = parseInt($('#assignDriverSelect').val(), 10);
        if (!saleManId) { alert('اختر مندوباً'); return; }
        call_ajax_json('POST', 'emergency-orders/' + orderId + '/assign', { saleManId: saleManId }, function () {
            $('#assignDriverModal').modal('hide');
            alert('تم تعيين المندوب');
            loadList();
        });
    });

    loadList();
    refreshTimer = setInterval(loadList, 60000);
})();
