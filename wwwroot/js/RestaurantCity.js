
var _RestaurantCityId = 0;
function filltableRestaurantCity(data) {
    $('#tableRestaurantCity').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" + 
            "<td>" + item.costDelivery + "</td>" +
            "<td>" + item.cityName + "</td>" +
        "<td>" + item.restaurantName + "</td>" 
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteRestaurantCity(" + item.restaurantCityId + ")'  >حذف</button>" +
            "  |  <button type='button' class='btn btn-primary' onclick='updateRestaurantCity(" + item.restaurantCityId + ")'  data-toggle='modal' data-target='#RestaurantCityModal'>تعديل</button></td></tr>";
        $('#tableRestaurantCity').append(rows); 
    });
}

function deleteRestaurantCity(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "RestaurantCity/Delete", object1, RefreshRestaurantCity);
    }
}
function RefreshRestaurantCity() {
    var obj = { Name: $("#Name").val() }
    call_ajax("GET", "RestaurantCity/GetAll", null, filltableRestaurantCity);
}

function updateRestaurantCity(id) {
    var object1 = {
        Id: id
    };
    call_ajax("GET", "RestaurantCity/GetById", object1, setdataRestaurantCity);
    _RestaurantCityId = id;
}

function setdataRestaurantCity(data) {
    $("#CityId").val(data.CityId).change;
    $("#RestaurantId").val(data.restaurantId).change; 
    $("#CostDelivery").val(data.costDelivery); 
}

function aftersaveRestaurantCity(RestaurantCity) { 
    $("#CostDelivery").val('');
    $("#CityId").val(0).change;
    $("#RestaurantId").val(0).change;
    _RestaurantCityId = 0;
    call_ajax("GET", "RestaurantCity/GetAll", null, filltableRestaurantCity);
}