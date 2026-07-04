/**
 * PDF §2.2 / §2.3.1 — اختيار زونات المطعم أو المندوب في لوحة التحكم.
 */
var ZonePicker = (function () {
    var allZones = [];
    var linksSummary = null;

    function zoneName(id) {
        var z = allZones.find(function (x) {
            return (x.zoneId || x.ZoneId) === id;
        });
        return z ? (z.name || z.Name) : ('#' + id);
    }

    function formatZoneLabels(ids) {
        ids = ids || [];
        if (!ids.length) return '<span class="text-muted">—</span>';
        return ids.map(function (id) {
            return '<span class="zone-tag">' + zoneName(id) + '</span>';
        }).join(' ');
    }

    function ensureZones(cb) {
        if (allZones.length) {
            if (cb) cb(allZones);
            return;
        }
        call_ajax('GET', 'zones/', null, function (data) {
            allZones = (data || []).filter(function (z) {
                return z.isActive !== false && z.IsActive !== false;
            });
            if (cb) cb(allZones);
        });
    }

    function loadSummary(cb) {
        if (linksSummary) {
            if (cb) cb(linksSummary);
            return;
        }
        call_ajax('GET', 'zones/links-summary', null, function (data) {
            linksSummary = data || { zones: [], byRestaurant: {}, bySaleMan: {} };
            allZones = linksSummary.zones || [];
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

        ensureZones(function (zones) {
            if (!zones.length) {
                $box.html('<p class="text-warning mb-0">لا توجد زونات — أضف زونات من <a href="/Home/Zones">إدارة المناطق</a></p>');
                return;
            }
            var html = '';
            zones.forEach(function (z) {
                var id = z.zoneId || z.ZoneId;
                var name = z.name || z.Name;
                var checked = selectedIds.indexOf(id) >= 0 ? ' checked' : '';
                html += '<label class="zone-picker-item">' +
                    '<input type="checkbox" class="zone-pick-cb" value="' + id + '"' + checked + '>' +
                    '<span>' + name + '</span></label>';
            });
            $box.html(html);
        });
    }

    function getSelected(containerId) {
        var ids = [];
        $('#' + containerId + ' .zone-pick-cb:checked').each(function () {
            ids.push(parseInt($(this).val(), 10));
        });
        return ids;
    }

    function loadRestaurant(restaurantId, containerId) {
        if (!restaurantId) {
            render(containerId, []);
            return;
        }
        call_ajax('GET', 'zones/restaurant/' + restaurantId, null, function (ids) {
            render(containerId, ids || []);
        });
    }

    function loadSaleMan(saleManId, containerId) {
        if (!saleManId) {
            render(containerId, []);
            return;
        }
        call_ajax('GET', 'zones/saleman/' + saleManId, null, function (ids) {
            render(containerId, ids || []);
        });
    }

    function saveRestaurant(restaurantId, containerId, cb) {
        if (!restaurantId) {
            if (cb) cb();
            return;
        }
        var ids = getSelected(containerId);
        call_ajax_json('PUT', 'zones/restaurant/' + restaurantId, ids, function () {
            invalidateSummary();
            if (cb) cb();
        });
    }

    function saveSaleMan(saleManId, containerId, cb) {
        if (!saleManId) {
            if (cb) cb();
            return;
        }
        var ids = getSelected(containerId);
        call_ajax_json('PUT', 'zones/saleman/' + saleManId, ids, function () {
            invalidateSummary();
            if (cb) cb();
        });
    }

    function suggestFromCoords(lat, lng, containerId) {
        var la = parseFloat(lat);
        var ln = parseFloat(lng);
        if (!la || !ln) return;
        call_ajax('GET', 'pricing/zones/resolve?lat=' + la + '&lng=' + ln, null, function (data) {
            if (!data || !data.inCoverage) return;
            var zid = data.zoneId;
            if (!zid) return;
            var $cb = $('#' + containerId + ' .zone-pick-cb[value="' + zid + '"]');
            if ($cb.length && $('#' + containerId + ' .zone-pick-cb:checked').length === 0) {
                $cb.prop('checked', true);
            }
        });
    }

    function restaurantZoneLabels(restaurantId) {
        if (!linksSummary || !linksSummary.byRestaurant) return '—';
        var ids = linksSummary.byRestaurant[restaurantId] || linksSummary.byRestaurant[String(restaurantId)] || [];
        return formatZoneLabels(ids);
    }

    function saleManZoneLabels(saleManId) {
        if (!linksSummary || !linksSummary.bySaleMan) return '—';
        var ids = linksSummary.bySaleMan[saleManId] || linksSummary.bySaleMan[String(saleManId)] || [];
        return formatZoneLabels(ids);
    }

    return {
        ensureZones: ensureZones,
        loadSummary: loadSummary,
        invalidateSummary: invalidateSummary,
        render: render,
        getSelected: getSelected,
        loadRestaurant: loadRestaurant,
        loadSaleMan: loadSaleMan,
        saveRestaurant: saveRestaurant,
        saveSaleMan: saveSaleMan,
        suggestFromCoords: suggestFromCoords,
        restaurantZoneLabels: restaurantZoneLabels,
        saleManZoneLabels: saleManZoneLabels
    };
})();
