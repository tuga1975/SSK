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

function failAlert(message) {
    alert(message);
}
function setSearchText(searchText) {
    $('#nric').val(searchText);
}


function getSource() {
    return JSON.stringify({
        soure: api.Soure,
        step: typeof stepNewEnrol == 'undefined' ? 0 : stepNewEnrol,
        printsuccsess: typeof printSuccsess == 'undefined' ? false : printSuccsess,
    });
}

window.addEventListener('message', function (event) {
    if (event.data.indexOf('ReturnSmartCard;') > -1) {
        var dataReturn = JSON.parse(event.data.split('ReturnSmartCard;')[1]);
        if (dataReturn.Soure == 'ReprintSmartCard') {
            GenerateImageSmartCardSuccess(dataReturn.Front, dataReturn.Back);
        } else if (dataReturn.Soure == 'UpdateSuperviseeBiodata') {
            GenerateImageSmartCardSuccess(dataReturn.Front, dataReturn.Back);
        } else if (dataReturn.Soure == 'UpdateSuperviseeBiodata_Reprint'){
            GenerateImageSmartCardReprintSuccess(dataReturn.Front, dataReturn.Back);
        }
    }
}); 