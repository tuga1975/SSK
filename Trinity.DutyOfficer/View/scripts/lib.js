var lstAlerts = [];

function activemenu(name) {
    $('[item-menu]').removeClass('active');
    $('[item-menu="' + name + '"]').addClass('active');
}

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

function getRealtimeNotificationServer(notification){
    var objNotify = JSON.parse(notification);
    if(objNotify){
        lstAlerts.push({
                    source: objNotify.Source,
                    type: objNotify.Type,
                    dateTime: objNotify.Date,
                    message: objNotify.Content
                });
    }
    var isLarge = lstAlerts.length >= 10;
    $(".notifyCount").text(isLarge ? "10+" : lstAlerts.length);
    $(".notifyCount").show();
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

function getActiveTab(){
    var activeTabNameNode = $("#menu span.active");
    if(activeTabNameNode){
        return activeTabNameNode[0].id;
    }
    return "";
}

