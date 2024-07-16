''' <summary>
''' ClassLibrary2 の BaseClass です。
''' </summary>
Public Class BaseClass
    Implements Interface1, Interface2, Interface3, Interface4

    ''' <summary>
    ''' ID です。
    ''' </summary>
    ''' <returns></returns>
    Public Property Id As Integer

    ''' <summary>
    ''' ClassLibrary2_Interface1_Name です。
    ''' </summary>
    ''' <returns></returns>
    Public Property ClassLibrary2_Interface1_Name As String Implements Interface1.ClassLibrary2_Interface1_Name
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    ''' <summary>
    ''' ClassLibrary2_Interface2_Name です。
    ''' </summary>
    ''' <returns></returns>
    Public Property ClassLibrary2_Interface2_Name As String Implements Interface2.ClassLibrary2_Interface2_Name
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    ''' <summary>
    ''' ClassLibrary2_Interface3_Name です。
    ''' </summary>
    ''' <returns></returns>
    Public Property ClassLibrary2_Interface3_Name As String Implements Interface3.ClassLibrary2_Interface3_Name
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    ''' <summary>
    ''' ClassLibrary2_Interface4_Name です。
    ''' </summary>
    ''' <returns></returns>
    Public Property ClassLibrary2_Interface4_Name As String Implements Interface4.ClassLibrary2_Interface4_Name
        Get
            Throw New NotImplementedException()
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property
End Class
