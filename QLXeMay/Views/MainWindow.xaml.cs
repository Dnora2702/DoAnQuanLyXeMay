using System;
using System.Windows;
using QLXeMay.Views.Auth; // Tham chiếu để gọi màn hình Đăng nhập
using QLXeMay.ViewModels; // Tham chiếu để gán DataContext

namespace QLXeMay.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // QUAN TRỌNG: Gán "bộ não" MainViewModel cho giao diện này
            // Nếu không có dòng này, các nút bấm chuyển trang sẽ không hoạt động
            this.DataContext = new MainViewModel();
        }

        /// <summary>
        /// Xử lý sự kiện khi người dùng nhấn nút Đăng xuất trên Menu
        /// </summary>
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Hiển thị hộp thoại xác nhận để tăng trải nghiệm người dùng
            MessageBoxResult result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất khỏi hệ thống?",
                "Xác nhận đăng xuất", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // 1. Xóa thông tin tài khoản đang đăng nhập trong bộ nhớ tạm
                App.CurrentUser = null;
                App.CurrentRole = 0;

                // 2. Khởi tạo lại cửa sổ Đăng nhập
                LoginView loginWindow = new LoginView();

                // 3. Hiển thị cửa sổ đăng nhập lên màn hình
                loginWindow.Show();

                // 4. Đóng cửa sổ chính hiện tại (Lệnh này chỉ chạy được ở file .xaml.cs)
                this.Close();
            }

        }
    }
}