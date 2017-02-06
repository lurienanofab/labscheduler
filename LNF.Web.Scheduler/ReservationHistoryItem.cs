using LNF.CommonTools;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LNF.Web.Scheduler
{
    public class ReservationHistoryItem
    {
        public Reservation Reservation { get; }
        public int ReservationID { get; set; }
        public DateTime BeginDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime? ActualBeginDateTime { get; set; }
        public DateTime? ActualEndDateTime { get; set; }
        public string Notes { get; set; }
        public int ResourceID { get; set; }
        public string ResourceName { get; set; }
        public int ActivityID { get; set; }
        public string ActivityName { get; set; }
        public bool Editable { get; set; }
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public double ChargeMuliplier { get; set; }
        public double ForgiveAmount { get; set; }
        public bool ApplyLateChargePenalty { get; set; }
        public bool IsActive { get; set; }
        public bool IsStarted { get; set; }
        public bool IsCanceledForModification { get; set; }

        public ReservationHistoryItem(ReservationHistoryFilterItem item)
        {
            Reservation = item.Reservation;
            SetProperties(item.IsCanceledForModification);
        }

        private void SetProperties(bool isCanceledForModification)
        {
            ReservationID = Reservation.ReservationID;
            BeginDateTime = Reservation.BeginDateTime;
            EndDateTime = Reservation.EndDateTime;
            ActualBeginDateTime = Reservation.ActualBeginDateTime;
            ActualEndDateTime = Reservation.ActualEndDateTime;
            Notes = Reservation.Notes;
            ResourceID = Reservation.Resource.ResourceID;
            ResourceName = Reservation.Resource.ResourceName;
            ActivityID = Reservation.Activity.ActivityID;
            ActivityName = Reservation.Activity.ActivityName;
            Editable = Reservation.Activity.Editable;
            AccountID = Reservation.Account.AccountID;
            AccountName = Reservation.Account.Name;
            ChargeMuliplier = Reservation.ChargeMultiplier;
            ForgiveAmount = (1.0 - Reservation.ChargeMultiplier) * 100.0;
            ApplyLateChargePenalty = Reservation.ApplyLateChargePenalty;
            IsActive = Reservation.IsActive;
            IsStarted = Reservation.IsStarted;
            IsCanceledForModification = isCanceledForModification;
        }

        public static IList<ReservationHistoryItem> CreateList(IEnumerable<ReservationHistoryFilterItem> reservations)
        {
            return reservations.Select(x => new ReservationHistoryItem(x)).ToList();
        }

        public static DataTable CreateDataTable(IEnumerable<ReservationHistoryFilterItem> reservations)
        {
            DataTable dt = InitTable();
            IList<ReservationHistoryItem> items = CreateList(reservations);
            FillTable(items, dt);
            return dt;
        }

        private static DataTable InitTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("ReservationID", typeof(int));
            dt.Columns.Add("BeginDateTime", typeof(DateTime));
            dt.Columns.Add("EndDateTime", typeof(DateTime));
            dt.Columns.Add("ActualBeginDateTime", typeof(DateTime));
            dt.Columns.Add("ActualEndDateTime", typeof(DateTime));
            dt.Columns.Add("Notes", typeof(string));
            dt.Columns.Add("ResourceID", typeof(int));
            dt.Columns.Add("ResourceName", typeof(string));
            dt.Columns.Add("ActivityID", typeof(int));
            dt.Columns.Add("ActivityName", typeof(string));
            dt.Columns.Add("Editable", typeof(bool));
            dt.Columns.Add("AccountID", typeof(int));
            dt.Columns.Add("AccountName", typeof(string));
            dt.Columns.Add("ChargeMuliplier", typeof(double));
            dt.Columns.Add("ForgiveAmount", typeof(double));
            dt.Columns.Add("ApplyLateChargePenalty", typeof(bool));
            dt.Columns.Add("IsActive", typeof(bool));
            dt.Columns.Add("IsStarted", typeof(bool));
            dt.Columns.Add("IsCanceledForModification", typeof(bool));
            return dt;
        }

        private static void FillTable(IList<ReservationHistoryItem> items, DataTable dt)
        {
            foreach (var i in items)
            {
                DataRow dr = dt.NewRow();
                dr["ReservationID"] = i.ReservationID;
                dr["BeginDateTime"] = i.BeginDateTime;
                dr["EndDateTime"] = i.EndDateTime;
                dr["ActualBeginDateTime"] = i.ActualBeginDateTime.GetValueOrDBNull();
                dr["ActualEndDateTime"] = i.ActualEndDateTime.GetValueOrDBNull();
                dr["Notes"] = i.Notes;
                dr["ResourceID"] = i.ResourceID;
                dr["ResourceName"] = i.ResourceName;
                dr["ActivityID"] = i.ActivityID;
                dr["ActivityName"] = i.ActivityName;
                dr["Editable"] = i.Editable;
                dr["AccountID"] = i.AccountID;
                dr["AccountName"] = i.AccountName;
                dr["ChargeMuliplier"] = i.ChargeMuliplier;
                dr["ForgiveAmount"] = i.ForgiveAmount;
                dr["ApplyLateChargePenalty"] = i.ApplyLateChargePenalty;
                dr["IsActive"] = i.IsActive;
                dr["IsStarted"] = i.IsStarted;
                dr["IsCanceledForModification"] = i.IsCanceledForModification;
                dt.Rows.Add(dr);
            }
        }
    }
}