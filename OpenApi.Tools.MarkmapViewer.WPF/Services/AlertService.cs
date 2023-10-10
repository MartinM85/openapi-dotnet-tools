using System.Windows;

namespace OpenApi.Tools.MarkmapViewer.Services
{
    public class AlertService : IAlertService
    {
        public void ShowAlert(string title, string message, string cancel = "OK")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK);
        }
    }
}
