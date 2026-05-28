using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using QLXeMay.Models;
using QLXeMay.Services;


namespace QLXeMay.ViewModels.Auth
{
    public class ChangePassViewModel : BaseViewModel
    {
        // ── Properties ───────────────────────────────────────────
        public string HeaderSubtitle => $"Tài khoản: {App.CurrentUser}";

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

        // ── Method gọi từ code-behind ─────────────────────────────
        public void ChangePass(PasswordBox txtOld, PasswordBox txtNew, PasswordBox txtConfirm)
        {
            HasError = false;
            HasSuccess = false;

            // --- Validation ---
            if (string.IsNullOrWhiteSpace(txtOld.Password))
            { ShowError("Vui lòng nhập mật khẩu hiện tại."); return; }

            if (!Validator.IsPasswordStrong(txtNew.Password))
            { ShowError("Mật khẩu mới phải có ít nhất 6 ký tự."); return; }

            if (txtNew.Password != txtConfirm.Password)
            { ShowError("Mật khẩu xác nhận không khớp."); return; }

            if (txtOld.Password == txtNew.Password)
            { ShowError("Mật khẩu mới phải khác mật khẩu hiện tại."); return; }

            // --- Kiểm tra mật khẩu cũ và cập nhật ---
            using (var db = new QLXeMayEntities())
            {
                var account = db.TaiKhoans
                    .FirstOrDefault(x => x.TenDangNhap == App.CurrentUser
                                      && x.MatKhau == txtOld.Password);

                if (account == null)
                { ShowError("Mật khẩu hiện tại không đúng."); return; }

                account.MatKhau = txtNew.Password;
                db.SaveChanges();
            }

            // Xóa các ô sau khi thành công
            txtOld.Clear();
            txtNew.Clear();
            txtConfirm.Clear();

            ShowSuccess("Đổi mật khẩu thành công!");
        }

        // ── Helpers ──────────────────────────────────────────────
        private void ShowError(string msg)
        {
            ErrorMessage = msg; HasError = true; HasSuccess = false;
        }

        private void ShowSuccess(string msg)
        {
            SuccessMessage = msg; HasSuccess = true; HasError = false;
        }
    }
}
