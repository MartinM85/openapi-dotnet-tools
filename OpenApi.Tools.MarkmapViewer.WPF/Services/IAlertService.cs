namespace OpenApi.Tools.MarkmapViewer.Services
{
    public interface IAlertService
    {
        void ShowAlert(string title, string message, string cancel = "OK");
    }
}