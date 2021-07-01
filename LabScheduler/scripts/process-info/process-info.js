(function ($) {
    function API(url) {
        this.getProcessInfo = function (resourceId) {
            return $.ajax({
                "url": url,
                "method": "POST",
                "contentType": "application/json",
                "dataType": "json",
                "data": JSON.stringify({ "command": "get-process-info", "resourceId": resourceId })
            });
        };

        this.moveUp = function (resourceId, processInfoId) {
            return $.ajax({
                "url": url,
                "method": "POST",
                "contentType": "application/json",
                "dataType": "json",
                "data": JSON.stringify({ "command": "move-up", "resourceId": resourceId, "processInfoId": processInfoId })
            });
        };

        this.moveDown = function (resourceId, processInfoId) {
            return $.ajax({
                "url": url,
                "method": "POST",
                "contentType": "application/json",
                "dataType": "json",
                "data": JSON.stringify({ "command": "move-down", "resourceId": resourceId, "processInfoId": processInfoId })
            });
        };

        this.updateProcessInfo = function (model) {
            return $.ajax({
                "url": url,
                "method": "POST",
                "contentType": "application/json",
                "dataType": "json",
                "data": JSON.stringify({ "command": "update-process-info", "model": model })
            });
        };

        this.deleteProcessInfo = function (resourceId, processInfoId) {
            return $.ajax({
                "url": url,
                "method": "POST",
                "contentType": "application/json",
                "dataType": "json",
                "data": JSON.stringify({ "command": "delete-process-info", "resourceId": resourceId, "processInfoId": processInfoId })
            });
        };

        this.updateProcessInfoLine = function (resourceId, model) {
            return $.ajax({
                "url": url,
                "method": "POST",
                "contentType": "application/json",
                "dataType": "json",
                "data": JSON.stringify({ "command": "update-process-info-line", "resourceId": resourceId, "model": model })
            });
        };

        this.deleteProcessInfoLine = function (resourceId, processInfoId, processInfoLineId) {
            return $.ajax({
                "url": url,
                "method": "POST",
                "contentType": "application/json",
                "dataType": "json",
                "data": JSON.stringify({ "command": "delete-process-info-line", "resourceId": resourceId, "processInfoId": processInfoId, "processInfoLineId": processInfoLineId })
            });
        };

        this.addProcessInfo = function (model) {
            return $.ajax({
                "url": url,
                "method": "POST",
                "contentType": "application/json",
                "dataType": "json",
                "data": JSON.stringify({ "command": "add-process-info", "model": model })
            });
        };

        this.addProcessInfoLine = function (resourceId, model) {
            return $.ajax({
                "url": url,
                "method": "POST",
                "contentType": "application/json",
                "dataType": "json",
                "data": JSON.stringify({ "command": "add-process-info-line", "resourceId": resourceId, "model": model })
            });
        };
    }

    $.fn.processInfo = function (options) {
        return this.each(function () {
            var $this = $(this);

            var opts = $.extend({}, { ajaxUrl: "ajax", resourceId: 0 }, options, $this.data());

            var api = new API(opts.ajaxUrl);

            var showError = function (msg) {
                alert(msg);
            };

            var isNumber = function (n) {
                return !isNaN(parseFloat(n)) && isFinite(n);
            };

            var closeProcessInfoLines = function () {
                // hide any visible process info line rows (row that contains the ProcessInfoLine table)
                $(".process-info", $this).removeClass("expanded");
                $(".process-info-line-parent", $this).removeClass("expanded");
            };

            var getProcessInfoLineParent = function (processInfoId) {
                return $(".process-info-line-parent[data-process-info-id=" + processInfoId + "]", $this);
            };

            var getProcessInfoLine = function (e) {
                if (isNumber(e)) {
                    return $(".process-info-line[data-process-info-line-id='" + e + "']", $this);
                } else if ("currentTarget" in e) {
                    return $(e.currentTarget).closest(".process-info-line");
                }
            };

            var getProcessInfo = function (e) {
                if (isNumber(e)) {
                    return $(".process-info[data-process-info-id='" + e + "']", $this);
                } else if ("currentTarget" in e) {
                    return $(e.currentTarget).closest(".process-info");
                }
            };

            var createProcessInfoModel = function (data) {
                return {
                    "infos": $.map(data, function (item) {
                        return $.extend({
                            "processInfoId": item.ProcessInfoID,
                            "processInfoName": item.ProcessInfoName,
                            "paramName": item.ParamName,
                            "valueName": item.ValueName,
                            "special": item.Special,
                            "allowNone": item.AllowNone ? "True" : "False",
                            "requireValue": item.RequireValue ? "True" : "False",
                            "requireSelection": item.RequireSelection ? "True" : "False",
                            "lines": null
                        }, createProcessInfoLineModel(item.Lines));
                    })
                };
            };

            var createProcessInfoLineModel = function (data) {
                return {
                    "lines": $.map(data, function (pil) {
                        return {
                            "processInfoLineId": pil.ProcessInfoLineID,
                            "processInfoId": pil.ProcessInfoID,
                            "param": pil.Param,
                            "minValue": pil.MinValue,
                            "maxValue": pil.MaxValue
                        };
                    })
                };
            };

            var fillProcessInfo = function (data) {
                $(".add-process-info-name", $this).val("");
                $(".add-param-name", $this).val("");
                $(".add-value-name", $this).val("");
                $(".add-special", $this).val("");
                $(".add-allow-none", $this).prop("checked", false);
                $(".add-require-value", $this).prop("checked", false);
                $(".add-require-selection", $this).prop("checked", false);

                var model = createProcessInfoModel(data);

                var html = Handlebars.templates.processinfo(model);
                $this.html(html.replace(/[\uFEFF]/g, ''));

                // add data to each process-info element
                $(".process-info", $this).each(function () {
                    var pinfo = $(this);
                    var processInfoId = pinfo.data("process-info-id");

                    var arr = data.filter(x => x.ProcessInfoID === processInfoId);
                    var item = arr.length > 0 ? arr[0] : null;
                    pinfo.data("item", item);

                    var parent = $(".process-info-line-parent[data-process-info-id='" + processInfoId + "']", $this);
                    fillProcessInfoLine(parent, item.Lines);
                });
            };

            var fillProcessInfoLine = function (parent, data) {
                $(".add-param", parent).val("");
                $(".add-min-value", parent).val("");
                $(".add-max-value", parent).val("");

                var model = createProcessInfoLineModel(data);

                var html = Handlebars.templates.processinfoline(model);
                $(".process-info-lines", parent).html(html.replace(/[\uFEFF]/g, ''));

                $(".process-info-line", parent).each(function () {
                    var pil = $(this);
                    var processInfoLineId = pil.data("process-info-line-id");
                    var arr = data.filter(x => x.ProcessInfoLineID === processInfoLineId);
                    var item = arr.length > 0 ? arr[0] : null;
                    pil.data("item", item);
                });
            };

            var validateProcessInfo = function (pinfo) {
                if (pinfo.ProcessInfoName + pinfo.ParamName === "") {
                    showError("Process Info Name and Param Name cannot both be blank.");
                    return false;
                }

                return true;
            };

            var validateProcessInfoLine = function (pil) {
                if (pil.Param === "") {
                    showError("Param cannot be blank.");
                    return false;
                }

                return true;
            };

            var handleExpand = function (pinfo) {
                handleProcessInfoCancel();
                pinfo.addClass("expanded");
                var item = pinfo.data("item");
                getProcessInfoLineParent(item.ProcessInfoID).addClass("expanded");
            };

            var handleCollapse = function (pinfo) {
                handleProcessInfoCancel();
                pinfo.removeClass("expanded");
            };

            var handleMoveUp = function (pinfo) {
                handleProcessInfoCancel();
                var item = pinfo.data("item");
                api.moveUp(item.ResourceID, item.ProcessInfoID).done(fillProcessInfo);
            };

            var handleMoveDown = function (pinfo) {
                handleProcessInfoCancel();
                var item = pinfo.data("item");
                api.moveDown(item.ResourceID, item.ProcessInfoID).done(fillProcessInfo);
            };

            var handleProcessInfoEdit = function (pinfo) {
                handleProcessInfoCancel();

                var item = pinfo.data("item");

                pinfo.addClass("editing");

                $(".process-info-name", pinfo)
                    .html($("<input/>", { "type": "text", "class": "edit-process-info-name", "maxlength": 50, "value": item.ProcessInfoName }).attr("size", 10));

                $(".param-name", pinfo)
                    .html($("<input/>", { "type": "text", "class": "edit-param-name", "maxlength": 50, "value": item.ParamName }).attr("size", 10));

                $(".value-name", pinfo)
                    .html($("<input/>", { "type": "text", "class": "edit-value-name", "maxlength": 50, "value": item.ValueName }).attr("size", 10));

                $(".special", pinfo)
                    .html($("<input/>", { "type": "text", "class": "edit-special", "maxlength": 50, "value": item.Special }).attr("size", 10));

                $(".allow-none", pinfo)
                    .html($("<input/>", { "type": "checkbox", "class": "edit-allow-none" }).prop("checked", item.AllowNone));

                $(".require-value", pinfo)
                    .html($("<input/>", { "type": "checkbox", "class": "edit-require-value" }).prop("checked", item.RequireValue));

                $(".require-selection", pinfo)
                    .html($("<input/>", { "type": "checkbox", "class": "edit-require-selection" }).prop("checked", item.RequireSelection));
            };

            var handleProcessInfoDelete = function (pinfo) {
                handleProcessInfoCancel();
                var item = pinfo.data("item");
                api.deleteProcessInfo(item.ResourceID, item.ProcessInfoID).done(fillProcessInfo);
            };

            var handleProcessInfoSave = function () {
                closeProcessInfoLines();

                var editing = $(".process-info.editing", $this);

                if (editing.length > 0) {
                    var item = editing.data("item");

                    var model = {
                        ProcessInfoID: item.ProcessInfoID,
                        ResourceID: item.ResourceID,
                        ProcessInfoName: $(".edit-process-info-name", editing).val(),
                        ParamName: $(".edit-param-name", editing).val(),
                        ValueName: $(".edit-value-name", editing).val(),
                        Special: $(".edit-special", editing).val(),
                        AllowNone: $(".edit-allow-none", editing).prop("checked"),
                        Order: item.Order,
                        RequireValue: $(".edit-require-value", editing).prop("checked"),
                        RequireSelection: $(".edit-require-selection", editing).prop("checked"),
                        MaxAllowed: item.MaxAllowed,
                        Deleted: item.Deleted
                    };

                    if (validateProcessInfo(model)) {
                        api.updateProcessInfo(model).done(fillProcessInfo);
                    }
                }
            };

            var handleProcessInfoCancel = function () {
                closeProcessInfoLines();

                var editing = $(".process-info.editing", $this);

                if (editing.length > 0) {
                    editing.removeClass("editing");

                    var item = editing.data("item");

                    $(".process-info-name", editing).html(item.ProcessInfoName);
                    $(".param-name", editing).html(item.ParamName);
                    $(".value-name", editing).html(item.ValueName);
                    $(".special", editing).html(item.Special);
                    $(".allow-none", editing).html(item.AllowNone ? "True" : "False");
                    $(".require-value", editing).html(item.RequireValue ? "True" : "False");
                    $(".require-selection", editing).html(item.RequireSelection ? "True" : "False");
                }
            };

            var handleProcessInfoLineEdit = function (pil) {
                var item = pil.data("item");

                pil.addClass("editing");

                $(".param", pil)
                    .html($("<input/>", { "type": "text", "class": "edit-param", "maxlength": 50, "value": item.Param }).attr("size", 10));

                $(".min-value", pil)
                    .html($("<input/>", { "type": "text", "class": "edit-min-value", "maxlength": 50, "value": item.MinValue }).attr("size", 10));

                $(".max-value", pil)
                    .html($("<input/>", { "type": "text", "class": "edit-max-value", "maxlength": 50, "value": item.MaxValue }).attr("size", 10));
            };

            var handleProcessInfoLineDelete = function (pil) {
                var item = pil.data("item");
                var parent = pil.closest(".process-info-line-parent");

                api.deleteProcessInfoLine(opts.resourceId, item.ProcessInfoID, item.ProcessInfoLineID).done(function (data) {
                    fillProcessInfoLine(parent, data);
                });
            };

            var handleProcessInfoLineSave = function () {
                var editing = $(".process-info-line.editing", $this);

                if (editing.length > 0) {
                    var item = editing.data("item");

                    var model = {
                        ProcessInfoLineID: item.ProcessInfoLineID,
                        ProcessInfoID: item.ProcessInfoID,
                        Param: $(".edit-param", editing).val(),
                        MinValue: parseInt($(".edit-min-value", editing).val()) || 0,
                        MaxValue: parseInt($(".edit-max-value", editing).val()) || 0,
                        Deleted: item.Deleted
                    };

                    if (validateProcessInfoLine(model)) {
                        var parent = editing.closest(".process-info-line-parent");
                        api.updateProcessInfoLine(opts.resourceId, model).done(function (data) {
                            fillProcessInfoLine(parent, data);
                        });
                    }
                }
            };

            var handleProcessInfoLineCancel = function () {
                var editing = $(".process-info-line.editing", $this);

                if (editing.length > 0) {
                    editing.removeClass("editing");

                    var item = editing.data("item");

                    $(".param", editing).html(item.Param);
                    $(".min-value", editing).html(item.MinValue);
                    $(".max-value", editing).html(item.MaxValue);
                }
            };

            var handleProcessInfoAdd = function () {
                var model = {
                    ProcessInfoID: 0,
                    ResourceID: opts.resourceId,
                    ProcessInfoName: $(".add-process-info-name", $this).val(),
                    ParamName: $(".add-param-name", $this).val(),
                    ValueName: $(".add-value-name", $this).val(),
                    Special: $(".add-special", $this).val(),
                    AllowNone: $(".add-allow-none", $this).prop("checked"),
                    Order: 0,
                    RequireValue: $(".add-require-value", $this).prop("checked"),
                    RequireSelection: $(".add-require-selection", $this).prop("checked"),
                    MaxAllowed: 1,
                    Deleted: false
                };

                if (validateProcessInfo(model)) {
                    api.addProcessInfo(model).done(function (data) {
                        fillProcessInfo(data);
                        var last = $(".process-info-table .process-info", $this).last();
                        handleExpand(last);
                    });
                }
            };

            var handleProcessInfoLineAdd = function (table) {
                var parent = table.closest(".process-info-line-parent");
                var processInfoId = parent.data("process-info-id");

                var model = {
                    ProcessInfoLineID: 0,
                    ProcessInfoID: processInfoId,
                    Param: $(".add-param", table).val(),
                    MinValue: parseInt($(".add-min-value", table).val()) || 0,
                    MaxValue: parseInt($(".add-max-value", table).val()) || 0,
                    Deleted: false
                };

                if (validateProcessInfoLine(model)) {
                    api.addProcessInfoLine(opts.resourceId, model).done(function (data) {
                        fillProcessInfoLine(parent, data);
                    });
                }
            };

            // this is the starting point
            api.getProcessInfo(opts.resourceId).done(function (data) {
                fillProcessInfo(data);
            });

            $this.on("click", ".add-process-info-button", function (e) {
                handleProcessInfoAdd();
            }).on("click", ".add-process-info-line-button", function (e) {
                var table = $(e.currentTarget).closest(".process-info-table");
                handleProcessInfoLineAdd(table);
            }).on("click", ".process-info .expand-button", function (e) {
                e.preventDefault();
                var pinfo = getProcessInfo(e);
                handleExpand(pinfo);
            }).on("click", ".process-info .collapse-button", function (e) {
                e.preventDefault();
                var pinfo = getProcessInfo(e);
                handleCollapse(pinfo);
            }).on("click", ".process-info .move-up-button", function (e) {
                e.preventDefault();
                var pinfo = getProcessInfo(e);
                handleMoveUp(pinfo);
            }).on("click", ".process-info .move-down-button", function (e) {
                e.preventDefault();
                var pinfo = getProcessInfo(e);
                handleMoveDown(pinfo);
            }).on("click", ".process-info .edit-button", function (e) {
                e.preventDefault();
                var pinfo = getProcessInfo(e);
                handleProcessInfoEdit(pinfo);
            }).on("click", ".process-info .delete-button", function (e) {
                e.preventDefault();
                var pinfo = getProcessInfo(e);
                handleProcessInfoDelete(pinfo);
            }).on("click", ".process-info .save-button", function (e) {
                e.preventDefault();
                handleProcessInfoSave();
            }).on("click", ".process-info .cancel-button", function (e) {
                e.preventDefault();
                handleProcessInfoCancel();
            }).on("click", ".process-info-line .edit-button", function (e) {
                e.preventDefault();
                var pil = getProcessInfoLine(e);
                handleProcessInfoLineEdit(pil);
            }).on("click", ".process-info-line .delete-button", function (e) {
                e.preventDefault();
                var pil = getProcessInfoLine(e);
                handleProcessInfoLineDelete(pil);
            }).on("click", ".process-info-line .save-button", function (e) {
                e.preventDefault();
                handleProcessInfoLineSave();
            }).on("click", ".process-info-line .cancel-button", function (e) {
                e.preventDefault();
                handleProcessInfoLineCancel();
            }).on("getProcessInfo", function (e, processInfoId, cb) {
                if (typeof cb === "function") {
                    var pinfo = null;

                    if (isNumber(processInfoId))
                        pinfo = getProcessInfo(processInfoId);

                    cb(pinfo);
                }
            }).on("getProcessInfoLine", function (e, processInfoLineId, cb) {
                if (typeof cb === "function") {
                    var pil = null;

                    if (isNumber(processInfoLineId))
                        pil = getProcessInfoLine(processInfoLineId);

                    cb(pil);
                }
            }).on("expand", function (e, processInfoId) {
                if (isNumber(processInfoId)) {
                    var pinfo = getProcessInfo(processInfoId);
                    if (pinfo.length > 0)
                        handleExpand(pinfo);
                }
            }).on("collapse", function (e, processInfoId) {
                if (isNumber(processInfoId)) {
                    var pinfo = getProcessInfo(processInfoId);
                    if (pinfo.length > 0)
                        handleCollapse(pinfo);
                }
            });
        });
    };
})(jQuery);