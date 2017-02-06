using LNF.CommonTools;
using LNF.Repository.Scheduler;
using System;
using System.Collections.Generic;
using System.Data;

namespace LNF.Web.Scheduler
{
    public class DataUtility
    {
        public static DataTable ResourceClientTable(IEnumerable<ResourceClientInfo> collection)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ClientID", typeof(int));
            dt.Columns.Add("DisplayName", typeof(string));
            
            foreach(var rc in collection)
            {
                var nr = dt.NewRow();
                nr.SetField("ClientID", rc.ClientID);
                nr.SetField("DisplayName", rc.DisplayName);
                dt.Rows.Add(nr);
            }

            return dt;
        }

        public static DataTable ConvertToReservationTable(IEnumerable<Reservation> collection)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ReservationID", typeof(int));
            dt.Columns.Add("ResourceID", typeof(int));
            dt.Columns.Add("ClientID", typeof(int));
            dt.Columns.Add("AccountID", typeof(int));
            dt.Columns.Add("ActivityID", typeof(int));
            dt.Columns.Add("BeginDateTime", typeof(DateTime));
            dt.Columns.Add("EndDateTime", typeof(DateTime));
            dt.Columns.Add("ActualBeginDateTime", typeof(DateTime));
            dt.Columns.Add("ActualEndDateTime", typeof(DateTime));
            dt.Columns.Add("ClientIDBegin", typeof(int));
            dt.Columns.Add("ClientIDEnd", typeof(int));
            dt.Columns.Add("CreatedOn", typeof(DateTime));
            dt.Columns.Add("LastModifiedOn", typeof(DateTime));
            dt.Columns.Add("Duration", typeof(double));
            dt.Columns.Add("Notes", typeof(string));
            dt.Columns.Add("ChargeMultiplier", typeof(double));
            dt.Columns.Add("ApplyLateChargePenalty", typeof(bool));
            dt.Columns.Add("AutoEnd", typeof(bool));
            dt.Columns.Add("HasProcessInfo", typeof(bool));
            dt.Columns.Add("HasInvitees", typeof(bool));
            dt.Columns.Add("IsActive", typeof(bool));
            dt.Columns.Add("IsStarted", typeof(bool));
            dt.Columns.Add("IsUnloaded", typeof(bool));
            dt.Columns.Add("RecurrenceID", typeof(int));
            dt.Columns.Add("GroupID", typeof(int));
            dt.Columns.Add("MaxReservationDuration", typeof(double));
            dt.Columns.Add("CancelledDateTime", typeof(DateTime));
            dt.Columns.Add("KeepAlive", typeof(bool));
            dt.Columns.Add("OriginalBeginDateTime", typeof(DateTime));
            dt.Columns.Add("OriginalEndDateTime", typeof(DateTime));
            dt.Columns.Add("OriginalModifiedOn", typeof(DateTime));
            dt.Columns.Add("ResourceName", typeof(string));
            dt.Columns.Add("Granularity", typeof(int));
            dt.Columns.Add("IsSchedulable", typeof(bool));
            dt.Columns.Add("Editable", typeof(bool));
            dt.Columns.Add("DisplayName", typeof(string));

            foreach (LNF.Repository.Scheduler.Reservation rsv in collection)
            {
                DataRow nr = dt.NewRow();
                nr["ReservationID"] = rsv.ReservationID;
                nr["ResourceID"] = rsv.Resource.ResourceID;
                nr["ClientID"] = rsv.Client.ClientID;
                nr["AccountID"] = rsv.Account.AccountID;
                nr["ActivityID"] = rsv.Activity.ActivityID;
                nr["BeginDateTime"] = rsv.BeginDateTime;
                nr["EndDateTime"] = rsv.EndDateTime;
                nr["ActualBeginDateTime"] = Utility.ConvertNullableDateTimeToObject(rsv.ActualBeginDateTime);
                nr["ActualEndDateTime"] = Utility.ConvertNullableDateTimeToObject(rsv.ActualEndDateTime);
                nr["ClientIDBegin"] = NullCheck(rsv.ClientIDBegin);
                nr["ClientIDEnd"] = NullCheck(rsv.ClientIDEnd);
                nr["CreatedOn"] = rsv.CreatedOn;
                nr["LastModifiedOn"] = rsv.LastModifiedOn;
                nr["Duration"] = rsv.Duration;
                nr["Notes"] = rsv.Notes;
                nr["ChargeMultiplier"] = rsv.ChargeMultiplier;
                nr["ApplyLateChargePenalty"] = rsv.ApplyLateChargePenalty;
                nr["AutoEnd"] = rsv.AutoEnd;
                nr["HasProcessInfo"] = rsv.HasProcessInfo;
                nr["HasInvitees"] = rsv.HasInvitees;
                nr["IsActive"] = rsv.IsActive;
                nr["IsStarted"] = rsv.IsStarted;
                nr["IsUnloaded"] = rsv.IsUnloaded;
                nr["RecurrenceID"] = Utility.ConvertNullableIntToObject(rsv.RecurrenceID);
                nr["GroupID"] = Utility.ConvertNullableIntToObject(rsv.GroupID);
                nr["MaxReservationDuration"] = rsv.MaxReservedDuration;
                nr["CancelledDateTime"] = Utility.ConvertNullableDateTimeToObject(rsv.CancelledDateTime);
                nr["KeepAlive"] = rsv.KeepAlive;
                nr["OriginalBeginDateTime"] = Utility.ConvertNullableDateTimeToObject(rsv.OriginalBeginDateTime);
                nr["OriginalEndDateTime"] = Utility.ConvertNullableDateTimeToObject(rsv.OriginalEndDateTime);
                nr["OriginalModifiedOn"] = Utility.ConvertNullableDateTimeToObject(rsv.OriginalModifiedOn);
                nr["ResourceName"] = rsv.Resource.ResourceName;
                nr["Granularity"] = rsv.Resource.Granularity;
                nr["IsSchedulable"] = rsv.Resource.IsSchedulable;
                nr["Editable"] = rsv.Activity.Editable;
                nr["DisplayName"] = rsv.Client.DisplayName;
                dt.Rows.Add(nr);
            }

            return dt;
        }

        public static object NullCheck(object obj)
        {
            if (obj == null)
                return DBNull.Value;
            else
                return obj;
        }
    }
}