using System.Data;

namespace LNF.Web.Scheduler
{
    public class DateTimeUtility
    {
        public static DataTable GetAllHours()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("HourValue", typeof(int));
            dt.Columns.Add("HourText", typeof(string));

            for (int i = 1; i <= 12; i++)
                dt.Rows.Add(i, i.ToString());

            return dt;
        }

        public static DataTable GetAllMinutes()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("MinValue", typeof(int));
            dt.Columns.Add("MinText", typeof(string));

            for (int i = 0; i <= 59; i++)
                dt.Rows.Add(i, i.ToString("00"));

            return dt;
        }

        public static DataTable GetAllAMPM()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Name", typeof(string));

            dt.Rows.Add("AM");
            dt.Rows.Add("PM");

            return dt;
        }
    }
}