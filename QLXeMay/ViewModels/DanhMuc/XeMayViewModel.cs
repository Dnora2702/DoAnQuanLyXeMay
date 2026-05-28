using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Data.Entity;
using Microsoft.Win32;
using QLXeMay.Models;

namespace QLXeMay.ViewModels.DanhMuc
{
    public class XeMayViewModel : BaseViewModel
    {
        private ObservableCollection<XeMay> _listXeMay;
        public ObservableCollection<XeMay> ListXeMay { get => _listXeMay; set { _listXeMay = value; OnPropertyChanged(); } }

        // Danh sách Hãng xe để hiển thị lên ComboBox
        private ObservableCollection<HangXe> _listHangXe;
        public ObservableCollection<HangXe> ListHangXe { get => _listHangXe; set { _listHangXe = value; OnPropertyChanged(); } }

        // Các thuộc tính của Xe Máy
        private string _tenXe;
        public string TenXe
        {
            get => _tenXe;
            set
            {
                _tenXe = value;
                OnPropertyChanged();
                if (string.IsNullOrWhiteSpace(_tenXe)) AddError(nameof(TenXe), "Tên xe không được để trống");
                else ClearErrors(nameof(TenXe));
            }
        }

        private HangXe _selectedHangXe;
        public HangXe SelectedHangXe
        {
            get => _selectedHangXe;
            set
            {
                _selectedHangXe = value;
                OnPropertyChanged();
                if (_selectedHangXe == null) AddError(nameof(SelectedHangXe), "Vui lòng chọn hãng xe");
                else ClearErrors(nameof(SelectedHangXe));
            }
        }

        private int _soLuong;
        public int SoLuong
        {
            get => _soLuong;
            set
            {
                _soLuong = value;
                OnPropertyChanged();
                if (_soLuong < 0) AddError(nameof(SoLuong), "Số lượng không được âm");
                else ClearErrors(nameof(SoLuong));
            }
        }

        private decimal _giaBan;
        public decimal GiaBan
        {
            get => _giaBan;
            set
            {
                _giaBan = value;
                OnPropertyChanged();
                if (_giaBan <= 0) AddError(nameof(GiaBan), "Giá bán phải lớn hơn 0");
                else ClearErrors(nameof(GiaBan));
            }
        }

        // Lưu đường dẫn ảnh
        private string _hinhAnh; public string HinhAnh { get => _hinhAnh; set { _hinhAnh = value; OnPropertyChanged(); } }

        // --- CHỨC NĂNG TÌM KIẾM THÊM VÀO ĐÂY ---
        private ObservableCollection<string> _listPriceFilters = new ObservableCollection<string>
        {
            "Tất cả mức giá",
            "Dưới 50.000.000",
            "50.000.000 - 100.000.000",
            "100.000.000 - 200.000.000",
            "Trên 200.000.000"
        };
        public ObservableCollection<string> ListPriceFilters { get => _listPriceFilters; set { _listPriceFilters = value; OnPropertyChanged(); } }

        private string _selectedPriceFilter = "Tất cả mức giá";
        public string SelectedPriceFilter
        {
            get => _selectedPriceFilter;
            set
            {
                _selectedPriceFilter = value;
                OnPropertyChanged();
                _ = SearchDataAsync();
            }
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                _searchKeyword = value;
                OnPropertyChanged();
                _ = SearchDataAsync(); // Tự động lọc mỗi khi người dùng gõ phím
            }
        }

        private XeMay _selectedItem;
        public XeMay SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    TenXe = SelectedItem.TenXe;
                    SoLuong = SelectedItem.SoLuong ?? 0;
                    GiaBan = SelectedItem.GiaBan ?? 0;
                    HinhAnh = SelectedItem.HinhAnh;
                    SelectedHangXe = ListHangXe.FirstOrDefault(x => x.MaHang == SelectedItem.MaHang);
                }
            }
        }

        public ICommand ChonAnhCommand { get; set; }
        public ICommand AddCommand { get; set; }
        public ICommand EditCommand { get; set; }
        public ICommand ExportPdfCommand { get; set; }

        public XeMayViewModel()
        {
            _ = LoadDataAsync();

            // Lệnh chọn ảnh từ máy tính
            ChonAnhCommand = new RelayCommand<object>((p) => true, (p) => {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
                if (openFileDialog.ShowDialog() == true)
                {
                    // Lấy đường dẫn ảnh người dùng vừa chọn
                    HinhAnh = openFileDialog.FileName;
                }
            });

            // Lệnh Thêm mới
            AddCommand = new RelayCommand<object>((p) => !HasErrors && !string.IsNullOrEmpty(TenXe) && SelectedHangXe != null, async (p) => {
                using (var db = new QLXeMayEntities())
                {
                    // Copy ảnh vào thư mục dự án (nếu có chọn ảnh)
                    string fileName = "default.jpg";
                    if (!string.IsNullOrEmpty(HinhAnh) && File.Exists(HinhAnh))
                    {
                        fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(HinhAnh);
                        string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "XeMay");
                        if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
                        string destFile = Path.Combine(targetPath, fileName);
                        if (!File.Exists(destFile)) File.Copy(HinhAnh, destFile, true);
                    }

                    // Lưu vào DB
                    db.XeMays.Add(new XeMay()
                    {
                        TenXe = TenXe,
                        MaHang = SelectedHangXe.MaHang,
                        SoLuong = SoLuong,
                        GiaBan = GiaBan,
                        HinhAnh = "/Images/XeMay/" + fileName // Lưu đường dẫn tương đối
                    });
                    await db.SaveChangesAsync();
                    _ = RefreshAsync();
                }
            });

            // Lệnh Sửa
            EditCommand = new RelayCommand<object>((p) => SelectedItem != null && !HasErrors && !string.IsNullOrEmpty(TenXe) && SelectedHangXe != null, async (p) => {
                using (var db = new QLXeMayEntities())
                {
                    var xe = await db.XeMays.FindAsync(SelectedItem.MaXe);
                    if (xe != null)
                    {
                        xe.TenXe = TenXe;
                        xe.MaHang = SelectedHangXe.MaHang;
                        xe.SoLuong = SoLuong;
                        xe.GiaBan = GiaBan;

                        // Nếu có chọn ảnh mới và đường dẫn là file trên đĩa (không bắt đầu bằng /Images/)
                        if (!string.IsNullOrEmpty(HinhAnh) && !HinhAnh.StartsWith("/Images/"))
                        {
                            string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(HinhAnh);
                            string targetPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "XeMay");
                            if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);
                            string destFile = Path.Combine(targetPath, fileName);
                            if (File.Exists(HinhAnh) && !File.Exists(destFile)) File.Copy(HinhAnh, destFile, true);
                            xe.HinhAnh = "/Images/XeMay/" + fileName;
                        }

                        await db.SaveChangesAsync();
                        _ = RefreshAsync();
                    }
                }
            });

            // Lệnh xuất PDF
            ExportPdfCommand = new RelayCommand<object>((p) => ListXeMay != null && ListXeMay.Count > 0, (p) => {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF File (*.pdf)|*.pdf";
                saveFileDialog.FileName = "DanhSachXeMay_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".pdf";
                if (saveFileDialog.ShowDialog() == true)
                {
                    Services.PdfExportService.ExportDanhSachXeMayToPdf(saveFileDialog.FileName, ListXeMay);
                }
            });
        }

        async Task LoadDataAsync()
        {
            using (var db = new QLXeMayEntities())
            {
                var xeMays = await db.XeMays.Include(x => x.HangXe).ToListAsync();
                ListXeMay = new ObservableCollection<XeMay>(xeMays);
                var hangXes = await db.HangXes.ToListAsync();
                ListHangXe = new ObservableCollection<HangXe>(hangXes);
            }
        }

        // --- HÀM XỬ LÝ TÌM KIẾM ---
        async Task SearchDataAsync()
        {
            using (var db = new QLXeMayEntities())
            {
                var query = db.XeMays.Include(x => x.HangXe).AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    var keyword = SearchKeyword.ToLower();
                    query = query.Where(x => x.TenXe.ToLower().Contains(keyword));
                }

                if (SelectedPriceFilter == "Dưới 50.000.000")
                {
                    query = query.Where(x => x.GiaBan < 50000000);
                }
                else if (SelectedPriceFilter == "50.000.000 - 100.000.000")
                {
                    query = query.Where(x => x.GiaBan >= 50000000 && x.GiaBan <= 100000000);
                }
                else if (SelectedPriceFilter == "100.000.000 - 200.000.000")
                {
                    query = query.Where(x => x.GiaBan > 100000000 && x.GiaBan <= 200000000);
                }
                else if (SelectedPriceFilter == "Trên 200.000.000")
                {
                    query = query.Where(x => x.GiaBan > 200000000);
                }

                var filteredList = await query.ToListAsync();
                ListXeMay = new ObservableCollection<XeMay>(filteredList);
            }
        }

        async Task RefreshAsync() { TenXe = ""; SoLuong = 0; GiaBan = 0; HinhAnh = null; SelectedHangXe = null; SelectedItem = null; SearchKeyword = ""; SelectedPriceFilter = "Tất cả mức giá"; await LoadDataAsync(); }
    }
}