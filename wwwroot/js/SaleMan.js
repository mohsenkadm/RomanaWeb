var _SaleManId = 0;

function escapeHtmlSaleMan(s) {
    return String(s == null ? '' : s)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

function filltableSaleMan(data) {
    var $wrap = $('#saleManCards');
    $wrap.empty();
    if (!data || data.length === 0) {
        $wrap.html('<div class="saleman-empty">لا توجد مندوبين</div>');
        md.showNotification('لا توجد معلومات');
        return;
    }

    $.each(data, function (i, item) {
        var id = item.saleManId;
        var active = item.isActive !== false && item.isActive !== 0;
        var working = (item.isAvailable === undefined) ? true : !!item.isAvailable;
        var multi = !!item.allowMultiOrders;
        var maxN = item.maxConcurrentOrders > 0 ? item.maxConcurrentOrders : 1;

        var activeBadge = active
            ? '<span class="sm-status sm-status-ok">نشط</span>'
            : '<span class="sm-status sm-status-off">ملغى</span>';
        var workBadge = working
            ? '<span class="sm-status sm-status-ok">يعمل</span>'
            : '<span class="sm-status sm-status-muted">متوقف</span>';
        var multiBadge = multi
            ? '<span class="sm-status sm-status-info">متعدد (' + maxN + ')</span>'
            : '<span class="sm-status sm-status-muted">طلب واحد</span>';

        var restaurantsHtml = (typeof RestaurantPicker !== 'undefined')
            ? RestaurantPicker.saleManRestaurantLabels(id)
            : '<span class="text-muted">—</span>';
        var zonesHtml = (typeof ZonePicker !== 'undefined')
            ? ZonePicker.saleManZoneLabels(id)
            : '<span class="text-muted">—</span>';

        var html =
            '<article class="saleman-card">' +
                '<div class="saleman-card-top">' +
                    '<div class="saleman-card-identity">' +
                        '<div class="saleman-avatar"><i class="material-icons">person</i></div>' +
                        '<div>' +
                            '<h5 class="saleman-card-name">' + escapeHtmlSaleMan(item.name || '') + '</h5>' +
                            '<div class="saleman-card-meta">' +
                                '<span><i class="material-icons">phone</i> ' + escapeHtmlSaleMan(item.phone || '—') + '</span>' +
                                (item.address ? '<span><i class="material-icons">place</i> ' + escapeHtmlSaleMan(item.address) + '</span>' : '') +
                            '</div>' +
                        '</div>' +
                    '</div>' +
                    '<div class="saleman-card-badges">' +
                        activeBadge + workBadge + multiBadge +
                    '</div>' +
                '</div>' +
                '<div class="saleman-card-assign">' +
                    '<div class="saleman-assign-block saleman-assign-block-res">' +
                        '<div class="saleman-assign-label"><i class="material-icons">storefront</i> المطاعم</div>' +
                        '<div class="saleman-assign-tags">' + restaurantsHtml + '</div>' +
                    '</div>' +
                    '<div class="saleman-assign-block saleman-assign-block-zone">' +
                        '<div class="saleman-assign-label"><i class="material-icons">map</i> الزونات</div>' +
                        '<div class="saleman-assign-tags">' + zonesHtml + '</div>' +
                    '</div>' +
                '</div>' +
                '<div class="saleman-card-actions">' +
                    '<button type="button" class="btn btn-sm ' + (active ? 'btn-danger' : 'btn-success') + '" onclick="toggleSaleManActive(' + id + ',' + (!active) + ')">' +
                        (active ? 'الغاء تنشيط' : 'تنشيط') +
                    '</button>' +
                    '<button type="button" class="btn btn-sm ' + (working ? 'btn-warning' : 'btn-success') + '" ' +
                        (active ? '' : 'disabled title="المندوب غير نشط" ') +
                        'onclick="toggleSaleManAvailability(' + id + ',' + (!working) + ')">' +
                        (working ? 'ايقاف العمل' : 'تفعيل العمل') +
                    '</button>' +
                    '<button type="button" class="btn btn-sm btn-primary" onclick="updateSaleMan(' + id + ')" data-toggle="modal" data-target="#SaleManModal">تعديل</button>' +
                    '<button type="button" class="btn btn-sm btn-outline-danger" onclick="deleteSaleMan(' + id + ')">حذف</button>' +
                '</div>' +
            '</article>';

        $wrap.append(html);
    });
}

function toggleSaleManActive(id, makeActive) {
    var msg = makeActive
        ? 'هل تريد تنشيط هذا المندوب؟ سيتمكن من استخدام التطبيق واستلام الطلبات.'
        : 'هل تريد الغاء تنشيط هذا المندوب؟ سيبقى حسابه لكن لن يتمكن من استخدام التطبيق ولن تُجلب له طلبات.';
    if (!confirm(msg)) return;
    call_ajax('POST', 'SaleMan/SetActive?Id=' + id + '&isActive=' + (!!makeActive),
        null, RefreshSaleMan);
}

function toggleSaleManAvailability(id, makeAvailable) {
    var verb = makeAvailable ? 'تفعيل' : 'ايقاف';
    if (!confirm('هل تريد ' + verb + ' حالة العمل لهذا المندوب؟')) return;
    call_ajax('POST', 'SaleMan/SetAvailability?Id=' + id + '&isAvailable=' + (!!makeAvailable),
        null, RefreshSaleMan);
}

function deleteSaleMan(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        call_ajax("DELETE", "SaleMan/Delete", { Id: id }, RefreshSaleMan);
    }
}

function RefreshSaleMan() {
    var obj = { Name: $("#Namese").val() };
    function loadTable() {
        call_ajax("GET", "SaleMan/GetAll", obj, filltableSaleMan);
    }
    function afterZones() {
        if (typeof RestaurantPicker !== 'undefined') {
            RestaurantPicker.invalidateSummary();
            RestaurantPicker.loadSummary(loadTable);
        } else {
            loadTable();
        }
    }
    if (typeof ZonePicker !== 'undefined') {
        ZonePicker.invalidateSummary();
        ZonePicker.loadSummary(afterZones);
    } else {
        afterZones();
    }
}

function openAddSaleMan() {
    _SaleManId = 0;
    $("#SaleManModalLabel").text("اضافة جديد");
    $("#Name").val('');
    $("#Phone").val('');
    $("#Address").val('');
    $("#Password").val('');
    $("#Password").attr('placeholder', 'اكتب كلمة المرور');
    $("#IsActive").prop("checked", true);
    $("#AllowMultiOrders").prop("checked", false);
    $("#MaxConcurrentOrders").val(2);
    $("#MaxConcurrentOrdersWrap").hide();
    if (typeof ZonePicker !== 'undefined') {
        ZonePicker.render('saleManZonePicker', []);
    }
    if (typeof RestaurantPicker !== 'undefined') {
        RestaurantPicker.render('saleManRestaurantPicker', []);
    }
}

function updateSaleMan(id) {
    call_ajax("GET", "SaleMan/GetById", { Id: id }, setdataSaleMan);
    _SaleManId = id;
    $("#SaleManModalLabel").text("تعديل المندوب");
    if (typeof ZonePicker !== 'undefined') {
        ZonePicker.loadSaleMan(id, 'saleManZonePicker');
    }
    if (typeof RestaurantPicker !== 'undefined') {
        RestaurantPicker.loadSaleMan(id, 'saleManRestaurantPicker');
    }
}

function setdataSaleMan(data) {
    $("#Name").val(data.name);
    $("#Phone").val(data.phone);
    $("#Address").val(data.address || '');
    $("#Password").val('');
    $("#Password").attr('placeholder', 'اترك فارغاً للإبقاء على كلمة المرور');
    $("#IsActive").prop("checked", !!data.isActive);
    var multi = !!data.allowMultiOrders;
    $("#AllowMultiOrders").prop("checked", multi);
    $("#MaxConcurrentOrders").val(multi ? Math.max(2, data.maxConcurrentOrders || 2) : 2);
    $("#MaxConcurrentOrdersWrap").toggle(multi);
}

function aftersaveSaleMan() {
    $("#Name").val('');
    $("#Phone").val('');
    $("#Address").val('');
    $("#Password").val('');
    $("#IsActive").prop("checked", true);
    $("#AllowMultiOrders").prop("checked", false);
    $("#MaxConcurrentOrders").val(2);
    $("#MaxConcurrentOrdersWrap").hide();
    if (typeof ZonePicker !== 'undefined') {
        ZonePicker.render('saleManZonePicker', []);
        ZonePicker.invalidateSummary();
    }
    if (typeof RestaurantPicker !== 'undefined') {
        RestaurantPicker.render('saleManRestaurantPicker', []);
        RestaurantPicker.invalidateSummary();
    }
    _SaleManId = 0;
    $("#SaleManModalLabel").text("اضافة جديد");
    $('#SaleManModal').modal('hide');
    RefreshSaleMan();
}
