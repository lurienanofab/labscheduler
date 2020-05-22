using LNF.CommonTools;
using LNF.Control;
using LNF.Data;
using LNF.Helpdesk;
using LNF.Impl.Repository.Data;
using LNF.Impl.Repository.Scheduler;
using LNF.Repository;
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
using System.Web;

namespace LNF.Web.Scheduler
{
    public static class AjaxUtility
    {
        public static object HandleRequest(HttpContextBase context, IProvider provider)
        {
            string action = context.Request["Action"].ToString();

            int resourceId = Utility.ConvertTo(context.Request["ResourceID"], 0);
            int clientId = Utility.ConvertTo(context.Request["ClientID"], 0);
            int ticketId = Utility.ConvertTo(context.Request["TicketID"], 0);

            string command = Utility.ConvertTo(context.Request["Command"], string.Empty);
            string message = string.Empty;

            object result = null;
            GenericResult gr;

            var res = provider.Scheduler.Reservation.GetResource(resourceId);
            var currentUser = context.CurrentUser(provider);

            switch (action)
            {
                case "SaveReservationHistory":
                    throw new NotImplementedException();
                case "get-tool-engineers":
                    gr = new GenericResult();
                    GetToolEngineers(provider, res, gr);
                    result = gr;
                    break;
                case "add-tool-engineer":
                    gr = new GenericResult();
                    AddToolEngineer(res, DA.Current.Single<Client>(clientId), gr);
                    GetToolEngineers(provider, res, gr);
                    result = gr;
                    break;
                case "delete-tool-engineer":
                    gr = new GenericResult();
                    DeleteToolEngineer(res, DA.Current.Single<Client>(clientId), gr);
                    GetToolEngineers(provider, res, gr);
                    result = gr;
                    break;
                case "get-buildings":
                    result = GetBuildings(DA.Current.Single<Resource>(resourceId));
                    break;
                case "get-labs":
                    int buildingId = Utility.ConvertTo(context.Request["BuildingID"], 0);
                    result = GetLabs(DA.Current.Single<Resource>(resourceId), DA.Current.Single<Building>(buildingId));
                    break;
                case "get-proctechs":
                    int labId = Utility.ConvertTo(context.Request["LabID"], 0);
                    result = GetProcessTechs(DA.Current.Single<Resource>(resourceId), DA.Current.Single<Lab>(labId));
                    break;
                case "add-resource":
                    result = AddResource(context);
                    break;
                case "upload-image":
                    result = UploadImage(context);
                    break;
                case "helpdesk-info":
                    result = HelpdeskInfo(provider.Scheduler.Resource.GetResource(resourceId));
                    break;
                case "helpdesk-list-tickets":
                    result = HelpdeskListTickets(provider.Scheduler.Resource.GetResource(resourceId));
                    break;
                case "helpdesk-detail":
                    result = HelpdeskDetail(ticketId, currentUser);
                    break;
                case "helpdesk-post-message":
                    message = Utility.ConvertTo(context.Request["Message"], string.Empty);
                    message = $"Posted from scheduler by: {currentUser.DisplayName} ({currentUser.Email})\n----------------------------------------\n{message}";
                    result = HelpdeskPostMessage(ticketId, message, currentUser);
                    break;
                case "send-hardware-issue-email":
                    message = Utility.ConvertTo(context.Request["message"], string.Empty);
                    string subject = Utility.ConvertTo(context.Request["subject"], string.Empty);
                    HelpdeskUtility.SendHardwareIssueEmail(res, currentUser.ClientID, subject, message);
                    break;
                case "interlock":
                    bool state = Utility.ConvertTo(context.Request["State"], false);
                    int duration = Utility.ConvertTo(context.Request["Duration"], 0);
                    uint d = (duration >= 0) ? (uint)duration : 0;
                    result = HandleInterlockRequest(provider, command, resourceId, state, d);
                    break;
                case "test":
                    result = new GenericResult() { Success = true, Message = $"current client: {currentUser.ClientID}", Data = null, Log = null };
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
                allDay,
                start,
                end,
                editable = rsv.ActualBeginDateTime == null && !rsv.IsStarted && rsv.IsActive,
                resourceId = rsv.Resource.ResourceID,
                resourceName = rsv.Resource.ResourceName,
                granularity = rsv.Resource.Granularity,
                gracePeriod = rsv.Resource.GracePeriod
            };
        }

        private static GenericResult HelpdeskInfo(IResource res)
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

        private static GenericResult HelpdeskListTickets(IResource res)
        {
            var result = new GenericResult();

            DataTable dt = HelpdeskUtility.GetTickets(res);
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

        private static GenericResult HelpdeskDetail(int ticketId, IClient currentUser)
        {
            var result = new GenericResult();
            var svc = new Helpdesk.Service(ConfigurationManager.AppSettings["HelpdeskUrl"], ConfigurationManager.AppSettings["HelpdeskApiKey"]);
            TicketDetailResponse response = svc.SelectTicketDetail(ticketId);
            result.Success = !response.Error;
            result.Message = response.Message;
            result.Data = new { response.Detail, currentUser.DisplayName, currentUser.Email };
            return result;
        }

        private static GenericResult HelpdeskPostMessage(int ticketId, string message, IClient currentUser)
        {
            var result = new GenericResult();
            var svc = new Helpdesk.Service(ConfigurationManager.AppSettings["HelpdeskUrl"], ConfigurationManager.AppSettings["HelpdeskApiKey"]);
            TicketDetailResponse response = svc.PostMessage(ticketId, message);
            result.Success = !response.Error;
            result.Message = response.Message;
            result.Data = new { response.Detail, currentUser.DisplayName, currentUser.Email };
            return result;
        }

        public static GenericResult HandleInterlockRequest(IProvider provider, string command, int resourceId, bool state = false, uint duration = 0)
        {
            var result = new GenericResult();

            if (!string.IsNullOrEmpty(command))
            {
                var res = provider.Scheduler.Resource.GetResource(resourceId);

                if (res != null)
                {
                    var inst = provider.Control.GetActionInstance(ActionType.Interlock, res.ResourceID);

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
                            var br = provider.Control.GetBlockState(p.BlockID);
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
                            var pr = provider.Control.SetPointState(p.PointID, state, duration);

                            if (pr.Error)
                            {
                                result.Success = false;
                                result.Message = pr.Message;
                            }
                            else
                            {
                                result.Success = true;
                                result.Message = $"command = {command}, resourceId = {resourceId}, state = {state}, duration = {duration}, message = ({pr.Message}), time = {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                            }
                            break;
                    }
                }
                else
                {
                    result.Success = false;
                    result.Message = $"Resource not found with ResourceID {resourceId}";
                }
            }
            else
            {
                result.Success = false;
                result.Message = "Missing parameter: Command";
            }

            return result;
        }

        private static GenericResult UploadImage(HttpContextBase ctx)
        {
            var result = new GenericResult();

            if (ctx.Request.Files.Count > 0)
            {
                string path = ctx.Request["Path"];
                int resourceId = Utility.ConvertTo(ctx.Request["ResourceID"], 0);
                if (!string.IsNullOrEmpty(path))
                {
                    if (resourceId > 0)
                    {
                        if (ctx.Request.Files[0].ContentLength < 1048596)
                        {
                            string id = resourceId.ToString().PadLeft(6, '0');

                            //Check for existing files
                            string fileName = $"images/{path}/{path}{id}.png";
                            string iconName = $"images/{path}/{path}{id}_icon.png";
                            string filePhysicalName = Path.GetFullPath(ctx.Request.PhysicalApplicationPath + fileName);
                            string iconPhysicalName = Path.GetFullPath(ctx.Request.PhysicalApplicationPath + iconName);
                            if (File.Exists(filePhysicalName)) File.Delete(filePhysicalName);
                            if (File.Exists(iconPhysicalName)) File.Delete(iconPhysicalName);

                            //Save original image
                            Bitmap bImage = new Bitmap(ctx.Request.Files[0].InputStream);
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
            IList<Building> query = DA.Current.Query<Building>().Where(x => x.BuildingIsActive).ToList();
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
                IList<Lab> query = DA.Current.Query<Lab>().Where(x => x.Building == b).ToList();
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
                IList<ProcessTech> query = DA.Current.Query<ProcessTech>().Where(x => x.Lab == l).ToList();
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

        private static void GetToolEngineers(IProvider provider, IResource res, GenericResult result)
        {
            object data = null;

            if (Validate(res, result))
            {
                IList<IResourceClient> toolEng = ResourceClients.GetToolEngineers(res.ResourceID).ToList();
                int[] existingToolEngClientIDs = toolEng.Select(x => AsAuthorized(x).ClientID).ToArray();
                IList<IClient> staff = provider.Data.Client.FindByPrivilege(ClientPrivilege.Staff, true).Where(x => !existingToolEngClientIDs.Contains(x.ClientID)).ToList();

                data = new
                {
                    ToolEngineers = toolEng.Select(x => new { AsAuthorized(x).ClientID, x.DisplayName }).OrderBy(x => x.DisplayName),
                    Staff = staff.Select(x => new { x.ClientID, x.DisplayName }).OrderBy(x => x.DisplayName)
                };
                result.Data = data;
            }
        }

        private static IAuthorized AsAuthorized(IResourceClient rc)
        {
            return rc;
        }

        private static void AddToolEngineer(IResource res, Client c, GenericResult result)
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
                    ResourceID = res.ResourceID
                };

                DA.Current.Insert(rc);
            }
        }

        private static void DeleteToolEngineer(IResource res, Client c, GenericResult result)
        {
            if (Validate(res, c, result))
            {
                ResourceClientInfo rci = DA.Current.Query<ResourceClientInfo>().FirstOrDefault(x => x.ClientID == c.ClientID && x.ResourceID == res.ResourceID);
                if (Validate(rci, result))
                {
                    ResourceClient rc = DA.Current.Single<ResourceClient>(rci.ResourceClientID);
                    DA.Current.Delete(rc);
                }
            }
        }

        private static GenericResult AddResource(HttpContextBase ctx)
        {
            var result = new GenericResult();

            if (Validate(ctx, result, out dynamic v))
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

                result.Data = new { r.ResourceID };
            }

            return result;
        }

        private static bool Validate(HttpContextBase ctx, GenericResult result, out object obj)
        {
            int buildingId = Utility.ConvertTo(ctx.Request["BuildingID"], 0);
            int labId = Utility.ConvertTo(ctx.Request["LabID"], 0);
            int proctechId = Utility.ConvertTo(ctx.Request["ProcessTechID"], 0);
            int resourceId = Utility.ConvertTo(ctx.Request["ResourceID"], 0);
            int editResourceId = Utility.ConvertTo(ctx.Request["EditResourceID"], 0);
            string resourceName = ctx.Request["ResourceName"];
            bool schedulable = Utility.ConvertTo(ctx.Request["Schedulable"], false);
            bool active = Utility.ConvertTo(ctx.Request["Active"], false);
            string description = ctx.Request["Description"];
            string helpdeskEmail = ctx.Request["HelpdeskEmail"];

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
                result.Message = $"Resource ID {editResourceId} is already in use.";
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

        private static bool Validate(IResource res, GenericResult result)
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

        private static bool Validate(IResource res, Client c, GenericResult result)
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