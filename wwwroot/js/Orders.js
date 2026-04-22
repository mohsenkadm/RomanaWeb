
function filltableOrders(data) {
    $('#tableOrders').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +  
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsDone" + item.orderId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" + 
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsApporve" + item.orderId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" + 
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsCancel" + item.orderId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" +
            "<td>" + item.netAmount + "</td>" +
            "<td>" + item.totalDiscount + "</td>" +
            "<td>" + item.total + "</td>" + 
            "<td>" + item.cityName + "</td>" +
            "<td>" + item.countriesName + "</td>" +
            "<td>" + item.phone + "</td>" +
            "<td>" + item.functionPoint + "</td>" +
            "<td>" + item.address + "</td>" +
            "<td>" + item.userName + "</td>" +
            "<td>" + item.saleManName + "</td>" +
            "<td>" + item.restaurantName + "</td>" +
            "<td>" + item.categoriesName + "</td>" +
            "<td>" + item.orderDate + "</td>" +
            "<td>" + item.orderNo + "</td>"+
            "  <td>  <button type='button' class='btn btn-info' onclick='OrderDetail(" + item.orderId + ")'  data-toggle='modal' data-target='#OrdersDetailsModal'>تفاصيل الطلب</button></td></tr>";

              $('#tableOrders').append(rows); 
        $('#IsDone' + item.orderId).attr('checked', item.isDone);
        $('#IsApporve' + item.orderId).attr('checked', item.isApporve);
        $('#IsCancel' + item.orderId).attr('checked', item.isCancel);
    });
} 
function filltableOrdersForRes(data) {
    $('#tableOrdersForRes').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +  
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsDone" + item.orderId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" + 
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsApporve" + item.orderId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" + 
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsCancel" + item.orderId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" +
            "<td>" + item.netAmount + "</td>" +
            "<td>" + item.totalDiscount + "</td>" +
            "<td>" + item.total + "</td>" +
            "<td>" + item.cityName + "</td>" +
            "<td>" + item.countriesName + "</td>" +
            "<td>" + item.phone + "</td>" +
            "<td>" + item.notes + "</td>" +
            "<td>" + item.functionPoint + "</td>" +
            "<td>" + item.address + "</td>" +
            "<td>" + item.userName + "</td>" +
            "<td>" + item.saleManName + "</td>" + 
            "<td>" + item.orderDate + "</td>" +
            "<td>" + item.orderNo + "</td>" 
            + "<td> <button type='button' class='btn btn-primary' onclick='SetIsDone(" + item.orderId + ")'  >انتهاء الطلب</button></td>"
            + " <td>  <button type='button' class='btn btn-warning' onclick='SetIsCancel(" + item.orderId + ")'  >الغاء</button></td>"
            + " <td>  <button type='button' class='btn btn-success' onclick='SetIsApporve(" + item.orderId + ")'  >موافقة</button></td>"
            + " <td>  <button type='button' class='btn btn-danger' onclick='deleteOrders(" + item.orderId + ")'  >حذف</button></td>" +
            " <td>  <button type='button' class='btn btn-success' onclick='setsaleman(" + item.orderId + ")'  data-toggle='modal' data-target='#setsalemanModal'>اختيار مندوب</button></td>" +
            "  <td>  <button type='button' class='btn btn-info' onclick='OrderDetailForRes(" + item.orderId + ")'  data-toggle='modal' data-target='#OrdersDetailsForResModal'>تفاصيل الطلب</button></td></tr>";

        $('#tableOrdersForRes').append(rows); 
        $('#IsDone' + item.orderId).attr('checked', item.isDone);
        $('#IsApporve' + item.orderId).attr('checked', item.isApporve);
        $('#IsCancel' + item.orderId).attr('checked', item.isCancel);
    });
}


function SetIsCancel(id) { 
    call_ajax("DELETE", "Orders/SetIsCancel/" + id, null, RefreshOrdersForRes);
}
function SetIsApporve(id) {

    call_ajax("Post", "Orders/SetIsApporve/"+id, null, RefreshOrdersForRes);
}
function SetIsDone(id) { 

    call_ajax("Post", "Orders/SetIsDone/" + id, null, RefreshOrdersForRes);
}

var _orderid = 0;
function setsaleman(id) { 
    _orderid = id; 
}
function deleteOrders(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "Orders/Delete", object1, RefreshOrdersForRes);
    }
} 
var _id = 0;
function OrderDetail(id) {
    var object1 = {
        Id: id,
    }
    _id = id;
    call_ajax("Get", "Orders/GetOrdersWithDetailAll", object1, SetDataOrderDetail);
}
function OrderDetailForRes(id) {
    var object1 = {
        Id: id,
    }
    _id = id;
    call_ajax("Get", "Orders/GetOrdersWithDetailAll", object1, SetDataOrderDetailForRes);
}

function SetDataOrderDetail(data) {
    $('#tableOrdersDetail').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +
            "<td> " + item.count + "</td> " +  
            "<td> " + item.price + "</td> " +  
            "<td>" + item.productsDetails + "</td>" + 
            "<td>" + item.productsName + "</td>" +
            "<td>" + item.subCategoriesName + "</td>" +
            "<td><img src='" + item.productsImage + "' alt='' border=3 height=50 width=50></img></td>" 
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteOrderDetail(" + item.orderDetailId + ")'  >حذف</button></td></tr>";
        $('#tableOrdersDetail').append(rows);
    });
}
function SetDataOrderDetailForRes(data) {
    $('#tableOrdersDetailForRes').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +
            "<td> " + item.notes2 + "</td> " +  
            "<td> " + item.count + "</td> " +  
            "<td> " + item.price + "</td> " +  
            "<td>" + item.productsDetails + "</td>" + 
            "<td>" + item.productsName + "</td>" +
            "<td>" + item.subCategoriesName + "</td>" +
            "<td><img src='" + item.productsImage + "' alt='' border=3 height=50 width=50></img></td></tr>";
        $('#tableOrdersDetailForRes').append(rows);
    });
}


function deleteOrderDetail(id) {
    var object1 = {
        Id: id,
    }
    call_ajax("DELETE", "Orders/DeleteDetails", object1, RefreshOrderDetail);
}
function RefreshOrderDetail() {
    OrderDetail(_id);
}
function RefreshOrders() {
    var obj = {
        OrderNo: $("#OrderNo").val(), RestaurantName: $("#RestaurantName").val(),
        datefrom: $("#datefrom").val(),
        dateto: $("#dateto").val(), CountriesId:$("#CountriesId").val()
    }
    call_ajax("GET", "Orders/GetAll", obj, filltableOrders);
}
function RefreshOrdersForRes() {
    _orderid = 0; $("#SaleManId").val(0).change();
    var obj = {
        OrderNo: $("#OrderNo").val(), RestaurantName: '',
        datefrom: $("#datefrom").val(),
        dateto: $("#dateto").val(),
    }
    call_ajax("GET", "Orders/GetAll", obj, filltableOrdersForRes);
}
