//javascript: window.history.forward(1);

//needs to be done here for IE
function handleMissingImage(img) {
    $(img).unbind('error').css({ 'display': 'none' });
}

function EC(TheTR, img) {
    var DataTR = eval('document.all.' + TheTR);
    if (DataTR.style.display == "block" || DataTR.style.display == "") {
        DataTR.style.display = "none";
        img.children[1].children[0].src = 'images/arr_down.gif';
    }
    else {
        DataTR.style.display = "block";
        img.children[1].children[0].src = 'images/arr_up.gif';
    }
}

var DebugUtility = {
    TimeTakenText: function (sd, ed, desc, units) {
        var diff = ed.getTime() - sd.getTime();
        var value = 0;

        if (typeof units == 'undefined')
            units = 'ms';

        switch (units) {
            case 'seconds':
                value = diff / 1000;
                break;
            case 'days':
                value = diff / 1000 / 60 / 60 / 24;
                break;
            case 'hours':
                value = diff / 1000 / 60 / 60;
                break;
            case 'minutes':
                value = diff / 1000 / 60;
                break;
            default:
                value = diff;
                units = 'ms';
                break;
        }

        return $('<div/>').addClass('debug-info')
            .append(
                $('<div/>').addClass('debug-info-label')
                    .html(desc)
            ).append(
                $('<div/>').addClass('debug-info-value')
                    .html(value.toFixed(3) + ' ' + units)
            ).append(
                $('<div/>').css({ 'clear': 'both' })
            );
    }
}

var startTime = new Date();

$('.ReservStartOrDelete').click(function () {
    $('body').css('pointer-events', 'none');
});

$('.treeview').treeview({ autoscroll: false });

$('.calendar-feed-url').focus(function (event) {
    $(this).select();
}).mouseup(function (event) {
    return false;
});

$('.ical-title-info-link').click(function (event) {
    event.preventDefault();
    var $this = $(this);
    var parent = $this.parents('.ical-container');
    if ($this.html() == 'More Info') {
        $this.html('Less Info');
        $('.ical-message', parent).show();
    }
    else {
        $this.html('More Info');
        $('.ical-message', parent).hide();
    }
});

$('.helpdesk-info').helpdeskinfo();
$('.helpdesk').helpdesk();
$('.interlock-state').control();
$('.numeric-text').numerictext();

if ($('.show-debug-info').val() == 'true') {
    $('form').append(
        DebugUtility.TimeTakenText(startTime, new Date(), 'JavaScript execution time')
    );
}

function disable(sender) {
    $(sender).prop('disabled', true);
}

// ------------------- Clientside Validations ------------------
var noErrors = true;

function updateErrorMsg(errMsg) {
    $('#divErrMsg').html($('#divErrMsg').html() + "<br/> &bull; " + errMsg);
    errors = false;
}

function resValidate() {
    errors = true;
    $('#divErrMsg').html("");

    //-----Traning Auth-----
    //var checkedCount = $('#divInviteeAuths').find('input[type="checkbox"]:checked').length;

    //var authChecked = checkedCount > 0;

    //if (authChecked == false) {
    //    //$('#divErrMsg').text("* Atleast one 'Trainer Authorization' should be selected");
    //    updateErrorMsg("At least one Trainer Authorization should be selected.");
    //}

    //-----Max Alloc-----
    var maxReservation = parseInt($(".max-reservation").val()); // Max Reservation Time  
    var maxSchedulable = parseInt($(".max-schedulable").val()); // Max Schedulable Hours
    var fence = parseInt($(".fence").val());                    // Reservation Fence

    if (maxSchedulable < maxReservation) {
        updateErrorMsg("Max Schedulable Hours (" + maxSchedulable + ") cannot be less than Max Reservation Time (" + maxReservation + ").");
    }
    if (maxSchedulable > fence) {
        updateErrorMsg("Max Schedulable Hours (" + maxSchedulable + ") cannot be greater than Reservation Fence. (" + fence + ")");
    }

    return errors;
}