var _SupportPhoneId = 0;

function getSupportAppTypeLabel(appType) {
    switch (parseInt(appType, 10)) {
        case 1: return 'تطبيق الزبائن';
        case 2: return 'تطبيق المتاجر';
        case 3: return 'تطبيق السائقين';
        default: return '-';
    }
}

function filltableSupportPhone(data) {
    $('#tableSupportPhone').empty();
    if (!data || data.length === 0) {
        md.showNotification('لا توجد معلومات');
        return;
    }
    $.each(data, function (i, item) {
        var activeBadge = item.isActive
            ? '<span class="badge" style="background:#4CAF50;color:#fff;">نشط</span>'
            : '<span class="badge" style="background:#9E9E9E;color:#fff;">غير نشط</span>';
        var rows = "<tr>" +
            "<td>" + activeBadge + "</td>" +
            "<td>" + (item.label || '-') + "</td>" +
            "<td>" + item.phone + "</td>" +
            "<td>" + getSupportAppTypeLabel(item.appType) + "</td>" +
            "<td><button type='button' class='btn btn-danger' onclick='deleteSupportPhone(" + item.supportPhoneId + ")'>حذف</button>" +
            " | <button type='button' class='btn btn-primary' onclick='updateSupportPhone(" + item.supportPhoneId + ")' data-toggle='modal' data-target='#SupportPhoneModal'>تعديل</button></td>" +
            "</tr>";
        $('#tableSupportPhone').append(rows);
    });
}

function deleteSupportPhone(id) {
    if (!confirm("هل تريد الحذف؟!")) return;
    call_ajax("DELETE", "SupportPhone/Delete", { Id: id }, RefreshSupportPhone);
}

function RefreshSupportPhone() {
    call_ajax("GET", "SupportPhone/GetAll", null, filltableSupportPhone);
}

function openAddSupportPhone() {
    _SupportPhoneId = 0;
    $("#SupportPhoneModalLabel").text("اضافة رقم دعم");
    $("#AppType").val('1');
    $("#Phone").val('');
    $("#Label").val('');
    $("#IsActive").prop("checked", true);
}

function updateSupportPhone(id) {
    call_ajax("GET", "SupportPhone/GetById", { Id: id }, setdataSupportPhone);
    _SupportPhoneId = id;
    $("#SupportPhoneModalLabel").text("تعديل رقم الدعم");
}

function setdataSupportPhone(data) {
    $("#AppType").val(data.appType);
    $("#Phone").val(data.phone);
    $("#Label").val(data.label || '');
    $("#IsActive").prop("checked", !!data.isActive);
}

function aftersaveSupportPhone() {
    $("#Phone").val('');
    $("#Label").val('');
    $("#AppType").val('1');
    $("#IsActive").prop("checked", true);
    _SupportPhoneId = 0;
    $("#SupportPhoneModalLabel").text("اضافة رقم دعم");
    $('#SupportPhoneModal').modal('hide');
    RefreshSupportPhone();
}
