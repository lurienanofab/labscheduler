Imports LNF.Cache
Imports LNF.Models.Data
Imports LNF.Models.Scheduler
Imports LNF.Repository
Imports LNF.Repository.Scheduler
Imports LNF.Scheduler
Imports LNF.Web.Scheduler
Imports LNF.Web.Scheduler.Content

Namespace Pages
    Public Class AdminResources
        Inherits SchedulerPage

        Public Overrides ReadOnly Property AuthTypes As ClientPrivilege
            Get
                Return PageSecurity.AdminAuthTypes
            End Get
        End Property

        Public Property Command As String
        Public Property ResourceID As Integer

        Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
            If Not IsPostBack Then
                PageInit()
                Select Case Command
                    Case "edit"
                        ShowEditForm()
                    Case "delete"
                        DeleteResource()
                        LoadResources()
                    Case Else
                        LoadResources()
                End Select
            End If
        End Sub

        Public Sub PageInit()
            hidBaseUrl.Value = VirtualPathUtility.ToAbsolute("~/AdminResources.aspx")
            hidAjaxUrl.Value = VirtualPathUtility.ToAbsolute("~/ajax/index.ashx")
            Command = Request.QueryString("Command")
            Dim id As Integer
            If Integer.TryParse(Request.QueryString("ResourceID"), id) Then
                ResourceID = id
            Else
                ResourceID = 0
            End If
        End Sub

        Public Sub DeleteResource()
            Dim res As Resource = DA.Scheduler.Resource.Single(ResourceID)
            res.IsActive = False
            res.IsSchedulable = False
        End Sub

        Public Sub ShowEditForm()
            If ResourceID = 0 Then
                LoadEditForm(Nothing)
            Else
                Dim res As ResourceModel = CacheManager.Current.Resources(Function(x) x.ResourceID = ResourceID).FirstOrDefault()
                If res IsNot Nothing Then
                    LoadEditForm(res)
                Else
                    lblErrMsg.Text = String.Format("No resource was found with ResourceID {0}", ResourceID)
                    LoadResources()
                End If
            End If
        End Sub

        Public Sub LoadEditForm(r As ResourceModel)
            If r IsNot Nothing Then
                hidResourceID.Value = r.ResourceID.ToString()
                hidProcTechID.Value = r.ProcessTechID.ToString()
                hidLabID.Value = r.LabID.ToString()
                hidBuildingID.Value = r.BuildingID.ToString()
                txtResourceID.Text = r.ResourceID.ToString()
                txtResourceName.Text = r.ResourceName
                chkSchedulable.Checked = r.IsSchedulable
                chkActive.Checked = r.ResourceIsActive
                imgPic.ImageUrl = String.Format("{0}/images/Resource/Resource{1}_icon.png", VirtualPathUtility.ToAbsolute("~"), ResourceID.ToString().PadLeft(6, Char.Parse("0")))
                imgPic.Visible = True
                txtDesc.Text = r.Description
                txtHelpdeskEmail.Text = r.HelpdeskEmail
                'Dim emailGroup As IEmailGroup = Providers.Email.GroupUtility.RetrieveGroup(r)
                'If emailGroup.Empty Then
                '   txtEmailGroup.Text = "[add group]"
                'Else
                '   txtEmailGroup.Text = emailGroup.GroupID
                'End If

                litToolEngineer.Text = "<table class=""tool-engineer-table""><tbody></tbody><tfoot><tr class=""footer""><td><select class=""tool-engineer-select""/></td><td class=""tool-engineer-action""><input type=""button"" value=""Add"" class=""add-tool-engineer""/></td></tr></tfoot></table>"
                btnAdd.Visible = False
                btnAddAnother.Visible = False
                btnUpdate.Visible = True
                lblAction.Text = "Edit"
            Else
                litToolEngineer.Text = "<span class=""nodata"">save before adding</span>"
                litResourceImage.Text = "<span class=""nodata"">save before adding</span>"
                panResourceImage.Visible = False
                btnAdd.Visible = True
                btnAddAnother.Visible = True
                btnUpdate.Visible = False
                lblAction.Text = "Add"
            End If

            pEditResource.Visible = True
            pListResource.Visible = False
        End Sub

        Private Sub LoadResources()
            Dim start As Date = Date.Now
            Dim query As IList(Of ResourceModel) = CacheManager.Current.Resources()
            Dim secondsTaken As Double = (Date.Now - start).TotalSeconds
            Dim items As IList(Of ResourceTableItem) = ResourceTableItem.CreateList(query)
            rptResources.DataSource = items
            rptResources.DataBind()
            pEditResource.Visible = False
            pListResource.Visible = True
        End Sub

        Private Class ResourceTableItem
            Private _Resource As ResourceModel
            Private _ActionLinks As String
            Private _BuildingName As String
            Private _LabName As String
            Private _ProcessTechName As String
            Private _ResourceID As String
            Private _ResourceName As String
            Private _ToolEngineer As String
            Private _Schedulable As Boolean
            Private _Picture As String

            Public ReadOnly Property Resource As ResourceModel
                Get
                    Return _Resource
                End Get
            End Property

            Public ReadOnly Property ActionLinks As String
                Get
                    Return _ActionLinks
                End Get
            End Property

            Public ReadOnly Property BuildingName As String
                Get
                    Return _BuildingName
                End Get
            End Property

            Public ReadOnly Property LabName As String
                Get
                    Return _LabName
                End Get
            End Property

            Public ReadOnly Property ProcessTechName As String
                Get
                    Return _ProcessTechName
                End Get
            End Property

            Public ReadOnly Property ResourceID As String
                Get
                    Return _ResourceID
                End Get
            End Property

            Public ReadOnly Property ResourceName As String
                Get
                    Return _ResourceName
                End Get
            End Property

            Public ReadOnly Property ToolEngineer As String
                Get
                    Return _ToolEngineer
                End Get
            End Property

            Public ReadOnly Property Schedulable As Boolean
                Get
                    Return _Schedulable
                End Get
            End Property

            Public ReadOnly Property Picture As String
                Get
                    Return _Picture
                End Get
            End Property

            Public Sub New(r As ResourceModel)
                _Resource = r
                _ActionLinks = GetActionLinks()
                _BuildingName = r.BuildingName
                _LabName = r.LabName
                _ProcessTechName = r.ProcessTechName
                _ResourceID = GetResourceID()
                _ResourceName = r.ResourceName
                _ToolEngineer = GetToolEngineer()
                _Schedulable = r.IsSchedulable
                _Picture = GetPicture()
            End Sub

            Private Function GetResourceID() As String
                Return (_Resource.ResourceID + 1000000).ToString().Substring(1)
            End Function

            Private Function GetActionLinks() As String
                Return String.Format("<a href=""{0}/AdminResources.aspx?Command=edit&ResourceID={1}""><img src=""{0}/images/edit.gif"" alt=""edit"" border=""0"" /></a>&nbsp;<a href=""{0}/AdminResources.aspx?Command=delete&ResourceID={1}"" class=""delete-resource-link""><img src=""{0}/images/delete.gif"" alt=""delete"" border=""0"" /></a>", VirtualPathUtility.ToAbsolute("~"), _Resource.ResourceID)
            End Function

            Private Function GetToolEngineer() As String
                Dim result As String = String.Empty
                Dim toolEngineers As IList(Of ResourceClientInfo) = DA.Current.Query(Of ResourceClientInfo)().Where(Function(x) x.ResourceID = _Resource.ResourceID).ToList().Where(Function(x) x.HasAuth(Convert.ToInt32(ClientAuthLevel.ToolEngineer))).ToList()
                For Each te As ResourceClientInfo In toolEngineers
                    result += String.Format("<div>{0}</div>", te.DisplayName)
                Next
                Return result
            End Function

            Private Function GetPicture() As String
                Return String.Format("<img src=""{0}/images/Resource/Resource{1}_icon.png"" alt="""" />", VirtualPathUtility.ToAbsolute("~"), GetResourceID())
            End Function

            Public Shared Function CreateList(source As IList(Of ResourceModel)) As IList(Of ResourceTableItem)
                Dim result As New List(Of ResourceTableItem)
                result = source.Select(Function(x) New ResourceTableItem(x)).ToList()
                Return result
            End Function
        End Class

    End Class
End Namespace