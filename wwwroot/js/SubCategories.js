
var _SubCategoriesId = 0;
function filltableSubCategories(data) {
    $('#tableSubCategories').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" + 
            "<td>" + item.subCategoriesName + "</td>" 
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteSubCategories(" + item.subCategoriesId + ")'  >حذف</button>" +
            "  |  <button type='button' class='btn btn-primary' onclick='updateSubCategories(" + item.subCategoriesId + ")'  data-toggle='modal' data-target='#SubCategoriesModal'>تعديل</button></td></tr>";
        $('#tableSubCategories').append(rows); 
    });
}

function deleteSubCategories(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "SubCategories/Delete", object1, RefreshSubCategories);
    }
}
function RefreshSubCategories() {

    var obj = { Name: $("#Name").val()}
    call_ajax("GET", "SubCategories/GetAll", obj, filltableSubCategories);
}

function updateSubCategories(id) {
    var object1 = {
        Id: id
    };
    call_ajax("GET", "SubCategories/GetById", object1, setdataSubCategories);
    _SubCategoriesId = id;
}

function setdataSubCategories(data) {
    $("#SubCategoriesName").val(data.subCategoriesName); 
}

function aftersaveSubCategories() {
     
    $("#SubCategoriesName").val('');
    _SubCategoriesId = 0;
    call_ajax("GET", "SubCategories/GetAll", null, filltableSubCategories);
}