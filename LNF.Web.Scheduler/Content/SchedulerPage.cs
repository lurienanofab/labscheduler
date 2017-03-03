/*
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
*/

using LNF.Cache;
using LNF.Models.Data;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using LNF.Web.Content;

namespace LNF.Web.Scheduler.Content
{
    public abstract class SchedulerPage : LNFPage
    {
        public new SchedulerMasterPage Master
        {
            get { return (SchedulerMasterPage)Page.Master; }
        }

        public override ClientPrivilege AuthTypes
        {
            get { return PageSecurity.DefaultAuthTypes; }
        }

        protected virtual ResourceModel GetCurrentResource()
        {
            return PathInfo.Current.GetResource();
        }

        /// <summary>
        /// Gets the current ViewType from session
        /// </summary>
        public ViewType GetCurrentView()
        {
            return CacheManager.Current.CurrentUserState().View;
        }

        /// <summary>
        /// Sets the current ViewType session variable
        /// </summary>
        public void SetCurrentView(ViewType value)
        {
            var userState = CacheManager.Current.CurrentUserState();
            if (userState.View != value)
            {
                userState.SetView(value);
                userState.AddAction("Changed view to {0}", value);
            }
        }
    }
}