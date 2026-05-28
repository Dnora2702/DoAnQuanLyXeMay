USE master;
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'QLXeMay')
BEGIN
    ALTER DATABASE QLXeMay SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QLXeMay;
END
GO
-- 1. Tạo Database
CREATE DATABASE QLXeMay;
GO
USE QLXeMay;
GO

-- 2. Tạo bảng Tài Khoản ( Quyen 1 là Admin, 2 là Nhân viên)
CREATE TABLE TaiKhoan (
    TenDangNhap VARCHAR(50) PRIMARY KEY,
    MatKhau VARCHAR(255) NOT NULL,
    Quyen INT DEFAULT 2
);

-- 3. Tạo 4 bảng Danh mục (Phát sinh mã tự động )
CREATE TABLE NhanVien (
    MaNV INT IDENTITY(1,1) PRIMARY KEY, -- IDENTITY(1,1) giúp mã tự động tăng
    TenNV NVARCHAR(100) NOT NULL,
    SDT VARCHAR(15),
    DiaChi NVARCHAR(200),
    TenDangNhap VARCHAR(50) UNIQUE FOREIGN KEY REFERENCES TaiKhoan(TenDangNhap)
);

CREATE TABLE KhachHang (
    MaKH INT IDENTITY(1,1) PRIMARY KEY,
    TenKH NVARCHAR(100) NOT NULL,
    SDT VARCHAR(15),
    DiaChi NVARCHAR(200)
);

CREATE TABLE HangXe (
    MaHang INT IDENTITY(1,1) PRIMARY KEY,
    TenHang NVARCHAR(100) NOT NULL
);

CREATE TABLE NhaCungCap (
    MaNCC INT IDENTITY(1,1) PRIMARY KEY,
    TenNCC NVARCHAR(100) NOT NULL,
    SDT VARCHAR(15),
    DiaChi NVARCHAR(200)
);

-- 4. Tạo bảng Xe Máy 
CREATE TABLE XeMay (
    MaXe INT IDENTITY(1,1) PRIMARY KEY,
    TenXe NVARCHAR(100) NOT NULL,
    MaHang INT FOREIGN KEY REFERENCES HangXe(MaHang),
    MaNCC INT FOREIGN KEY REFERENCES NhaCungCap(MaNCC),
    SoLuong INT DEFAULT 0,
    GiaNhap DECIMAL(18,2) DEFAULT 0,
    GiaBan DECIMAL(18,2) DEFAULT 0,
    HinhAnh NVARCHAR(255) -- Chỉ lưu tên file (vd: wave-alpha.jpg)
);

-- 5. Tạo bảng Hóa Đơn (Dùng chung cho cả Nhập kho và Bán hàng)
CREATE TABLE HoaDon (
    MaHD INT IDENTITY(1,1) PRIMARY KEY,
    NgayLap DATETIME DEFAULT GETDATE(),
    LoaiHD NVARCHAR(20) NOT NULL, -- Sẽ lưu giá trị: 'Nhap' hoặc 'Ban'
    MaNV INT FOREIGN KEY REFERENCES NhanVien(MaNV),
    MaKH INT NULL FOREIGN KEY REFERENCES KhachHang(MaKH), -- Sẽ Null nếu là hóa đơn nhập
    MaNCC INT NULL FOREIGN KEY REFERENCES NhaCungCap(MaNCC), -- Sẽ Null nếu là hóa đơn bán
    TongTien DECIMAL(18,2) DEFAULT 0
);

-- 6. Tạo bảng Chi Tiết Hóa Đơn
CREATE TABLE ChiTietHoaDon (
    MaHD INT FOREIGN KEY REFERENCES HoaDon(MaHD),
    MaXe INT FOREIGN KEY REFERENCES XeMay(MaXe),
    SoLuong INT NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL,
    ThanhTien DECIMAL(18,2) NOT NULL,
    PRIMARY KEY (MaHD, MaXe) -- Khóa chính kết hợp
);

-- 7. Insert sẵn một tài khoản Admin để test đăng nhập
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, Quyen) 
VALUES ('admin', '123456', 1);

INSERT INTO NhanVien (TenNV, SDT, DiaChi, TenDangNhap)
VALUES (N'Quản trị viên', '0123456789', N'TP.HCM', 'admin');
GO


-- Dữ liệu các bảng --

-- 1. Insert dữ liệu mẫu cho Hãng Xe
INSERT INTO HangXe (TenHang) VALUES 
(N'Honda'),
(N'Yamaha'),
(N'Suzuki'),
(N'Piaggio');

-- 2. Insert dữ liệu mẫu cho Nhà Cung Cấp
INSERT INTO NhaCungCap (TenNCC, SDT, DiaChi) VALUES 
(N'Công ty TNHH Phát Tiến', '0901234567', N'Quận 1, TP.HCM'),
(N'Đại lý Yamaha Town', '0912345678', N'Quận 5, TP.HCM'),
(N'Công ty Suzuki Việt Nam', '0987654321', N'Khu công nghiệp Biên Hòa, Đồng Nai');

-- 3. Insert dữ liệu mẫu cho Khách Hàng
INSERT INTO KhachHang (TenKH, SDT, DiaChi) VALUES 
(N'Nguyễn Văn A', '0909111222', N'Bình Thạnh, TP.HCM'),
(N'Trần Thị B', '0933444555', N'Gò Vấp, TP.HCM'),
(N'Lê Văn C', '0977888999', N'Quận 10, TP.HCM');

-- 4. Insert dữ liệu mẫu cho Xe Máy 
-- (Lưu ý: MaHang và MaNCC phải khớp với ID sinh ra ở trên, mặc định là 1, 2, 3...)
INSERT INTO XeMay (TenXe, MaHang, MaNCC, SoLuong, GiaNhap, GiaBan, HinhAnh) VALUES 
(N'Honda Wave Alpha 110', 1, 1, 20, 15000000, 18000000, 'wave-alpha.jpg'),
(N'Honda Air Blade 125', 1, 1, 15, 38000000, 42000000, 'air-blade.jpg'),
(N'Yamaha Exciter 155 VVA', 2, 2, 10, 45000000, 50000000, 'exciter-155.jpg'),
(N'Suzuki Raider R150', 3, 3, 5, 48000000, 52000000, 'raider-150.jpg');
GO