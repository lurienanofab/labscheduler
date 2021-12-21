using LNF.Cache;
using LNF.Data;
using LNF.Scheduler;
using LNF.Web.Content;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Pages
{
    public class CacheUtility : OnlineServicesPage
    {
        #region Controls
        protected Label lblClientCount;
        protected Label lblOrgCount;
        protected Label lblAccountCount;
        protected Label lblClientAccountCount;
        protected Label lblClientOrgCount;
        protected Label lblRoomCount;
        protected Label lblActivityCount;
        protected Label lblSchedulerPropertyCount;
        protected Label lblApproximateSize;
        protected Label lblKeyMessage;
        protected TextBox txtKey;
        protected TextBox txtValue;
        protected TextBox txtExpire;
        protected GridView gv;
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["mode"] == "api")
            {
                HandleApiRequest();
                return;
            }

            if (!string.IsNullOrEmpty(Request.QueryString["refresh"]))
            {
                CacheManager.Current.RemoveValue(Request.QueryString["refresh"]);
                Response.Redirect("~/CacheUtility.aspx");
            }

            lblClientCount.Text = Provider.Data.Client.GetActiveClients().Count().ToString();
            lblOrgCount.Text = CacheManager.Current.Orgs().Count().ToString();
            lblAccountCount.Text = CacheManager.Current.Accounts().Count().ToString();
            lblClientAccountCount.Text = "n/a"; //CacheManager.Current.ClientAccounts().Count().ToString();
            lblClientOrgCount.Text = CacheManager.Current.ClientOrgs().Count().ToString();
            lblRoomCount.Text = CacheManager.Current.Rooms().Count().ToString();
            lblActivityCount.Text = CacheManager.Current.Activities().Count().ToString();
            lblSchedulerPropertyCount.Text = CacheManager.Current.SchedulerProperties().Count().ToString();

            // assuming the approximate size is returned in bytes...
            var sizeBytes = (double)CacheManager.Current.GetApproximateSize();
            if (sizeBytes >= 0)
            {
                var sizeKilobytes = sizeBytes / 1024;
                var sizeMegabytes = sizeKilobytes / 1024;
                lblApproximateSize.Text = $"{sizeMegabytes:0.00} MB";
            }
            else
            {
                lblApproximateSize.Text = "[error]";
            }
        }

        protected void Key_Command(object sender, CommandEventArgs e)
        {
            lblKeyMessage.Text = string.Empty;

            if (e.CommandName == "clear")
            {
                CacheManager.Current.ClearCache();
            }
            else if (!string.IsNullOrEmpty(txtKey.Text))
            {
                if (e.CommandName == "get")
                    LoadKeyValue(txtKey.Text);

                if (e.CommandName == "set")
                    CacheManager.Current.SetValue(txtKey.Text, txtValue.Text, GetAbsoluteExpiration());

                if (e.CommandName == "delete")
                    CacheManager.Current.RemoveValue(txtKey.Text);
            }

            lblKeyMessage.Text = "OK";
        }

        private DateTimeOffset? GetAbsoluteExpiration()
        {
            DateTimeOffset? result = null;

            if (!string.IsNullOrEmpty(txtExpire.Text))
            {
                var ts = TimeSpan.Parse(txtExpire.Text);
                result = DateTimeOffset.Now.Add(ts);
            }

            return result;
        }

        private void LoadKeyValue(string key)
        {
            txtValue.Text = string.Empty;

            var value = CacheManager.Current.GetValue(key);

            if (value != null)
            {
                System.Collections.IEnumerable enumerable = null;

                if (!(value is string))
                    enumerable = value as System.Collections.IEnumerable;

                if (enumerable != null)
                {
                    gv.DataSource = enumerable;
                    gv.DataBind();
                }
                else
                    txtValue.Text = value.ToString();
            }
        }

        protected void HandleApiRequest()
        {
            Response.ContentType = "application/json";

            try
            {
                var command = Request.QueryString["command"];
                var key = Request.QueryString["key"];
                var value = Request.QueryString["value"];
                var expire = Request.QueryString["expire"]; // # of seconds until the cached value is expired

                if (command == "get")
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        Response.StatusCode = 400;
                        throw new Exception("Invalid parameter: key");
                    }

                    Response.Write(JsonConvert.SerializeObject(new { command, key, value = CacheManager.Current.GetValue(key) }));
                }
                else if (command == "set")
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        Response.StatusCode = 400;
                        throw new Exception("Invalid parameter: key");
                    }

                    if (string.IsNullOrEmpty(value))
                    {
                        Response.StatusCode = 400;
                        throw new Exception("Invalid parameter: value");
                    }

                    DateTimeOffset? absoluteExpiration = null;

                    if (!string.IsNullOrEmpty(expire) && double.TryParse(expire, out double e))
                        absoluteExpiration = DateTimeOffset.Now.AddSeconds(e);

                    CacheManager.Current.SetValue(key, value, absoluteExpiration);

                    Response.Write(JsonConvert.SerializeObject(new { command, key, value, absoluteExpiration }));
                }
                else if (command == "delete")
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        Response.StatusCode = 400;
                        throw new Exception("Invalid parameter: key");
                    }

                    Response.Write(JsonConvert.SerializeObject(new { command, key, value = CacheManager.Current.RemoveValue(key) }));
                }
                else if (command == "refresh")
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        Response.StatusCode = 400;
                        throw new Exception("Invalid parameter: key");
                    }

                    switch (key)
                    {
                        case "Clients":
                            CacheManager.Current.RemoveValue("Clients");
                            Response.Write(JsonConvert.SerializeObject(new { command, key, count = Provider.Data.Client.GetActiveClients().Count() }));
                            break;
                        case "Orgs":
                            CacheManager.Current.RemoveValue("Orgs");
                            Response.Write(JsonConvert.SerializeObject(new { command, key, count = CacheManager.Current.Orgs().Count() }));
                            break;
                        case "Accounts":
                            CacheManager.Current.RemoveValue("Accounts");
                            Response.Write(JsonConvert.SerializeObject(new { command, key, count = CacheManager.Current.Accounts().Count() }));
                            break;
                        //case "ClientAccounts":
                        //    CacheManager.Current.RemoveMemoryCacheValue("ClientAccounts");
                        //    Response.Write(JsonConvert.SerializeObject(new { command, key, count = CacheManager.Current.ClientAccounts().Count() }));
                        //    break;
                        case "ClientOrgs":
                            CacheManager.Current.RemoveValue("ClientOrgs");
                            Response.Write(JsonConvert.SerializeObject(new { command, key, count = CacheManager.Current.ClientOrgs().Count() }));
                            break;
                        case "Rooms":
                            CacheManager.Current.RemoveValue("Rooms");
                            Response.Write(JsonConvert.SerializeObject(new { command, key, count = CacheManager.Current.Rooms().Count() }));
                            break;
                        case "Activities":
                            CacheManager.Current.RemoveValue("Activities");
                            Response.Write(JsonConvert.SerializeObject(new { command, key, count = CacheManager.Current.Activities().Count() }));
                            break;
                        case "SchedulerProperties":
                            CacheManager.Current.RemoveValue("SchedulerProperties");
                            Response.Write(JsonConvert.SerializeObject(new { command, key, count = CacheManager.Current.SchedulerProperties().Count() }));
                            break;
                        default:
                            Response.Write(JsonConvert.SerializeObject(new { command, key, count = 0 }));
                            break;
                    }
                }
                else if (command == "clear-all")
                {
                    CacheManager.Current.ClearCache();
                    Response.Write(JsonConvert.SerializeObject(new { command }));
                }
                else if (command == "get-size")
                {
                    Response.Write(JsonConvert.SerializeObject(new { command, size = CacheManager.Current.GetApproximateSize() }));
                }
                else
                {
                    Response.Write(JsonConvert.SerializeObject(new
                    {
                        clients = Provider.Data.Client.GetActiveClients().Count(),
                        orgs = CacheManager.Current.Orgs().Count(),
                        accounts = CacheManager.Current.Accounts().Count(),
                        clientAccounts = -1, //CacheManager.Current.ClientAccounts().Count(),
                        clientOrgs = CacheManager.Current.ClientOrgs().Count(),
                        rooms = CacheManager.Current.Rooms().Count(),
                        activities = CacheManager.Current.Activities().Count()
                    }));
                }
            }
            catch (Exception ex)
            {
                if (Response.StatusCode == 200)
                    Response.StatusCode = 500;
                Response.Write(JsonConvert.SerializeObject(new { error = ex.Message }));
            }

            Response.End();
        }

        protected virtual string Trim(object value, int len)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            string result = value.ToString();

            if (result.Length > len)
                return result.Substring(0, len) + "...";
            else
                return result;
        }
    }
}
