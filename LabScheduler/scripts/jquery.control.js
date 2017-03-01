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