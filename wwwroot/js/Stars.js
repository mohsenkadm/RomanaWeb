 
function filltableStars(data) {
    $('#tableStars').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +
            "<td>" + item.starsCount + "</td>" +
            "<td>" + item.comments + "</td>" +
            "<td>" + item.userName + "</td>" +
            "<td>" + item.restaurantName + "</td>" 
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteStars(" + item.starsId + ")'  >حذف</button></td></tr>";
        $('#tableStars').append(rows); 
    });
}

function deleteStars(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "Stars/Delete", object1, RefreshStars);
    }
}

function RefreshStars() {
    var count = $("#indexid").text(); 
    var obj = {
        RestaurantName: $("#RestaurantName").val(),
        index: count
    }
    call_ajax("GET", "Stars/GetAll", obj, filltableStars);
}
 