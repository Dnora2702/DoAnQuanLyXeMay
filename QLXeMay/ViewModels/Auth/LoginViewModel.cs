using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QLXeMay.Models;
using System.Windows.Input;
using System.Windows;
using QLXeMay.Views.Auth;
using QLXeMay.Services;
using System.Data.Entity;

namespace QLXeMay.ViewModels.Auth
{

    
public class LoginViewModel : BaseViewModel
    {
        // ── Properties ──────────────────────────────────────────
        private string _userName;
        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set { _hasError = value; OnPropertyChanged(); }
        }

        // ── Commands ─────────────────────────────────────────────
        public ICommand LoginCommand { get; set; }
        public ICommand RegisterWindowCommand { get; set; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand<object>(
                (p) => true,
                async (p) => await LoginAsync()
            );

            RegisterWindowCommand = new RelayCommand<object>(
                (p) => true,
                (p) =>
                {
                    var regWindow = new RegisterView();
                    regWindow.ShowDialog();
                }
            );
        }

        // ── Logic ─────────────────────────────────────────────────
        // ── Logic ─────────────────────────────────────────────────
        private async System.Threading.Tasks.Task LoginAsync()
        {
            // --- Validation ---
            if (string.IsNullOrWhiteSpace(UserName))
            {
                ShowError("Vui lòng nhập tên đăng nhập.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowError("Vui lòng nhập mật khẩu.");
                return;
            }

            // --- Kiểm tra DB ---
            using (var db = new QLXeMayEntities())
            {
                string hashedPass = SecurityHelper.HashPassword(Password);
                
                // Add fallback for old unhashed passwords
                var user = await db.TaiKhoans
                    .FirstOrDefaultAsync(x => x.TenDangNhap == UserName && (x.MatKhau == hashedPass || x.MatKhau == Password));

                if (user == null)
                {
                    ShowError("Sai tên đăng nhập hoặc mật khẩu.");
                    return;
                }

                // If user logged in with old password, update it to hashed
                if (user.MatKhau == Password && user.MatKhau != hashedPass)
                {
                    user.MatKhau = hashedPass;
                    await db.SaveChangesAsync();
                }

                // --- Đăng nhập thành công: Lưu thông tin phiên làm việc ---
                App.CurrentUser = user.TenDangNhap;
                App.CurrentRole = user.Quyen ?? 2; // 1=Admin, 2=Nhân viên

                // Mở MainWindow, đóng LoginView
                var mainWindow = new Views.MainWindow();
                mainWindow.Show();
                DialogService.CloseWindow(this);
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }
    }
}
