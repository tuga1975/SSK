function editFontSize(status) {
    var px = parseInt($('#content').css('font-size').replace('px', ''));
    if (status) {
        px++;
    } else if (!status && px > 0) {
        px--;
    }
    $('#content').css('font-size', px);
}
function PopupMessage(title, body) {
    api.server.PopupMessage(title, body, function () {
        $('#PopupMessage').modal({
            backdrop: 'static',
            keyboard: false
        });
    });
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

//function displayNRICLogin() {
//    $('.col-sm-3').hide();
//    $('#NRIC-login').show();
//}

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

function getToday() {
    var today = new Date();
    var dd = today.getDate();
    var MM = today.getMonth() + 1; //January is 0!
    var yyyy = today.getFullYear();
    var hh = today.getHours();
    var mm = today.getMinutes();

    if (dd < 10) {
        dd = '0' + dd
    }

    if (mm < 10) {
        mm = '0' + mm
    }

    today = dd + '/' + MM + '/' + yyyy + '  ' + hh + ':' + mm;
    return today;
}
function refreshQueueNumbers(servingQueueNumber, currentQueueNumber, nextQueueNumberList, holdingList) {

    var today = getToday();
    $('#currentDateTime').text(today);
    if (servingQueueNumber.trim().length > 0) {
        var split = servingQueueNumber.split('-');

        var queue = "";
        for (var i = 0; i < split.length; i++) {
            if (split[i].length > 0) {

                queue += "<li class='box-child'><span style='margin-top:-8px' ><i>" + (i + 1) + "</i><b>" + split[i] + "</b></span></li>";

            }

            $('[add-time-NowServing]').html(queue);
        }

    }
    else {
        $('[add-time-NowServing]').html('');
    }
    if (currentQueueNumber.trim().length > 0) {
        var split = currentQueueNumber.split('-');

        var queue = "";
        for (var i = 0; i < split.length; i++) {
            if (split[i].length > 0) {

                queue += "<li class='box-child'><span style='margin-top:-8px' ><i>" + (i + 1) + "</i><b>" + split[i] + "</b></span></li>";

            }

            $('[add-time-CurrentTimeslot]').html(queue);
        }

    } else {
        $('[add-time-CurrentTimeslot]').text('');
    }
    var nextQueueNumbers;
    if (nextQueueNumberList != null)
        nextQueueNumbers = JSON.parse(nextQueueNumberList);
    else
        nextQueueNumbers = null;

    $('[add-time-NextTimeslot] span').remove();
    if (nextQueueNumbers != null && nextQueueNumbers.length > 0) {

        for (var i = 0; i < nextQueueNumbers.length; i++) {
            $('[add-time-NextTimeslot]').append('<li class="box-child" > <span style="margin-top:-8px"><i>' + (i + 1) + '</i><b>' + nextQueueNumbers[i] + '</b></span></li >');
        }
    }
    var holdingQueueNumbers;
    if (holdingList != null) {
        holdingQueueNumbers = JSON.parse(holdingList);
    }
    else { holdingQueueNumbers = null; }

    $('[add-time-HoldingList] span').remove();
    if (holdingQueueNumbers != null && holdingQueueNumbers.length > 0) {

        for (var i = 0; i < holdingQueueNumbers.length; i++) {
            $('[add-time-HoldingList]').append('<li class="box-child" > <span style="margin-top:-8px"><i>' + (i + 1) + '</i><b>' + holdingQueueNumbers[i] + '</b></span></li >');
        }
    }

}

function setTimeslot(currentTimeslot, nextTimeslot) {
    $('#currentTimeslot').text(currentTimeslot);
    $('#nextTimeslot').text(nextTimeslot);
}

function showMessage(message) {
    alert(message);
}