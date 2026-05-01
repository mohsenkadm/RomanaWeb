
function filltableSaleMan(data) {
    $('#tableSaleMan').empty();
    if (data.length === 0) {  
        md.showNotification('لا توجد معلومات');
        return;
    } 
    $.each(data, function (i, item) {
        // Section 6: working/stopped badge + toggle button.
        var working = (item.isAvailable === undefined) ? true : !!item.isAvailable;
        var badge = working
            ? '<span class="badge" style="background:#4CAF50;color:#fff;padding:4px 10px;border-radius:12px;">يعمل</span>'
            : '<span class="badge" style="background:#9E9E9E;color:#fff;padding:4px 10px;border-radius:12px;">متوقف</span>';
        var btnLabel = working ? 'ايقاف' : 'تفعيل';
        var btnClass = working ? 'btn-warning' : 'btn-success';
        var availabilityCell =
            '<td>' + badge +
            ' <button type="button" class="btn btn-sm ' + btnClass + '" ' +
                'style="margin-right:6px" ' +
                'onclick="toggleSaleManAvailability(' + item.saleManId + ',' + (!working) + ')">' +
                btnLabel + '</button></td>';

        var rows = "<tr>" + 
            "<td>" + item.restaurantName + "</td>" +  
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsActive" + item.saleManId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" +
            availabilityCell +
            "<td>" + item.password + "</td>" +  
            "<td>" + item.address + "</td>" +   
            "<td>" + item.phone + "</td>" +
            "<td>" + item.name + "</td>"
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteSaleMan(" + item.saleManId + ")'  >حذف</button></td></tr>";
        $('#tableSaleMan').append(rows);  
        $('#IsActive' + item.saleManId).attr('checked', item.isActive); 
    });
}

// Section 6: flip working/stopped state of a single driver from the dashboard.
function toggleSaleManAvailability(id, makeAvailable) {
    var verb = makeAvailable ? 'تفعيل' : 'ايقاف';
    if (!confirm('هل تريد ' + verb + ' حالة العمل لهذا المندوب؟')) return;
    call_ajax('POST', 'SaleMan/SetAvailability?Id=' + id + '&isAvailable=' + (!!makeAvailable),
        null, RefreshSaleMan);
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
