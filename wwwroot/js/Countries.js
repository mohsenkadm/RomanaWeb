
var _CountriesId = 0;
function filltableCountries(data) {
    $('#tableCountries').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" + 
            "<td>" + item.countriesName + "</td>" 
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteCountries(" + item.countriesId + ")'  >حذف</button>" +
            "  |  <button type='button' class='btn btn-primary' onclick='updateCountries(" + item.countriesId + ")'  data-toggle='modal' data-target='#CountriesModal'>تعديل</button></td></tr>";
        $('#tableCountries').append(rows); 
    });
}

function deleteCountries(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "Countries/Delete", object1, RefreshCountries);
    }
}
function RefreshCountries() {
     
    call_ajax("GET", "Countries/GetAll", null, filltableCountries);
}

function updateCountries(id) {
    var object1 = {
        Id: id
    };
    call_ajax("GET", "Countries/GetById", object1, setdataCountries);
    _CountriesId = id;
}

function setdataCountries(data) {
    $("#CountriesName").val(data.countriesName); 
}

function aftersaveCountries() {
     
    $("#CountriesName").val('');
    _CountriesId = 0;
    call_ajax("GET", "Countries/GetAll", null, filltableCountries);
}