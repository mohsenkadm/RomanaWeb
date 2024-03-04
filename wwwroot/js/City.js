
var _CityId = 0;
function filltableCity(data) {
    $('#tableCity').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" + 
            "<td>" + item.countriesName + "</td>" +
            "<td>" + item.cityName + "</td>" 
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteCity(" + item.cityId + ")'  >حذف</button>" +
            "  |  <button type='button' class='btn btn-primary' onclick='updateCity(" + item.cityId + ")'  data-toggle='modal' data-target='#CityModal'>تعديل</button></td></tr>";
        $('#tableCity').append(rows); 
    });
}

function deleteCity(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "City/Delete", object1, RefreshCity);
    }
}
function RefreshCity() {

    var obj = { Name: $("#Name").val()}
    call_ajax("GET", "City/GetAll", obj, filltableCity);
}

function updateCity(id) {
    var object1 = {
        Id: id
    };
    call_ajax("GET", "City/GetById", object1, setdataCity);
    _CityId = id;
}

function setdataCity(data) {
    $("#CityName").val(data.cityName);
    $("#CountriesId").val(data.countriesId).change(); 
}

function aftersaveCity() {
     
    $("#CityName").val('');
    $("#CountriesId").val(0).change();
    _CityId = 0;
    call_ajax("GET", "City/GetAll", null, filltableCity);
}