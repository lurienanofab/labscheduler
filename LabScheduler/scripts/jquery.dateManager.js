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
    $.fn.dateManager = function () {
        return this.each(function () {
            var $this = $(this);

            var formatDate = function (d) {
                var dd = d.getDate();
                var mm = d.getMonth() + 1;
                var yy = d.getFullYear();
                return (mm + 100).toString().substr(1, 2) + '/' + (dd + 100).toString().substr(1, 2) + '/' + yy.toString();
            }

            var getEndDate = function (d, index) {
                switch (index) {
                    case 3:
                        return '';
                    default:
                        return formatDate(d);
                }
            }

            var getStartDate = function (d, index) {
                switch (index) {
                    case 3: //All
                        return '';
                    case 2: //1 year
                        return formatDate(new Date(d.getFullYear() - 1, d.getMonth(), d.getDate()));
                    case 1: //3 months
                        return formatDate(new Date(d.getFullYear(), d.getMonth() - 3, d.getDate()));
                    case 0: //30 days
                        return formatDate(new Date(d.getFullYear(), d.getMonth(), d.getDate() - 30));
                }
            }

            var checkDateRange = function () {
                var now = new Date();
                var r = -1;
                for (i = 0; i < 4; i++) {
                    if ($('.edate', $this).val() == getEndDate(now, i)) {
                        if ($('.sdate', $this).val() == getStartDate(now, i)) {
                            r = i;
                            break;
                        }
                    }
                }
                $('.daterange-select', $this).get(0).selectedIndex = r;
            }

            var setDateRange = function (r) {
                var now = new Date();
                $('.sdate', $this).val(getStartDate(now, r));
                $('.edate', $this).val(getEndDate(now, r));
            }

            $('.datepicker', $this).datepicker({
                'onSelect': function (date, obj) {
                    checkDateRange();
                }
            });

            $('.daterange-select', $this).change(function () {
                var val = $(this).val();
                setDateRange(parseInt(val));
                setDateRange(parseInt(val));
            });

            checkDateRange();
        });
    }
}(jQuery));