' vb
Imports SmartPrepModern.APISync.Models
Imports SmartPrepModern.APISync.Repositories
Imports SmartPrepModern.Components

Namespace Views.ReviewDirector
    Public Class SlotsView
        Inherits UserControl

        Private _allCategoryViews As New List(Of CategoryGroupView)()

        Public Sub New()
            InitializeComponent()
            LoadRepository()
        End Sub

        Private Sub SetLoading(isLoading As Boolean)
            Me.Dispatcher.Invoke(Sub()
                pnlLoading.Visibility = If(isLoading, Visibility.Visible, Visibility.Collapsed)
                icCategoryGroups.IsEnabled = Not isLoading
            End Sub)
        End Sub

        Private Sub SetLoadingMessage(message As String)
            txtLoadingStatus.Text = message
        End Sub

        Private Async Sub LoadRepository()
            SetLoading(True)
            Try
                Dim response = Await SlotsRepo.get_categoriesAsync()
                
                If response IsNot Nothing AndAlso response.Success AndAlso response.Data IsNot Nothing Then
                    Me.Dispatcher.Invoke(Sub()
                        _allCategoryViews.Clear()
                        cmbCategoryFilter.Items.Clear()
                
                        cmbCategoryFilter.Items.Add("ALL")

                        For Each cat In response.Data
                            cmbCategoryFilter.Items.Add(cat.name.ToUpper())

                            Dim groupView As New CategoryGroupView()
                            groupView.SetCategory(cat.id, cat.name)
                            
                            AddHandler groupView.RequestLoading, AddressOf SetLoading
                            AddHandler groupView.RequestLoadingMessage, AddressOf SetLoadingMessage
                            AddHandler groupView.AddTopicRequested, AddressOf HandleAddTopicRequested
                            AddHandler groupView.CategoryDeleted, AddressOf HandleCategoryDeleted
                            
                            _allCategoryViews.Add(groupView)
                        Next

                        cmbCategoryFilter.SelectedIndex = 0
                        RefreshDisplayedCategories() 
                    End Sub)

                    For Each cat In response.Data
                        Dim targetView = _allCategoryViews.FirstOrDefault(Function(v) v.CategoryId = cat.id)
                        
                        If targetView IsNot Nothing Then
                            Dim req As New GetByCategoryIdRequest With {.category_id = cat.id}
                            Dim slotResp = Await SlotsRepo.get_slots_by_categoryAsync(req)
                            
                            If slotResp IsNot Nothing AndAlso slotResp.Success AndAlso slotResp.Data IsNot Nothing Then
                                targetView.LoadSlots(slotResp.Data)
                            End If
                        End If
                    Next
                End If
            Catch ex As Exception
                MessageBox.Show($"Sync Error: {ex.Message}")
            Finally
                SetLoading(False)
            End Try
        End Sub

        Private Sub OpenAddCategory_Click(sender As Object, e As RoutedEventArgs)
            pnlAddCategory.Visibility = Visibility.Visible
            pnlAddSlot.Visibility = Visibility.Collapsed
            MainDialogHost.IsOpen = True
        End Sub

        Private Sub HandleAddTopicRequested(sender As Object, categoryId As Integer)
            pnlAddCategory.Visibility = Visibility.Collapsed
            pnlAddSlot.Visibility = Visibility.Visible
            btnConfirmAddSlot.Tag = categoryId
            txtNewSlotName.Clear()
            Me.Dispatcher.Invoke(Sub() MainDialogHost.IsOpen = True)
        End Sub

        Private Sub HandleCategoryDeleted(sender As Object, categoryId As Integer)
            LoadRepository()
        End Sub

        Private Async Sub ConfirmAddCategory_Click(sender As Object, e As RoutedEventArgs)
            Dim catName = txtNewCategoryName.Text.Trim()
            If String.IsNullOrEmpty(catName) Then Return
            
            MainDialogHost.IsOpen = False
            SetLoading(True)
            Try
                Dim resp = Await SlotsRepo.create_categoryAsync(New CategoryCreateRequest With {.name = catName})
                If resp.Success Then LoadRepository()
            Finally
                SetLoading(False)
            End Try
        End Sub

        Private Async Sub ConfirmAddSlot_Click(sender As Object, e As RoutedEventArgs)
            Dim slotName = txtNewSlotName.Text.Trim()
            Dim catId = CInt(btnConfirmAddSlot.Tag)
            If String.IsNullOrEmpty(slotName) Then Return

            MainDialogHost.IsOpen = False
            SetLoading(True)
            Try
                Dim resp = Await SlotsRepo.create_slotAsync(New SlotCreateRequest With {
                    .category_id = catId, 
                    .slot_name = slotName
                })
                If resp.Success Then LoadRepository()
            Finally
                SetLoading(False)
            End Try
        End Sub

        Private Sub cmbCategoryFilter_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            RefreshDisplayedCategories()
        End Sub

        Private Sub RefreshDisplayedCategories()
            If cmbCategoryFilter.SelectedItem Is Nothing Then 
                icCategoryGroups.Items.Clear()
                Return
            End If

            Dim selectedFilter As String = cmbCategoryFilter.SelectedItem.ToString().ToUpper()

            Me.Dispatcher.Invoke(Sub()
                icCategoryGroups.Items.Clear()

                If selectedFilter = "ALL" Then
                    For Each view In _allCategoryViews
                        icCategoryGroups.Items.Add(view)
                    Next
                    Return
                End If

                ' Filter for the single selected category[cite: 8]
                For Each view In _allCategoryViews
                    If view.CategoryName.ToUpper() = selectedFilter Then
                        icCategoryGroups.Items.Add(view)
                        Exit For ' Optimization since names are unique[cite: 8]
                    End If
                Next
            End Sub)
        End Sub
    End Class
End Namespace
