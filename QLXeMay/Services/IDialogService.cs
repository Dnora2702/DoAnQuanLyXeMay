using System.Windows;

namespace QLXeMay.Services
{
    public interface IDialogService
    {
        void ShowMessage(string message, string title);
        void ShowError(string message, string title);
        void ShowSuccess(string message, string title);
        bool ShowConfirmation(string message, string title);
        void CloseWindow(object viewModel);
    }
}
