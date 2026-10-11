
var _ordersPage = 1;
var _ordersPageSize = 25;
var _ordersTotalPages = 1;
var _ordersLoading = false;

function filltableOrders(payload) {
    $('#tableOrders').empty();

    var data = [];
    if (payload && Array.isArray(payload.items)) {
        data = payload.items;
        _ordersPage = payload.page || 1;
        _ordersPageSize = payload.pageSize || _ordersPageSize;
        _ordersTotalPages = payload.totalPages || 1;
        if (payload.totalCount != null) {
            $('#ordSummaryTotal').text(Number(payload.totalCount).toLocaleString());
            $('#ordersTotalCount').text(payload.totalCount);
        }
        renderOrdersPagination();
    } else if (Array.isArray(payload)) {
        data = payload;
    }

    if (!data || !data.length) {
        $('#tableOrders').append("<tr><td colspan='20' class='text-center'>لا توجد طلبات</td></tr>");
        return;
    }
    $.each(data, function (i, item) {
        var cancelDisabled = item.isCancel ? " disabled" : "";
        var deliveryFee = (item.costDelivery != null && item.costDelivery !== '') ? item.costDelivery : '-';
        var canReassign = !item.isCancel && !item.isDone && item.isDeliveryConfirmed !== true && item.isDelivered !== true;
        var reassignBtn = canReassign
            ? " <button type='button' class='btn btn-success btn-sm' onclick='openReassignDriver(" + item.orderId + ")' title='تبديل المندوب'><i class='material-icons' style='font-size:14px;vertical-align:middle'>swap_horiz</i> تبديل مندوب</button>"
            : "";
        var logBtn = " <button type='button' class='btn btn-secondary btn-sm' onclick='openOrderLog(" + item.orderId + ")' title='سجل الطلب'><i class='material-icons' style='font-size:14px;vertical-align:middle'>history</i> سجل</button>";
        var rows = "<tr>" +
            "<td class='rm-actions-cell' style='white-space:nowrap'>" +
            "<button type='button' class='btn btn-info btn-sm' onclick='OrderDetail(" + item.orderId + ")' data-toggle='modal' data-target='#OrdersDetailsModal'>تفاصيل</button> " +
            "<button type='button' class='btn btn-warning btn-sm' onclick='AdminCancelOrder(" + item.orderId + ")'" + cancelDisabled + ">الغاء</button> " +
            "<button type='button' class='btn btn-primary btn-sm' onclick='openAdminOrderControl(" + item.orderId + ")' data-toggle='modal' data-target='#AdminOrderControlModal'>تعديل</button>" +
            reassignBtn +
            logBtn +
            "</td>" +
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsDone" + item.orderId + "' disabled>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" +
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsApporve" + item.orderId + "' disabled>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" +
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsCancel" + item.orderId + "' disabled>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" +
            "<td>" + item.netAmount + "</td>" +
            "<td>" + item.totalDiscount + "</td>" +
            "<td>" + item.total + "</td>" +
            "<td>" + deliveryFee + "</td>" +
            "<td>" + (item.cityName || '') + "</td>" +
            "<td>" + (item.countriesName || '') + "</td>" +
            "<td>" + (item.phone || '') + "</td>" +
            "<td>" + (item.functionPoint || '') + "</td>" +
            "<td>" + (item.address || '') + "</td>" +
            "<td>" + (item.userName || '') + "</td>" +
            "<td>" + (item.saleManName || '') + "</td>" +
            "<td>" + (item.saleManPhone || '') + "</td>" +
            "<td>" + (item.restaurantName || '') + "</td>" +
            "<td>" + (item.categoriesName || '') + "</td>" +
            "<td>" + item.orderDate + "</td>" +
            "<td><strong>" + (item.orderNo || '') + "</strong></td></tr>";

        $('#tableOrders').append(rows);
        $('#IsDone' + item.orderId).attr('checked', item.isDone);
        $('#IsApporve' + item.orderId).attr('checked', item.isApporve);
        $('#IsCancel' + item.orderId).attr('checked', item.isCancel);
        window._ordersCache = window._ordersCache || {};
        window._ordersCache[item.orderId] = item;
    });
}

function renderOrdersPagination() {
    var $ul = $('#ordersPagination');
    if (!$ul.length) return;
    $ul.empty();

    var page = _ordersPage;
    var total = _ordersTotalPages;
    $('#ordersPageInfo').text(page + ' / ' + total);

    function addItem(label, targetPage, disabled, active) {
        var cls = 'page-item';
        if (disabled) cls += ' disabled';
        if (active) cls += ' active';
        var $li = $('<li>').addClass(cls);
        var $a = $('<a>').addClass('page-link').attr('href', '#').text(label);
        if (!disabled && !active) {
            $a.on('click', function (e) {
                e.preventDefault();
                loadOrdersPage(targetPage);
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

function loadOrdersPage(page) {
    if (_ordersLoading) return;
    if (page < 1) page = 1;
    _ordersPage = page;
    _ordersLoading = true;

    var pageSize = parseInt($('#ordersPageSize').val(), 10) || _ordersPageSize;
    _ordersPageSize = pageSize;

    var status = $("#FilterOrderStatus").val();
    var obj = {
        OrderNo: $("#OrderNo").val(),
        RestaurantName: $("#RestaurantName").val(),
        datefrom: $("#datefrom").val(),
        dateto: $("#dateto").val(),
        CountriesId: $("#CountriesId").val(),
        Phone: $("#FilterPhone").val() || '',
        orderStatus: status === '' ? null : parseInt(status, 10),
        page: _ordersPage,
        pageSize: _ordersPageSize
    };
    call_ajax("GET", "Orders/GetAll", obj, function (data) {
        _ordersLoading = false;
        filltableOrders(data);
    });
    setTimeout(function () { _ordersLoading = false; }, 8000);
}

function AdminCancelOrder(id) {
    if (!confirm("هل تريد الغاء هذا الطلب؟")) return;
    call_ajax("DELETE", "Orders/SetIsCancel/" + id, null, RefreshOrders);
}

function mapAdminStatusCode(item) {
    if (!item) return 0;
    if (item.isCancel) return 9;
    if (item.isDeliveryConfirmed) return 8;
    if (item.isDelivered === true) return 7;
    if (item.isOutForDelivery) return 6;
    if (item.isPickedUpFromRestaurant) return 5;
    if (item.isDriverEnRouteToPickup) return 4;
    if (item.isSaleManApprove === true) return 3;
    if (item.isPreparing) return 2;
    if (item.isApporve) return 1;
    return 0;
}

function openAdminOrderControl(id) {
    window._adminOrderId = id;
    var item = (window._ordersCache && window._ordersCache[id]) || {};
    $("#adminCtrlOrderNo").text(item.orderNo || id);
    $("#adminCtrlAddress").val(item.address || '');
    $("#adminCtrlPhone").val(item.phone || '');
    $("#adminCtrlFunctionPoint").val(item.functionPoint || '');
    $("#adminCtrlNotes").val(item.notes || '');
    $("#adminCtrlCostDelivery").val(item.costDelivery != null ? item.costDelivery : '');
    $("#adminCtrlStatus").val(String(mapAdminStatusCode(item)));
}

function saveAdminOrderFields() {
    var id = window._adminOrderId;
    if (!id) return;
    var payload = {
        OrderId: id,
        Address: $("#adminCtrlAddress").val(),
        Phone: $("#adminCtrlPhone").val(),
        FunctionPoint: $("#adminCtrlFunctionPoint").val(),
        Notes: $("#adminCtrlNotes").val(),
        CostDelivery: $("#adminCtrlCostDelivery").val() === '' ? null : parseFloat($("#adminCtrlCostDelivery").val())
    };
    $.ajax({
        method: "POST",
        url: "/Orders/AdminUpdateOrder",
        contentType: "application/json",
        data: JSON.stringify(payload),
        headers: { 'Authorization': 'Bearer ' + getCookie("token2") },
        success: function (resp) {
            if (resp.success) {
                md.showNotification(resp.msg || "تم التحديث");
                RefreshOrders();
            } else {
                md.showNotification(resp.msg || "حدث خطأ");
            }
        },
        error: function () { md.showNotification("حدث خطأ عند الاتصال"); }
    });
}

function applyAdminOrderStatus() {
    var id = window._adminOrderId;
    if (!id) return;
    var statusCode = parseInt($("#adminCtrlStatus").val(), 10);
    if (isNaN(statusCode)) {
        md.showNotification("اختر الحالة");
        return;
    }
    if (statusCode === 9 && !confirm("تأكيد الغاء الطلب؟")) return;
    $.ajax({
        method: "POST",
        url: "/Orders/AdminSetStatus/" + id + "?statusCode=" + statusCode,
        headers: { 'Authorization': 'Bearer ' + getCookie("token2") },
        success: function (resp) {
            if (resp.success) {
                md.showNotification(resp.msg || "تم تحديث الحالة");
                $('#AdminOrderControlModal').modal('hide');
                RefreshOrders();
            } else {
                md.showNotification(resp.msg || "حدث خطأ");
            }
        },
        error: function () { md.showNotification("حدث خطأ عند الاتصال"); }
    });
} 
function filltableOrdersForRes(data) {
    $('#tableOrdersForRes').empty();
    $.each(data, function (i, item) {
        var rows = "<tr>" +  
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsDone" + item.orderId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" + 
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsApporve" + item.orderId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" + 
            "<td>" +
            "<div class='form-check'>" +
            "<label class='form-check-label'>" +
            "<input class='form-check-input' type='checkbox' id='IsCancel" + item.orderId + "'>" +
            "<span class='form-check-sign'>" +
            "<span class='check'></span>" +
            "</span>" +
            "</label>" +
            "</div>" +
            "</td>" +
            "<td>" + item.netAmount + "</td>" +
            "<td>" + item.totalDiscount + "</td>" +
            "<td>" + item.total + "</td>" +
            "<td>" + item.cityName + "</td>" +
            "<td>" + item.countriesName + "</td>" +
            "<td>" + item.phone + "</td>" +
            "<td>" + item.notes + "</td>" +
            "<td>" + item.functionPoint + "</td>" +
            "<td>" + item.address + "</td>" +
            "<td>" + item.userName + "</td>" +
            "<td>" + item.saleManName + "</td>" + 
            "<td>" + item.orderDate + "</td>" +
            "<td>" + item.orderNo + "</td>" 
            + "<td> <button type='button' class='btn btn-primary' onclick='SetIsDone(" + item.orderId + ")'  >انتهاء الطلب</button></td>"
            + " <td>  <button type='button' class='btn btn-warning' onclick='SetIsCancel(" + item.orderId + ")'  >الغاء</button></td>"
            + " <td>  <button type='button' class='btn btn-success' onclick='SetIsApporve(" + item.orderId + ")'  >موافقة</button></td>"
            + " <td>  <button type='button' class='btn btn-danger' onclick='deleteOrders(" + item.orderId + ")'  >حذف</button></td>" +
            " <td>  <button type='button' class='btn btn-success' onclick='setsaleman(" + item.orderId + ")'  data-toggle='modal' data-target='#setsalemanModal'>اختيار مندوب</button></td>" +
            "  <td>  <button type='button' class='btn btn-info' onclick='OrderDetailForRes(" + item.orderId + ")'  data-toggle='modal' data-target='#OrdersDetailsForResModal'>تفاصيل الطلب</button></td></tr>";

        $('#tableOrdersForRes').append(rows); 
        $('#IsDone' + item.orderId).attr('checked', item.isDone);
        $('#IsApporve' + item.orderId).attr('checked', item.isApporve);
        $('#IsCancel' + item.orderId).attr('checked', item.isCancel);
    });
}


function SetIsCancel(id) { 
    call_ajax("DELETE", "Orders/SetIsCancel/" + id, null, RefreshOrdersForRes);
}
function SetIsApporve(id) {

    call_ajax("Post", "Orders/SetIsApporve/"+id, null, RefreshOrdersForRes);
}
function SetIsDone(id) { 

    call_ajax("Post", "Orders/SetIsDone/" + id, null, RefreshOrdersForRes);
}

var _orderid = 0;
function setsaleman(id) { 
    _orderid = id; 
}
function deleteOrders(id) {
    var result = confirm("هل تريد الحذف؟!");
    if (result == true) {
        var object1 = {
            Id: id,
        }
        call_ajax("DELETE", "Orders/Delete", object1, RefreshOrdersForRes);
    }
} 
var _id = 0;
function OrderDetail(id) {
    var object1 = {
        Id: id,
    }
    _id = id;
    call_ajax("Get", "Orders/GetOrdersWithDetailAll", object1, SetDataOrderDetail);
}
function OrderDetailForRes(id) {
    var object1 = {
        Id: id,
    }
    _id = id;
    call_ajax("Get", "Orders/GetOrdersWithDetailAll", object1, SetDataOrderDetailForRes);
}

function SetDataOrderDetail(data) {
    var details = data;
    if (data && data.details) {
        details = data.details;
        if (data.driver) {
            $('#orderDriverInfo').remove();
            var d = data.driver;
            $('#tableOrdersDetail').before(
                '<div id="orderDriverInfo" class="alert alert-info" style="animation:fadeIn .4s">' +
                '<strong>السائق:</strong> ' + (d.name || '-') + ' | <strong>هاتف:</strong> ' + (d.phone || '-') + '</div>');
        }
    }
    $('#tableOrdersDetail').empty();
    $.each(details, function (i, item) {
        var rows = "<tr>" +
            "<td> " + item.count + "</td> " +  
            "<td> " + item.price + "</td> " +  
            "<td>" + item.productsDetails + "</td>" + 
            "<td>" + item.productsName + "</td>" +
            "<td>" + item.subCategoriesName + "</td>" +
            "<td class='rm-img-cell'><img src='" + item.productsImage + "' alt='' class='rm-table-thumb'></td>" 
            + "<td> <button type='button' class='btn btn-danger' onclick='deleteOrderDetail(" + item.orderDetailId + ")'  >حذف</button></td></tr>";
        $('#tableOrdersDetail').append(rows);
    });
}
function SetDataOrderDetailForRes(data) {
    var details = (data && data.details) ? data.details : data;
    $('#tableOrdersDetailForRes').empty();
    $.each(details, function (i, item) {
        var rows = "<tr>" +
            "<td> " + item.notes2 + "</td> " +  
            "<td> " + item.count + "</td> " +  
            "<td> " + item.price + "</td> " +  
            "<td>" + item.productsDetails + "</td>" + 
            "<td>" + item.productsName + "</td>" +
            "<td>" + item.subCategoriesName + "</td>" +
            "<td class='rm-img-cell'><img src='" + item.productsImage + "' alt='' class='rm-table-thumb'></td></tr>";
        $('#tableOrdersDetailForRes').append(rows);
    });
}


function deleteOrderDetail(id) {
    var object1 = {
        Id: id,
    }
    call_ajax("DELETE", "Orders/DeleteDetails", object1, RefreshOrderDetail);
}
function RefreshOrderDetail() {
    OrderDetail(_id);
}
function RefreshOrders() {
    loadOrdersPage(1);
}

var _orderLogCache = [];

function actorTypeLabel(t) {
    t = (t || '').toLowerCase();
    if (t === 'admin') return 'أدمن';
    if (t === 'restaurant') return 'مطعم';
    if (t === 'driver') return 'مندوب';
    if (t === 'customer') return 'زبون';
    return 'نظام';
}

function renderOrderLogRows(logs) {
    logs = logs || [];
    var q = ($('#orderLogSearch').val() || '').trim().toLowerCase();
    if (q) {
        logs = logs.filter(function (l) {
            var hay = [
                l.title || l.Title,
                l.details || l.Details,
                l.actorName || l.ActorName,
                l.actorType || l.ActorType,
                l.action || l.Action
            ].join(' ').toLowerCase();
            return hay.indexOf(q) !== -1;
        });
    }
    if (!logs.length) {
        $('#orderLogTimeline').html('<div class="text-center text-muted py-4">لا توجد أحداث مطابقة</div>');
        return;
    }
    var html = '<ul class="list-unstyled mb-0" style="border-right:2px solid #e2e8f0;padding-right:14px;">';
    logs.forEach(function (l) {
        var title = l.title || l.Title || '—';
        var details = l.details || l.Details || '';
        var actorType = actorTypeLabel(l.actorType || l.ActorType);
        var actorName = l.actorName || l.ActorName || '—';
        var at = l.createdAt || l.CreatedAt;
        var when = at ? new Date(at).toLocaleString('ar-IQ') : '—';
        html += '<li class="mb-3">' +
            '<div style="font-size:12px;color:#64748b;margin-bottom:2px">' + when +
            ' · <span class="badge badge-info">' + actorType + '</span> ' + (actorName || '') + '</div>' +
            '<div style="font-weight:700">' + title + '</div>' +
            (details ? '<div style="font-size:13px;color:#475569">' + details + '</div>' : '') +
            '</li>';
    });
    html += '</ul>';
    $('#orderLogTimeline').html(html);
}

function openOrderLog(orderId) {
    var cached = (window._ordersCache && window._ordersCache[orderId]) || {};
    $('#orderLogOrderLabel').text(cached.orderNo || orderId);
    $('#orderLogSearch').val('');
    $('#orderLogTimeline').html('<div class="text-center text-muted py-4">جاري التحميل...</div>');
    $('#orderLogModal').modal('show');
    call_ajax('GET', 'admin/orders/' + orderId + '/logs', null, function (data) {
        data = data || {};
        var order = data.order || data.Order || {};
        var orderNo = order.orderNo || order.OrderNo || cached.orderNo || orderId;
        $('#orderLogOrderLabel').text(orderNo);
        _orderLogCache = data.logs || data.Logs || [];
        renderOrderLogRows(_orderLogCache);
    });
}

$(document).on('input', '#orderLogSearch', function () {
    renderOrderLogRows(_orderLogCache);
});

function openReassignDriver(orderId) {
    var cached = (window._ordersCache && window._ordersCache[orderId]) || {};
    $('#reassignOrderId').val(orderId);
    $('#reassignOrderLabel').text(cached.orderNo || orderId);
    $('#reassignDriverSelect').html('<option value="">جاري التحميل...</option>');
    $('#reassignDriverModal').modal('show');
    call_ajax('GET', 'admin/orders/drivers', null, function (data) {
        var rows = data || [];
        var opts = '<option value="">— اختر —</option>';
        rows.forEach(function (d) {
            var id = d.saleManId != null ? d.saleManId : d.SaleManId;
            var name = d.name || d.Name || '-';
            var phone = d.phone || d.Phone || '';
            var avail = d.isAvailable !== undefined ? d.isAvailable : d.IsAvailable;
            var idle = d.isIdle !== undefined ? d.isIdle : d.IsIdle;
            var active = d.activeOrders != null ? d.activeOrders : (d.ActiveOrders || 0);
            var tag = !avail ? ' (متوقف)' : (idle ? ' — فارغ' : ' — مشغول (' + active + ')');
            opts += '<option value="' + id + '"' + (!avail ? ' disabled' : '') + '>' +
                name + (phone ? ' | ' + phone : '') + tag + '</option>';
        });
        $('#reassignDriverSelect').html(opts);
    });
}

$(document).on('click', '#btnConfirmReassign', function () {
    var orderId = parseInt($('#reassignOrderId').val(), 10);
    var saleManId = parseInt($('#reassignDriverSelect').val(), 10);
    if (!orderId) { md.showNotification('طلب غير صالح'); return; }
    if (!saleManId) { md.showNotification('اختر مندوباً'); return; }
    call_ajax_json('POST', 'admin/orders/' + orderId + '/reassign-driver', { saleManId: saleManId }, function () {
        $('#reassignDriverModal').modal('hide');
        loadOrdersPage(_ordersPage);
    });
});

function RefreshOrdersForRes() {
    _orderid = 0; $("#SaleManId").val(0).change();
    var obj = {
        OrderNo: $("#OrderNo").val(), RestaurantName: '',
        datefrom: $("#datefrom").val(),
        dateto: $("#dateto").val(),
    }
    call_ajax("GET", "Orders/GetAll", obj, filltableOrdersForRes);
}
