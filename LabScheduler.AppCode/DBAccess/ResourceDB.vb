Imports LNF
Imports LNF.Models.Scheduler

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

            Dim labIdParam As Integer?

            If labId = -1 Then
                labIdParam = Nothing
            Else
                labIdParam = labId
            End If

            Dim result As IList(Of IResource) = ServiceProvider.Current.Scheduler.Resource.SelectByLab(labIdParam).ToList()

            Return result
        End Function

        Public Shared Function SelectResourceListItemsByLab(labId As Integer) As IList(Of ResourceListItem)
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
