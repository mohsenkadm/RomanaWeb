// Section 5.2: admin Ratings page. Filters the existing /Stars + /DriverStars endpoints
// and the new /analytics/ratings endpoint, then renders 4 charts and a table.
(function () {
    if (typeof Chart === 'undefined') return;

    var charts = {};
    function destroy(n) { if (charts[n]) { charts[n].destroy(); charts[n] = null; } }
    function token() { try { return localStorage.getItem('Token') || sessionStorage.getItem('Token') || ''; } catch (e) { return ''; } }
    function get(u) {
        return $.ajax({
            url: u, type: 'GET',
            beforeSend: function (x) { var t = token(); if (t) x.setRequestHeader('Authorization', 'Bearer ' + t); }
        });
    }
    function isoToday() { return new Date().toISOString().slice(0, 10); }
    function isoDaysAgo(n) { var d = new Date(); d.setDate(d.getDate() - n); return d.toISOString().slice(0, 10); }

    function bar(id, labels, values, label, color) {
        destroy(id);
        charts[id] = new Chart(document.getElementById(id), {
            type: 'bar',
            data: { labels: labels, datasets: [{ label: label, data: values, backgroundColor: color }] },
            options: { responsive: true, scales: { y: { beginAtZero: true, max: 5 } } }
        });
    }
    function line(id, labels, values, label, color) {
        destroy(id);
        charts[id] = new Chart(document.getElementById(id), {
            type: 'line',
            data: { labels: labels, datasets: [{ label: label, data: values, borderColor: color, fill: false }] },
            options: { responsive: true }
        });
    }
    function pie(id, labels, values, colors) {
        destroy(id);
        charts[id] = new Chart(document.getElementById(id), {
            type: 'pie',
            data: { labels: labels, datasets: [{ data: values, backgroundColor: colors }] },
            options: { responsive: true }
        });
    }

    function passesFilter(r, minStars, maxStars, hasComment) {
        var s = r.starsCount || r.StarsCount || 0;
        if (s < minStars || s > maxStars) return false;
        var c = (r.comments || r.Comments || '').trim();
        if (hasComment === 'yes' && !c) return false;
        if (hasComment === 'no' && c) return false;
        return true;
    }

    function loadTable(minStars, maxStars, hasComment) {
        $('#ratingsTableBody').empty();
        $.when(get('/Stars/GetAll?index=0'), get('/DriverStars/GetAll'))
            .done(function (storeRes, driverRes) {
                var store = ((storeRes[0] && storeRes[0].data) || []).filter(function (r) { return passesFilter(r, minStars, maxStars, hasComment); });
                var driver = ((driverRes[0] && driverRes[0].data) || []).filter(function (r) { return passesFilter(r, minStars, maxStars, hasComment); });

                store.forEach(function (r) {
                    $('#ratingsTableBody').append(
                        '<tr><td>متجر</td><td>' + (r.restaurantName || r.RestaurantName || ('#' + (r.restaurantId || r.RestaurantId))) +
                        '</td><td>' + (r.starsCount || r.StarsCount) + '</td><td>' + (r.comments || r.Comments || '-') +
                        '</td><td>' + (r.orderId || r.OrderId || '-') + '</td></tr>');
                });
                driver.forEach(function (r) {
                    $('#ratingsTableBody').append(
                        '<tr><td>سائق</td><td>' + (r.saleManName || r.SaleManName || ('#' + (r.saleManId || r.SaleManId))) +
                        '</td><td>' + (r.starsCount || r.StarsCount) + '</td><td>' + (r.comments || r.Comments || '-') +
                        '</td><td>' + (r.orderId || r.OrderId || '-') + '</td></tr>');
                });
            });
    }

    function refresh() {
        var from = $('#ratFrom').val(), to = $('#ratTo').val();
        var min = parseInt($('#ratMinStars').val(), 10) || 0;
        var max = parseInt($('#ratMaxStars').val(), 10) || 5;
        var hc = $('#ratHasComment').val() || 'any';
        if (!from || !to) return;
        var qs = '?from=' + from + '&to=' + to;

        get('/analytics/ratings' + qs).done(function (r) {
            var d = (r && r.data) || {};
            var perStore = d.perStore || [];
            var perDriver = d.perDriver || [];
            var dist = d.distribution || [];

            bar('chartAvgStore', perStore.map(x => '#' + x.restaurantId), perStore.map(x => Math.round(x.avg * 10) / 10), 'متوسط النجوم', '#4CAF50');
            bar('chartAvgDriver', perDriver.map(x => '#' + x.saleManId), perDriver.map(x => Math.round(x.avg * 10) / 10), 'متوسط النجوم', '#2196F3');
            pie('chartStarDistribution',
                dist.map(x => x.stars + ' ★'),
                dist.map(x => x.count),
                ['#F44336', '#FF9800', '#FFC107', '#8BC34A', '#4CAF50', '#9E9E9E']);

            // Volume over time = sum of ratings counts per day; we don't have exact
            // dates without an extra endpoint, so plot the per-store rating volumes.
            line('chartRatingsVolume', perStore.map(x => '#' + x.restaurantId), perStore.map(x => x.count), 'عدد التقييمات', '#3F51B5');
        });

        loadTable(min, max, hc);
    }

    $(function () {
        if ($('#ratFrom').length === 0) return;
        $('#ratFrom').val(isoDaysAgo(30));
        $('#ratTo').val(isoToday());
        $('#ratRefresh').on('click', refresh);
        refresh();
    });
})();
