<%@ Page Title="Recurring Reservations" Language="vb" AutoEventWireup="false" MasterPageFile="~/MasterPageScheduler.Master" CodeBehind="UserRecurringReservationEdit.aspx.vb" Inherits="LabScheduler.Pages.UserRecurringReservationEdit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="http://ajax.googleapis.com/ajax/libs/angularjs/1.4.8/angular.min.js"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="recurring-reservations">

        <div runat="server" id="divRecurrenceDetail" class="detail" data-id="" ng-app="ReservationRecurrenceApp">
            <h5>Modify Recurring Reservation</h5>

            <em class="text-muted">Note: Modifing these settings will not alter existing reservations. You can edit existing reservations by clicking the ID link in the list below.</em>

            <hr />

            <div ng-controller="ReservationRecurrenceController">
                <div class="form-horizontal">
                    <div class="form-group">
                        <label class="control-label col-sm-2">Resource</label>
                        <div class="col-sm-2">
                            <p class="form-control-static"><a href="{{rr.getResourceUrl()}}" ng-bind="rr.getResourceName()"></a></p>
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="control-label col-sm-2">Start Date</label>
                        <div class="col-sm-2">
                            <input type="text" ng-model="rr.BeginDate" datetime="MM/DD/YYYY" class="form-control" />
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="control-label col-sm-2">Start Time</label>
                        <div class="col-sm-2">
                            <input type="text" ng-model="rr.BeginTime" class="form-control" />
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="control-label col-sm-2">End Date</label>
                        <div class="col-sm-2">
                            <input type="text" ng-model="rr.EndDate" class="form-control" />
                        </div>
                    </div>
                    <div class="form-group">
                        <label class="control-label col-sm-2">End Time</label>
                        <div class="col-sm-2">
                            <input type="text" ng-model="rr.EndTime" class="form-control" />
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="col-sm-offset-2 col-sm-10">
                            <div class="checkbox">
                                <label>
                                    <input type="checkbox" ng-model="rr.AutoEnd">
                                    Auto End
                                </label>
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="col-sm-offset-2 col-sm-10">
                            <div class="checkbox">
                                <label>
                                    <input type="checkbox" ng-model="rr.KeepAlive">
                                    Keep Alive
                                </label>
                            </div>
                        </div>
                    </div>
                    <div class="form-group">
                        <div class="col-sm-offset-2 col-sm-10">
                            <button type="button" ng-click="update()" class="btn btn-primary">Update</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <input type="hidden" runat="server" id="hidClientID" />
        <asp:Repeater runat="server" ID="rptRecurrence">
            <ItemTemplate>
                <div class="detail" data-id='<%#Eval("RecurrenceID")%>'>
                    <h5>Modify Recurring Reservation</h5>
                    <span style="color: #808080; font-style: italic;"></span>
                    <br />
                    <br />
                    <strong>Resource:</strong>
                    <input type="hidden" class="resource-url" value='<%#Eval("ResourceUrl")%>' />
                    <span class="resource-name"></span>
                    <div class="group">
                        <table>
                            <tr>
                                <td>Start Time:</td>
                                <td>
                                    <input type="text" class="begin-time" />
                                    <i>e.g. 10:30 AM</i>
                                </td>
                            </tr>
                            <tr>
                                <td>End Time:</td>
                                <td>
                                    <input type="text" class="end-time" />
                                    <i>e.g. 2:15 PM</i>
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <label for='auto_end_<%#Eval("RecurrenceID")%>'>Auto End:</label>
                                </td>
                                <td>
                                    <input type="checkbox" class="auto-end" id='auto_end_<%#Eval("RecurrenceID")%>' />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                    <label for='keep_alive_<%#Eval("RecurrenceID")%>'>Keep Alive:</label>
                                </td>
                                <td>
                                    <input type="checkbox" class="keep-alive" id='keep_alive_<%#Eval("RecurrenceID")%>' />
                                </td>
                            </tr>
                            <tr>
                                <td>Notes:</td>
                                <td>
                                    <textarea class="notes" rows="2" cols="5"></textarea>
                                </td>
                            </tr>
                        </table>
                    </div>
                    <div class="group">
                        <div class="group-box">
                            <div class="group-box-title">Pattern of Recurrence</div>
                            <div class="group-box-content">
                                <label>
                                    <input type="radio" class="recurrence-pattern" name="pattern_name" data-name="weekly" value="1" checked="checked" />
                                    Weekly
                                </label>
                                <label>
                                    <input type="radio" class="recurrence-pattern" name="pattern_name" data-name="monthly" value="2" />
                                    Monthly
                                </label>
                                <div class="group-box-section pattern weekly">
                                    <label>
                                        <input type="radio" class="pattern-param1" name="weekly_pattern_param1" value="0" checked="checked" />
                                        Sunday
                                    </label>
                                    <label>
                                        <input type="radio" class="pattern-param1" name="weekly_pattern_param1" value="1" />
                                        Monday
                                    </label>
                                    <label>
                                        <input type="radio" class="pattern-param1" name="weekly_pattern_param1" value="2" />
                                        Tuesday
                                    </label>
                                    <label>
                                        <input type="radio" class="pattern-param1" name="weekly_pattern_param1" value="3" />
                                        Wednesday
                                    </label>
                                    <label>
                                        <input type="radio" class="pattern-param1" name="weekly_pattern_param1" value="4" />
                                        Thursday
                                    </label>
                                    <label>
                                        <input type="radio" class="pattern-param1" name="weekly_pattern_param1" value="5" />
                                        Friday
                                    </label>
                                    <label>
                                        <input type="radio" class="pattern-param1" name="weekly_pattern_param1" value="6" />
                                        Saturday
                                    </label>
                                </div>
                                <div class="group-box-section pattern monthly">
                                    The
                                    <select class="pattern-param1">
                                        <option value="1" selected="selected">First</option>
                                        <option value="2">Second</option>
                                        <option value="3">Third</option>
                                        <option value="4">Fourth</option>
                                        <option value="5">Last</option>
                                    </select>
                                    <select class="pattern-param2">
                                        <option value="0" selected="selected">Sunday</option>
                                        <option value="1">Monday</option>
                                        <option value="2">Tuesday</option>
                                        <option value="3">Wednesday</option>
                                        <option value="4">Thursday</option>
                                        <option value="5">Friday</option>
                                        <option value="6">Saturday</option>
                                    </select>
                                    of every month
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="group">
                        <div class="group-box">
                            <div class="group-box-title">Range of Recurrence</div>
                            <div class="group-box-content">
                                <table>
                                    <tr>
                                        <td>Start:</td>
                                        <td>
                                            <input type="text" class="range-begin-date" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>End:</td>
                                        <td>
                                            <div class="range-end-options">
                                                <div>
                                                    <label>
                                                        <input type="radio" class="range-end-infinite-option" name="range_option" value="1" checked="checked" />
                                                        Infinite
                                                    </label>
                                                </div>
                                                <div style="margin-top: 5px;">
                                                    <label>
                                                        <input type="radio" class="range-end-date-option" name="range_option" value="2" />
                                                        Date
                                                    </label>
                                                    <input type="text" class="range-end-date disabled" readonly="readonly" />
                                                </div>
                                            </div>
                                        </td>
                                    </tr>
                                </table>
                            </div>
                        </div>
                    </div>
                    <div class="group" style="padding-bottom: 20px;">
                        <input type="hidden" class="return-url" value='<%#Eval("ReturnUrl") %>' />
                        <input type="button" value="Save" class="save-recurrence" />
                        <input type="button" value="Cancel" class="cancel-recurrence" />
                    </div>
                    <hr />
                    <h5>Existing Reservations</h5>
                    <div style="width: 1000px;">
                        <table class="reservation-list" style="width: 100%;">
                            <thead>
                                <tr>
                                    <th>ID</th>
                                    <th>Begin Date</th>
                                    <th>End Date</th>
                                    <th>Auto End</th>
                                    <th>Keep Alive</th>
                                    <th>Notes</th>
                                </tr>
                            </thead>
                            <tbody>
                            </tbody>
                        </table>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
        <asp:Literal runat="server" ID="litMessage"></asp:Literal>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="scripts" runat="server">
    <script>
        var id = $(".detail").data("id");

        angular.module('datetime', []);
        angular.module('datetime').factory('datetime', function () {
            return {
                link: function (scope, element, attrs, ngModel) {
                    console.log(scope);
                }
            };
        });

        var app = angular.module('ReservationRecurrenceApp', [])
            .controller('ReservationRecurrenceController', ['$scope', function ($scope, $http) {
                lnfapi.ReservationRecurrence.get(id).done(function (rr) {
                    $scope.$apply(function () {
                        $scope.rr = {
                            Resource: rr.Resource,
                            BeginDate: moment(rr.BeginDate).format("MM/DD/YYYY"),
                            BeginTime: moment(rr.BeginTime).format("hh:mm:ss"),
                            EndDate: rr.EndDate == null ? "" : moment(rr.EndDate).format("MM/DD/YYYY"),
                            EndTime: moment(rr.EndTime).format("hh:mm:ss"),
                            AutoEnd: rr.AutoEnd,
                            KeepAlive: rr.KeepAlive,
                            getResourceName: function () {
                                return this.Resource.ResourceName + " [" + this.Resource.ResourceID + "]";
                            },
                            getResourceUrl: function () {
                                return "/sselscheduler/ResourceDayWeek.aspx?Path="
                                    + this.Resource.ProcessTech.Lab.BuildingID
                                    + ":" + this.Resource.ProcessTech.Lab.LabID
                                    + ":" + this.Resource.ProcessTech.ProcessTechID
                                    + ":" + this.Resource.ResourceID;
                            }
                        };

                        $scope.update = function () {
                            console.log($scope.rr.BeginDate);
                            console.log($scope.rr.BeginTime);
                        }
                    });
                });
            }]);
    </script>
</asp:Content>
