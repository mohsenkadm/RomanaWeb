
var _ProductsId = 0;  
function filltableProducts(data) {
    $('#tableProducts').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +
            "<td class='rm-actions-cell'><div class='rm-actions'>" +
            "<button type='button' class='btn btn-primary btn-sm' onclick='updateProducts(" + item.productsId + ")' data-toggle='modal' data-target='#ProductsModal'>تعديل</button> " +
            "<button type='button' class='btn btn-danger btn-sm' onclick='deleteProducts(" + item.productsId + ")'>حذف</button> " +
            "<button type='button' class='btn btn-info btn-sm' onclick='ShowImage(" + item.productsId + ")' data-toggle='modal' data-target='#ImageModal'>صور</button>" +
            "</div></td>" +
            "<td class='rm-actions-cell'><button type='button' class='btn btn-info btn-sm' onclick='showIngredients(" + item.productsId + ")' data-toggle='modal' data-target='#IngredientsModal'>المكونات</button></td>" +
            "<td class='rm-actions-cell'><button type='button' class='btn btn-warning btn-sm' onclick='showSizes(" + item.productsId + ")' data-toggle='modal' data-target='#SizesModal'>الأحجام</button></td>" +
            "<td>" + (item.restaurantName || '') + "</td>" +
            "<td><strong>" + (item.productsName || '') + "</strong></td>" +
            "<td class='rm-wrap-cell'>" + (item.productsDetails || '') + "</td>" +
            "<td>" + item.productsPrice + "</td>" +
            "<td class='rm-img-cell'><img src='" + (item.productsImageFirst || '') + "' alt='' class='rm-table-thumb'></td>" +
            "<td>" + (item.subCategoriesName || '') + "</td></tr>";
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
    $("#PreparationTimeMinutes").val(data.preparationTimeMinutes || 15);
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
            "<td class='rm-img-cell'><img src='" + item.imagePath + "' alt='' class='rm-table-thumb'></td>"
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

// ==================== SIZES ====================

var _currentSizeProductId = 0;

function showSizes(productId) {
    _currentSizeProductId = productId;
    refreshSizes();
}

function refreshSizes() {
    call_ajax("GET", "Products/GetSizesByProductId/" + _currentSizeProductId, null, fillSizesTable);
}

function fillSizesTable(data) {
    $('#tableSizes').empty();
    if (!data || data.length === 0) {
        $('#tableSizes').append('<tr><td colspan="3" class="text-center text-muted">لا توجد أحجام مضافة</td></tr>');
        return;
    }
    $.each(data, function (i, item) {
        var row = "<tr>" +
            "<td>" + item.sizeName + "</td>" +
            "<td>" + item.sizePrice + "</td>" +
            "<td><button type='button' class='btn btn-danger btn-sm' onclick='deleteSize(" + item.productSizeId + ")'>حذف</button></td>" +
            "</tr>";
        $('#tableSizes').append(row);
    });
}

function addSize() {
    var name = $('#NewSizeName').val().trim();
    var price = parseFloat($('#NewSizePrice').val()) || 0;
    if (!name) { md.showNotification('رجاءا ادخل اسم الحجم'); return; }
    var obj = { productsId: _currentSizeProductId, sizeName: name, sizePrice: price };
    call_ajax_json("POST", "Products/PostSize", obj, function () {
        $('#NewSizeName').val('');
        $('#NewSizePrice').val('');
        refreshSizes();
    });
}

function deleteSize(sizeId) {
    if (!confirm('هل تريد حذف هذا الحجم؟')) return;
    call_ajax("DELETE", "Products/DeleteSize/" + sizeId, null, refreshSizes);
}

// ==================== INGREDIENTS ====================

var _currentIngredientProductId = 0;

function showIngredients(productId) {
    _currentIngredientProductId = productId;
    refreshIngredients();
}

function refreshIngredients() {
    call_ajax("GET", "Products/GetIngredientsByProductId/" + _currentIngredientProductId, null, fillIngredientsTable);
}

function fillIngredientsTable(data) {
    $('#tableIngredients').empty();
    if (!data || data.length === 0) {
        $('#tableIngredients').append('<tr><td colspan="2" class="text-center text-muted">لا توجد مكونات مضافة</td></tr>');
        return;
    }
    $.each(data, function (i, item) {
        var row = "<tr>" +
            "<td>" + item.ingredientName + "</td>" +
            "<td><button type='button' class='btn btn-danger btn-sm' onclick='deleteIngredient(" + item.productIngredientId + ")'>حذف</button></td>" +
            "</tr>";
        $('#tableIngredients').append(row);
    });
}

function addIngredient() {
    var name = $('#NewIngredientName').val().trim();
    if (!name) { md.showNotification('رجاءا ادخل اسم المكون'); return; }
    var obj = { productsId: _currentIngredientProductId, ingredientName: name };
    call_ajax_json("POST", "Products/PostIngredient", obj, function () {
        $('#NewIngredientName').val('');
        refreshIngredients();
    });
}

function deleteIngredient(ingredientId) {
    if (!confirm('هل تريد حذف هذا المكون؟')) return;
    call_ajax("DELETE", "Products/DeleteIngredient/" + ingredientId, null, refreshIngredients);
}
