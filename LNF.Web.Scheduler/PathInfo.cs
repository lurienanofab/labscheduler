using LNF.Impl.Repository.Scheduler;
using LNF.Scheduler;
using System;
using System.Configuration;
using System.Linq;
using System.Web;

namespace LNF.Web.Scheduler
{
    public struct LocationPathInfo
    {
        public int LabID { get; private set; }
        public int LabLocationID { get; private set; }

        public static LocationPathInfo Empty => new LocationPathInfo();

        public static LocationPathInfo Parse(string value)
        {
            LocationPathInfo result = new LocationPathInfo();

            if (string.IsNullOrEmpty(value))
                return result;

            string[] splitter = value.Split(PathInfo.AllowedDelimiters);

            if (splitter.Length == 0)
                return result;

            if (splitter.Length > 1)
            {
                result.LabID = int.Parse(splitter[0]);
                result.LabLocationID = int.Parse(splitter[1]);
            }
            else
            {
                result.LabID = int.Parse(splitter[0]);
            }

            return result;
        }

        public static LocationPathInfo Create(int labId, int labLocationId)
        {
            return new LocationPathInfo()
            {
                LabID = labId,
                LabLocationID = labLocationId
            };
        }

        public static LocationPathInfo Create(ILab lab)
        {
            LocationPathInfo result = new LocationPathInfo();

            if (lab != null)
            {
                result.LabID = lab.LabID;
            }

            return result;
        }

        public static LocationPathInfo Create(ILabLocation loc)
        {
            LocationPathInfo result = new LocationPathInfo();

            if (loc != null)
            {
                result.LabID = loc.LabID;
                result.LabLocationID = loc.LabLocationID;
            }

            return result;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(ToString());
        }

        public override string ToString()
        {
            string pathDelimiter = PathInfo.PathDelimiter;
            string result = string.Empty;

            if (LabID == 0)
                return result;
            else
                result += LabID.ToString();

            if (LabLocationID == 0)
                return result;
            else
                result += pathDelimiter + LabLocationID.ToString();

            return result;
        }

        public string UrlEncode()
        {
            return HttpUtility.UrlEncode(ToString());
        }
    }

    public struct PathInfo
    {
        // only allow the following delimiters
        public readonly static char[] AllowedDelimiters = { ':', ',', '-', '|', '$' };

        public int BuildingID { get; private set; }
        public int LabID { get; private set; }
        public int ProcessTechID { get; private set; }
        public int ResourceID { get; private set; }

        public static string PathDelimiter
        {
            get
            {
                string result = ConfigurationManager.AppSettings["TreeView.PathDelimiter"];

                if (!AllowedDelimiters.Contains(char.Parse(result)))
                    throw new InvalidOperationException(string.Format("Invalid delimiter value. Use one of the following: {0}", string.Join(", ", AllowedDelimiters)));

                return result;
            }
        }

        public static PathInfo Empty => new PathInfo();

        public static PathInfo Parse(string value)
        {
            PathInfo result = new PathInfo();

            if (string.IsNullOrEmpty(value))
                return result;

            string[] splitter = value.Split(AllowedDelimiters);

            if (splitter.Length == 0)
                return result;

            if (splitter.Length > 3)
            {
                result.BuildingID = int.Parse(splitter[0]);
                result.LabID = int.Parse(splitter[1]);
                result.ProcessTechID = int.Parse(splitter[2]);
                result.ResourceID = int.Parse(splitter[3]);
            }
            else if (splitter.Length > 2)
            {
                result.BuildingID = int.Parse(splitter[0]);
                result.LabID = int.Parse(splitter[1]);
                result.ProcessTechID = int.Parse(splitter[2]);
            }
            else if (splitter.Length > 1)
            {
                result.BuildingID = int.Parse(splitter[0]);
                result.LabID = int.Parse(splitter[1]);
            }
            else if (splitter.Length > 0)
                result.BuildingID = int.Parse(splitter[0]);

            return result;
        }

        public static PathInfo Create(IProvider provider, int resourceId)
        {
            var res = provider.Scheduler.Resource.GetResource(resourceId);
            return Create(res);
        }

        public static PathInfo Create(int buildingId, int labId, int procTechId, int resourceId)
        {
            return new PathInfo()
            {
                BuildingID = buildingId,
                LabID = labId,
                ProcessTechID = procTechId,
                ResourceID = resourceId
            };
        }

        public static PathInfo Create(IBuilding bldg)
        {
            PathInfo result = new PathInfo();

            if (bldg != null)
                result.BuildingID = bldg.BuildingID;

            return result;
        }

        public static PathInfo Create(ILab lab)
        {
            PathInfo result = new PathInfo();

            if (lab != null)
            {
                result.BuildingID = lab.BuildingID;
                result.LabID = lab.LabID;
            }

            return result;
        }

        public static PathInfo Create(IProcessTech pt)
        {
            PathInfo result = new PathInfo();

            if (pt != null)
            {
                result.BuildingID = pt.BuildingID;
                result.LabID = pt.LabID;
                result.ProcessTechID = pt.ProcessTechID;
            }

            return result;
        }

        public static PathInfo Create(IResource res)
        {
            PathInfo result = new PathInfo();

            if (res != null)
            {
                result.BuildingID = res.BuildingID;
                result.LabID = res.LabID;
                result.ProcessTechID = res.ProcessTechID;
                result.ResourceID = res.ResourceID;
            }

            return result;
        }

        public static PathInfo Create(Resource res)
        {
            PathInfo result = new PathInfo();

            if (res != null)
            {
                result.ResourceID = res.ResourceID;

                if (res.ProcessTech != null)
                {
                    result.ProcessTechID = res.ProcessTech.ProcessTechID;

                    if (res.ProcessTech.Lab != null)
                    {
                        result.LabID = res.ProcessTech.Lab.LabID;

                        if (res.ProcessTech.Lab.Building != null)
                        {
                            result.BuildingID = res.ProcessTech.Lab.Building.BuildingID;
                        }
                    }
                }
            }

            return result;
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(ToString());
        }

        public override string ToString()
        {
            string pathDelimiter = PathDelimiter;
            string result = string.Empty;

            if (BuildingID == 0 && ResourceID == 0)
                return result;
            else
                result += BuildingID.ToString();

            if (LabID == 0 && ResourceID == 0)
                return result;
            else
                result += pathDelimiter + LabID.ToString();

            if (ProcessTechID == 0 && ResourceID == 0)
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
    }
}
