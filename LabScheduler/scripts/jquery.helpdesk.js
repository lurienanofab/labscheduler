(function ($) {
    $.fn.helpdesk = function (options) {
        return this.each(function () {
            var $this = $(this);

            var options = {
                "ajaxUrl": $(".ajax-url", $this).val(),
                "queue": $(".helpdesk-queue", $this).val(),
                "resource": $.parseJSON($(".helpdesk-resource", $this).val()),
                "fromEmail": $(".helpdesk-from-email", $this).val(),
                "fromName": $(".helpdesk-from-name", $this).val(),
            };

            var output = $('.tickets', $this);

            $(".ticket-dialog", $this).find(".helpdesk-frame").attr("frameborder", 0);

            $(".ticket-dialog", $this).on("show.bs.modal", function (e) {
                var button = $(e.relatedTarget);
                var ticketId = button.data("ticketid");
                var url = '/ostclient/ticket.html?ticketID=' + ticketId + "&source=Scheduler";
                $(this).find(".helpdesk-frame").attr("src", url);
            });

            var displayMessage = function (tickets) {
                if (tickets && tickets.length && tickets.length > 0) {
                    output.html("");
                    $.each(tickets, function (index, value) {
                        var ticket = $('<div class="helpdesk-ticket"><table><tbody></tbody></table></div>');

                        $('<tr/>').append(
                            '<td class="helpdesk-label" style="width: 100px;">TicketID:</td>'
                        ).append(
                            $('<td/>').append(
                                $("<a/>", { "href": "#", "data-ticketid": value.ticketID, "data-toggle": "modal", "data-target": ".ticket-dialog" }).html(value.ticketID)
                            )
                        ).appendTo($('tbody', ticket));

                        $('<tr/>').append(
                            $('<td class="helpdesk-label">Created On:</td>')
                        ).append(
                            $('<td/>').html(format_date(value.created))
                        ).appendTo($('tbody', ticket));

                        $('<tr/>').append(
                            $('<td class="helpdesk-label">Created By:</td>')
                        ).append(
                            $('<td/>').html(value.email)
                        ).appendTo($('tbody', ticket));

                        $('<tr/>').append(
                            $('<td class="helpdesk-label">Assigned To:</td>')
                        ).append(
                            $('<td/>').html((value.assigned_to == null) ? '<span class="nodata">[Unassigned]</span>' : value.assigned_to)
                        ).appendTo($('tbody', ticket));

                        $('<tr/>').append(
                            $('<td class="helpdesk-label">Subject:</td>')
                        ).append(
                            $('<td/>').html(value.subject)
                        ).appendTo($('tbody', ticket));

                        output.append(ticket);
                    });
                }
                else
                    output.html('<span class="nodata">There are no open tickets at this time.</span>');
            };

            var outputTickets = function (tickets) {
                displayMessage(tickets);
            };

            var outputError = function (err) {
                output.html($("<div/>").html(err).css({ "color": "#ff0000" }));
            };

            var createTicketError = function (msg, callback) {
                $('.create-ticket-message', $this).html($('<div/>').css({ "margin-top": "5px", "color": "#ff0000" }).html(msg));
                if (typeof callback == 'function') callback();
                return false;
            }

            var TicketPriorty = {
                GeneralQuestion: 1,
                ProcessIssue: 2,
                HardwareIssue: 3
            }

            var sendEmailForHardwareIssue = function (pri, subject, message) {
                if (pri == TicketPriorty.HardwareIssue) {
                    $.ajax({
                        url: "ajax/helpdesk.ashx",
                        type: "POST",
                        data: {
                            "command": "send-hardware-issue-email",
                            "subject": subject,
                            "message": message,
                            "resourceId": options.resource.id
                        },
                        dataType: "json",
                        success: function (json) {
                            console.log(json);
                        },
                        error: function (err) {
                            console.log(err);
                        }
                    })
                }
            }

            var getTicketPriorty = function (ticketType) {
                pri = TicketPriorty.GeneralQuestion;
                switch (ticketType) {
                    case "General Question":
                        pri = TicketPriorty.GeneralQuestion;
                        break;
                    case "Hardware Issue":
                        pri = TicketPriorty.HardwareIssue;
                        break;
                    case "Process Issue":
                        pri = TicketPriorty.ProcessIssue;
                        break;
                }
                return pri;
            }

            var getMessageHeader = function (resource, client, reservationText, ticketType) {
                var result = "Resource ID: " + resource.id + "\r\n"
                    + "Resource Name: " + resource.name + "\r\n"
                    + "Created By: " + client + "\r\n"
                    + "Reservation: " + reservationText + "\r\n"
                    + "Type: " + ticketType;
                return result;
            }

            var getMessageBody = function (resource, reservationId, client, reservationText, messageText, ticketType) {
                var result = getMessageHeader(resource, client, reservationText, ticketType) + "\r\n";
                var host = window.location.protocol + "//" + window.location.host;
                if (reservationId)
                    result += "Reservation History: " + host + "/scheduler/history/" + reservationId + "\r\n";
                result += "--------------------------------------------------" + "\r\n";
                result += messageText + "\r\n";
                return result;
            }

            var createTicket = function (callback) {
                if (typeof callback != "function")
                    callback = function () { console.log("createTicket"); };

                var subject = $(".subject", $this).val();
                if (subject == "")
                    return createTicketError("Please enter a subject.", callback);
                subject = "[" + options.resource.id + ":" + options.resource.name + "] " + subject;

                var typeOption = $(".type", $this).find("option:selected");
                var type = typeOption.text();
                var pri = getTicketPriorty(type);

                var message = $(".message", $this).val();
                if (message == "")
                    return createTicketError("Please enter a message.", callback);
                var reservationOption = $(".reservations", $this).find("option:selected");
                var reservationId = parseInt(reservationOption.val());
                var reservationText = reservationOption.text();
                message = getMessageBody(options.resource, reservationId, options.fromName, reservationText, message, type);

                $.ajax({
                    url: options.ajaxUrl,
                    type: 'POST',
                    data: {
                        "command": "add-ticket",
                        "resource_id": options.resource.id,
                        "email": options.fromEmail,
                        "name": options.fromName,
                        "queue": options.queue,
                        "subject": subject,
                        "message": message,
                        "pri": pri,
                        "search": "by-resource"
                    },
                    dataType: 'json',
                    success: function (json) {
                        if (json.error)
                            createTicketError(json.message);
                        else {
                            $('.create-ticket-message', $this).html($('<div/>').css({ "margin-top": "5px", "color": "#003366", "font-weight": "bold" }).html("Ticket created!"));
                            if (json.tickets) {
                                sendEmailForHardwareIssue(pri, subject, message);
                                outputTickets(json.tickets);
                            }
                            else
                                output.html('<span class="nodata">There are no open tickets at this time.</span>');
                        }
                    },
                    error: function (err) {
                        createTicketError(err)
                    },
                    complete: function () {
                        callback();
                    }
                });
            };

            var disable = false;

            if (options.ajaxUrl != "") {
                $.ajax({
                    'url': options.ajaxUrl,
                    'type': 'POST',
                    'data': { 'command': 'select-tickets-by-resource', 'resource_id': options.resource.id },
                    'dataType': 'json',
                    'success': function (json) {
                        if (json.tickets)
                            outputTickets(json.tickets);
                        else
                            output.html('<span class="nodata">There are no open tickets at this time.</span>');
                    },
                    'error': function (err) {
                        outputError(err);
                    }
                });
                disabled = options.queue == "";
            }
            else {
                disable = true;
                outputError("The helpdesk URL was not provided.");
            }

            var createTicketButton = $(".create-ticket", $this);

            if (disable)
                createTicketButton.addClass("disabled");

            $(".create-ticket", $this)
                .on("click", function (e) {
                    e.preventDefault();
                    var button = $(this);
                    if (!button.hasClass("disabled")) {
                        button.addClass("disabled");
                        createTicket(function () {
                            button.removeClass("disabled");
                        });
                    }
                });
        });
    }

    function format_date(text) {
        var splitter = text.split(' ');
        var date_part = splitter[0];
        var time_part = splitter[1];

        splitter = date_part.split('-');
        var yy = splitter[0];
        var mm = splitter[1];
        var dd = splitter[2];

        splitter = time_part.split(':');
        var hh = splitter[0];
        var mi = splitter[1];
        var ss = splitter[2];
        var ampm = ' AM';

        if (parseInt(hh) == 12) {
            ampm = ' PM';
        }
        else if (parseInt(hh) > 12) {
            hh = (parseInt(hh) - 12) + '';
            ampm = ' PM';
        }
        else
            hh = parseInt(hh) + '';

        return mm + '/' + dd + '/' + yy + ' ' + hh + ':' + mi + ':' + ss + ampm;
    }
}(jQuery));