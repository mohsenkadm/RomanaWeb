
var _UserId = 0;
var _usersPage = 1;
var _usersPageSize = 25;
var _usersTotalPages = 1;
var _usersLoading = false;

function escHtml(v) {
    if (v === null || v === undefined) return '';
    return String(v)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

function filltableUsers(payload) {
    $('#tableUsers').empty();

    var items = [];
    if (payload && Array.isArray(payload.items)) {
        items = payload.items;
        _usersPage = payload.page || 1;
        _usersPageSize = payload.pageSize || _usersPageSize;
        _usersTotalPages = payload.totalPages || 1;

        $('#userSummaryTotal').text(payload.totalCount != null ? payload.totalCount : '-');
        $('#userSummaryActive').text(payload.activeCount != null ? payload.activeCount : '-');
        $('#userTotalCount').text(payload.totalCount != null ? payload.totalCount : 0);
        renderUsersPagination();
    } else if (Array.isArray(payload)) {
        // fallback for unexpected shape
        items = payload;
    }

    if (!items.length) {
        $('#tableUsers').append(
            "<tr><td colspan='7' class='text-center text-muted'>لا توجد نتائج</td></tr>"
        );
        return;
    }

    $.each(items, function (i, item) {
        var uid = item.userId;
        var rows = "<tr>" +
            "<td>" + escHtml(item.functionPoint) + "</td>" +
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' disabled id='IsActive" + uid + "'>" +
            "<span class='form-check-sign'><span class='check'></span></span>" +
            "</label></div></td>" +
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' disabled id='IsConfirm" + uid + "'>" +
            "<span class='form-check-sign'><span class='check'></span></span>" +
            "</label></div></td>" +
            "<td>" + escHtml(item.address) + "</td>" +
            "<td>" + escHtml(item.phone) + "</td>" +
            "<td>" + escHtml(item.name) + "</td>" +
            "<td class='rm-actions-cell'>" +
            "<button type='button' class='btn btn-danger btn-sm' onclick='deleteUsers(" + uid + ")'>حذف</button> " +
            "<button type='button' class='btn btn-primary btn-sm' onclick='updateUsers(" + uid + ")' data-toggle='modal' data-target='#UsersModal'>تعديل</button>" +
            "</td></tr>";
        $('#tableUsers').append(rows);
        $('#IsConfirm' + uid).prop('checked', item.isConfirm === true);
        $('#IsActive' + uid).prop('checked', item.isActive === true);
    });
}

function renderUsersPagination() {
    var $ul = $('#usersPagination');
    if (!$ul.length) return;
    $ul.empty();

    var page = _usersPage;
    var total = _usersTotalPages;
    $('#usersPageInfo').text(page + ' / ' + total);

    function addItem(label, targetPage, disabled, active) {
        var cls = 'page-item';
        if (disabled) cls += ' disabled';
        if (active) cls += ' active';
        var $li = $('<li>').addClass(cls);
        var $a = $('<a>').addClass('page-link').attr('href', '#').text(label);
        if (!disabled && !active) {
            $a.on('click', function (e) {
                e.preventDefault();
                loadUsersPage(targetPage);
            });
        } else {
            $a.on('click', function (e) { e.preventDefault(); });
        }
        $li.append($a);
        $ul.append($li);
    }

    addItem('«', page - 1, page <= 1, false);

    var windowSize = 5;
    var start = Math.max(1, page - Math.floor(windowSize / 2));
    var end = Math.min(total, start + windowSize - 1);
    start = Math.max(1, end - windowSize + 1);

    if (start > 1) {
        addItem('1', 1, false, false);
        if (start > 2) addItem('…', page, true, false);
    }

    for (var p = start; p <= end; p++) {
        addItem(String(p), p, false, p === page);
    }

    if (end < total) {
        if (end < total - 1) addItem('…', page, true, false);
        addItem(String(total), total, false, false);
    }

    addItem('»', page + 1, page >= total, false);
}

function loadUsersPage(page) {
    if (_usersLoading) return;
    if (page < 1) page = 1;
    _usersPage = page;
    _usersLoading = true;

    var pageSize = parseInt($('#usersPageSize').val(), 10) || _usersPageSize;
    _usersPageSize = pageSize;

    var obj = {
        Name: ($("#Namese").val() || '').trim(),
        page: _usersPage,
        pageSize: _usersPageSize
    };

    call_ajax("GET", "Users/GetAll", obj, function (data) {
        _usersLoading = false;
        filltableUsers(data);
    });

    // if call_ajax fails silently on error path, unlock after short delay
    setTimeout(function () { _usersLoading = false; }, 8000);
}

function deleteUsers(id) {
    if (!confirm("هل تريد الحذف؟!")) return;
    call_ajax("DELETE", "Users/Delete", { Id: id }, RefreshUsers);
}

function RefreshUsers() {
    loadUsersPage(1);
}

function updateUsers(id) {
    call_ajax("GET", "Users/GetById", { Id: id }, setdataUsers);
    _UserId = id;
}

function setdataUsers(data) {
    if (!data) return;
    $("#Name").val(data.name || '');
    $("#Address").val(data.address || '');
    $("#Phone").val(data.phone || '');
    $("#FunctionPoint").val(data.functionPoint || '');
    $("#Password").val('');
    $("#Lat").val(data.lat || '');
    $("#Long").val(data.long || '');
    $("#Code").val('');
    $("#UserName").val(data.name || '');
    $("#IsConfirm").prop("checked", data.isConfirm === true);
    $("#IsActive").prop("checked", data.isActive === true);
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
    $("#UserName").val('');
    $("#IsActive").prop("checked", true);
    $("#IsConfirm").prop("checked", false);
    _UserId = 0;
    $('#UsersModal').modal('hide');
    RefreshUsers();
}

function downloadUsersExcel() {
    var userToken = typeof getCookie === 'function' ? getCookie("token2") : '';
    var name = ($("#Namese").val() || '').trim();
    var url = "/Users/GetExcelAll?Name=" + encodeURIComponent(name);

    mouseevent("progress");
    $('.progress').fadeIn();

    fetch(url, {
        method: 'GET',
        headers: { 'Authorization': 'Bearer ' + userToken }
    })
        .then(function (res) {
            if (!res.ok) throw new Error('export failed');
            var disposition = res.headers.get('Content-Disposition') || '';
            var fileName = 'report-users.xlsx';
            var match = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(disposition);
            if (match && match[1]) fileName = match[1].replace(/['"]/g, '');
            return res.blob().then(function (blob) { return { blob: blob, fileName: fileName }; });
        })
        .then(function (file) {
            var link = document.createElement('a');
            link.href = URL.createObjectURL(file.blob);
            link.download = file.fileName;
            document.body.appendChild(link);
            link.click();
            link.remove();
            URL.revokeObjectURL(link.href);
        })
        .catch(function () {
            if (typeof md !== 'undefined') md.showNotification('فشل تصدير الاكسل — تأكد من تسجيل الدخول');
        })
        .finally(function () {
            mouseevent("default");
            $('.progress').fadeOut();
        });
}
