
function filltableReportRes(data) {
    $('#tableReportRes').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +   
            "<td>" + item.countProducts + "</td>" + 
            "<td>" + item.countDelivery + "</td>" + 
            "<td>" + item.totalNetDelivery + "</td>" + 
            "<td>" + item.countOrder + "</td>" + 
            "<td>" + item.netAmount + "</td>" + 
            "<td>" + item.name + "</td></tr>";

        $('#tableReportRes').append(rows);  
    });
}  
function RefreshReportRes() {
    var obj = { RestaurantName: $("#RestaurantName").val(),
        datefrom: $("#datefrom").val(),
        dateto: $("#dateto").val()
    }
    call_ajax("GET", "Restaurant/GetReportRes", obj, filltableReportRes);
} 
