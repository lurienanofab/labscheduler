using LNF.Scheduler;
using System;
using System.Web;

namespace LNF.Web.Scheduler.TreeView
{
    public class ResourceNode : TreeViewNode<IResourceTree>
    {
        public bool IsSchedulable { get; private set; }
        public ResourceState State { get; private set; }
        public string StateNotes { get; private set; }
        public DateTime? RepairEndDateTime { get; private set; }
        public string RepairNotes { get; private set; }
        public ClientAuthLevel AuthLevel { get; private set; }
        public ClientAuthLevel EveryoneAuthLevel { get; private set; }
        public ClientAuthLevel EffectiveAuthLevel { get; private set; }

        public ResourceNode(SchedulerResourceTreeView view, IResourceTree item, INode parent) : base(view, item, parent)
        {
            Load();
        }

        public override string GetUrl(HttpContextBase context)
        {
            return VirtualPathUtility.ToAbsolute(string.Format("~/ResourceDayWeek.aspx?Path={0}&Date={1:yyyy-MM-dd}", context.Server.UrlEncode(Value), context.Request.SelectedDate()));
        }

        public override string GetImageUrl(HttpContextBase context)
        {
            return string.Format("/scheduler/image/resource_icon/{0}", ID);
        }

        public override bool IsExpanded(string path) => PathInfo.Parse(path).ResourceID == ID;

        protected override void Load()
        {
            ID = Item.ResourceID;
            Name = Resources.CleanResourceName(Item.ResourceName);
            Description = Item.ResourceDescription;
            IsSchedulable = Item.IsSchedulable;
            State = Item.State;
            StateNotes = Item.StateNotes;
            RepairEndDateTime = null;
            RepairNotes = string.Empty;
            AuthLevel = Item.AuthLevel;
            EveryoneAuthLevel = Item.EveryoneAuthLevel;
            EffectiveAuthLevel = Item.EffectiveAuthLevel;

            if (State == ResourceState.Online && !IsSchedulable)
            {
                ReservationInProgress repair = Reservations.GetRepairReservationInProgress(Item);
                if (repair != null)
                {
                    RepairEndDateTime = repair.EndDateTime;
                    RepairNotes = repair.Notes;
                }
            }
        }

        public ClientAuthLevel GetClientAuth() => EffectiveAuthLevel;

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
