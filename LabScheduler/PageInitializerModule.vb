Imports Microsoft.Web.Infrastructure.DynamicModuleHelper

' for more info see:
' https://simpleinjector.readthedocs.io/en/latest/webformsintegration.html

<Assembly: PreApplicationStartMethod(GetType(PageInitializerModule), "Initialize")>

Public Class PageInitializerModule
    Implements IHttpModule

    Private app As HttpApplication

    Public Shared Sub Initialize()
        DynamicModuleUtility.RegisterModule(GetType(PageInitializerModule))
    End Sub

    Public Sub Init(context As HttpApplication) Implements IHttpModule.Init
        app = context
        AddHandler app.PreRequestHandlerExecute, AddressOf App_OnPreRequestHandlerExecute
    End Sub

    Private Sub App_OnPreRequestHandlerExecute(sender As Object, e As EventArgs)
        Dim handler = app.Context.CurrentHandler

        If handler IsNot Nothing Then
            Dim name As String = handler.GetType().Assembly.FullName
            If Not name.StartsWith("System.Web") AndAlso Not name.StartsWith("Microsoft") Then
                Global_asax.InitializeHandler(handler)
            End If
        End If
    End Sub

    Public Sub Dispose() Implements IHttpModule.Dispose
        ' nothing to do here
    End Sub
End Class