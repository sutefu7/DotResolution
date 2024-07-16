Imports ClassLibrary1
Imports Xxx = ClassLibrary1.BaseClass
Imports Yyy = ClassLibrary1

' コメント、ドキュメントコメント、継承のチェック

''' <summary>
''' ConsoleApp1 の Class1 です。
''' </summary>
Public Class Class1
    Inherits BaseClass
    Implements Interface1

    ''' <summary>
    ''' Name です。
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Property ClassLibrary1_Interface1_Name As String
        Get
            Return MyBase.ClassLibrary1_Interface1_Name
        End Get
        Set(value As String)
            MyBase.ClassLibrary1_Interface1_Name = value
        End Set
    End Property

End Class

' Class1A です。1
''' <summary>
''' Class1A です。2
''' </summary>
Public Class Class1A

End Class

''' <summary>
''' Class1B です。1
''' </summary>
Public Class Class1B
    Inherits Class1A

End Class

' Class1C です。
Public Class Class1C
    Inherits Xxx

End Class


Public Class Class1D
    Inherits Yyy.BaseClass

End Class

Public Class Class1E
    Inherits Global.ClassLibrary1.BaseClass

End Class