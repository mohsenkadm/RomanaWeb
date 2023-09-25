
var _CodeResId = 0;
function filltableCodeRes(data) {
    $('#tableCodeRes').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsFree" + item.codeResId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" + 
            "<td>" + item.code + "</td>" 
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteCodeRes(" + item.codeResId + ")'  >حذف</button>" +
            "  |  <button type='button' class='btn btn-primary' onclick='updateCodeRes(" + item.codeResId + ")'  data-toggle='modal' data-target='#CodeResModal'>تعديل</button></td></tr>";
        $('#tableCodeRes').append(rows); 
    });
}

function deleteCodeRes(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "CodeRes/Delete", object1, RefreshCodeRes);
    }
}
function RefreshCodeRes() {
    call_ajax("GET", "CodeRes/GetAll", null, filltableCodeRes);
}

function updateCodeRes(id) {
    var object1 = {
        Id: id
    };
    call_ajax("GET", "CodeRes/GetById", object1, setdataCodeRes);
    _CodeResId = id;
}

function setdataCodeRes(data) {
    if (data.isFree === true) {
        $("#IsFree").prop("checked", true);
    }
    else {
        $("#IsFree").prop("checked", false);
    }
    $("#Code").val(data.code); 
}

function aftersaveCodeRes() {
    $("#IsFree").prop("checked", false);
    $("#Code").val('');
    _CodeResId = 0;
    call_ajax("GET", "CodeRes/GetAll", null, filltableCodeRes);
}