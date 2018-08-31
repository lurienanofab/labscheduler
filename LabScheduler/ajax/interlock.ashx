<%@ WebHandler Language="C#" Class="LabScheduler.Api.Interlock" %>

using LNF;
using LNF.Control;
using Newtonsoft.Json;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace LabScheduler.Api
{
    public class Interlock : HttpTaskAsyncHandler, IReadOnlySessionState
    {
        public override async Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            string command = context.Request["command"];

            object result = null;

            switch (command)
            {
                case "get-state":
                    result = await InterlockManager.GetState(int.Parse(context.Request["id"]));
                    break;
                case "set-state":
                    result = await InterlockManager.SetState(int.Parse(context.Request["id"]), bool.Parse(context.Request["state"]));
                    break;
                case "":
                    throw new Exception("Missing parameter: command");
                default:
                    throw new Exception("Invalid command: " + command);
            }

            context.Response.Write(JsonConvert.SerializeObject(result));
        }
    }

    public static class InterlockManager
    {
        public static async Task<object> SetState(int resourceId, bool state)
        {
            try
            {
                var inst = ActionInstanceUtility.Find(ActionType.Interlock, resourceId);
                var point = inst.GetPoint();
                var block = point.Block;
                var pointResponse = (await ServiceProvider.Current.Control.SetPointState(point, state, 0)).EnsureSuccess();

                return new
                {
                    block.BlockID,
                    block.BlockName,
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

        public static async Task<object> GetState(int resourceId)
        {
            try
            {
                var inst = ActionInstanceUtility.Find(ActionType.Interlock, resourceId);
                var point = inst.GetPoint();
                var block = point.Block;
                var blockResponse = (await ServiceProvider.Current.Control.GetBlockState(block)).EnsureSuccess();

                return new
                {
                    State = blockResponse.BlockState.GetPointState(point.PointID),
                    block.BlockID,
                    block.BlockName,
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