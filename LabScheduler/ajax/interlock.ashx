<%@ WebHandler Language="C#" Class="LabScheduler.Api.Interlock" %>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.ServiceModel;
using LNF;
using LNF.Control;
using LNF.Repository;
using LNF.Repository.Control;

namespace LabScheduler.Api
{
    public class Interlock : IHttpHandler, IReadOnlySessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            string command = context.Request["command"];

            object result = null;

            switch (command)
            {
                case "get-state":
                    result = InterlockManager.GetState(int.Parse(context.Request["id"]));
                    break;
                case "set-state":
                    result = InterlockManager.SetState(int.Parse(context.Request["id"]), bool.Parse(context.Request["state"]));
                    break;
                case "":
                    throw new Exception("Missing parameter: command");
                default:
                    throw new Exception("Invalid command: " + command);
            }

            context.Response.Write(Providers.Serialization.Json.SerializeObject(result));
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }

    public static class InterlockManager
    {
        public static object SetState(int resourceId, bool state)
        {
            try
            {
                var inst = ActionInstanceUtility.Find(ActionType.Interlock, resourceId);
                var point = inst.GetPoint();
                var block = point.GetBlock();
                var pointResponse = Providers.Control.SetPointState(point, state, 0).EnsureSuccess();
                return new
                {
                    BlockID = block.BlockID,
                    BlockName = block.BlockName,
                    PointID = point.PointID,
                    InstanceName = inst.Name,
                    ActionID = inst.ActionID,
                    TimeTaken = (DateTime.Now - pointResponse.StartTime).TotalSeconds
                };
            }
            catch (EndpointNotFoundException)
            {
                return new { Error = true, Message = "The Wago Service is not running." };
            }
            catch (Exception ex)
            {
                return new { Error = true, Message = ex.Message };
            }
        }

        public static object GetState(int resourceId)
        {
            try
            {
                var inst = ActionInstanceUtility.Find(ActionType.Interlock, resourceId);
                var point = inst.GetPoint();
                var block = point.GetBlock();
                var blockResponse = Providers.Control.GetBlockState(block).EnsureSuccess();
                return new
                {
                    State = blockResponse.BlockState.GetPointState(point.PointID),
                    BlockID = block.BlockID,
                    BlockName = block.BlockName,
                    PointID = point.PointID,
                    InstanceName = inst.Name,
                    ActionID = inst.ActionID,
                    TimeTaken = (DateTime.Now - blockResponse.StartTime).TotalSeconds
                };
            }
            catch (EndpointNotFoundException)
            {
                return new { Error = true, Message = "The Wago Service is not running." };
            }
            catch (Exception ex)
            {
                return new { Error = true, Message = ex.Message };
            }
        }
    }
}