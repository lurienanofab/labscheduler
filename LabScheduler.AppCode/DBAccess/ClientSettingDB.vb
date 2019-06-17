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
        Public Sub New(ByVal ClientID As Integer) ' non-standard name needed to avoid name clash
            Using reader As ExecuteReaderResult = DA.Command().Param(New With {ClientID}).ExecuteReader("sselScheduler.dbo.procClientSettingSelect")
                If reader.Read() Then
                    IsValid = True
                    Me.ClientID = Convert.ToInt32(reader("ClientID"))
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
        End Sub

        Public Sub Insert()
            DA.Command() _
                .Param("ClientID", ClientID) _
                .Param("BuildingID", BuildingID) _
                .Param("LabID", LabID) _
                .Param("DefaultView", DefaultView) _
                .Param("BeginHour", BeginHour) _
                .Param("EndHour", EndHour) _
                .Param("WorkDays", WorkDays) _
                .Param("EmailCreateReserv", EmailCreateReserv) _
                .Param("EmailModifyReserv", EmailModifyReserv) _
                .Param("EmailDeleteReserv", EmailDeleteReserv) _
                .Param("EmailInvited", EmailInvited) _
                .ExecuteNonQuery("sselScheduler.dbo.procClientSettingInsert")
        End Sub

        Public Sub Update()
            DA.Command() _
                .Param("ClientID", ClientID) _
                .Param("BuildingID", BuildingID) _
                .Param("LabID", LabID) _
                .Param("DefaultView", DefaultView) _
                .Param("BeginHour", BeginHour) _
                .Param("EndHour", EndHour) _
                .Param("WorkDays", WorkDays) _
                .Param("EmailCreateReserv", EmailCreateReserv) _
                .Param("EmailModifyReserv", EmailModifyReserv) _
                .Param("EmailDeleteReserv", EmailDeleteReserv) _
                .Param("EmailInvited", EmailInvited) _
                .ExecuteNonQuery("procClientSettingUpdate")
        End Sub
    End Class

End Namespace