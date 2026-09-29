' vb
Imports SmartPrepModern.APISync.Models

Namespace Components
    Public Class QuestionPreviewDialog
        Inherits UserControl

        Public Sub New()
            InitializeComponent()
        End Sub

        Public Sub LoadItems(items As List(Of QuestionnaireItem))

            Dim displayList As New List(Of QuestionnaireItem)()
            Dim index As Integer = 1

            For Each originalItem In items
                originalItem.id = index
                displayList.Add(originalItem)
                index += 1
            Next

            Me.Dispatcher.Invoke(Sub()
                icQuestions.ItemsSource = displayList
                txtItemCount.Text = $"{displayList.Count} TOTAL ITEMS"
            End Sub)
        End Sub
    End Class
End Namespace
