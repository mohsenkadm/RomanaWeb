var _qpRowIndex = 0;
var _qpSubCategories = [];
var _qpInitialized = false;

function qpInit() {
    if (_qpInitialized) return;
    _qpInitialized = true;

    call_ajax('GET', 'Restaurant/GetAll', { Name: '', index: 0 }, qpFillRestaurants);
    qpLoadSubCategories(0, qpSeedRows);

    $('#qpRestaurantId').on('change', function () {
        var resId = parseInt($(this).val(), 10) || 0;
        qpLoadSubCategories(resId);
    });

    $('#qpAddRowBtn').on('click', qpAddRow);
    $('#qpSaveAllBtn').on('click', qpSaveAll);
    $('#qpClearAllBtn').on('click', qpClearAll);
    $('#qpDefaultSubCat').on('change', qpUpdateCount);
    $(document).on('input', '.qp-name, .qp-price, .qp-subcat', qpUpdateCount);
}

function qpSeedRows() {
    qpAddRow();
    qpAddRow();
    qpAddRow();
}

function qpLoadSubCategories(restaurantId, callback) {
    if (restaurantId > 0) {
        call_ajax('GET', 'RestaurantSubCategories/GetByResId/' + restaurantId, null, function (data) {
            qpApplySubCategories(data);
            if (typeof callback === 'function') callback();
        });
        return;
    }

    call_ajax('GET', 'SubCategories/GetAll', { Name: '' }, function (data) {
        qpApplySubCategories(data);
        if (typeof callback === 'function') callback();
    });
}

function qpFillRestaurants(data) {
    var sel = $('#qpRestaurantId');
    sel.find('option:not(:first)').remove();
    (data || []).forEach(function (r) {
        sel.append('<option value="' + r.restaurantId + '">' + (r.name || '') + '</option>');
    });
}

function qpApplySubCategories(data) {
    _qpSubCategories = data || [];

    var sel = $('#qpDefaultSubCat');
    sel.find('option:not(:first)').remove();
    _qpSubCategories.forEach(function (s) {
        var id = s.subCategoriesId;
        var name = s.subCategoriesName || '';
        sel.append('<option value="' + id + '">' + name + '</option>');
    });

    qpRefreshAllRowSubCats();
}

function qpRefreshAllRowSubCats() {
    $('#qpRows tr').each(function () {
        var current = parseInt($(this).find('.qp-subcat').val(), 10) || 0;
        $(this).find('.qp-subcat').html(qpSubCatOptions(current));
    });
}

function qpSubCatOptions(selectedId) {
    var html = '<option value="0">— افتراضي —</option>';
    _qpSubCategories.forEach(function (s) {
        var id = s.subCategoriesId;
        var name = s.subCategoriesName || '';
        var sel = (selectedId && selectedId == id) ? ' selected' : '';
        html += '<option value="' + id + '"' + sel + '>' + name + '</option>';
    });
    return html;
}

function qpAddRow() {
    _qpRowIndex++;
    var id = _qpRowIndex;
    var rowNum = $('#qpRows tr').length + 1;
    var row = '<tr data-row-id="' + id + '">' +
        '<td class="qp-row-num text-muted">' + rowNum + '</td>' +
        '<td><input type="text" class="form-control form-control-sm qp-name alignright" placeholder="اسم المنتج" autocomplete="off" /></td>' +
        '<td><input type="number" step="0.01" min="0" class="form-control form-control-sm qp-price alignright" placeholder="0" /></td>' +
        '<td><select class="form-control form-control-sm qp-subcat alignright">' + qpSubCatOptions() + '</select></td>' +
        '<td><input type="text" class="form-control form-control-sm qp-details alignright" placeholder="اختياري" autocomplete="off" /></td>' +
        '<td><input type="file" accept="image/*" class="form-control form-control-sm qp-image" /></td>' +
        '<td><button type="button" class="btn btn-sm btn-outline-danger qp-remove" title="حذف"><i class="material-icons" style="font-size:16px;">close</i></button></td>' +
        '</tr>';
    $('#qpRows').append(row);
    qpBindRowEvents(id);
    qpRenumberRows();
    qpUpdateCount();
    $('#qpRows tr[data-row-id="' + id + '"] .qp-name').focus();
}

function qpBindRowEvents(id) {
    var $row = $('#qpRows tr[data-row-id="' + id + '"]');
    $row.find('.qp-price').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            qpAddRow();
        }
    });
    $row.find('.qp-remove').on('click', function () {
        if ($('#qpRows tr').length <= 1) {
            $row.find('input, select').val('');
            $row.find('.qp-subcat').val(0);
            qpUpdateCount();
            return;
        }
        $row.remove();
        qpRenumberRows();
        qpUpdateCount();
    });
}

function qpRenumberRows() {
    $('#qpRows tr').each(function (i) {
        $(this).find('.qp-row-num').text(i + 1);
    });
}

function qpUpdateCount() {
    var count = 0;
    $('#qpRows tr').each(function () {
        var name = $(this).find('.qp-name').val().trim();
        var price = parseFloat($(this).find('.qp-price').val());
        if (name && price > 0) count++;
    });
    $('#qpReadyCount').text(count);
}

function qpClearAll() {
    if (!confirm('هل تريد مسح جميع الصفوف؟')) return;
    $('#qpRows').empty();
    _qpRowIndex = 0;
    qpAddRow();
    qpAddRow();
    qpAddRow();
    $('#qpResultSummary').hide().empty();
}

function qpReadFileAsBase64(file) {
    return new Promise(function (resolve, reject) {
        var reader = new FileReader();
        reader.onload = function () { resolve(reader.result); };
        reader.onerror = reject;
        reader.readAsDataURL(file);
    });
}

function qpCollectRows() {
    var items = [];
    var defaultSub = parseInt($('#qpDefaultSubCat').val(), 10) || 0;

    $('#qpRows tr').each(function (i) {
        var name = $(this).find('.qp-name').val().trim();
        var price = parseFloat($(this).find('.qp-price').val());
        if (!name || !(price > 0)) return;

        var subCat = parseInt($(this).find('.qp-subcat').val(), 10) || 0;
        if (subCat <= 0) subCat = defaultSub > 0 ? defaultSub : 0;

        items.push({
            index: i,
            name: name,
            price: price,
            subCategoriesId: subCat,
            details: $(this).find('.qp-details').val().trim(),
            fileInput: $(this).find('.qp-image')[0]
        });
    });
    return items;
}

async function qpSaveAll() {
    var restaurantId = parseInt($('#qpRestaurantId').val(), 10) || 0;
    if (restaurantId <= 0) {
        md.showNotification('رجاءاً اختر المطعم أولاً');
        return;
    }

    var rows = qpCollectRows();
    if (rows.length === 0) {
        md.showNotification('أدخل منتجاً واحداً على الأقل (اسم + سعر)');
        return;
    }

    var missingSub = rows.filter(function (r) { return r.subCategoriesId <= 0; });
    if (missingSub.length > 0) {
        md.showNotification('حدد الصنف الافتراضي أو صنفاً لكل منتج');
        return;
    }

    $('#qpSaveAllBtn').prop('disabled', true);
    $('#qpProgress').show();
    $('#qpProgressBar').css('width', '30%');
    $('#qpProgressText').text('جاري تجهيز البيانات...');
    $('#qpResultSummary').hide().empty();

    var payload = {
        restaurantId: restaurantId,
        defaultSubCategoriesId: parseInt($('#qpDefaultSubCat').val(), 10) || null,
        defaultPreparationTimeMinutes: parseInt($('#qpDefaultPrep').val(), 10) || 15,
        items: []
    };

    for (var i = 0; i < rows.length; i++) {
        var r = rows[i];
        var item = {
            productsName: r.name,
            productsPrice: r.price,
            subCategoriesId: r.subCategoriesId,
            productsDetails: r.details
        };
        if (r.fileInput && r.fileInput.files && r.fileInput.files[0]) {
            item.base64Image = await qpReadFileAsBase64(r.fileInput.files[0]);
        }
        payload.items.push(item);
        $('#qpProgressBar').css('width', (30 + ((i + 1) / rows.length) * 50) + '%');
    }

    $('#qpProgressText').text('جاري الحفظ...');
    $('#qpProgressBar').css('width', '85%');

    call_ajax_json('POST', 'Products/PostBulk', payload, function (data) {
        qpFinishSave(data);
    });
}

function qpFinishSave(data) {
    $('#qpSaveAllBtn').prop('disabled', false);
    $('#qpProgressBar').css('width', '100%');
    $('#qpProgressText').text('اكتمل الحفظ');

    var saved = (data && data.saved) ? data.saved.length : 0;
    var failed = (data && data.failed) ? data.failed : [];
    var html = '<div class="alert ' + (failed.length ? 'alert-warning' : 'alert-success') + ' alignright">' +
        '<strong>النتيجة:</strong> تم حفظ ' + saved + ' منتج';
    if (failed.length) {
        html += ' — فشل ' + failed.length + '<ul class="mb-0 mt-2">';
        failed.forEach(function (f) {
            html += '<li>' + (f.name || ('صف ' + ((f.index || 0) + 1))) + ': ' + (f.reason || '') + '</li>';
        });
        html += '</ul>';
    }
    html += '</div>';
    $('#qpResultSummary').html(html).show();

    if (saved > 0 && failed.length === 0) {
        $('#qpRows').empty();
        _qpRowIndex = 0;
        qpAddRow();
        qpAddRow();
        qpAddRow();
    }
    qpUpdateCount();

    setTimeout(function () {
        $('#qpProgress').fadeOut();
        $('#qpProgressBar').css('width', '0%');
    }, 1200);
}
