using Bu.DTO;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu.FORM_CHAMCONG
{
    public partial class FrmChiTietLuong : DevExpress.XtraEditors.XtraForm
    {
        private BANGLUONG_DTO _bangLuongItem;

        public FrmChiTietLuong()
        {
            InitializeComponent();
        }

        public FrmChiTietLuong(BANGLUONG_DTO item) : this()
        {
            _bangLuongItem = item;
        }

        public void SetContext(BANGLUONG_DTO item)
        {
            _bangLuongItem = item;
            DisplayData();
        }

        private void FrmChiTietLuong_Load(object sender, EventArgs e)
        {
            if (_bangLuongItem != null)
            {
                DisplayData();
            }
            Functions.TranslationManager.Translate(this);
        }

        private void DisplayData()
        {
            if (_bangLuongItem == null) return;

            var item = _bangLuongItem;

            // 1. Header
            lblTitle.Text = $"CHI TIẾT BẢNG LƯƠNG - THÁNG {item.THANG}/{item.NAM}";
            lblThongTin.Text = $"Mã NV: {item.MANV} | Họ tên: {item.HOTEN} | Kỳ công: {item.MAKYCONG}";

            string trangThaiText;
            if (item.IS_LEGACY == 1)
            {
                trangThaiText = "Trạng thái: Bảng lương lịch sử (chỉ đọc)";
            }
            else if (!string.IsNullOrEmpty(item.TRANGTHAI_CHITRA))
            {
                trangThaiText = $"Trạng thái: {item.TRANGTHAI_CHITRA}";
            }
            else
            {
                trangThaiText = "Trạng thái: Bảng lương hiện hành (Desktop)";
            }
            lblTrangThai.Text = trangThaiText;

            decimal tongThuNhap = item.TONG_CONG ?? 0m;
            decimal khauTru = (item.TIEN_BHXH_TRICH ?? 0m) +
                              (item.TIEN_CONG_DOAN ?? 0m) +
                              (item.TIEN_TAMUNG ?? 0m) +
                              (item.THUE_TNCN ?? 0m) +
                              (item.KHOAN_TRU_KHAC ?? 0m);
            decimal thucLinh = item.THUC_LINH ?? 0m;

            lblTongThuNhap.Text = $"Tổng thu nhập: {tongThuNhap:n0} đ";
            lblTongKhauTru.Text = $"Tổng khấu trừ: {khauTru:n0} đ";
            lblThucLinh.Text = $"Thực lĩnh: {thucLinh:n0} đ";

            // 2. Tab 1: Khoản Mục Lương
            var listKhoanMuc = new List<ChiTietKhoanMucItem>();

            // Nhóm Tham chiếu
            decimal baseSalary = (item.DAILY_RATE != null && item.CONG_CHUAN != null) ? (item.DAILY_RATE.Value * item.CONG_CHUAN.Value) : 0m;
            listKhoanMuc.Add(new ChiTietKhoanMucItem
            {
                Nhom = "1. Tham chiếu",
                TenKhoan = "Mức lương tháng thỏa thuận / Mức gốc",
                DonVi = "đồng/tháng",
                SoLuong = "-",
                DonGia = baseSalary,
                ThanhTien = 0,
                GhiChu = "Mức quyền hưởng căn cứ theo Hợp đồng lao động"
            });
            listKhoanMuc.Add(new ChiTietKhoanMucItem
            {
                Nhom = "1. Tham chiếu",
                TenKhoan = "Ngày công chuẩn trong tháng",
                DonVi = "ngày",
                SoLuong = (item.CONG_CHUAN ?? 0).ToString("0.##"),
                DonGia = 0,
                ThanhTien = 0,
                GhiChu = "Số ngày làm việc tiêu chuẩn kỳ tính lương"
            });
            listKhoanMuc.Add(new ChiTietKhoanMucItem
            {
                Nhom = "1. Tham chiếu",
                TenKhoan = "Đơn giá công nhật (Daily Rate)",
                DonVi = "đồng/ngày",
                SoLuong = "1",
                DonGia = item.DAILY_RATE ?? 0m,
                ThanhTien = item.DAILY_RATE ?? 0m,
                GhiChu = "Đơn giá 1 ngày công = Lương thỏa thuận / Công chuẩn"
            });

            // Nhóm Thu nhập
            decimal luongNgay = (item.LUONG_CA_NGAY ?? 0m) > 0 ? (item.LUONG_CA_NGAY ?? 0m) : (item.LUONG_CONG_THUCTE ?? 0m);
            listKhoanMuc.Add(new ChiTietKhoanMucItem
            {
                Nhom = "2. Thu nhập",
                TenKhoan = "Lương ngày công thực tế (Ca ngày)",
                DonVi = "ngày",
                SoLuong = (item.CONG_LAMNGAY ?? item.CONG_THUCTE ?? 0m).ToString("0.##"),
                DonGia = item.DAILY_RATE ?? 0m,
                ThanhTien = luongNgay,
                GhiChu = "Lương tính theo số công thực tế làm việc ca ngày"
            });

            if ((item.CONG_LAMDEM ?? 0m) > 0 || (item.LUONG_CA_DEM ?? 0m) > 0)
            {
                listKhoanMuc.Add(new ChiTietKhoanMucItem
                {
                    Nhom = "2. Thu nhập",
                    TenKhoan = "Lương làm ca đêm (hệ số 130%)",
                    DonVi = "ngày",
                    SoLuong = (item.CONG_LAMDEM ?? 0m).ToString("0.##"),
                    DonGia = (item.DAILY_RATE ?? 0m) * 1.30m,
                    ThanhTien = item.LUONG_CA_DEM ?? 0m,
                    GhiChu = "Làm việc ca đêm được trả thêm 30% lương theo BLLĐ"
                });
            }

            decimal tienChuyenCan = item.TIEN_CHUYENCAN ?? 0m;
            decimal tienAnCa = item.TIEN_AN_CA ?? 0m;
            decimal phuCapTong = item.PHUCAP_CONG_THUCTE ?? 0m;
            decimal phuCapKhac = Math.Max(0m, phuCapTong - tienChuyenCan - tienAnCa);

            if (phuCapKhac > 0m)
            {
                listKhoanMuc.Add(new ChiTietKhoanMucItem
                {
                    Nhom = "2. Thu nhập",
                    TenKhoan = "Phụ cấp theo công / Phụ cấp khác",
                    DonVi = "ngày",
                    SoLuong = (item.CONG_THUCTE ?? 0m).ToString("0.##"),
                    DonGia = item.DAILY_ALLOWANCE ?? 0m,
                    ThanhTien = phuCapKhac,
                    GhiChu = "Các khoản phụ cấp theo công (đã tách riêng chuyên cần và ăn ca)"
                });
            }
            else if (phuCapTong > 0m && tienChuyenCan == 0m && tienAnCa == 0m)
            {
                listKhoanMuc.Add(new ChiTietKhoanMucItem
                {
                    Nhom = "2. Thu nhập",
                    TenKhoan = "Phụ cấp theo công thực tế",
                    DonVi = "ngày",
                    SoLuong = (item.CONG_THUCTE ?? 0m).ToString("0.##"),
                    DonGia = item.DAILY_ALLOWANCE ?? 0m,
                    ThanhTien = phuCapTong,
                    GhiChu = "Tổng các khoản phụ cấp trả theo ngày công thực tế"
                });
            }

            if ((item.TIEN_TANGCA ?? 0m) > 0)
            {
                listKhoanMuc.Add(new ChiTietKhoanMucItem
                {
                    Nhom = "2. Thu nhập",
                    TenKhoan = "Tiền làm thêm giờ (Tăng ca OT)",
                    DonVi = "giờ/đợt",
                    SoLuong = "-",
                    DonGia = 0,
                    ThanhTien = item.TIEN_TANGCA ?? 0m,
                    GhiChu = "Làm thêm ngoài giờ đã công bố theo hệ số luật định"
                });
            }

            if (tienChuyenCan > 0m)
            {
                listKhoanMuc.Add(new ChiTietKhoanMucItem
                {
                    Nhom = "2. Thu nhập",
                    TenKhoan = "Tiền thưởng chuyên cần",
                    DonVi = "tháng",
                    SoLuong = "1",
                    DonGia = tienChuyenCan,
                    ThanhTien = tienChuyenCan,
                    GhiChu = "Đạt tiêu chuẩn chuyên cần quy định trong tháng"
                });
            }

            if (tienAnCa > 0m)
            {
                listKhoanMuc.Add(new ChiTietKhoanMucItem
                {
                    Nhom = "2. Thu nhập",
                    TenKhoan = "Tiền ăn ca / Trợ cấp cơm trưa",
                    DonVi = "tháng",
                    SoLuong = "1",
                    DonGia = tienAnCa,
                    ThanhTien = tienAnCa,
                    GhiChu = "Hỗ trợ tiền ăn ca theo quy chế doanh nghiệp"
                });
            }

            if ((item.KHOAN_CONG_KHAC ?? 0m) > 0)
            {
                listKhoanMuc.Add(new ChiTietKhoanMucItem
                {
                    Nhom = "2. Thu nhập",
                    TenKhoan = "Khoản cộng phát sinh khác",
                    DonVi = "khoản",
                    SoLuong = "1",
                    DonGia = item.KHOAN_CONG_KHAC ?? 0m,
                    ThanhTien = item.KHOAN_CONG_KHAC ?? 0m,
                    GhiChu = "Các khoản phụ cấp hoặc thưởng phát sinh được duyệt"
                });
            }

            // Nhóm Khấu trừ
            listKhoanMuc.Add(new ChiTietKhoanMucItem
            {
                Nhom = "3. Khấu trừ",
                TenKhoan = "Bảo hiểm Xã hội (BHXH 8%)",
                DonVi = "%",
                SoLuong = "8%",
                DonGia = item.LUONG_BHXH ?? 0m,
                ThanhTien = item.TIEN_BHXH ?? 0m,
                GhiChu = "Trích nộp BHXH phần người lao động chịu"
            });

            listKhoanMuc.Add(new ChiTietKhoanMucItem
            {
                Nhom = "3. Khấu trừ",
                TenKhoan = "Bảo hiểm Y tế (BHYT 1.5%)",
                DonVi = "%",
                SoLuong = "1.5%",
                DonGia = item.LUONG_BHXH ?? 0m,
                ThanhTien = item.TIEN_BHYT ?? 0m,
                GhiChu = "Trích nộp BHYT phần người lao động chịu"
            });

            listKhoanMuc.Add(new ChiTietKhoanMucItem
            {
                Nhom = "3. Khấu trừ",
                TenKhoan = "Bảo hiểm Thất nghiệp (BHTN 1%)",
                DonVi = "%",
                SoLuong = "1%",
                DonGia = item.LUONG_BHXH ?? 0m,
                ThanhTien = item.TIEN_BHTN ?? 0m,
                GhiChu = "Trích nộp BHTN phần người lao động chịu"
            });

            listKhoanMuc.Add(new ChiTietKhoanMucItem
            {
                Nhom = "3. Khấu trừ",
                TenKhoan = "Đoàn phí Công đoàn (1%)",
                DonVi = "%",
                SoLuong = "1%",
                DonGia = item.LUONG_BHXH ?? 0m,
                ThanhTien = item.TIEN_CONG_DOAN ?? 0m,
                GhiChu = "Đoàn phí công đoàn người lao động đóng"
            });

            if ((item.TIEN_TAMUNG ?? 0m) > 0)
            {
                listKhoanMuc.Add(new ChiTietKhoanMucItem
                {
                    Nhom = "3. Khấu trừ",
                    TenKhoan = "Tạm ứng trong kỳ",
                    DonVi = "lần",
                    SoLuong = "1",
                    DonGia = item.TIEN_TAMUNG ?? 0m,
                    ThanhTien = item.TIEN_TAMUNG ?? 0m,
                    GhiChu = "Khấu trừ phiếu tạm ứng đã thanh toán trong kỳ"
                });
            }

            if ((item.THUE_TNCN ?? 0m) > 0)
            {
                listKhoanMuc.Add(new ChiTietKhoanMucItem
                {
                    Nhom = "3. Khấu trừ",
                    TenKhoan = "Thuế thu nhập cá nhân (TNCN)",
                    DonVi = "kỳ",
                    SoLuong = "-",
                    DonGia = 0,
                    ThanhTien = item.THUE_TNCN ?? 0m,
                    GhiChu = "Tạm khấu trừ theo biểu thuế lũy tiến từng phần"
                });
            }

            if ((item.KHOAN_TRU_KHAC ?? 0m) > 0)
            {
                listKhoanMuc.Add(new ChiTietKhoanMucItem
                {
                    Nhom = "3. Khấu trừ",
                    TenKhoan = "Khoản trừ khác",
                    DonVi = "khoản",
                    SoLuong = "1",
                    DonGia = item.KHOAN_TRU_KHAC ?? 0m,
                    ThanhTien = item.KHOAN_TRU_KHAC ?? 0m,
                    GhiChu = "Các khoản khấu trừ phát sinh khác"
                });
            }

            gcKhoanMuc.DataSource = listKhoanMuc;
            gvKhoanMuc.ExpandAllGroups();

            // 3. Tab 2: Căn Cứ & Nguồn Tính
            List<CanCuItem> listCanCu;
            if (item.IS_LEGACY == 1)
            {
                listCanCu = new List<CanCuItem>
                {
                    new CanCuItem
                    {
                        ThanhPhan = "Toàn bộ bảng lương",
                        Nguon = "Bảng lương lịch sử kế thừa",
                        GiaTri = $"Thực lĩnh: {item.THUC_LINH ?? 0:n0} đ",
                        TrangThai = "Dữ liệu cũ, chưa có thông tin nguồn đầy đủ"
                    }
                };
            }
            else
            {
                listCanCu = new List<CanCuItem>
                {
                    new CanCuItem
                    {
                        ThanhPhan = "Lương thỏa thuận / Mức gốc",
                        Nguon = "Hợp đồng lao động & TB_LUONG_HIEULUC",
                        GiaTri = $"{baseSalary:n0} đ/tháng",
                        TrangThai = "Hợp đồng có hiệu lực trong kỳ"
                    },
                    new CanCuItem
                    {
                        ThanhPhan = "Số ngày công chuẩn",
                        Nguon = "Danh mục kỳ công (TB_KYCONG)",
                        GiaTri = $"{item.CONG_CHUAN ?? 0} ngày",
                        TrangThai = "Kỳ công đã công bố"
                    },
                    new CanCuItem
                    {
                        ThanhPhan = "Công thực tế tích lũy",
                        Nguon = "Chấm công chi tiết (TB_BANGCONG_CHITIET)",
                        GiaTri = $"{item.CONG_THUCTE ?? 0} ngày (Ngày: {item.CONG_LAMNGAY ?? item.CONG_THUCTE ?? 0}, Đêm: {item.CONG_LAMDEM ?? 0})",
                        TrangThai = "Dữ liệu chấm công đã chốt"
                    },
                    new CanCuItem
                    {
                        ThanhPhan = "Phụ cấp cố định & theo ngày",
                        Nguon = "Phụ cấp nhân viên (TB_NHANVIEN_PHUCAP)",
                        GiaTri = $"{item.PHUCAP_CONG_THUCTE ?? 0:n0} đ",
                        TrangThai = "Đã đối chiếu theo ngày công"
                    },
                    new CanCuItem
                    {
                        ThanhPhan = "Làm thêm giờ (Tăng ca OT)",
                        Nguon = "Bảng tăng ca & phân đoạn (TB_TANGCA)",
                        GiaTri = $"{item.TIEN_TANGCA ?? 0:n0} đ",
                        TrangThai = "Giờ OT đã duyệt và công bố"
                    },
                    new CanCuItem
                    {
                        ThanhPhan = "Tạm ứng",
                        Nguon = "Bảng tạm ứng lương (TB_UNGLUONG)",
                        GiaTri = $"{item.TIEN_TAMUNG ?? 0:n0} đ",
                        TrangThai = "Phiếu tạm ứng được duyệt"
                    },
                    new CanCuItem
                    {
                        ThanhPhan = "Chính sách Bảo hiểm & Thuế",
                        Nguon = "Căn cứ pháp lý BHXH & Thuế TNCN",
                        GiaTri = $"Lương đóng BH: {item.LUONG_BHXH ?? 0:n0} đ",
                        TrangThai = "Quy định hiện hành"
                    }
                };
            }
            gcCanCu.DataSource = listCanCu;

            // 4. Tab 3: Bảo Hiểm & Thuế
            decimal luongBH = item.LUONG_BHXH ?? 0m;
            decimal bhxh_nld = item.TIEN_BHXH ?? 0m;
            decimal bhxh_nsdld = item.TIEN_BHXH_NSDLD ?? (luongBH * 0.175m);

            decimal bhyt_nld = item.TIEN_BHYT ?? 0m;
            decimal bhyt_nsdld = item.TIEN_BHYT_NSDLD ?? (luongBH * 0.03m);

            decimal bhtn_nld = item.TIEN_BHTN ?? 0m;
            decimal bhtn_nsdld = item.TIEN_BHTN_NSDLD ?? (luongBH * 0.01m);

            decimal cd_nld = item.TIEN_CONG_DOAN ?? 0m;
            decimal cd_nsdld = item.TIEN_KINH_PHI_CD_NSDLD ?? (luongBH * 0.02m);

            var listBaoHiem = new List<BaoHiemThueItem>
            {
                new BaoHiemThueItem
                {
                    ChiTieu = "Bảo hiểm Xã hội (BHXH)",
                    TyLe = "NLĐ: 8% | Doanh nghiệp: 17.5%",
                    TienNLD = bhxh_nld,
                    TienNSDLD = bhxh_nsdld,
                    GhiChu = "Trần đóng theo mức tham chiếu lương cơ sở"
                },
                new BaoHiemThueItem
                {
                    ChiTieu = "Bảo hiểm Y tế (BHYT)",
                    TyLe = "NLĐ: 1.5% | Doanh nghiệp: 3.0%",
                    TienNLD = bhyt_nld,
                    TienNSDLD = bhyt_nsdld,
                    GhiChu = "Luật Bảo hiểm Y tế"
                },
                new BaoHiemThueItem
                {
                    ChiTieu = "Bảo hiểm Thất nghiệp (BHTN)",
                    TyLe = "NLĐ: 1.0% | Doanh nghiệp: 1.0%",
                    TienNLD = bhtn_nld,
                    TienNSDLD = bhtn_nsdld,
                    GhiChu = "Luật Việc làm - Tối đa 20 tháng lương tối thiểu vùng"
                },
                new BaoHiemThueItem
                {
                    ChiTieu = "Kinh phí & Đoàn phí Công đoàn",
                    TyLe = "Đoàn viên: 1% | Doanh nghiệp: 2%",
                    TienNLD = cd_nld,
                    TienNSDLD = cd_nsdld,
                    GhiChu = "Luật Công đoàn"
                },
                new BaoHiemThueItem
                {
                    ChiTieu = "Giảm trừ bản thân (Thuế TNCN)",
                    TyLe = "11.000.000 đ/tháng",
                    TienNLD = item.GIAM_TRU_BAN_THAN ?? 11000000m,
                    TienNSDLD = 0,
                    GhiChu = "Nghị quyết 954/2020/UBTVQH14"
                },
                new BaoHiemThueItem
                {
                    ChiTieu = $"Giảm trừ người phụ thuộc ({item.SO_NGUOI_PHU_THUOC ?? 0} người)",
                    TyLe = "4.400.000 đ/người/tháng",
                    TienNLD = item.GIAM_TRU_PHU_THUOC ?? ((item.SO_NGUOI_PHU_THUOC ?? 0) * 4400000m),
                    TienNSDLD = 0,
                    GhiChu = "Hồ sơ người phụ thuộc đã đăng ký hợp lệ"
                },
                new BaoHiemThueItem
                {
                    ChiTieu = "Thu nhập tính thuế & Thuế TNCN",
                    TyLe = "Biểu thuế lũy tiến từng phần",
                    TienNLD = item.THUE_TNCN ?? 0m,
                    TienNSDLD = item.THU_NHAP_TINH_THUE ?? 0m,
                    GhiChu = "Thuế TNCN tạm khấu trừ trong kỳ lương"
                }
            };
            gcBaoHiemThue.DataSource = listBaoHiem;
        }

        private void btnIn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (gcKhoanMuc.IsPrintingAvailable)
            {
                gcKhoanMuc.ShowPrintPreview();
            }
            else
            {
                XtraMessageBox.Show("Chức năng in chưa sẵn sàng!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDieuChinh_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            if (_bangLuongItem != null)
            {
                using (var frm = new FrmDieuChinhLuong(_bangLuongItem))
                {
                    if (frm.ShowDialog(this) == DialogResult.OK)
                    {
                        // Refresh data after adjustment from database
                        ReloadDataFromDatabase();
                        DisplayData();
                    }
                }
            }
        }

        private void ReloadDataFromDatabase()
        {
            if (_bangLuongItem == null || _bangLuongItem.IDBL <= 0) return;
            try
            {
                using (var db = new DA.MyEntities())
                {
                    var bl = db.TB_BANGLUONG.FirstOrDefault(x => x.IDBL == _bangLuongItem.IDBL);
                    if (bl != null)
                    {
                        _bangLuongItem.LUONG_CONG_THUCTE = bl.LUONG_CONG_THUCTE;
                        _bangLuongItem.PHUCAP_CONG_THUCTE = bl.PHUCAP_CONG_THUCTE;
                        _bangLuongItem.TIEN_TANGCA = bl.TIEN_TANGCA;
                        _bangLuongItem.TIEN_CHUYENCAN = bl.TIEN_CHUYENCAN;
                        _bangLuongItem.TIEN_AN_CA = bl.TIEN_AN_CA;
                        _bangLuongItem.KHOAN_CONG_KHAC = bl.KHOAN_CONG_KHAC;
                        _bangLuongItem.TONG_CONG = bl.TONG_CONG;
                        _bangLuongItem.TIEN_BHXH_TRICH = bl.TIEN_BHXH_TRICH;
                        _bangLuongItem.TIEN_CONG_DOAN = bl.TIEN_CONG_DOAN;
                        _bangLuongItem.TIEN_TAMUNG = bl.TIEN_TAMUNG;
                        _bangLuongItem.THUE_TNCN = bl.THUE_TNCN;
                        _bangLuongItem.KHOAN_TRU_KHAC = bl.KHOAN_TRU_KHAC;
                        _bangLuongItem.HOAN_THUE = bl.HOAN_THUE;
                        _bangLuongItem.THUC_LINH = bl.THUC_LINH;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[FrmChiTietLuong] ReloadDataFromDatabase failed: {ex.Message}");
            }
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }

        private class ChiTietKhoanMucItem
        {
            public string Nhom { get; set; }
            public string TenKhoan { get; set; }
            public string DonVi { get; set; }
            public string SoLuong { get; set; }
            public decimal DonGia { get; set; }
            public decimal ThanhTien { get; set; }
            public string GhiChu { get; set; }
        }

        private class CanCuItem
        {
            public string ThanhPhan { get; set; }
            public string Nguon { get; set; }
            public string GiaTri { get; set; }
            public string TrangThai { get; set; }
        }

        private class BaoHiemThueItem
        {
            public string ChiTieu { get; set; }
            public string TyLe { get; set; }
            public decimal TienNLD { get; set; }
            public decimal TienNSDLD { get; set; }
            public string GhiChu { get; set; }
        }
    }
}
