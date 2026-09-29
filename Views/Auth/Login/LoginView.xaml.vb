' SmartPrepModern.Views.Auth

Imports SmartPrepModern.APISync.Repositories
Imports SmartPrepModern.APISync.Models
Imports SmartPrepModern.GlobalContext

Namespace Views.Auth
    Public Class LoginView
        Inherits UserControl

        Private OnSuccess As LoginSuccessHandler
        Public Event RequestNavigateToRegister As EventHandler
        Public Event RequestNavigateToReset As EventHandler

        Public Sub New(successCallback As LoginSuccessHandler)
            InitializeComponent()
            Me.OnSuccess = successCallback
        End Sub

        Private Async Sub Login_Click(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrWhiteSpace(txtUsername.Text) OrElse String.IsNullOrWhiteSpace(txtPassword.Password) Then
                lblStatus.Text = "Enter your username and password."
                Return
            End If

            btnLogin.IsEnabled = False
            lblStatus.Text = "Signing you in..."

            Try
                Dim loginReq As New UserLogin With {
                    .username = txtUsername.Text,
                    .password = txtPassword.Password
                }

                Dim response = Await AuthRepo.loginAsync(loginReq)

                If response.Success Then
                    ' --- SESSION INITIALIZATION PROTOCOL ---
                    ' Populate the global UserSession for use in AccountView and Analytics
                    UserSession.UserID = response.Data.id
                    UserSession.Role = response.Data.role
                    UserSession.Username = txtUsername.Text
                    UserSession.Email = response.Data.email
                    UserSession.Status = "active" ' If they logged in, status is active
                    
                    ' Handshake with parent window/layout
                    OnSuccess?.Invoke(response.Data.role, response.Data.id)
                Else
                    lblStatus.Foreground = New SolidColorBrush(Color.FromRgb(224, 82, 117))
                    lblStatus.Text = response.ErrorMessage 
                    btnLogin.IsEnabled = True
                End If

            Catch ex As Exception
                lblStatus.Text = "We couldn’t connect. Please try again."
                btnLogin.IsEnabled = True
            End Try
        End Sub

        ' NEW: Handler for Reset Password Navigation
        Private Sub ForgotPassword_Click(sender As Object, e As RoutedEventArgs)
            RaiseEvent RequestNavigateToReset(Me, EventArgs.Empty)
        End Sub

        Private Sub ShowSignup_Click(sender As Object, e As RoutedEventArgs)
            RaiseEvent RequestNavigateToRegister(Me, EventArgs.Empty)
        End Sub

        Private Sub Exit_Click(sender As Object, e As RoutedEventArgs)
            Dim result = MessageBox.Show("Are you sure you want to close NovaStudy?", 
                                        "Close application", 
                                        MessageBoxButton.YesNo, 
                                        MessageBoxImage.Warning)

            If result = MessageBoxResult.Yes Then
                Application.Current.Shutdown()
            End If
        End Sub
    End Class
End Namespace
