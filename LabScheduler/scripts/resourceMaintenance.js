﻿/*
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
    $.fn.resourceMaintenance = function () {
        return this.each(function () {
            var $this = $(this);

            //var opt = $.extend({}, {}, options, $this.data());

            var getStartOption = function () {
                var opt = $(".repair-duration-start-option:checked", $this);
                return opt.val();
            };

            var getEndOption = function () {
                var opt = $(".repair-duration-end-option:checked", $this);
                return opt.val();
            };

            var getStartMinutes = function () {
                var v = parseFloat($(".repair-duration-start", $this).val());
                if ("hours" === getStartOption())
                    return v * 60;
                else
                    return v;
            };

            var getEndMinutes = function () {
                var v = parseFloat($(".repair-duration-end", $this).val());
                if ("hours" === getEndOption())
                    return v * 60;
                else
                    return v;
            };

            var getDuration = function () {
                var startTime = getRepairStartTime();
                var endTime = getRepairEndTime();

                var result = "";

                if (!startTime || !endTime)
                    return result;

                if (!startTime.isValid() || !endTime.isValid())
                    return result;

                result += startTime !== null && startTime.isValid() ? startTime.format("M/D/YYYY h:mm:ss A") : "?";
                result += " to ";
                result += endTime !== null && endTime.isValid() ? endTime.format("M/D/YYYY h:mm:ss A") : "?";

                return result;
            };

            var getRepairStartTime = function () {
                var minutes = getStartMinutes();

                if (isNaN(minutes))
                    return null;

                var m = moment();
                m.subtract(minutes, 'minutes');

                return m;
            };

            var getRepairEndTime = function () {
                var minutes = getEndMinutes();

                if (isNaN(minutes))
                    return null;

                var m = moment();
                m.add(minutes, 'minutes');

                return m;
            };

            $this.on("change", ".repair-duration input", function (e) {
                $(".repair-duration-message", $this).html(getDuration());
            }).on("change", ".repair-status", function (e) {
                $(".repair-duration", $this).toggle($(".repair-status.offline", $this).prop("checked"));
            });

            $(".repair-duration-message", $this).html(getDuration());
        });
    };
}(jQuery));