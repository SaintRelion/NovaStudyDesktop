' VB
Imports System.Windows.Controls
Imports MaterialDesignThemes.Wpf
Imports SmartPrepModern.GlobalContext

Namespace Layout
    Public Class MainLayout
        Inherits UserControl ' Necessary for VB inheritance

        Public Sub New()
            InitializeComponent()
            LoadSidebar()
        End Sub

        Public Sub SetView(view As UserControl)
            MainContent.Content = view
        End Sub

        Public Sub LockSidebar(lock As Boolean)
            For Each child In SidebarButtons.Children
                Dim btn = TryCast(child, Button)
                If btn IsNot Nothing Then
                    btn.IsEnabled = Not lock
                    btn.Opacity = If(lock, 0.4, 1.0)
                End If
            Next
            btnLogout.IsEnabled = Not lock
        End Sub

        Private Sub LoadSidebar()
            SidebarButtons.Children.Clear()

            AddSidebarButton("Performance", "ViewDashboard", AddressOf ExamAnalytics_Click)
            AddSidebarButton("Progress", "ChartAreaspline", AddressOf ComparisonAnalytics_Click)
            AddSidebarButton("Leaderboard", "TrophyVariant", AddressOf Leaderboard_Click)

            Select Case UserSession.Role
                Case "Admin"
                    AddSidebarButton("People", "AccountGroup", AddressOf ManageUsers_Click)
                Case "ReviewDirector"
                    AddSidebarButton("Learning library", "Library", AddressOf UploadSlots_Click)
                    AddSidebarButton("Create assessment", "AutoFix", AddressOf Generate_Click)
                    AddSidebarButton("Assessments", "ClipboardEdit", AddressOf ManageExams_Click)
                Case "Reviewee"
                    AddSidebarButton("My assessments", "ClipboardList", AddressOf ExamSession_Click)
            End Select

            AddSidebarButton("Profile", "AccountCircle", AddressOf Account_Click)

            ' Load first nav button view
            Dim firstBtn = SidebarButtons.Children.OfType(Of Button)().FirstOrDefault()
            If firstBtn IsNot Nothing Then LoadView(firstBtn, DirectCast(firstBtn.Tag, RoutedEventHandler))
        End Sub

        Private Sub AddSidebarButton(text As String, iconKind As String, handler As RoutedEventHandler)
            ' Create the inner StackPanel
            Dim contentStack As New StackPanel With {.Orientation = Orientation.Horizontal}
            
            ' Icon
            Dim icon As New PackIcon With {
                .Kind = DirectCast([Enum].Parse(GetType(PackIconKind), iconKind), PackIconKind),
                .Width = 18, .Height = 18,
                .Margin = New Thickness(0, 0, 8, 0),
                .VerticalAlignment = VerticalAlignment.Center
            }

            ' Text
            Dim txt As New TextBlock With {
                .Text = text,
                .VerticalAlignment = VerticalAlignment.Center,
                .FontSize = 13, .FontWeight = FontWeights.SemiBold
            }

            contentStack.Children.Add(icon)
            contentStack.Children.Add(txt)

            ' The Button
            Dim btn As New Button With {
                .Content = contentStack,
                .Height = 38,
                .Margin = New Thickness(3, 0, 3, 0),
                .Padding = New Thickness(14, 0, 14, 0),
                .HorizontalContentAlignment = HorizontalAlignment.Center,
                .Background = Brushes.Transparent,
                .BorderThickness = New Thickness(0),
                .Foreground = New SolidColorBrush(Color.FromRgb(70, 93, 88)),
                .Style = TryCast(Application.Current.FindResource("MaterialDesignFlatButton"), Style),
                .Tag = handler
            }

            AddHandler btn.Click, Sub(s, e) LoadView(btn, handler)
            SidebarButtons.Children.Add(btn)
        End Sub

        Private Sub LoadView(selectedBtn As Button, handler As RoutedEventHandler)
            ' Reset all buttons to "inactive" look
            For Each btn As Button In SidebarButtons.Children.OfType(Of Button)()
                btn.Foreground = New SolidColorBrush(Color.FromRgb(70, 93, 88))
                btn.Background = Brushes.Transparent
            Next

            selectedBtn.Foreground = Brushes.White
            selectedBtn.Background = New SolidColorBrush(Color.FromRgb(23, 107, 92))
            
            ' Update Header Title based on button text
            Dim sp = TryCast(selectedBtn.Content, StackPanel)
            Dim tb = TryCast(sp.Children(1), TextBlock)
            txtHeaderTitle.Text = tb.Text

            handler.Invoke(selectedBtn, New RoutedEventArgs())
        End Sub

        ' --- View Handlers remain the same, ensuring paths match your project ---
        Private Sub Logout_Click(sender As Object, e As RoutedEventArgs)
            If MessageBox.Show("Sign out of your current session?", "Sign out", MessageBoxButton.YesNo) = MessageBoxResult.Yes Then
                UserSession.Logout()
                Dim parentWin = Window.GetWindow(Me)
                If TypeOf parentWin Is MainWindow Then CType(parentWin, MainWindow).ShowLogin()
            End If
        End Sub

        Private Sub ExamAnalytics_Click(sender As Object, e As RoutedEventArgs)
            MainContent.Content = New SmartPrepModern.Views.Analytics.ExamAnalyticsView()
        End Sub

        Private Sub ComparisonAnalytics_Click(sender As Object, e As RoutedEventArgs)
            MainContent.Content = New SmartPrepModern.Views.Analytics.ComparisonAnalyticsView()
        End Sub

        Private Sub Leaderboard_Click(sender As Object, e As RoutedEventArgs)
            MainContent.Content = New SmartPrepModern.Views.Analytics.LeaderboardView()
        End Sub

        ' ADMIN
        Private Sub ManageUsers_Click(sender As Object, e As RoutedEventArgs)
            MainContent.Content = New SmartPrepModern.Views.Admin.ManageUsersView()
        End Sub

        ' REVIEW DIRECTOR
        Private Sub UploadSlots_Click(sender As Object, e As RoutedEventArgs)
            MainContent.Content = New SmartPrepModern.Views.ReviewDirector.SlotsView()
        End Sub

        Private Sub Generate_Click(sender As Object, e As RoutedEventArgs)
            MainContent.Content = New SmartPrepModern.Views.ReviewDirector.GenerateView()
        End Sub

        Private Sub ManageExams_Click(sender As Object, e As RoutedEventArgs) 
            MainContent.Content = New SmartPrepModern.Views.ReviewDirector.ManageExamView()
        End Sub

        ' REVIEWEE
        Private Sub ExamSession_Click(sender As Object, e As RoutedEventArgs)
            MainContent.Content = New SmartPrepModern.Views.Reviewee.ExamSessionView()
        End Sub

        ' ALL
        Private Sub Account_Click(sender As Object, e As RoutedEventArgs)
            MainContent.Content = New SmartPrepModern.Views.Account.AccountView()
        End Sub
    End Class
End Namespace
