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
    $.fn.facilityDownTime = function (options) {
        return this.each(function () {
            var $this = $(this);

            var defaults = {};

            var opt = $.extend({}, defaults, options, $this.data());

            var loadLabs = function () {
                var def = $.Deferred();

                var labs = $(".labs", $this);

                $.ajax({
                    "url": "/webapi/scheduler/lab/active"
                }).done(function (data, textStatus, jqXHR) {
                    var special = $("<option/>", { "value": "default" });
                    var specialIds = [1, 9];
                    var specialHtml = "";
                    var amp = "";

                    var options = [];

                    options.push(special);

                    $.each(data, function (index, value) {
                        if ($.inArray(value.LabID, specialIds) > -1) {
                            specialHtml += amp + value.DisplayName;
                            amp = " & ";
                        }

                        options.push($("<option/>", { "value": value.LabID }).html(value.DisplayName));
                    });

                    special.html(specialHtml);

                    options.push($("<option/>", { "value": "all" }).html("All"));

                    labs.html(options);

                    def.resolve();
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    def.reject(jqXHR.responseJSON);
                });

                return def.promise();
            }

            var getFullResourceName = function (res) {
                return res.ProcessTech.Lab.DisplayName + ": " + res.ProcessTech.ProcessTechName + ": " + res.ResourceName;
            }

            var loadTools = function (lab) {
                $.ajax({
                    "url": "/webapi/scheduler/resource/active/lab/" + lab
                }).done(function (data, textStatus, jqXHR) {
                    var options = [];
                    $.each(data, function (index, value) {
                        options.push($("<option/>", { "value": value.ResourceID }).html(getFullResourceName(value)));
                    });
                    $(".tool-list", $this).html(options);
                }).fail(function (jqXHR, textStatus, errorThrown) {
                    $(".tool-list", $this).html($("<option/>").html("error: failed to load tools").prop("disabled", true));
                });
            }

            var getSelectedTab = function () {
                var result;

                if ($(".selected-tab", $this).val() == "manage")
                    result = "#manage";
                else
                    result = "#edit";

                console.log("selected tab: " + result);

                return result;
            }
            
            var toggleTabVisibility = function () {
                // set all wrapper visibility to hidden
                $(".tab-wrapper", $this).css({ "visibility": "hidden" });

                // get the active tab
                var activeTab = $(".nav-tabs li.active > a[data-toggle='tab']", $this);

                // the pane id is in the href attribute
                var id = activeTab.attr("href");

                // get the active tab pane
                var pane = $this.find(id);

                // make the wrapper in the active pane visible
                pane.find(".tab-wrapper").css({ "visibility": "visible" });
            }

            loadLabs().done(function () {
                loadTools($(".labs", $this).val());
            }).fail(function (err) {
                $(".new-message", $this).html($("<div/>", { "class": "alert alert-danger", "role": "alert" }).html(err.ExceptionMessage));
            });

            $(".nav-tabs a[href='" + getSelectedTab() + "']", $this).tab("show");

            toggleTabVisibility();

            $this.on("loadLabs", function (e) {
                loadLabs();
            }).on("loadTools", function (e, labId) {
                loadTools(labId);
            }).on("change", ".labs", function (e) {
                $this.trigger("loadTools", $(".labs", $this).val());
            }).on("shown.bs.tab", ".nav-tabs a[data-toggle='tab']", function (e) {
                toggleTabVisibility();
            });
        });
    };
}(jQuery));