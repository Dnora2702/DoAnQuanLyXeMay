using System.Linq;
using QLXeMay.Models;

using System.Windows;
using System.Windows.Input;

namespace QLXeMay.ViewModels
{
    public class TrangChuViewModel : BaseViewModel
    {
        private decimal _tongDoanhThu;
        public decimal TongDoanhThu { get => _tongDoanhThu; set { _tongDoanhThu = value; OnPropertyChanged(); } }

        private int _tongHoaDon;
        public int TongHoaDon { get => _tongHoaDon; set { _tongHoaDon = value; OnPropertyChanged(); } }

        private int _tongKhachHang;
        public int TongKhachHang { get => _tongKhachHang; set { _tongKhachHang = value; OnPropertyChanged(); } }

        private int _tongXeTonKho;
        public int TongXeTonKho { get => _tongXeTonKho; set { _tongXeTonKho = value; OnPropertyChanged(); } }

        public ICommand ShowSupportCommand { get; set; }
        public ICommand ShowIntroCommand { get; set; }
        public ICommand ShowPrivacyCommand { get; set; }

        public TrangChuViewModel()
        {
            LoadThongKe();

            ShowSupportCommand = new RelayCommand<object>((p) => true, (p) => {
                MessageBox.Show("Liên hệ qua các kênh sau:\n1. Email: nguyenvantuong2605@gmail.com\n2. SĐT: 0984371025", "Hỗ trợ khách hàng", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            ShowIntroCommand = new RelayCommand<object>((p) => true, (p) => {
                string intro = "Cửa hàng XE MÁY PRO được thành lập với mục tiêu trở thành địa chỉ mua bán xe máy uy tín và chất lượng hàng đầu tại HUIT. Chúng tôi mong muốn mang đến cho khách hàng những trải nghiệm mua sắm an toàn, dễ dàng và tiện lợi với nhiều dòng xe chính hãng phù hợp cho mọi nhu cầu sử dụng.\n\nTại XE MÁY PRO, khách hàng có thể dễ dàng tìm thấy nhiều loại xe máy khác nhau từ xe số, xe tay ga đến xe thể thao đến từ các thương hiệu nổi tiếng như Honda, Yamaha, Suzuki và nhiều hãng khác. Không chỉ cung cấp xe mới chất lượng cao, chúng tôi còn hỗ trợ mua bán xe và dịch vụ trả góp nhanh chóng với thủ tục đơn giản.\n\nMục tiêu của chúng tôi là tạo ra một môi trường mua bán minh bạch, chuyên nghiệp và thân thiện. Khách hàng không chỉ được tư vấn tận tình mà còn được hỗ trợ bảo hành, bảo dưỡng và sửa chữa với đội ngũ Nhóm .NET hàng đầu khóa 15DHTH với kỹ thuật viên giàu kinh nghiệm.\n\nMỗi chiếc xe không chỉ là phương tiện di chuyển mà còn là người bạn đồng hành trong cuộc sống hằng ngày. Vì vậy, XE MÁY PRO luôn cố gắng mang đến những sản phẩm tốt nhất với mức giá hợp lý nhất để đáp ứng nhu cầu của mọi khách hàng.\n\nNếu bạn đang tìm kiếm một chiếc xe phù hợp cho công việc, học tập hay đam mê tốc độ, hãy đến với XE MÁY PRO ngay hôm nay để trải nghiệm dịch vụ chuyên nghiệp và sở hữu chiếc xe mơ ước của mình!";
                MessageBox.Show(intro, "Giới thiệu XE MÁY PRO", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            ShowPrivacyCommand = new RelayCommand<object>((p) => true, (p) => {
                string privacy = "1. Bảo mật thông tin khách hàng\nXE MÁY PRO cam kết bảo mật tuyệt đối các thông tin cá nhân của khách hàng như họ tên, số điện thoại, địa chỉ và email. Thông tin chỉ được sử dụng nhằm mục đích hỗ trợ mua bán và chăm sóc khách hàng.\n\n2. Không chia sẻ thông tin cho bên thứ ba\nChúng tôi không cung cấp, mua bán hoặc trao đổi thông tin khách hàng cho bất kỳ cá nhân hay tổ chức nào khác nếu chưa có sự đồng ý từ khách hàng, trừ trường hợp theo yêu cầu của pháp luật.\n\n3. Bảo mật thông tin thanh toán\nMọi thông tin liên quan đến thanh toán đều được mã hóa và lưu trữ an toàn nhằm đảm bảo quyền lợi và tránh rủi ro cho khách hàng khi giao dịch tại XE MÁY PRO.\n\n4. Quyền kiểm tra và cập nhật thông tin\nKhách hàng có quyền yêu cầu kiểm tra, chỉnh sửa hoặc xóa thông tin cá nhân đã cung cấp cho cửa hàng bất cứ lúc nào thông qua bộ phận chăm sóc khách hàng.\n\n5. Cam kết an toàn hệ thống\nXE MÁY PRO luôn nâng cấp hệ thống bảo mật, sử dụng các biện pháp kỹ thuật phù hợp để ngăn chặn việc truy cập trái phép, mất mát hoặc rò rỉ dữ liệu khách hàng.";
                MessageBox.Show(privacy, "Chính sách bảo mật", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public void LoadThongKe()
        {
            using (var db = new QLXeMayEntities())
            {
                // Dùng LINQ để đếm và tính tổng từ cơ sở dữ liệu (thêm AsNoTracking để luôn lấy mới nhất)
                TongDoanhThu = db.HoaDons.AsNoTracking().Any() ? (decimal)db.HoaDons.AsNoTracking().Sum(x => x.TongTien) : 0m;
                TongHoaDon = db.HoaDons.AsNoTracking().Count();
                TongKhachHang = db.KhachHangs.AsNoTracking().Count();
                TongXeTonKho = db.XeMays.AsNoTracking().Any() ? (db.XeMays.AsNoTracking().Sum(x => x.SoLuong) ?? 0) : 0;
            }
        }
    }
}