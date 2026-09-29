' vb
Imports SmartPrepModern.APISync.Models
Imports SmartPrepModern.APISync.Repositories
Imports SmartPrepModern.Components.Models

Namespace Components
    Public Class UniversalStatsView
        Inherits UserControl

        Public Event DeepForensicsRequested(sender As Object, categoryId As Integer)

        Private _examIntel As ExamAnalyticsResponse 
        Private _currentExamId As Integer
        Private _currentUserId As Integer
        Private _lastTopicId As Integer 

        Public ReadOnly Property LastClickedCategoryId As Integer
            Get
                Return _lastClickedCategoryId
            End Get
        End Property
        Private _lastClickedCategoryId As Integer = -1

        Public Sub New()
            InitializeComponent()
        End Sub

        Public Async Function FetchExamIntel(examId As Integer, Optional userId As Integer? = Nothing) As Task
            _currentExamId = examId
            _currentUserId = If(userId.HasValue, userId.Value, -1)

            Me.Dispatcher.Invoke(Sub()
                pnlLoading.Visibility = Visibility.Visible
                Me.Tag = If(userId.HasValue, "Individual", "Global")
            End Sub)
            
            Try
                Dim req As New StatsRequest With {.examination_id = examId}
                If userId.HasValue Then req.user_id = userId.Value

                Dim resp = Await AnalyticsRepo.get_exam_analyticsAsync(req)

                Me.Dispatcher.Invoke(Sub()
                    If resp?.Success AndAlso resp.Data IsNot Nothing AndAlso resp.Data.topic_breakdown IsNot Nothing AndAlso 
                        resp.Data.topic_breakdown.Any() Then
                        _examIntel = resp.Data
                        pnlEmptyState.Visibility = Visibility.Collapsed
                        
                        txtOverallComp.Text = $"{_examIntel.overall_competency}%"
                        txtAccuracyLabel.Text = If(_currentUserId > 0, "INDIVIDUAL ACCURACY", "OVERALL ACCURACY")
                        icTopicBreakdown.ItemsSource = _examIntel.topic_breakdown

                        If _examIntel.ai_analysis IsNot Nothing Then
                            txtAiSummary.Text = _examIntel.ai_analysis.summary
                            lstAiRecommendations.ItemsSource = _examIntel.ai_analysis.recommendations
                            txtAnalysisWarning.Visibility = Visibility.Collapsed
                        Else
                            txtAiSummary.Text = "AI analysis is pending or not yet generated for this dataset."
                            lstAiRecommendations.ItemsSource = Nothing
                            txtAnalysisWarning.Visibility = Visibility.Visible
                        End If

                        If _currentUserId <= 0 Then
                            btnRefreshAI.Visibility = Visibility.Visible
                        Else
                            btnRefreshAI.Visibility = Visibility.Collapsed
                        End If
                    Else
                        pnlEmptyState.Visibility = Visibility.Visible
                        icTopicBreakdown.ItemsSource = Nothing
                        txtOverallComp.Text = "0%"
                    End If
                End Sub)

            Catch ex As Exception
                Me.Dispatcher.Invoke(Sub() pnlEmptyState.Visibility = Visibility.Visible)
            Finally
                ' 2. ALWAYS HIDE LOADING AT THE END
                Me.Dispatcher.Invoke(Sub() pnlLoading.Visibility = Visibility.Collapsed)
            End Try
        End Function

        Private Sub btnDeepAnalysis_Click(sender As Object, e As RoutedEventArgs)
            e.Handled = True
            If _currentUserId <= 0 Then
                MessageBox.Show("Please select a learner to view the detailed response analysis.", "Select learner", MessageBoxButton.OK, MessageBoxImage.Information)
                Return
            End If

            Dim metric = TryCast(DirectCast(sender, Button).DataContext, PerformanceMetric)
            If metric Is Nothing Then Return

            RaiseEvent DeepForensicsRequested(Me, metric.id)
        End Sub

        Private Async Sub btnRefreshAI_Click(sender As Object, e As RoutedEventArgs)
            If _currentExamId <= 0 Then Return
            
            ' Lock UI
            btnRefreshAI.IsEnabled = False
            btnRefreshAI.Content = "GENERATING ANALYSIS... PLEASE WAIT"
            pnlLoading.Visibility = Visibility.Visible
            
            Try
                Dim payload As New GenerateAnalysisRequest With { .examination_id = _currentExamId }
                Dim resp = Await AnalyticsRepo.generate_overall_analysisAsync(payload)
                
                If resp IsNot Nothing AndAlso resp.Success Then
                    Await FetchExamIntel(_currentExamId)
                Else
                    MessageBox.Show("Failed to generate AI analysis. Waiting for data.")
                End If
            Catch ex As Exception
                MessageBox.Show($"Error generating AI analysis: {ex.Message}")
            Finally
                ' Restore UI
                btnRefreshAI.IsEnabled = True
                btnRefreshAI.Content = "GENERATE / RE-RUN BATCH ANALYSIS"
                pnlLoading.Visibility = Visibility.Collapsed
            End Try
        End Sub
    End Class
End Namespace
