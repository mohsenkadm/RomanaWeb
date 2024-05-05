
var _DeliveryId = 0;
function filltableDelivery(data) {
    $('#tableDelivery').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" + 
            "<td>" + item.countriesName + "</td>" +
            "<td>" + item.cityName + "</td>" +
            "<td>" + item.functionPoint + "</td>" +
            "<td>" + item.netAmount + "</td>" +
            "<td>" + item.costDelivery + "</td>" +
            "<td>" + item.dateInsert + "</td>" +
            "<td>" + item.notes + "</td>" +
            "<td>" + item.phone + "</td>" +
            "<td>" + item.address + "</td>" +
            "<td>" + item.name + "</td>" +
            "<td>" + item.restaurantName + "</td>" +
            "<td>" + item.no + "</td>" 
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteDelivery(" + item.deliveryId + ")'  >حذف</button></td></tr>";
        $('#tableDelivery').append(rows); 
    });
}

function deleteDelivery(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "Delivery/Delete", object1, RefreshDelivery);
    }
} 
function RefreshDelivery() {
    var obj = {
        Name: $("#Name").val(), CountriesId: $("#CountriesId").val()
        , RestaurantName: $("#RestaurantName").val(),
        datefrom: $("#datefrom").val(),
        dateto: $("#dateto").val(),No: $("#No").val(),
    } 
    call_ajax("GET", "Delivery/GetAll", obj, filltableDelivery);
}
 
  