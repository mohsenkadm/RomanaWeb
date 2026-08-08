(function () {
    var ordersCache = [];
    var currentFilter = 'all';
    var searchTimer = null;

    function fmtDate(d) {
        return d.getFullYear() + '-' +
            String(d.getMonth() + 1).padStart(2, '0') + '-' +
            String(d.getDate()).padStart(2, '0');
    }

    function fmtDateTime(value) {
        if (!value) return '—';
        var d = new Date(value);
        if (isNaN(d.getTime())) return String(value);
        return d.toLocaleString('ar-IQ', {
            year: 'numeric', month: '2-digit', day: '2-digit',
            hour: '2-digit', minute: '2-digit'
        });
    }

    function fmtMoney(n) {
        var v = Number(n) || 0;
        return v.toLocaleString('en-US', { maximumFractionDigits: 0 }) + ' د.ع';
    }

    function escapeHtml(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    function queryParams() {
        return {
            dateFrom: $('#actDateFrom').val(),
            dateTo: $('#actDateTo').val(),
            name: $('#actDriverName').val() || ''
        };
    }

    function val(obj) {
        var keys = Array.prototype.slice.call(arguments, 1);
        for (var i = 0; i < keys.length; i++) {
            if (obj[keys[i]] !== undefined && obj[keys[i]] !== null) return obj[keys[i]];
        }
        return undefined;
    }

    function renderRows(rows) {
        rows = rows || [];
        if (!rows.length) {
            $('#driverActivityBody').html('<tr><td colspan="9" class="text-center">لا توجد بيانات</td></tr>');
            return;
        }
        var html = '';
        rows.forEach(function (r) {
            var id = val(r, 'saleManId', 'SaleManId');
            var name = val(r, 'driverName', 'DriverName') || '-';
            var phone = val(r, 'phone', 'Phone') || '-';
            var avail = val(r, 'isAvailable', 'IsAvailable');
            var badge = avail
                ? '<span class="badge" style="background:#4caf50;color:#fff">يعمل</span>'
                : '<span class="badge" style="background:#9e9e9e;color:#fff">متوقف</span>';
            html += '<tr>' +
                '<td>' + escapeHtml(name) + '</td>' +
                '<td>' + escapeHtml(phone) + '</td>' +
                '<td>' + badge + '</td>' +
                '<td>' + (val(r, 'assignedOrders', 'AssignedOrders') || 0) + '</td>' +
                '<td>' + (val(r, 'deliveredOrders', 'DeliveredOrders') || 0) + '</td>' +
                '<td>' + (val(r, 'cancelledOrders', 'CancelledOrders') || 0) + '</td>' +
                '<td>' + fmtMoney(val(r, 'totalDeliveryFees', 'TotalDeliveryFees') || 0) + '</td>' +
                '<td>' + (val(r, 'totalRouteKm', 'TotalRouteKm') || 0) + '</td>' +
                '<td class="da-actions-col">' +
                '<button type="button" class="da-btn-details" data-id="' + id + '" data-name="' + escapeHtml(name) + '" data-phone="' + escapeHtml(phone) + '">' +
                '<i class="material-icons">visibility</i> تفاصيل</button></td>' +
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

    function statusClass(group) {
        if (group === 'delivered') return 'da-status-delivered';
        if (group === 'cancelled') return 'da-status-cancelled';
        if (group === 'active') return 'da-status-active';
        return 'da-status-pending';
    }

    function initialOf(name) {
        var t = String(name || 'م').trim();
        return t ? t.charAt(0) : 'م';
    }

    function filteredOrders() {
        var q = ($('#daOrderSearch').val() || '').trim().toLowerCase();
        return ordersCache.filter(function (o) {
            var group = val(o, 'statusGroup', 'StatusGroup') || 'active';
            if (currentFilter !== 'all' && group !== currentFilter) return false;
            if (!q) return true;
            var hay = [
                val(o, 'orderNo', 'OrderNo'),
                val(o, 'restaurantName', 'RestaurantName'),
                val(o, 'customerName', 'CustomerName'),
                val(o, 'deliveryAddress', 'DeliveryAddress'),
                val(o, 'customerPhone', 'CustomerPhone'),
                val(o, 'statusText', 'StatusText')
            ].join(' ').toLowerCase();
            return hay.indexOf(q) !== -1;
        });
    }

    function renderOrderCards() {
        var rows = filteredOrders();
        $('#daVisibleCount').text(rows.length ? ('عرض ' + rows.length + ' من ' + ordersCache.length + ' طلب') : '');

        if (!ordersCache.length) {
            $('#daOrdersBody').html(
                '<div class="da-empty"><i class="material-icons">inbox</i>لا توجد طلبات لهذا المندوب في الفترة المحددة</div>'
            );
            return;
        }
        if (!rows.length) {
            $('#daOrdersBody').html(
                '<div class="da-empty"><i class="material-icons">search_off</i>لا نتائج مطابقة للبحث أو الفلتر</div>'
            );
            return;
        }

        var html = '<div class="da-order-list">';
        rows.forEach(function (o) {
            var group = val(o, 'statusGroup', 'StatusGroup') || 'active';
            var statusText = val(o, 'statusText', 'StatusText') || '—';
            var orderNo = val(o, 'orderNo', 'OrderNo') || '—';
            var restaurant = val(o, 'restaurantName', 'RestaurantName') || '—';
            var address = val(o, 'deliveryAddress', 'DeliveryAddress') || '—';
            var customer = val(o, 'customerName', 'CustomerName') || '—';
            var phone = val(o, 'customerPhone', 'CustomerPhone') || '—';
            var amount = val(o, 'orderAmount', 'OrderAmount') || 0;
            var fee = val(o, 'deliveryFee', 'DeliveryFee') || 0;
            var km = val(o, 'routeDistanceKm', 'RouteDistanceKm');
            var fromZone = val(o, 'fromZone', 'FromZone');
            var toZone = val(o, 'toZone', 'ToZone');
            var notes = val(o, 'notes', 'Notes');
            var zoneLine = (fromZone || toZone)
                ? (escapeHtml(fromZone || '—') + ' ← ' + escapeHtml(toZone || '—'))
                : '—';

            html += '<article class="da-order-card">' +
                '<div class="da-order-top">' +
                '<div><h6 class="da-order-no"><span>#' + escapeHtml(orderNo) + '</span> طلب رقم ' + escapeHtml(orderNo) + '</h6>' +
                '<p class="da-order-date"><i class="material-icons" style="font-size:14px;vertical-align:middle">event</i> ' +
                escapeHtml(fmtDateTime(val(o, 'orderDate', 'OrderDate'))) + '</p></div>' +
                '<span class="da-status ' + statusClass(group) + '">' + escapeHtml(statusText) + '</span>' +
                '</div>' +
                '<div class="da-order-grid">' +
                '<div><span class="da-field-label">المطعم</span><span class="da-field-value">' + escapeHtml(restaurant) + '</span></div>' +
                '<div><span class="da-field-label">عنوان التوصيل</span><span class="da-field-value">' + escapeHtml(address) + '</span></div>' +
                '<div><span class="da-field-label">الزبون</span><span class="da-field-value">' + escapeHtml(customer) +
                (phone && phone !== '—' ? ' · ' + escapeHtml(phone) : '') + '</span></div>' +
                '<div><span class="da-field-label">منطقة التسعير</span><span class="da-field-value">' + zoneLine + '</span></div>' +
                '<div><span class="da-field-label">مبلغ الطلب</span><span class="da-field-value da-field-money">' + fmtMoney(amount) + '</span></div>' +
                '<div><span class="da-field-label">رسوم التوصيل</span><span class="da-field-value da-field-money">' + fmtMoney(fee) + '</span></div>' +
                '</div>' +
                '<div class="da-order-footer">' +
                '<span>المسافة: <strong>' + (km != null && km !== '' ? escapeHtml(km) + ' كم' : '—') + '</strong></span>' +
                (notes ? '<span>ملاحظة: <strong>' + escapeHtml(notes) + '</strong></span>' : '') +
                '</div></article>';
        });
        html += '</div>';
        $('#daOrdersBody').html(html);
    }

    function fillSummary(summary) {
        summary = summary || {};
        $('#daStatAll').text(val(summary, 'totalOrders', 'TotalOrders') || 0);
        $('#daStatDelivered').text(val(summary, 'delivered', 'Delivered') || 0);
        $('#daStatCancelled').text(val(summary, 'cancelled', 'Cancelled') || 0);
        $('#daStatFees').text(fmtMoney(val(summary, 'totalDeliveryFees', 'TotalDeliveryFees') || 0));
    }

    function openDriverDetails(saleManId, fallbackName, fallbackPhone) {
        currentFilter = 'all';
        ordersCache = [];
        $('#daOrderSearch').val('');
        $('.da-filter').removeClass('active');
        $('.da-filter[data-filter="all"]').addClass('active');
        $('#driverOrdersModalTitle').text(fallbackName || 'تفاصيل الطلبات');
        $('#daDriverMeta').text((fallbackPhone || '—') + ' · جاري التحميل...');
        $('#daAvatar').text(initialOf(fallbackName));
        fillSummary({});
        $('#daOrdersBody').html('<div class="da-loading"><i class="material-icons">autorenew</i>جاري تحميل الطلبات...</div>');
        $('#daVisibleCount').text('');
        $('#driverOrdersModal').modal('show');

        var p = queryParams();
        var url = 'driver-activity/' + saleManId + '/orders?dateFrom=' + encodeURIComponent(p.dateFrom) +
            '&dateTo=' + encodeURIComponent(p.dateTo) +
            '&_=' + Date.now();

        call_ajax('GET', url, null, function (data) {
            data = data || {};
            var driver = val(data, 'driver', 'Driver') || {};
            var name = val(driver, 'name', 'Name') || fallbackName || 'المندوب';
            var phone = val(driver, 'phone', 'Phone') || fallbackPhone || '—';
            var from = val(data, 'dateFrom', 'DateFrom');
            var to = val(data, 'dateTo', 'DateTo');
            var range = '';
            if (from || to) {
                range = ' · الفترة: ' + fmtDateTime(from).split(',')[0] + ' → ' + fmtDateTime(to).split(',')[0];
            }
            $('#driverOrdersModalTitle').text(name);
            $('#daDriverMeta').text(phone + range);
            $('#daAvatar').text(initialOf(name));
            fillSummary(val(data, 'summary', 'Summary'));
            ordersCache = val(data, 'orders', 'Orders') || [];
            renderOrderCards();
        });
    }

    $('#btnActSearch').on('click', loadReport);

    $('#btnActExcel').on('click', function () {
        var p = queryParams();
        window.open('/driver-activity/excel?dateFrom=' + encodeURIComponent(p.dateFrom) +
            '&dateTo=' + encodeURIComponent(p.dateTo) +
            '&name=' + encodeURIComponent(p.name));
    });

    $('#driverActivityBody').on('click', '.da-btn-details', function () {
        var id = $(this).data('id');
        if (!id) return;
        openDriverDetails(id, $(this).data('name'), $(this).data('phone'));
    });

    $('.da-filter').on('click', function () {
        $('.da-filter').removeClass('active');
        $(this).addClass('active');
        currentFilter = $(this).data('filter') || 'all';
        renderOrderCards();
    });

    $('#daOrderSearch').on('input', function () {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(renderOrderCards, 160);
    });

    var today = new Date();
    var monthAgo = new Date();
    monthAgo.setDate(today.getDate() - 30);
    $('#actDateFrom').val(fmtDate(monthAgo));
    $('#actDateTo').val(fmtDate(today));
    loadReport();
})();
