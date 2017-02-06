using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using System;

namespace LNF.Web.Scheduler.TreeView
{
    public class ResourceTreeItem : TreeItem<ResourceModel>
    {
        public bool IsSchedulable { get; private set; }
        public ResourceState State { get; private set; }
        public string StateNotes { get; private set; }
        public DateTime? RepairEndDateTime { get; private set; }
        public string RepairNotes { get; private set; }

        public ResourceTreeItem(ResourceModel item, ITreeItem parent) : base(item, parent) { }

        public override TreeItemType Type { get { return TreeItemType.Resource; } }

        protected override void Load(ResourceModel item)
        {
            ID = item.ResourceID;
            Name = item.ResourceName;
            Description = item.Description;
            IsSchedulable = item.IsSchedulable;
            State = item.State;
            StateNotes = item.StateNotes;
            RepairEndDateTime = null;
            RepairNotes = string.Empty;

            if (State == ResourceState.Online && !IsSchedulable)
            {
                ReservationInProgress repair = ReservationUtility.GetRepairReservationInProgress(item.ResourceID);
                if (repair != null)
                {
                    RepairEndDateTime = repair.EndDateTime;
                    RepairNotes = repair.Notes;
                }
            }
        }

        public ClientAuthLevel GetClientAuth()
        {
            var rc = CacheManager.Current.GetCurrentResourceClient(ID);

            if (rc != null)
                return rc.AuthLevel;
            else
                return ClientAuthLevel.UnauthorizedUser;
        }

        public override string CssClass
        {
            get
            {
                string className = base.CssClass;

                ClientAuthLevel auth = GetClientAuth();

                if (IsSchedulable)
                {
                    if (auth == ClientAuthLevel.ToolEngineer)
                        className += " schedulable-engineer";
                    else if (auth >= ClientAuthLevel.AuthorizedUser)
                        className += " schedulable-user";
                }
                else
                    className += " not-schedulable";

                switch (State)
                {
                    case ResourceState.Online:
                        className += " online";
                        break;
                    case ResourceState.Offline:
                        className += " offline";
                        break;
                    case ResourceState.Limited:
                        className += " limited";
                        break;
                }

                return className;
            }
        }

        public override string ToolTip
        {
            get
            {
                string tt = Name + ": ";

                switch (State)
                {
                    case ResourceState.Online:
                        if (IsSchedulable)
                            tt += "Online";
                        else
                            tt += string.Format("Not Schedulable until approximately {0:M/d/yyyy h:mm tt}{2}Notes: {1}", RepairEndDateTime, RepairNotes, Environment.NewLine);
                        break;
                    case ResourceState.Offline:
                        tt += " Offline";
                        break;
                    case ResourceState.Limited:
                        tt += string.Format("Available with limited functionality{1}Notes: {0}", StateNotes, Environment.NewLine);
                        break;
                }

                return tt;
            }
        }
    }
}
