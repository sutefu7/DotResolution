' 各メンバーチェック

Imports System.Runtime.InteropServices

''' <summary>
''' Class2 です。
''' </summary>
Public Class Class2

    ''' <summary>
    ''' Aaa です。
    ''' </summary>
    Public Enum Aaa
        B1
        B2
        B3
    End Enum

    ''' <summary>
    ''' AaaDelegate です。
    ''' </summary>
    ''' <param name="aa"></param>
    Public Delegate Sub AaaDelegate(aa As Aaa)

    ''' <summary>
    ''' Bbb です。
    ''' </summary>
    Public Event Bbb As EventHandler

    ''' <summary>
    ''' m_age です。
    ''' </summary>
    Private m_age As Integer = 0

    ''' <summary>
    ''' i1, i2, i3 です。
    ''' </summary>
    Private i1, i2, i3 As Integer

    ''' <summary>
    ''' インデクサーです。
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns></returns>
    Default Public Property this(index As Integer) As Integer
        Get
            Return 0
        End Get
        Set(value As Integer)

        End Set
    End Property

    ''' <summary>
    ''' Age です。
    ''' </summary>
    ''' <returns></returns>
    Public Property Age As Integer

    ''' <summary>
    ''' コンストラクタです。
    ''' </summary>
    Public Sub New()

    End Sub

    ''' <summary>
    ''' SetData() です。
    ''' </summary>
    Public Sub SetData()

    End Sub

    ''' <summary>
    ''' GetAge() です。
    ''' </summary>
    ''' <returns></returns>
    Public Function GetAge() As Integer
        Return 0
    End Function

    Public Class Class2A

        Public Property Age As Integer

        ''' <summary>
        ''' Class2A の + オペレーターです。
        ''' </summary>
        ''' <param name="self"></param>
        ''' <param name="other"></param>
        ''' <returns></returns>
        Public Shared Operator +(ByVal self As Class2A, ByVal other As Class2A) As Class2A
            Return New Class2A With {.Age = self.Age + other.Age}
        End Operator

    End Class

    Friend Class NativeMethods

        ''' <summary>
        ''' MessageBox です。
        ''' </summary>
        ''' <param name="hWnd"></param>
        ''' <param name="txt"></param>
        ''' <param name="caption"></param>
        ''' <param name="Typ"></param>
        ''' <returns></returns>
        Declare Auto Function MBox Lib "user32.dll" Alias "MessageBox" (
        ByVal hWnd As Integer,
        ByVal txt As String,
        ByVal caption As String,
        ByVal Typ As Integer) As Integer

        ''' <summary>
        ''' MoveFileW です。
        ''' </summary>
        ''' <param name="src"></param>
        ''' <param name="dst"></param>
        ''' <returns></returns>
        <DllImport("KERNEL32.DLL", EntryPoint:="MoveFileW", SetLastError:=True,
            CharSet:=CharSet.Unicode, ExactSpelling:=True,
            CallingConvention:=CallingConvention.StdCall)>
        Public Shared Function MoveFile(
        ByVal src As String,
        ByVal dst As String) As Boolean
            ' Leave the body of the function empty.
        End Function

    End Class

End Class
