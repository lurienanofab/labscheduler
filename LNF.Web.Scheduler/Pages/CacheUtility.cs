using LNF.Impl.Cache;
using LNF.Web.Content;
using LNF.Web.Scheduler.TreeView;
using System;

namespace LNF.Web.Scheduler.Pages
{
    public class CacheUtility : LNFPage
    {
        protected virtual void DeleteKey(string key)
        {
            throw new NotImplementedException();
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

        protected virtual TimeSpan Ping()
        {
            return TimeSpan.Zero;
            //return RedisCacheClient.Default.Ping();
        }
    }
}
