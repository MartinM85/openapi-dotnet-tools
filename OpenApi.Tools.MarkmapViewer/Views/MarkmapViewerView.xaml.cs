using OpenApi.Tools.MarkmapViewer.ViewModels;

namespace OpenApi.Tools.MarkmapViewer.Views;

public partial class MarkmapViewerView : ContentPage
{
    public MarkmapViewerView(MarkmapViewerViewModel model)
    {
        InitializeComponent();

        BindingContext = model;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        SizeChanged += MarkmapViewerView_SizeChanged;
    }

    protected override void OnDisappearing()
    {
        SizeChanged -= MarkmapViewerView_SizeChanged;
        try
        {
            _fitTask?.Wait();
        }
        catch { }
        finally
        {
            _fitTask?.Dispose();
            _fitTask = null;
        }
        base.OnDisappearing();
    }

    bool _fitWindowSize = false;
    Task _fitTask;
    private void MarkmapViewerView_SizeChanged(object sender, EventArgs e)
    {
        if (sender is MarkmapViewerView view)
        {
            webView.HeightRequest = view.Bounds.Height - header1.Height - header2.Height;
            webView.WidthRequest = view.Bounds.Width - 10;

            // do not fit window size of the markmap when window size is changing quickly and previous fit is still in progress
            if (webView.Source != null)
            {
                if (_fitTask != null)
                {
                    if (_fitTask.IsCompleted)
                    {
                        _fitTask.Dispose();
                        _fitTask = null;
                        _fitWindowSize = false;
                    }
                    else
                    {
                        _fitWindowSize = true;
                    }
                }
                else
                {
                    _fitTask = FitWindowSizeAsync();
                }
            }
        }
    }

    private async Task FitWindowSizeAsync()
    {
        do
        {
            try
            {
                // it's not supported on all platforms
                await webView.EvaluateJavaScriptAsync("mm.fit()");
            }
            catch { }
            finally { }
        }
        while (_fitWindowSize);
    }
}