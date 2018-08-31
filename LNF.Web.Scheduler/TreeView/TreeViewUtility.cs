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

        public static string TreeItemTypeToString(NodeType type)
        {
            switch (type)
            {
                case NodeType.Building:
                    return "building";
                case NodeType.Lab:
                    return "lab";
                case NodeType.ProcessTech:
                    return "proctech";
                case NodeType.Resource:
                    return "resource";
                default:
                    return Enum.GetName(typeof(NodeType), type).ToLower();
            }
        }
    }
}
