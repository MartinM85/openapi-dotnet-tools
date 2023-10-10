using OpenApi.Tools.Core;
using OpenApi.Tools.MarkmapViewer.Services;
using OpenApi.Tools.MarkmapViewer.WPF.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

namespace OpenApi.Tools.MarkmapViewer.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Task _fitTask;

        public MainWindow()
        {
            InitializeComponent();
            var tools = new OpenApiTools();
            var alertService = new AlertService();
            DataContext = new MarkmapViewerViewModel(tools, alertService);
            SizeChanged += MainWindow_SizeChanged;
        }
        
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is MainWindow window)
            {
                webView.Height = window.ActualHeight - header1.ActualHeight - header2.ActualHeight - 35;
                webView.Width = window.ActualWidth - 10;

                if (webView.Source != null)
                {
                    if (_fitTask != null)
                    {
                        if (_fitTask.IsCompleted)
                        {
                            _fitTask.Dispose();
                            _fitTask = null;
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
            try
            {
                await webView.ExecuteScriptAsync("mm.fit()");
            }
            catch { }
            finally { }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            SizeChanged -= MainWindow_SizeChanged;
        }
    }
}
