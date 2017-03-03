(function ($) {
    $.fn.schedule = function () {
        return this.each(function () {
            var $this = $(this);

            var getResourceId = function () {
                return $this.data("resource");
            };

            var getGranularity = function () {
                return $this.data("granularity");
            };

            var getView = function () {
                var uri = new URI();
                var qs = uri.query(true);
                if (qs.view) {
                    var splitter = qs.view.split("-");
                    switch (splitter[0]) {
                        case "week":
                            return "agendaWeek";
                        case "day":
                            return "agendaDay";
                        default:
                            return "month";
                    }
                }
                else
                    return "month";
            };

            var getHourView = function () {
                var uri = new URI();
                var qs = uri.query(true);
                if (qs.view) {
                    var splitter = qs.view.split("-");
                    if (splitter.length > 1) {
                        switch (splitter[1]) {
                            case "default":
                                return "default";
                            default:
                                return "full";
                        }
                    }
                    else
                        return "full";
                }
                else
                    return "full";
            };

            var load = function (dayView, hourView) {
                $this.html($("<div/>", { "class": "calendar" }));
                $(".calendar", $this).fullCalendar({
                    "events": "ajax/?Action=schedule&ResourceID=" + getResourceId(),
                    "header": {
                        "left": "title",
                        "center": "",
                        "right": "today prev,next month,agendaWeek,agendaDay"
                    },
                    "defaultView": dayView,
                    "slotDuration": getGranularity(),
                    "minTime": hourView === "default" ? "08:00:00" : "00:00:00",
                    "maxTime": hourView === "default" ? "17:00:00" : "24:00:00",
                    "viewRender": function (view, element) {
                        var h = $(".fc-slats", element).outerHeight();
                        if (view.name === "agendaWeek" || view.name === "agendaDay") {
                            var cell = $("div.fc-widget-header > table > thead > tr > th.fc-axis.fc-widget-header", element);
                            cell.css("text-align", "center").html($("<a/>", { "href": "#" }).html(hourView === "fullday" ? "default" : "full day").on("click", function (e) {
                                e.preventDefault();
                                var link = $(this);
                                if (hourView === "fullday")
                                    load(view.name, "default");
                                else
                                    load(view.name, "fullday");
                            }));

                            $(".calendar", $this).fullCalendar('option', 'contentHeight', h + 65);
                        }
                    },
                    "eventRender": function (event, element) {
                        element.on("mousemove", function (e) {
                            showTooltip({
                                "x": e.pageX + 15,
                                "y": e.pageY + 5,
                                "type": "window",
                                "title": "Click to send email to reserver",
                                "content": $("<div/>").append(
                                        $("<div/>").css("font-weight", "bold").html("Used by Zhang, Cheng")
                                    ).append(
                                        $("<div/>").css("font-weight", "bold").html("Phone: 734-123-4567")
                                    ).append(
                                        $("<div/>").css("font-weight", "bold").html("Email: chengzh@umich.edu")
                                    ).append(
                                        $("<div/>").css("font-weight", "bold").html("5:01pm - 5:21pm")
                                    )
                            });
                        }).on("mouseleave", function (e) {
                            hideTooltip();
                        });
                    }
                });
            };

            var getDefaultDayView = function () {
                var val = $this.data("dayview");
                switch (val) {
                    case "DayView":
                        return "agendaDay";
                    case "WeekView":
                        return "agendaWeek";
                    default:
                        return "month";
                }
            };

            var getDefaultHourView = function () {
                return $this.data("hourview") || "default";
            };

            load(getDefaultDayView(), getDefaultHourView());

            var headers = $(".fc-day-header", $this);

            var getHeader = function (x, y) {
                var result = null;
                headers.each(function () {
                    var offset = $(this).offset();
                    var width = $(this).width();
                    if (x >= offset.left && x <= offset.left + width) {
                        result = $(this);
                        return false;
                    }
                });
                return result;
            };

            var mouse = {
                time: null,
                row: null,
                header: null,
                moment: function () {
                    if (this.header === null)
                        return moment("invalid");

                    if (this.time === null)
                        return moment("invalid");

                    var m1 = moment(this.header.text(), "ddd M/D");
                    var m2 = this.time.indexOf(":") === -1 ? moment(this.time, "ha") : moment(this.time, "h:mma");

                    return m1.add(m2);
                }
            };

            var tooltip = $("<div/>", { "class": "tooltip" }).append(
                $("<div/>", { "class": "title" }).hide()
            ).append(
                $("<div/>", { "class": "content" })
            ).appendTo("body");

            var showTooltip = function (options) {

                tooltip.attr("class", "tooltip");

                if (options.type === null || options.type === "" || options.type === "default")
                    tooltip.addClass("default");
                else
                    tooltip.addClass(options.type);

                if (options.title)
                    $(".title", tooltip).html(options.title).show();
                else
                    $(".title", tooltip).hide();

                tooltip.css({ "top": options.y, "left": options.x });

                $(".content", tooltip).html(options.content);

                tooltip.show();
            };

            var hideTooltip = function () {
                tooltip.hide();
            };

            $this.on("schedule.load", function (e, dayView, hourView) {
                load(dayView, hourView);
            }).on("mousemove", ".fc-slats", function (e) {
                var slats = $(this);

                var h = getHeader(e.clientX, e.clientY);

                if (h === null)
                    hideTooltip();
                else {
                    mouse.header = h;
                    if (mouse.moment().isValid()) {
                        if (mouse.moment().isBefore(moment())) {
                            showTooltip({
                                "x": e.pageX + 15,
                                "y": e.pageY + 5,
                                "content": "You cannot create a reservation in the past"
                            });
                        } else {
                            hideTooltip();
                        }
                    }
                }
            }).on("mouseleave", ".fc-slats", function (e) {
                hideTooltip();
            }).on("mouseenter", ".fc-slats > table > tbody > tr", function (e) {
                mouse.row = $(this).index();
                mouse.time = $(this).find(".fc-time").text();
                //var h = getHeader(e.clientX, e.clientY);
                //if (h != null) mouse.header = h;
            });
        });
    };
}(jQuery));