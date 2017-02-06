using System.Data;

namespace LNF.Web.Scheduler
{
    public class DateTimeUtility
    {
        public static DataTable GetAllHours()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("HID", typeof(int));

            for (int i = 1; i <= 12; i++)
                dt.Rows.Add(i);

            return dt;
        }

        public static DataTable GetAllMinutes()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("MinID", typeof(int));

            for (int i = 0; i <= 59; i++)
                dt.Rows.Add(i);

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