
function fmtIqd(n) {
    var v = Math.round(Number(n) || 0);
    return v.toLocaleString('en-US') + ' د.ع';
}

function val(obj) {
    var keys = Array.prototype.slice.call(arguments, 1);
    for (var i = 0; i < keys.length; i++) {
        if (obj && obj[keys[i]] !== undefined && obj[keys[i]] !== null) return obj[keys[i]];
    }
    return undefined;
}

function renderResTotals(totals) {
    totals = totals || {};
    $('#resTotCount').text(val(totals, 'restaurantsCount', 'RestaurantsCount') || 0);
    $('#resTotOrders').text(val(totals, 'countOrder', 'CountOrder') || 0);
    $('#resTotTotal').text(fmtIqd(val(totals, 'total', 'Total') || 0));
    $('#resTotDiscount').text(fmtIqd(val(totals, 'totalDiscount', 'TotalDiscount') || 0));
    $('#resTotNet').text(fmtIqd(val(totals, 'netAmount', 'NetAmount') || 0));
    $('#resTotDelivery').text(fmtIqd(val(totals, 'costDelivery', 'CostDelivery') || 0));
}

function filltableReportRes(payload) {
    $('#tableReportRes').empty();
    var rows = [];
    var totals = null;
    if (payload && Array.isArray(payload)) {
        rows = payload;
    } else if (payload) {
        rows = val(payload, 'rows', 'Rows') || [];
        totals = val(payload, 'totals', 'Totals');
    }

    if (!totals) {
        totals = {
            restaurantsCount: rows.length,
            countOrder: rows.reduce(function (s, r) { return s + (Number(val(r, 'countOrder', 'CountOrder')) || 0); }, 0),
            total: rows.reduce(function (s, r) { return s + (Number(val(r, 'total', 'Total')) || 0); }, 0),
            totalDiscount: rows.reduce(function (s, r) { return s + (Number(val(r, 'totalDiscount', 'TotalDiscount')) || 0); }, 0),
            netAmount: rows.reduce(function (s, r) { return s + (Number(val(r, 'netAmount', 'NetAmount')) || 0); }, 0),
            costDelivery: rows.reduce(function (s, r) { return s + (Number(val(r, 'costDelivery', 'CostDelivery', 'totalNetDelivery', 'TotalNetDelivery')) || 0); }, 0)
        };
    }
    renderResTotals(totals);

    if (!rows.length) {
        $('#tableReportRes').append("<tr><td colspan='6' class='text-center'>لا توجد بيانات</td></tr>");
        return;
    }

    $.each(rows, function (i, item) {
        var name = val(item, 'name', 'Name') || '-';
        var countOrder = val(item, 'countOrder', 'CountOrder') || 0;
        var total = val(item, 'total', 'Total') || 0;
        var discount = val(item, 'totalDiscount', 'TotalDiscount') || 0;
        var net = val(item, 'netAmount', 'NetAmount') || 0;
        var delivery = val(item, 'costDelivery', 'CostDelivery', 'totalNetDelivery', 'TotalNetDelivery') || 0;
        var rowsHtml = "<tr>" +
            "<td>" + name + "</td>" +
            "<td>" + countOrder + "</td>" +
            "<td>" + fmtIqd(total) + "</td>" +
            "<td>" + fmtIqd(discount) + "</td>" +
            "<td>" + fmtIqd(net) + "</td>" +
            "<td>" + fmtIqd(delivery) + "</td></tr>";
        $('#tableReportRes').append(rowsHtml);
    });
}

function RefreshReportRes() {
    var obj = {
        RestaurantName: $("#RestaurantName").val(),
        datefrom: $("#datefrom").val(),
        dateto: $("#dateto").val()
    };
    call_ajax("GET", "Restaurant/GetReportRes", obj, filltableReportRes);
}

$(document).on('click', '#excel-btn', function (e) {
    e.preventDefault();
    var qs = "RestaurantName=" + encodeURIComponent($("#RestaurantName").val() || "") +
        "&datefrom=" + encodeURIComponent($("#datefrom").val() || "") +
        "&dateto=" + encodeURIComponent($("#dateto").val() || "");
    downloadExcel("/Restaurant/GetReportResExcel?" + qs, "restaurant-report.xlsx");
});
