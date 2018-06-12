Imports MahApps.Metro.Controls
Imports MahApps.Metro.Controls.Dialogs
Imports System.Windows.Threading
Imports System.ComponentModel
Imports System.IO
Imports System.IO.File
Imports System.Text.RegularExpressions
Imports Alphaleonis
Imports System.Text.Encoding
Imports System.Collections.ObjectModel


Public Class MainWindow
    Inherits MetroWindow

    Public userID As String
    Public userPassword As String
    Public _globaltotallinecount As Integer = 0
    Dim _globalError As Integer = 0
    Dim WithEvents BackgroundWorker1 As BackgroundWorker
    Dim _processerror As Integer = 0
    Dim OutFolder As String
    Public LoadFilesItemList As New List(Of LoadFileItem)
    Public Property FileItemsList As New ObservableCollection(Of FileItem)

    Public Sub New()
        InitializeComponent()
        Me.BackgroundWorker1 = New System.ComponentModel.BackgroundWorker()

    End Sub

    Private Async Sub BT_Go_Click(sender As Object, e As RoutedEventArgs) Handles BT_Go.Click
        _globalError = 0

        Dim outdir As String = Path.GetDirectoryName(OutFolder)
        Dim outputfile As String = OutFolder

        Using fout As FileStream = New FileStream(outdir + "\\lvdiprodata\EXPORTS01\PS100000\Long Paths\Destination\Error.log", FileMode.Create, FileAccess.Write)
            Using fstr_out As StreamWriter = New StreamWriter(fout, System.Text.Encoding.UTF8)


                Dim inputFileList As String
                inputFileList = TB_FileList.Text

                Dim fileList As List(Of String) = New List(Of String)
                Try
                    Using fl As FileStream = New FileStream(inputFileList, FileMode.Open, FileAccess.Read)
                        Using fl_in As StreamReader = New StreamReader(fl)
                            Dim isHeader As Boolean
                            fl_in.ReadLine()
                            While Not (fl_in.Peek() = -1)
                                _globaltotallinecount += 1
                                If Not CB_Header.IsChecked Then
                                    isHeader = False
                                End If
                                Dim line As String = fl_in.ReadLine()
                                isHeader = True
                                fileList.Add(line)
                            End While
                        End Using
                    End Using

                Catch ex As Exception
                    MsgBox(ex.ToString)
                End Try

                If fileList.Count > 0 Then
                    For Each entry In fileList
                        Dim fields() As String = entry.Split(New Char() {"|"c})

                        Try
                            Dim sourceFile As String = fields(0)

                            If sourceFile.Length < 0 Then
                                _globalError = 1
                                fstr_out.WriteLine("Missing Source File")
                            End If


                            Dim destFile As String = fields(1)
                            If destFile.Length < 0 Then
                                _globalError = 1
                                fstr_out.WriteLine("Missing Target File")
                            End If

                            sourceFile = RemoveIlegalChar(sourceFile)
                            destFile = RemoveIlegalChar(destFile)

                            If Not Alphaleonis.Win32.Filesystem.File.Exists(sourceFile) Then
                                _globalError = 1
                                fstr_out.WriteLine(sourceFile + "|" + destFile + "|" + "Source File does not exist")

                            ElseIf Alphaleonis.Win32.Filesystem.File.Exists(destFile) Then
                                _globalError = 1
                                fstr_out.WriteLine(sourceFile + "|" + destFile + "|" + "Duplicate target file")

                            Else
                                Dim destFolder As String = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(destFile)

                                If Not Alphaleonis.Win32.Filesystem.Directory.Exists(destFolder) Then
                                    Alphaleonis.Win32.Filesystem.Directory.CreateDirectory(destFolder)
                                End If


                                Alphaleonis.Win32.Filesystem.File.Copy(sourceFile, destFile)
                                windowgrip.IsEnabled = True
                                _globalError = 0
                                fstr_out.WriteLine(sourceFile + "|" + destFile)
                            End If
                        Catch ex As Exception
                            MsgBox(ex.ToString)
                        End Try
                    Next
                End If
            End Using
        End Using

        LoadFilesItemList.Clear()
        If TB_FileList.Text = "" Then
            Await ShowMessageAsync("", "Please select the an input file.")
            Exit Sub

        ElseIf Not Alphaleonis.Win32.Filesystem.File.Exists(TB_FileList.Text) Then
            Await ShowMessageAsync("", "Please ensure the input file exist")
            Exit Sub
        End If

        For Each item In FileItemsList
            Dim f As New LoadFileItem
            f.FileName = item.FileName.Text
            LoadFilesItemList.Add(f)
        Next

        OutFolder = TB_FileList.Text

        windowgrip.IsEnabled = False
        Me.BackgroundWorker1 = New BackgroundWorker
        BackgroundWorker1.WorkerReportsProgress = True
        'Start the work 
        BackgroundWorker1.RunWorkerAsync()
    End Sub

    Public Function RemoveIlegalChar(ByVal file As String) As String
        file = file.Replace("\\\", "\\")
        file = file.Replace(",", " |")
        Return file
    End Function

    Private Sub Window_Closed(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Application.Current.Shutdown()
    End Sub

    Private Sub Window_Loaded(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Loaded
        TB_FileList.Text = ""
        CB_Header.IsChecked = True
    End Sub

    Private Sub worker_ProgressChanged(ByVal sender As Object, ByVal e As System.ComponentModel.ProgressChangedEventArgs) Handles BackgroundWorker1.ProgressChanged
    End Sub

    Private Async Sub worker_RunWorkerCompleted(ByVal sender As System.Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BackgroundWorker1.RunWorkerCompleted
        ' Do some time-consuming work on this thread.
        If e.Error IsNot Nothing Then
            Await ShowMessageAsync("", "The application background thread encountered error during processing.")
            windowgrip.IsEnabled = True
        Else
            If e.Result <> "" Then
                windowgrip.IsEnabled = True

                Await ShowMessageAsync("", e.Result)
            Else
                If _globalError = 1 Then
                    Await ShowMessageAsync("", "Complete with errors. Please check error log. ")
                Else
                    Await ShowMessageAsync("", "Complete! ")
                End If
            End If
            windowgrip.IsEnabled = True
        End If
    End Sub

    Private Sub BT_Browse_Click(sender As Object, e As RoutedEventArgs) Handles BT_Browse.Click
        Dim dlg As New Microsoft.Win32.OpenFileDialog
        dlg.InitialDirectory = Path.GetDirectoryName(Directory.GetCurrentDirectory)
        dlg.FileName = ""
        dlg.DefaultExt = ".txt" ' Default file extension
        dlg.Filter = "Text File (.txt)|*.txt" ' Filter files by extension

        ' Show open file dialog box
        Dim result? As Boolean = dlg.ShowDialog()

        ' Process open file dialog box results
        If result = True Then
            ' Open document
            TB_FileList.Text = dlg.FileName
        End If
    End Sub

End Class

