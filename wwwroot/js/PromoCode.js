
var _PromoCodeId = 0;
function filltablePromoCode(data) {
    $('#tablePromoCode').empty();
    $.each(data, function (i, item) {
        var statusBadge = item.isActive
            ? "<span class='badge-active'>فعال</span>"
            : "<span class='badge-inactive'>غير فعال</span>";
        var scopeBadge = item.isForAllStores
            ? "<span class='badge-all'>جميع المتاجر</span>"
            : item.restaurantName;
        var rows = "<tr style='animation: promoFadeIn " + (0.05 * i) + "s ease-out both'>" +
            "<td>" + (item.restaurantName || '-') + "</td>" +
            "<td>" + item.percentage + "%</td>" +
            "<td>" + (item.discountAmount || 0) + " د.ع</td>" +
            "<td>" + item.promoName + "</td>" +
            "<td>" + (item.maxOrders > 0 ? item.maxOrders : 'غير محدود') + "</td>" +
            "<td>" + (item.maxUsagePerUser > 0 ? item.maxUsagePerUser : 'غير محدود') + "</td>" +
            "<td>" + (item.usedOrders || 0) + "</td>" +
            "<td>" + scopeBadge + "</td>" +
            "<td>" + statusBadge + "</td>" +
            "<td><button type='button' class='btn btn-danger btn-sm' onclick='deletePromoCode(" + item.promoCodeId + ")'>حذف</button>" +
            " <button type='button' class='btn btn-primary btn-sm' onclick='updatePromoCode(" + item.promoCodeId + ")' data-toggle='modal' data-target='#PromoCodeModal'>تعديل</button></td></tr>";
        $('#tablePromoCode').append(rows);
    });
}

function deletePromoCode(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = { Id: id };
        call_ajax("DELETE", "PromoCode/Delete", object1, RefreshPromoCode);
    }
}
function RefreshPromoCode() {
    call_ajax("GET", "PromoCode/GetAll", null, filltablePromoCode);
}

function updatePromoCode(id) {
    var object1 = { Id: id };
    call_ajax("GET", "PromoCode/GetById", object1, setdataPromoCode);
    _PromoCodeId = id;
}

function setdataPromoCode(data) {
    $("#PromoName").val(data.promoName);
    $("#Percentage").val(data.percentage);
    $("#DiscountAmount").val(data.discountAmount || 0);
    $("#MaxOrders").val(data.maxOrders || 0);
    $("#MaxUsagePerUser").val(data.maxUsagePerUser ?? 1);
    $("#RestaurantIdPromo").val(data.restaurantId);
    $("#IsForAllStores").prop('checked', data.isForAllStores);
    $("#IsActivePromo").prop('checked', data.isActive);
}

function aftersavePromoCode() {
    $("#Percentage").val('');
    $("#PromoName").val('');
    $("#DiscountAmount").val('');
    $("#MaxOrders").val('');
    $("#MaxUsagePerUser").val('1');
    $("#RestaurantIdPromo").val(0);
    $("#IsForAllStores").prop('checked', false);
    $("#IsActivePromo").prop('checked', true);
    _PromoCodeId = 0;
    call_ajax("GET", "PromoCode/GetAll", null, filltablePromoCode);
}
