
var _RestaurantSubCategoriesId = 0;
function filltableRestaurantSubCategories(data) {
    $('#tableRestaurantSubCategories').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" + 
            "<td>" + item.subCategoriesName + "</td>" +
        "<td>" + item.restaurantName + "</td>" 
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteRestaurantSubCategories(" + item.restaurantSubCategoriesId + ")'  >حذف</button>" +
            "  |  <button type='button' class='btn btn-primary' onclick='updateRestaurantSubCategories(" + item.restaurantSubCategoriesId + ")'  data-toggle='modal' data-target='#RestaurantSubCategoriesModal'>تعديل</button></td></tr>";
        $('#tableRestaurantSubCategories').append(rows); 
    });
}

function deleteRestaurantSubCategories(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "RestaurantSubCategories/Delete", object1, RefreshRestaurantSubCategories);
    }
}
function RefreshRestaurantSubCategories() {
    var obj = { Name: $("#Name").val() }
    call_ajax("GET", "RestaurantSubCategories/GetAll", null, filltableRestaurantSubCategories);
}

function updateRestaurantSubCategories(id) {
    var object1 = {
        Id: id
    };
    call_ajax("GET", "RestaurantSubCategories/GetById", object1, setdataRestaurantSubCategories);
    _RestaurantSubCategoriesId = id;
}

function setdataRestaurantSubCategories(data) {
    $("#SubCategoriesId").val(data.subCategoriesId).change;
    $("#RestaurantId").val(data.restaurantId).change; 
}

function aftersaveRestaurantSubCategories(RestaurantSubCategories) { 
    $("#SubCategoriesId").val(0).change;
    $("#RestaurantId").val(0).change;
    _RestaurantSubCategoriesId = 0;
    call_ajax("GET", "RestaurantSubCategories/GetAll", null, filltableRestaurantSubCategories);
}