using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Scheduler;
using System;

namespace LNF.Web.Scheduler.TreeView
{
    public class ResourceNode : TreeViewNode<ResourceTreeItem>
    {
        public bool IsSchedulable { get; private set; }
        public ResourceState State { get; private set; }
        public string StateNotes { get; private set; }
        public DateTime? RepairEndDateTime { get; private set; }
        public string RepairNotes { get; private set; }
        public ClientAuthLevel AuthLevel { get; private set; }
        public ClientAuthLevel EveryoneAuthLevel { get; private set; }
        public ClientAuthLevel EffectiveAuthLevel { get; private set; }

        public ResourceNode(ResourceTreeItem item, INode parent) : base(item, parent) { }

        public override NodeType Type { get { return NodeType.Resource; } }

        protected override void Load(ResourceTreeItem item)
        {
            ID = item.ResourceID;
            Name = item.ResourceName;
            Description = item.ResourceDescription;
            IsSchedulable = item.IsSchedulable;
            State = item.State;
            StateNotes = item.StateNotes;
            RepairEndDateTime = null;
            RepairNotes = string.Empty;
            AuthLevel = item.AuthLevel;
            EveryoneAuthLevel = item.EveryoneAuthLevel;
            EffectiveAuthLevel = item.EffectiveAuthLevel;

            if (State == ResourceState.Online && !IsSchedulable)
            {
                ReservationInProgress repair = ReservationUtility.GetRepairReservationInProgress(item);
                if (repair != null)
                {
                    RepairEndDateTime = repair.EndDateTime;
                    RepairNotes = repair.Notes;
                }
            }
        }

        public ClientAuthLevel GetClientAuth()
        {
            return EffectiveAuthLevel;
            //var rc = CacheManager.Current.GetCurrentResourceClient(ID);

            //if (rc != null)
            //    return rc.AuthLevel;
            //else
            //    return ClientAuthLevel.UnauthorizedUser;
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
