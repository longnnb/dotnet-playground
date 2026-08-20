using System.Diagnostics;
using System.Windows;

namespace CancellationTokenWpf;

/// <summary>
/// Interaction logic for MainWindow.xaml -- demonstrates <see cref="CancellationTokenSource"/>
/// with a configurable timeout, manual cancellation combined via
/// <see cref="CancellationTokenSource.CreateLinkedTokenSource(CancellationToken[])"/>, a
/// <see cref="CancellationToken.Register(Action)"/> callback, and (via
/// <see cref="ProgressWorker"/>) both the CPU-bound polling and I/O-bound await-native shapes
/// cancellable work takes.
/// </summary>
/// <remarks>
/// An earlier version of this window had four bugs, all fixed here: a 2-second timeout raced
/// against a ~10-second loop, so the CTS always expired first and the success path -- and
/// manual cancellation -- could never be observed (fixed with a shorter loop and a configurable
/// timeout, defaulting to 30s so all three outcomes are reachable); the Start button was never
/// disabled, so double-clicking it created two concurrent loops sharing one
/// <see cref="CancellationTokenSource"/>, and the first loop to finish would dispose a source
/// the second was still using; <c>CancelButton.IsEnabled = false</c> only ran on the success
/// path, leaving it enabled (pointing at an already-disposed source) after a timeout; and the
/// cancellation check in the work loop was a braceless <c>if</c> whose apparent body was three
/// comment lines, with the real statement four lines below -- see <see cref="ProgressWorker"/>.
/// </remarks>
public partial class MainWindow : Window
{
    private CancellationTokenSource? _manualCancelSource;

    /// <summary>Initializes the window.</summary>
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        var timeoutSeconds = int.TryParse(TimeoutTextBox.Text, out var parsed) ? parsed : 30;

        using var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        using var manualSource = new CancellationTokenSource();
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, manualSource.Token);

        // CancelButton_Click reaches this instance through the field; it never sees timeoutSource
        // or the linked source, only the reason a click should ever cancel for.
        _manualCancelSource = manualSource;

        using var registration = linkedSource.Token.Register(() =>
            Debug.WriteLine($"token.Register callback fired. TimedOut={timeoutSource.IsCancellationRequested}, ManuallyCancelled={manualSource.IsCancellationRequested}"));

        var progress = new Progress<int>(value =>
        {
            WorkProgressBar.Value = value;
            StatusText.Text = $"{value}/{ProgressWorker.StepCount}";
        });

        StartButton.IsEnabled = false;
        CancelButton.IsEnabled = true;

        try
        {
            if (UseAsyncWorkCheckBox.IsChecked == true)
            {
                await ProgressWorker.RunAsyncWorkAsync(progress, linkedSource.Token);
            }
            else
            {
                await Task.Run(() => ProgressWorker.RunBlockingWork(progress, linkedSource.Token), linkedSource.Token);
            }

            StatusText.Text = "Done";
        }
        catch (OperationCanceledException)
        {
            StatusText.Text = timeoutSource.IsCancellationRequested ? "Cancelled (timeout)" : "Cancelled (manual)";
        }
        finally
        {
            StartButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
            _manualCancelSource = null;
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        _manualCancelSource?.Cancel();
    }
}
