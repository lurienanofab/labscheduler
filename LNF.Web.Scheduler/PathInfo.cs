using LNF.Cache;
using LNF.Models.Scheduler;
using LNF.Scheduler;
using System;
using System.Configuration;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler
{
    public struct PathInfo
    {
        private int _BuildingID;
        private int _LabID;
        private int _ProcessTechID;
        private int _ResourceID;

        // only allow the following delimiters
        private readonly static char[] _delimiters = { ':', ',', '-', '|', '$' };

        public int BuildingID { get { return _BuildingID; } }
        public int LabID { get { return _LabID; } }
        public int ProcessTechID { get { return _ProcessTechID; } }
        public int ResourceID { get { return _ResourceID; } }

        public static string PathDelimiter
        {
            get
            {
                string result = ConfigurationManager.AppSettings["TreeView.PathDelimiter"];

                if (!_delimiters.Contains(char.Parse(result)))
                    throw new InvalidOperationException(string.Format("Invalid delimiter value. Use one of the following: {0}", string.Join(", ", _delimiters)));

                return result;
            }
        }

        public static PathInfo Parse(string value)
        {
            PathInfo result = new PathInfo();

            if (string.IsNullOrEmpty(value))
                return result;

            string[] splitter = value.Split(_delimiters);

            if (splitter.Length == 0)
                return result;

            if (splitter.Length > 3)
            {
                result._BuildingID = int.Parse(splitter[0]);
                result._LabID = int.Parse(splitter[1]);
                result._ProcessTechID = int.Parse(splitter[2]);
                result._ResourceID = int.Parse(splitter[3]);
            }
            else if (splitter.Length > 2)
            {
                result._BuildingID = int.Parse(splitter[0]);
                result._LabID = int.Parse(splitter[1]);
                result._ProcessTechID = int.Parse(splitter[2]);
            }
            else if (splitter.Length > 1)
            {
                result._BuildingID = int.Parse(splitter[0]);
                result._LabID = int.Parse(splitter[1]);
            }
            else if (splitter.Length > 0)
                result._BuildingID = int.Parse(splitter[0]);

            return result;
        }

        public static PathInfo Create(BuildingModel bldg)
        {
            PathInfo result = new PathInfo();

            if (bldg != null)
                result._BuildingID = bldg.BuildingID;

            return result;
        }

        public static PathInfo Create(LabModel lab)
        {
            PathInfo result = new PathInfo();

            if (lab != null)
            {
                result._BuildingID = lab.BuildingID;
                result._LabID = lab.LabID;
            }

            return result;
        }

        public static PathInfo Create(ProcessTechModel pt)
        {
            PathInfo result = new PathInfo();

            if (pt != null)
            {
                result._BuildingID = pt.BuildingID;
                result._LabID = pt.LabID;
                result._ProcessTechID = pt.ProcessTechID;
            }

            return result;
        }

        public static PathInfo Create(ResourceModel res)
        {
            PathInfo result = new PathInfo();

            if (res != null)
            {
                result._BuildingID = res.BuildingID;
                result._LabID = res.LabID;
                result._ProcessTechID = res.ProcessTechID;
                result._ResourceID = res.ResourceID;
            }

            return result;
        }
        public BuildingModel GetBuilding()
        {
            if (BuildingID > 0)
                return CacheManager.Current.GetBuilding(BuildingID);
            else
                return null;
        }

        public LabModel GetLab()
        {
            if (LabID > 0)
                return CacheManager.Current.GetLab(LabID);
            else
                return null;
        }

        public ProcessTechModel GetProcessTech()
        {
            if (ProcessTechID > 0)
                return CacheManager.Current.GetProcessTech(ProcessTechID);
            else
                return null;
        }

        public ResourceModel GetResource()
        {
            if (ResourceID > 0)
                return CacheManager.Current.GetResource(ResourceID);
            else
                return null;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(ToString());
        }

        public override string ToString()
        {
            string pathDelimiter = PathDelimiter;
            string result = string.Empty;

            if (BuildingID == 0)
                return result;
            else
                result += BuildingID.ToString();

            if (LabID == 0)
                return result;
            else
                result += pathDelimiter + LabID.ToString();

            if (ProcessTechID == 0)
                return result;
            else
                result += pathDelimiter + ProcessTechID.ToString();

            if (ResourceID == 0)
                return result;
            else
                result += pathDelimiter + ResourceID.ToString();

            return result;
        }

        public string UrlEncode()
        {
            return HttpUtility.UrlEncode(ToString());
        }

        public static PathInfo Current
        {
            get
            {
                return Parse(GetCurrentPath());
            }
        }

        public static string GetCurrentPath()
        {
            if (string.IsNullOrEmpty(HttpContext.Current.Request.QueryString["Path"]))
                return string.Empty;
            else
                return HttpContext.Current.Request.QueryString["Path"];
        }
    }
}
