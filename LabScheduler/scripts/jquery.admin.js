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
    var baseUrl = function () { return $('.base-url').val(); }
    var ajaxUrl = function () { return $('.ajax-url').val(); }

    $.fn.adminResourceList = function () {
        return this.each(function () {
            var $this = $(this);

            var oTable = $('.resource-table', $this).dataTable({
                'bStateSave': true,
                'aaSorting': [[5, 'asc']],
                'aoColumnDefs': [
                        { 'bSortable': false, 'bSearchable': false, 'aTargets': [0] }
                ],
                'fnInitComplete': function (oSettings, json) {
                    $('.resource-list-working').hide();
                    $('.resource-table-container', $this).css({ 'visibility': 'visible' });
                }
            });

            $this.on('click', '.delete-resource-link', function (event) {
                var ask = confirm("Are you sure you want to delete this resource?");
                if (!ask) event.preventDefault();
            }).on('click', '.add-resource-button', function (event) {
                window.location = baseUrl() + '?Command=edit&ResourceID=0';
            });
        });
    }

    $.fn.adminResourceEditForm = function () {
        return this.each(function () {
            var $this = $(this);

            var resourceId = $('.resource-id', $this).val();
            var proctechId = $('.proctech-id', $this).val();
            var labId = $('.lab-id', $this).val();
            var buildingId = $('.building-id', $this).val();

            var loadBuildings = function (callback) {
                $.getJSON("/services/scheduler/building", function (data) {
                    if ($.isArray(data)) {
                        var select = $('.building-select', $this);
                        select.find("option").remove();
                        $.each(data, function (index, item) {
                            if (item.IsActive)
                                select.append($("<option/>").val(item.BuildingID).prop("checked", item.BuildingID == buildingId).text(item.BuildingName));
                        });
                    }
                    if (typeof callback == 'function')
                        callback(select.val());
                });
            }

            var loadLabs = function (id, callback) {
                $.getJSON("/services/scheduler/building/" + id + "/labs", function (data) {
                    if ($.isArray(data)) {
                        var select = $('.lab-select', $this);
                        select.find("option").remove();
                        $.each(data, function (index, item) {
                            if (item.IsActive)
                                select.append($("<option/>").val(item.LabID).prop("checked", item.LabID == labId).text(item.LabName));
                        });
                    }
                    if (typeof callback == 'function')
                        callback(select.val());
                });
            }

            var loadProcessTechs = function (id) {
                $.getJSON("/services/scheduler/lab/" + id + "/proctechs", function (data) {
                    if ($.isArray(data)) {
                        var select = $('.proctech-select', $this);
                        select.find("option").remove();
                        $.each(data, function (index, item) {
                            if (item.IsActive)
                                select.append($("<option/>").val(item.ProcessTechID).prop("checked", item.ProcessTechID == proctechId).text(item.ProcessTechName));
                        });
                    }
                });
            }

            var deleteToolEngineer = function (item, callback) {
                console.log(item.ResourceClientID);
                $.ajax({
                    "type": "DELETE",
                    "url": "/services/scheduler/resource-client/" + item.ResourceClientID,
                    "success": function (data) {
                        if (typeof callback == "function")
                            callback();
                    },
                    "error": function (err) {
                        alert(err.statusText);
                    }
                });
            }

            var addToolEngineer = function () {
                var select = $('.tool-engineer-select', $this);
                var clientId = select.val();
                var item = { ClientID: clientId, ResourceID: resourceId, AuthLevel: 16 };
                $.post("/services/scheduler/resource-client/set-auth-level", item, function (data) {
                    loadToolEngineers();
                });
            }

            var toolEngineerControl = function (item) {
                var result = $("<a/>").attr("href", "#").append(
                    $("<img/>").attr("alt", "delete").attr("border", "0").attr("src", baseUrl().replace("AdminResources.aspx", "images/delete.gif"))
                ).click(function (e) {
                    e.preventDefault();
                    deleteToolEngineer(item, loadToolEngineers)
                });

                return result;
            }

            var toolEngineerRow = function (item) {
                var result = $("<tr/>").append(
                    $("<td/>").html(item.DisplayName).addClass('tool-engineer-display-name')
                ).append(
                    $("<td/>").append(toolEngineerControl(item)).addClass('tool-engineer-action')
                );
                return result;
            }

            var loadToolEngineers = function () {
                var table = $('.tool-engineer-table', $this);
                if (table.length > 0) {
                    $.getJSON("/services/scheduler/resource/" + resourceId + "/tooleng", function (data) {
                        if ($.isArray(data)) {
                            table.find("tbody tr").remove();
                            $.each(data, function (index, item) {
                                table.find("tbody").append(toolEngineerRow(item));
                            });
                        }
                    });
                }
            }

            var loadStaff = function () {
                $.getJSON("/services/scheduler/resource/" + resourceId + "/tooleng/available", function (data) {
                    if ($.isArray(data)) {
                        var select = $('.tool-engineer-select', $this);
                        select.find("option").remove();
                        $.each(data, function (index, item) {
                            select.append($("<option/>").val(item.ClientID).text(item.DisplayName));
                        });
                    }
                });
            }

            loadBuildings(function (buildingId) {
                loadLabs(buildingId, function (labId) {
                    loadProcessTechs(labId);
                });
            });

            loadToolEngineers();

            loadStaff();

            $this.on('change', '.building-select', function (e) {
                var buildingId = $('.building-select', $this).val();
                loadLabs(buildingId, function (labId) {
                    loadProcessTechs(labId);
                });
            }).on('change', '.lab-select', function (e) {
                var labId = $('.lab-select', $this).val();
                loadProcessTechs(labId);
            }).on('click', '.add-tool-engineer', function(e){
                addToolEngineer();
            });
        });
    }

    //$.fn.adminResourceEditForm = function () {
    //    return this.each(function () {
    //        var $this = $(this);

    //        var resourceId = $('.resource-id', $this).val();

    //        var appendError = function (target, message) {
    //            target.append($('<div/>').html(message).addClass('error'));
    //        }

    //        var makeRequest = function (options) {
    //            $.ajax({
    //                'url': ajaxUrl(),
    //                'type': 'POST',
    //                'dataType': 'json',
    //                'data': options.data,
    //                'success': function (json) {
    //                    if (!json.Success)
    //                        appendError(options.errorTarget, json.Message);
    //                    if (typeof options.success == 'function')
    //                        options.success(json);
    //                },
    //                'error': function (err) {
    //                    appendError(options.errorTarget, err.statusText);
    //                    if (typeof options.error == 'function')
    //                        options.error(err);
    //                }
    //            });
    //        }

    //        var createStaffSelect = function (staff) {
    //            var result = $('<select class="tool-engineer-select"/>');
    //            $.each(staff, function (index, item) {
    //                result.append(
    //                        $('<option/>')
    //                            .attr('value', item.ClientID)
    //                            .html(item.DisplayName)
    //                    );
    //            });
    //            return result;
    //        }

    //        var deleteToolEngineer = function (clientId, callback) {
    //            makeRequest({
    //                'data': { 'Action': 'delete-tool-engineer', 'ResourceID': resourceId, 'ClientID': clientId },
    //                'errorTarget': $('.tool-engineer-error', $this),
    //                'success': function (json) {
    //                    if (typeof callback == 'function')
    //                        callback(json.Data);
    //                }
    //            });
    //        }

    //        var addToolEngineer = function (clientId, callback) {
    //            makeRequest({
    //                'data': { 'Action': 'add-tool-engineer', 'ResourceID': resourceId, 'ClientID': clientId },
    //                'errorTarget': $('.tool-engineer-error', $this),
    //                'success': function (json) {
    //                    if (json.Success) {
    //                        if (typeof callback == 'function')
    //                            callback(json.Data);
    //                    }
    //                }
    //            });
    //        }

    //        var fillToolEngineerTable = function (table, data) {
    //            table.find('tr').remove();
    //            $.each(data.ToolEngineers, function (index, item) {
    //                table.append(
    //                        $('<tr/>').append(
    //                            $('<td/>')
    //                                .html(item.DisplayName)
    //                                .addClass('tool-engineer-display-name')
    //                        ).append(
    //                            $('<td/>')
    //                                .append(
    //                                    $('<a href="#"/>').append(
    //                                        $('<img alt="delete" border="0" />').attr('src', baseUrl().replace('AdminResources.aspx', 'images/delete.gif'))
    //                                    ).click(function (event) {
    //                                        event.preventDefault();
    //                                        deleteToolEngineer(item.ClientID, function (data) {
    //                                            fillToolEngineerTable(table, data);
    //                                        });
    //                                    })
    //                                ).addClass('tool-engineer-action')
    //                        )
    //                    );
    //            });
    //            if (data.Staff.length > 0) {
    //                table.append(
    //                $('<tr/>').append(
    //                    $('<td/>').append(
    //                        createStaffSelect(data.Staff)
    //                    )
    //                ).append(
    //                    $('<td/>').append(
    //                        $('<input type="button" value="Add" />').click(function () {
    //                            var cid = $('.tool-engineer-select', $this).val()
    //                            addToolEngineer(cid, function (data) {
    //                                fillToolEngineerTable(table, data);
    //                            });
    //                        })
    //                    ).addClass('tool-engineer-action')
    //                ).addClass('footer'));
    //            }
    //        }

    //        var loadToolEngineers = function () {
    //            var table = $('.tool-engineer-table', $this);
    //            if (table.length > 0) {
    //                makeRequest({
    //                    'data': { 'Action': 'get-tool-engineers', 'ResourceID': resourceId },
    //                    'errorTarget': $('.tool-engineer-error', $this),
    //                    'success': function (json) {
    //                        if (json.Success) {
    //                            fillToolEngineerTable(table, json.Data);
    //                        }
    //                    }
    //                });
    //            }
    //        }

    //        var validate = function (data) {
    //            var result = true;
    //            $('.validation-message', $this).html('');
    //            data.BuildingID = $('.building-select', $this).val();
    //            data.LabID = $('.lab-select', $this).val();
    //            data.ProcessTechID = $('.proctech-select', $this).val();
    //            data.ResourceID = $('.resource-id', $this).val();
    //            data.EditResourceID = $('.resource-id-textbox', $this).val();
    //            data.ResourceName = $('.resource-name-textbox', $this).val();
    //            data.Schedulable = $('.schedulable-checkbox input[type="checkbox"]').is(':checked');
    //            data.Active = $('.active-checkbox input[type="checkbox"]').is(':checked');
    //            data.Description = $('.resource-description-textarea', $this).val();
    //            data.HelpdeskEmail = $('.resource-helpdesk-textbox', $this).val();
    //            if (data.EditResourceID == '') {
    //                appendError($('.validation-message', $this), 'Resource ID must not be blank.');
    //                result = false;
    //            }
    //            else if (parseInt(data.EditResourceID) != data.EditResourceID) {
    //                appendError($('.validation-message', $this), 'Resource ID must be an integer.');
    //                result = false;
    //            }
    //            if (data.ResourceName == '') {
    //                appendError($('.validation-message', $this), 'Resource Name must not be blank.');
    //                result = false;
    //            }
    //            return result;
    //        }

    //        var addResource = function (callbacks) {
    //            var data = { 'Action': 'add-resource' };
    //            if (validate(data)) {
    //                makeRequest({
    //                    'data': data,
    //                    'errorTarget': $('.validation-message', $this),
    //                    'success': function (json) {
    //                        if (typeof callbacks.success == 'function')
    //                            callbacks.success(json);
    //                    },
    //                    'error': function (err) {
    //                        if (typeof callbacks.error == 'function')
    //                            callbacks.error(err);
    //                    }
    //                });
    //            }
    //        }

    //        var updateResource = function (callbacks) {
    //            var data = { 'Action': 'update-resource' };
    //            if (validate(data)) {
    //                makeRequest({
    //                    'data': data,
    //                    'errorTarget': $('.validation-message', $this),
    //                    'success': function (json) {
    //                        if (typeof callbacks.success == 'function')
    //                            callbacks.success(json);
    //                    },
    //                    'error': function (err) {
    //                        if (typeof callbacks.error == 'function')
    //                            callbacks.error(err);
    //                    }
    //                });
    //            }
    //        }

    //        var fillSelect = function (args) {
    //            args.select.find('option').remove();
    //            $.each(args.data, function (index, item) {
    //                var opt = $('<option/>')
    //                    .attr('value', args.value(item))
    //                    .html(args.text(item));
    //                //if (item.Selected)
    //                //    opt.prop('selected', true);
    //                args.select.append(opt);
    //            });
    //            if (typeof args.callback == 'function')
    //                args.callback(args.select);
    //        }

    //        var loadBuildings = function (rID, callback) {
    //            $.ajax({
    //                'url': "/services/scheduler/building",
    //                'type': 'GET',
    //                'dataType': 'json',
    //                'success': function (json) {
    //                    fillSelect({
    //                        'select': $('.building-select', $this),
    //                        'data': json,
    //                        'value': function (item) {
    //                            return item.BuildingID;
    //                        },
    //                        'text': function (item) {
    //                            return item.BuildingName;
    //                        },
    //                        'callback': function (select) {
    //                            console.log(select);
    //                            if (typeof callback == 'function')
    //                                callback(select.val());
    //                        }
    //                    });
    //                },
    //                'error': function (err) {
    //                    alert(err.statusText);
    //                }
    //            });
    //        }

    //        var loadLabs = function (rID, buildingId, callback) {
    //            makeRequest({
    //                'data': { 'Action': 'get-labs', 'ResourceID': rID, 'BuildingID': buildingId },
    //                'errorTarget': $('.general-error'),
    //                'success': function (json) {
    //                    fillSelect($('.lab-select', $this), json.Data, function (select) {
    //                        if (typeof callback == 'function')
    //                            callback(select.val());
    //                    });
    //                }
    //            });
    //        }

    //        var loadProcTechs = function (rID, labId, callback) {
    //            makeRequest({
    //                'data': { 'Action': 'get-proctechs', 'ResourceID': rID, 'LabID': labId },
    //                'errorTarget': $('.general-error'),
    //                'success': function (json) {
    //                    if (json.Success) {
    //                        fillSelect($('.proctech-select', $this), json.Data, function (select) {
    //                            if (typeof callback == 'function')
    //                                callback(select.val());
    //                        });
    //                    }
    //                }
    //            });
    //        }

    //        var uploadFile = function () {
    //            $('.tool-image-error', $this).html('');
    //            var form = $('.image-upload', $this).closest('form');
    //            var origAction = form.attr('action');
    //            var origTarget = form.attr('target');
    //            var origEnctype = form.attr('enctype');
    //            var frame = $('<iframe name="uploadFrame" frameborder="0"/>')
    //                .css({ 'width': '1px', 'height': '1px', 'visibility': 'hidden', 'display': 'none' })
    //                .appendTo($this)
    //                .load(function () {
    //                    var f = $(this);
    //                    var contents = f.contents().text();
    //                    try {
    //                        var json = $.parseJSON(contents);

    //                        if (json.Success)
    //                            $('.resource-image', $this).attr('src', json.Data.IconUrl + '?time=' + new Date().getTime());
    //                        else
    //                            appendError($('.tool-image-error', $this), json.Message);

    //                        setTimeout(function () {
    //                            f.remove();
    //                            $('.image-upload', $this).val('');
    //                        }, 250);
    //                    }
    //                    catch (err) {
    //                        f.css({
    //                            'width': '498px',
    //                            'height': '300px',
    //                            'visibility': 'visible',
    //                            'display': 'block',
    //                            'border': 'solid 1px #AAAAAA',
    //                            'margin-top': '10px'
    //                        });
    //                    }
    //                    finally {
    //                        form.attr('action', origAction)
    //                            .attr('target', origTarget)
    //                            .attr('enctype', origEnctype)
    //                            .find('.upload-param').remove();
    //                    }
    //                });

    //            form.attr('action', ajaxUrl())
    //                .attr('target', 'uploadFrame')
    //                .attr('enctype', 'multipart/form-data')
    //                .append($('<input type="hidden" name="Action" class="upload-param" value="upload-image"/>'))
    //                .append($('<input type="hidden" name="ResourceID" class="upload-param"/>').val(resourceId))
    //                .append($('<input type="hidden" name="Path" class="upload-param" value="Resource"/>'))
    //                .submit();
    //        }

    //        loadToolEngineers();
    //        loadBuildings(resourceId, function (buildingId) {
    //            loadLabs(resourceId, buildingId, function (labId) {
    //                loadProcTechs(resourceId, labId);
    //            });
    //        });


    //        $this.on('click', '.edit-cancel-button', function (event) {
    //            var container = $(this).closest('div');
    //            var origHtml = container.html();
    //            container.html('<span class="nodata">Working...</span>');
    //            window.location = baseUrl();
    //        }).on('click', '.edit-add-button', function (event) {
    //            var container = $(this).closest('div');
    //            var origHtml = container.html();
    //            container.html('<span class="nodata">Working...</span>');
    //            addResource({
    //                'success': function (json) {
    //                    if (json.Success)
    //                        window.location = baseUrl() + '?Command=edit&ResourceID=' + json.Data.ResourceID;
    //                    else
    //                        container.html(origHtml);
    //                },
    //                'error': function (err) {
    //                    container.html(origHtml);
    //                }
    //            });
    //        }).on('click', '.edit-add-another-button', function (event) {
    //            var container = $(this).closest('div');
    //            var origHtml = container.html();
    //            container.html('<span class="nodata">Working...</span>');
    //            addResource({
    //                'success': function (json) {
    //                    if (json.Success)
    //                        window.location = baseUrl() + '?Command=edit&ResourceID=0';
    //                    else
    //                        container.html(origHtml);
    //                },
    //                'error': function (err) {
    //                    container.html(origHtml);
    //                }
    //            });
    //        }).on('click', '.edit-update-button', function (event) {
    //            var container = $(this).closest('div');
    //            var origHtml = container.html();
    //            container.html('<span class="nodata">Working...</span>');
    //            updateResource({
    //                'success': function (json) {
    //                    if (json.Success)
    //                        window.location = baseUrl();
    //                    else
    //                        container.html(origHtml);
    //                },
    //                'error': function (err) {
    //                    container.html(origHtml);
    //                }
    //            });
    //        }).on('click', '.edit-upload-button', function (event) {
    //            uploadFile();
    //        }).on('change', '.building-select', function (event) {
    //            var buildingId = $(this).val();
    //            loadLabs(0, buildingId, function (labId) {
    //                loadProcTechs(labId);
    //            });
    //        }).on('change', '.lab-select', function (event) {
    //            var labId = $(this).val();
    //            loadProcTechs(0, labId);
    //        });
    //    });
    //}
}(jQuery));