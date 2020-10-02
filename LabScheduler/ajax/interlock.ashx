<%@ WebHandler Language="C#" Class="LabScheduler.Api.Interlock" %>

using LNF;
using LNF.Control;
using Newtonsoft.Json;
using System;
using System.ServiceModel;
using System.Web;
using System.Web.SessionState;

namespace LabScheduler.Api
{
    public class Interlock : IHttpHandler, IReadOnlySessionState
    {
        [Inject] public IProvider Provider { get; set; }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            string command = context.Request["command"];

            object result = null;

            var mgr = new InterlockManager(Provider);

            switch (command)
            {
                case "get-state":
                    result = mgr.GetState(int.Parse(context.Request["id"]));
                    break;
                case "set-state":
                    result = mgr.SetState(int.Parse(context.Request["id"]), bool.Parse(context.Request["state"]));
                    break;
                case "":
                    throw new Exception("Missing parameter: command");
                default:
                    throw new Exception("Invalid command: " + command);
            }

            context.Response.Write(JsonConvert.SerializeObject(result));
        }

        public bool IsReusable { get { return false; } }
    }

    public class InterlockManager
    {
        private readonly IProvider _provider;

        public IProvider Provider { get { return _provider; } }

        public InterlockManager(IProvider provider)
        {
            _provider = provider;
        }

        public object SetState(int resourceId, bool state)
        {
            try
            {
                var inst = ActionInstances.Find(ActionType.Interlock, resourceId);
                var point = inst.GetPoint();
                var pointResponse = Provider.Control.SetPointState(point.PointID, state, 0).EnsureSuccess();

                return new
                {
                    point.BlockID,
                    point.BlockName,
                    point.PointID,
                    InstanceName = inst.Name,
                    inst.ActionID,
                    TimeTaken = (DateTime.Now - pointResponse.StartTime).TotalSeconds
                };
            }
            catch (EndpointNotFoundException)
            {
                return new { Error = true, Message = "The Wago Service is not running." };
            }
            catch (Exception ex)
            {
                return new { Error = true, ex.Message };
            }
        }

        public object GetState(int resourceId)
        {
            try
            {
                var inst = ActionInstances.Find(ActionType.Interlock, resourceId);
                var point = inst.GetPoint();
                var blockResponse = Provider.Control.GetBlockState(point.BlockID).EnsureSuccess();

                return new
                {
                    State = blockResponse.BlockState.GetPointState(point.PointID),
                    point.BlockID,
                    point.BlockName,
                    point.PointID,
                    InstanceName = inst.Name,
                    inst.ActionID,
                    TimeTaken = (DateTime.Now - blockResponse.StartTime).TotalSeconds
                };
            }
            catch (EndpointNotFoundException)
            {
                return new { Error = true, Message = "The Wago Service is not running." };
            }
            catch (Exception ex)
            {
                return new { Error = true, ex.Message };
            }
        }
    }
}