 
function filltableSaleMan(data) {
    $('#tableSaleMan').empty();
    if (data.length === 0) {  
        md.showNotification('لا توجد معلومات');
        return;
    } 
    $.each(data, function (i, item) {
        var rows = "<tr>" + 
            "<td>" + item.restaurantName + "</td>" +  
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsActive" + item.userId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" +  
            "<td>" + item.password + "</td>" +  
            "<td>" + item.address + "</td>" +   
            "<td>" + item.phone + "</td>" +
            "<td>" + item.name + "</td>"
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteSaleMan(" + item.userId + ")'  >حذف</button></td></tr>";
        $('#tableSaleMan').append(rows);  
        $('#IsActive' + item.userId).attr('checked', item.isActive); 
    });
}

function deleteSaleMan(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "SaleMan/Delete", object1, RefreshSaleMan);
    }
}
function RefreshSaleMan() { 
    var obj = { Name: $("#Namese").val() }
    call_ajax("GET", "SaleMan/GetAll", obj, filltableSaleMan);
}
 