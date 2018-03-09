﻿var api = new Api();
function Api() {
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
                data = tConvert24hrTo12hr(data);
            }
            content = content.replace(new RegExp('{' + item + '}', "g"), data == null ? '' : data);
        }
        return content;
    };
    function tConvert24hrTo12hr(time) {
        var hourEnd = time.indexOf(":");
        var H = +time.substr(0, hourEnd);
        var h = H % 12 || 12;
        var ampm = (H < 12 || H === 24) ? " AM" : " PM";
        time = h + time.substr(hourEnd, 3) + ampm;
        return time; // return adjusted time or original string
    };
}
function AddContentPage(html, model) {
    if (model != null)
        api.model = JSON.parse(model);
    else
        api.model = null;
    $('#content').html('<div class="chi-content">' + html + '</div>');
    $('#content button[onclick]').each(function () {
        var value = $(this).attr('onclick');
        $(this).removeAttr('onclick');
        $(this).attr('valonclick', value);
    });
    api.callReady(api.model);
}
function AddContentPopup(html, model, id) {
    if (model != null)
        api.model = JSON.parse(model);
    else
        api.model = null;
    if ($('#panel-popup > [id="' + id + '"]').length > 0)
        $('#panel-popup > [id="' + id + '"]').replaceWith(html);
    else
        $('#panel-popup').append(html);

    $('#panel-popup > [id="' + id + '"]').find('button[onclick]').each(function () {
        var value = $(this).attr('onclick');
        $(this).removeAttr('onclick');
        $(this).attr('valonclick', value);
    });
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
    eval(script);
}
function ShowMessageBox(title, message,id) {
    api.server.ShowPopupMessage(title, message, id, function () {
        $('#PopupMessage').modal({
            backdrop: 'static',
            keyboard: false
        });
    });
}
$(document).ready(function () {
    $('body').on('click', 'a[href]', function (event) {
        event.preventDefault();
        var href = $(this).attr('href');
        if (typeof href != 'undefined' && $.trim(href).length > 0) {
            try {
                eval(href);
            } catch (e) {

            }
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
