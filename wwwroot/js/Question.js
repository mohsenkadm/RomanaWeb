var _QuestionId = 0;
function deleteQuestion(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "Questions/Delete", object1, RefreshQuestions);
    }
}
function updateQuestion(id) {
    var object1 = {
        Id: id
    };
    call_ajax("GET", "Questions/GetById", object1, setdataQuestion);
    _QuestionId = id;
}
function filltableQuestions(data) {
    $('#tableQuestions').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsShow" + item.questionId + "' disabled>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" +
            "<td>" + item.questionText + "</td>" +
            "<td>" + (item.answerText ? item.answerText.substring(0, 50) + "..." : "") + "</td>" +
            "<td>" + item.displayOrder + "</td>" +
            "<td> <button type='button' class='btn btn-danger' onclick='deleteQuestion(" + item.questionId + ")'>حذف</button>" +
            "  |  <button type='button' class='btn btn-primary' onclick='updateQuestion(" + item.questionId + ")' data-toggle='modal' data-target='#QuestionModal'>تعديل</button></td></tr>";
        $('#tableQuestions').append(rows);
        $('#IsShow' + item.questionId).attr('checked', item.isShow);
    });
}



function RefreshQuestions() {
    call_ajax("GET", "Questions/GetAll", null, filltableQuestions);
}



function setdataQuestion(data) {
    $("#QuestionText").val(data.questionText);
    $("#AnswerText").val(data.answerText);
    $("#DisplayOrder").val(data.displayOrder);

    if (data.isShow === true) {
        $("#IsShowQuestion").prop("checked", true);
    }
    else {
        $("#IsShowQuestion").prop("checked", false);
    }
}

function aftersaveQuestion() {
    $("#QuestionText").val('');
    $("#AnswerText").val('');
    $("#DisplayOrder").val('0');
    $("#IsShowQuestion").prop("checked", false);
    _QuestionId = 0;
    RefreshQuestions();
}
