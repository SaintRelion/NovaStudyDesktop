Namespace APISync.Models

    Public Class QuestionOut
        Public Property id As Integer
        Public Property question_text As String
        Public Property correct_answer As String
        Public Property choices As Dictionary(Of String, String)
        Public Property answer As String
    End Class

End Namespace
