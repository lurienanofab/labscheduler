var pis, pils, rpis;
var initProcessInfo = function () {
    if (!$("#divPI").length) {  // $("#divPI") dont exist in this page return, (divPI wont be there in Confirmation page)
        return;
    }
    var resourceId = $('.resource-id').val();

    var url_pi = "/webapi/scheduler/process-info-details/resource/" + resourceId + "/reservation/" + getReservationID();
    $.ajax({ "url": url_pi, "type": "GET", "data": { "command": "" }, "dataType": "json" }).done(function (data) {
        if (data.Error)
        { console.log("processinfo.js pis ajax error:", data); }
        else
        {
            var js_p = JSON.parse(data);
            pis = js_p.ProcessInfos;
            pils = js_p.ProcessInfoLines;
            rpis = js_p.ReservationProcessInfos;
            generateUIFromRPIs(rpis);
            $('.imgResLoader').hide();
            $(".Button.btnSubmitRes").removeAttr("disabled").removeClass("tdisable");
        }
    }).fail(function (err) { });
};
var submitPIJsonDataAndRedirect = function () {
    // create processinfos with ajax webapi call
    var url_pi = "/webapi/scheduler/reservation-process-infos-save/";
    $.ajax({ "url": url_pi, "type": "GET", "data": { "jsonrpis": rpis }, "dataType": "json" }).done(function (data) {
        if (data.Error)
        { console.log("processinfo.js save pis ajax error:", data);}
        else
        {

        }
    }).fail(function (err) { });
    // and redirect
    //window.location.href = $('.hidRedirectPath').val();
};
var getReservationID = function () {
    return $('.reservation-id').val();
};
//-------------------------------------------------
var generateUIFromRPIs = function (rpis) {
    var tableContent = "";
    var templatePIHDData = _.template("<tr class='thbottombordercolor' runNumber='<%= RunNumber%>' >  <th></th><th></th>    <th> Run no- <%= RunNumber + 1 %></th>    <th></th>"
                           + "<th>Run Count: <input type='text' style='width:40px;' class='txtRunCount' value='<%= RunCountValue %>' />&nbsp; <img src='images/deleteGrid.gif' class='imgDeleteRun' /> </th> </tr>");
    var templatePIData = _.template("<tr><td></td><td> <%= ParamName %> </td><td> <%= ValueName %> </td></tr>"
                         + "<tr rpi_id='<%= RPIID%>' ><td></td><td> <%= ProcessInfoName %> </td><td> <%= ProcessInfoDropDown %> </td><td> <input type='text' class='txtPil' value='<%= Value %>' /></td>"
                         + "<td><%= CancelOrAddButton %> </td></tr>");

    var unique_runs_nums = getUniqueByProperty(rpis, "RunNumber");  // this only returns the run numbers not RPI objects

    _.each(unique_runs_nums, function (rnum, rindex, unique_runs_nums) {

        var rpis_objs_for_this_run = _.where(rpis, { "RunNumber": rnum, "Active":true });
        console.log("--1--", rpis_objs_for_this_run, "<--rpis_objs_for_this_run-", rnum);

        var unique_by_piIds = getUniqueByProperty(rpis_objs_for_this_run, "ProcessInfoID");
        //console.log("--2--", unique_by_piIds, "<--unique_by_piIds -|");

        tableContent += "<table class='pi'> ";
        tableContent += templatePIHDData({
            RunNumber: rnum,
            RunCountValue: '1'
        });

        _.each(unique_by_piIds, function (piid, pindex, unique_by_piIds) {
            var pi = _.findWhere(pis, { "ProcessInfoID": piid })

            var rpis_by_pis = _.where(rpis_objs_for_this_run, {"ProcessInfoID": piid});
            //console.log("==3==", rpis_by_pis, "-----", pi, piid);
            
            _.each(rpis_by_pis, function (rpis_x, currentIndex, rpis_by_pis) {
                var pil = _.findWhere(pils, { "ProcessInfoLineID": rpis_x.ProcessInfoLineID });
                //---
                tableContent += templatePIData({
                    RPIID: rpis_x.ReservationProcessInfoID,
                    ParamName: pi.ParamName,
                    ValueName: pi.ValueName,
                    ProcessInfoName: pi.ProcessInfoName,
                    ProcessInfoDropDown: createDropDownForPIL(rpis_x),
                    Value: rpis_x.Value,
                    CancelOrAddButton: createDeleteOrAddButton(rpis_by_pis, currentIndex, rpis_x)
                });
            });
            tableContent += "<tr class='trbottombordercolor'> <td></td></tr>";
            //console.log("-----------------------------------------", unique_by_piIds);
        });
        tableContent += "</table>";
    });

    tableContent += "<input type='Button' class='btnAddRun' value='Add Run' > </input>";
    $("#divPI").html(tableContent);

    addEventListeners();
    console.log("--------- UI generation done -----------");
}
var addEventListeners = function () {
    // submit button listener
    $(".btnSubmitRes").click(function () {
        var str_rpis = JSON.stringify(rpis);      //reading jQuery.parseJSON($('#hidProcessInfoData').val())
        $(".hidProcessInfoData").val(str_rpis);
        console.log($(".btnSubmitRes").val(), "<===");
    });

    $(".ddlPil").change(function () {
        setRPIValue($(this), "ProcessInfoLineID", this.value);
    });
    $('.txtPil').bind('input', function () {
        setRPIValue($(this), "Value", this.value);
    });
    $(".btnAddProcessInfo").click(function () {
        var duplicate_rpi = _.chain(rpis).findWhere({ "ReservationProcessInfoID": getRPIIDFromRow($(this)) }).clone().value();
        rpis.push(duplicate_rpi);
        generateUIFromRPIs(rpis);
    });
    $(".btnDeleteProcessInfo").click(function () {
        //setRPIValue($(this), "Active", false);
        removeRPI($(this));
        $(this).closest('tr').prev().remove();
        $(this).closest('tr').remove();
        //generateUIFromRPIs(rpis);
    });
    $(".btnAddRun").click(function () {
        console.log('Adding processRuns');
        var currentRunNumber = _.max(getUniqueByProperty(rpis, "RunNumber"));
        addNewRPIsBasedonStructure(pis, currentRunNumber + 1);
        generateUIFromRPIs(rpis);
    });
    $(".imgDeleteRun").click(function () {
        var runNumber = getAttributeValueFromRow($(this), "RunNumber");
        deleteRpisBasedonRunNumber(runNumber);
        console.log("=====3");  // if there is only one run in the list disable the delete butto.n
        generateUIFromRPIs(rpis);
    });
};
var deleteRpisBasedonRunNumber = function(runNumber){
    rpis = _.filter(rpis, function (rpi) { return rpi.RunNumber != runNumber; });  // this will replace rpis with new rpis after filtering
};
var addNewRPIsBasedonStructure = function (pis, runNumber) {
    _.each(pis, function (pi, index, pis) {
        var newRPI = { "ProcessInfoID": pi.ProcessInfoID, "ReservationProcessInfoID": getDummyRPIID(), "ReservationID": getReservationID(), "ProcessInfoLineID": -1, "Value": 0, "Special": 0, "RunNumber": runNumber, "ChargeMultiplier": 0, "Active": true };
        rpis.push(newRPI);
    });
};
var currentDummyRpiid = -12345; // this can be random number which only used in segregating the processinfo UI elements 
var getDummyRPIID = function () {
    return currentDummyRpiid += 1;
};
var getAttributeValueFromRow = function (src, attr) {
    return parseInt($(src).closest('tr').attr(attr));
};
var getRPIIDFromRow = function (src ) {
    return getAttributeValueFromRow(src, 'rpi_id');
};
var removeRPI = function (src) {
    var rpi_id = getRPIIDFromRow(src);
    rpis = _.without(rpis, _.findWhere(rpis, { "ReservationProcessInfoID": rpi_id }) );
};
var setRPIValue = function (src, propName, value) {
    var rpi_id = getRPIIDFromRow(src);
    _.findWhere(rpis, {"ReservationProcessInfoID": rpi_id})[propName] = value;
    console.log(rpi_id, "  ,   ", _.findWhere(rpis, { "ReservationProcessInfoID": rpi_id })[propName]);
};
var createDeleteOrAddButton = function (rpis_by_pis, currentIndex, rpis_x) {
    var isLast = currentIndex == (rpis_by_pis.length - 1);

    if (isLast) {
        return "<input type='Button' class='btnAddProcessInfo' value='Add ProcessInfo'> </input>";
    }
    return "<input type='Button' class='btnDeleteProcessInfo' value='Delete'> </input>";
};
var createDropDownForPIL = function (rpis_x) {
    //rpis_x.ProcessInfoID  all ProcessInfoLines as DDL elements for this ProcessInfo
    var ddhtml = "<select class='ddlPil'>";
    var allPILs = _.where(pils, { "ProcessInfoID": rpis_x.ProcessInfoID });
    var templateOps = _.template("<option value='<%= pilid%>' <%= sel %> ><%= text %></option>");

    _.each(allPILs, function (pil, index, allPILs) {
        ddhtml += templateOps({
            text:pil.ParameterName,
            pilid: pil.ProcessInfoLineID,
            sel: (rpis_x.ProcessInfoLineID == pil.ProcessInfoLineID ? "selected" : "")
        });
    });
    return ddhtml + "</select>";
}
var getUniqueByProperty = function (arr, propName) {
    return _.chain(rpis).map(function (item) { return item[propName] }).uniq().sortBy(function (i) { return i; }).value();
};
