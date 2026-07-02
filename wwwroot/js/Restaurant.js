
var _RestaurantId = 0;

function rmEscAttr(s) {
    return (s || '').replace(/\\/g, '\\\\').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
}

function rmRestStatusBadges(item) {
    var html = '';
    if (item.isActive) {
        html += '<span class="rm-status-badge rm-status-active">نشط</span> ';
    } else {
        html += '<span class="rm-status-badge rm-status-inactive">غير نشط</span> ';
    }
    if (item.isTop) {
        html += '<span class="rm-status-badge rm-status-top">مفضلة</span> ';
    }
    if (item.isStars) {
        html += '<span class="rm-status-badge rm-status-stars">تقييم</span> ';
    }
    return '<td class="rm-status-cell">' + (html || '<span class="text-muted">—</span>') + '</td>';
}

function rmRestOpenCloseCell(item) {
    var closed = !!item.isClosed;
    var badge = closed
        ? '<span class="rm-status-badge rm-status-closed">مغلق</span>'
        : '<span class="rm-status-badge rm-status-open">مفتوح</span>';
    var btnLabel = closed ? 'فتح' : 'إغلاق';
    var btnClass = closed ? 'btn-success' : 'btn-warning';
    return '<td class="rm-openclose-cell">' + badge +
        ' <button type="button" class="btn btn-sm ' + btnClass + '" style="margin-right:6px" ' +
        'onclick="toggleRestaurantClosed(' + item.restaurantId + ',' + closed + ')">' +
        btnLabel + '</button></td>';
}

function toggleRestaurantClosed(id, currentlyClosed) {
    var newClosed = !currentlyClosed;
    var verb = newClosed ? 'إغلاق' : 'فتح';
    if (!confirm('هل تريد ' + verb + ' هذا المتجر؟')) return;
    call_ajax('POST', 'Restaurant/SetIsColsed/' + id + '/' + newClosed, null, RefreshRestaurant);
}

function filltableRestaurant(data) {
    $('#tableRestaurant').empty();
    if (data.length === 0) {
        md.showNotification('لا توجد معلومات');
        return;
    }
    $.each(data, function (i, item) {
        var safeName = rmEscAttr(item.name);
        var rows = "<tr>" +
            "<td class='rm-actions-cell'><div class='rm-actions'>" +
            "<button type='button' class='btn btn-primary btn-sm' onclick='updateRestaurant(" + item.restaurantId + ")' data-toggle='modal' data-target='#RestaurantModal'>تعديل</button> " +
            "<button type='button' class='btn btn-danger btn-sm' onclick='deleteRestaurant(" + item.restaurantId + ")'>حذف</button></div></td>" +
            "<td class='rm-actions-cell'><button type='button' class='btn btn-info btn-sm' onclick='openRestaurantProducts(" + item.restaurantId + ",\"" + safeName + "\")' data-toggle='modal' data-target='#ResProductsModal'>المنتجات</button></td>" +
            "<td><strong>" + (item.name || '') + "</strong></td>" +
            "<td>" + (item.categoriesName || '') + "</td>" +
            "<td class='rm-wrap-cell'>" + (item.address || '') + "</td>" +
            rmRestStatusBadges(item) +
            rmRestOpenCloseCell(item) +
            "</tr>";
        $('#tableRestaurant').append(rows);
    });
    if (window.RM && RM.initTableWraps) {
        RM.initTableWraps(document.getElementById('tableRestaurant').closest('.card-body'));
    }
}

function filltableResNotApproveAll(data) {
    $('#tableRestaurant').empty();
    if (data.length === 0) {
        md.showNotification('لا توجد معلومات');
        return;
    }
    $.each(data, function (i, item) {
        var rows = "<tr>" +
            "<td class='rm-actions-cell'><button type='button' class='btn btn-danger btn-sm' onclick='SetIsApproveShop(" + item.restaurantId + ")'>موافق</button></td>" +
            "<td><strong>" + (item.name || '') + "</strong></td>" +
            "<td class='rm-wrap-cell'>" + (item.details || '') + "</td>" +
            "<td>" + (item.categoriesName || '') + "</td>" +
            "<td class='rm-img-cell'><img src='" + (item.background || '') + "' alt='' class='rm-table-thumb'></td>" +
            "<td class='rm-img-cell'><img src='" + (item.logo || '') + "' alt='' class='rm-table-thumb'></td>" +
            "<td>" + (item.phone || '') + "</td>" +
            "<td>" + (item.address || '') + "</td></tr>";
        $('#tableRestaurant').append(rows);
    });
}

function deleteRestaurant(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = { Id: id };
        call_ajax("DELETE", "Restaurant/Delete", object1, RefreshRestaurant);
    }
}
function RefreshRestaurant() {
    var count = $("#indexid").text();
    var obj = { Name: $("#Namese").val(), index: count };
    call_ajax("GET", "Restaurant/GetAll", obj, filltableRestaurant);
}

function RefreshResNotApprove() {
    call_ajax("GET", "Restaurant/GetResNotApproveAll", null, filltableResNotApproveAll);
}
function SetIsApproveShop(id) {
    var obj = { Id: id };
    call_ajax("POST", "Restaurant/SetIsApproved", obj, RefreshResNotApprove);
}

function updateRestaurant(id) {
    var object1 = { Id: id };
    call_ajax("GET", "Restaurant/GetById", object1, setdataRestaurant);
    _RestaurantId = id;
}

function setdataRestaurant(data) {
    $("#Name").val(data.name);
    $("#Details").val(data.details);
    $("#Address").val(data.address);
    $("#Phone").val(data.phone);
    $("#MinimumPrice").val(data.minimumPrice);
    $("#Areaname").val(data.areaname);
    $("#Password").val(data.password);
    $("#Lat").val(data.lat);
    $("#Long").val(data.long);
    $("#Whatsapp").val(data.whatsapp);
    $("#UserName").val(data.userName);
    $("#Insta").val(data.insta);
    $("#CostDelivery").val(data.costDelivery);
    $("#CategoriesId").val(data.categoriesId).change();
    $("#IsStars").prop("checked", data.isStars === true);
    $("#IsActive").prop("checked", data.isActive === true);
    $("#IsClosed").prop("checked", data.isClosed === true);
    $("#IsTop").prop("checked", data.isTop === true);
}

function aftersaveRestaurant() {
    $("#Name").val('');
    $("#Details").val('');
    $("#Address").val('');
    $("#Logo").val('');
    $("#Background").val('');
    $("#Phone").val('');
    $("#MinimumPrice").val('');
    $("#CostDelivery").val('');
    $("#Areaname").val('');
    $("#Lat").val('');
    $("#Long").val('');
    $("#Whatsapp").val('');
    $("#Password").val('');
    $("#UserName").val('');
    $("#Insta").val('');
    $("#CategoriesId").val(0).change();
    $("#IsStars").prop("checked", true);
    $("#IsActive").prop("checked", true);
    $("#IsClosed").prop("checked", false);
    $("#IsTop").prop("checked", false);
    _RestaurantId = 0;
    RefreshRestaurant();
}

var _resProdRestaurantId = 0;
var _resProdEditId = 0;
var _resProdStoreLabel = '';

function resetResProductForm() {
    _resProdEditId = 0;
    $('#resProdName,#resProdDetails,#resProdPrice,#resProdImage').val('');
    $('#resProdPrep').val(15);
    $('#resProdSubCat').val(0);
}

function openRestaurantProducts(restaurantId, restaurantName) {
    _resProdRestaurantId = restaurantId;
    _resProdStoreLabel = restaurantName || '';
    resetResProductForm();
    $('#resProdTitle').html('<i class="material-icons" style="vertical-align:middle;margin-left:8px;font-size:22px;">restaurant_menu</i> منتجات: ' + _resProdStoreLabel);
    $('#resProdStoreName').text(_resProdStoreLabel);
    $('#resProdSearch').val('');
    loadResSubCategories(restaurantId);
    loadResProducts();
}

function loadResSubCategories(restaurantId) {
    call_ajax('GET', 'RestaurantSubCategories/GetByResId/' + restaurantId, null, function (data) {
        var sel = $('#resProdSubCat');
        sel.find('option:not(:first)').remove();
        (data || []).forEach(function (s) {
            sel.append('<option value="' + s.subCategoriesId + '">' + (s.subCategoriesName || '') + '</option>');
        });
    });
}

function renderResProductsTable(data) {
    var list = data || [];
    var availCount = 0;
    $('#resProdTable').empty();
    list.forEach(function (p, i) {
        var avail = p.isAvailable !== false;
        if (avail) availCount++;
        var img = p.productsImageFirst
            ? "<img src='" + p.productsImageFirst + "' alt='' class='rm-table-thumb'>"
            : "<span class='text-muted'>—</span>";
        var availBadge = avail
            ? "<span class='badge-avail-yes'>متوفر</span>"
            : "<span class='badge-avail-no'>غير متوفر</span>";
        $('#resProdTable').append(
            '<tr style="animation:rmFadeInUp .35s ease ' + (i * 0.03) + 's both">' +
            '<td class="rm-actions-cell"><div class="rm-actions">' +
            '<button class="btn btn-primary btn-sm" onclick="editResProduct(' + p.productsId + ')">تعديل</button> ' +
            '<button class="btn btn-danger btn-sm" onclick="deleteResProduct(' + p.productsId + ')">حذف</button> ' +
            '<button class="btn btn-sm ' + (avail ? 'btn-warning' : 'btn-success') + '" onclick="toggleResProdAvail(' + p.productsId + ',' + !avail + ')">' + (avail ? 'إيقاف' : 'تفعيل') + '</button>' +
            '</div></td>' +
            '<td>' + availBadge + '</td>' +
            '<td>' + (p.preparationTimeMinutes || 15) + ' د</td>' +
            '<td><strong>' + (p.productsPrice != null ? p.productsPrice : '') + '</strong></td>' +
            '<td>' + (p.subCategoriesName || '—') + '</td>' +
            '<td class="rm-wrap-cell">' + (p.productsDetails || '') + '</td>' +
            '<td><strong>' + (p.productsName || '') + '</strong></td>' +
            '<td>' + img + '</td></tr>'
        );
    });
    $('#resProdCount').text(list.length);
    $('#resProdAvailCount').text(availCount);
    if (window.RM && RM.initTableWraps) {
        RM.initTableWraps(document.getElementById('resProdTableWrap'));
    }
}

function loadResProducts() {
    if (!_resProdRestaurantId) return;
    var name = ($('#resProdSearch').val() || '').trim();
    var url = name.length
        ? 'Products/GetByRestaurantId/' + _resProdRestaurantId + ',0,' + encodeURIComponent(name)
        : 'Products/GetByRestaurantId/' + _resProdRestaurantId + '/0';
    $('#resProdLoading').show();
    $('#resProdTableWrap').css('opacity', '0.5');
    call_ajax('GET', url, null, function (data) {
        $('#resProdLoading').hide();
        $('#resProdTableWrap').css('opacity', '1');
        renderResProductsTable(data);
    });
}

function toggleResProdAvail(id, isAvailable) {
    call_ajax('POST', 'Products/SetIsAvailable/' + id + '/' + isAvailable, null, loadResProducts);
}

function editResProduct(id) {
    call_ajax('GET', 'Products/GetById/' + id, null, function (p) {
        if (!p) return;
        _resProdEditId = id;
        $('#resProdName').val(p.productsName);
        $('#resProdDetails').val(p.productsDetails);
        $('#resProdPrice').val(p.productsPrice);
        $('#resProdPrep').val(p.preparationTimeMinutes || 15);
        $('#resProdSubCat').val(p.subCategoriesId || 0);
        document.querySelector('.res-prod-form-card').scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    });
}

function deleteResProduct(id) {
    if (!confirm('حذف المنتج؟')) return;
    call_ajax('DELETE', 'Products/Delete', { Id: id }, loadResProducts);
}

function saveResProduct() {
    if (!$('#resProdName').val().trim()) {
        md.showNotification('رجاءاً أدخل اسم المنتج');
        return;
    }
    var subCatId = parseInt($('#resProdSubCat').val(), 10) || 0;
    if (subCatId <= 0) {
        md.showNotification('رجاءاً اختر الصنف');
        return;
    }
    var fd = new FormData();
    fd.append('ProductsId', _resProdEditId);
    fd.append('ProductsName', $('#resProdName').val());
    fd.append('ProductsDetails', $('#resProdDetails').val());
    fd.append('ProductsPrice', $('#resProdPrice').val() || 0);
    fd.append('PreparationTimeMinutes', $('#resProdPrep').val() || 15);
    fd.append('IsAvailable', true);
    fd.append('RestaurantId', _resProdRestaurantId);
    fd.append('SubCategoriesId', subCatId);
    var f = $('#resProdImage')[0].files[0];
    if (f) fd.append('FileChoose', f);
    call_ajax_withfile('POST', 'Products/Post', fd, function () {
        resetResProductForm();
        loadResProducts();
    });
}
