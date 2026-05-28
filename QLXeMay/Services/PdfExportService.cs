using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using FastReport;
using FastReport.Export.PdfSimple;
using FastReport.Utils;

namespace QLXeMay.Services
{
    public class PdfExportService
    {
        public static void ExportDanhSachXeMayToPdf(string filePath, IEnumerable<Models.XeMay> data)
        {
            try
            {
                // Ép load thư viện Microsoft.CSharp vào bộ nhớ để FastReport có thể dùng để biên dịch Script
                var _ = typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly;

                using (Report report = new Report())
                {
                    // Tạo trang
                    ReportPage page = new ReportPage();
                    page.PaperWidth = 210; // A4 width in mm
                    page.PaperHeight = 297; // A4 height in mm
                    report.Pages.Add(page);

                    // Thêm Tiêu đề (ReportTitleBand)
                    ReportTitleBand titleBand = new ReportTitleBand();
                    titleBand.Height = Units.Centimeters * 2;
                    page.ReportTitle = titleBand;

                    TextObject titleText = new TextObject();
                    titleText.Bounds = new System.Drawing.RectangleF(0, 0, Units.Centimeters * 19, Units.Centimeters * 1);
                    titleText.Text = "DANH SÁCH KHO XE MÁY";
                    titleText.HorzAlign = HorzAlign.Center;
                    titleText.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
                    titleBand.Objects.Add(titleText);

                    // Đăng ký dữ liệu
                    report.Dictionary.RegisterBusinessObject(data, "XeMayList", 3, true);

                    // Tạo Header
                    DataHeaderBand headerBand = new DataHeaderBand();
                    headerBand.Height = Units.Centimeters * 1;

                    TextObject headerTenXe = new TextObject();
                    headerTenXe.Bounds = new System.Drawing.RectangleF(0, 0, Units.Centimeters * 8, Units.Centimeters * 1);
                    headerTenXe.Text = "TÊN XE";
                    headerTenXe.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
                    headerTenXe.Border.Lines = BorderLines.All;
                    headerBand.Objects.Add(headerTenXe);

                    TextObject headerSoLuong = new TextObject();
                    headerSoLuong.Bounds = new System.Drawing.RectangleF(Units.Centimeters * 8, 0, Units.Centimeters * 4, Units.Centimeters * 1);
                    headerSoLuong.Text = "SỐ LƯỢNG";
                    headerSoLuong.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
                    headerSoLuong.Border.Lines = BorderLines.All;
                    headerBand.Objects.Add(headerSoLuong);

                    TextObject headerGiaBan = new TextObject();
                    headerGiaBan.Bounds = new System.Drawing.RectangleF(Units.Centimeters * 12, 0, Units.Centimeters * 7, Units.Centimeters * 1);
                    headerGiaBan.Text = "GIÁ BÁN (VNĐ)";
                    headerGiaBan.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
                    headerGiaBan.Border.Lines = BorderLines.All;
                    headerBand.Objects.Add(headerGiaBan);

                    // Data Band
                    DataBand dataBand = new DataBand();
                    dataBand.DataSource = report.GetDataSource("XeMayList");
                    dataBand.Height = Units.Centimeters * 1;
                    dataBand.Header = headerBand;
                    page.Bands.Add(dataBand);

                    TextObject dataTenXe = new TextObject();
                    dataTenXe.Bounds = new System.Drawing.RectangleF(0, 0, Units.Centimeters * 8, Units.Centimeters * 1);
                    dataTenXe.Text = "[XeMayList.TenXe]";
                    dataTenXe.Font = new System.Drawing.Font("Arial", 11);
                    dataTenXe.Border.Lines = BorderLines.All;
                    dataBand.Objects.Add(dataTenXe);

                    TextObject dataSoLuong = new TextObject();
                    dataSoLuong.Bounds = new System.Drawing.RectangleF(Units.Centimeters * 8, 0, Units.Centimeters * 4, Units.Centimeters * 1);
                    dataSoLuong.Text = "[XeMayList.SoLuong]";
                    dataSoLuong.Font = new System.Drawing.Font("Arial", 11);
                    dataSoLuong.Border.Lines = BorderLines.All;
                    dataBand.Objects.Add(dataSoLuong);

                    TextObject dataGiaBan = new TextObject();
                    dataGiaBan.Bounds = new System.Drawing.RectangleF(Units.Centimeters * 12, 0, Units.Centimeters * 7, Units.Centimeters * 1);
                    dataGiaBan.Text = "[XeMayList.GiaBan]";
                    dataGiaBan.Format = new FastReport.Format.NumberFormat() { UseLocale = true, DecimalDigits = 0 };
                    dataGiaBan.Font = new System.Drawing.Font("Arial", 11);
                    dataGiaBan.Border.Lines = BorderLines.All;
                    dataBand.Objects.Add(dataGiaBan);

                    // Chuẩn bị và Xuất
                    report.Prepare();

                    using (PDFSimpleExport export = new PDFSimpleExport())
                    {
                        report.Export(export, filePath);
                    }
                }
                MessageBox.Show("Xuất file PDF thành công!\n" + filePath, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất PDF: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void ExportHoaDonToPdf(string filePath, Models.KhachHang khachHang, IEnumerable<Models.ChiTietHoaDon> gioHang, decimal tongTien)
        {
            try
            {
                // Ép load thư viện Microsoft.CSharp vào bộ nhớ để FastReport có thể dùng để biên dịch Script
                var _ = typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly;

                using (Report report = new Report())
                {
                    ReportPage page = new ReportPage();
                    page.PaperWidth = 210;
                    page.PaperHeight = 297;
                    report.Pages.Add(page);

                    // Header Hóa đơn
                    ReportTitleBand titleBand = new ReportTitleBand();
                    titleBand.Height = Units.Centimeters * 5;
                    page.ReportTitle = titleBand;

                    TextObject titleText = new TextObject();
                    titleText.Bounds = new System.Drawing.RectangleF(0, 0, Units.Centimeters * 19, Units.Centimeters * 1);
                    titleText.Text = "HÓA ĐƠN BÁN HÀNG";
                    titleText.HorzAlign = HorzAlign.Center;
                    titleText.Font = new System.Drawing.Font("Arial", 16, System.Drawing.FontStyle.Bold);
                    titleBand.Objects.Add(titleText);

                    TextObject storeInfo = new TextObject();
                    storeInfo.Bounds = new System.Drawing.RectangleF(0, Units.Centimeters * 1, Units.Centimeters * 19, Units.Centimeters * 1);
                    storeInfo.Text = "CỬA HÀNG XE MÁY PRO - ĐỊA CHỈ: KHU VỰC HUIT, TP.HCM";
                    storeInfo.HorzAlign = HorzAlign.Center;
                    storeInfo.Font = new System.Drawing.Font("Arial", 11);
                    titleBand.Objects.Add(storeInfo);

                    TextObject khInfo = new TextObject();
                    khInfo.Bounds = new System.Drawing.RectangleF(0, Units.Centimeters * 3, Units.Centimeters * 19, Units.Centimeters * 1.5f);
                    khInfo.Text = $"Khách hàng: {khachHang.TenKH}\nSố điện thoại: {khachHang.SDT}\nNgày lập: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}";
                    khInfo.Font = new System.Drawing.Font("Arial", 11);
                    titleBand.Objects.Add(khInfo);

                    // Dữ liệu
                    report.Dictionary.RegisterBusinessObject(gioHang, "GioHang", 3, true);

                    // Bảng Header
                    DataHeaderBand headerBand = new DataHeaderBand();
                    headerBand.Height = Units.Centimeters * 1;

                    string[] headers = { "TÊN XE", "SL", "ĐƠN GIÁ", "THÀNH TIỀN" };
                    float[] widths = { 8f, 2f, 4f, 5f };
                    float currentX = 0;

                    for (int i = 0; i < headers.Length; i++)
                    {
                        TextObject headerObj = new TextObject();
                        headerObj.Bounds = new System.Drawing.RectangleF(Units.Centimeters * currentX, 0, Units.Centimeters * widths[i], Units.Centimeters * 1);
                        headerObj.Text = headers[i];
                        headerObj.Font = new System.Drawing.Font("Arial", 11, System.Drawing.FontStyle.Bold);
                        headerObj.Border.Lines = BorderLines.All;
                        headerObj.HorzAlign = HorzAlign.Center;
                        headerBand.Objects.Add(headerObj);
                        currentX += widths[i];
                    }

                    // Bảng chi tiết
                    DataBand dataBand = new DataBand();
                    dataBand.DataSource = report.GetDataSource("GioHang");
                    dataBand.Height = Units.Centimeters * 1;
                    dataBand.Header = headerBand;
                    page.Bands.Add(dataBand);

                    TextObject dataTen = new TextObject();
                    dataTen.Bounds = new System.Drawing.RectangleF(0, 0, Units.Centimeters * 8, Units.Centimeters * 1);
                    dataTen.Text = "[GioHang.XeMay.TenXe]";
                    dataTen.Font = new System.Drawing.Font("Arial", 11);
                    dataTen.Border.Lines = BorderLines.All;
                    dataBand.Objects.Add(dataTen);

                    TextObject dataSl = new TextObject();
                    dataSl.Bounds = new System.Drawing.RectangleF(Units.Centimeters * 8, 0, Units.Centimeters * 2, Units.Centimeters * 1);
                    dataSl.Text = "[GioHang.SoLuong]";
                    dataSl.Font = new System.Drawing.Font("Arial", 11);
                    dataSl.Border.Lines = BorderLines.All;
                    dataSl.HorzAlign = HorzAlign.Center;
                    dataBand.Objects.Add(dataSl);

                    TextObject dataDonGia = new TextObject();
                    dataDonGia.Bounds = new System.Drawing.RectangleF(Units.Centimeters * 10, 0, Units.Centimeters * 4, Units.Centimeters * 1);
                    dataDonGia.Text = "[GioHang.DonGia]";
                    dataDonGia.Format = new FastReport.Format.NumberFormat() { UseLocale = true, DecimalDigits = 0 };
                    dataDonGia.Font = new System.Drawing.Font("Arial", 11);
                    dataDonGia.Border.Lines = BorderLines.All;
                    dataDonGia.HorzAlign = HorzAlign.Right;
                    dataBand.Objects.Add(dataDonGia);

                    TextObject dataThanhTien = new TextObject();
                    dataThanhTien.Bounds = new System.Drawing.RectangleF(Units.Centimeters * 14, 0, Units.Centimeters * 5, Units.Centimeters * 1);
                    dataThanhTien.Text = "[GioHang.ThanhTien]";
                    dataThanhTien.Format = new FastReport.Format.NumberFormat() { UseLocale = true, DecimalDigits = 0 };
                    dataThanhTien.Font = new System.Drawing.Font("Arial", 11);
                    dataThanhTien.Border.Lines = BorderLines.All;
                    dataThanhTien.HorzAlign = HorzAlign.Right;
                    dataBand.Objects.Add(dataThanhTien);

                    // Tổng tiền
                    ReportSummaryBand summaryBand = new ReportSummaryBand();
                    summaryBand.Height = Units.Centimeters * 1.5f;
                    page.ReportSummary = summaryBand;

                    TextObject txtTongTien = new TextObject();
                    txtTongTien.Bounds = new System.Drawing.RectangleF(0, Units.Centimeters * 0.5f, Units.Centimeters * 19, Units.Centimeters * 1);
                    txtTongTien.Text = $"TỔNG CỘNG: {tongTien:N0} VNĐ";
                    txtTongTien.HorzAlign = HorzAlign.Right;
                    txtTongTien.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Bold);
                    summaryBand.Objects.Add(txtTongTien);

                    report.Prepare();

                    using (PDFSimpleExport export = new PDFSimpleExport())
                    {
                        report.Export(export, filePath);
                    }
                }
                MessageBox.Show("Xuất file Hóa đơn PDF thành công!\n" + filePath, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất PDF Hóa đơn: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
