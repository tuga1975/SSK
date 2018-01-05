var api = {
    countNoti: 0,
    model: null,
    server: {},
    countloading: 0,
    timeLoading: null,
    loading: function (status) {
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
    },
    callback: [],
    callback_this: [],
    Guid: function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
}

function AddContentPage(html, model) {
    if (model != null)
        api.model = JSON.parse(model);
    else
        api.model = null;
    var content = $('#content');
    content.html('<div class="chi-content">' + html + '</div>');
}
function pushNoti(count) {
    api.countNoti = count;
    $('[notinumber]').text(api.countNoti > 0 ? api.countNoti : '');
}
var otherFunc = function () {
    alert(arguments.length); // Outputs: 10
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

function updateNRICTextValue(value) {
    $('#txtNRIC').text(value);
}

function displayNRICLogin() {
    $('.col-sm-3').hide();
    $('#NRIC-login').show();
}

function setNRICFocus() {
    $('#txtNRIC').focus();
}

function displayLogoutButton(display) {
    if (display == false) {
        $('#btn-logout').hide();
    }
    else {
        $('#btn-logout').show();
    }
}

function refreshQueueNumbers(currentQueueNumber, nextQueueNumberList) {
    if (currentQueueNumber) {
        $('[show-queue]').text(currentQueueNumber);
    } else {
        $('[show-queue]').text('');
    }
    var nextQueueNumbers;
    if (nextQueueNumberList != null)
        nextQueueNumbers = JSON.parse(nextQueueNumberList);
    else
        nextQueueNumbers = null;

    $('[show-queue-list] span').remove();
    if (nextQueueNumbers != null && nextQueueNumbers.length > 0) {
        for (var i = 0; i < nextQueueNumbers.length; i++) {
            $('[show-queue-list]').append('<span>' + nextQueueNumbers[i] + '</span>');
        }
    }
}


function base64ArrayBuffer(arrayBuffer) {
    var base64 = ''
    var encodings = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/'

    var bytes = new Uint8Array(arrayBuffer)
    var byteLength = bytes.byteLength
    var byteRemainder = byteLength % 3
    var mainLength = byteLength - byteRemainder

    var a, b, c, d
    var chunk

    // Main loop deals with bytes in chunks of 3
    for (var i = 0; i < mainLength; i = i + 3) {
        // Combine the three bytes into a single integer
        chunk = (bytes[i] << 16) | (bytes[i + 1] << 8) | bytes[i + 2]

        // Use bitmasks to extract 6-bit segments from the triplet
        a = (chunk & 16515072) >> 18 // 16515072 = (2^6 - 1) << 18
        b = (chunk & 258048) >> 12 // 258048   = (2^6 - 1) << 12
        c = (chunk & 4032) >> 6 // 4032     = (2^6 - 1) << 6
        d = chunk & 63               // 63       = 2^6 - 1

        // Convert the raw binary segments to the appropriate ASCII encoding
        base64 += encodings[a] + encodings[b] + encodings[c] + encodings[d]
    }

    // Deal with the remaining bytes and padding
    if (byteRemainder == 1) {
        chunk = bytes[mainLength]

        a = (chunk & 252) >> 2 // 252 = (2^6 - 1) << 2

        // Set the 4 least significant bits to zero
        b = (chunk & 3) << 4 // 3   = 2^2 - 1

        base64 += encodings[a] + encodings[b] + '=='
    } else if (byteRemainder == 2) {
        chunk = (bytes[mainLength] << 8) | bytes[mainLength + 1]

        a = (chunk & 64512) >> 10 // 64512 = (2^6 - 1) << 10
        b = (chunk & 1008) >> 4 // 1008  = (2^6 - 1) << 4

        // Set the 2 least significant bits to zero
        c = (chunk & 15) << 2 // 15    = 2^4 - 1

        base64 += encodings[a] + encodings[b] + encodings[c] + '='
    }

    return base64
}