using LNF.CommonTools;
using LNF.Data;
using LNF.Scheduler;
using LNF.Web.Scheduler.Models;
using LNF.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;

namespace LNF.Web.Scheduler
{
    public class ReservationHistoryUtility
    {
        private readonly IProvider _provider;

        private ReservationHistoryUtility(IProvider provider)
        {
            _provider = provider;
        }

        public static ReservationHistoryUtility Create(IProvider provider)
        {
            return new ReservationHistoryUtility(provider);
        }

        public IList<ReservationHistoryItem> GetReservationHistoryData(IClient client, DateTime? sd, DateTime? ed, bool includeCanceledForModification)
        {
            // Select Past Reservations
            var reservations = _provider.Scheduler.Reservation.SelectHistory(client.ClientID, sd.GetValueOrDefault(Reservations.MinReservationBeginDate), ed.GetValueOrDefault(Reservations.MaxReservationEndDate));
            var filtered = _provider.Scheduler.Reservation.FilterCancelledReservations(reservations, includeCanceledForModification);
            var result = ReservationHistoryItem.CreateList(filtered);
            return result;
        }

        public bool ReservationCanBeForgiven(IClient client, IReservationItem rsv, DateTime now, int maxForgivenDay, IEnumerable<IHoliday> holidays)
        {
            // first, only admins and staff can possibly forgive
            if (!client.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Staff | ClientPrivilege.Developer))
                return false;

            // always let admins forgive even after business day cutoff
            if (client.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer))
                return true;

            return IsBeforeForgiveCutoff(rsv, now, maxForgivenDay, holidays);
        }

        public bool ReservationAccountCanBeChanged(IClient client, IReservationItem rsv, DateTime now, IEnumerable<IHoliday> holidays)
        {
            // admins can always change the account
            if (client.HasPriv(ClientPrivilege.Administrator | ClientPrivilege.Developer))
                return true;

            // reserver and staff can before 3rd business day of next period
            if (client.HasPriv(ClientPrivilege.Staff) || client.ClientID == rsv.ClientID)
                return IsBeforeChangeAccountCutoff(rsv, now, holidays);

            return false;
        }

        public bool ReservationNotesCanBeChanged(IClient client, IReservationItem rsv)
        {
            // staff can always change notes
            if (client.HasPriv(ClientPrivilege.Staff | ClientPrivilege.Administrator | ClientPrivilege.Developer))
                return true;

            // otherwise notes can be changed by the reserver
            return client.ClientID == rsv.ClientID;
        }

        public bool IsBeforeForgiveCutoff(IReservationItem rsv, DateTime now, int maxForgivenDay, IEnumerable<IHoliday> holidays)
        {
            // Normal lab users cannot forgive reservations
            // Staff can forgive a reservation on the same day it ended, and during the following 3 three business days
            // Admins can always forgive reservations

            DateTime maxDay = rsv.ActualBeginDateTime.GetValueOrDefault(rsv.EndDateTime);
            return IsBeforeForgiveCutoff(now, maxDay, maxForgivenDay, rsv.Editable, holidays);
        }

        public bool IsBeforeForgiveCutoff(ReservationHistoryItem item, DateTime now, int maxForgivenDay, IEnumerable<IHoliday> holidays)
        {
            // Normal lab users cannot forgive reservations
            // Staff can forgive a reservation on the same day it ended, and during the following 3 three business days
            // Admins can always forgive reservations

            DateTime maxDay = item.ActualBeginDateTime.GetValueOrDefault(item.EndDateTime);
            return IsBeforeForgiveCutoff(now, maxDay, maxForgivenDay, item.Editable, holidays);
        }

        public bool IsBeforeForgiveCutoff(DateTime now, DateTime maxDay, int maxForgivenDay, bool editable, IEnumerable<IHoliday> holidays)
        {
            // Normal lab users cannot forgive reservations
            // Staff can forgive a reservation on the same day it ended, and during the following 3 three business days
            // Admins can always forgive reservations

            // start checking at the beginning of the next day
            DateTime cutoff = Utility.NextBusinessDay(maxDay.Date.AddDays(1), maxForgivenDay, holidays);

            // cutoff has now been incremented to the cutoff date - i.e. 3 (for example) business days after the reservation ended

            // Display Forgive Charge button 
            // extra if clause added by gpr, 1/3/05
            // 2011-07-11 Setting the cell visibility wasn't working, it's better to set the LinkButton control visibility.
            //       Also using cell index was getting messy because if a column is added all hell breaks loose. It's better
            //       to use FindControl on the whole row
            return now >= maxDay && now < cutoff && editable;
        }

        public bool IsBeforeChangeAccountCutoff(IReservationItem rsv, DateTime now, IEnumerable<IHoliday> holidays)
        {
            // Normal lab users can modify their own reservation's account on the same day it ended through 3 business days after the 1st of the following month
            // Staff can modify any reservation's account on the same day it ended through 3 business days after the 1st of the following month
            // Admins can always modify a reservation's account

            DateTime maxDay = rsv.ActualBeginDateTime.GetValueOrDefault(rsv.EndDateTime);

            // need to see if current date is between maxDay and next period business day cutoff
            var d = maxDay.AddMonths(1).FirstOfMonth();
            DateTime cutoff = Utility.NextBusinessDay(d, holidays);

            return now >= maxDay && now < cutoff && rsv.Editable;
        }

        public IEnumerable<ReservationHistoryClient> SelectReservationHistoryClients(DateTime sd, DateTime ed, int clientId)
        {
            // Dim canViewEveryone = CurrentUser.HasPriv(ClientPrivilege.Staff Or ClientPrivilege.Administrator Or ClientPrivilege.Developer)

            // allow everyone to see other users history
            bool canViewEveryone = true;

            var priv = ClientPrivilege.LabUser | ClientPrivilege.Staff;

            //Dim sd As Date = GetStartDate()
            //Dim ed As Date = GetEndDate()

            IEnumerable<IClient> clients = _provider.Data.Client.GetActiveClients(sd, ed, priv: priv).Where(x => canViewEveryone || x.ClientID == clientId).ToList();
            List<ReservationHistoryClient> result = CreateReservationHistoryClients(clients);

            return result;
        }

        public List<ReservationHistoryClient> CreateReservationHistoryClients(IEnumerable<IClient> clients)
        {
            List<ReservationHistoryClient> list = clients.Select(x => new ReservationHistoryClient { ClientID = x.ClientID, DisplayName = x.DisplayName }).ToList();
            List<ReservationHistoryClient> result = list.Distinct(new ReservationHistoryClientComparer()).OrderBy(x => x.DisplayName).ToList();
            return result;
        }

        private Message GetMessage(WorkerRequest body)
        {
            return new Message(body)
            {
                Formatter = new XmlMessageFormatter(new[] { typeof(WorkerRequest) })
            };
        }
    }
}
