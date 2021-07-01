Imports LNF
Imports LNF.DataAccess
Imports LNF.Web

Namespace DBAccess
    Public Class DB
        Private _provider As IProvider

        Public Sub New(provider As IProvider)
            _provider = provider
        End Sub

        Public ReadOnly Property Provider As IProvider
            Get
                Return _provider
            End Get
        End Property

        Public ReadOnly Property DataSession As ISession
            Get
                Return Provider.DataAccess.Session
            End Get
        End Property
    End Class
End Namespace

