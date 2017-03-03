(function ($) {
    $.fn.control = function () {
        return this.each(function () {
            var $this = $(this);

            var $this = $(this);

            var id = $this.data("id");
            var state = false;

            var displayError = function (err) {
                $(".status", $this).html($("<div/>").css("color", "#ff0000").html(err));
            }

            var displayStatus = function () {
                if (state)
                    $(".status", $this).html("Tool Enabled");
                else
                    $(".status", $this).html("Tool Disabled");

                $(".toggle-button", $this).prop("disabled", false);
            }


            var updateState = function () {
                $(".toggle-button", $this).prop("disabled", true);

                return $.ajax({
                    "url": "ajax/",
                    "type": "GET",
                    "data": { "Action": "interlock", "Command": "get-point-state", "ResourceID": id, "ts": (new Date()).valueOf() },
                    "success": function (data, textStatus, jqXHR) {
                        if (data.Success) {
                            state = data.Data.State;
                            displayStatus();
                        }
                        else
                            displayError(data.Message);
                    },
                    "error": function (jqXHR, textStatus, errorThrown) {
                        var title = $(jqXHR.responseText).filter("title").html();
                        displayError(title || errorThrown);
                    }
                });
            }

            var loading = function () {
                $(".status", $this).html($("<img/>", { "src": "//ssel-apps.eecs.umich.edu/static/images/ajax-loader-4.gif", "alt": "loading..." }));
            }

            var toggleState = function () {
                return $.ajax({
                    "url": "ajax/",
                    "type": "GET",
                    "data": { "Action": "interlock", "Command": "set-point-state", "ResourceID": id, "State": !state, "ts": (new Date()).valueOf() },
                    "success": function (data, textStatus, jqXHR) {
                        if (data.Success)
                            updateState();
                        else
                            displayError(data.Message);
                    },
                    "error": function (jqXHR, textStatus, errorThrown) {
                        var title = $(jqXHR.responseText).filter("title").html();
                        displayError(title || errorThrown);
                    }
                });
            }

            loading();
            updateState();

            $this.on("click", ".toggle-button", function (e) {
                loading();
                toggleState();
            });
        });
    }
}(jQuery));