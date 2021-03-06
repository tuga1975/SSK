﻿function pushNoti(count) {
    api.countNoti = count;
    $('[notinumber]').text(api.countNoti > 0 ? api.countNoti : '');
}
var otherFunc = function () {
    alert(arguments.length); // Outputs: 10
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

function updateNRICTextValue(value) {
    $('#txtNRIC').val(value);
} 

function updateLoginNRICTextValue(value) {
    $('#loginNRIC').val(value);
    $('#loginPassword').focus();
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

window.addEventListener('message', function (event) {
    if (event.data.indexOf('ReturnGenerateImageMUBAndTTLabel;') > -1) {
        api.loading(false);
        var dataReturn = JSON.parse(event.data.split('ReturnGenerateImageMUBAndTTLabel;')[1]);
        api.server.CallPrintingMUBAndTT(dataReturn.action, dataReturn.json, dataReturn.canvas);
    }
}); 