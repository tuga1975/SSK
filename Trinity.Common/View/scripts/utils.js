﻿var api = new Api();
function Api() {
    this.Soure = '';
    this.countNoti = 0;
    this.model = null;
    this.server = {};
    this.countloading = 0;
    this.timeLoading = null;
    this.loading = function (status) {
        if (status) {
            clearTimeout(api.timeLoading);
            if (api.countloading == 0)
                $('.loading').css('display', 'block');
            api.countloading++;
        } else if (!status && api.countloading > 0) {
            api.countloading--;
            if (api.countloading == 0) {
                clearTimeout(api.timeLoading);
                api.timeLoading = setTimeout(function () {
                    $('.loading').css('display', 'none');
                }, 100);
            }
        }
    };
    this.callback = [];
    this.callback_this = [];
    this.Guid = function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    };
    var arrayReadrCallback = [];
    this.callReady = function (model) {
        for (var i = 0; i < arrayReadrCallback.length; i++) {
            arrayReadrCallback[i].call(this, model);
        }
        arrayReadrCallback = [];
    };
    this.ready = function (callback) {
        arrayReadrCallback.push(callback);
    };
    this.DumpDataToTxt = function (content, model) {
        for (var item in model) {
            var data = model[item];
            if (data != null && (item == 'Morning_Open_Time' || item == 'Morning_Close_Time' || item == 'Afternoon_Open_Time' || item == 'Afternoon_Close_Time' || item == 'Evening_Open_Time' || item == 'Evening_Close_Time')) {
                data = FormatTime(data);
            }
            content = content.replace(new RegExp('{' + item + '}', "g"), data == null ? '' : data);
        }
        return content;
    };
    this.ToDateCSharp = function (txtDate) {
        if (txtDate == null)
            return null;
        var _dateSplit = txtDate.split("/");
        if (_dateSplit.length != 3)
            return null;

        var _dateConvert = new Date(parseInt(_dateSplit[2]), parseInt(_dateSplit[1]) - 1, parseInt(_dateSplit[0]));
        if (_dateConvert == null || isNaN(_dateConvert.getTime())) {
            return null;
        }
        return _dateConvert.getFullYear() + '-' + (_dateConvert.getMonth() + 1) + '-' + _dateConvert.getDate();
    };
    function FormatTime(time) {
        var hourEnd = time.indexOf(":");
        var H = +time.substr(0, hourEnd);
        time = H + time.substr(hourEnd, 3);
        return time; // return adjusted time or original string
    };
}
function AddContentPage(html, model, file) {
    if (model != null)
        api.model = JSON.parse(model);
    else
        api.model = null;
    api.Soure = file;
    $('#content').html('<div class="chi-content">' + html + '</div>');
    api.callReady(api.model);
}
function AddContentPopup(html, model, id) {
    if (model != null)
        api.model = JSON.parse(model);
    else
        api.model = null;
    if (id == 'PopupMessage.html') {
        var divMessagepanel = $('#panel-popup > [id="' + id + '"]');
        if (divMessagepanel.length == 0) {
            divMessagepanel = $('<div id="' + id + '"></div>').appendTo($('#panel-popup'));
        }
        divMessagepanel.append(html);
    } else {
        if ($('#panel-popup > [id="' + id + '"]').length > 0)
            $('#panel-popup > [id="' + id + '"]').replaceWith(html);
        else
            $('#panel-popup').append(html);
    }
    api.callReady(api.model);
}
function createEvent(arrayFun) {
    arrayFun = JSON.parse(arrayFun);
    arrayFun.forEach(function (fun) {
        api.server[fun] = function () {
            var arg = [];
            var callback = null;
            var Guid = api.Guid();
            $.each(arguments || [], function (index, item) {
                if (typeof item == 'function')
                    callback = item;
                else
                    arg.push(item);
            });
            api.loading(true);
            if (callback != null) {
                api.callback_this[Guid] = this;
                api.callback[Guid] = callback;
                eval('window.external.ClientCallServer("' + fun + '","' + Guid + '"' + createArguments(arg) + ');');
            } else {
                eval('window.external.ClientCallServer("' + fun + '",null' + createArguments(arg) + ');');
            }
        }
    });
    api.callReady();
}
function callEventCallBack(guid, model) {
    var call = api.callback[guid];
    if (typeof item != 'undefined') {
        if (model != null)
            call.call(api.callback_this[guid], JSON.parse(model));
        else
            call.call(api.callback_this[guid]);
        delete api.callback_this[guid];
        delete api.callback[guid];
    }
}
function createArguments(arguments) {
    var array = Array.prototype.slice.call(arguments);
    var arrayNew = [];
    array.forEach(function (item) {
        if (typeof item == 'string')
            arrayNew.push(JSON.stringify(item));
        else
            arrayNew.push(item);
    });
    if (arrayNew.length > 0)
        return "," + arrayNew.join(",");
    return "";
}
function setLoading(status) {
    api.loading(status);
}
function RunScript(script) {
    try {
        eval(script)
    } catch (e) {
        api.server.LogErrScript(script);
    }
}
function alert(content) {
    ShowMessage('', content);
}
function ShowMessage(title, message, id) {
    id = id == null ? api.Guid() +'-front-end' : id;
    if (title != null && title.length > 0 && (message == null || (message != null && message.length == 0))) {
        message = title;
        title = '';
    }
    var struc = {
        title: title,
        message: message,
        id: id
    };
    api.server.LoadPopupHtml('PopupMessage.html', JSON.stringify(struc), function () {
        $('#panel-popup [popup-message="' + struc.id + '"]').modal({
            backdrop: 'static',
            keyboard: false
        });
    });
}
function ShowMessageConfirm(title, message,callback, id) {
    id = id == null ? '' : id;
    var struc = {
        title: title,
        message: message,
        id: id
    };
    api.server.LoadPopupHtml('PopupMessageConfirm.html', JSON.stringify(struc), function () {
        $('#PopupMessageConfirm').modal({
            backdrop: 'static',
            keyboard: false
        });
        if (typeof callback == 'function') {
            $('#PopupMessageConfirm button').unbind('click');
            $('#PopupMessageConfirm button').click(function () {
                var status = $(this).attr('btn-confirm');
                $('#PopupMessageConfirm').modal('toggle');
                callback((status == 'false' ? false : true));
            });
        }
    });
}
$(document).ready(function () {
    $('body').on('DOMNodeInserted', 'a', function () {
        if (typeof $(this).attr('isRegistration') == 'undefined') {
            $(this).attr('isRegistration', true);
            var value = $(this).attr('onclick');
            $(this).removeAttr('onclick');
            $(this).attr('valonclick', value);
            value = $(this).attr('href');
            value = typeof value == 'undefined' ? '' : value;
            if (value.indexOf('#') != 0) {
                $(this).attr('valhref', value);
                $(this).attr('href', 'javascript:;');
            }
        }
    });
    $('body').on('DOMNodeInserted', 'button[onclick]', function () {
        if (typeof $(this).attr('isRegistration') == 'undefined') {
            $(this).attr('isRegistration', true);
            var value = $(this).attr('onclick');
            $(this).removeAttr('onclick');
            $(this).attr('valonclick', value);
        }
    });

    $('body').on('click', 'a', function (event) {
        event.preventDefault();
        var href = $(this).attr('valhref');
        if (typeof href != 'undefined' && $.trim(href).length > 0) {
            try {
                eval('(function() { ' + href + ' })()');
            } catch (e) {

            }
        }
        try {
            if ($(this).is('[valonclick]')) {
                eval('(function() { ' + $(this).attr('valonclick') + ' })()');
            }
        } catch (e) {
        }
    });
    $('body').on('click', 'button', function (event) {
        event.preventDefault();
        try {
            if ($(this).is('[valonclick]')) {
                eval('(function() { ' + $(this).attr('valonclick') + ' })()');
            }
        } catch (e) {
        }

    });
});

var time_action_app = new Date().getTime();
window.addEventListener('load', function () {
    document.body.addEventListener('touchstart', function (e) {
        time_action_app = new Date().getTime();
    }, false);
    document.body.addEventListener('touchmove', function (e) {
        time_action_app = new Date().getTime();
    }, false);
    document.body.addEventListener('mousemove', function (e) {
        time_action_app = new Date().getTime();
    }, false);
    document.body.addEventListener('click', function (e) {
        time_action_app = new Date().getTime();
    }, false);
}, false);

function getTimeActionApp() {
    return time_action_app;
}

function getSource() {
    return api.Soure;
}
window.onerror = function (msg, url, line, col, error) {
    // Note that col & error are new to the HTML 5 spec and may not be 
    // supported in every browser.  It worked for me in Chrome.
    var extra = !col ? '' : '\ncolumn: ' + col;
    extra += !error ? '' : '\nerror: ' + error;
    // You can view the information in an alert to see things working like this:
    api.server.LogErrScript("Soure: " + api.Soure + "\nError: " + msg + "\nUrl: " + url + "\nLine: " + (line + extra));
    // TODO: Report this error via ajax so you can keep track
    //       of what pages have JS issues

    var suppressErrorAlert = true;
    // If you return true, then error alerts (like in older versions of 
    // Internet Explorer) will be suppressed.
    return suppressErrorAlert;
};
