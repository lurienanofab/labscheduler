using LNF.Cache;
using LNF.CommonTools;
using LNF.Control;
using LNF.Data;
using LNF.Helpdesk;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Repository;
using LNF.Repository.Control;
using LNF.Repository.Data;
using LNF.Repository.Scheduler;
using LNF.Scheduler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace LNF.Web.Scheduler
{
    public static class AjaxUtility
    {
        public static IClientManager ClientManager => DA.Use<IClientManager>();

        public static async Task<object> HandleRequest(HttpContext context)
        {
            string action = context.Request["Action"].ToString();

            int resourceId = Utility.ConvertTo(context.Request["ResourceID"], 0);
            int clientId = Utility.ConvertTo(context.Request["ClientID"], 0);
            int ticketId = Utility.ConvertTo(context.Request["TicketID"], 0);

            string command = Utility.ConvertTo(context.Request["Command"], string.Empty);
            string message = string.Empty;

            object result = null;
            GenericResult gr;
            ResourceModel model = CacheManager.Current.ResourceTree().GetResource(resourceId);

            switch (action)
            {
                case "SaveReservationHistory":
                    throw new NotImplementedException();
                case "get-tool-engineers":
                    gr = new GenericResult();
                    GetToolEngineers(model, gr);
                    result = gr;
                    break;
                case "add-tool-engineer":
                    gr = new GenericResult();
                    AddToolEngineer(model, DA.Current.Single<Client>(clientId), gr);
                    GetToolEngineers(model, gr);
                    result = gr;
                    break;
                case "delete-tool-engineer":
                    gr = new GenericResult();
                    DeleteToolEngineer(model, DA.Current.Single<Client>(clientId), gr);
                    GetToolEngineers(model, gr);
                    result = gr;
                    break;
                case "get-buildings":
                    result = GetBuildings(DA.Current.Single<Resource>(resourceId));
                    break;
                case "get-labs":
                    int buildingId = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("BuildingID"), 0);
                    result = GetLabs(DA.Current.Single<Resource>(resourceId), DA.Current.Single<Building>(buildingId));
                    break;
                case "get-proctechs":
                    int labId = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("LabID"), 0);
                    result = GetProcessTechs(DA.Current.Single<Resource>(resourceId), DA.Current.Single<Lab>(labId));
                    break;
                case "add-resource":
                    result = AddResource();
                    break;
                case "upload-image":
                    result = UploadImage();
                    break;
                case "helpdesk-info":
                    result = HelpdeskInfo(DA.Current.Single<Resource>(resourceId));
                    break;
                case "helpdesk-list-tickets":
                    result = HelpdeskListTickets(DA.Current.Single<Resource>(resourceId));
                    break;
                case "helpdesk-detail":
                    result = HelpdeskDetail(ticketId);
                    break;
                case "helpdesk-post-message":
                    message = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("Message"), string.Empty);
                    message = string.Format("Posted from scheduler by: {0} ({1})\n----------------------------------------\n{2}", CacheManager.Current.CurrentUser.DisplayName, CacheManager.Current.Email, message);
                    result = HelpdeskPostMessage(ticketId, message);
                    break;
                case "send-hardware-issue-email":
                    message = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("message"), string.Empty);
                    string subject = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("subject"), string.Empty);
                    HelpdeskUtility.SendHardwareIssueEmail(CacheManager.Current.ResourceTree().GetResource(resourceId), CacheManager.Current.CurrentUser.ClientID, subject, message);
                    break;
                case "interlock":
                    bool state = Utility.ConvertTo(context.Request["State"], false);
                    int duration = Utility.ConvertTo(context.Request["Duration"], 0);
                    uint d = (duration >= 0) ? (uint)duration : 0;
                    result = await HandleInterlockRequest(command, resourceId, state, d);
                    break;
                case "test":
                    result = new GenericResult() { Success = true, Message = string.Format("current client: {0}", CacheManager.Current.CurrentUser.ClientID), Data = null, Log = null };
                    break;
                default:
                    result = new GenericResult
                    {
                        Success = false,
                        Message = "Invalid action."
                    };
                    break;
            }

            return result;
        }

        private static object GetScheduleItem(Reservation rsv)
        {
            DateTime start = rsv.ActualBeginDateTime ?? rsv.BeginDateTime;
            DateTime end = rsv.ActualEndDateTime ?? rsv.EndDateTime;
            bool allDay = start.Date != end.Date;

            return new
            {
                id = rsv.ReservationID,
                title = rsv.Client.DisplayName,
                allDay = allDay,
                start = start,
                end = end,
                editable = rsv.ActualBeginDateTime == null && !rsv.IsStarted && rsv.IsActive,
                resourceId = rsv.Resource.ResourceID,
                resourceName = rsv.Resource.ResourceName,
                granularity = rsv.Resource.Granularity,
                gracePeriod = rsv.Resource.GracePeriod
            };
        }

        private static GenericResult HelpdeskInfo(Resource res)
        {
            var result = new GenericResult();

            DataTable dt = HelpdeskUtility.GetTickets(res);
            List<object> list = new List<object>();

            if (dt != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new
                    {
                        resource_id = dr["resource_id"],
                        priority_urgency = dr["priority_urgency"]
                    });
                }
            }

            result.Data = new { tickets = list };

            return result;
        }

        private static GenericResult HelpdeskListTickets(Resource resource)
        {
            var result = new GenericResult();

            DataTable dt = HelpdeskUtility.GetTickets(resource);
            object[] list = new object[] { };

            if (dt != null)
            {
                list = dt.AsEnumerable().Select(x => new
                {
                    ticketID = x["ticketID"],
                    created = x["created"],
                    email = x["email"],
                    assigned_to = x["assigned_to"],
                    subject = x["subject"],
                    priority_desc = x["priority_desc"]
                }).ToArray();
            }

            result.Data = new { tickets = list };

            return result;
        }

        private static GenericResult HelpdeskDetail(int ticketId)
        {
            var result = new GenericResult();
            var svc = new Helpdesk.Service(ConfigurationManager.AppSettings["HelpdeskUrl"], ConfigurationManager.AppSettings["HelpdeskApiKey"]);
            TicketDetailResponse response = svc.SelectTicketDetail(ticketId);
            result.Success = !response.Error;
            result.Message = response.Message;
            result.Data = new { Detail = response.Detail, DisplayName = CacheManager.Current.CurrentUser.DisplayName, Email = CacheManager.Current.Email };
            return result;
        }

        private static GenericResult HelpdeskPostMessage(int ticketId, string message)
        {
            var result = new GenericResult();
            var svc = new Helpdesk.Service(ConfigurationManager.AppSettings["HelpdeskUrl"], ConfigurationManager.AppSettings["HelpdeskApiKey"]);
            TicketDetailResponse response = svc.PostMessage(ticketId, message);
            result.Success = !response.Error;
            result.Message = response.Message;
            result.Data = new { Detail = response.Detail, DisplayName = CacheManager.Current.CurrentUser.DisplayName, Email = CacheManager.Current.Email };
            return result;
        }

        public static async Task<GenericResult> HandleInterlockRequest(string command, int resourceId, bool state = false, uint duration = 0)
        {
            var result = new GenericResult();

            if (!string.IsNullOrEmpty(command))
            {
                Resource res = DA.Current.Query<Resource>().FirstOrDefault(x => x.ResourceID == resourceId);

                if (res != null)
                {
                    var inst = DA.Current.Query<ActionInstance>().FirstOrDefault(x => x.ActionID == res.ResourceID);

                    if (inst == null)
                    {
                        result.Success = false;
                        result.Message = "No interlock for this tool";
                        return result;
                    }

                    var p = inst.GetPoint();

                    switch (command)
                    {
                        case "get-point-state":
                            var br = await ServiceProvider.Current.Control.GetBlockState(p.Block);
                            var bs = br.BlockState;
                            if (bs.Points != null)
                            {
                                result.Data = bs.Points.First(x => x.PointID == p.PointID);
                                result.Message = "ok";
                            }
                            else
                            {
                                result.Success = false;
                                result.Message = "Unable to get block state";
                            }
                            break;
                        case "set-point-state":
                            var pr = await ServiceProvider.Current.Control.SetPointState(p, state, duration);

                            if (pr.Error)
                            {
                                result.Success = false;
                                result.Message = pr.Message;
                            }
                            else
                            {
                                result.Success = true;
                                result.Message = string.Format("command = {0}, resourceId = {1}, state = {2}, duration = {3}, message = ({4}), time = {5:yyyy-MM-dd HH:mm:ss}", command, resourceId, state, duration, pr.Message, DateTime.Now);
                            }
                            break;
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = string.Format("Resource not found with ResourceID {0}", resourceId);
                }
            }
            else
            {
                result.Success = false;
                result.Message = "Missing parameter: Command";
            }

            return result;
        }

        private static GenericResult UploadImage()
        {
            var result = new GenericResult();

            if (ServiceProvider.Current.Context.GetRequestFileCount() > 0)
            {
                string path = ServiceProvider.Current.Context.GetRequestValue("Path").ToString();
                int resourceId = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("ResourceID"), 0);
                if (!string.IsNullOrEmpty(path))
                {
                    if (resourceId > 0)
                    {
                        if (ServiceProvider.Current.Context.GetRequestFileContentLength(0) < 1048596)
                        {
                            string id = resourceId.ToString().PadLeft(6, '0');

                            //Check for existing files
                            string fileName = string.Format("images/{0}/{0}{1}.png", path, id);
                            string iconName = string.Format("images/{0}/{0}{1}_icon.png", path, id);
                            string filePhysicalName = Path.GetFullPath(ServiceProvider.Current.Context.GetRequestPhysicalApplicationPath() + fileName);
                            string iconPhysicalName = Path.GetFullPath(ServiceProvider.Current.Context.GetRequestPhysicalApplicationPath() + iconName);
                            if (File.Exists(filePhysicalName)) File.Delete(filePhysicalName);
                            if (File.Exists(iconPhysicalName)) File.Delete(iconPhysicalName);

                            //Save original image
                            Bitmap bImage = new Bitmap(ServiceProvider.Current.Context.GetRequestFileInputStream(0));
                            bImage.Save(filePhysicalName, ImageFormat.Png);

                            //Save icon image
                            int iconHeight = 32;
                            int iconWidth = Convert.ToInt32(bImage.Width * iconHeight / bImage.Height);
                            Bitmap bIcon = new Bitmap(bImage, iconWidth, iconHeight);
                            bIcon.Save(iconPhysicalName, ImageFormat.Png);

                            bImage.Dispose();
                            bImage = null;
                            bIcon.Dispose();
                            bIcon = null;

                            result.Data = new
                            {
                                FileUrl = VirtualPathUtility.ToAbsolute("~/" + fileName),
                                IconUrl = VirtualPathUtility.ToAbsolute("~/" + iconName)
                            };
                        }
                        else
                        {
                            result.Success = false;
                            result.Message = "Upload failed. The file size must not exceed 1 MB.";
                        }
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = "Upload failed. Required parameter ResourceID not specified.";
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = "Upload failed. Required parameter Path not specified.";
                }
            }

            return result;
        }

        private static GenericResult GetBuildings(Resource r)
        {
            var result = new GenericResult() { Success = true, Message = string.Empty, Data = null };

            int id = (r == null) ? 0 : r.ProcessTech.Lab.Building.BuildingID;
            IList<Building> query = DA.Current.Query<Building>().Where(x => x.IsActive).ToList();
            result.Data = query
                .Select(x => new { Text = x.BuildingName, Value = x.BuildingID, Selected = x.BuildingID == id })
                .OrderBy(x => x.Text);

            return result;
        }

        private static GenericResult GetLabs(Resource r, Building b)
        {
            var result = new GenericResult() { Success = true, Message = string.Empty, Data = null };

            if (b != null)
            {
                int id = (r == null) ? 0 : r.ProcessTech.Lab.LabID;
                IList<Lab> query = b.Labs().ToList();
                result.Data = query
                    .Select(x => new { Text = x.LabName, Value = x.LabID, Selected = x.LabID == id })
                    .OrderBy(x => x.Text);
            }
            else
            {
                result.Success = false;
                result.Message = "Invalid BuildingID.";
            }

            return result;
        }

        private static GenericResult GetProcessTechs(Resource r, Lab l)
        {
            var result = new GenericResult();

            if (l != null)
            {
                int id = (r == null) ? 0 : r.ProcessTech.ProcessTechID;
                IList<ProcessTech> query = l.GetProcessTechs().ToList();
                result.Data = query
                    .Select(x => new { Text = x.ProcessTechName, Value = x.ProcessTechID, Selected = x.ProcessTechID == id })
                    .OrderBy(x => x.Text);
            }
            else
            {
                result.Success = false;
                result.Message = "Invalid LabID.";
            }

            return result;
        }

        private static void GetToolEngineers(ResourceModel res, GenericResult result)
        {
            object data = null;

            if (Validate(res, result))
            {
                IList<ResourceClientInfo> toolEng = ResourceClientInfoUtility.GetToolEngineers(res.ResourceID).ToList();
                int[] existingToolEngClientIDs = toolEng.Select(x => x.ClientID).ToArray();
                IList<Client> staff = ClientManager.FindByPrivilege(ClientPrivilege.Staff, true).Where(x => !existingToolEngClientIDs.Contains(x.ClientID)).ToList();

                data = new
                {
                    ToolEngineers = toolEng.Select(x => new { ClientID = x.ClientID, DisplayName = x.DisplayName }).OrderBy(x => x.DisplayName),
                    Staff = staff.Select(x => new { ClientID = x.ClientID, DisplayName = x.DisplayName }).OrderBy(x => x.DisplayName)
                };
                result.Data = data;
            }
        }

        private static void AddToolEngineer(ResourceModel res, Client c, GenericResult result)
        {
            if (Validate(res, c, result))
            {
                ResourceClient rc = new ResourceClient
                {
                    AuthLevel = ClientAuthLevel.ToolEngineer,
                    ClientID = c.ClientID,
                    EmailNotify = null,
                    Expiration = null,
                    PracticeResEmailNotify = null,
                    Resource = DA.Current.Single<Resource>(res.ResourceID)
                };

                DA.Current.Insert(rc);
            }
        }

        private static void DeleteToolEngineer(ResourceModel res, Client c, GenericResult result)
        {
            if (Validate(res, c, result))
            {
                ResourceClientInfo rc = DA.Current.Query<ResourceClientInfo>().FirstOrDefault(x => x.ClientID == c.ClientID && x.ResourceID == res.ResourceID);
                if (Validate(rc, result))
                {
                    DA.Current.Delete(rc.GetResourceClient());
                }
            }
        }

        private static GenericResult AddResource()
        {
            var result = new GenericResult();

            if (Validate(result, out dynamic v))
            {
                Lab lab = DA.Current.Single<Lab>(v.LabID);

                Resource r = new Resource()
                {
                    ResourceID = v.EditResourceID,
                    ProcessTech = DA.Current.Single<ProcessTech>(v.ProcessTechID),
                    ResourceName = v.ResourceName,
                    IsSchedulable = v.Schedulable,
                    IsActive = v.Active,
                    Description = v.Description,
                    HelpdeskEmail = v.HelpdeskEmail,
                    UseCost = 0,
                    HourlyCost = 0,
                    ReservFence = 0,
                    Granularity = 5,
                    Offset = 0,
                    MinReservTime = 5,
                    MaxReservTime = 120,
                    MaxAlloc = 120,
                    MinCancelTime = 5,
                    GracePeriod = 5,
                    AutoEnd = 15,
                    AuthDuration = 12,
                    AuthState = false,
                    OTFSchedTime = 0,
                    IPAddress = string.Empty,
                    State = ResourceState.Online,
                    IsReady = false
                };

                DA.Current.Insert(r);

                DA.Current.Insert(new[]{
                    new Cost()
                    {
                        ChargeTypeID = 5,
                        TableNameOrDescription = "ToolCost",
                        RecordID = r.ResourceID,
                        AcctPer = "Hourly",
                        AddVal = 0,
                        MulVal = 0,
                        EffDate = DateTime.Now
                    },
                    new Cost()
                    {
                        ChargeTypeID = 15,
                        TableNameOrDescription = "ToolCost",
                        RecordID = r.ResourceID,
                        AcctPer = "Hourly",
                        AddVal = 0,
                        MulVal = 0,
                        EffDate = DateTime.Now
                    },
                    new Cost()
                    {
                        ChargeTypeID = 25,
                        TableNameOrDescription = "ToolCost",
                        RecordID = r.ResourceID,
                        AcctPer = "Hourly",
                        AddVal = 0,
                        MulVal = 0,
                        EffDate = DateTime.Now
                    }
                });

                result.Data = new { ResourceID = r.ResourceID };
            }

            return result;
        }

        //private static GenericResult UpdateResource()
        //{
        //    var result = new GenericResult();

        //    dynamic v;

        //    if (Validate(result, out v))
        //    {
        //        ApiResource item = new ApiResource()
        //        {
        //            ResourceID = v.ResourcID,
        //            ResourceName = v.ResourceName,
        //            LabID = v.LabID,
        //            ProcessTechID = v.ProcessTechID,
        //            Description = v.Description,
        //            HelpdeskEmail = v.HelpdeskEmail,
        //            IsActive = v.IsActive,
        //            IsSchedulable = v.IsSchedulable
        //        };

        //        ApiInstance.Current.Request("scheduler/resource").Put(item.ResourceID, item);
        //    }

        //    return result;
        //}

        [Obsolete("Use webapi from now on.")]
        private static Task<GenericResult> SaveReservationHistory()
        {
            throw new NotImplementedException();
            //int rsvId = Utility.ConvertTo(Providers.Context.Current.GetRequestValue("ReservationID"), 0);
            //string notes = Utility.ConvertTo(Providers.Context.Current.GetRequestValue("ReservationNotes"), "=x=");
            //double forgivenPct = Utility.ConvertTo(Providers.Context.Current.GetRequestValue("ReservationForgivenPercentage"), -1D);
            //int acctId = Utility.ConvertTo(Providers.Context.Current.GetRequestValue("ReservationAccountID"), 0);
            //bool emailClient = Utility.ConvertTo(Providers.Context.Current.GetRequestValue("EmailClient"), 0).Equals(1);
            //var result = await ReservationUtility.SaveReservationHistory(rsvId, notes, forgivenPct, acctId, emailClient);
            //return result;
        }

        private static bool Validate(GenericResult result, out object obj)
        {
            int buildingId = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("BuildingID"), 0);
            int labId = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("LabID"), 0);
            int proctechId = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("ProcessTechID"), 0);
            int resourceId = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("ResourceID"), 0);
            int editResourceId = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("EditResourceID"), 0);
            string resourceName = ServiceProvider.Current.Context.GetRequestValue("ResourceName").ToString();
            bool schedulable = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("Schedulable"), false);
            bool active = Utility.ConvertTo(ServiceProvider.Current.Context.GetRequestValue("Active"), false);
            string description = ServiceProvider.Current.Context.GetRequestValue("Description").ToString();
            string helpdeskEmail = ServiceProvider.Current.Context.GetRequestValue("HelpdeskEmail").ToString();

            obj = null;

            if (buildingId == 0)
            {
                result.Success = false;
                result.Message = "Invalid BuildingID.";
                return false;
            }

            if (labId == 0)
            {
                result.Success = false;
                result.Message = "Invalid LabID.";
                return false;
            }

            if (proctechId == 0)
            {
                result.Success = false;
                result.Message = "Invalid ProcessTechID.";
                return false;
            }

            if (editResourceId == 0)
            {
                result.Success = false;
                result.Message = "Invalid ResourceID.";
                return false;
            }

            if (string.IsNullOrEmpty(resourceName))
            {
                result.Success = false;
                result.Message = "Invalid ResourceName.";
                return false;
            }

            if (DA.Current.Single<Resource>(editResourceId) != null && editResourceId != resourceId)
            {
                result.Success = false;
                result.Message = string.Format("Resource ID {0} is already in use.", editResourceId);
                return false;
            }

            if (!string.IsNullOrEmpty(helpdeskEmail))
            {
                if (!ValidateEmail(helpdeskEmail))
                {
                    result.Success = false;
                    result.Message = "Helpdesk Email is not a valid email address.";
                    return false;
                }
            }

            obj = new
            {
                BuildingID = buildingId,
                LabID = labId,
                ProcessTechID = proctechId,
                ResourceID = resourceId,
                EditResourceID = editResourceId,
                ResourceName = resourceName,
                Schedulable = schedulable,
                Active = active,
                Description = description,
                HelpdeskEmail = helpdeskEmail
            };

            return true;
        }

        private static bool Validate(ResourceClientInfo rc, GenericResult result)
        {
            if (rc == null)
            {
                result.Success = false;
                AppendMessage(result, "Could not find a ResourceClient with the given ResourceID and ClientID.");
                return false;
            }

            return true;
        }

        private static bool Validate(ResourceModel res, GenericResult result)
        {
            if (res == null)
            {
                result.Success = false;
                AppendMessage(result, "Could not find a Resource with the given ResourceID.");
                return false;
            }

            return true;
        }

        private static bool Validate(Client c, GenericResult result)
        {
            if (c == null)
            {
                result.Success = false;
                AppendMessage(result, "Could not find a Client with the given ClientID.");
                return false;
            }

            return true;
        }

        private static bool Validate(ResourceModel res, Client c, GenericResult result)
        {
            if (!Validate(res, result)) return false;
            if (!Validate(c, result)) return false;
            return true;
        }

        private static bool ValidateEmail(string email)
        {
            try
            {
                MailAddress addr = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static void AppendMessage(GenericResult result, string text)
        {
            if (!result.Message.Contains(text))
                result.Message = (result.Message + " " + text).Trim();
        }
    }
}