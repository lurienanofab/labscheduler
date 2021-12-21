using LNF.Data;
using LNF.Scheduler;
using System;

namespace LNF.Web.Scheduler.Repository.Models.Scheduler
{
    public class ReservationInviteeItem : IReservationInviteeItem
    {
        public int InviteeID { get; set; }
        public int ReservationID { get; set; }
        public int ResourceID { get; set; }
        public int ProcessTechID { get; set; }
        public DateTime BeginDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public DateTime? ActualBeginDateTime { get; set; }
        public DateTime? ActualEndDateTime { get; set; }
        public bool IsStarted { get; set; }
        public bool IsActive { get; set; }
        public bool InviteeActive { get; set; }
        public string InviteeLName { get; set; }
        public string InviteeFName { get; set; }
        public string InviteeDisplayName => Clients.GetDisplayName(InviteeLName, InviteeFName);
        public ClientPrivilege InviteePrivs { get; set; }
    }
}
