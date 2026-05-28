using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace QLXeMay
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Lưu thông tin người dùng đang đăng nhập (dùng toàn app)
        public static string CurrentUser { get; set; }
        public static int CurrentRole { get; set; } // 1 = Admin, 2 = Nhân viên

    }
}
