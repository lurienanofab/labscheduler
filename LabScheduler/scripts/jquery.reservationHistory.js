/*
  Copyright 2017 University of Michigan

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
*/

(function ($) {
    $.fn.reservationHistory = function (options) {
        return this.each(function () {
            var $this = $(this);

            $('.save-button', $this).on("click", function (event) {
                $('.message', $this).html('');
                $('.controls input', $this).prop('disabled', true);

                var opt = $.extend({}, { "url": null }, options, $this.data());

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

                var id = getReservationId();
                var accountId = getReservationAccountId();
                var forgivenPct = getReservationForgiven();
                var emailClient = getEmailClient();
                var notes = getReservationNotes();

                $(".working-history", $this).show();

                // if either of these aren't null we need to update billing (forgivenPct might be zero) 
                var updateBilling = accountId !== null || forgivenPct !== null;

                if (updateBilling)
                    $(".working-billing", $this).show();

                $.ajax({
                    "url": opt.url + '/' + id + '/save-history',
                    "type": "POST",
                    "data": {
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