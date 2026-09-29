Namespace APISync.Models

    Public Class ComparativeTrendResponse
        Public Property exam_id As Integer
        Public Property user_id As Integer?
        Public Property trend_label As String
        Public Property current_status As String
        Public Property delta As Double
        Public Property history As List(Of BatchPerformance)
    End Class

End Namespace
