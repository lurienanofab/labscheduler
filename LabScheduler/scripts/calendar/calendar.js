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
    $.fn.calendar = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opt = $.extend({}, { date: null, month: null, returnto: "default", headers: "S,M,T,W,T,F,S" }, options, $this.data());

            var getFirstOfMonth = function (date) {
                return moment(moment(date).format("YYYY-MM-01"));
            }

            var selectedDate;
            var selectedMonth;

            if (opt.date == null)
                selectedDate = moment();
            else
                selectedDate = moment(opt.date, "YYYY-MM-DD");

            if (opt.month == null)
                selectedMonth = getFirstOfMonth(selectedDate);
            else
                selectedMonth = getFirstOfMonth(opt.month);

            var createCalendarWeek = function(date){
                var cw = {};
                cw.sunday = date.clone();
                cw.monday = date.clone().add(1, 'days');
                cw.tuesday = date.clone().add(2, 'days');
                cw.wednesday = date.clone().add(3, 'days');
                cw.thursday = date.clone().add(4, 'days');
                cw.friday = date.clone().add(5, 'days');
                cw.saturday = date.clone().add(6, 'days');
                return cw
            }

            var generateWeeks = function (date) {
                var result = [];

                var fom = getFirstOfMonth(date);

                //get the most recent Sunday before fom
                var sd = fom.clone().day(0);

                //check if the first day of the month is a Sunday, if so add another row for the last week of the previous month
                //because we always want to see some days from the the previous month
                if (sd.isSame(fom))
                    sd.subtract(7, 'days');

                var prevMonth = getPrevMonth(fom);
                var nextMonth = getNextMonth(fom);

                console.log({ 'fom': fom.format("YYYY-MM-DD"), 'prevMonth': prevMonth.format("YYYY-MM-DD"), 'nextMonth': nextMonth.format("YYYY-MM-DD") });

                while (sd.isBefore(nextMonth))
                {
                    var cw = createCalendarWeek(sd);
                    result.push(cw);
                    sd.add(7, 'days');
                }

                //check if the last day of the month is a saturday, if so add another row for the 1st week of the next month
                //because we always want to see some days from the next month
                var lom = nextMonth.clone().subtract(1, 'days');

                if (lom.day() == 6)
                {
                    var cw = createCalendarWeek(nextMonth);
                    result.push(cw);
                }

                return result;
            }

            var draw = function () {
                var root = $("<div/>", { "class": "calendar-root" });

                root.append(createHeader());
                root.append(createTable());

                $this.html(root);
            }

            var createHeader = function () {
                var header = $("<div/>", { "class": "calendar-header" });

                var prevMonth = $("<a/>", { "class": "month-prev", "href": "#" });
                header.append(prevMonth);

                var nextMonth = $("<a/>", { "class": "month-next", "href": "#" });
                header.append(nextMonth);

                var monthText = $("<div/>", { "class": "month-text" }).html(selectedMonth.format("MMMM YYYY"));
                header.append(monthText);

                return header;
            }

            var createTable = function () {
                var table = $("<table/>", { "class": "calendar-table" });

                table.append($("<thead/>"));
                table.append($("<tbody/>"));

                var row;

                row = $("<tr/>")

                row.html($.map(opt.headers.split(","), function (value, index) {
                    return $("<th/>").html(value);
                }));

                $("thead", table).append(row);

                var weeks = generateWeeks(selectedMonth);

                $.each(weeks, function (index, value) {
                    row = $("<tr/>");
                    row.append(createDayCell(value.sunday));
                    row.append(createDayCell(value.monday));
                    row.append(createDayCell(value.tuesday));
                    row.append(createDayCell(value.wednesday));
                    row.append(createDayCell(value.thursday));
                    row.append(createDayCell(value.friday));
                    row.append(createDayCell(value.saturday));
                    $("tbody", table).append(row);
                });

                return table;
            }

            var getNextMonth = function (date) {
                return date.clone().add(1, 'months');
            }

            var getPrevMonth = function (date) {
                return date.clone().subtract(1, 'months');
            }

            var createDayCell = function (date) {
                var cell = $("<td/>");

                var d = moment(moment(date).format("YYYY-MM-DD"));

                if (d.isBefore(selectedMonth))
                    cell.addClass("date-prev-month");
                else if (d.isSameOrAfter(getNextMonth(selectedMonth)))
                    cell.addClass("date-next-month");

                if (d.isSame(moment().format("YYYY-MM-DD")))
                    cell.addClass("date-today");

                if (d.isSame(selectedDate.format("YYYY-MM-DD")))
                    cell.CssClass = "date-selected";

                var link = $("<a/>", { "class": "date" });

                var uri = new URI();
                uri.setSearch("Date", d.format("YYYY-MM-DD"));
                var href = uri.toString();

                link.prop("href", href);

                link.html(d.date());

                cell.append(link);

                return cell;
            }

            $this.on("click", ".month-prev", function (e) {
                e.preventDefault();
                selectedMonth.subtract(1, 'months');
                draw();
            }).on("click", ".month-next", function (e) {
                e.preventDefault();
                selectedMonth.add(1, 'months');
                draw();
            });
        });
    }
}(jQuery));