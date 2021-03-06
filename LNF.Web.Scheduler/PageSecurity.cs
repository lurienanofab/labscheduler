using LNF.Cache;
using LNF.Data;
using LNF.Scheduler;
using LNF.Web.Scheduler.Content;
using System.IO;

namespace LNF.Web.Scheduler
{
    /// <summary>
    /// Page access right checking, mainly used to check if user types url directly.
    /// </summary>
    public class PageSecurity
    {
        public static readonly ClientPrivilege DefaultAuthTypes = ClientPrivilege.LabUser | ClientPrivilege.Administrator | ClientPrivilege.Staff | ClientPrivilege.WebSiteAdmin;

        public static readonly ClientPrivilege AdminAuthTypes = ClientPrivilege.Administrator;

        public static bool CheckAccessRight(SchedulerPage page, IClient currentUser)
        {
            var pageName = Path.GetFileName(page.AppRelativeVirtualPath).ToLower();

            if (pageName.StartsWith("admin"))
            {
                // Only administrator can see the admin related pages
                return currentUser.HasPriv(ClientPrivilege.Administrator);
            }
            else
            {
                switch (pageName)
                {
                    case "resourcedocs.aspx":
                    case "resourceconfig.aspx":
                    case "resourcemaintenance.aspx":
                        var resourceId = page.ContextBase.Request.SelectedPath().ResourceID;
                        if (resourceId > 0)
                        {
                            ClientAuthLevel authLevel = CacheManager.Current.GetAuthLevel(resourceId, currentUser);
                            // So far, only Tool Engineer can see the 3 pages
                            return (authLevel & ClientAuthLevel.ToolEngineer) > 0;
                        }
                        break;
                    default:
                        return true;
                }
            }

            return false;
        }
    }
}