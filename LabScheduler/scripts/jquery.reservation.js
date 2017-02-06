var debug = null;

(function ($) {
    $.fn.dataTableExt.afnSortData['dom-checkbox'] = function (oSettings, iColumn) {
        return $.map(oSettings.oApi._fnGetTrNodes(oSettings), function (tr, i) {
            return $('td:eq(' + iColumn + ') input[type="checkbox"]:first', tr).prop('checked') ? 1 : 0;
        });
    };

    var renderCheckboxColumn = function (key, disabled, source, type, val) {
        if (type === 'display') {
            var v = new String(source[key]).toLowerCase();
            var checked = v === "true" || v === "1" || v === "yes" || v === "on";
            return '<div style="text-align: center;"><input type="checkbox"' + (checked ? " checked" : "") + (disabled ? " disabled" : "") + ' />';
        }
        else if (type === 'set') {
            source[key] = val;
            return;
        }
        else
            return source[key];
    };

    $.fn.reservation = function (opt) {
        return this.each(function () {
            var $this = $(this);

            var getClientId = function () {
                return $(".client-id", $this).val();
            };

            var formatDate = function (d, format) {
                if (!d) return "";

                var minDate = new Date("1/1/0001 12:00:00 AM");

                var date = new Date(d);

                if (isNaN(date.getTime()) || date.getTime() === minDate.getTime())
                    return "";

                var result = format;

                result = result.replace("yyyy", date.getFullYear());
                result = result.replace("MM", new String(date.getMonth() + 1 + 100).substr(1));
                result = result.replace("M", new String(date.getMonth() + 1));
                result = result.replace("dd", new String(date.getDate() + 100).substr(1));
                result = result.replace("d", new String(date.getDate()));

                var hours = date.getHours();
                var hr, ampm;

                if (hours === 0) {
                    hr = 12;
                    ampm = "AM";
                }
                else if (hours < 12) {
                    hr = hours;
                    ampm = "AM";
                }
                else if (hours === 12) {
                    hr = hours;
                    ampm = "PM";
                }
                else {
                    hr = hours - 12;
                    ampm = "PM";
                }

                result = result.replace("HH", new String(hours + 100).substr(1));
                result = result.replace("H", new String(hours));
                result = result.replace("hh", new String(hr + 100).substr(1));
                result = result.replace("h", new String(hr));
                result = result.replace("mm", new String(date.getMinutes() + 100).substr(1));
                result = result.replace("ss", new String(date.getSeconds() + 100).substr(1));
                result = result.replace("TT", ampm.toUpperCase());
                result = result.replace("tt", ampm.toLowerCase());

                return result;
            };

            if (opt === 'recurring') {

                var setParam = function (param, value) {
                    if (param.is("input[type='radio']"))
                        param.prop("checked", param.val() === value);
                    else if (param.is("select"))
                        param.find("option[value='" + value + "']").prop("selected", true);
                };

                var getParam = function (params) {
                    if (params.length === 0)
                        return 0;

                    if (params.length > 1)
                        return params.filter(":checked").first().val();

                    return params.val();
                };

                var deleteRecurrence = function (id, callback) {
                    $.ajax({
                        "url": "/api/scheduler/reservation-recurrence?id=" + id,
                        "type": "DELETE",
                        "complete": function (jqXHR, textStatus) {
                            callback();
                        }
                    });
                };

                var getRecurrenceDetail = function (id, callback) {
                    $.ajax({
                        "url": "/webapi/scheduler/reservation-recurrence/single?id=" + id,
                        "type": "GET",
                        "success": function (data, textStatus, jqXHR) {
                            callback(data);
                        },
                        "error": function (jqXHR, textStatus, errorThrown) {
                            alert(textStatus);
                        }
                    });
                };

                var getRecurrence = function (detail) {
                    var result = {
                        RecurrenceID: detail.data("id"),
                        BeginTime: $(".begin-time", detail).val(),
                        EndTime: $(".end-time", detail).val(),
                        AutoEnd: $(".auto-end", detail).prop("checked"),
                        KeepAlive: $(".keep-alive", detail).prop("checked"),
                        Notes: $(".notes", detail).val(),
                        PatternID: $(".recurrence-pattern:checked", detail).val(),
                        PatternParam1: 0,
                        PatternParam2: 0,
                        EndDate: null
                    };

                    var name = $(".recurrence-pattern:checked", detail).data("name");
                    result.PatternParam1 = getParam($(".pattern." + name + " .pattern-param1", detail));
                    result.PatternParam2 = getParam($(".pattern." + name + " .pattern-param2", detail));

                    if ($(".range-end-date-option", detail).is(":checked"))
                        result.EndDate = $(".range-end-date", detail).val();

                    return result;
                };

                var saveRecurrence = function (data, callback) {
                    var result = { success: true, message: "" };

                    $.ajax({
                        "url": "/api/scheduler/reservation-recurrence",
                        "type": "PATCH",
                        "data": JSON.stringify(data),
                        "contentType": "application/json",
                        "error": function (jqXHR, textStatus, errorThrown) {
                            result.success = false;
                            result.message = textStatus;
                        },
                        "complete": function (jqXHR, textStatus) {
                            callback(result);
                        }
                    });
                };

                var tbl = $(".recurring-reservations-table", $this).dataTable({
                    "bProcessing": true,
                    "sPaginationType": "full_numbers",
                    "sAjaxSource": "/api/scheduler/reservation-recurrence/datatables?IsActive=true&ClientID=" + getClientId(),
                    "aoColumnDefs": [
                        { "aTargets": [0], "sWidth": "150px", "mData": "ResourceName" },
                        {
                            "aTargets": [1], "sWidth": "80px", "mData": "BeginDate", "mRender": function (data) {
                                return formatDate(data, "M/d/yyyy");
                            }
                        },
                        {
                            "aTargets": [2], "sWidth": "80px", "mData": "EndDate", "mRender": function (data) {
                                return formatDate(data, "M/d/yyyy");
                            }
                        },
                        {
                            "aTargets": [3], "sWidth": "80px", "mData": "BeginTime", "mRender": function (data) {
                                return formatDate(data, "h:mm tt");
                            }
                        },
                        {
                            "aTargets": [4], "sWidth": "80px", "mData": "EndTime", "mRender": function (data) {
                                return formatDate(data, "h:mm tt");
                            }
                        },
                        { "aTargets": [5], "sWidth": "90px", "mData": "PatternName" },
                        {
                            "aTargets": [6], "sWidth": "80px", "bSortable": false, "bSearchable": false, "mData": null, "mRender": function (data, type, full) {
                                return '<div class="row-controls">'
                                    + '<a href="UserRecurringReservationEdit2.aspx?id=' + full.RecurrenceID + '" class="edit"><img src="//ssel-apps.eecs.umich.edu/static/images/edit.png" /></a> | '
                                    + '<a href="#" class="delete" data-id="' + full.RecurrenceID + '"><img src="//ssel-apps.eecs.umich.edu/static/images/delete.png" /></a>'
                                    + '</div>';
                            }
                        }
                    ]
                });

                $this.on("click", ".delete", function (e) {
                    e.preventDefault();
                    var id = $(this).data("id");
                    deleteRecurrence(id, function () {
                        //tbl.fnReloadAjax();
                    });
                });

                $(".detail", $this).each(function () {
                    var detail = $(this);
                    var id = detail.data("id");

                    getRecurrenceDetail(id, function (data) {

                        $(".resource-name", detail).html($("<a/>").attr("href", $(".resource-url", detail).val().replace("%id", data.ResourceID)).html(data.ResourceName));
                        $(".begin-time", detail).val(formatDate(data.BeginTime, "h:mm TT"));
                        $(".end-time", detail).val(formatDate(data.EndTime, "h:mm TT"));
                        $(".auto-end", detail).prop("checked", data.AutoEnd);
                        $(".keep-alive", detail).prop("checked", data.KeepAlive);
                        $(".notes", detail).val(data.Notes);

                        var name = data.PatternName.toLowerCase().replace(" ", "-");
                        $(".recurrence-pattern", detail).prop("checked", false);
                        $(".recurrence-pattern[data-name='" + name + "']", detail).prop("checked", true);
                        $(".pattern", detail).hide();
                        $(".pattern", detail).hide();

                        var pattern = $(".pattern." + name, detail);
                        pattern.show();

                        $(".pattern-param1", pattern).each(function () {
                            setParam($(this), data.PatternParam1);
                        });

                        $(".pattern-param2", pattern).each(function () {
                            setParam($(this), data.PatternParam2);
                        });

                        $(".range-begin-date", detail).val(formatDate(data.BeginDate, "M/d/yyyy"));
                        if (data.EndDate) {
                            $(".range-end-date-option", detail).prop("checked", true);
                            $(".range-end-infinite-option", detail).prop("checked", false);
                            $(".range-end-date", detail)
                                .removeClass("disabled")
                                .prop("readonly", false)
                                .val(formatDate(data.EndDate, "M/d/yyyy"));
                        }
                        else {
                            $(".range-end-date-option", detail).prop("checked", false);
                            $(".range-end-infinite-option", detail).prop("checked", true);
                            $(".range-end-date", detail)
                                .addClass("disabled")
                                .prop("readonly", true)
                                .val("");
                        }

                        detail.on("change", ".recurrence-pattern", function (e) {
                            var name = $(this).data("name");
                            $(".pattern", detail).css("display", "none");
                            $(".pattern." + name, detail).css("display", "block");
                        }).on("change", ".range-end-options input[type='radio']", function () {
                            if ($(".range-end-infinite-option", detail).prop("checked"))
                                $(".range-end-date", detail).addClass("disabled").prop("readonly", true);
                            else if ($(".range-end-date-option", detail).prop("checked"))
                                $(".range-end-date", detail).removeClass("disabled").prop("readonly", false);
                        }).on("click", ".save-recurrence", function (e) {
                            saveRecurrence(getRecurrence(detail), function (result) {
                                if (result.success)
                                    window.location = $(".return-url", detail).val();
                                else
                                    alert(result.message);
                            });

                        }).on("click", ".cancel-recurrence", function (e) {
                            window.location = $(".return-url", detail).val();
                        });
                    });

                    var list = $(".reservation-list", detail).dataTable({
                        "bProcessing": true,
                        "sAjaxSource": "/api/scheduler/reservation/datatables?RecurrenceID=" + id + "&Startable=true",
                        "aoColumns": [
                            {
                                "sWidth": "90px", "mData": "ReservationID", "mRender": function (data) {
                                    return '<a href="Reservation.aspx?reservation=' + data + '&recurrence=' + id + '">' + data + '</a>';
                                }
                            },
                            {
                                "sWidth": "120px", "mData": "BeginDateTime", "mRender": function (data) {
                                    return formatDate(data, "M/d/yyyy<br />h:mm:ss TT");
                                }
                            },
                            {
                                "sWidth": "120px", "mData": "EndDateTime", "mRender": function (data) {
                                    return formatDate(data, "M/d/yyyy<br />h:mm:ss TT");
                                }
                            },
                            {
                                "sSortDataType": "dom-checkbox", "sWidth": "90px", "mData": function (source, type, val) {
                                    return renderCheckboxColumn("AutoEnd", true, source, type, val);
                                }
                            },
                            {
                                "sSortDataType": "dom-checkbox", "sWidth": "90px", "mData": function (source, type, val) {
                                    return renderCheckboxColumn("KeepAlive", true, source, type, val);
                                }
                            },
                            {
                                "mData": "Notes", "mRender": function (data) {
                                    return data === "" ? '<span style="font-style: italic; color: #808080;">[none]</span>' : data;
                                }
                            }
                        ]
                    });

                });

                return;
            }

            var togglePattern = function () {
                //show the week day radio list when the 'Weekly' radio is selected
                if ($('.weekly-radio', $this).is(':checked')) {
                    $('.monthly-panel', $this).hide();
                    $('.weekly-panel', $this).show();
                }
                //show the monthly options when the 'Monthly' radio is selected
                if ($('.monthly-radio', $this).is(':checked')) {
                    $('.weekly-panel', $this).hide();
                    $('.monthly-panel', $this).show();
                }
            };

            var toggleRange = function () {
                //when 'End By' radio is selected enable the 'End By' datepicker
                if ($('.endby-radio', $this).is(':checked'))
                    $('.endby-textbox', $this).removeAttr('disabled').css('background-color', '');
                //when 'Infinite' radio is selected disable the 'End By' datepicker
                if ($('.infinite-radio', $this).is(':checked'))
                    $('.endby-textbox', $this).prop('disabled', true).css('background-color', '#D3D3D3').val('');
            };

            var toggleAutoEndWarning = function () {
                $(".autoend-warning", $this).toggle($(".autoend-checkbox", $this).prop("checked"));
            };

            var onIsRecurringChanged = function () {
                var isRecurring = $(".is-recurring-checkbox", $this).prop("checked");

                var redirectUrl = window.location.href.replace(/[\?&]recurring=1/, "");

                if (isRecurring) {
                    var separator = redirectUrl.indexOf("?") >= 0 ? "&" : "?";
                    window.location = [redirectUrl, "recurring=1"].join(separator)
                } else {
                    window.location = window.location.href.replace("&recurring=1", "");
                }
            };

            //Page Load...

            //convert textboxes to datepickers
            $('.datepicker', $this).datepicker();

            toggleAutoEndWarning();

            togglePattern();

            toggleRange();

            //-------------------------

            var autoEndWarningText = "";

            //Event Handlers
            $this.on("change", ".is-recurring-checkbox", function (e) {
                onIsRecurringChanged();
            }).on("change", '.autoend-checkbox', function (e) {
                toggleAutoEndWarning();
            }).on('change', '.weekly-radio', function (e) {
                togglePattern();
            }).on('change', '.monthly-radio', function (e) {
                togglePattern();
            }).on('change', '.infinite-radio', function (e) {
                toggleRange();
            }).on('change', '.endby-radio', function (e) {
                toggleRange();
            });
        });
    };
}(jQuery));