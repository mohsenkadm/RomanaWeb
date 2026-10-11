(function () {
    function val(obj, a, b) {
        if (!obj) return undefined;
        if (obj[a] !== undefined && obj[a] !== null) return obj[a];
        if (b && obj[b] !== undefined && obj[b] !== null) return obj[b];
        return undefined;
    }

    function formatIqd(n) {
        var v = Math.round(Number(n) || 0);
        try { return v.toLocaleString('en-US') + ' د.ع'; }
        catch (e) { return v + ' د.ع'; }
    }

    function formatDate(d) {
        if (!d) return '—';
        try {
            var dt = new Date(d);
            if (isNaN(dt.getTime())) return String(d);
            return dt.toLocaleString('ar-IQ');
        } catch (e) {
            return String(d);
        }
    }

    function defaultDates() {
        var to = new Date();
        var from = new Date();
        from.setDate(from.getDate() - 30);
        function iso(d) {
            var m = (d.getMonth() + 1).toString().padStart(2, '0');
            var day = d.getDate().toString().padStart(2, '0');
            return d.getFullYear() + '-' + m + '-' + day;
        }
        if (!$('#dsFrom').val()) $('#dsFrom').val(iso(from));
        if (!$('#dsTo').val()) $('#dsTo').val(iso(to));
    }

    function loadSettlements() {
        defaultDates();
        var q = {
            datefrom: $('#dsFrom').val() || null,
            dateto: $('#dsTo').val() || null,
            name: $('#dsName').val() || null
        };
        call_ajax('GET', 'SaleMan/GetSettlements', q, renderRows);
    }

    function renderRows(rows) {
        var $body = $('#dsBody');
        $body.empty();
        if (!rows || !rows.length) {
            $body.html('<tr><td colspan="11" class="text-center text-muted">لا توجد عمليات تصفير في هذه الفترة</td></tr>');
            return;
        }
        rows.forEach(function (r) {
            var id = val(r, 'settlementId', 'SettlementId');
            var name = val(r, 'saleManName', 'SaleManName') || '—';
            var phone = val(r, 'saleManPhone', 'SaleManPhone') || '—';
            var food = formatIqd(val(r, 'foodNetAmount', 'FoodNetAmount'));
            var fee = formatIqd(val(r, 'deliveryFeeAmount', 'DeliveryFeeAmount'));
            var curFood = formatIqd(val(r, 'currentFoodNetBalance', 'CurrentFoodNetBalance'));
            var curFee = formatIqd(val(r, 'currentDeliveryFeeBalance', 'CurrentDeliveryFeeBalance'));
            var count = val(r, 'ordersCount', 'OrdersCount') || 0;
            var when = formatDate(val(r, 'settledAt', 'SettledAt'));
            var admin = val(r, 'adminName', 'AdminName') || '—';

            $body.append(
                '<tr>' +
                    '<td>' + id + '</td>' +
                    '<td>' + name + '</td>' +
                    '<td>' + phone + '</td>' +
                    '<td><strong>' + food + '</strong></td>' +
                    '<td><strong>' + fee + '</strong></td>' +
                    '<td>' + curFood + '</td>' +
                    '<td>' + curFee + '</td>' +
                    '<td>' + count + '</td>' +
                    '<td>' + when + '</td>' +
                    '<td>' + admin + '</td>' +
                    '<td class="ds-actions-col">' +
                        '<button type="button" class="ds-btn-details" data-id="' + id + '">' +
                            '<i class="material-icons">list_alt</i> تفاصيل' +
                        '</button>' +
                    '</td>' +
                '</tr>'
            );
        });
    }

    function openDetails(settlementId) {
        $('#dsModalTitle').text('تفاصيل التصفير #' + settlementId);
        $('#dsModalMeta').text('جاري التحميل...');
        $('#dsOrdersBody').html('<div class="ds-empty">جاري التحميل...</div>');
        $('#dsStatFood').text('…');
        $('#dsStatFee').text('…');
        $('#dsStatCount').text('…');
        $('#dsOrdersModal').modal('show');

        call_ajax('GET', 'SaleMan/GetSettlementOrders/' + settlementId, null, function (data) {
            var settlement = val(data, 'settlement', 'Settlement') || {};
            var orders = val(data, 'orders', 'Orders') || [];
            var name = val(settlement, 'saleManName', 'SaleManName') || 'المندوب';
            var phone = val(settlement, 'saleManPhone', 'SaleManPhone') || '';
            var when = formatDate(val(settlement, 'settledAt', 'SettledAt'));

            $('#dsModalTitle').text(name + ' — تصفير #' + settlementId);
            $('#dsModalMeta').text((phone ? phone + ' · ' : '') + when);
            $('#dsStatFood').text(formatIqd(val(settlement, 'foodNetAmount', 'FoodNetAmount')));
            $('#dsStatFee').text(formatIqd(val(settlement, 'deliveryFeeAmount', 'DeliveryFeeAmount')));
            $('#dsStatCount').text(orders.length);

            if (!orders.length) {
                $('#dsOrdersBody').html('<div class="ds-empty">لا توجد طلبات مرتبطة بهذه العملية</div>');
                return;
            }

            var html = '<div class="ds-order-row" style="font-weight:700;color:#64748b">' +
                '<div>رقم الطلب</div><div>المطعم / الزبون</div><div>أكل صافي</div><div>أجور توصيل</div></div>';
            orders.forEach(function (o) {
                var no = val(o, 'orderNo', 'OrderNo') || val(o, 'orderId', 'OrderId');
                var res = val(o, 'restaurantName', 'RestaurantName') || '—';
                var user = val(o, 'userName', 'UserName') || '';
                var food = formatIqd(val(o, 'foodNetAmount', 'FoodNetAmount'));
                var fee = formatIqd(val(o, 'deliveryFeeAmount', 'DeliveryFeeAmount'));
                html +=
                    '<div class="ds-order-row">' +
                        '<div><strong>#' + no + '</strong></div>' +
                        '<div>' + res + (user ? '<br><small class="text-muted">' + user + '</small>' : '') + '</div>' +
                        '<div>' + food + '</div>' +
                        '<div>' + fee + '</div>' +
                    '</div>';
            });
            $('#dsOrdersBody').html(html);
        });
    }

    $(document).ready(function () {
        if (!$('#dsBody').length) return;
        defaultDates();
        loadSettlements();
        $('#dsSearchBtn').on('click', loadSettlements);
        $('#dsName').on('keypress', function (e) {
            if (e.which === 13) loadSettlements();
        });
        $('#dsBody').on('click', '.ds-btn-details', function () {
            var id = parseInt($(this).data('id'), 10);
            if (id) openDetails(id);
        });
    });
})();
