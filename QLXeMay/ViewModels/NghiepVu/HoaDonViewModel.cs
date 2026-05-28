using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Data.Entity;
using QLXeMay.Models;
using QLXeMay.Services;

namespace QLXeMay.ViewModels.NghiepVu
{
    public class HoaDonViewModel : BaseViewModel
    {
        // --- 1. KHAI BÁO CÁC DANH SÁCH ---
        private ObservableCollection<KhachHang> _listKH;
        public ObservableCollection<KhachHang> ListKhachHang { get => _listKH; set { _listKH = value; OnPropertyChanged(); } }

        private ObservableCollection<XeMay> _listXe;
        public ObservableCollection<XeMay> ListXeMay { get => _listXe; set { _listXe = value; OnPropertyChanged(); } }

        // --- 2. KHAI BÁO CÁC THUỘC TÍNH RÀNG BUỘC (BINDING) ---
        private KhachHang _selectedKH;
        public KhachHang SelectedKhachHang { get => _selectedKH; set { _selectedKH = value; OnPropertyChanged(); } }

        private XeMay _selectedXe;
        public XeMay SelectedXe { get => _selectedXe; set { _selectedXe = value; OnPropertyChanged(); } }

        private int _soLuongBan = 1;
        public int SoLuongBan { get => _soLuongBan; set { _soLuongBan = value; OnPropertyChanged(); } }

        private ObservableCollection<ChiTietHoaDon> _gioHang;
        public ObservableCollection<ChiTietHoaDon> GioHang { get => _gioHang; set { _gioHang = value; OnPropertyChanged(); } }

        private decimal _tongTien;
        public decimal TongTien { get => _tongTien; set { _tongTien = value; OnPropertyChanged(); } }

        // --- 3. KHAI BÁO CÁC LỆNH (COMMANDS) ---
        public RelayCommand<object> AddToCartCommand { get; set; }
        public RelayCommand<object> ThanhToanCommand { get; set; }
        public RelayCommand<object> XuatHoaDonCommand { get; set; }

        public HoaDonViewModel()
        {
            GioHang = new ObservableCollection<ChiTietHoaDon>();
            _ = LoadDataAsync();

            // ==========================================
            // LỆNH 1: THÊM VÀO GIỎ HÀNG
            // ==========================================
            AddToCartCommand = new RelayCommand<object>(
                (p) => SelectedXe != null && SoLuongBan > 0 && SoLuongBan <= SelectedXe.SoLuong,
                (p) => {
                    decimal giaBan = SelectedXe.GiaBan ?? 0m;

                    var existItem = GioHang.FirstOrDefault(x => x.MaXe == SelectedXe.MaXe);
                    if (existItem != null)
                    {
                        int currentQty = existItem.SoLuong;
                        int newQty = currentQty + SoLuongBan;

                        existItem.SoLuong = newQty;
                        existItem.ThanhTien = newQty * giaBan;
                    }
                    else
                    {
                        GioHang.Add(new ChiTietHoaDon
                        {
                            MaXe = SelectedXe.MaXe,
                            XeMay = SelectedXe,
                            SoLuong = SoLuongBan,
                            DonGia = giaBan,
                            ThanhTien = SoLuongBan * giaBan
                        });
                    }
                    TinhTongTien();
                }
            );

            // ==========================================
            // LỆNH 2: THANH TOÁN & LƯU DATABASE
            // ==========================================
            ThanhToanCommand = new RelayCommand<object>(
              (p) => SelectedKhachHang != null && GioHang.Count > 0,
              async (p) => await ThanhToanAsync()
            );

            // ==========================================
            // LỆNH 3: XUẤT HÓA ĐƠN RA FILE PDF
            // ==========================================
            XuatHoaDonCommand = new RelayCommand<object>(
                (p) => SelectedKhachHang != null && GioHang.Count > 0,
                (p) => {
                    var sfd = new Microsoft.Win32.SaveFileDialog();
                    sfd.Filter = "PDF file (*.pdf)|*.pdf";
                    sfd.FileName = "HoaDon_" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".pdf";

                    if (sfd.ShowDialog() == true)
                    {
                        PdfExportService.ExportHoaDonToPdf(sfd.FileName, SelectedKhachHang, GioHang, TongTien);
                    }
                }
            );
        }

        private async Task ThanhToanAsync()
        {
            using (var db = new QLXeMayEntities())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var hd = new HoaDon()
                        {
                            MaKH = SelectedKhachHang.MaKH,
                            NgayLap = DateTime.Now,
                            TongTien = TongTien,
                            LoaiHD = "Bán lẻ"
                        };
                        db.HoaDons.Add(hd);
                        await db.SaveChangesAsync(); // Lưu để SQL sinh MaHD trước khi thêm chi tiết

                        foreach (var item in GioHang)
                        {
                            db.ChiTietHoaDons.Add(new ChiTietHoaDon()
                            {
                                MaHD = hd.MaHD, // Bây giờ hd.MaHD đã có giá trị thực
                                MaXe = item.MaXe,
                                SoLuong = item.SoLuong,
                                DonGia = item.DonGia,
                                ThanhTien = item.ThanhTien
                            });

                            var xeInDb = await db.XeMays.FindAsync(item.MaXe);
                            if (xeInDb != null)
                            {
                                if (xeInDb.SoLuong < item.SoLuong)
                                {
                                    throw new Exception($"Xe {xeInDb.TenXe} không đủ số lượng trong kho.");
                                }
                                xeInDb.SoLuong -= item.SoLuong;
                            }
                        }

                        await db.SaveChangesAsync();
                        transaction.Commit();

                        DialogService.ShowSuccess("Thanh toán thành công! Đã trừ số lượng trong kho.", "Thông báo");

                        // Làm mới sau khi bán
                        GioHang.Clear();
                        SelectedKhachHang = null;
                        SelectedXe = null;
                        SoLuongBan = 1;
                        TinhTongTien();
                        await LoadDataAsync();
                    }
                    catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                    {
                        transaction.Rollback();
                        string errorMsgs = "";
                        foreach (var validationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                errorMsgs += $"Cột bị lỗi: {validationError.PropertyName} - Chi tiết: {validationError.ErrorMessage}\n";
                            }
                        }
                        DialogService.ShowError("Bạn đang thiếu hoặc sai dữ liệu ở: \n\n" + errorMsgs, "Lỗi Validation SQL");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        DialogService.ShowError("Lỗi hệ thống: " + ex.Message, "Lỗi");
                    }
                }
            }
        }



        // --- 4. CÁC HÀM XỬ LÝ PHỤ TRỢ ---
        async Task LoadDataAsync()
        {
            using (var db = new QLXeMayEntities())
            {
                var khachHangs = await db.KhachHangs.ToListAsync();
                ListKhachHang = new ObservableCollection<KhachHang>(khachHangs);
                
                var xeMays = await db.XeMays.Where(x => x.SoLuong > 0).ToListAsync();
                ListXeMay = new ObservableCollection<XeMay>(xeMays);
            }
        }

        void TinhTongTien()
        {
            TongTien = GioHang.Sum(x => x.ThanhTien);
        }
    }
}