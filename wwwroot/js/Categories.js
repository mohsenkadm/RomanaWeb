
var _categoriesId = 0;
function filltableCategories(data) {
    $('#tableCategories').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +
            "<td><img src='" + item.categoriesImages + "' alt='' border=3 height=50 width=50></img></td>" +
            "<td>" + item.categoriesName + "</td>" 
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteCategories(" + item.categoriesId + ")'  >حذف</button>" +
            "  |  <button type='button' class='btn btn-primary' onclick='updateCategories(" + item.categoriesId + ")'  data-toggle='modal' data-target='#CategoriesModal'>تعديل</button></td></tr>";
        $('#tableCategories').append(rows); 
    });
}

function deleteCategories(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "Categories/Delete", object1, RefreshCategories);
    }
}
function RefreshCategories() {
    call_ajax("GET", "Categories/GetAll", null, filltableCategories);
}

function updateCategories(id) {
    var object1 = {
        Id: id
    };
    call_ajax("GET", "Categories/GetById", object1, setdataCategories);
    _categoriesId = id;
}

function setdataCategories(data) {
    $("#CategoriesName").val(data.categoriesName); 
}

function aftersaveCategories(Categories) {
     
    $("#CategoriesName").val('');
    $("#imageca").val('');
    _categoriesId = 0;
    call_ajax("GET", "Categories/GetAll", null, filltableCategories);
}