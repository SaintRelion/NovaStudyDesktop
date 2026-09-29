' vb
Imports LiveCharts
Imports LiveCharts.Wpf
Imports SmartPrepModern.APISync.Models
Imports SmartPrepModern.APISync.Repositories
Imports SmartPrepModern.Components.Models

Namespace Components
    Public Class GrowthTrendView
        Inherits UserControl

        Private _isMasteryMode As Boolean

        Private _currentExamId As Integer
        Private _currentUserId As Integer?

        Public Event QuestionForensicsRequested(sender As Object, examId As Integer, userId As Integer, attemptIndex As Integer, reviewees As List(Of RevieweeStatusOut), attemptMap As Dictionary(Of Integer, Integer), dateLabel As String)
        Public Event LoadingStateChanged(sender As Object, isLoading As Boolean)

        Public Sub New()
            InitializeComponent()
        End Sub

        Public Sub SetContext(examId As Integer, userId As Integer?)
            _currentExamId = examId
            _currentUserId = userId
        End Sub

        Private _loadedReviewees As List(Of RevieweeStatusOut)
        Private _isoDatePerPoint As New Dictionary(Of Integer, String)()
        Private _examineesPerPoint As New Dictionary(Of Integer, List(Of Integer))()
        Private _attemptIndexPerUser As New Dictionary(Of Integer, Dictionary(Of Integer, Integer))() ' pointIndex → (userId → attemptIndex)

        Public Sub SetReviewees(reviewees As List(Of RevieweeStatusOut))
            _loadedReviewees = If(reviewees, New List(Of RevieweeStatusOut)())
        End Sub

        ' vb
        Public Sub RenderMultiSlotTrend(resp As GrowthTrendResponse)
            _isMasteryMode = True

            If resp IsNot Nothing AndAlso resp.history IsNot Nothing Then
                Dim seriesData As New SeriesCollection()
                Dim historyList = resp.history.Cast(Of SlotHistoryPoint)()
                
                Dim accuracyFormatter As Func(Of ChartPoint, String) = Function(cp) $"({cp.Y:N2}%)"

                Dim dateLabels = historyList.GroupBy(Function(h) h.date_recorded).
                    Select(Function(g) 
                        Dim count = g.Max(Function(x) x.examinee_count)
                        Dim role = SmartPrepModern.GlobalContext.UserSession.Role
        
                        If role = "Reviewee" Then
                            Return $"{g.Key} (YOU)"
                        Else
                            Return If(count > 0, $"{g.Key} ({count} Reviewee/s)", g.Key)
                        End If
                    End Function).ToList()

                Dim rawDates = historyList.Select(Function(h) h.date_recorded).Distinct().ToList()
                
                For Each slotName In resp.unique_slots
                    Dim values As New ChartValues(Of Double)()
                    Dim currentSlot = slotName 
                    
                    For Each dt In rawDates
                        Dim currentDate = dt
                        Dim point = historyList.FirstOrDefault(Function(h) h.slot_name = currentSlot AndAlso h.date_recorded = currentDate)
                        values.Add(If(point IsNot Nothing, point.accuracy, 0))
                    Next

                    seriesData.Add(New LineSeries With {
                        .Title = slotName,
                        .Values = values,
                        .PointGeometrySize = 8,
                        .StrokeThickness = 3,
                        .Fill = Brushes.Transparent,
                        .LabelPoint = accuracyFormatter
                    })
                Next

                Me.Dispatcher.Invoke(Sub()
                    txtTrendLabel.Text = resp.trend_label.ToUpper()
                    chartGrowth.Series = seriesData
                    axisX.Labels = dateLabels
                    chartGrowth.LegendLocation = LegendLocation.Right
                    chartGrowth.Update(True, True)
                End Sub)
            End If
        End Sub

        Public Sub RenderTrend(data As ComparativeTrendResponse)    
            _isMasteryMode = False
            _examineesPerPoint.Clear()
            _attemptIndexPerUser.Clear()
            _isoDatePerPoint.Clear()

            Dim accuracyFormatter As Func(Of ChartPoint, String) = Function(cp) $"({cp.Y:N2}%)"

            If data.history IsNot Nothing AndAlso data.history.Count > 0 Then
                Dim values As New ChartValues(Of Double)()
                Dim labels As New List(Of String)()

                For Each row In data.history
                    values.Add(Math.Round(row.average_accuracy, 2))

                    Dim labelText = row.date_recorded
                    Dim role = SmartPrepModern.GlobalContext.UserSession.Role

                    If role = "Reviewee" Then
                        labelText &= " (YOU)"
                    ElseIf row.examinee_count > 0 Then
                        labelText &= $" ({row.examinee_count} Reviewee/s)"
                    End If
                    labels.Add(labelText)

                    Dim pointIdx = labels.Count - 1
                    _isoDatePerPoint(pointIdx) = row.date_recorded

                    _examineesPerPoint(pointIdx) = If(row.examinee_ids, New List(Of Integer)())

                    Dim userAttemptMap As New Dictionary(Of Integer, Integer)(If(row.attempt_map, New Dictionary(Of Integer, Integer)()))
                    Dim ids = If(row.examinee_ids, New List(Of Integer)())
                    Dim idxs = If(row.attempt_indices, New List(Of Integer)())
                    For i = 0 To Math.Min(ids.Count, idxs.Count) - 1
                        If Not userAttemptMap.ContainsKey(ids(i)) Then userAttemptMap(ids(i)) = idxs(i)
                    Next
                    _attemptIndexPerUser(pointIdx) = userAttemptMap
                Next

                Dim series As New LineSeries With {
                    .Title = "Accuracy",
                    .Values = values,
                    .PointGeometry = DefaultGeometries.Circle,
                    .PointGeometrySize = 10,
                    .StrokeThickness = 4,
                    .Fill = Brushes.Transparent,
                    .LabelPoint = accuracyFormatter 
                }

                Me.Dispatcher.Invoke(Sub()
                    If _currentUserId.HasValue AndAlso _currentUserId.Value > 0 Then
                        txtTrendLabel.Text = "INDIVIDUAL PROGRESSION"
                    Else
                        txtTrendLabel.Text = "OVERALL BATCH PERFORMANCE"
                    End If

                    ' Update Chart
                    chartGrowth.LegendLocation = LegendLocation.None
                    axisX.Labels = labels
                    chartGrowth.Series = New SeriesCollection From {series}
                    chartGrowth.Update(True, True)
                End Sub)
            End If
        End Sub

        Private Sub chartGrowth_DataClick(sender As Object, chartPoint As ChartPoint)
            Dim pointIndex = CInt(chartPoint.X)
            Dim dateLabel = axisX.Labels(pointIndex)
            Dim isoDate = If(_isoDatePerPoint.TryGetValue(pointIndex, Nothing),
                            _isoDatePerPoint(pointIndex),
                            dateLabel)

            If _isMasteryMode Then Return

            Try
                Dim revieweesForPoint As List(Of RevieweeStatusOut)
                Dim attemptToPass As Integer

                If _currentUserId.HasValue AndAlso _currentUserId.Value > 0 Then
                    revieweesForPoint = If(
                        _loadedReviewees?.Where(Function(r) r.id = _currentUserId.Value).ToList(),
                        New List(Of RevieweeStatusOut)())
                    attemptToPass = pointIndex + 1
                Else
                    Dim ids As List(Of Integer) = Nothing
                    If _examineesPerPoint.TryGetValue(pointIndex, ids) AndAlso
                    ids IsNot Nothing AndAlso ids.Count > 0 Then
                        revieweesForPoint = If(
                            _loadedReviewees?.Where(Function(r) ids.Contains(r.id)).ToList(),
                            New List(Of RevieweeStatusOut)())
                    Else
                        revieweesForPoint = If(_loadedReviewees, New List(Of RevieweeStatusOut)())
                    End If
                    attemptToPass = -1
                End If

                Dim userAttemptMap As New Dictionary(Of Integer, Integer)()
                _attemptIndexPerUser.TryGetValue(pointIndex, userAttemptMap)
                If _currentUserId.HasValue AndAlso _currentUserId.Value > 0 AndAlso userAttemptMap IsNot Nothing Then
                    Dim mappedAttempt As Integer
                    If userAttemptMap.TryGetValue(_currentUserId.Value, mappedAttempt) Then attemptToPass = mappedAttempt
                End If

                RaiseEvent QuestionForensicsRequested(
                    Me,
                    _currentExamId,
                    If(_currentUserId, -1),
                    attemptToPass,
                    revieweesForPoint,
                    userAttemptMap,
                    isoDate) 
            Catch ex As Exception
                Debug.WriteLine($"> Chart Click Error: {ex.Message}")
            Finally
                RaiseEvent LoadingStateChanged(Me, False)
            End Try
        End Sub

        Public Sub ClearChart()
            _isoDatePerPoint.Clear()
            _examineesPerPoint.Clear()
            Me.Dispatcher.Invoke(Sub()
                If chartGrowth.Series IsNot Nothing Then
                    chartGrowth.Series.Clear()
                End If
                axisX.Labels = Nothing
                
                ' Optional: reset the label to a neutral state
                txtTrendLabel.Text = "PREPARING ANALYTICS..."
            End Sub)
        End Sub
    End Class
End Namespace
