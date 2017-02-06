using System.Web.UI.WebControls;

namespace LNF.Web.Scheduler.Content
{
    public class SortablePage : SchedulerPage
    {
        public string SortExpression
        {
            get
            {
                string result = ViewState["SortExpression"] == null ? GetDefaultSortExpression() : ViewState["SortExpression"].ToString();
                return result;
            }
            set
            {
                ViewState["SortExpression"] = value;
            }
        }

        public string SortOrder
        {
            get
            {
                string result = ViewState["SortOrder"] == null ? GetDefaultSortOrder() : ViewState["SortOrder"].ToString();
                return result;
            }
            set
            {
                ViewState["SortOrder"] = value;
            }
        }

        public string GetSort()
        {
            if (string.IsNullOrEmpty(SortExpression)) return string.Empty;
            return (SortExpression + " " + SortOrder).Trim();
        }

        public void HandleSortCommand(DataGridSortCommandEventArgs e)
        {
            if (SortExpression != e.SortExpression)
            {
                SortExpression = e.SortExpression;
                SortOrder = "ASC";
            }
            else
            {
                SortOrder = SortOrder == "ASC" ? "DESC" : "ASC";
            }
        }

        protected virtual string GetDefaultSortExpression()
        {
            return string.Empty;
        }

        protected virtual string GetDefaultSortOrder()
        {
            return string.Empty;
        }
    }
}