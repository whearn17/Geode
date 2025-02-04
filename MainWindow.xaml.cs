using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Google.Apis.Gmail.v1;

namespace geode;
public partial class MainWindow : Window
{
    string? credentialFile;
    private GmailService? gmailService;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void btnBrowse_Click(object sender, RoutedEventArgs e)
    {
        var fileDialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*"
        };

        if (fileDialog.ShowDialog() == true)
        {
            txtConfigPath.Text = fileDialog.FileName;
            credentialFile = fileDialog.FileName;
        }
    }

    private async void btnLogin_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(credentialFile))
        {
            MessageBox.Show("Please select your credentials.json file first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            btnLogin.IsEnabled = false;
            btnLogin.Content = "Authenticating...";

            gmailService = await GmailAuthService.AuthenticateAsync(credentialFile);

            var email = await GmailAuthService.GetUserEmailAsync(gmailService);

            MessageBox.Show($"Successfully logged in as: {email}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            OpenEmailDownloader();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Authentication failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            btnLogin.IsEnabled = true;
            btnLogin.Content = "Login";
        }
    }

    private void OpenEmailDownloader()
    {
        if (gmailService != null)
        {
            var downloaderWindow = new EmailDownloadWindow(gmailService);
            downloaderWindow.Show();
            this.Hide();
        }
        else
        {
            MessageBox.Show("Please login first.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void btnMinimize_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void btnClose_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }
}