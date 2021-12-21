(function ($) {
    function API(url) {
        this.getLabs = function () {
            return $.ajax({
                "url": url + "?command=get-labs",
            });
        };

        this.getTools = function (labId) {
            return $.ajax({
                "url": url + "?command=get-tools&labId=" + labId,
            });
        };

        this.insertReservation = function (args) {
            return $.ajax({
                "url": url + "?command=make-group",
                "method": "POST",
                "contentType": "application/json",
                "data": JSON.stringify(args)
            });
        };

        this.updateReservation = function (args) {
            return $.ajax({
                "url": url + "?command=modify-group",
                "method": "POST",
                "contentType": "application/json",
                "data": JSON.stringify(args)
            });
        };

        this.deleteReservation = function (groupId) {
            return $.ajax({
                "url": url + "?command=delete-group&groupId=" + groupId,
            });
        };

        this.getGroups = function () {
            return $.ajax({
                "url": url + "?command=get-groups",
            });
        };

        this.getGroup = function (groupId) {
            return $.ajax({
                "url": url + "?command=get-group&groupId=" + groupId,
            });
        };
    }

    $.fn.facilityDowntime = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opts = $.extend({ "ajaxUrl": "ajax", "date": dayjs().format("YYYY-MM-DD"), "clientId": 0 }, options, $this.data());

            function toggleCheckAll() {
                var checked = $(".check-all", $this).prop("checked") === true;
                $('.tools option:visible', $this).prop("selected", checked);
            }

            var api = new API(opts.ajaxUrl);

            var loadLabs = function () {
                var def = $.Deferred();

                api.getLabs().done(function (data) {
                    $(".labs", $this).html("");

                    $.each(data, function (_, lab) {
                        $(".labs", $this).append($("<option/>", { "value": lab.LabID }).html(lab.LabDisplayName))
                    });

                    def.resolve();
                }).fail(function (jqXHR) {
                    showAlert(jqXHR.responseJSON.message, "danger");
                    def.reject(jqXHR.responseJSON);
                });

                return def.promise();
            };

            var fillToolsList = function (data) {
                $(".tools", $this).html("");

                if (data) {
                    $.each(data, function (_, tool) {
                        $(".tools", $this).append($("<option/>", { "value": tool.ResourceID, "data-proctech-id": tool.ProcessTechID, "data-lab-id": tool.LabID }).html(tool.ResourceDisplayName))
                    });
                }
            };

            var loadTools = function () {
                var def = $.Deferred();

                var labId = $(".labs", $this).val();

                api.getTools(labId).done(function (data) {
                    fillToolsList(data);
                    def.resolve();
                }).fail(function (jqXHR) {
                    showAlert(jqXHR.responseJSON.message, "danger");
                    def.reject(jqXHR.responseJSON);
                });

                return def.promise();
            }

            var searchTools = function (search) {
                if (search) {
                    $(".tools option", $this).each(function () {
                        var option = $(this);
                        if (option.text().toLowerCase().search(search) >= 0)
                            option.show();
                        else
                            option.hide();
                    })
                } else {
                    $(".tools option", $this).show();
                }
            };

            var loadHoursSelect = function (select, d) {
                select.html("");

                var hour = d.hour();
                var hr;

                if (hour == 0)
                    hr = 12;
                else if (hour > 12)
                    hr = hour - 12;
                else
                    hr = hour;

                for (x = 1; x < 13; x++) {
                    select.append($("<option/>", { "value": x }).html(x).prop("selected", hr == x));
                }
            };

            var loadMinutesSelect = function (select, d) {
                select.html("");
                for (x = 0; x < 60; x++) {
                    select.append($("<option/>", { "value": x }).html(("0" + x).substr(-2)).prop("selected", d.minute() == x));
                }
            };

            var loadAmPmSelect = function (select, d) {
                select.html("");
                select.append($("<option/>", { "value": "AM" }).html("AM").prop("selected", d.hour() < 12));
                select.append($("<option/>", { "value": "PM" }).html("PM").prop("selected", d.hour() >= 12));
            };

            var getDateTime = function (target) {
                var sd = dayjs($(".date-text", target).datepicker("getDate"));

                var a = $(".ampm-select", target).val();

                var h = (a == "AM")
                    ? parseInt($(".hour-select", target).val()) - 1
                    : parseInt($(".hour-select", target).val()) + 12;

                var m = $(".min-select", target).val();

                var result = sd.add(h, "hours").add(m, "minutes");

                return result;
            };

            var fillGroupsTable = function (data) {
                var now = dayjs();

                var getDeleteButton = function (x) {
                    var ed = dayjs(x.EndDateTime);
                    if (now.isSame(ed) || now.isBefore(ed)) {
                        return $("<a/>", { "href": "#", "class": "group-delete-button", "data-toggle": "modal", "data-target": ".delete-confirmation-dialog", "data-group-id": x.GroupID }).html(
                            $("<img/>", { "src": "images/im_delete.gif" })
                        );
                    }
                };

                var getEditButton = function (x) {
                    return $("<a/>", { "href": "#", "class": "group-edit-button", "data-group-id": x.GroupID }).html(
                        $("<img/>", { "src": "images/im_edit.gif" })
                    );
                };

                $(".groups-table tbody", $this).html($.map(data, function (x) {
                    return $("<tr/>")
                        .append($("<td/>").html(x.GroupID))
                        .append($("<td/>").html(x.DisplayName))
                        .append($("<td/>").html(dayjs(x.BeginDateTime).format("MM/DD/YYYY hh:mm A")))
                        .append($("<td/>").html(dayjs(x.EndDateTime).format("MM/DD/YYYY hh:mm A")))
                        .append($("<td/>").css("text-align", "center").html(getDeleteButton(x)))
                        .append($("<td/>").css("text-align", "center").html(getEditButton(x)));
                }));
            };

            var loadGroups = function () {
                var def = $.Deferred();

                api.getGroups().done(function (data) {
                    fillGroupsTable(data);
                    def.resolve();
                }).fail(function (jqXHR) {
                    showAlert(jqXHR.responseJSON.message, "danger");
                    def.reject(jqXHR.responseJSON);
                });

                return def.promise();
            };

            var showAlert = function (msg, type) {
                var alert = $(".alert-dismissible", $this);

                if (msg) {
                    alert.removeClass().addClass("alert alert-dismissible alert-" + type)
                    $(".alert-text", alert).html(msg)
                    alert.show();
                } else {
                    $(".alert-text", alert).html("");
                    alert.hide();
                }
            };

            var selectTab = function (index) {
                $("#tabs1 li", $this).eq(index).find("a").tab("show");
            };

            var modifyReservation = function (groupId) {
                var def = $.Deferred();

                showAlert();

                var sd = getDateTime($(".start-date", $this));
                var ed = getDateTime($(".end-date", $this));
                var notes = $(".notes", $this).val();

                api.updateReservation({
                    "clientId": opts.clientId,
                    "groupId": groupId,
                    "start": sd.format("YYYY-MM-DD HH:mm:ss"),
                    "end": ed.format("YYYY-MM-DD HH:mm:ss"),
                    "notes": notes
                }).done(function (data) {
                    console.log(data);
                    showAlert(data.message, "success");
                    selectTab(1);
                    def.resolve();
                }).fail(function (jqXHR) {
                    showAlert(jqXHR.responseJSON.message, "danger");
                    def.reject(jqXHR.responseJSON);
                });

                return def.promise();
            };

            var makeReservation = function () {
                var def = $.Deferred();

                showAlert();

                var tools = $.map($(".tools option:selected", $this), function (option) {
                    return $(option).val();
                });

                if (tools.length > 0) {
                    var sd = getDateTime($(".start-date", $this));
                    var ed = getDateTime($(".end-date", $this));
                    var notes = $(".notes", $this).val();

                    api.insertReservation({
                        "clientId": opts.clientId,
                        "tools": tools,
                        "start": sd.format("YYYY-MM-DD HH:mm:ss"),
                        "end": ed.format("YYYY-MM-DD HH:mm:ss"),
                        "notes": notes
                    }).done(function (data) {
                        showAlert(data.message, "success");
                        selectTab(1);
                        def.resolve();
                    }).fail(function (jqXHR) {
                        showAlert(jqXHR.responseJSON.message, "danger");
                        def.reject(jqXHR.responseJSON);
                    });
                } else {
                    def.reject({ "message": "No tools were selected." });
                }

                return def.promise();
            };

            var deleteReservation = function (groupId) {
                var def = $.Deferred();

                showAlert();

                api.deleteReservation(groupId).done(function (data) {
                    showAlert("You have successfully deleted a Facility Down Time reservation. [GroupID: " + groupId + "]", "success");
                    fillGroupsTable(data);
                    def.resolve();
                }).fail(function (jqXHR) {
                    showAlert(jqXHR.responseJSON.message, "danger");
                    def.reject(jqXHR.responseJSON);
                });

                return def.promise();
            };

            var cancelEdit = function () {
                $(".modify-reservation", $this).removeData("group-id");
                $(".created-by .display-name", $this).html("");
                $(".new-reservation-tab", $this).html("New Reservation");
                loadDefaultDateRange();
                $(".new-reservation-tabpanel", $this).removeClass("editing");

                loadTools().done(function () {
                    searchTools($(".search").val().toLowerCase());
                    selectTab(1);
                });
            };

            var loadEdit = function (groupId) {
                var def = $.Deferred();

                showAlert();

                api.getGroup(groupId).done(function (data) {
                    $(".modify-reservation", $this).data("group-id", groupId);
                    $(".created-by .display-name", $this).html(data.DisplayName);
                    $(".new-reservation-tab", $this).html("Edit Reservation");
                    loadStartDateTime(dayjs(data.BeginDateTime));
                    loadEndDateTime(dayjs(data.EndDateTime));
                    $(".new-reservation-tabpanel", $this).addClass("editing");
                    fillToolsList(data.Reservations);
                    selectTab(0);
                    def.resolve();
                }).fail(function (jqXHR) {
                    showAlert(jqXHR.responseJSON.message, "danger");
                    def.reject(jqXHR.responseJSON);
                });

                return def.promise();
            };

            var loadDefaultDateRange = function () {
                var now = dayjs();
                var h = now.hour();
                var m = now.minute();

                //var date = dayjs(opts.date);
                var date = dayjs(now.format("YYYY-MM-DD"));
                var sd = date.add(h, "hours").add(m, "minutes");
                var ed = date.add(1, "days").add(h, "hours").add(m, "minutes");

                loadStartDateTime(sd);
                loadEndDateTime(ed);
            };

            var loadStartDateTime = function (d) {
                $(".start-date .date-text", $this).datepicker("update", d.format("MM/DD/YYYY"));
                loadHoursSelect($(".start-date .hour-select", $this), d);
                loadMinutesSelect($(".start-date .min-select", $this), d);
                loadAmPmSelect($(".start-date .ampm-select", $this), d);
            };

            var loadEndDateTime = function (d) {
                $(".end-date .date-text", $this).datepicker("update", d.format("MM/DD/YYYY"));
                loadHoursSelect($(".end-date .hour-select", $this), d);
                loadMinutesSelect($(".end-date .min-select", $this), d);
                loadAmPmSelect($(".end-date .ampm-select", $this), d);
            };

            $this.on("change", ".labs", function (e) {
                $(".search", $this).val("");
                loadTools();
            }).on("show.bs.tab", '.tabs a[data-toggle="tab"]', function (e) {
                var target = $(e.target);
                if (target.hasClass("manage-reservations-tab")) {
                    loadGroups().always(function () {
                        if ($(".new-reservation-tabpanel", $this).hasClass("editing"))
                            cancelEdit();
                    });
                }
            }).on("show.bs.modal", ".delete-confirmation-dialog", function (e) {
                var button = $(e.relatedTarget);
                var groupId = button.data("group-id");
                var modal = $(this);
                $('.group-id', modal).text(groupId);
                $(".delete-group-confirm-button", modal).data("group-id", groupId);
            }).on("keyup", ".search", function () {
                searchTools($(this).val().toLowerCase());
            }).on("change", ".check-all", function () {
                toggleCheckAll();
            }).on("click", ".make-reservation", function () {
                makeReservation();
            }).on("click", ".delete-group-confirm-button", function () {
                var btn = $(this);
                var groupId = btn.data("group-id");
                deleteReservation(groupId).always(function () {
                    $(".delete-confirmation-dialog", $this).modal("hide");
                });
            }).on("click", ".group-edit-button", function (e) {
                e.preventDefault();
                var btn = $(this);
                var groupId = btn.data("group-id");
                loadEdit(groupId);
            }).on("click", ".modify-reservation", function () {
                var btn = $(this);
                var groupId = btn.data("group-id");
                modifyReservation(groupId);
            }).on("click", ".cancel-modify-reservation", function () {
                cancelEdit();
            });

            loadLabs().done(function () {
                loadTools().done(function () {
                    $(".bs-datepicker", $this).datepicker({ "autoclose": true });
                    loadDefaultDateRange();
                    $(".tabs", $this).show();
                });
            });
        });
    };
})(jQuery);