<%--
  Copyright 2017 University of Michigan

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.    
--%>

<%@ WebHandler Language="C#" Class="LabScheduler.Api.Interlock" %>

using LNF;
using LNF.Control;
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

            context.Response.Write(Providers.Serialization.Json.SerializeObject(result));
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
                var pointResponse = (await Providers.Control.SetPointState(point, state, 0)).EnsureSuccess();

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

        public static async Task<object> GetState(int resourceId)
        {
            try
            {
                var inst = ActionInstanceUtility.Find(ActionType.Interlock, resourceId);
                var point = inst.GetPoint();
                var block = point.Block;
                var blockResponse = (await Providers.Control.GetBlockState(block)).EnsureSuccess();

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