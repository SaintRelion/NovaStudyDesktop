' SmartPrepModern.Views.Auth.Register

Imports SmartPrepModern.APISync.Repositories
Imports SmartPrepModern.APISync.Models

Namespace Views.Auth
    Public Class RegisterView
        Inherits UserControl

        Public Event RequestNavigateToLogin As EventHandler

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Async Sub Register_Click(sender As Object, e As RoutedEventArgs)
            ' Validation
            If String.IsNullOrWhiteSpace(txtRegUsername.Text) OrElse 
               String.IsNullOrWhiteSpace(txtRegPassword.Password) OrElse 
               String.IsNullOrWhiteSpace(txtRegEmail.Text) Then
                lblRegStatus.Text = "Requirement Failure: All fields must be populated."
                Return
            End If

            ' UI State
            btnRegister.IsEnabled = False
            lblRegStatus.Text = "Synchronizing with Central Database..."

            Try
                Dim selectedRole As String = CType(cmbRole.SelectedItem, ComboBoxItem).Content.ToString()

                ' Create Request Model based on SR-LEXICON UserRegister
                Dim regReq As New UserRegister With {
                    .username = txtRegUsername.Text,
                    .password = txtRegPassword.Password,
                    .email = txtRegEmail.Text,
                    .role = selectedRole
                }

                ' Use AuthRepo as per SR-LEXICON
                Dim response = Await AuthRepo.registerAsync(regReq)

                If response.Success Then
                    MessageBox.Show("Your account is ready. You can now sign in.", "Account created", MessageBoxButton.OK, MessageBoxImage.Information)
                    RaiseEvent RequestNavigateToLogin(Me, EventArgs.Empty)
                Else
                    lblRegStatus.Text = $"Account could not be created: {response.ErrorMessage}"
                    btnRegister.IsEnabled = True
                End If

            Catch ex As Exception
                lblRegStatus.Text = "System Error: Registration server unreachable."
                btnRegister.IsEnabled = True
            End Try
        End Sub

        Private Sub BackToLogin_Click(sender As Object, e As RoutedEventArgs)
            RaiseEvent RequestNavigateToLogin(Me, EventArgs.Empty)
        End Sub
    End Class
End Namespace
