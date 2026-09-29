Imports SmartPrepModern.APISync.Repositories
Imports SmartPrepModern.APISync.Models

Namespace Views.Auth
    Public Class ResetPasswordView
        Inherits UserControl

        Public Event RequestNavigateToLogin As EventHandler

        Public Sub New()
            InitializeComponent()
        End Sub

        ''' <summary>
        ''' PHASE 1: Initialize recovery by requesting a signed JWT token.
        ''' </summary>
        Private Async Sub RequestToken_Click(sender As Object, e As RoutedEventArgs)
            If String.IsNullOrWhiteSpace(txtResetEmail.Text) Then
                lblResetStatus.Text = "Validation Error: Email required."
                Return
            End If

            btnRequestToken.IsEnabled = False
            lblResetStatus.Text = "Sending your verification code..."

            Try
                Dim req As New PasswordResetRequest With {.email = txtResetEmail.Text}
                
                ' Call per SR-LEXICON
                Dim response = Await AuthRepo.request_resetAsync(req)

                If response.Success Then
                    ' Flip to verification panel
                    pnlRequest.Visibility = Visibility.Collapsed
                    pnlVerify.Visibility = Visibility.Visible
                    lblResetStatus.Text = "Code sent. Check your inbox."
                    lblResetStatus.Foreground = New SolidColorBrush(Colors.Gray)
                Else
                    lblResetStatus.Text = $"Request Denied: {response.ErrorMessage}"
                    btnRequestToken.IsEnabled = True
                End If
            Catch ex As Exception
                lblResetStatus.Text = "Critical Error: Server unreachable."
                btnRequestToken.IsEnabled = True
            End Try
        End Sub

        ''' <summary>
        ''' PHASE 2: Submit the token and new password for final credential update.
        ''' </summary>
        Private Async Sub ConfirmReset_Click(sender As Object, e As RoutedEventArgs)
            ' Input Validation
            If String.IsNullOrWhiteSpace(txtToken.Text) OrElse String.IsNullOrWhiteSpace(txtNewPassword.Password) Then
                lblResetStatus.Text = "Validation Error: Missing token or password."
                Return
            End If

            btnConfirmReset.IsEnabled = False
            lblResetStatus.Text = "Updating your password..."

            Try
                ' Model matches SR-LEXICON PasswordResetConfirm
                Dim req As New PasswordResetConfirm With {
                    .token = txtToken.Text,
                    .new_password = txtNewPassword.Password
                }

                ' Call per SR-LEXICON
                Dim response = Await AuthRepo.confirm_resetAsync(req)

                If response.Success Then
                    MessageBox.Show("Your password has been updated.", "Password updated", MessageBoxButton.OK, MessageBoxImage.Information)
                    RaiseEvent RequestNavigateToLogin(Me, EventArgs.Empty)
                Else
                    lblResetStatus.Text = $"Update Failure: {response.ErrorMessage}"
                    btnConfirmReset.IsEnabled = True
                End If
            Catch ex As Exception
                lblResetStatus.Text = "Critical Error: Synchronization protocol failed."
                btnConfirmReset.IsEnabled = True
            End Try
        End Sub

        Private Sub BackToLogin_Click(sender As Object, e As RoutedEventArgs)
            RaiseEvent RequestNavigateToLogin(Me, EventArgs.Empty)
        End Sub
    End Class
End Namespace
