/**
 * اختيار مطاعم المندوب في لوحة التحكم (RestaurantSaleMan — متعدد).
 */
var RestaurantPicker = (function () {
    var allRestaurants = [];
    var linksSummary = null;

    function restaurantName(id) {
        var r = allRestaurants.find(function (x) {
            return (x.restaurantId || x.RestaurantId) === id;
        });
        return r ? (r.name || r.Name) : ('#' + id);
    }

    function formatRestaurantLabels(ids) {
        ids = ids || [];
        if (!ids.length) return '<span class="sm-tag-empty">لا توجد مطاعم</span>';
        return ids.map(function (id) {
            return '<span class="restaurant-tag">' + restaurantName(id) + '</span>';
        }).join('');
    }

    function ensureRestaurants(cb) {
        if (allRestaurants.length) {
            if (cb) cb(allRestaurants);
            return;
        }
        call_ajax('GET', 'Restaurant/GetAll', null, function (data) {
            allRestaurants = (data || []).filter(function (r) {
                return r.isDelete !== true && r.IsDelete !== true;
            });
            if (cb) cb(allRestaurants);
        });
    }

    function loadSummary(cb) {
        if (linksSummary) {
            if (cb) cb(linksSummary);
            return;
        }
        call_ajax('GET', 'RestaurantSaleMan/links-summary', null, function (data) {
            linksSummary = data || { restaurants: [], bySaleMan: {}, byRestaurant: {} };
            allRestaurants = linksSummary.restaurants || [];
            if (cb) cb(linksSummary);
        });
    }

    function invalidateSummary() {
        linksSummary = null;
    }

    function render(containerId, selectedIds) {
        selectedIds = selectedIds || [];
        var $box = $('#' + containerId);
        if (!$box.length) return;

        ensureRestaurants(function (restaurants) {
            if (!restaurants.length) {
                $box.html('<p class="text-warning mb-0">لا توجد مطاعم — أضف مطاعم من <a href="/Home/Restaurant">إدارة المطاعم</a></p>');
                return;
            }
            var html = '';
            restaurants.forEach(function (r) {
                var id = r.restaurantId || r.RestaurantId;
                var name = r.name || r.Name || ('#' + id);
                var checked = selectedIds.indexOf(id) >= 0
                    || selectedIds.indexOf(String(id)) >= 0
                    || selectedIds.indexOf(Number(id)) >= 0 ? ' checked' : '';
                html += '<label class="zone-picker-item saleman-check-row">' +
                    '<input type="checkbox" class="restaurant-pick-cb" value="' + id + '"' + checked + '>' +
                    '<span class="saleman-check-name">' + name + '</span></label>';
            });
            $box.html(html);
        });
    }

    function getSelected(containerId) {
        var ids = [];
        $('#' + containerId + ' .restaurant-pick-cb:checked').each(function () {
            ids.push(parseInt($(this).val(), 10));
        });
        return ids;
    }

    function loadSaleMan(saleManId, containerId) {
        if (!saleManId) {
            render(containerId, []);
            return;
        }
        call_ajax('GET', 'RestaurantSaleMan/saleman/' + saleManId, null, function (ids) {
            render(containerId, ids || []);
        });
    }

    function saveSaleMan(saleManId, containerId, cb) {
        if (!saleManId) {
            if (cb) cb();
            return;
        }
        var ids = getSelected(containerId);
        call_ajax_json('PUT', 'RestaurantSaleMan/saleman/' + saleManId, ids, function () {
            invalidateSummary();
            if (cb) cb();
        });
    }

    function saleManRestaurantLabels(saleManId) {
        if (!linksSummary || !linksSummary.bySaleMan) return '—';
        var ids = linksSummary.bySaleMan[saleManId] || linksSummary.bySaleMan[String(saleManId)] || [];
        return formatRestaurantLabels(ids);
    }

    return {
        ensureRestaurants: ensureRestaurants,
        loadSummary: loadSummary,
        invalidateSummary: invalidateSummary,
        render: render,
        getSelected: getSelected,
        loadSaleMan: loadSaleMan,
        saveSaleMan: saveSaleMan,
        saleManRestaurantLabels: saleManRestaurantLabels
    };
})();
