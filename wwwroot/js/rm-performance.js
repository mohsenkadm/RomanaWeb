/* ============================================================
   rm-performance.js
   Lightweight performance & UX helpers for Romana dashboard.
   - Debounce & throttle
   - Lazy image loading (IntersectionObserver)
   - AJAX caching wrapper
   - DOM batch writer (requestAnimationFrame)
   - Global loader overlay
   - Staggered reveal animations
   - Tooltip auto-init
   ============================================================ */
(function (global, $) {
    'use strict';

    var RM = global.RM || {};

    /* ---------- Debounce ---------- */
    RM.debounce = function (fn, wait) {
        var t;
        return function () {
            var ctx = this, args = arguments;
            clearTimeout(t);
            t = setTimeout(function () { fn.apply(ctx, args); }, wait || 250);
        };
    };

    /* ---------- Throttle ---------- */
    RM.throttle = function (fn, limit) {
        var inThrottle;
        return function () {
            var ctx = this, args = arguments;
            if (!inThrottle) {
                fn.apply(ctx, args);
                inThrottle = true;
                setTimeout(function () { inThrottle = false; }, limit || 200);
            }
        };
    };

    /* ---------- DOM batch writer ---------- */
    RM.batch = function (writes) {
        if (!Array.isArray(writes)) return;
        requestAnimationFrame(function () {
            for (var i = 0; i < writes.length; i++) {
                try { writes[i](); } catch (e) { /* ignore */ }
            }
        });
    };

    /* ---------- AJAX cache (GET only, short TTL) ---------- */
    var _cache = {};
    RM.cachedGet = function (url, data, ttlMs) {
        ttlMs = ttlMs || 15000;
        var key = url + '|' + JSON.stringify(data || {});
        var hit = _cache[key];
        var now = Date.now();
        if (hit && (now - hit.t) < ttlMs) {
            return $.Deferred().resolve(hit.v).promise();
        }
        return $.ajax({ url: url, type: 'GET', data: data }).then(function (v) {
            _cache[key] = { t: Date.now(), v: v };
            return v;
        });
    };
    RM.invalidateCache = function () { _cache = {}; };

    /* ---------- Lazy image loading ---------- */
    RM.initLazyImages = function () {
        if (!('IntersectionObserver' in global)) {
            document.querySelectorAll('img[data-src]').forEach(function (img) {
                img.src = img.dataset.src;
                img.classList.add('rm-loaded');
            });
            return;
        }
        var io = new IntersectionObserver(function (entries, obs) {
            entries.forEach(function (e) {
                if (e.isIntersecting) {
                    var img = e.target;
                    if (img.dataset.src) {
                        img.src = img.dataset.src;
                        img.addEventListener('load', function () { img.classList.add('rm-loaded'); }, { once: true });
                    }
                    obs.unobserve(img);
                }
            });
        }, { rootMargin: '80px' });
        document.querySelectorAll('img[data-src]').forEach(function (img) {
            img.classList.add('rm-lazy');
            io.observe(img);
        });
    };

    /* ---------- Staggered reveal on scroll ---------- */
    RM.initReveal = function () {
        if (!('IntersectionObserver' in global)) return;
        var io = new IntersectionObserver(function (entries, obs) {
            entries.forEach(function (e) {
                if (e.isIntersecting) {
                    e.target.style.animation = 'rmFadeInUp 0.45s ease-out both';
                    obs.unobserve(e.target);
                }
            });
        }, { threshold: 0.08 });
        document.querySelectorAll('[data-rm-reveal]').forEach(function (el) { io.observe(el); });
    };

    /* ---------- Loader overlay ---------- */
    RM.showLoader = function () {
        var o = document.getElementById('rmLoaderOverlay');
        if (!o) {
            o = document.createElement('div');
            o.id = 'rmLoaderOverlay';
            o.className = 'rm-loader-overlay';
            o.innerHTML = '<div class="rm-spinner" role="status" aria-label="Loading"></div>';
            document.body.appendChild(o);
        }
        o.classList.add('show');
    };
    RM.hideLoader = function () {
        var o = document.getElementById('rmLoaderOverlay');
        if (o) o.classList.remove('show');
    };

    /* ---------- Tooltip init ---------- */
    RM.initTooltips = function () {
        if ($ && $.fn && $.fn.tooltip) {
            $('[rel="tooltip"], [data-toggle="tooltip"]').tooltip({ trigger: 'hover', container: 'body' });
        }
    };

    /* ---------- AJAX global spinner (non-intrusive) ---------- */
    RM.bindGlobalAjax = function () {
        if (!$) return;
        var timer = null;
        $(document).ajaxStart(function () {
            timer = setTimeout(RM.showLoader, 180);
        }).ajaxStop(function () {
            clearTimeout(timer);
            RM.hideLoader();
        }).ajaxError(function () {
            clearTimeout(timer);
            RM.hideLoader();
        });
    };

    /* ---------- Smooth number count-up ---------- */
    RM.countUp = function (el, target, durationMs) {
        target = parseFloat(target) || 0;
        durationMs = durationMs || 900;
        var start = null, from = 0;
        function step(ts) {
            if (!start) start = ts;
            var p = Math.min(1, (ts - start) / durationMs);
            var eased = 1 - Math.pow(1 - p, 3);
            var val = Math.floor(from + (target - from) * eased);
            el.textContent = val.toLocaleString();
            if (p < 1) requestAnimationFrame(step);
            else el.textContent = target.toLocaleString();
        }
        requestAnimationFrame(step);
    };

    /* ---------- Auto boot ---------- */
    RM.boot = function () {
        RM.initLazyImages();
        RM.initReveal();
        RM.initTooltips();
        RM.bindGlobalAjax();
    };

    global.RM = RM;

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', RM.boot);
    } else {
        RM.boot();
    }
})(window, window.jQuery);
