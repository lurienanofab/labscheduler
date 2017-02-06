(function ($) {
    $.fn.reservationView = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opts = $.extend({}, { "onClick": null, "path": null }, options, $this.data());

            var getRedirectUrl = function (args) {
                var result = "ReservationController.ashx?Command=" + args.Command + "&ReservationID=" + args.ReservationID + "&Date=" + args.Date + "&State=" + args.State;

                if (args.Path)
                    result += "&Path=" + args.Path;

                return result;
            };

            $this.on("click", ".reservation-action", function (e) {
                var cell = $(this);

                var args = {
                    Command: cell.data("command"),
                    ReservationID: parseInt(cell.data("reservation-id")), //0 for new reservations
                    Date: cell.data("date"), //every cell has one
                    State: cell.data("state"),
                    Path: opts.path
                };

                var valid = true;

                if (!args.Command) {
                    console.log("ReservationView Error: clickable cell with no Command", args);
                    valid = false;
                }

                if (isNaN(args.ReservationID)) {
                    console.log("ReservationView Error: clickable cell with no ReservationID", args);
                    valid = false;
                }

                if (!moment(args.Date, "YYYY-MM-DD[T]HH:mm:ss").isValid()) {
                    console.log("ReservationView Error: clickable cell with invalid Date", args);
                    valid = false;
                }

                if (!args.State) {
                    console.log("ReservationView Error: clickable cell with no State", args);
                    valid = false;
                }

                if (typeof opts.onClick === "function")
                    valid = opts.onClick.call(cell, args);

                if (valid)
                    window.location = getRedirectUrl(args);
            }).on("click", ".ReservDelete", function (e) {
                e.stopPropagation();
                if (!confirm("Are you sure you want to cancel this reservation?"))
                    e.preventDefault();
            }).on("click", ".ReservModify", function (e) {
                e.stopPropagation();
            }).on("mouseover", "[data-tooltip]", function (e) {
                e.stopPropagation();
                var cell = $(this);
                var tooltip = cell.data("tooltip");
                if (tooltip) {
                    var caption = cell.data("caption");
                    if (caption)
                        overlib(tooltip, CAPTION, caption);
                    else
                        overlib(tooltip);
                }
            }).on("mouseout", "[data-tooltip]", function (e) {
                e.stopPropagation();
                nd();
            });

            // if the modal is there then show it
            var model = $(".start-confirmation-dialog", $this).modal({
                show: true
            });
        });
    };
}(jQuery));