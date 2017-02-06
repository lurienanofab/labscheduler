Imports LNF.CommonTools
Imports LNF.Models.Scheduler
Imports LNF.Repository

Namespace DBAccess

    <Serializable>
    <Obsolete("Use LNF.Repository.Scheduler.ClientSetting")>
    Public Class ClientSettingDB
        Public IsValid As Boolean = False
        Public ClientID As Integer = -1
        Public BuildingID As Integer = -1
        Public LabID As Integer = -1
        Public DefaultView As ViewType = ViewType.DayView
        Public BeginHour As Double
        Public EndHour As Double
        Public WorkDays As String = "0,0,0,0,0,0,0"
        Public EmailCreateReserv As Boolean
        Public EmailModifyReserv As Boolean
        Public EmailDeleteReserv As Boolean
        Public EmailInvited As Boolean

        ' Returns the preferences for the specified client
        Public Sub New(ByVal ClID As Integer) ' non-standard name needed to avoid name clash
            Using dba As New SQLDBAccess("cnSselScheduler")
                Using reader As IDataReader = dba.ApplyParameters(New With {.ClientID = ClID}).ExecuteReader("procClientSettingSelect")
                    If reader.Read() Then
                        IsValid = True
                        ClientID = Convert.ToInt32(reader("ClientID"))
                        BuildingID = Convert.ToInt32(reader("BuildingID"))
                        LabID = Convert.ToInt32(reader("LabID"))
                        DefaultView = CType(reader("DefaultView"), ViewType)
                        BeginHour = Convert.ToDouble(reader("BeginHour"))
                        EndHour = Convert.ToDouble(reader("EndHour"))
                        WorkDays = reader("WorkDays").ToString()
                        EmailCreateReserv = Convert.ToBoolean(reader("EmailCreateReserv"))
                        EmailModifyReserv = Convert.ToBoolean(reader("EmailModifyReserv"))
                        EmailDeleteReserv = Convert.ToBoolean(reader("EmailDeleteReserv"))
                        EmailInvited = Convert.ToBoolean(reader("EmailInvited"))
                    End If
                    reader.Close()
                End Using
            End Using
        End Sub

        Public Sub Insert()
            Using dba As New SQLDBAccess("cnSselScheduler")
                With dba.SelectCommand
                    .AddParameter("@ClientID", ClientID)
                    .AddParameter("@BuildingID", BuildingID)
                    .AddParameter("@LabID", LabID)
                    .AddParameter("@DefaultView", DefaultView)
                    .AddParameter("@BeginHour", BeginHour)
                    .AddParameter("@EndHour", EndHour)
                    .AddParameter("@WorkDays", WorkDays)
                    .AddParameter("@EmailCreateReserv", EmailCreateReserv)
                    .AddParameter("@EmailModifyReserv", EmailModifyReserv)
                    .AddParameter("@EmailDeleteReserv", EmailDeleteReserv)
                    .AddParameter("@EmailInvited", EmailInvited)
                End With
                dba.ExecuteNonQuery("procClientSettingInsert")
            End Using
        End Sub

        Public Sub Update()
            Using dba As New SQLDBAccess("cnSselScheduler")
                With dba.SelectCommand
                    .AddParameter("@ClientID", ClientID)
                    .AddParameter("@BuildingID", BuildingID)
                    .AddParameter("@LabID", LabID)
                    .AddParameter("@DefaultView", DefaultView)
                    .AddParameter("@BeginHour", BeginHour)
                    .AddParameter("@EndHour", EndHour)
                    .AddParameter("@WorkDays", WorkDays)
                    .AddParameter("@EmailCreateReserv", EmailCreateReserv)
                    .AddParameter("@EmailModifyReserv", EmailModifyReserv)
                    .AddParameter("@EmailDeleteReserv", EmailDeleteReserv)
                    .AddParameter("@EmailInvited", EmailInvited)
                End With
                dba.ExecuteNonQuery("procClientSettingUpdate")
            End Using
        End Sub
    End Class

End Namespace