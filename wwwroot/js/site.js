
const baseUrl = "/"; 

function call_ajax_withfile(method, url, object, call_back_func) {
    var userToken = getCookie("token2");
    mouseevent("progress");
    $('.progress').fadeIn();
    $.ajax({
        method: method, contentType: false, processData: false,
        url: baseUrl + url,
        cache: true, async: false,
        data: object,
        headers: {
            'Authorization': `Bearer ${userToken}`,
        },
        success: (respons) => {
            //   /  debugger;
            mouseevent("default");
            $('.preloader').fadeOut();
            if (respons.success) {
                if (typeof (call_back_func) == 'function') {
                    if (respons.data != undefined)
                        call_back_func(respons.data);
                    else
                        call_back_func();
                }
                else if (typeof (call_back_func) == 'string'
                    && call_back_func == 'return') {
                    return respons.data;
                }
                if (respons.msg != null && respons.msg != undefined)
                    md.showNotification(respons.msg);
            }
            else {
                if (respons.msg != null && respons.msg != undefined)
                    md.showNotification(respons.msg);
            }

        },
        error: (e) => {
            mouseevent("default");
            $('.progress').fadeOut();
            md.showNotification('حدث خطأ عند الأتصال');
        }
    });
}

function call_ajax(method, url, object, call_back_func) {
    var userToken = getCookie("token2");
    mouseevent("progress");
    $('.progress').fadeIn();
    $.ajax({
        method: method,
        url: "/" + url,
        cache: true, async: true,
        data: object,
        headers: {
            'Authorization': `Bearer ${userToken}`,
        },
        success: (respons) => {
            //   /  debugger;
            mouseevent("default");
            $('.preloader').fadeOut();
            if (respons.success) {
                if (typeof (call_back_func) == 'function') {
                    if (respons.data != undefined)
                        call_back_func(respons.data);
                    else
                        call_back_func();
                }
                else if (typeof (call_back_func) == 'string'
                    && call_back_func == 'return') {
                    return respons.data;
                }
                if (respons.msg != null && respons.msg != undefined) 
                md.showNotification(respons.msg);
            }
            else {
                if (respons.msg != null && respons.msg != undefined) 
                    md.showNotification(respons.msg);
            }

        },
        error: (e) => {
            mouseevent("default");
            $('.progress').fadeOut();
            md.showNotification('حدث خطأ عند الأتصال');
        }
    });
} 

function call_ajax_json(method, url, object, call_back_func) {
    var userToken = getCookie("token2");
    mouseevent("progress");
    $('.progress').fadeIn();
    $.ajax({
        method: method,
        url: "/" + url,
        contentType: "application/json",
        data: JSON.stringify(object),
        headers: {
            'Authorization': `Bearer ${userToken}`,
        },
        success: (respons) => {
            mouseevent("default");
            $('.preloader').fadeOut();
            if (respons.success) {
                if (typeof (call_back_func) == 'function') {
                    if (respons.data != undefined)
                        call_back_func(respons.data);
                    else
                        call_back_func();
                }
                if (respons.msg != null && respons.msg != undefined)
                    md.showNotification(respons.msg);
            } else {
                if (respons.msg != null && respons.msg != undefined)
                    md.showNotification(respons.msg);
            }
        },
        error: (e) => {
            mouseevent("default");
            $('.progress').fadeOut();
            md.showNotification('حدث خطأ عند الأتصال');
        }
    });
}

var indexviews = 1;
var listviews = [
    { view: 'Home/Login', index: 1 },
];
function NextNavicationViews() {
    window.history.back();
}
function PrevNavicationViews() {
    window.history.forward();
}

function call_Action(url, fromnav = 0) {
    var userToken = getCookie("token2");
    // Token/cookie security check - redirect to login if no token for protected pages
    var publicPages = ['home/login', 'home/loginforres', 'do'];
    var isPublic = publicPages.some(p => url.toLowerCase().indexOf(p) >= 0);
    if (!isPublic && (!userToken || userToken.trim() === '')) {
        window.location.href = '/Home/Login';
        return;
    }
    // Real URL navigation instead of AJAX partial loading
    window.location.href = '/' + url;
}
function mouseevent(type) {
    $("body").css("cursor", type);
    //type =progress ,default
}
function setCookie(cname, cvalue, exdays) {
    //debugger;
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}
function getCookie(cname) {
    //  debugger;
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

/** Authenticated Excel download (Bearer from cookie token2) — replaces window.open. */
function downloadExcel(url, fallbackFileName) {
    var userToken = typeof getCookie === 'function' ? getCookie('token2') : '';
    if (!url) return;
    if (url.charAt(0) !== '/') url = '/' + url;
    fallbackFileName = fallbackFileName || 'export.xlsx';

    if (typeof mouseevent === 'function') mouseevent('progress');
    if (window.$) $('.progress').fadeIn();

    fetch(url, {
        method: 'GET',
        headers: { 'Authorization': 'Bearer ' + userToken }
    })
        .then(function (res) {
            var ctype = (res.headers.get('Content-Type') || '').toLowerCase();
            if (!res.ok || ctype.indexOf('application/json') >= 0) {
                return res.text().then(function (text) {
                    var msg = 'فشل تصدير الاكسل — تأكد من تسجيل الدخول';
                    try {
                        var j = JSON.parse(text);
                        if (j && j.msg) msg = j.msg;
                    } catch (e) { }
                    throw new Error(msg);
                });
            }
            var disposition = res.headers.get('Content-Disposition') || '';
            var fileName = fallbackFileName;
            var match = /filename\*?=(?:UTF-8''|")?([^\";]+)/i.exec(disposition);
            if (match && match[1]) fileName = decodeURIComponent(match[1].replace(/['"]/g, ''));
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
        .catch(function (err) {
            var msg = (err && err.message) ? err.message : 'فشل تصدير الاكسل';
            if (typeof md !== 'undefined' && md.showNotification) md.showNotification(msg);
            else alert(msg);
        })
        .finally(function () {
            if (typeof mouseevent === 'function') mouseevent('default');
            if (window.$) $('.progress').fadeOut();
        });
}


