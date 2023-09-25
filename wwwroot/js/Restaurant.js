
var _RestaurantId = 0;
function filltableRestaurant(data) {
    $('#tableRestaurant').empty();
    if (data.length === 0) {  
        md.showNotification('لا توجد معلومات');
        return;
    } 
    $.each(data, function (i, item) {
        var rows = "<tr>" +
            "<td>" + item.code + "</td>" +
            "<td>" + item.areaname + "</td>" +
            "<td>" + item.minimumPrice + "</td>" +
            "<td>" + item.lat + "</td>" +
            "<td>" + item.long + "</td>" +  
            "<td>" + item.password + "</td>" +
            "<td>" + item.userName + "</td>" +  
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsClosed" + item.restaurantId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" + "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsStars" + item.restaurantId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" + 
            "<td>" + item.whatsapp + "</td>" +   
            "<td>" + item.address + "</td>" +   
            "<td>" + item.phone + "</td>" +
            "<td><img src='" + item.logo + "' alt='' border=3 height=50 width=50></img></td>" +
            "<td><img src='" + item.background + "' alt='' border=3 height=50 width=50></img></td>" +
            "<td>" + item.categoriesName + "</td>" +   
            "<td>" + item.details + "</td>" +   
            "<td>" + item.name + "</td>"
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteCategories(" + item.restaurantId + ")'  >حذف</button>" +
            "  |  <button type='button' class='btn btn-primary' onclick='updateCategories(" + item.restaurantId + ")'  data-toggle='modal' data-target='#RestaurantModal'>تعديل</button> </button></td></tr>";
        $('#tableRestaurant').append(rows);
        $('#IsClosed' + item.restaurantId).attr('checked', item.isClosed); 
        $('#IsStars' + item.restaurantId).attr('checked', item.isStars); 
    });
}

function deleteRestaurant(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "Restaurant/Delete", object1, RefreshRestaurant);
    }
}
function RefreshRestaurant() {
    var count = $("#indexid").text();
    var obj = { Name: $("#Namese").val(), index: count }
    call_ajax("GET", "Restaurant/GetAll", obj, filltableRestaurant);
}

function updateRestaurant(id) {
    var object1 = {
        Id: id
    };
    call_ajax("GET", "Restaurant/GetById", object1, setdataRestaurant);
    _RestaurantId = id;
}

function setdataRestaurant(data) {  
    
    $("#Name").val(data.name);   
    $("#Details").val(data.details); 
    $("#Address").val(data.address); 
    $("#Phone").val(data.phone);
    $("#MinimumPrice").val(data.minimumPrice);
    $("#Areaname").val(data.areaname);
    $("#Password").val(data.password);
    $("#Lat").val(data.lat);
    $("#Long").val(data.long); 
    $("#Whatsapp").val(data.whatsapp); 
    $("#Password").val(data.password);
    $("#UserName").val(data.userName);
    $("#CategoriesId").val(data.categoriesId).change();
    
    if (data.isStars === true) {
        $("#IsStars").prop("checked", true);
    }
    else {
        $("#IsStars").prop("checked", false);
    }  
    if (data.isClosed === true) {
        $("#IsClosed").prop("checked", true);
    }
    else {
        $("#IsClosed").prop("checked", false);
    }  
}
function aftersaveRestaurant(Restaurant) {
     
    $("#Name").val('');
    $("#Details").val('');
    $("#Address").val('');  
    $("#Logo").val('');  
    $("#Background").val('');  
    $("#Phone").val('');  
    $("#MinimumPrice").val('');
    $("#Areaname").val('');
    $("#Lat").val('');
    $("#Long").val(''); 
    $("#Whatsapp").val('');
    $("#Password").val('');
    $("#UserName").val('');
    $("#CategoriesId").val(0).change();
    $("#IsStars").prop("checked", true); 
    $("#IsClosed").prop("checked", false); 
    _RestaurantId = 0;
    RefreshRestaurant();
} 