(function ($) {
    $.fn.staging = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opts = $.extend({}, { "isStaging": false }, options, $this.data());

            if (opts.isStaging) {
                var staging = $("<div/>").css({
                    'background-color': '#ffff00',
                    'color': '#808000',
                    'font-size': '20px',
                    'font-weight': 'bold',
                    'border-top': '2px ridge',
                    'border-bottom': '2px ridge',
                    'padding': '5px',
                    'text-align': 'center',
                    'position': 'fixed',
                    'top': '0',
                    'z-index': '1',
                    'width': '100%'
                }).html("STAGING");

                var getGlimpseState = function () {
                    var cookieValue = Cookies.get('glimpsePolicy');
                    return cookieValue;
                };

                var setGlimpseState = function (state) {
                    Cookies.set('glimpsePolicy', state, { expires: new Date('Sat, 01 Jan 2050 12:00:00 GMT'), path: '/' });
                };

                var toggleGlimpse = function () {
                    var glimpseState = getGlimpseState();
                    if (glimpseState === 'On')
                        setGlimpseState('');
                    else
                        setGlimpseState('On');

                    window.location.reload();
                };

                var toggle = $("<a/>", { "href": "#", "class": "toggle-glimpse" });

                var setToggleText = function () {
                    var glimpseState = getGlimpseState();
                    if (glimpseState === "On")
                        toggle.html("turn off profiling");
                    else
                        toggle.html("turn on profiling");
                };

                staging.prepend($("<span/>").css({ "font-size": "10pt", "float": "left", "margin-top": "3px" }).append("[").append(toggle).append("]"));

                if (window.self === window.top)
                    staging.append($("<span/>").css({ "font-size": "10pt", "float": "right", "margin-right": "20px", "margin-top": "3px" }).html('[<a href="http://ssel-sched.eecs.umich.edu/sselscheduler/">switch to production</a> | <a href="//ssel-apps.eecs.umich.edu/login">log out</a>]'));
                else
                    staging.append($("<span/>").css({ "font-size": "10pt", "float": "right", "margin-right": "20px", "margin-top": "3px" }).html('[<a href="http://ssel-sched.eecs.umich.edu/sselonline/?view=/sselscheduler" target="_top">switch to production</a> | <a href="//ssel-apps.eecs.umich.edu/login">log out</a>]'));

                staging.on("click", ".toggle-glimpse", function (e) {
                    e.preventDefault();
                    toggleGlimpse();
                    setToggleText();
                });

                setToggleText();

                $("form", $this).prepend(staging);

                $(".scheduler-main", $this).css("margin-top", "40px");
            }
        });
    };
}(jQuery));