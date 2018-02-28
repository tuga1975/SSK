function pushNoti(count) {
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

function setAvatar(base64Str1, base64Str2) {
    if (base64Str1 != "") {
        $('#userPhoto1').attr('src', 'data:image/jpg;base64,' + base64Str1);
        $('#PopupUserPhoto1').css('background-image', 'url(data:image/jpg;base64,' + base64Str1 + ')');
    }
    if (base64Str2 != "") {
        $('#userPhoto2').attr('src', 'data:image/jpg;base64,' + base64Str2);
        $('#PopupUserPhoto2').css('background-image', 'url(data:image/jpg;base64,' + base64Str2 + ')');
    }
}