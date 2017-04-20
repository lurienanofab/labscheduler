(function ($) {
    $.fn.reservationHistory = function (options) {
        return this.each(function () {
            var $this = $(this);

            $('.save-button', $this).on("click", function (event) {
                $('.message', $this).html('');
                $('.controls input', $this).prop('disabled', true);

                var opt = $.extend({}, { "url": null, "clientId": 0 }, options, $this.data());

                var getReservationId = function () {
                    var input = $(".reservation-id", $this);
                    if (input.length > 0)
                        return input.val();
                    else
                        return null;
                }

                var getReservationNotes = function () {
                    var input = $(".reservation-notes", $this);
                    if (input.length > 0)
                        return input.val();
                    else
                        return null;
                }

                var getReservationAccountId = function () {
                    var input = $(".reservation-account-id", $this);
                    if (input.length > 0)
                        return input.val();
                    else
                        return null;
                }

                var getReservationForgiven = function () {
                    var input = $(".reservation-forgiven-percentage", $this);
                    if (input.length > 0)
                        return input.val();
                    else
                        return null;
                }

                var getEmailClient = function () {
                    var input = $(".email-client input[type='checkbox']", $this);
                    if (input.length > 0)
                        return input.is(":checked");
                    else
                        return null;
                }

                var getSelectedClientId = function () {
                    var input = $(".selected-client-id", $this);
                    if (input.length > 0)
                        return input.val();
                    else
                        return null;
                }

                var getStartDate = function () {
                    var input = $(".start-date", $this);
                    if (input.length > 0)
                        return input.val();
                    else
                        return null;
                }

                var getEndDate = function () {
                    var input = $(".end-date", $this);
                    if (input.length > 0)
                        return input.val();
                    else
                        return null;
                }

                var getClientId = function () {
                    // the ClientID of the user making the change
                    return opt.clientId;
                }

                var id = getReservationId();
                var accountId = getReservationAccountId();
                var forgivenPct = getReservationForgiven();
                var emailClient = getEmailClient();
                var notes = getReservationNotes();
                var clientId = getClientId();

                $(".working-history", $this).show();

                // if either of these aren't null we need to update billing (forgivenPct might be zero) 
                var updateBilling = accountId !== null || forgivenPct !== null;

                if (updateBilling)
                    $(".working-billing", $this).show();

                $.ajax({
                    "url": opt.url + '/' + id + '/save-history',
                    "type": "POST",
                    "data": {
                        "ClientID": clientId,
                        "Notes": notes,
                        "AccountID": accountId,
                        "ForgivenPercentage": forgivenPct,
                        "EmailClient": emailClient
                    }
                }).done(function (data, textStatus, jqXHR) {
                    if (data) {
                        if (forgivenPct) {
                            $(".forgiven-percentage", $this).html(parseFloat(forgivenPct).toFixed(1) + "%");
                            $(".reservation-forgiven-percentage", $this).val("");
                        }

                        if (updateBilling) {
                            $.ajax({
                                "url": opt.url + '/update-billing',
                                "type": "POST",
                                "data": {
                                    "ClientID": getSelectedClientId(),
                                    "StartDate": getStartDate(),
                                    "EndDate": getEndDate()
                                }
                            }).done(function (data, textStatus, jqXHR) {
                                if (!data)
                                    $(".message", $this).append($("<div/>").css({ "color": "#ff0000", "font-weight": "bold" }).html("Update failed!"));
                            }).fail(function (jqXHR, textStatus, errorThrown) {
                                var errmsg = jqXHR.responseJSON.ExceptionMessage;
                                $(".message", $this).append($("<div/>").css({ "color": "#ff0000", "font-weight": "bold" }).html(errmsg));
                            }).always(function () {
                                $(".working-billing", $this).hide();
                            });
                        }
                    }
                    else {
                        $(".message", $this).append($("<div/>").css({ "color": "#ff0000", "font-weight": "bold" }).html("Update failed!"));
                    }
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    var errmsg = jqXHR.responseJSON.ExceptionMessage;
                    $(".message", $this).append($("<div/>").css({ "color": "#ff0000", "font-weight": "bold" }).html(errmsg));
                }).always(function () {
                    $(".working-history", $this).hide();
                    $('.controls input', $this).prop('disabled', false);
                });
            });
        });
    };
}(jQuery));