using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Google.Apis.Gmail.v1;
using Microsoft.Win32;
using System.IO;

namespace geode;

public partial class EmailDownloadWindow : Window
{
    private readonly GmailService _gmailService;
    private int setCounter = 0;

    public EmailDownloadWindow(GmailService gmailService)
    {
        InitializeComponent();
        _gmailService = gmailService;
        AddNewSet();
    }

    private void AddNewSet()
    {
        setCounter++;

        var setGrid = new Grid
        {
            Margin = new Thickness(0, 0, 0, 20)
        };

        setGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        setGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        setGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Header with remove button
        var headerGrid = new Grid();
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var setLabel = new TextBlock
        {
            Text = $"Email Set {setCounter}",
            Foreground = System.Windows.Media.Brushes.White,
            FontSize = 18,
            Margin = new Thickness(0, 0, 0, 10)
        };
        Grid.SetColumn(setLabel, 0);

        var removeButton = new Button
        {
            Content = "×",
            Width = 30,
            Height = 30,
            Style = (Style)FindResource("WindowButtonStyle"),
            Visibility = setCounter == 1 ? Visibility.Collapsed : Visibility.Visible
        };
        removeButton.Click += (s, e) => emailSetsPanel.Children.Remove(setGrid);
        Grid.SetColumn(removeButton, 1);

        headerGrid.Children.Add(setLabel);
        headerGrid.Children.Add(removeButton);
        Grid.SetRow(headerGrid, 0);
        setGrid.Children.Add(headerGrid);

        // Username TextBox
        var usernameBox = new TextBox
        {
            Style = (Style)FindResource("ModernTextBox"),
            Tag = "Username/Email",
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(5),
            FontSize = 14,
            Background = System.Windows.Media.Brushes.Transparent,
            Foreground = System.Windows.Media.Brushes.White,
            CaretBrush = System.Windows.Media.Brushes.White
        };
        Grid.SetRow(usernameBox, 1);
        setGrid.Children.Add(usernameBox);

        // Message IDs TextBox with improved multiline support and styling
        var messageIdsBox = new TextBox
        {
            Style = (Style)FindResource("ModernTextBox"),
            Tag = "Message IDs (one per line)",
            Height = 100,
            TextWrapping = TextWrapping.Wrap,
            AcceptsReturn = true,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            Margin = new Thickness(0, 0, 0, 0),
            Padding = new Thickness(5),
            FontSize = 14,
            Background = System.Windows.Media.Brushes.Transparent,
            Foreground = System.Windows.Media.Brushes.White,
            CaretBrush = System.Windows.Media.Brushes.White,
            AcceptsTab = true,
            IsUndoEnabled = true
        };
        Grid.SetRow(messageIdsBox, 2);
        setGrid.Children.Add(messageIdsBox);

        emailSetsPanel.Children.Add(setGrid);
    }

    private void btnAddSet_Click(object sender, RoutedEventArgs e)
    {
        AddNewSet();
    }

    private async void btnDownload_Click(object sender, RoutedEventArgs e)
    {
        var downloadSets = new List<(string username, string[] messageIds)>();

        foreach (Grid setGrid in emailSetsPanel.Children)
        {
            var username = ((TextBox)setGrid.Children[1]).Text.Trim();
            var messageIdsText = ((TextBox)setGrid.Children[2]).Text.Trim();

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(messageIdsText))
            {
                var messageIds = messageIdsText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => id.Trim())
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToArray();
                downloadSets.Add((username, messageIds));
            }
        }

        if (downloadSets.Count == 0)
        {
            MessageBox.Show("Please add at least one valid email set to download.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        OpenFolderDialog outputPathPicker = new OpenFolderDialog();

        string outputPath = "";

        if (outputPathPicker.ShowDialog() == true)
        {
            outputPath = outputPathPicker.FolderName;
        }

        foreach (var downloadSet in downloadSets)
        {
            foreach (string rfcMessageId in downloadSet.messageIds)
            {
                try
                {
                    MessageBox.Show($"Collecing message ID {rfcMessageId} for user {downloadSet.username}", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    await GmailDownloadService.DownloadSingleEmailAsync(_gmailService, downloadSet.username, rfcMessageId, Path.Combine(outputPath, downloadSet.username));
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"An error occured while getting message ID {rfcMessageId}: {exception.ToString()}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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