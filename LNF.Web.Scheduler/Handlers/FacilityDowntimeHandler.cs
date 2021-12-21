using LNF.Data;
using LNF.Mail;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using LNF.Web.Scheduler.FacilityDownTime;

namespace LNF.Web.Scheduler.Handlers
{
    public class FacilityDowntimeHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.TrySkipIisCustomErrors = true;

            string command = context.Request.QueryString["command"];

            if (string.IsNullOrEmpty(command))
                throw new Exception("Missing required paramter: command");

            using (var conn = GetConnection())
            using (var tx = conn.BeginTransaction())
            {
                try
                {
                    var repo = new FacilityDownTimeRepository(conn, tx);

                    if (context.Request.HttpMethod.ToUpper() == "POST")
                    {
                        using (var stream = context.Request.InputStream)
                        using (var sr = new StreamReader(stream))
                        {
                            string message;

                            if (command == "make-group")
                            {
                                var json = sr.ReadToEnd();
                                var args = JsonConvert.DeserializeObject<InsertReservationArgs>(json);
                                MakeGroup(repo, args);
                                message = string.Format("You created a new Facility Down Time reservation from {0} to {1} on {2} tools. User reservations have been deleted as well.", args.Start, args.End, args.Tools.Count());
                            }
                            else if (command == "modify-group")
                            {
                                var json = sr.ReadToEnd();
                                var args = JsonConvert.DeserializeObject<UpdateReservationArgs>(json);
                                ModifyGroup(repo, args);
                                message = string.Format("Facility Down Time reservation has been modified. New date range is {0} to {1}. User reservations have been deleted as well.", args.Start, args.End);
                            }
                            else
                            {
                                throw new Exception($"Unknown POST command: {command}");
                            }

                            context.Response.Write(JsonConvert.SerializeObject(new { message }));
                        }
                    }
                    else
                    {
                        int groupId;
                        IEnumerable<object> groups;

                        switch (command)
                        {
                            case "get-labs":
                                var labs = repo.GetLabs();
                                context.Response.Write(JsonConvert.SerializeObject(labs));
                                break;
                            case "get-tools":
                                var labId = int.Parse(context.Request["labId"]);
                                var tools = repo.GetTools(labId);
                                context.Response.Write(JsonConvert.SerializeObject(tools));
                                break;
                            case "get-group":
                                groupId = int.Parse(context.Request["groupId"]);
                                var group = repo.GetGroup(groupId);
                                context.Response.Write(JsonConvert.SerializeObject(group));
                                break;
                            case "get-groups":
                                groups = repo.GetGroups();
                                context.Response.Write(JsonConvert.SerializeObject(groups));
                                break;
                            case "delete-group":
                                groupId = int.Parse(context.Request["groupId"]);
                                repo.DeleteGroup(groupId);
                                groups = repo.GetGroups();
                                context.Response.Write(JsonConvert.SerializeObject(groups));
                                break;
                            default:
                                throw new Exception($"Unknown GET command: {command}");
                        }
                    }

                    tx.Commit();
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    context.Response.StatusCode = 500;
                    context.Response.Write(JsonConvert.SerializeObject(new { message = ex.Message }));
                }
            }
        }

        private void ModifyGroup(FacilityDownTimeRepository repo, UpdateReservationArgs args)
        {
            ValidateDateRange(args);

            repo.UpdateFacilityDownTime(args.GroupID, args.Start, args.End);
            repo.UpdateReservationsByGroup(args.GroupID, args.Start, args.End, args.Notes);

            var group = repo.GetReservationsByGroup(args.GroupID);
            var reservations = repo.GetReservationsByDateRange(args.Start, args.End, false);

            //Delete all the reservations in this period
            foreach (var rsv in group)
            {
                // Find and Remove any unstarted reservations made during time of repair
                var existing = reservations.Where(x => x.ResourceID == rsv.ResourceID).ToList();
                HandleExistingReservations(repo, existing, args.ClientID, args.End);
            }
        }

        private void MakeGroup(FacilityDownTimeRepository repo, InsertReservationArgs args)
        {
            if (args.Tools.Count() > 0)
            {
                ValidateDateRange(args);

                var reservations = repo.GetReservationsByDateRange(args.Start, args.End, false);

                // 2009-07-19 make a reservation group
                int groupId = repo.CreateNewFacilityDownTime(args.ClientID, args.Start, args.End);

                // If we ever want to allow notes for FDT reservations, do it here.
                string notes = args.Notes;

                // Loop through each selected tool and make reservation and delete other people's reservation
                foreach (int resourceId in args.Tools)
                {
                    // Find and Remove any un-started reservations made during time of repair
                    var existing = reservations.Where(x => x.ResourceID == resourceId).ToList();
                    HandleExistingReservations(repo, existing, args.ClientID, args.End);
                    repo.InsertFacilityDownTimeReservation(resourceId, args.ClientID, groupId, args.Start, args.End, notes);
                }
            }
            else
            {
                throw new Exception("No tools selected, so no reservations were made.");
            }
        }

        private void HandleExistingReservations(FacilityDownTimeRepository repo, IEnumerable<FacilityDownTimeReservation> reservations, int clientId, DateTime ed)
        {
            foreach (var rsv in reservations)
            {
                if (rsv.ActualBeginDateTime == null)
                {
                    // handle unstarted reservations
                    repo.CancelAndForgiveReservation(rsv.ReservationID, "Cancelled and forgiven for facility down time.", clientId);
                    repo.EmailOnCanceledByRepair(rsv.ReservationID, true, "LNF Facility Down", "Facility is down, thus we have to disable the tool.", ed, clientId);
                }
                else
                {
                    // handle started reservations
                }

                // [jg 2019-09-12] Original comment no longer accurate (see below).
                // We have to disable all those reservations that have been activated by setting IsActive to 0.  
                // The catch here is that we must compare the "Actual" usage time with the repair time because if the user ends the reservation before the repair starts, we still 
                // have to charge the user for that reservation

                // [jg 2019-09-12] Started reservations should not be cancelled (IsActive = 0). Rather they should be ended and not forgiven
                // (tool engineers can forgive manually if needed). Repairs should be ignored. A tool can be in repair and have a FDT at
                // the same time (per Sandrine) becuase when the FDT is over the tool should still be in repair until the repair is ended.

                if (!IsRepair(rsv))
                {
                    // Non repair reservations should be ended (not cancelled) and not forgiven.
                    // The user will have to request that the reservation be forgiven.
                    // We only need to deal with in-progress reservations.
                    if (rsv.ActualEndDateTime == null)
                    {
                        //Provider.Scheduler.Reservation.EndReservation(New EndReservationArgs(existing.ReservationID, Date.Now, CurrentUser.ClientID))
                        repo.EndReservation(rsv.ReservationID, clientId);
                    }
                }
            }
        }

        private bool IsRepair(FacilityDownTimeReservation rsv)
        {
            return !rsv.Editable;
        }

        private void ValidateDateRange(IDateRange rng)
        {
            // Calculate the correct time format (AM/PM)
            if (rng.Start >= rng.End)
            {
                throw new Exception("Error: Please make sure your start time and end time are correct.");
            }
        }

        /// <summary>
        /// Creates a new SqlConnection object and opens it.
        /// </summary>
        private SqlConnection GetConnection()
        {
            var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString);
            conn.Open();
            return conn;
        }

        public bool IsReusable => false;
    }
}