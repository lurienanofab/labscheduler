using System;

namespace LNF.Web.Scheduler.TreeView
{
    public static class TreeViewUtility
    {
        public static string Coalesce(params string[] args)
        {
            foreach (string s in args)
            {
                if (!string.IsNullOrEmpty(s))
                    return s;
            }

            return string.Empty;
        }
    }
}
