Imports System.DirectoryServices
Imports MahApps.Metro.Controls.Dialogs
Public Class Window1


    Dim myScerectKey As String = "124m/gH+fsWF1hbxd6!~78435FblCVmE8Q=="


    Private Async Sub BT_Login_Click(sender As Object, e As RoutedEventArgs) Handles BT_Login.Click
        If (TB_Username.Text = "") Then

            Await ShowMessageAsync("", "Please enter a username.")
            TB_Username.Focus()
            Exit Sub

        End If

        If (TB_Password.Password = "") Then

            Await ShowMessageAsync("", "Please enter a password.")
            TB_Password.Focus()
            Exit Sub

        End If

        Dim userName As String = TB_Username.Text.ToLower
        Dim userPwd As String = DecodeData(myScerectKey, TB_Password.Password)

        If ((TB_Username.Text.ToLower = "adm_zlin") And (ValidateActiveDirectoryLogin(userName, userPwd))) Then

            Dim MainWindow1 As MainWindow = New MainWindow

            MainWindow1.userID = userName
            MainWindow1.userPassword = userPwd

            MainWindow1.Show()
            Me.Hide()
        Else
            If ((TB_Username.Text.ToLower <> "adm_zlin")) Then
                Await ShowMessageAsync("", "Login Failed. You are not authorized to use this application.")
            Else
                Await ShowMessageAsync("", "Login Failed.")
            End If

            Exit Sub
        End If
    End Sub

    Function DecodeData(ByVal secretkey As String, ByVal encryptedtext As String) As String
        Dim cipherText As String = encryptedtext

        Dim password As String = secretkey

        Dim plainText As String = ""

        Dim wrapper As New Simple3Des(password)

        ' DecryptData throws if the wrong password is used.
        Try
            plainText = wrapper.DecryptData(cipherText)

        Catch ex As Exception
            ' MsgBox("DecodeData error")
            Return plainText
        End Try

        Return plainText
    End Function

    Private Function ValidateActiveDirectoryLogin(ByVal Username As String, ByVal Password As String) As Boolean
        Dim Success As Boolean = False

        Try

            Dim Entry As New System.DirectoryServices.DirectoryEntry("LDAP://lw.com", Username, Password)
            Dim Searcher As New System.DirectoryServices.DirectorySearcher(Entry)

            Searcher.SearchScope = DirectoryServices.SearchScope.OneLevel


            Dim Results As System.DirectoryServices.SearchResult = Searcher.FindOne

            Success = Not (Results Is Nothing)

        Catch ex As Exception

            ' MsgBox("validate AD error")
            Success = False

        End Try

        Return Success

    End Function

    Private Sub LoginWindow_Closed(ByVal sender As System.Object, ByVal e As System.EventArgs)



        Application.Current.Shutdown()




    End Sub


    Private Sub LoginWindow_Loaded(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub
End Class
