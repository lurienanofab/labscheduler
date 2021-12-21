using LNF.Data;
using LNF.Mail;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace LNF.Web.Scheduler.FacilityDownTime
{
    public class FacilityDownTimeRepository
    {
        private readonly SqlConnection _conn;
        private readonly SqlTransaction _tx;
        private readonly DataTable _schedulerProps;

        public static readonly string DateFormat = "MM/dd/yyyy hh:mm tt";

        public FacilityDownTimeRepository(SqlConnection conn, SqlTransaction tx)
        {
            _conn = conn;
            _tx = tx;
            _schedulerProps = new DataTable();
            FillSchedulerProperties(_schedulerProps);
        }

        public IEnumerable<FacilityDownTimeReservation> GetReservationsByDateRange(DateTime sd, DateTime ed, bool includeDeleted)
        {
            return GetReservationsByDateRange(null, sd, ed, includeDeleted);
        }

        public IEnumerable<FacilityDownTimeReservation> GetReservationsByDateRange(int? resourceId, DateTime sd, DateTime ed, bool includeDeleted)
        {
            object active = DBNull.Value;

            if (!includeDeleted)
                active = true;

            object rid = DBNull.Value;

            if (resourceId.HasValue)
                rid = resourceId.Value;

            using (var cmd = GetCommand("sselScheduler.dbo.procReservationItemSelect"))
            {
                cmd.Parameters.AddWithValue("Action", "SelectByDateRange", SqlDbType.NVarChar, 50);
                cmd.Parameters.AddWithValue("ResourceID", rid, SqlDbType.Int);
                cmd.Parameters.AddWithValue("StartDate", sd, SqlDbType.DateTime);
                cmd.Parameters.AddWithValue("EndDate", ed, SqlDbType.DateTime);
                cmd.Parameters.AddWithValue("IsActive", active, SqlDbType.Bit);

                var dt = FillDataTable(cmd);

                var result = new List<FacilityDownTimeReservation>();

                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(CreateFacilityDownTimeReservation(dr));
                }

                return result;
            }
        }

        public IEnumerable<FacilityDownTimeLab> GetLabs()
        {
            var result = new List<FacilityDownTimeLab>
            {
                new FacilityDownTimeLab { LabID = 0, LabDisplayName = "All" },
                new FacilityDownTimeLab { LabID = -1, LabDisplayName = "Clean Room & ROBIN" }
            };

            using (var cmd = GetCommand("SELECT LabID, DisplayName AS LabDisplayName FROM sselScheduler.dbo.Lab WHERE IsActive = 1 ORDER BY DisplayName", CommandType.Text))
            {
                var dt = FillDataTable(cmd);
                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new FacilityDownTimeLab { LabID = dr.Field<int>("LabID"), LabDisplayName = dr.Field<string>("LabDisplayName") });
                }
            }

            return result;
        }

        public string GetResourceDisplayName(DataRow dr)
        {
            return string.Format("{0}: {1}: {2}", dr["LabDisplayName"], dr["ProcessTechName"], dr["ResourceName"]);
        }

        public IEnumerable<FacilityDownTimeResource> GetTools(int labId)
        {
            int[] labs;

            if (labId == 0)
                labs = new int[0];
            else if (labId == -1)
                labs = new int[] { 1, 9 };
            else
                labs = new int[] { labId };

            string filter = string.Empty;

            if (labs.Length > 0)
                filter = $" AND lab.LabID IN ({string.Join(",", labs)})";

            var sql = $"SELECT res.ResourceID, pt.ProcessTechID, lab.LabID, res.ResourceName, pt.ProcessTechName, lab.DisplayName as LabDisplayName FROM sselScheduler.dbo.[Resource] res INNER JOIN sselScheduler.dbo.ProcessTech pt ON pt.ProcessTechID = res.ProcessTechID INNER JOIN sselScheduler.dbo.Lab lab ON lab.LabID = pt.LabID WHERE res.IsActive = 1 AND pt.IsActive = 1 AND lab.IsActive = 1{filter} ORDER BY lab.DisplayName, pt.ProcessTechName, res.ResourceName";

            var result = new List<FacilityDownTimeResource>();

            using (var cmd = GetCommand(sql, CommandType.Text))
            {
                var dt = FillDataTable(cmd);

                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new FacilityDownTimeResource
                    {
                        ResourceID = dr.Field<int>("ResourceID"),
                        ProcessTechID = dr.Field<int>("ProcessTechID"),
                        LabID = dr.Field<int>("LabID"),
                        ResourceDisplayName = GetResourceDisplayName(dr)
                    });
                }
            }

            return result;
        }

        public FacilityDownTimeGroup GetGroup(int groupId)
        {
            using (var cmd = GetCommand("sselScheduler.dbo.procReservationGroupSelect")) //= DataCommand.Create().Param(New With {.Action = "ByGroupID", GroupID}).ExecuteReader()
            {
                cmd.Parameters.AddWithValue("Action", "ByGroupID", SqlDbType.NVarChar, 50);
                cmd.Parameters.AddWithValue("GroupID", groupId, SqlDbType.Int);

                var dt = FillDataTable(cmd);

                if (dt.Rows.Count > 0)
                {
                    DataRow dr = dt.Rows[0];

                    return new FacilityDownTimeGroup
                    {
                        GroupID = dr.Field<int>("GroupID"),
                        ClientID = dr.Field<int>("ClientID"),
                        DisplayName = dr.Field<string>("DisplayName"),
                        BeginDateTime = dr.Field<DateTime>("BeginDateTime"),
                        EndDateTime = dr.Field<DateTime>("EndDateTime"),
                        Reservations = GetReservationsByGroup(groupId)
                    };
                }

                throw new Exception($"Cannot find ReservationGroup record with GroupID: {groupId}");
            }
        }

        public IEnumerable<FacilityDownTimeGroup> GetGroups()
        {
            var result = new List<FacilityDownTimeGroup>();

            using (var cmd = GetCommand("sselScheduler.dbo.procReservationGroupSelect"))
            {
                cmd.Parameters.AddWithValue("Action", "GetActiveFacilityDownTime", SqlDbType.NVarChar, 50);

                var dt = FillDataTable(cmd);

                var rows = dt.Select(string.Empty, "BeginDateTime DESC");

                foreach (DataRow dr in rows)
                {
                    result.Add(new FacilityDownTimeGroup
                    {
                        ClientID = dr.Field<int>("ClientID"),
                        GroupID = dr.Field<int>("GroupID"),
                        DisplayName = dr.Field<string>("DisplayName"),
                        BeginDateTime = dr.Field<DateTime>("BeginDateTime"),
                        EndDateTime = dr.Field<DateTime>("EndDateTime"),
                        Reservations = null
                    });
                }
            }

            return result;
        }

        public IEnumerable<FacilityDownTimeReservation> GetReservationsByGroup(int groupId)
        {
            var result = new List<FacilityDownTimeReservation>();

            using (var cmd = GetCommand("sselScheduler.dbo.procReservationItemSelect"))
            {
                cmd.Parameters.AddWithValue("Action", "SelectByGroup", SqlDbType.NVarChar, 50);
                cmd.Parameters.AddWithValue("GroupID", groupId, SqlDbType.Int);

                var dt = FillDataTable(cmd);

                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(CreateFacilityDownTimeReservation(dr));
                }
            }

            return result;
        }

        private FacilityDownTimeReservation CreateFacilityDownTimeReservation(DataRow dr)
        {
            return new FacilityDownTimeReservation
            {
                ReservationID = dr.Field<int>("ReservationID"),
                ResourceID = dr.Field<int>("ResourceID"),
                ResourceName = dr.Field<string>("ResourceName"),
                ResourceDisplayName = GetResourceDisplayName(dr),
                ProcessTechID = dr.Field<int>("ProcessTechID"),
                ProcessTechName = dr.Field<string>("ProcessTechName"),
                LabID = dr.Field<int>("LabID"),
                LabName = dr.Field<string>("LabName"),
                LabDisplayName = dr.Field<string>("LabDisplayName"),
                BeginDateTime = dr.Field<DateTime>("BeginDateTime"),
                EndDateTime = dr.Field<DateTime>("EndDateTime"),
                ActualBeginDateTime = dr.Field<DateTime?>("ActualBeginDateTime"),
                ActualEndDateTime = dr.Field<DateTime?>("ActualEndDateTime"),
                Editable = dr.Field<bool>("Editable")
            };
        }

        public void DeleteGroup(int groupId)
        {
            using (var cmd = GetCommand("sselScheduler.dbo.procReservationGroupDelete"))
            {
                cmd.Parameters.AddWithValue("Action", "ByGroupID", SqlDbType.NVarChar, 50);
                cmd.Parameters.AddWithValue("GroupID", groupId, SqlDbType.Int);
                Execute(cmd);
            }
        }

        public int InsertFacilityDownTimeReservation(int resourceId, int clientId, int groupId, DateTime sd, DateTime ed, string notes)
        {
            // Important considerations:
            //      1. Activity is always the FacilityDownTime activity as defined in sselScheduler.dbo.Activity
            //      2. Account is always the LabAccount as defined by LNF.Scheduler.Properties.LabAccount
            //      3. ActualBeginDateTime and ActualEndDateTime are set to beginDateTime and endDateTime
            //      4. ClientIDBegin and ClientIDEnd are set to clientId, and IsStarted is set to true
            //      5. #3 and #4 means that FacilityDownTime reservations are already started and ended when created, regardless scheduled begin/end

            int accountId = int.Parse(GetPropertyValue("LabAccountID"));
            int activityId = int.Parse(GetPropertyValue("FacilityDownTimeActivity"));

            int reservationId;

            using (var cmd = GetCommand("sselScheduler.dbo.procReservationInsert"))
            {
                cmd.Parameters.AddWithValue("Action", "InsertFacilityDownTime", SqlDbType.NVarChar, 50);
                cmd.Parameters.AddOutputParameter("ReservationID", SqlDbType.Int);
                cmd.Parameters.AddWithValue("ClientID", clientId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("ResourceID", resourceId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("AccountID", accountId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("ActivityID", activityId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("BeginDateTime", sd, SqlDbType.DateTime);
                cmd.Parameters.AddWithValue("EndDateTime", ed, SqlDbType.DateTime);
                cmd.Parameters.AddWithValue("Notes", notes, SqlDbType.NVarChar, 500);
                cmd.Parameters.AddWithValue("GroupID", groupId, SqlDbType.Int);

                Execute(cmd);

                reservationId = Convert.ToInt32(cmd.Parameters["ReservationID"].Value);
            }

            return reservationId;
        }

        public void EndReservation(int reservationId, int clientId)
        {
            using (var cmd = GetCommand("sselScheduler.dbo.procReservationUpdate"))
            {
                cmd.Parameters.AddWithValue("Action", "End", SqlDbType.NVarChar, 50);
                cmd.Parameters.AddWithValue("ReservationID", reservationId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("ClientID", clientId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("ModifiedByClientID", clientId, SqlDbType.Int);
                Execute(cmd);
            }
        }

        public void CancelAndForgiveReservation(int reservationId, string note, int clientId)
        {
            using (var cmd = GetCommand("sselScheduler.dbo.procReservationUpdate"))
            {
                cmd.Parameters.AddWithValue("Action", "AppendNotes", SqlDbType.NVarChar, 50);
                cmd.Parameters.AddWithValue("ReservationID", reservationId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("AppendNotes", note, SqlDbType.NVarChar, 500);
                Execute(cmd);
            }

            using (var cmd = GetCommand("sselScheduler.dbo.procReservationDelete"))
            {
                cmd.Parameters.AddWithValue("Action", "WithForgive", SqlDbType.NVarChar, 50);
                cmd.Parameters.AddWithValue("ReservationID", reservationId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("ModifiedByClientID", clientId, SqlDbType.Int);
                Execute(cmd);
            }
        }

        public int CreateNewFacilityDownTime(int clientId, DateTime sd, DateTime ed)
        {
            int accountId = int.Parse(GetPropertyValue("LabAccountID"));
            int activityId = int.Parse(GetPropertyValue("FacilityDownTimeActivity"));

            using (var cmd = GetCommand("sselScheduler.dbo.procReservationGroupInsert"))
            {
                cmd.Parameters.AddWithValue("Action", "InsertNew", SqlDbType.NVarChar, 50);
                cmd.Parameters.AddWithValue("ClientID", clientId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("AccountID", accountId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("ActivityID", activityId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("BeginDateTime", sd, SqlDbType.DateTime);
                cmd.Parameters.AddWithValue("EndDateTime", ed, SqlDbType.DateTime);
                cmd.Parameters.AddWithValue("IsActive", true, SqlDbType.Bit);
                cmd.Parameters.AddWithValue("CreatedOn", DateTime.Now, SqlDbType.DateTime);
                cmd.Parameters.AddOutputParameter("GroupID", SqlDbType.Int);

                Execute(cmd);

                var groupId = Convert.ToInt32(cmd.Parameters["GroupID"].Value);

                return groupId;
            }
        }

        public void EmailOnCanceledByRepair(int reservationId, bool isRemoved, string state, string notes, DateTime ed, int clientId)
        {
            DataRow drRsv = GetReservation(reservationId);

            string from = GetPropertyValue("SchedulerEmail");
            IEnumerable<string> to = new[] { drRsv.Field<string>("Email") };

            string subject = $"{CommonTools.SendEmail.CompanyName} - Reservation Canceled";
            string body = string.Format("{0}{8}{8}Your reservation beginning at {1} and ending at {2} for resource {3} has been {4} because this resource has been marked '{5}' until {6}.{8}{8}The reason for the change:{8}{7}{8}{8}If you have any questions, please contact the tool engineer.",
                Clients.GetDisplayName(drRsv.Field<string>("LName"), drRsv.Field<string>("FName")),
                drRsv.Field<DateTime>("BeginDateTime").ToString(DateFormat),
                drRsv.Field<DateTime>("EndDateTime").ToString(DateFormat),
                drRsv.Field<string>("ResourceName"),
                (isRemoved) ? "removed" : "forced to end",
                state,
                ed.ToString(DateFormat),
                notes,
                Environment.NewLine);

            if (to.Count() == 0) return;

            SendEmail(new SendMessageArgs { ClientID = clientId, Caller = "LNF.Web.Scheduler.Handlers.Repo.EmailOnCanceledByRepair", Subject = subject, Body = body, From = from, To = to });
        }

        public void SendEmail(SendMessageArgs args)
        {
            var mailRepo = Impl.Mail.MailRepo.Create(_conn, _tx);

            int messageId = mailRepo.InsertMessage(args.ClientID, args.Caller, args.From, args.Subject, args.Body);

            if (messageId == 0)
                throw new Exception("Failed to create message [messageId = 0]");

            mailRepo.InsertRecipients(messageId, AddressType.To, args.To);
            mailRepo.InsertRecipients(messageId, AddressType.Cc, args.Cc);
            mailRepo.InsertRecipients(messageId, AddressType.Bcc, args.Bcc);

            try
            {
                Impl.Mail.MailUtility.Send(args);
                mailRepo.SetMessageSent(messageId);
            }
            catch (Exception ex)
            {
                mailRepo.SetMessageError(messageId, ex.Message);
                throw ex;
            }
        }

        public DataRow GetReservation(int reservationId)
        {
            using (var cmd = GetCommand("SELECT * FROM sselScheduler.dbo.v_ReservationInfo WHERE ReservationID = @ReservationID", CommandType.Text))
            {
                cmd.Parameters.AddWithValue("ReservationID", reservationId, SqlDbType.Int);
                var dt = FillDataTable(cmd);
                return dt.Rows[0];
            }
        }

        public void UpdateFacilityDownTime(int groupId, DateTime sd, DateTime ed)
        {
            using (var cmd = GetCommand("sselScheduler.dbo.procReservationGroupUpdate"))
            {
                cmd.Parameters.AddWithValue("Action", "ByGroupID", SqlDbType.NVarChar, 50);
                cmd.Parameters.AddWithValue("GroupID", groupId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("BeginDateTime", sd, SqlDbType.DateTime);
                cmd.Parameters.AddWithValue("EndDateTime", ed, SqlDbType.DateTime);
                Execute(cmd);
            }
        }

        public void UpdateReservationsByGroup(int groupId, DateTime sd, DateTime ed, string notes)
        {
            using (var cmd = GetCommand("sselScheduler.dbo.procReservationUpdate"))
            {
                cmd.Parameters.AddWithValue("Action", "ByGroupID", SqlDbType.NVarChar, 50);
                cmd.Parameters.AddWithValue("GroupID", groupId, SqlDbType.Int);
                cmd.Parameters.AddWithValue("BeginDateTime", sd, SqlDbType.DateTime);
                cmd.Parameters.AddWithValue("EndDateTime", ed, SqlDbType.DateTime);
                cmd.Parameters.AddWithValue("Notes", GetStringOrDBNull(notes), SqlDbType.NVarChar, 500);
                Execute(cmd);
            }
        }

        private object GetStringOrDBNull(string s)
        {
            object result;

            if (string.IsNullOrEmpty(s))
                result = DBNull.Value;
            else
                result = s;

            return result;
        }

        public string GetPropertyValue(string name)
        {
            if (_schedulerProps == null)
                throw new Exception("Call GetSchedulerProperties() first.");

            var result = _schedulerProps.AsEnumerable()
                .First(x => x.Field<string>("PropertyName") == name)
                .Field<string>("PropertyValue");

            return result;
        }

        private void FillSchedulerProperties(DataTable dt)
        {
            using (var cmd = GetCommand("SELECT * FROM sselScheduler.dbo.SchedulerProperty", CommandType.Text))
            {
                FillDataTable(cmd, dt);
            }
        }

        private DataTable FillDataTable(SqlCommand cmd)
        {
            var dt = new DataTable();
            FillDataTable(cmd, dt);
            return dt;
        }

        private void FillDataTable(SqlCommand cmd, DataTable dt)
        {
            using (var adap = new SqlDataAdapter(cmd))
            {
                adap.Fill(dt);
            }
        }

        private int Execute(SqlCommand cmd) => cmd.ExecuteNonQuery();

        private SqlCommand GetCommand(string sql, CommandType commandType = CommandType.StoredProcedure)
        {
            if (_conn == null)
                throw new Exception("The SqlConnection object has not been initialized yet.");

            return new SqlCommand(sql, _conn, _tx) { CommandType = commandType };
        }
    }
}
