' vb
Imports System.Collections.ObjectModel
Imports Microsoft.Win32
Imports System.Windows.Threading
Imports SmartPrepModern.APISync.Models
Imports SmartPrepModern.APISync.Repositories

Namespace Components
    Public Class CategoryGroupView
        Inherits UserControl

        Public Event AddTopicRequested(sender As Object, categoryId As Integer)
        Public Event CategoryDeleted(sender As Object, categoryId As Integer)
        Public Event RequestLoading(isLoading As Boolean)
        Public Event RequestLoadingMessage(message As String)

        Public Property CategoryId As Integer
        Public Property CategoryName As String

        Private _slots As New ObservableCollection(Of SourceReferenceItem)()
        Private ReadOnly _statusTimer As New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(3)}
        Private _statusRefreshRunning As Boolean

        Public Sub New()
            InitializeComponent()
            icSlots.ItemsSource = _slots
            AddHandler _statusTimer.Tick, AddressOf StatusTimer_Tick
            AddHandler Me.Loaded, Sub() UpdateStatusTimer()
            AddHandler Me.Unloaded, Sub() _statusTimer.Stop()
        End Sub

        Public Sub SetCategory(id As Integer, name As String)
            Me.CategoryId = id
            Me.CategoryName = name
            txtCategoryName.Text = name.ToUpper()
        End Sub

        Public Sub LoadSlots(items As List(Of SourceReferenceItem))
            Me.Dispatcher.Invoke(Sub()
                _slots.Clear()
                For Each item In items : _slots.Add(item) : Next
                UpdateStatusTimer()
            End Sub)
        End Sub

        Private Sub UpdateStatusTimer()
            If Me.IsLoaded AndAlso _slots.Any(Function(slot) Not slot.can_modify) Then
                _statusTimer.Start()
            Else
                _statusTimer.Stop()
            End If
        End Sub

        Private Async Sub StatusTimer_Tick(sender As Object, e As EventArgs)
            If _statusRefreshRunning Then Return
            _statusRefreshRunning = True
            Try
                Await RefreshCategoryData()
            Finally
                _statusRefreshRunning = False
            End Try
        End Sub

        
        ' --- ACTIONS ---
        Private Sub AddTopic_Click(sender As Object, e As RoutedEventArgs)
            RaiseEvent AddTopicRequested(Me, Me.CategoryId)
        End Sub
        
        Private Async Sub UploadFile_Click(sender As Object, e As RoutedEventArgs)
            Dim btn = DirectCast(sender, Button)
            Dim item = DirectCast(btn.DataContext, SourceReferenceItem)
            Dim fileType = btn.Tag.ToString() 

            If fileType = "questionnaire" Then
                ' 1. Check for Active Exam Dependencies
                If item.active_exam_count > 0 Then
                    Dim blockMsg = $"ACTION BLOCKED: This topic is used in {item.active_exam_count} active examination(s).{vbCrLf}{vbCrLf}" &
                                "To update these questions, you must first delete the Examinations referencing " &
                                "this topic in the 'Manage Exams' section."
                    
                    MessageBox.Show(blockMsg, "Dependency Conflict", MessageBoxButton.OK, MessageBoxImage.Stop)
                    Return
                End If

                ' 2. Simple confirmation for unused topics
                If item.item_count > 0 Then
                    Dim confirm = MessageBox.Show($"Re-uploading will replace the existing {item.item_count} items. Proceed?", 
                                                "Confirm Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                    If confirm <> MessageBoxResult.Yes Then Return
                End If
            End If

            ' Trigger the standard file picker and upload logic
            Await InitiateFileUpload(btn, item, fileType)
        End Sub

        Private Async Function InitiateFileUpload(uploadButton As Button, item As SourceReferenceItem, fileType As String) As Task
            Dim ofd As New OpenFileDialog With {
                .Filter = "Documents (*.pdf;*.docx)|*.pdf;*.docx|PDF Files (*.pdf)|*.pdf|Word Documents (*.docx)|*.docx",
                .Title = $"Select {fileType.ToUpper()} for {item.slot_name}"
            }

            If ofd.ShowDialog() = True Then
                If fileType = "questionnaire" AndAlso String.Equals(System.IO.Path.GetExtension(ofd.FileName), ".docx", StringComparison.OrdinalIgnoreCase) Then
                    Dim proceed = MessageBox.Show(
                        "PDF is recommended for questionnaires. This app uses a dedicated PDF parsing route, while Word layout and automatic numbering may be interpreted differently. Your PDF copy may extract more consistently. If you continue with Word, compare the imported item count and preview against the original.",
                        "PDF recommended for questionnaires",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning)
                    If proceed <> MessageBoxResult.Yes Then Return
                End If
                uploadButton.IsEnabled = False
                uploadButton.Opacity = 0.45
                RaiseEvent RequestLoading(True)
                RaiseEvent RequestLoadingMessage(If(fileType = "questionnaire", "UPLOADING QUESTIONNAIRE...", "UPLOADING MATERIAL..."))
                Try
                    Dim fileBytes = Await Task.Run(Function() System.IO.File.ReadAllBytes(ofd.FileName))
                    Dim fileName = System.IO.Path.GetFileName(ofd.FileName)

                    Dim req As New UnifiedUploadRequest With {
                        .file = fileBytes,
                        .slot_id = item.id,
                        .file_name = fileName,
                        .file_type = fileType
                    }

                    Dim res = Await SlotsRepo.upload_source_fileAsync(req)
                    If res.Success AndAlso res.Data IsNot Nothing AndAlso (res.Data.status = "success" OrElse res.Data.status = "queued") Then
                        If fileType = "questionnaire" Then
                            item.questionnaire_status = "pending"
                        Else
                            item.is_material_uploaded = True
                        End If
                        Dim index = _slots.IndexOf(item)
                        If index >= 0 Then _slots(index) = item
                        UpdateStatusTimer()
                        Await RefreshCategoryData(True)
                    Else
                        Dim errorText = If(res.Data?.message, res.ErrorMessage)
                        MessageBox.Show(errorText, "Upload blocked", MessageBoxButton.OK, MessageBoxImage.Warning)
                    End If
                Catch ex As Exception
                    MessageBox.Show($"Upload Failed: {ex.Message}")
                Finally
                    uploadButton.IsEnabled = True
                    uploadButton.Opacity = 1
                    RaiseEvent RequestLoading(False)
                    RaiseEvent RequestLoadingMessage("PROCESSING REQUEST...")
                End Try
            End If
        End Function

        Private Async Function RefreshCategoryData(Optional force As Boolean = False) As Task
            Dim req As New GetByCategoryIdRequest With {.category_id = Me.CategoryId}
            Dim resp = Await SlotsRepo.get_slots_by_categoryAsync(req)
            If resp Is Nothing OrElse Not resp.Success OrElse resp.Data Is Nothing Then Return
            Dim changed = _slots.Count <> resp.Data.Count
            If Not changed Then
                For index = 0 To _slots.Count - 1
                    Dim current = _slots(index)
                    Dim updated = resp.Data(index)
                    If current.id <> updated.id OrElse current.slot_name <> updated.slot_name OrElse
                       current.item_count <> updated.item_count OrElse
                       current.questionnaire_status <> updated.questionnaire_status OrElse
                       current.questionnaire_error <> updated.questionnaire_error OrElse
                       current.questionnaire_discarded <> updated.questionnaire_discarded OrElse
                       current.is_material_uploaded <> updated.is_material_uploaded Then
                        changed = True
                        Exit For
                    End If
                Next
            End If
            If changed OrElse force Then LoadSlots(resp.Data)
        End Function

        Private Async Sub DeleteSlot_Click(sender As Object, e As RoutedEventArgs)
            Dim item = DirectCast(DirectCast(sender, Button).DataContext, SourceReferenceItem)
            
            If MessageBox.Show($"Are you sure you want to delete '{item.slot_name}'?{vbCrLf}This removes all materials and questions.", 
                               "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning) = MessageBoxResult.Yes Then
                
                RaiseEvent RequestLoading(True)
                Try
                    Dim req As New DeleteSlotRequest With {.slot_id = item.id}
                    Dim res = Await SlotsRepo.delete_slotAsync(req)
                    If res.Success Then Await RefreshCategoryData()
                Finally
                    RaiseEvent RequestLoading(False)
                End Try
            End If
        End Sub

        Private Async Sub RenameSlot_Click(sender As Object, e As RoutedEventArgs)
            Dim item = DirectCast(DirectCast(sender, Button).DataContext, SourceReferenceItem)
            Dim newName = InputBox($"Enter new name for '{item.slot_name}':", "Rename Topic", item.slot_name)
            
            If Not String.IsNullOrWhiteSpace(newName) AndAlso newName <> item.slot_name Then
                RaiseEvent RequestLoading(True)
                Try
                    Dim req As New SlotUpdateRequest With {
                        .slot_id = item.id,
                        .new_slot_name = newName
                    }
                    Dim res = Await SlotsRepo.update_slot_nameAsync(req)
                    If res.Success Then Await RefreshCategoryData()
                Finally
                    RaiseEvent RequestLoading(False)
                End Try
            End If
        End Sub

        Private Async Sub ViewQuestions_Click(sender As Object, e As RoutedEventArgs)
            Dim slot = DirectCast(DirectCast(sender, Button).DataContext, SourceReferenceItem)
            
            RaiseEvent RequestLoading(True)
            Try
                Dim req As New GetBySlotIdRequest With {.slot_id = slot.id}
                Dim res = Await SlotsRepo.get_items_by_slotAsync(req)
                RaiseEvent RequestLoading(False)

                If res IsNot Nothing AndAlso res.Success AndAlso res.Data IsNot Nothing AndAlso res.Data.Count > 0 Then
                    Dim dialog As New QuestionPreviewDialog()
                    dialog.LoadItems(res.Data)
                    Await MaterialDesignThemes.Wpf.DialogHost.Show(dialog, "QuestionPreviewDialog")
                ElseIf res IsNot Nothing AndAlso Not res.Success Then
                    MessageBox.Show(res.ErrorMessage, "Question preview unavailable", MessageBoxButton.OK, MessageBoxImage.Warning)
                Else
                    MessageBox.Show("No questions are available for this topic yet.", "Question preview", MessageBoxButton.OK, MessageBoxImage.Information)
                End If
            Catch ex As Exception
                MessageBox.Show($"Failed to load preview: {ex.Message}")
            Finally
                RaiseEvent RequestLoading(False)
            End Try
        End Sub

        Private Async Sub DeleteCategory_Click(sender As Object, e As RoutedEventArgs)
            If _slots.Count > 0 Then
                MessageBox.Show("This category cannot be deleted because it contains active topics. " & 
                                "Please delete all individual topics (slots) inside first.", 
                                "Action Blocked", MessageBoxButton.OK, MessageBoxImage.Stop)
                Return
            End If

            ' 2. Prompt for confirmation
            Dim confirm = MessageBox.Show($"Are you sure you want to delete the category '{Me.CategoryName}'?", 
                                        "Confirm Category Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning)

            If confirm = MessageBoxResult.Yes Then
                RaiseEvent RequestLoading(True)
                Try
                    ' Call the API
                    Dim req As New GetByCategoryIdRequest With {.category_id = Me.CategoryId}
                    Dim res = Await SlotsRepo.delete_categoryAsync(req) 

                    If res.Success Then
                        ' Notify the parent container (likely the main View) to remove this component from the UI
                        RaiseEvent CategoryDeleted(Me, Me.CategoryId)
                    Else
                        MessageBox.Show(res.ErrorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error)
                    End If
                Catch ex As Exception
                    MessageBox.Show($"Request failed: {ex.Message}")
                Finally
                    RaiseEvent RequestLoading(False)
                End Try
            End If
        End Sub
    End Class
End Namespace
