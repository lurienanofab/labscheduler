(function ($) {
    $.fn.dateManager = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opt = $.extend({}, { "onChange": null, "format": "mm/dd/yyyy" }, options);

            var $datepicker = $('.datepicker', $this).datepicker({ 'autoclose': true, 'format': opt.format, 'verbose': false });
            var $daterange = $('.daterange-select', $this);

            var writeLog = function (label, obj) {
                if (opt.verbose)
                    console.log(label, obj);
            };

            var onChange = function () {
                if (typeof opt.onChange === 'function') {
                    opt.onChange({
                        'sdate': formatDate($('.sdate', $this).datepicker('getDate'), opt.format),
                        'edate': formatDate($('.edate', $this).datepicker('getDate'), opt.format)
                    });
                }
            };

            var now = new Date();

            var formatDate = function (d, f) {
                if (d === null || d === '')
                    return '';

                var dd = d.getDate();
                var mm = d.getMonth() + 1;
                var yy = d.getFullYear();
                var dow = d.getDay();

                var monthNames = ['January', 'Februray', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
                var dayNames = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

                var result = f.toString()
                    .replace('mm', (mm + 100).toString().substr(1, 2))
                    .replace('m', mm.toString())
                    .replace('dd', (dd + 100).toString().substr(1, 2))
                    .replace('d', dd.toString())
                    .replace('yyyy', yy.toString())
                    .replace('yy', yy.toString().substr(2, 2))
                    .replace('MM', monthNames[mm - 1])
                    .replace('M', monthNames[mm - 1].substr(0, 3))
                    .replace('DD', dayNames[dow])
                    .replace('D', dayNames[dow].substr(0, 3));

                return result;
            };

            var ranges = [
                { 'text': '30 days', 'sdate': formatDate(new Date(now.getFullYear(), now.getMonth(), now.getDate() - 30), opt.format), 'edate': formatDate(now, opt.format) },
                { 'text': '3 months', 'sdate': formatDate(new Date(now.getFullYear(), now.getMonth() - 3, now.getDate()), opt.format), 'edate': formatDate(now, opt.format) },
                { 'text': '1 year', 'sdate': formatDate(new Date(now.getFullYear() - 1, now.getMonth(), now.getDate()), opt.format), 'edate': formatDate(now, opt.format) },
                { 'text': 'All', 'sdate': '', 'edate': '' }
            ];

            writeLog('ranges', ranges);

            var checkDateRange = function () {
                var index = -1;

                var sd = formatDate($('.sdate', $this).datepicker('getDate'), opt.format);
                var ed = formatDate($('.edate', $this).datepicker('getDate'), opt.format);

                for (i = 0; i < ranges.length; i++) {
                    if (ed === ranges[i].edate) {
                        if (sd === ranges[i].sdate) {
                            index = i;
                            break;
                        }
                    }
                }

                writeLog('checkDateRange', { 'sd': sd, 'ed': ed, 'index': index });

                $daterange.get(0).selectedIndex = index;
            };

            var setDateRange = function (r) {
                $datepicker.off('changeDate');

                writeLog('setDateRange', { 'range': ranges[r] });

                $('.sdate', $this).datepicker('setDate', ranges[r].sdate);
                $('.edate', $this).datepicker('setDate', ranges[r].edate);

                onChange();

                $datepicker.on('changeDate', handleChangeDate);
            };

            var handleChangeDate = function () {
                checkDateRange();
                onChange();
            };

            $datepicker.on('changeDate', handleChangeDate);

            $daterange.change(function () {
                var val = parseInt($(this).val());
                setDateRange(val);
            });

            checkDateRange();
        });
    };
}(jQuery));