<%@ WebHandler Language="C#" Class="LabScheduler.Ajax.LabLocationHandler" %>

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;
using Newtonsoft.Json;

namespace LabScheduler.Ajax
{
    public class LabLocationHandler : IHttpHandler, IRequiresSessionState
    {
        private HttpContext _context;
        private SqlConnection _conn;
        private int _statusCode = 200;

        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
            _context = context;

            string command;
            object result = null;

            using (_conn = new SqlConnection(ConfigurationManager.ConnectionStrings["cnSselData"].ConnectionString))
            {
                _conn.Open();

                try
                {
                    if (_context.Request.HttpMethod == "GET")
                    {
                        command = _context.Request.QueryString["Command"];

                        if (command == "get-lablocations")
                        {
                            var search = _context.Request.QueryString["Search"];
                            result = new { LabLocations = GetLabLocations(search) };
                        }
                        else if (command == "get-resource-lablocations")
                        {
                            var labLocationId = int.Parse(_context.Request.QueryString["LabLocationID"]);
                            var labId = GetLabID(labLocationId);
                            result = new { ResourceLabLocations = GetResourceLabLocations(labLocationId), AvailableResources = GetAvailableResources(labId) };
                        }
                        else if (command == "get-current-client-id")
                        {
                            result = new { ClientID = GetClientID() };
                        }
                        else if (command == "remove-session-vars")
                        {
                            RemoveSessionVars(_context);
                            result = new { Result = "Session vars removed OK!" };
                        }
                        else
                        {
                            _statusCode = 405;
                            throw new Exception("Method not allowed.");
                        }
                    }
                    else if (_context.Request.HttpMethod == "POST")
                    {
                        AuthCheck();

                        command = _context.Request.Form["Command"];

                        if (command == "delete-lablocation")
                        {
                            string locationName = _context.Request.Form["LocationName"];

                            int clientId = GetClientID();

                            if (!string.IsNullOrEmpty(locationName))
                            {
                                DeleteByLocationName(locationName, clientId);
                                RemoveSessionVars(_context);
                                result = new { LabLocations = GetLabLocations() };
                            }
                        }
                        else if (command == "delete-resource-lablocation")
                        {
                            var labLocationId = int.Parse(_context.Request.Form["LabLocationID"]);
                            int resourceLabLocationId = int.Parse(_context.Request.Form["ResourceLabLocationID"]);
                            var labId = GetLabID(labLocationId);
                            int clientId = GetClientID();

                            DeleteResourceLabLocation(resourceLabLocationId, clientId);

                            result = new { ResourceLabLocations = GetResourceLabLocations(labLocationId), AvailableResources = GetAvailableResources(labId) };
                        }
                        else if (command == "add-resource-lablocation")
                        {
                            var labLocationId = int.Parse(_context.Request.Form["LabLocationID"]);
                            var resourceId = int.Parse(_context.Request.Form["ResourceID"]);
                            var labId = GetLabID(labLocationId);

                            AddResourceLabLocation(labLocationId, resourceId);

                            result = new { ResourceLabLocations = GetResourceLabLocations(labLocationId), AvailableResources = GetAvailableResources(labId) };
                        }
                        else if (command == "modify-lablocation")
                        {
                            var labLocationId = int.Parse(_context.Request.Form["LabLocationID"]);
                            var locationName = _context.Request.Form["LocationName"];
                            var labId = int.Parse(_context.Request.Form["LabID"]);

                            if (string.IsNullOrEmpty(locationName))
                            {
                                _statusCode = 400;
                                throw new Exception("Name must not be blank.");
                            }

                            ModifyLabLocation(labLocationId, locationName, labId);

                            result = new { LabLocations = GetLabLocations() };
                        }
                        else if (command == "add-lablocation")
                        {
                            var locationName = _context.Request.Form["LocationName"];
                            var labId = Convert.ToInt32(_context.Request.Form["LabID"]);
                            AddLabLocation(locationName, labId);
                            result = new { LabLocations = GetLabLocations() };
                        }
                        else
                        {
                            _statusCode = 405;
                            throw new Exception("Method not allowed.");
                        }
                    }
                    else
                    {
                        _statusCode = 405;
                        throw new Exception("Method not allowed.");
                    }
                }
                catch (Exception ex)
                {
                    if (_statusCode == 200)
                        _statusCode = 500;

                    result = new { ErrorMessage = ex.Message };

                    _context.Server.ClearError();
                    _context.Response.TrySkipIisCustomErrors = true;
                    _context.Response.SuppressFormsAuthenticationRedirect = true;
                }

                _conn.Close();
            }

            _context.Response.ContentType = "application/json";
            _context.Response.StatusCode = _statusCode;
            _context.Response.Write(JsonConvert.SerializeObject(result));
        }

        public void AuthCheck()
        {
            if (!_context.User.Identity.IsAuthenticated)
            {
                _statusCode = 401;
                throw new Exception("Unauthorized.");
            }
        }

        public void RemoveSessionVars(HttpContext context)
        {
            context.Session.Remove("LabLocations");
            context.Session.Remove("ResourceLabLocations");
        }

        public void DeleteByLocationName(string locationName, int clientId)
        {
            using (var cmd = new SqlCommand("sselScheduler.dbo.procLabLocationDelete", _conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("Action", "ByLocationName");
                cmd.Parameters.AddWithValue("LocationName", locationName);
                cmd.Parameters.AddWithValue("ClientID", clientId);
                cmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<LabLocationModel> GetLabLocations(string search = null)
        {
            string sql;

            if (!string.IsNullOrEmpty(search))
            {
                sql = "SELECT DISTINCT ll.LabLocationID, ll.LabID, lab.DisplayName AS LabDisplayName, ll.LocationName "
                    + "FROM sselScheduler.dbo.LabLocation ll "
                    + "INNER JOIN sselScheduler.dbo.Lab lab ON lab.LabID = ll.LabID "
                    + "LEFT JOIN sselScheduler.dbo.ResourceLabLocation rll ON rll.LabLocationID = ll.LabLocationID "
                    + "LEFT JOIN sselScheduler.dbo.[Resource] res ON res.ResourceID = rll.ResourceID "
                    + "WHERE lab.DisplayName LIKE @Search OR res.ResourceName LIKE @Search OR res.ResourceID LIKE @Search OR ll.LocationName LIKE @Search "
                    + "ORDER BY lab.DisplayName, ll.LocationName";
            }
            else
            {
                sql = "SELECT ll.LabLocationID, ll.LabID, lab.DisplayName AS LabDisplayName, ll.LocationName "
                    + "FROM sselScheduler.dbo.LabLocation ll "
                    + "INNER JOIN sselScheduler.dbo.Lab lab ON lab.LabID = ll.LabID "
                    + "ORDER BY lab.DisplayName, ll.LocationName";
            }

            using (var cmd = new SqlCommand(sql, _conn))
            using (var adap = new SqlDataAdapter(cmd))
            {
                if (!string.IsNullOrEmpty(search))
                {
                    cmd.Parameters.AddWithValue("Search", string.Format("%{0}%", search));
                }

                var dt = new DataTable();
                adap.Fill(dt);

                var result = new List<LabLocationModel>();

                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new LabLocationModel
                    {
                        LabLocationID = dr.Field<int>("LabLocationID"),
                        LabID = dr.Field<int>("LabID"),
                        LabDisplayName = dr.Field<string>("LabDisplayName"),
                        LocationName = dr.Field<string>("LocationName")
                    });
                }

                return result;
            }
        }

        public IEnumerable<ResourceLabLocationModel> GetResourceLabLocations(int labLocationId)
        {
            using (var cmd = new SqlCommand("SELECT rll.ResourceLabLocationID, rll.LabLocationID, rll.ResourceID, res.ResourceName FROM sselScheduler.dbo.ResourceLabLocation rll INNER JOIN sselScheduler.dbo.[Resource] res ON res.ResourceID = rll.ResourceID WHERE rll.LabLocationID = @LabLocationID", _conn))
            using (var adap = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("LabLocationID", labLocationId);

                var dt = new DataTable();
                adap.Fill(dt);

                var result = new List<ResourceLabLocationModel>();

                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new ResourceLabLocationModel
                    {
                        ResourceLabLocationID = dr.Field<int>("ResourceLabLocationID"),
                        LabLocationID = dr.Field<int>("LabLocationID"),
                        ResourceID = dr.Field<int>("ResourceID"),
                        ResourceName = dr.Field<string>("ResourceName")
                    });
                }

                return result;
            }
        }

        public IEnumerable<AvailableResourceModel> GetAvailableResources(int labId)
        {
            using (var cmd = new SqlCommand("SELECT res.ResourceID, res.ResourceName FROM sselScheduler.dbo.[Resource] res INNER JOIN sselScheduler.dbo.ProcessTech pt ON pt.ProcessTechID = res.ProcessTechID WHERE res.IsActive = 1 AND pt.LabID = @LabID AND NOT EXISTS (SELECT * FROM sselScheduler.dbo.ResourceLabLocation rll WHERE rll.ResourceID = res.ResourceID) ORDER BY res.ResourceName", _conn))
            using (var adap = new SqlDataAdapter(cmd))
            {
                cmd.Parameters.AddWithValue("LabID", labId);

                var dt = new DataTable();
                adap.Fill(dt);

                var result = new List<AvailableResourceModel>();

                foreach (DataRow dr in dt.Rows)
                {
                    result.Add(new AvailableResourceModel
                    {
                        ResourceID = dr.Field<int>("ResourceID"),
                        ResourceName = dr.Field<string>("ResourceName")
                    });
                }

                return result;
            }
        }

        public void DeleteResourceLabLocation(int resourceLabLocationId, int clientId)
        {
            using (var cmd = new SqlCommand("INSERT sselScheduler.dbo.ResourceLabLocationDeleted (ResourceLabLocationID, LabLocationID, ResourceID, DeletedByClientID, DeletedDateTime) SELECT ResourceLabLocationID, LabLocationID, ResourceID, @ClientID, GETDATE() FROM sselScheduler.dbo.ResourceLabLocation WHERE ResourceLabLocationID = @ResourceLabLocationID", _conn))
            {
                cmd.Parameters.AddWithValue("ResourceLabLocationID", resourceLabLocationId);
                cmd.Parameters.AddWithValue("ClientID", clientId);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SqlCommand("DELETE sselScheduler.dbo.ResourceLabLocation WHERE ResourceLabLocationID = @ResourceLabLocationID", _conn))
            {
                cmd.Parameters.AddWithValue("ResourceLabLocationID", resourceLabLocationId);
                cmd.ExecuteNonQuery();
            }
        }

        public void AddResourceLabLocation(int labLocationId, int resourceId)
        {
            using (var cmd = new SqlCommand("INSERT sselScheduler.dbo.ResourceLabLocation (LabLocationID, ResourceID) VALUES (@LabLocationID, @ResourceID)", _conn))
            {
                cmd.Parameters.AddWithValue("LabLocationID", labLocationId);
                cmd.Parameters.AddWithValue("ResourceID", resourceId);
                cmd.ExecuteNonQuery();
            }
        }

        public void ModifyLabLocation(int labLocationId, string locationName, int labId)
        {
            if (string.IsNullOrEmpty(locationName))
            {
                _statusCode = 400;
                throw new Exception("Name must not be blank.");
            }

            int count;

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM sselScheduler.dbo.LabLocation WHERE LocationName = @LocationName AND LabLocationID <> @LabLocationID", _conn))
            {
                cmd.Parameters.AddWithValue("LabLocationID", labLocationId);
                cmd.Parameters.AddWithValue("LocationName", locationName);
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }

            if (count > 0)
            {
                _statusCode = 400;
                throw new Exception("This name is already in use.");
            }

            using (var cmd = new SqlCommand("UPDATE sselScheduler.dbo.LabLocation SET LocationName = @LocationName, LabID = @LabID WHERE LabLocationID = @LabLocationID", _conn))
            {
                cmd.Parameters.AddWithValue("LabLocationID", labLocationId);
                cmd.Parameters.AddWithValue("LocationName", locationName);
                cmd.Parameters.AddWithValue("LabID", labId);
                cmd.ExecuteNonQuery();
            }
        }

        public void AddLabLocation(string locationName, int labId)
        {
            if (string.IsNullOrEmpty(locationName))
            {
                _statusCode = 400;
                throw new Exception("Name must not be blank.");
            }

            int count;

            using (var cmd = new SqlCommand("SELECT COUNT(*) FROM sselScheduler.dbo.LabLocation WHERE LocationName = @LocationName", _conn))
            {
                cmd.Parameters.AddWithValue("LocationName", locationName);
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }

            if (count > 0)
            {
                _statusCode = 400;
                throw new Exception("This name is already in use.");
            }

            using (var cmd = new SqlCommand("INSERT sselScheduler.dbo.LabLocation (LabID, LocationName) VALUES (@LabID, @LocationName)", _conn))
            {
                cmd.Parameters.AddWithValue("LabID", labId);
                cmd.Parameters.AddWithValue("LocationName", locationName);
                cmd.ExecuteNonQuery();
            }
        }

        public int GetClientID()
        {
            using (var cmd = new SqlCommand("SELECT ClientID FROM sselData.dbo.Client WHERE UserName = @UserName", _conn))
            {
                cmd.Parameters.AddWithValue("UserName", _context.User.Identity.Name);
                var result = Convert.ToInt32(cmd.ExecuteScalar());
                return result;
            }
        }

        public int GetLabID(int labLocationId)
        {
            using (var cmd = new SqlCommand("SELECT LabID FROM sselScheduler.dbo.LabLocation WHERE LabLocationID = @LabLocationID", _conn))
            {
                cmd.Parameters.AddWithValue("LabLocationID", labLocationId);
                var result = Convert.ToInt32(cmd.ExecuteScalar());
                return result;
            }
        }
    }

    public class LabLocationModel
    {
        public int LabLocationID { get; set; }
        public int LabID { get; set; }
        public string LabDisplayName { get; set; }
        public string LocationName { get; set; }
    }

    public class ResourceLabLocationModel
    {
        public int ResourceLabLocationID { get; set; }
        public int LabLocationID { get; set; }
        public int ResourceID { get; set; }
        public string ResourceName { get; set; }
    }

    public class AvailableResourceModel
    {
        public int ResourceID { get; set; }
        public string ResourceName { get; set; }
    }
}
