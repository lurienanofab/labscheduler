Imports LNF
Imports LNF.Repository
Imports LNF.Scheduler

Namespace DBAccess
    Public Class ResourceDB
        Public Shared Function SelectByLab(labId As Integer) As IList(Of IResource)

            ' This method is called from ReservationFacilityDownTime.aspx when the lab select changes.
            ' when labId is -1 use "default labs" (must convert to null)
            ' when labId is 0 use "all labs" (the default value)
            ' when labId > 0 select for single lab

            ' IResourceManager.SelectByLab expects null to mean "use default labs".
            ' However the select option for "Clean Room & Wet Chemistry" has a value of -1.
            ' So if -1 is passed in it should be treated as null

            Dim cmd As IDataCommand = DataCommand.Create(CommandType.Text)

            If labId = -1 Then
                cmd.Param("LabID", DBNull.Value)
            Else
                cmd.Param("LabID", labId)
            End If

            Dim dt As DataTable = cmd.FillDataTable("SELECT * FROM sselScheduler.dbo.v_ResourceInfo WHERE ResourceIsActive = 1 AND ISNULL(@LabID, LabID) = LabID")

            Dim result As New List(Of IResource)

            For Each dr As DataRow In dt.Rows
                Dim r As New Impl.Repository.Scheduler.ResourceInfo With {
                    .ResourceID = dr.Field(Of Integer)("ResourceID"),
                    .LabID = dr.Field(Of Integer)("LabID"),
                    .ProcessTechID = dr.Field(Of Integer)("ProcessTechID"),
                    .ResourceName = dr.Field(Of String)("ResourceName"),
                    .LabDisplayName = dr.Field(Of String)("LabDisplayName"),
                    .ProcessTechName = dr.Field(Of String)("ProcessTechName")
                }

                result.Add(r)
            Next

            Return result
        End Function

        Public Shared Function SelectResourceListItemsByLab(labId As Integer) As IList(Of ResourceListItem)
            ' Cannot pass IProvider as a parameter because this is used by an ObjectDataSource (odsTool) in ReservationFacilityDownTime.aspx

            Dim resources As IList(Of IResource) = SelectByLab(labId)

            Dim result As New List(Of ResourceListItem)()

            For Each res As IResource In resources
                result.Add(New ResourceListItem With {
                    .ResourceID = res.ResourceID,
                    .ResourceName = res.GetResourceName(ResourceNamePartial.LabName)
                })
            Next

            Return result
        End Function
    End Class

    Public Class ResourceListItem
        Public Property ResourceID As Integer
        Public Property ResourceName As String
    End Class
End Namespace
