(function ($) {
    $.fn.lablocation = function (options) {
        return this.each(function () {
            var _this = this;

            var $this = $(_this);

            var opts = $.extend({}, { "ajaxUrl": "ajax", "onload": null, "oninit": null }, options, $this.data());

            var handleError = function (jqXHR) {
                alert(jqXHR.responseJSON.ErrorMessage);
            };

            var collapseAll = function () {
                var expanded = $(".expanded", $this);
                expanded.removeClass("expanded");
                $(".resource-lab-locations", expanded).remove();
            };

            var loadAvailableResources = function (data) {
                $(".available-resource-select", $this).each(function () {
                    var select = $(this);
                    var reslabloc = select.closest(".resource-lab-locations");
                    if (data.AvailableResources.length > 0) {
                        select.html($.map(data.AvailableResources, function (item) {
                            return $("<option/>").val(item.ResourceID).html(getResourceName(item));
                        }));
                    } else {
                        select.html($("<option/>").html("No resources available in this lab")).prop("disabled", true);
                        $(".add-reslabloc", reslabloc).prop("disabled", true);
                    }
                });
            };

            var getLocationName = function (item) {
                return item.LabDisplayName + "/" + item.LocationName;
            };

            var getResourceName = function (item) {
                var rid = new String(1000000 + item.ResourceID);
                return "[" + rid.substr(1) + "] " + item.ResourceName;
            };

            var loadLocations = function (data) {
                $(".add-labloc-text", $this).val("");

                $(".lab-location-items", $this).html($.map(data.LabLocations, function (item) {
                    return $("<div/>", { "class": "lab-location", "data-lab-location-id": item.LabLocationID, "data-location-name": item.LocationName, "data-lab-id": item.LabID })
                        .append("[")
                        .append($("<a/>", { "class": "delete-labloc", "href": "#", "data-location-name": item.LocationName }).html("x"))
                        .append("] ")
                        .append($("<a/>", { "class": "expand-labloc location-name", "href": "#", "data-lab-location-id": item.LabLocationID }).html(getLocationName(item)));
                }));

                if (typeof opts.onload === "function")
                    opts.onload.call(_this, data);
            };

            var loadResourceLabLocations = function (data, labloc) {
                $(".resource-lab-locations", labloc).remove();

                var labLocationId = labloc.data("labLocationId");
                var labId = labloc.data("labId");

                var reslabloc = $("<div/>", { "class": "resource-lab-locations" });

                reslabloc.append(
                    $.map(data.ResourceLabLocations, function (item) {
                        return $("<div/>", { "class": "row mb-2" }).append(
                            $("<div/>", { "class": "col" })
                                .append("[")
                                .append($("<a/>", { "class": "delete-reslabloc", "href": "#", "data-lab-location-id": item.LabLocationID, "data-resource-lab-location-id": item.ResourceLabLocationID }).html("x"))
                                .append("] ")
                                .append($("<span/>", { "class": "resource-name" }).html(getResourceName(item)))
                        );
                    })
                );

                reslabloc.append(
                    $("<div/>", { "class": "row mb-2" }).append(
                        $("<div/>", { "class": "col-7" }).append(
                            $("<div/>", { "class": "input-group input-group" }).append(
                                $("<select/>", { "class": "custom-select available-resource-select" })
                            ).append(
                                $("<div/>", { "class": "input-group-append" }).append(
                                    $("<button/>", { "class": "btn btn-outline-secondary add-reslabloc", "type": "button", "data-lab-location-id": labLocationId }).html("Add")
                                )
                            )
                        )
                    )
                );

                reslabloc.append(
                    $("<div/>", { "class": "row mb-2" }).append(
                        $("<div/>", { "class": "col-7" }).append(
                            $("<div/>", { "class": "input-group input-group" }).append(
                                $("<div/>", { "class": "input-group-prepend" }).append(
                                    $("<select/>", { "class": "custom-select modify-lab-select" })
                                        .append($("<option/>", { "value": 1 }).html("Clean Room"))
                                        .append($("<option/>", { "value": 9 }).html("ROBIN"))
                                )
                            ).append(
                                $("<input/>", { "type": "text", "class": "form-control modify-locname-text" }).val(labloc.data("locationName"))
                            ).append(
                                $("<div/>", { "class": "input-group-append" }).append(
                                    $("<button/>", { "class": "btn btn-outline-secondary modify-labloc", "type": "button", "data-lab-location-id": labLocationId }).html("Modify")
                                )
                            )
                        )
                    )
                );

                $(".modify-lab-select", reslabloc).val(labId);

                labloc.append(reslabloc);

                loadAvailableResources(data);
            };

            var search = function (searchText) {
                $.ajax({
                    "url": opts.ajaxUrl + "?Command=get-lablocations&Search=" + searchText,
                    "method": "GET"
                }).done(loadLocations).fail(handleError);
            };

            $.ajax({
                "url": opts.ajaxUrl + "?Command=get-lablocations",
                "method": "GET"
            }).done(loadLocations).fail(handleError).always(function () {
                if (typeof opts.oninit === "function")
                    opts.oninit.call(_this, {
                        "search": search
                    });
            });

            $this.on("click", ".delete-labloc", function (e) {
                e.preventDefault();

                var locationName = $(this).data("locationName");

                $.ajax({
                    "url": opts.ajaxUrl,
                    "method": "POST",
                    "data": { "Command": "delete-lablocation", "LocationName": locationName }
                }).done(loadLocations).fail(handleError);
            }).on("click", ".expand-labloc", function (e) {
                e.preventDefault();

                var link = $(this);

                var labLocationId = link.data("labLocationId");

                $.ajax({
                    "url": opts.ajaxUrl + "?Command=get-resource-lablocations&LabLocationID=" + labLocationId,
                    "method": "GET"
                }).done(function (data) {
                    var labloc = link.closest(".lab-location");
                    var isExpanded = labloc.hasClass("expanded");

                    collapseAll();

                    if (!isExpanded) {
                        labloc.addClass("expanded");
                        loadResourceLabLocations(data, labloc);
                    }
                }).fail(handleError);
            }).on("click", ".delete-reslabloc", function (e) {
                e.preventDefault();

                var link = $(this);

                var labLocationId = link.data("labLocationId");
                var resourceLabLocationId = link.data("resourceLabLocationId");
                var labloc = link.closest(".lab-location");

                $.ajax({
                    "url": opts.ajaxUrl,
                    "method": "POST",
                    "data": { "Command": "delete-resource-lablocation", "LabLocationID": labLocationId, "ResourceLabLocationID": resourceLabLocationId }
                }).done(function (data) {
                    loadResourceLabLocations(data, labloc);
                }).fail(handleError);
            }).on("click", ".add-reslabloc", function (e) {
                var labloc = $(this).closest(".lab-location");
                var reslabloc = $(this).closest(".resource-lab-locations");
                var labLocationId = $(this).data("labLocationId");
                var select = $(".available-resource-select", reslabloc);
                var resourceId = select.val();

                $.ajax({
                    "url": opts.ajaxUrl,
                    "method": "POST",
                    "data": { "Command": "add-resource-lablocation", "LabLocationID": labLocationId, "ResourceID": resourceId }
                }).done(function (data) {
                    loadResourceLabLocations(data, labloc);
                }).fail(handleError);
            }).on("click", ".modify-labloc", function (e) {
                var reslabloc = $(this).closest(".resource-lab-locations");
                var labLocationId = $(this).data("labLocationId");
                var locationName = $(".modify-locname-text", reslabloc).val();
                var labId = $(".modify-lab-select", reslabloc).val();

                $.ajax({
                    "url": opts.ajaxUrl,
                    "method": "POST",
                    "data": { "Command": "modify-lablocation", "LabLocationID": labLocationId, "LocationName": locationName, "LabID": labId }
                }).done(loadLocations).fail(handleError);
            }).on("click", ".add-labloc", function (e) {
                var locationName = $(".add-labloc-text", $this).val();
                var labId = $(".add-labloc-lab-select", $this).val();
                $.ajax({
                    "url": opts.ajaxUrl,
                    "method": "POST",
                    "data": { "Command": "add-lablocation", "LocationName": locationName, "LabID": labId }
                }).done(loadLocations).fail(handleError);
            });
        });
    };
})(jQuery);