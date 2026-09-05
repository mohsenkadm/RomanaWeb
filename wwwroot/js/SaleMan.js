var _SaleManId = 0;

function filltableSaleMan(data) {
    $('#tableSaleMan').empty();
    if (data.length === 0) {  
        md.showNotification('لا توجد معلومات');
        return;
    } 
    $.each(data, function (i, item) {
        // نشاط الحساب: تنشيط / الغاء تنشيط (IsActive) — الحساب يبقى لكن التطبيق يُقفل.
        var active = item.isActive !== false && item.isActive !== 0;
        var activeBadge = active
            ? '<span class="badge" style="background:#4CAF50;color:#fff;padding:4px 10px;border-radius:12px;">نشط</span>'
            : '<span class="badge" style="background:#F44336;color:#fff;padding:4px 10px;border-radius:12px;">ملغى</span>';
        var activeBtnLabel = active ? 'الغاء تنشيط' : 'تنشيط';
        var activeBtnClass = active ? 'btn-danger' : 'btn-success';
        var activityCell =
            '<td>' + activeBadge +
            ' <button type="button" class="btn btn-sm ' + activeBtnClass + '" ' +
                'style="margin-right:6px" ' +
                'onclick="toggleSaleManActive(' + item.saleManId + ',' + (!active) + ')">' +
                activeBtnLabel + '</button></td>';

        // حالة العمل: تفعيل / ايقاف (IsAvailable).
        var working = (item.isAvailable === undefined) ? true : !!item.isAvailable;
        var badge = working
            ? '<span class="badge" style="background:#4CAF50;color:#fff;padding:4px 10px;border-radius:12px;">يعمل</span>'
            : '<span class="badge" style="background:#9E9E9E;color:#fff;padding:4px 10px;border-radius:12px;">متوقف</span>';
        var btnLabel = working ? 'ايقاف' : 'تفعيل';
        var btnClass = working ? 'btn-warning' : 'btn-success';
        var availabilityCell =
            '<td>' + badge +
            ' <button type="button" class="btn btn-sm ' + btnClass + '" ' +
                'style="margin-right:6px" ' +
                (active ? '' : 'disabled title="المندوب غير نشط" ') +
                'onclick="toggleSaleManAvailability(' + item.saleManId + ',' + (!working) + ')">' +
                btnLabel + '</button></td>';

        var multi = !!item.allowMultiOrders;
        var maxN = item.maxConcurrentOrders > 0 ? item.maxConcurrentOrders : 1;
        var multiCell = multi
            ? '<td><span class="badge" style="background:#2196F3;color:#fff;padding:4px 10px;border-radius:12px;">متعدد (' + maxN + ')</span></td>'
            : '<td><span class="badge" style="background:#607D8B;color:#fff;padding:4px 10px;border-radius:12px;">طلب واحد</span></td>';

        var zoneCell = typeof ZonePicker !== 'undefined'
            ? '<td class="zone-tags-cell">' + ZonePicker.saleManZoneLabels(item.saleManId) + '</td>'
            : '<td>—</td>';

        var rows = "<tr>" + 
            activityCell +
            availabilityCell +
            multiCell +
            "<td>—</td>" +  
            "<td>" + (item.address || '') + "</td>" +   
            "<td>" + (item.phone || '') + "</td>" +
            "<td>" + (item.name || '') + "</td>" +
            zoneCell
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteSaleMan(" + item.saleManId + ")'>حذف</button>"
            + " | <button type='button' class='btn btn-primary' onclick='updateSaleMan(" + item.saleManId + ")' data-toggle='modal' data-target='#SaleManModal'>تعديل</button></td></tr>";
        $('#tableSaleMan').append(rows);  
    });
}

// نشاط المندوب: تنشيط / الغاء تنشيط (الحساب يبقى، التطبيق يُقفل عند الإلغاء).
function toggleSaleManActive(id, makeActive) {
    var msg = makeActive
        ? 'هل تريد تنشيط هذا المندوب؟ سيتمكن من استخدام التطبيق واستلام الطلبات.'
        : 'هل تريد الغاء تنشيط هذا المندوب؟ سيبقى حسابه لكن لن يتمكن من استخدام التطبيق ولن تُجلب له طلبات.';
    if (!confirm(msg)) return;
    call_ajax('POST', 'SaleMan/SetActive?Id=' + id + '&isActive=' + (!!makeActive),
        null, RefreshSaleMan);
}

// حالة العمل: تفعيل / ايقاف.
function toggleSaleManAvailability(id, makeAvailable) {
    var verb = makeAvailable ? 'تفعيل' : 'ايقاف';
    if (!confirm('هل تريد ' + verb + ' حالة العمل لهذا المندوب؟')) return;
    call_ajax('POST', 'SaleMan/SetAvailability?Id=' + id + '&isAvailable=' + (!!makeAvailable),
        null, RefreshSaleMan);
}

function deleteSaleMan(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "SaleMan/Delete", object1, RefreshSaleMan);
    }
}
function RefreshSaleMan() { 
    var obj = { Name: $("#Namese").val() };
    if (typeof ZonePicker !== 'undefined') {
        ZonePicker.loadSummary(function () {
            call_ajax("GET", "SaleMan/GetAll", obj, filltableSaleMan);
        });
    } else {
        call_ajax("GET", "SaleMan/GetAll", obj, filltableSaleMan); 
    }
}

function openAddSaleMan() {
    _SaleManId = 0;
    $("#SaleManModalLabel").text("اضافة جديد");
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
    }
}

function updateSaleMan(id) {
    var object1 = { Id: id };
    call_ajax("GET", "SaleMan/GetById", object1, setdataSaleMan);
    _SaleManId = id;
    $("#SaleManModalLabel").text("تعديل المندوب");
    if (typeof ZonePicker !== 'undefined') {
        ZonePicker.loadSaleMan(id, 'saleManZonePicker');
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
    }
    _SaleManId = 0;
    $("#SaleManModalLabel").text("اضافة جديد");
    $('#SaleManModal').modal('hide');
    RefreshSaleMan();
}
