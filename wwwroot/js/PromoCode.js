
var _PromoCodeId = 0;
function filltablePromoCode(data) {
    $('#tablePromoCode').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" + 
            "<td>" + item.percentage + "</td>" +
        "<td>" + item.promoName + "</td>" 
            + "<td> <button type='button' class='btn btn-danger' onclick='deletePromoCode(" + item.promoCodeId + ")'  >حذف</button>" +
            "  |  <button type='button' class='btn btn-primary' onclick='updatePromoCode(" + item.promoCodeId + ")'  data-toggle='modal' data-target='#PromoCodeModal'>تعديل</button></td></tr>";
        $('#tablePromoCode').append(rows); 
    });
}

function deletePromoCode(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "PromoCode/Delete", object1, RefreshPromoCode);
    }
}
function RefreshPromoCode() {
    call_ajax("GET", "PromoCode/GetAll", null, filltablePromoCode);
}

function updatePromoCode(id) {
    var object1 = {
        Id: id
    };
    call_ajax("GET", "PromoCode/GetById", object1, setdataPromoCode);
    _PromoCodeId = id;
}

function setdataPromoCode(data) {
    $("#PromoName").val(data.promoName); 
    $("#Percentage").val(data.percentage); 
}

function aftersavePromoCode() {
     
    $("#Percentage").val('');
    $("#PromoName").val('');
    _PromoCodeId = 0;
    call_ajax("GET", "PromoCode/GetAll", null, filltablePromoCode);
}