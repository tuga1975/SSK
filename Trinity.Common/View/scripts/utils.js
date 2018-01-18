var api = new Api();
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
}
function AddContentPage(html, model) {

    if (model != null)
        api.model = JSON.parse(model);
    else
        api.model = null;
    $('#content').html('<div class="chi-content">' + html + '</div>');

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
});
