(function ($) {
    $.fn.preferences = function () {
        return this.each(function () {
            var $this = $(this);

            var accs = $(".lblAccounts", $this).text().split(",");
            var accsNames = $(".lblAccountsNames", $this).text().split(":");

            var createNode = function (acct) {
                var icon = $('<span/>').addClass('ui-icon').addClass('ui-icon-arrowthick-2-n-s');
                return $('<li/>')
                    .attr('value', acct.id)
                    .addClass('ui-state-default')
                    .append(icon)
                    .append(acct.name);
            }

            for (var i = 0; i < accs.length; i++) {
                $(".listAccountSortable", $this).append(createNode({ id: accs[i], name: accsNames[i] }));
            }

            $(".listAccountSortable", $this).sortable();
            $(".listAccountSortable", $this).disableSelection();

            var toggleHourRange = function () {
                if ($('.working-hours.allday input[type="radio"]', $this).is(':checked'))
                    $('.hour-range', $this).prop('disabled', true);
                else
                    $('.hour-range', $this).prop('disabled', false);
            }

            $this.on('change', '.working-hours', function (event) {
                toggleHourRange();
            }).on('click', '.pref-submit', function (event) {
                //alert('Before:  ' + $(".accounts-result input").val());
                var orderedAccounts = [];

                $(".listAccountSortable li", $this).each(function () {
                    orderedAccounts.push($(this).val());
                });

                $(".accounts-result input", $this).val(orderedAccounts.join(","));
                //alert('AFTER:  ' + $(".accounts-result input").val());
            });

            toggleHourRange();
        });
    }
}(jQuery));