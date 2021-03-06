﻿Public Class NumericBox
    Inherits System.Web.UI.UserControl

    Private _DecimalPlaces As Integer
    Private _PositiveNumber As Boolean
    Private _RealNumber As Boolean
    Private _CssClass As String

    Public Property CssClass As String
        Get
            Return _CssClass
        End Get
        Set(value As String)
            _CssClass = value
            txtNumericText.CssClass = GetCssClass()
        End Set
    End Property

    Public Property DecimalPlaces As Integer
        Get
            Return _DecimalPlaces
        End Get
        Set(value As Integer)
            _DecimalPlaces = value
            txtNumericText.Attributes("data-decimals") = _DecimalPlaces.ToString()
        End Set
    End Property

    Public Property PositiveNumber As Boolean
        Get
            Return _PositiveNumber
        End Get
        Set(value As Boolean)
            _PositiveNumber = value
            txtNumericText.Attributes("data-positive") = _PositiveNumber.ToString().ToLower()
        End Set
    End Property

    Public Property RealNumber As Boolean
        Get
            Return _RealNumber
        End Get
        Set(value As Boolean)
            _RealNumber = value
            txtNumericText.Attributes("data-real") = _RealNumber.ToString().ToLower()
        End Set
    End Property

    Public Property Text As String
        Get
            Return txtNumericText.Text
        End Get
        Set(value As String)
            txtNumericText.Text = value
        End Set
    End Property

    Public Property MaxLength As Integer
        Get
            Return txtNumericText.MaxLength
        End Get
        Set(value As Integer)
            txtNumericText.MaxLength = value
        End Set
    End Property

    Public Property Width As Unit
        Get
            Return txtNumericText.Width
        End Get
        Set(value As Unit)
            txtNumericText.Width = value
        End Set
    End Property

    Private Function GetCssClass() As String
        Return ("numeric-text " + _CssClass).Trim()
    End Function

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        txtNumericText.Attributes.Add("data-decimals", _DecimalPlaces.ToString())
        txtNumericText.Attributes.Add("data-positive", _PositiveNumber.ToString().ToLower())
        txtNumericText.CssClass = GetCssClass()
    End Sub

End Class