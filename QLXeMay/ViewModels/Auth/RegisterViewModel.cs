using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QLXeMay.Models;
using QLXeMay.Services;
using System.Data.Entity;

namespace QLXeMay.ViewModels.Auth
{
    public class RegisterViewModel: BaseViewModel
    {
        // ── Properties ───────────────────────────────────────────
        private string _userName;
        public string UserName
        {
            get => _userName;
            set { _userName = value; OnPropertyChanged(); }
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
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

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set { _successMessage = value; OnPropertyChanged(); }
        }

        private bool _hasSuccess;
        public bool HasSuccess
        {
            get => _hasSuccess;
            set { _hasSuccess = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; set; }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand<object>(
                (p) => true,
                async (p) => await RegisterAsync()
            );
        }

        // ── Method gọi từ code-behind ─────────────────────────────
        public async System.Threading.Tasks.Task RegisterAsync()
        {
            // Reset thông báo
            HasError = false;
            HasSuccess = false;

            // --- Validation ---
            if (!Validator.IsNotEmpty(UserName))
            { ShowError("Tên đăng nhập không được để trống."); return; }

            if (UserName.Length < 4)
            { ShowError("Tên đăng nhập phải có ít nhất 4 ký tự."); return; }

            if (!Validator.IsNotEmpty(FullName))
            { ShowError("Họ và tên không được để trống."); return; }

            if (!Validator.IsValidPhone(Phone))
            { ShowError("Số điện thoại không hợp lệ (VD: 0912345678)."); return; }

            if (!Validator.IsPasswordStrong(Password))
            { ShowError("Mật khẩu phải có ít nhất 6 ký tự."); return; }

            if (Password != ConfirmPassword)
            { ShowError("Mật khẩu xác nhận không khớp."); return; }

            // --- Kiểm tra tên đăng nhập đã tồn tại chưa ---
            using (var db = new QLXeMayEntities())
            {
                bool exists = await db.TaiKhoans.AnyAsync(x => x.TenDangNhap == UserName);
                if (exists)
                { ShowError("Tên đăng nhập đã tồn tại, vui lòng chọn tên khác."); return; }

                // --- Tạo TaiKhoan ---
                var newAccount = new TaiKhoan
                {
                    TenDangNhap = UserName.Trim(),
                    MatKhau = SecurityHelper.HashPassword(Password),
                    Quyen = 2  // Mặc định là Nhân viên
                };
                db.TaiKhoans.Add(newAccount);

                // --- Tạo NhanVien tương ứng ---
                var newStaff = new NhanVien
                {
                    TenNV = FullName.Trim(),
                    SDT = Phone.Trim(),
                    DiaChi = "",
                    TenDangNhap = UserName.Trim()
                };
                db.NhanViens.Add(newStaff);

                await db.SaveChangesAsync();
            }

            // Xóa form sau khi thành công
            UserName = "";
            FullName = "";
            Phone = "";
            Password = "";
            ConfirmPassword = "";

            ShowSuccess("Đăng ký thành công! Tài khoản đã được tạo.");
        }

        // ── Helpers ───────────────────────────────────────────────
        private void ShowError(string msg)
        {
            ErrorMessage = msg;
            HasError = true;
            HasSuccess = false;
        }

        private void ShowSuccess(string msg)
        {
            SuccessMessage = msg;
            HasSuccess = true;
            HasError = false;
        }
    }
}
