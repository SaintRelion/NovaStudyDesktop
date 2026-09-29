Imports SmartPrepModern.APISync.Models
Imports System.ComponentModel

Namespace Components.Models
    Public Class QuestionForensicWrapper 
        Implements INotifyPropertyChanged

            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

            Protected Sub OnPropertyChanged(propertyName As String)
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
            End Sub

            Private _isAIVisible As Boolean = True
            Public Property IsAIVisible As Boolean
                Get
                    Return _isAIVisible
                End Get
                Set(value As Boolean)
                    _isAIVisible = value
                    OnPropertyChanged(NameOf(IsAIVisible))
                End Set
            End Property

            Public Property Id As Integer

            ' Mapping directly to your QuestionForensic model properties
            Public Property CategoryId As Integer
            Public Property CategoryName As String
            Public Property SlotName As String
            Public Property QuestionText As String
            Public Property StudentAnswer As String
            Public Property CorrectAnswer As String
            Public Property IsCorrect As Boolean
            
            ' Full Option Analysis
            Public Property OptionAnalysis As Dictionary(Of String, String)
            
            ' For Comparative Logic
            Public Property IsComparative As Boolean = False
            Public Property PreviousAnswer As String
            Public Property WasCorrect As Boolean
    End Class
End Namespace
