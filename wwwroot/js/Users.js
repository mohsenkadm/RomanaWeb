
var _UserId = 0;
function filltableUsers(data) {
    $('#tableUsers').empty();
    if (data.length === 0) {  
        md.showNotification('لا توجد معلومات');
        return;
    } 
    $.each(data, function (i, item) {
        var rows = "<tr>" +
            "<td>" + item.code + "</td>" + 
            "<td>" + item.functionPoint + "</td>" +
            "<td>" + item.lat + "</td>" +
            "<td>" + item.long + "</td>" +  
            "<td>" + item.password + "</td>" + 
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
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsConfirm" + item.userId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" +    
            "<td>" + item.address + "</td>" +   
            "<td>" + item.phone + "</td>" +
            "<td>" + item.name + "</td>"
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteUsers(" + item.userId + ")'  >حذف</button>" +
            "  |  <button type='button' class='btn btn-primary' onclick='updateUsers(" + item.userId + ")'  data-toggle='modal' data-target='#UsersModal'>تعديل</button> </button></td></tr>";
        $('#tableUsers').append(rows); 
        $('#IsConfirm' + item.userId).attr('checked', item.isConfirm); 
        $('#IsActive' + item.userId).attr('checked', item.isActive); 
    });
}

function deleteUsers(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "Users/Delete", object1, RefreshUsers);
    }
}
function RefreshUsers() { 
    var obj = { Name: $("#Namese").val() }
    call_ajax("GET", "Users/GetAll", obj, filltableUsers);
}

function updateUsers(id) {
    var object1 = {
        Id: id
    };
    call_ajax("GET", "Users/GetById", object1, setdataUsers);
    _UserId = id;
}

function setdataUsers(data) {  
    
    $("#Name").val(data.name);    
    $("#Address").val(data.address); 
    $("#Phone").val(data.phone);
    $("#FunctionPoint").val(data.functionPoint); 
    $("#Password").val(data.password);
    $("#Lat").val(data.lat);
    $("#Long").val(data.long);  
    $("#Password").val(data.password);  
    $("#Code").val(data.code);
    
    if (data.isConfirm === true) {
        $("#IsConfirm").prop("checked", true);
    }
    else {
        $("#IsConfirm").prop("checked", false);
    }
    if (data.isActive === true) {
        $("#IsActive").prop("checked", true);
    }
    else {
        $("#IsActive").prop("checked", false);
    }   
}
function aftersaveUsers() {
     
    $("#Name").val('');
    $("#Phone").val('');
    $("#Address").val('');  
    $("#FunctionPoint").val('');  
    $("#Password").val('');   
    $("#Lat").val('');
    $("#Long").val(''); 
    $("#Code").val('');  
    $("#IsActive").prop("checked", true); 
    $("#IsConfirm").prop("checked", false); 
    _UserId = 0;
    RefreshUsers();
} 