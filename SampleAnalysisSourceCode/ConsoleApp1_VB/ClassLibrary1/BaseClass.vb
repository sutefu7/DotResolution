''' <summary>
''' ClassLibrary1 の BaseClass です。
''' </summary>
Public Class BaseClass
    Implements Interface1, Interface2, Interface3, Interface4

    ''' <summary>
    ''' ID です。
    ''' </summary>
    ''' <returns></returns>
    Public Property Id As Integer

    ''' <summary>
    ''' ClassLibrary1_Interface1_Name です。
    ''' </summary>
    ''' <returns></returns>
    Public Overridable Property ClassLibrary1_Interface1_Name As String Implements Interface1.ClassLibrary1_Interface1_Name
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    ''' <summary>
    ''' ClassLibrary1_Interface2_Name です。
    ''' </summary>
    ''' <returns></returns>
    Public Property ClassLibrary1_Interface2_Name As String Implements Interface2.ClassLibrary1_Interface2_Name
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    ''' <summary>
    ''' ClassLibrary1_Interface3_Name です。
    ''' </summary>
    ''' <returns></returns>
    Public Property ClassLibrary1_Interface3_Name As String Implements Interface3.ClassLibrary1_Interface3_Name
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    ''' <summary>
    ''' ClassLibrary1_Interface4_Name です。
    ''' </summary>
    ''' <returns></returns>
    Public Property ClassLibrary1_Interface4_Name As String Implements Interface4.ClassLibrary1_Interface4_Name
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property
End Class
