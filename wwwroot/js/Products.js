
var _ProductsId = 0;  
function filltableProducts(data) {
    $('#tableProducts').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +   
            "<td>" + item.subCategoriesName + "</td>" +
            "<td><img src='" + item.productsImageFirst + "' alt='' border=3 height=50 width=50></img></td>" + 
            "<td>" + item.productsPrice + "</td>" +
            "<td>" + item.productsDetails + "</td>" +
            "<td>" + item.productsName + "</td>" +
            "<td>" + item.restaurantName + "</td>" +
            "<td> <button type='button' class='btn btn-info' onclick='ShowImage(" + item.productsId + ")'  data-toggle='modal' data-target='#ImageModal' >عرض الصور</button> |" +
            " <button type = 'button' class='btn btn-danger' onclick = 'deleteProducts(" + item.productsId + ")' > حذف</button > " +
            "  |  <button type='button' class='btn btn-primary' onclick='updateProducts(" + item.productsId + ")'  data-toggle='modal' data-target='#ProductsModal'>تعديل</button></td></tr>";
        $('#tableProducts').append(rows); 
    });
}

 

  
function deleteProducts(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "Products/Delete", object1, RefreshProducts);
    }
}

function RefreshProducts() {
    var count = $("#indexid").text(); 
    var obj = {
        Name: $("#Name").val(),
        RestaurantName: $("#RestaurantName").val(),
        SubCategoriesName: $("#SubCategoriesName").val(),
        index: count
    }
        call_ajax("GET", "Products/GetAll", obj, filltableProducts); 
} 

function updateProducts(id) { 
        call_ajax("GET", "Products/GetById/"+id, null, setdataProducts);
        _ProductsId = id; 
}

function setdataProducts(data) {
     
    $("#ProductsName").val(data.productsName);   
    $("#ProductsPrice").val(data.productsPrice);  
    $("#ProductsDetails").val(data.productsDetails);  
    $("#RestaurantId").val(data.restaurantId).change();
    $("#SubCategoriesId").val(data.subCategoriesId).change();
}
function aftersaveProducts(data) { 
    
    $("#ProductsName").val('');  
    $("#Detail").val('');  
    $("#ProductsPrice").val('');  
    $("#ProductsDetails").val('');
    $("#SubCategoriesId").val(0).change();  
    $("#RestaurantId").val(0).change();  
    _ProductsId = 0;
    RefreshProducts();
}
var ProductsId2 = 0;


function filltableImage(data) {
    $('#tableImage').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +
            "<td><img src='" + item.imagePath + "' alt='' border=3 height=50 width=50></img></td>"
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteImage(" + item.imageId + ")'  >حذف</button></tr>";
        $('#tableImage').append(rows);
    });
}

function RefreshImage() { 
    call_ajax("GET", "GetImagesByProductsId/" + ProductsId2, null, filltableImage);
}

function ShowImage(id) {
    ProductsId2 = id;
    RefreshImage();
}

function deleteImage(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "Products/DeleteImage", object1, RefreshImage);
    }
} 