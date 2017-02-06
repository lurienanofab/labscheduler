using LNF.Cache;
using LNF.Data;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Repository.Data;
using LNF.Scheduler;

namespace LNF.Web.Scheduler
{
    /// <summary>
    /// Page access right checking, mainly used to check if user types url directly.
    /// </summary>
    public class PageSecurity
    {
        public static readonly ClientPrivilege DefaultAuthTypes = ClientPrivilege.LabUser | ClientPrivilege.Administrator | ClientPrivilege.Staff | ClientPrivilege.WebSiteAdmin;

        public static readonly ClientPrivilege AdminAuthTypes = ClientPrivilege.Administrator;

        public static bool CheckAccessRight(string pageName, Client currentUser)
        {
            pageName = pageName.ToLower();

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
                        if (PathInfo.Current.ResourceID > 0)
                        {
                            ClientAuthLevel authLevel = CacheManager.Current.GetAuthLevel(PathInfo.Current.ResourceID, CacheManager.Current.ClientID);
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

        public static bool IsOnKiosk()
        {
            return CacheManager.Current.IsOnKiosk();
        }
    }
}