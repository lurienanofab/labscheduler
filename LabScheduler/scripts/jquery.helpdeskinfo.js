(function ($) {
    $.fn.helpdeskinfo = function () {
        return this.each(function () {
            var $this = $(this);

            var options = {
                'url': $('.helpdesk-info-url', $this).val(),
                'multitool': $('.helpdesk-info-multitool', $this).val() == "true",
                'resources': $.parseJSON($('.helpdesk-info-resources', $this).val())
            };

            var output = $('.helpdesk-info-output', $this);

            var filterByResource = function (resource, items) {
                var result = [];
                $.each(items, function (index, value) {
                    if (value.resource_id == resource.id)
                        result.push(value);
                });
                return result;
            }

            var toolinfo = function (v, u, n) {
                var self = this;
                this.data = v;
                this.urgent = filterByResource(v, u);
                this.normal = filterByResource(v, n);
                this.count = function (items) {
                    var result = 0;
                    $.each(items, function (x, item) {
                        var c = parseInt(item.ticket_count);
                        if (!isNaN(c)) result += c;
                    });
                    return result;
                };
                this.ticketCount = function () {
                    return self.count(self.normal) + self.count(self.urgent);
                };
                this.hasTickets = function () {
                    return self.ticketCount() > 0;
                };
                this.openTicketsMessage = function () {
                    var ticketCount = self.ticketCount();
                    var result = ticketCount + ' open ticket';
                    if (ticketCount > 1) result += 's';
                    return result;
                };
                this.urgentMessage = function () {
                    var result = '';
                    var urgentCount = self.count(self.urgent);
                    if (urgentCount > 0) {
                        var msg = urgentCount + ' hardware issue' + (urgentCount > 1 ? 's' : '');
                        result += '<span style="color: #ff0000;">&nbsp;(' + msg + ', availability may be limited)</span>';
                    }
                    return result;
                };
            }

            var displayMessage = function (urgent, normal) {
                var message = null;
                if (urgent.length > 0 || normal.length > 0) {
                    if (options.multitool) {
                        message = $('<div/>').addClass('multitool');
                        message.append('<div style="padding: 5px 5px 0 5px;"><strong>Helpdesk:</strong></div>');
                        $.each(options.resources, function (index, value) {
                            var tool = new toolinfo(value, urgent, normal);
                            if (tool.hasTickets()) {
                                message.append($('<div class="helpdesk-info-warning" style="padding-left: 15px;"/>')
                                     .append('&bull;&nbsp;')
                                     .append(value.name)
                                     .append(': ')
                                     .append(tool.openTicketsMessage())
                                     .append(tool.urgentMessage()));
                            }
                            //else
                            //    message.append($('<div class="helpdesk-info-message">There are no open tickets at this time.</div>'));
                        });
                    }
                    else {
                        message = $('<div/>').addClass('singletool');
                        $.each(options.resources, function (index, value) {
                            var tool = new toolinfo(value, urgent, normal);
                            if (tool.hasTickets()) {
                                message.append($('<div class="helpdesk-info-warning"/>')
                                     .append('Helpdesk: ')
                                     .append(tool.openTicketsMessage())
                                     .append(tool.urgentMessage()));
                            }
                            else
                                message = $('<div class="helpdesk-info-message">There are no open tickets at this time.</div>');
                        });

                        /*var urgentCount = 0;//tool.count(tool.urgent);
                        var normalCount = 2;//tool.count(tool.normal);
                        if (urgentCount > 0)
                            message = $('<div class="helpdesk-info-urgent">There is an open high priority helpdesk ticket for this resource. Availability may be limited. Please click on the Helpdesk tab for more information.</div>');
                        else {
                            if (normalCount == 1)
                                message = $('<div class="helpdesk-info-warning">There is ' + normalCount + ' open helpdesk ticket.</div>');
                            else
                                message = $('<div class="helpdesk-info-warning">There are ' + normalCount + ' open helpdesk tickets.</div>');
                        }*/
                    }
                }
                else
                    message = $('<div class="helpdesk-info-message">There are no open tickets at this time.</div>');

                output.html('').append(message);
            };

            function find(id, resources) {
                var result = -1;
                $.each(resources, function (index, value) {
                    if (value.id == id) {
                        result = index;
                        return false;
                    }
                });
                return result;
            };

            var outputSummary = function (summary) {
                var urgent = [];
                var normal = [];
                if ($.isArray(summary) && summary.length > 0) {
                    $.each(summary, function (x, item) {
                        var r = find(parseInt(item.resource_id), options.resources);
                        if (r >= 0) {
                            item.resource = options.resources[r];
                            var urgency = parseInt(item.priority_urgency);
                            if (!isNaN(urgency)) {
                                if (urgency <= 20)
                                    urgent.push(item);
                                else
                                    normal.push(item);
                            }
                        }
                    });
                }
                displayMessage(urgent, normal);
            };

            var outputError = function (err) {
                output.html('').append(
                    $('<div/>').html(err).css({ 'color': '#ff0000' })
                );
            };

            var getResourceList = function () {
                var idlist = [];
                $.each(options.resources, function (i, r) {
                    idlist.push(r.id);
                });
                return idlist.join(",");
            }

            if (options.url != '') {
                if (options.resources.length > 0) {
                    $.ajax({
                        'url': options.url,
                        'type': 'POST',
                        'data': { 'command': 'summary', 'resources': getResourceList() },
                        'dataType': 'json',
                        'success': function (json) {
                            if (!json.error)
                                outputSummary(json.summary);
                            else
                                outputError(json.message);
                        },
                        'error': function (err) {
                            outputError(err);
                        }
                    });
                }
                else
                    $this.hide();
            }
            else
                outputError('The helpdesk URL was not provided.');

        });
    }
}(jQuery));