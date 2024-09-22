using System.Windows;

namespace CancellationTest;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private CancellationTokenSource? _tokenSource;

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        _tokenSource = new CancellationTokenSource();
        var token = _tokenSource.Token;

        var progress = new Progress<int>(value =>
        {
            progressBar.Value = value;
            txtReport.Text = value.ToString();
        });

        try
        {
            btnCancel.IsEnabled = true;
            await Task.Run(() => Loop(progress, token), token);
            btnCancel.IsEnabled = false;
            txtReport.Text = "Done";
        }
        catch (OperationCanceledException ex)
        {
            txtReport.Text = "Cancelled";
        }
        finally
        {
            _tokenSource.Dispose();
        }
    }

    private void Loop(IProgress<int> progress, CancellationToken token = default)
    {
        for (var i = 0; i < 100; i++)
        {
            if (token.IsCancellationRequested)
                //progress.Report(0);
                //return;
                // may do some cleanup here
                token.ThrowIfCancellationRequested();


            progress.Report(i);
            Thread.Sleep(100);
        }
    }

    private void Cancel_Button_Click(object sender, RoutedEventArgs e)
    {
        _tokenSource?.Cancel();
        btnCancel.IsEnabled = false;
    }
}