' vb
Imports System.Windows.Controls
Imports MaterialDesignThemes.Wpf
Imports SmartPrepModern.APISync.Models
Imports SmartPrepModern.APISync.Repositories
Imports SmartPrepModern.Components

Namespace Views.ReviewDirector
    Public Class ManageExamView
        Inherits UserControl

        Private _currentQuestions As New List(Of QuestionnaireItem)()
        Private _selectedExamId As Integer = -1

        Public Sub New()
            InitializeComponent()
            AddHandler examSelector.ExamSelected, AddressOf HandleExamSelected
            
            ' Fetch initial Global Rules on load
            AddHandler Me.Loaded, Async Sub()
                Await examSelector.RefreshList()
                Await LoadRulesContext(-1)
            End Sub
        End Sub

        ' ---------------------------------------------------------
        ' CONTEXT SWITCHING
        ' ---------------------------------------------------------

        Private Async Sub HandleExamSelected(sender As Object, exam As ExamListOut)
            _selectedExamId = exam.id
            _currentQuestions.Clear()
            btnPreview.IsEnabled = False
            btnPreview.Content = "LOADING QUESTIONS..."
            
            ' UI Visibility Swap
            pnlGlobalSettings.Visibility = Visibility.Collapsed
            pnlDetails.Visibility = Visibility.Visible
            
            ' Set Exam Basic Info
            txtExamTitle.Text = exam.exam_name
            txtStudentCount.Text = exam.metric_count.ToString()

            Try
                Dim req As New ExamGetRequest With {.exam_id = _selectedExamId}
                Dim resp = Await ExamRepo.get_examAsync(req)
                If exam.id <> _selectedExamId Then Return

                If resp IsNot Nothing AndAlso resp.Success AndAlso resp.Data IsNot Nothing AndAlso resp.Data.questions IsNot Nothing Then
                    txtItemCount.Text = resp.Data.total_items.ToString()
                    icTopicPills.ItemsSource = resp.Data.topics
                    
                    ' FIX: Manually map QuestionOut (flat options) to QuestionnaireItem (Dictionary choices)
                    _currentQuestions = resp.Data.questions.Select(Function(q)
                        Dim item As New QuestionnaireItem With {
                            .id = q.id,
                            .question_text = q.question_text,
                            .correct_answer = q.answer,
                            .choices = New Dictionary(Of String, String)()
                        }
                        item.choices = New Dictionary(Of String, String)(q.choices)
                        Return item
                    End Function).ToList()
                    btnPreview.IsEnabled = _currentQuestions.Count > 0
                    btnPreview.Content = If(btnPreview.IsEnabled, "PREVIEW QUESTIONS", "NO QUESTIONS AVAILABLE")
                Else
                    btnPreview.Content = "PREVIEW UNAVAILABLE"
                End If

                ' Load the specific Rules (Timers) for this Exam
                Await LoadRulesContext(_selectedExamId)
            Catch ex As Exception
                btnPreview.Content = "PREVIEW UNAVAILABLE"
                MessageBox.Show("Error loading exam details: " & ex.Message)
            End Try
        End Sub

        ' ---------------------------------------------------------
        ' RULES (TIMERS) LOGIC
        ' ---------------------------------------------------------

        Private Async Function LoadRulesContext(id As Integer) As Task
            Try
                Dim req As New ExamRuleRequest With {.examination_id = id}
                Dim resp = Await ExamRepo.get_exam_ruleAsync(req)

                ' Using FindControl to reach into the templates defined in XAML
                Dim txtPerQ = FindControl(Of TextBox)(If(id = -1, contentGlobal, contentExamOverride), "txtPerQuestion")
                Dim txtRev = FindControl(Of TextBox)(If(id = -1, contentGlobal, contentExamOverride), "txtReviewTimer")

                If resp.Success AndAlso resp.Data IsNot Nothing Then
                    txtPerQ.Text = resp.Data.rule.per_question_timer.ToString()
                    txtRev.Text = resp.Data.rule.review_timer.ToString()
                ElseIf id <> -1 Then
                    Dim globalReq As New ExamRuleRequest With {.examination_id = -1}
                    Dim globalResp = Await ExamRepo.get_exam_ruleAsync(globalReq)
                    If globalResp.Success Then
                        txtPerQ.Text = globalResp.Data.rule.per_question_timer.ToString()
                        txtRev.Text = globalResp.Data.rule.review_timer.ToString()
                    End If
                End If
            Catch ex As Exception
                Debug.WriteLine("Rule load failed: " & ex.Message)
            End Try
        End Function

        Private Async Sub SaveRules_Click(sender As Object, e As RoutedEventArgs)
            Try
                Dim activeContent = If(_selectedExamId = -1, contentGlobal, contentExamOverride)
                
                Dim perQ = FindControl(Of TextBox)(activeContent, "txtPerQuestion").Text
                Dim revT = FindControl(Of TextBox)(activeContent, "txtReviewTimer").Text

                Dim req As New ExamRuleRequest With {
                    .examination_id = _selectedExamId,
                    .per_question_timer = Integer.Parse(perQ),
                    .review_timer = Integer.Parse(revT),
                    .status = 1
                }

                Dim resp = Await ExamRepo.upsert_exam_ruleAsync(req)
                If resp.Success Then
                    MessageBox.Show("Rules saved successfully.")
                End If
            Catch ex As Exception
                MessageBox.Show("Validation Error: Please check timer values.")
            End Try
        End Sub

        Private Function FindControl(Of T As FrameworkElement)(container As ContentControl, name As String) As T
            Return DirectCast(container.Template.FindName(name, container), T)
        End Function

        ' ---------------------------------------------------------
        ' ORIGINAL ACTIONS (txtConfirmPurge restored)
        ' ---------------------------------------------------------

        Private Async Sub btnRename_Click(sender As Object, e As RoutedEventArgs)
            Dim newName = InputBox("Enter new examination name:", "Rename Exam", txtExamTitle.Text)
            If Not String.IsNullOrWhiteSpace(newName) AndAlso newName <> txtExamTitle.Text Then
                Try
                    Dim resp = Await ExamRepo.rename_examAsync(New ExamRenameRequest With {.exam_id = _selectedExamId, .new_name = newName})
                    If resp?.Success Then
                        txtExamTitle.Text = newName
                        Await examSelector.RefreshList()
                    End If
                Catch ex As Exception
                    MessageBox.Show("Rename failed: " & ex.Message)
                End Try
            End If
        End Sub

        Private Async Sub btnPreview_Click(sender As Object, e As RoutedEventArgs)
            If _currentQuestions.Count = 0 Then
                MessageBox.Show("No questions are available for this assessment.", "Question preview", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If

            Dim preview As New QuestionPreviewDialog()
            preview.LoadItems(_currentQuestions)

            Await DialogHost.Show(preview, "MainDialogHost")
        End Sub

        Private Sub btnDelete_Click(sender As Object, e As RoutedEventArgs)
            pnlNuclearDelete.DataContext = New With {.exam_name = txtExamTitle.Text}
            pnlNuclearDelete.Visibility = Visibility.Visible
            ' Restored reference to original control ID
            txtConfirmPurge.Clear()
        End Sub

        Private Async Sub ExecuteDelete_Click(sender As Object, e As RoutedEventArgs)
            ' Restored reference to original control ID
            If txtConfirmPurge.Text.Trim().ToUpper() = "PURGE" Then
                pnlNuclearDelete.Visibility = Visibility.Collapsed
                Try
                    Dim resp = Await ExamRepo.delete_examAsync(New ExamDeleteRequest With {.exam_id = _selectedExamId})
                    If resp?.Success Then
                        pnlDetails.Visibility = Visibility.Collapsed
                        pnlGlobalSettings.Visibility = Visibility.Visible
                        _selectedExamId = -1
                        Await examSelector.RefreshList()
                    End If
                Catch ex As Exception
                    MessageBox.Show($"Deletion failed: {ex.Message}")
                End Try
            Else
                MessageBox.Show("Safety Lock Active: You must type 'PURGE' exactly.", "Validation Error")
            End If
        End Sub

        Private Sub CancelDelete_Click(sender As Object, e As RoutedEventArgs)
            pnlNuclearDelete.Visibility = Visibility.Collapsed
        End Sub
    End Class
End Namespace
