Namespace APISync.Models

    Public Class SourceReferenceItem
        Public Property id As Integer
        Public Property category_id As Integer
        Public Property slot_name As String
        Public Property material_path As String
        Public Property questionnaire_path As String
        Public Property is_material_uploaded As Boolean
        Public Property is_questionnaire_extracted As Boolean
        Public Property questionnaire_status As String
        Public Property questionnaire_error As String
        Public Property questionnaire_discarded As Integer
        Public ReadOnly Property can_modify As Boolean
            Get
                Return questionnaire_status <> "pending" AndAlso questionnaire_status <> "processing"
            End Get
        End Property
        Public ReadOnly Property questionnaire_status_label As String
            Get
                Select Case questionnaire_status
                    Case "pending" : Return "QUEUED FOR EXTRACTION"
                    Case "processing" : Return "EXTRACTING QUESTIONS..."
                    Case "failed" : Return "EXTRACTION FAILED — REUPLOAD TO RETRY"
                    Case "done"
                        If questionnaire_discarded > 0 Then Return $"{questionnaire_discarded} INCOMPLETE ITEMS SKIPPED — REVIEW QUESTIONS"
                End Select
                Return ""
            End Get
        End Property
        Public Property item_count As Integer
        Public Property active_exam_count As Integer
        Public Property created_at As String
    End Class

End Namespace
