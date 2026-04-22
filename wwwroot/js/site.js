
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

 
