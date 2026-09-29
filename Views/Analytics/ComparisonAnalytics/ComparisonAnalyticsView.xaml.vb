Imports SmartPrepModern.APISync.Models
Imports SmartPrepModern.APISync.Repositories
Imports SmartPrepModern.GlobalContext
Imports SmartPrepModern.Components.Models
Imports System.Windows.Threading

Namespace Views.Analytics
    Public Class ComparisonAnalyticsView
        Inherits UserControl

        Private _lastClickedUserId As Integer = -1
        Private _lastClickedAttemptIndex As Integer = -1
        Private _lastClickedIsoDate As String = String.Empty
        Private _lastClickedReviewees As List(Of RevieweeStatusOut) = New List(Of RevieweeStatusOut)()
        Private _lastClickedAttemptMap As Dictionary(Of Integer, Integer) = New Dictionary(Of Integer, Integer)()

        Private _currentExamId As Integer = 0
        Private _currentUserId As Integer = -1
        Private _analysisExpanded As Boolean = False
        Private _examListVisible As Boolean = True   ' tracks whether the left panel is shown

        ' Row heights
        Private ReadOnly _rowChartFull     As New GridLength(1, GridUnitType.Star)   ' analysis hidden
        Private ReadOnly _rowAnalysisFull  As New GridLength(0, GridUnitType.Star)

        Private ReadOnly _rowChartSplit    As New GridLength(3, GridUnitType.Star)   ' 50/50 ish (3:2)
        Private ReadOnly _rowAnalysisSplit As New GridLength(2, GridUnitType.Star)

        Public Sub New()
            InitializeComponent()

            AddHandler ctrlGrowthChart.QuestionForensicsRequested, AddressOf HandleChartPointClick
            AddHandler ctrlItemAnalysis.DeepAnalysisRequested, AddressOf HandleDeepAnalysis

            ctrlItemAnalysis.Visibility = Visibility.Collapsed
            
            rowAnalysis.Height = _rowAnalysisFull
            rowChart.Height = _rowChartFull

            AddHandler Me.Loaded, Async Sub()
                Await ctrlExams.RefreshList()
            End Sub
        End Sub

        Private Async Sub HandleExamSelection(sender As Object, exam As ExamListOut)
            If exam.id = _currentExamId Then Return
            _currentExamId = exam.id
            _currentUserId = -1
            _lastClickedAttemptIndex = -1
            _lastClickedUserId = -1
            _lastClickedReviewees = New List(Of RevieweeStatusOut)()

            If _currentExamId <= 0 Then
                pnlNoSelection.Visibility = Visibility.Visible
                pnlDashboard.Visibility = Visibility.Collapsed
                tbSelectedExamTitle.Visibility = Visibility.Collapsed
                tbSelectedExamTitle.Text = ""
                HideAnalysisPanel()
                RestoreExamListPanel()
                Return
            End If

            pnlNoSelection.Visibility = Visibility.Collapsed
            pnlDashboard.Visibility = Visibility.Visible

            tbSelectedExamTitle.Text = exam.exam_name
            tbSelectedExamTitle.Visibility = Visibility.Visible

            CollapseExamListPanel()
            HideAnalysisPanel()

            If UserSession.Role = "Reviewee" Then
                _currentUserId = UserSession.UserID
                _lastClickedUserId = UserSession.UserID
            End If

            ' Load reviewees alongside everything else so the chart has them for point clicks
            Dim revieweesTask = ctrlReviewees.LoadReviewees(_currentExamId)

            Await Task.WhenAll(
                revieweesTask,
                RefreshTrend(_currentUserId)
            )

            ' Pass loaded reviewees into the chart so chart point clicks can filter them
            Dim loadedReviewees = ctrlReviewees.GetLoadedReviewees()
            ctrlGrowthChart.SetReviewees(If(loadedReviewees, New List(Of RevieweeStatusOut)()))
        End Sub

        Private Async Function RefreshTrend(userId As Integer?) As Task
            If _currentExamId = 0 Then Return

            pnlLoading.Visibility = Visibility.Visible
            Try
                Dim req As New StatsRequest With {
                    .examination_id = _currentExamId,
                    .user_id        = If(userId.HasValue, userId.Value, -1)
                }

                Dim resp = Await AnalyticsRepo.get_comparative_trendAsync(req)

                If resp?.Success AndAlso resp.Data IsNot Nothing Then
                    If resp.Data.history IsNot Nothing AndAlso resp.Data.history.Count > 0 Then
                        ctrlGrowthChart.SetContext(_currentExamId, req.user_id)
                        ctrlGrowthChart.RenderTrend(resp.Data)
                    Else
                        ctrlGrowthChart.ClearChart()
                        Dim role = UserSession.Role
                        Dim msg  = If(role = "Reviewee",
                            "You haven't taken this exam yet.",
                            "This student has no attempts recorded for this exam.")
                        MessageBox.Show(msg, "NO DATA FOUND", MessageBoxButton.OK, MessageBoxImage.Information)
                    End If
                End If
            Catch ex As Exception
                Debug.WriteLine($"[ComparisonAnalyticsView] RefreshTrend error: {ex.Message}")
            Finally
                pnlLoading.Visibility = Visibility.Collapsed
            End Try
        End Function

        Private Async Sub HandleChartPointClick(
            sender As Object,
            examId As Integer,
            userId As Integer,
            attemptIndex As Integer,
            reviewees As List(Of RevieweeStatusOut),
            attemptMap As Dictionary(Of Integer, Integer),
            isoDate As String)

            ' Cache the context so HandleDeepAnalysis can use it
            _lastClickedUserId = userId
            _lastClickedAttemptIndex = attemptIndex
            _lastClickedIsoDate = isoDate
            _lastClickedReviewees = If(reviewees?.Count > 0, reviewees, If(ctrlReviewees.GetLoadedReviewees(), New List(Of RevieweeStatusOut)()))
            _lastClickedAttemptMap = If(attemptMap, New Dictionary(Of Integer, Integer)())  

            ShowAnalysisPanel()
            pnlLoading.Visibility = Visibility.Visible
            Try
                Await Dispatcher.InvokeAsync(Sub() pnlLoading.UpdateLayout(), DispatcherPriority.Render)
                Await ctrlItemAnalysis.FetchAndCacheAnalysis(examId, forceRefresh:=True)
                If examId <> _currentExamId Then Return
                ctrlItemAnalysis.RenderDateAnalysis(isoDate)
                ctrlItemAnalysis.SetSelectedDate(isoDate)
            Finally
                pnlLoading.Visibility = Visibility.Collapsed
            End Try
        End Sub

        Private Async Sub HandleDeepAnalysis(sender As Object, examId As Integer, isoDate As String)
            Await OpenDeepAnalysis(examId, isoDate)
        End Sub

        Private Async Function OpenDeepAnalysis(examId As Integer, isoDate As String) As Task
            ctrlDeepAnalysis.Visibility = Visibility.Visible
            ctrlDeepAnalysis.SetReviewees(_lastClickedReviewees, examId, _lastClickedAttemptIndex, -1, _lastClickedAttemptMap)
            ' MessageBox.Show($"userId:{_lastClickedUserId} attempt:{_lastClickedAttemptIndex} date:{isoDate}")

            ' Show strip so admin can pick a reviewee; hide only for Reviewee role (single user)
            If UserSession.Role = "Reviewee" Then
                ctrlDeepAnalysis.HideRevieweeStrip()
            Else
                ctrlDeepAnalysis.ShowRevieweeStrip()
            End If

            Await ctrlDeepAnalysis.LoadContext(examId, _lastClickedUserId, _lastClickedAttemptIndex, -1, isoDate)
            pnlDeepAnalysisOverlay.Visibility = Visibility.Visible
        End Function

        Private Sub HandleForensicClose()
            pnlDeepAnalysisOverlay.Visibility = Visibility.Collapsed
            ctrlDeepAnalysis.Visibility = Visibility.Collapsed
        End Sub

        Private Sub ShowAnalysisPanel()
            _analysisExpanded = True
            pnlChartContainer.Visibility = Visibility.Collapsed
            pnlAnalysisHeader.Visibility = Visibility.Visible
            ctrlItemAnalysis.Visibility = Visibility.Visible
            rowChart.Height = New GridLength(0)
            rowAnalysis.Height = New GridLength(1, GridUnitType.Star)
        End Sub

        Private Sub ShowAnalysisPanelPartial()
            _analysisExpanded = True
            ctrlItemAnalysis.Visibility = Visibility.Visible
            AnimateGridRow(rowChart.Height, New GridLength(0.8, GridUnitType.Star),
                        rowAnalysis.Height, New GridLength(0.2, GridUnitType.Star))
        End Sub

        Private Sub CollapseAnalysisPanel()
            _analysisExpanded = False
            ctrlItemAnalysis.Visibility = Visibility.Collapsed
            pnlAnalysisHeader.Visibility = Visibility.Collapsed
            pnlChartContainer.Visibility = Visibility.Visible
            rowChart.Height = _rowChartFull
            rowAnalysis.Height = _rowAnalysisFull
        End Sub


        Private Sub HideAnalysisPanel()
            _analysisExpanded = False
            ctrlItemAnalysis.Visibility = Visibility.Collapsed
            ctrlItemAnalysis.ClearAndReset()
            pnlAnalysisHeader.Visibility = Visibility.Collapsed
            pnlChartContainer.Visibility = Visibility.Visible
            rowChart.Height = _rowChartFull
            rowAnalysis.Height = _rowAnalysisFull
        End Sub
                
        Public Sub ToggleAnalysisPanel()
            If _analysisExpanded Then
                CollapseAnalysisPanel()
            Else
                ShowAnalysisPanel()
            End If
        End Sub

        Private Sub BtnMinimizeAnalysis_Click(sender As Object, e As RoutedEventArgs)
            ToggleAnalysisPanel()
        End Sub

        Private Async Sub BtnBackToGlobal_Click(sender As Object, e As RoutedEventArgs)
            _currentExamId = 0
            _currentUserId = -1
            ctrlExams.ClearSelection()
            ctrlGrowthChart.ClearChart()
            HideAnalysisPanel()
            btnBackToGlobal.Visibility = Visibility.Collapsed

            ' Restore the exam list panel
            RestoreExamListPanel()

            Await RefreshTrend(-1)
        End Sub

        ' ── Exam list panel show / hide ──────────────────────────────────────────
        Private Sub CollapseExamListPanel()
            If Not _examListVisible Then Return
            _examListVisible = False
            btnBackToGlobal.Visibility = Visibility.Visible
            AnimateColumnWidth(colExamSelector, colExamSelector.ActualWidth, 0)
        End Sub

        Private Sub RestoreExamListPanel()
            If _examListVisible Then Return
            _examListVisible = True
            AnimateColumnWidth(colExamSelector, colExamSelector.ActualWidth, 350,
                               onComplete:=Sub()
                                               pnlNoSelection.Visibility = Visibility.Visible
                                               pnlDashboard.Visibility = Visibility.Collapsed
                                           End Sub)
        End Sub

        Private Sub AnimateColumnWidth(col As ColumnDefinition,
                                       fromWidth As Double, toWidth As Double,
                                       Optional onComplete As Action = Nothing)
            Dim startTime = DateTime.Now
            Const totalMs = 280.0
            Dim timer As New DispatcherTimer() With {.Interval = TimeSpan.FromMilliseconds(16)}
            AddHandler timer.Tick, Sub(s, e)
                Dim elapsed = (DateTime.Now - startTime).TotalMilliseconds
                Dim t = Math.Min(elapsed / totalMs, 1.0)
                Dim eased = 1 - (1 - t) * (1 - t)   ' quadratic ease-out
                col.Width = New GridLength(fromWidth + (toWidth - fromWidth) * eased)
                If t >= 1.0 Then
                    CType(s, DispatcherTimer).Stop()
                    col.Width = New GridLength(toWidth)
                    onComplete?.Invoke()
                End If
            End Sub
            timer.Start()
        End Sub

        ' ── Row animation ────────────────────────────────────────────────────────
        Private Sub AnimateGridRow(fromChart As GridLength, toChart As GridLength,
                                   fromAnalysis As GridLength, toAnalysis As GridLength,
                                   Optional onComplete As Action = Nothing)
            Dim startTime = DateTime.Now
            Dim fromChartVal = fromChart.Value
            Dim toChartVal = toChart.Value
            Dim fromAnalysisVal = fromAnalysis.Value
            Dim toAnalysisVal = toAnalysis.Value
            Const totalMs = 250.0

            Dim timer As New DispatcherTimer() With {.Interval = TimeSpan.FromMilliseconds(16)}
            AddHandler timer.Tick, Sub(s, e)
                Dim elapsed = (DateTime.Now - startTime).TotalMilliseconds
                Dim t = Math.Min(elapsed / totalMs, 1.0)
                Dim eased = 1 - (1 - t) * (1 - t) ' quadratic ease-out

                rowChart.Height = New GridLength(fromChartVal + (toChartVal - fromChartVal) * eased, GridUnitType.Star)
                rowAnalysis.Height = New GridLength(fromAnalysisVal + (toAnalysisVal - fromAnalysisVal) * eased, GridUnitType.Star)

                If t >= 1.0 Then
                    CType(s, DispatcherTimer).Stop()
                    rowChart.Height = toChart
                    rowAnalysis.Height = toAnalysis
                    onComplete?.Invoke()
                End If
            End Sub
            timer.Start()
        End Sub

    End Class
End Namespace
