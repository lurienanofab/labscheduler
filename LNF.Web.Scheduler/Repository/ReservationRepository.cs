using LNF.Data;
using LNF.Scheduler;
using LNF.Web.Scheduler.Repository.Models.Data;
using LNF.Web.Scheduler.Repository.Models.Scheduler;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LNF.Web.Scheduler.Repository
{
    public class ReservationRepository : RepositoryBase, IReservationRepository
    {
        public IAutoEndLog AddAutoEndLog(int reservationId, string autoEndLogAction)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();

                int autoEndLogId;
                IAutoEndLog result;

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = CreateCommand(conn, "sselScheduler.dbo.procAutoEndLogInsert", tx))
                        {
                            AddParameter(cmd, "ReservationID", reservationId, DbType.Int32);
                            AddParameter(cmd, "AutoEndLogAction", autoEndLogAction, DbType.String);
                            var outParam = AddParameter(cmd, "AutoEndLogID", DbType.Int32, ParameterDirection.Output);
                            cmd.ExecuteNonQuery();
                            autoEndLogId = Convert.ToInt32(outParam.Value);
                        }

                        using (var cmd = CreateCommand(conn, "sselScheduler.dbo.procAutoEndLogSelect", tx))
                        {
                            AddParameter(cmd, "Action", "SelectOne", DbType.String);
                            AddParameter(cmd, "AutoEndLogID", autoEndLogId, DbType.Int32);
                            var dt = FillDataTable(cmd);
                            result = AutoMap<AutoEndLog>(dt).FirstOrDefault();
                        }

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

                return result;
            }
        }

        public IReservationInviteeItem AddInvitee(int reservationId, int inviteeId)
        {
            using (var conn = CreateConnection())
            {
                IReservationInviteeItem result;

                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = CreateCommand(conn, "sselScheduler.dbo.procReservationInviteeInsert", tx))
                        {
                            AddParameter(cmd, "ReservationID", reservationId, DbType.Int32);
                            AddParameter(cmd, "InviteeID", inviteeId, DbType.Int32);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = CreateCommand(conn, "sselScheduler.dbo.procReservationInviteeSelect", tx))
                        {
                            AddParameter(cmd, "Action", "SelectOne", DbType.String);
                            AddParameter(cmd, "ReservationID", reservationId, DbType.Int32);
                            AddParameter(cmd, "ClientID", inviteeId, DbType.Int32);
                            var dt = FillDataTable(cmd);
                            result = AutoMap<ReservationInviteeItem>(dt).FirstOrDefault();
                        }

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }

                return result;
            }
        }

        public void AppendNotes(int reservationId, string notes)
        {
            using (var conn = CreateConnection())
            using (var cmd = CreateCommand(conn, "sselScheduler.dbo.procReservationUpdate"))
            {
                conn.Open();
                AddParameter(cmd, "Action", "AppendNotes", DbType.String);
                AddParameter(cmd, "ReservationID", reservationId, DbType.Int32);
                AddParameter(cmd, "AppendNotes", notes, DbType.String);
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public IEnumerable<IClientAccount> AvailableAccounts(int reservationId, ActivityAccountType activityAccountType)
        {
            using (var conn = CreateConnection())
            using (var cmd = CreateCommand(conn, "sselScheduler.dbo.procReservationSelect"))
            {
                AddParameter(cmd, "Action", "AvailableAccounts", DbType.String);
                AddParameter(cmd, "ReservationID", reservationId, DbType.Int32);
                AddParameter(cmd, "ActivityAccountType", (int)activityAccountType, DbType.Int32);
                var dt = FillDataTable(cmd);
                var result = AutoMap<ClientAccount>(dt);
                return result;
            }
        }

        public void CancelAndForgive(int reservationId, string note, int? modifiedByClientId)
        {
            using (var conn = CreateConnection())
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = CreateCommand(conn, "sselScheduler.dbo.procReservationUpdate", tx))
                        {
                            AddParameter(cmd, "Action", "AppendNotes", DbType.String);
                            AddParameter(cmd, "ReservationID", reservationId, DbType.Int32);
                            AddParameter(cmd, "AppendNotes", note, DbType.String);
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = CreateCommand(conn, "sselScheduler.dbo.procReservationDelete", tx))
                        {
                            AddParameter(cmd, "Action", "WithForgive", DbType.String);
                            AddParameter(cmd, "ReservationID", reservationId, DbType.Int32);
                            AddParameter(cmd, "ModifiedByClientID", modifiedByClientId, DbType.Int32);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        throw ex;
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        public int CancelByGroup(int groupId, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public int CancelByRecurrence(int recurrenceId, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public void CancelReservation(int reservationId, string note, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public IReservation CreateReservation(int resourceId, int clientId, int accountId, int activityId, DateTime beginDateTime, DateTime endDateTime, double duration, string notes, bool autoEnd, bool hasProcessInfo, bool hasInvitees, int? recurrenceId, bool isActive, bool keepAlive, double maxReservedDuration, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public bool DeleteInvitee(int reservationId, int inviteeId)
        {
            throw new NotImplementedException();
        }

        public void EndAndForgiveForRepair(int reservationId, string notes, int? endedByClientId, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public int EndPastUnstarted(int reservationId, DateTime endDate, int? endedByClientId)
        {
            throw new NotImplementedException();
        }

        public void EndReservation(EndReservationArgs args)
        {
            throw new NotImplementedException();
        }

        public void ExtendReservation(int reservationId, int totalMinutes, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ReservationHistoryFilterItem> FilterCancelledReservations(IEnumerable<IReservationItem> reservations, bool includeCanceledForModification)
        {
            throw new NotImplementedException();
        }

        public int[] FilterInvitedReservations(int[] reservationIds, int clientId)
        {
            throw new NotImplementedException();
        }

        public IReservation FromDataRow(DataRow dr)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AvailableInvitee> GetAvailableInvitees(int reservationId, int resourceId, int activityId, int clientId)
        {
            throw new NotImplementedException();
        }

        public int GetAvailableSchedMin(int resourceId, int clientId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> GetCurrentReservations()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationHistory> GetHistory(int reservationId)
        {
            throw new NotImplementedException();
        }

        public IReservationInviteeItem GetInvitee(int reservationId, int inviteeId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationInviteeItem> GetInvitees(int reservationId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationInviteeItem> GetInvitees(int[] reservations)
        {
            throw new NotImplementedException();
        }

        public DateTime? GetLastRepairEndTime(int resourceId)
        {
            throw new NotImplementedException();
        }

        public IReservation GetNextReservation(int resourceId, int reservationId)
        {
            throw new NotImplementedException();
        }

        public IReservation GetPreviousRecurrence(int recurrenceId, int exclude = 0)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> GetRecurringReservations(int recurrenceId, DateTime? sd, DateTime? ed)
        {
            throw new NotImplementedException();
        }

        public double GetReservableMinutes(int resourceId, int clientId, TimeSpan reservFence, TimeSpan maxAlloc, DateTime now)
        {
            throw new NotImplementedException();
        }

        public IReservation GetReservation(int reservationId)
        {
            throw new NotImplementedException();
        }

        public IReservationItem GetReservationItem(int reservationId)
        {
            throw new NotImplementedException();
        }

        public IReservationRecurrence GetReservationRecurrence(int recurrenceId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationRecurrence> GetReservationRecurrencesByClient(int clientId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationRecurrence> GetReservationRecurrencesByLabLocation(int labLocationId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationRecurrence> GetReservationRecurrencesByProcessTech(int processTechId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationRecurrence> GetReservationRecurrencesByResource(int resourceId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> GetReservations(DateTime sd, DateTime ed, int clientId = 0, int resourceId = 0, int activityId = 0, bool? started = null, bool? active = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ReservationStateItem> GetReservationStates(DateTime sd, DateTime ed, string kioskIp, int? clientId = null, int? resourceId = null, int? reserverId = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationWithInvitees> GetReservationsWithInvitees(DateTime sd, DateTime ed, int clientId = 0, int resourceId = 0, int activityId = 0, bool? started = null, bool? active = null)
        {
            throw new NotImplementedException();
        }

        public IReservationWithInvitees GetReservationWithInvitees(int reservationId)
        {
            throw new NotImplementedException();
        }

        public IResource GetResource(int reservationId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IResourceClient> GetResourceClients(int resourceId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IResource> GetResources(IEnumerable<IReservationItem> reservations)
        {
            throw new NotImplementedException();
        }

        public AvailableReservationMinutesResult GetAvailableReservationMinutes(IResource res, int reservationId, int clientId, DateTime beginDateTime)
        {
            throw new NotImplementedException();
        }

        public IReservation InsertFacilityDownTime(int resourceId, int clientId, int groupId, DateTime beginDateTime, DateTime endDateTime, string notes, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public IReservationItem InsertForModification(InsertReservationArgs args)
        {
            throw new NotImplementedException();
        }

        public IReservation InsertRepair(int resourceId, int clientId, DateTime beginDateTime, DateTime endDateTime, DateTime actualBeginDateTime, string notes, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public IReservationItem InsertReservation(InsertReservationArgs args)
        {
            throw new NotImplementedException();
        }

        public int InsertReservationRecurrence(int resourceId, int clientId, int accountId, int activityId, int patternId, int param1, int? param2, DateTime beginDate, DateTime? endDate, DateTime beginTime, double duration, bool autoEnd, bool keepAlive, string notes)
        {
            throw new NotImplementedException();
        }

        public bool InviteeExists(int reservationId, int inviteeId)
        {
            throw new NotImplementedException();
        }

        public bool IsInvited(int reservationId, int clientId)
        {
            throw new NotImplementedException();
        }

        public int PurgeReservation(int reservationId)
        {
            throw new NotImplementedException();
        }

        public int PurgeReservations(int resourceId, DateTime sd, DateTime ed)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> ReservationsInGranularityWindow(IResource res)
        {
            throw new NotImplementedException();
        }

        public SaveReservationHistoryResult SaveReservationHistory(IReservationItem rsv, int accountId, double? forgivenPct, string notes, bool emailClient, int modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public bool SaveReservationRecurrence(int recurrenceId, int patternId, int param1, int? param2, DateTime beginDate, TimeSpan beginTime, double duration, DateTime? endDate, bool autoEnd, bool keepAlive, string notes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> SelectAutoEnd()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationItem> SelectByClient(int clientId, DateTime sd, DateTime ed, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationItem> SelectByDateRange(DateTime sd, DateTime ed, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationItem> SelectByGroup(int groupId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationItem> SelectByLabLocation(int labLocationId, DateTime sd, DateTime ed, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationItem> SelectByProcessTech(int procTechId, DateTime sd, DateTime ed, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationItem> SelectByResource(int resourceId, DateTime sd, DateTime ed, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<FutureReservation> SelectFutureReservations()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> SelectEndableReservations(int resourceId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> SelectExisting(int resourceId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> SelectHistory(int clientId, DateTime sd, DateTime ed)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ReservationToForgiveForRepair> SelectHistoryToForgiveForRepair(int resourceId, DateTime sd, DateTime ed)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationInviteeItem> SelectInviteesByClient(int clientId, DateTime sd, DateTime ed, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationInviteeItem> SelectInviteesByDateRange(DateTime sd, DateTime ed, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationInviteeItem> SelectInviteesByLabLocation(int labLocationId, DateTime sd, DateTime ed, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationInviteeItem> SelectInviteesByProcessTech(int processTechId, DateTime sd, DateTime ed, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservationInviteeItem> SelectInviteesByResource(int resourceId, DateTime sd, DateTime ed, bool includeDeleted)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> SelectOverwritable(int resourceId, DateTime sd, DateTime ed)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> SelectPastEndableRepair()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> SelectPastUnstarted()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<RecentReservation> SelectRecentReservations(int resourceId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> SelectReservationsByPeriod(DateTime period)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReservation> SelectUnstarted(int resourceId, DateTime sd, DateTime ed)
        {
            throw new NotImplementedException();
        }

        public void StartReservation(int reservationId, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public void UpdateAccount(int reservationId, int accountId, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public int UpdateByGroup(int groupId, DateTime sd, DateTime ed, string notes, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public void UpdateCharges(int reservationId, string notes, double chargeMultiplier, bool applyLateChargePenalty, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public IReservationItem UpdateFacilityDownTime(int reservationId, DateTime beginDateTime, DateTime endDateTime, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public void UpdateNotes(int reservationId, string notes)
        {
            throw new NotImplementedException();
        }

        public IReservation UpdateRepair(int reservationId, DateTime endDateTime, string notes, int? modifiedByClientId)
        {
            throw new NotImplementedException();
        }

        public IReservationItem UpdateReservation(UpdateReservationArgs args)
        {
            throw new NotImplementedException();
        }

        public IReservationGroup UpdateReservationGroup(int groupId, DateTime beginDateTime, DateTime endDateTime)
        {
            throw new NotImplementedException();
        }

        public bool UpdateReservationHistory(ReservationHistoryUpdate model)
        {
            throw new NotImplementedException();
        }
    }
}
