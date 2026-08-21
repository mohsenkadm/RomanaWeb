(function () {
    var STATUS_LABELS = {
        0: 'قيد الانتظار',
        1: 'قيد المعالجة',
        2: 'حُلت',
        3: 'لم يتم حلها'
    };

    function val(obj, a, b) {
        if (!obj) return undefined;
        if (obj[a] !== undefined && obj[a] !== null) return obj[a];
        if (b && obj[b] !== undefined && obj[b] !== null) return obj[b];
        return undefined;
    }

    function fmtDate(v) {
        if (!v) return '-';
        var d = new Date(v);
        if (isNaN(d.getTime())) return v;
        return d.toLocaleString('ar-IQ');
    }

    function esc(s) {
        return String(s == null ? '' : s)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function statusBadge(status) {
        var s = parseInt(status, 10);
        if (isNaN(s)) s = 0;
        var label = STATUS_LABELS[s] || 'غير معروف';
        return '<span class="pr-badge pr-badge-' + s + '">' + label + '</span>';
    }

    function updateStats(stats) {
        stats = stats || {};
        $('#prTotal').text(stats.total != null ? stats.total : '-');
        $('#prPending').text(stats.pending != null ? stats.pending : '-');
        $('#prInProgress').text(stats.inProgress != null ? stats.inProgress : '-');
        $('#prResolved').text(stats.resolved != null ? stats.resolved : '-');
        $('#prUnresolved').text(stats.unresolved != null ? stats.unresolved : '-');
    }

    function renderRows(rows) {
        rows = rows || [];
        if (!rows.length) {
            $('#prTableBody').html('<tr><td colspan="8" class="text-center text-muted">لا توجد بلاغات</td></tr>');
            return;
        }

        var html = '';
        rows.forEach(function (r, i) {
            var id = val(r, 'problemReportId', 'ProblemReportId');
            var driver = val(r, 'saleManName', 'SaleManName') || '-';
            var phone = val(r, 'saleManPhone', 'SaleManPhone') || '';
            var orderNo = val(r, 'orderNo', 'OrderNo');
            var orderId = val(r, 'orderId', 'OrderId');
            var createdAt = val(r, 'createdAt', 'CreatedAt');
            var orderDate = val(r, 'orderDate', 'OrderDate');
            var message = val(r, 'message', 'Message') || '';
            var status = val(r, 'status', 'Status');
            var note = val(r, 'adminNote', 'AdminNote') || '';

            html += '<tr style="animation-delay:' + (Math.min(i, 12) * 0.03) + 's">' +
                '<td>' + id + '</td>' +
                '<td><strong>' + esc(driver) + '</strong>' +
                    (phone ? '<br/><small class="text-muted" style="direction:ltr;display:inline-block">' + esc(phone) + '</small>' : '') +
                '</td>' +
                '<td><strong>' + (orderNo != null ? orderNo : '-') + '</strong>' +
                    '<br/><small class="text-muted">#' + (orderId || '-') + '</small></td>' +
                '<td>' + fmtDate(createdAt) + '</td>' +
                '<td>' + fmtDate(orderDate) + '</td>' +
                '<td><div class="pr-msg">' + esc(message) + '</div>' +
                    (note ? '<small class="text-muted d-block mt-1">ملاحظة: ' + esc(note) + '</small>' : '') +
                '</td>' +
                '<td>' + statusBadge(status) + '</td>' +
                '<td>' +
                    '<button type="button" class="btn btn-sm btn-primary btn-pr-status" ' +
                        'data-id="' + id + '" data-status="' + status + '" data-note="' + esc(note) + '">' +
                        'تغيير الحالة</button>' +
                '</td></tr>';
        });
        $('#prTableBody').html(html);
    }

    function filterParams() {
        var params = { take: 200 };
        var status = $('#prFilterStatus').val();
        if (status !== '') params.status = status;
        var orderNo = ($('#prFilterOrderNo').val() || '').trim();
        if (orderNo) params.orderNo = orderNo;
        var driver = ($('#prFilterDriver').val() || '').trim();
        if (driver) params.driverName = driver;
        var from = $('#prFilterFrom').val();
        if (from) params.dateFrom = from;
        var to = $('#prFilterTo').val();
        if (to) params.dateTo = to;
        return params;
    }

    function loadStats() {
        var p = filterParams();
        var q = {};
        if (p.dateFrom) q.dateFrom = p.dateFrom;
        if (p.dateTo) q.dateTo = p.dateTo;
        call_ajax('GET', 'problem-reports/stats', q, updateStats);
    }

    function loadList() {
        call_ajax('GET', 'problem-reports', filterParams(), function (rows) {
            renderRows(rows);
            loadStats();
        });
    }

    function openStatusModal(id, status, note) {
        $('#prModalId').val(id);
        $('#prModalStatus').val(String(status != null ? status : 0));
        $('#prModalNote').val(note || '');
        $('#prStatusModal').modal('show');
    }

    function saveStatus() {
        var id = parseInt($('#prModalId').val(), 10);
        if (!id) return;
        var status = parseInt($('#prModalStatus').val(), 10);
        var note = ($('#prModalNote').val() || '').trim();
        call_ajax_json('POST', 'problem-reports/' + id + '/status', {
            status: status,
            adminNote: note
        }, function () {
            $('#prStatusModal').modal('hide');
            if (typeof md !== 'undefined' && md.showNotification) {
                md.showNotification('تم تحديث الحالة');
            }
            loadList();
        });
    }

    $('#prSearchBtn, #prRefreshBtn').on('click', function (e) {
        e.preventDefault();
        loadList();
    });

    $('#prFilterOrderNo, #prFilterDriver').on('keypress', function (e) {
        if (e.which === 13) {
            e.preventDefault();
            loadList();
        }
    });

    $(document).on('click', '.btn-pr-status', function () {
        openStatusModal(
            $(this).data('id'),
            $(this).data('status'),
            $(this).attr('data-note') || ''
        );
    });

    $('#prModalSave').on('click', function (e) {
        e.preventDefault();
        saveStatus();
    });

    $(function () {
        if ($('#prTableBody').length === 0) return;
        loadList();
    });
})();
