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

        public static string TreeItemTypeToString(TreeItemType type)
        {
            switch (type)
            {
                case TreeItemType.Building:
                    return "building";
                case TreeItemType.Lab:
                    return "lab";
                case TreeItemType.ProcessTech:
                    return "proctech";
                case TreeItemType.Resource:
                    return "resource";
                default:
                    return Enum.GetName(typeof(TreeItemType), type).ToLower();
            }
        }
    }
}
