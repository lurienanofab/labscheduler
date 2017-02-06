(function ($) {
    $.fn.numerictext = function (options) {
        return this.each(function () {
            var $this = $(this);

            // by default:
            //      negative numbers are allowed (positive === false)
            //      real numbers are allowed such as 1.2345 (real === true)
            //      any number of decimals are allowed (decimals === 0)  [when real is false no decimals are allowed]
            var opts = $.extend({}, { "positive": false, "real": true, "decimals": 0 }, options, $this.data());

            var allowed = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "Left", "Right", "Up", "Down", "Backspace", "Insert", "Del", "NumLock", "Tab", "Home", "End", "PageUp", "PageDown"];

            var keys = {
                48: "0", 49: "1", 50: "2", 51: "3", 52: "4", 53: "5", 54: "6", 55: "7", 56: "8", 57: "9",
                96: "0", 97: "1", 98: "2", 99: "3", 100: "4", 101: "5", 102: "6", 103: "7", 104: "8", 105: "9",
                190: ".", 110: ".",
                37: "Left", 38: "Up", 39: "Right", 40: "Down",
                8: "Backspace", 9: "Tab", 33: "PageUp", 34: "PageDown", 35: "End", 36: "Home", 45: "Insert", 46: "Del", 144: "NumLock"
            };

            if (!opts.positive)
                allowed.push("-");

            if (opts.options.real)
                allowed.push(".");

            $this.on("keydown", function (e) {
                var keyPressed = e.key || keys[e.which];

                var currentVal = $this.val();

                // never allow multiple periods
                if (currentVal.indexOf(".") !== -1 && keyPressed === ".") {
                    e.preventDefault();
                    return;
                }

                if ($.inArray(keyPressed, allowed) === -1) {
                    e.preventDefault();
                    return;
                }
            }).on("change", function (e) {
                var $this = $(this);

                var currentVal = $this.val();
                var decimals = parseInt(opts.decimals);
                if (isNaN(decimals)) decimals = 0;

                if (currentVal.indexOf(".") !== -1 && decimals > 0) {
                    var n = parseFloat(currentVal);
                    if (isNaN(n)) n = 0;
                    var v = n.toFixed(decimals);
                    $this.val(v);
                }
            });
        });
    };
}(jQuery));