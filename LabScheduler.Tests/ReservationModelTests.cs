using LNF;
using LNF.Web.Scheduler.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Specialized;

namespace LabScheduler.Tests
{
    [TestClass]
    public class ReservationModelTests
    {
        [TestMethod]
        public void CanConfirmYes()
        {
            var qs = new NameValueCollection
            {
                ["Path"] = "4-1-18-40061",
                ["Date"] = "2019-03-25"
            };

            using (var test = new SchedulerContextManager(username: "jgett", queryString: qs))
            {
                DateTime now = DateTime.Parse("2019-03-25 13:00");

                ReservationModel model = new ReservationModel(test.ContextBase, ServiceProvider.Current, now)
                {
                    ActivityID = 6, //ddlActivity.SelectedValue
                    AccountID = 67, //ddlAccount.SelectedValue
                    AutoEnd = false, //chkAutoEnd.Checked
                    KeepAlive = true, //chkKeepAlive.Checked
                    Notes = string.Empty, //txtNotes.Text
                    ReservationProcessInfoJson = string.Empty, //hidProcessInfoData.Value
                    DurationText = string.Empty, //txtDuration.Text
                    DurationSelectedValue = "60", //ddlDuration.SelectedValue
                    StartTimeHourSelectedValue = "20", //ddlStartTimeHour.SelectedValue
                    StartTimeMinuteSelectedValue = "0" //dlStartTimeMin.SelectedValue
                };

                var rsv = model.CreateOrModifyReservation();

                Assert.IsNull(test.ContextBase.Session["ErrorMessage"]);
                Assert.AreEqual("/sselscheduler/ResourceDayWeek.aspx?Path=4-1-18-40061&Date=2019-03-25", test.ContextBase.Response.RedirectLocation);
            }
        }

        [TestMethod]
        public void CanGetCurrentActivity()
        {
            //using (var test = new SchedulerContextManager())
            //{
            //    DropDownList ddlActivity = new DropDownList();
            //    ddlActivity.Items.Add(new ListItem { Text = "Processing", Value = "6", Selected = true });

            //    ReservationModel model = new ReservationModel(test.Context);
            //    ActivityItem act;

            //    act = model.GetCurrentActivity(ddlActivity);
            //    Assert.AreEqual(6, act.ActivityID);
            //    Assert.AreEqual("Processing", act.ActivityName);

            //    var act1 = model.GetCurrentActivity(ddlActivity);
            //    var act2 = model.GetCurrentActivity(ddlActivity);
            //    var act3 = model.GetCurrentActivity(ddlActivity);
            //    var act4 = model.GetCurrentActivity(ddlActivity);
            //}
        }
    }
}
