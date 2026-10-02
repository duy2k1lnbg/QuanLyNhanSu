using Bu;
using Bu.DTO;
using Bu.CLASS_SYSTEM;
using DA;
using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace QLyNSu.FORM_NHANSU
{
    public partial class FrmPhuLucHopDong : DevExpress.XtraEditors.XtraForm
    {
        private TB_HOPDONG _hopDong;
        private string _tenNhanVien;
        private BindingList<DieuKhoanPhuLucItem> _items = new BindingList<DieuKhoanPhuLucItem>();
        private LUONG_HIEULUC _luongHieuLuc;

        public class DieuKhoanPhuLucItem : INotifyPropertyChanged
        {
            public int IdPc { get; set; }

            private string _khoanMuc;
            private string _cachHuong;
            private decimal _mucCu;
            private decimal _mucMoi;
            private string _ghiChu;

            public string KhoanMuc
            {
                get => _khoanMuc;
                set { _khoanMuc = value; OnPropertyChanged(nameof(KhoanMuc)); }
            }

            public string CachHuong
            {
                get => _cachHuong;
                set { _cachHuong = value; OnPropertyChanged(nameof(CachHuong)); }
            }

            public decimal MucCu
            {
                get => _mucCu;
                set { _mucCu = value; OnPropertyChanged(nameof(MucCu)); OnPropertyChanged(nameof(ChenhLech)); }
            }

            public decimal MucMoi
            {
                get => _mucMoi;
                set { _mucMoi = value; OnPropertyChanged(nameof(MucMoi)); OnPropertyChanged(nameof(ChenhLech)); }
            }

            public decimal ChenhLech => MucMoi - MucCu;

            public string GhiChu
            {
                get => _ghiChu;
                set { _ghiChu = value; OnPropertyChanged(nameof(GhiChu)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            }
        }

        public FrmPhuLucHopDong()
        {
            InitializeComponent();
            _luongHieuLuc = new LUONG_HIEULUC();
            this.Load += FrmPhuLucHopDong_Load;
        }

        public FrmPhuLucHopDong(TB_HOPDONG hopDong, string tenNhanVien = "") : this()
        {
            _hopDong = hopDong;
            _tenNhanVien = tenNhanVien;
        }

        private void FrmPhuLucHopDong_Load(object sender, EventArgs e)
        {
            if (_hopDong != null)
            {
                txtSoHD.Text = _hopDong.SOHD ?? string.Empty;
                txtNhanVien.Text = $"{_hopDong.MANV} - {_tenNhanVien}";
                txtNgayKyHD.Text = _hopDong.NGAYKY?.ToString("dd/MM/yyyy") ?? string.Empty;
                txtThoiHanHD.Text = _hopDong.THOIHAN ?? "1 Năm";

                dtNgayKy.DateTime = DateTime.Today;
                dtNgayHieuLuc.DateTime = DateTime.Today;
                txtSoPhuLuc.Text = $"{_hopDong.SOHD}/PL01";
                txtCanCu.Text = $"Căn cứ HĐLĐ số {_hopDong.SOHD} ký ngày {(_hopDong.NGAYKY.HasValue ? _hopDong.NGAYKY.Value.ToString("dd/MM/yyyy") : "...")}";
                txtNoiDung.Text = "Điều chỉnh mức lương theo năng lực và hiệu quả công việc";

                LoadDieuKhoanMucLuong(dtNgayHieuLuc.DateTime);
                dtNgayHieuLuc.EditValueChanged += DtNgayHieuLuc_EditValueChanged;
            }

            gcDieuKhoan.DataSource = _items;
            gvDieuKhoan.CellValueChanged += GvDieuKhoan_CellValueChanged;
            Functions.TranslationManager.Translate(this);
        }

        private void DtNgayHieuLuc_EditValueChanged(object sender, EventArgs e)
        {
            if (dtNgayHieuLuc.EditValue != null && _hopDong != null)
            {
                LoadDieuKhoanMucLuong(dtNgayHieuLuc.DateTime);
            }
        }

        private void LoadDieuKhoanMucLuong(DateTime effectiveDate)
        {
            _items.Clear();
            decimal manv = (decimal)(_hopDong.MANV ?? 0);
            decimal mucLuongHienTai = _hopDong.LUONG_THOA_THUAN ?? 0;

            try
            {
                var hieuLuc = _luongHieuLuc.GetLuongHieuLucTaiNgay(manv, effectiveDate);
                if (hieuLuc != null && hieuLuc.LUONG_THANG > 0)
                {
                    mucLuongHienTai = hieuLuc.LUONG_THANG;
                }
            }
            catch
            {
                // Fallback to contract agreed salary
            }

            _items.Add(new DieuKhoanPhuLucItem
            {
                IdPc = 0,
                KhoanMuc = "Lương thỏa thuận / chính",
                CachHuong = "Cố định theo tháng",
                MucCu = mucLuongHienTai,
                MucMoi = mucLuongHienTai,
                GhiChu = "Điều chỉnh mức lương chính"
            });

            // Load existing allowances for the employee
            try
            {
                using (var db = new MyEntities())
                {
                    int imanv = (int)manv;
                    var pcs = (from np in db.TB_NHANVIEN_PHUCAP
                               where np.MANV == imanv && np.DELETED_DATE == null
                               join pc in db.TB_PHUCAP on np.IDPC equals pc.IDPC
                               select new { np.IDPC, pc.TENPC, np.SOTIEN, np.CACH_TINH }).ToList();

                    if (pcs.Count > 0)
                    {
                        foreach (var p in pcs)
                        {
                            _items.Add(new DieuKhoanPhuLucItem
                            {
                                IdPc = (int)p.IDPC,
                                KhoanMuc = p.TENPC,
                                CachHuong = p.CACH_TINH ?? "Theo tháng",
                                MucCu = p.SOTIEN ?? 0,
                                MucMoi = p.SOTIEN ?? 0,
                                GhiChu = "Phụ cấp theo hợp đồng/phụ lục"
                            });
                        }
                    }
                    else
                    {
                        _items.Add(new DieuKhoanPhuLucItem
                        {
                            IdPc = 1,
                            KhoanMuc = "Phụ cấp trách nhiệm / công việc",
                            CachHuong = "Theo tháng",
                            MucCu = 0,
                            MucMoi = 0,
                            GhiChu = "Phụ cấp bổ sung (nếu có)"
                        });
                    }
                }
            }
            catch
            {
                _items.Add(new DieuKhoanPhuLucItem
                {
                    IdPc = 1,
                    KhoanMuc = "Phụ cấp trách nhiệm / công việc",
                    CachHuong = "Theo tháng",
                    MucCu = 0,
                    MucMoi = 0,
                    GhiChu = "Phụ cấp bổ sung (nếu có)"
                });
            }
        }

        private void GvDieuKhoan_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            gvDieuKhoan.RefreshData();
        }

        private void btnLuuPhuLuc_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            gvDieuKhoan.PostEditor();
            gvDieuKhoan.UpdateCurrentRow();

            if (_hopDong == null)
            {
                XtraMessageBox.Show("Không có thông tin hợp đồng gốc!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string soPhuLuc = txtSoPhuLuc.Text.Trim();
            if (string.IsNullOrEmpty(soPhuLuc))
            {
                XtraMessageBox.Show("Vui lòng nhập Số phụ lục!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSoPhuLuc.Focus();
                return;
            }

            if (dtNgayHieuLuc.EditValue == null)
            {
                XtraMessageBox.Show("Vui lòng chọn Ngày có hiệu lực của phụ lục!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtNgayHieuLuc.Focus();
                return;
            }

            var luongItem = _items.FirstOrDefault(x => x.IdPc == 0 || x.KhoanMuc.Contains("Lương"));
            decimal luongMoi = luongItem != null ? luongItem.MucMoi : 0;
            if (luongMoi <= 0)
            {
                XtraMessageBox.Show("Mức lương mới phải lớn hơn 0!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime ngayHieuLuc = dtNgayHieuLuc.DateTime;
            DateTime? denNgay = dtDenNgay.EditValue != null ? (DateTime?)dtDenNgay.DateTime : null;

            if (denNgay.HasValue && denNgay.Value < ngayHieuLuc)
            {
                XtraMessageBox.Show("Ngày kết thúc hiệu lực không được nhỏ hơn ngày bắt đầu hiệu lực!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dtDenNgay.Focus();
                return;
            }

            string noiDung = txtNoiDung.Text.Trim();
            string canCu = txtCanCu.Text.Trim();

            var dictPhuCap = new Dictionary<int, decimal>();
            foreach (var it in _items.Where(x => x.IdPc > 0))
            {
                dictPhuCap[it.IdPc] = it.MucMoi;
            }

            int userId = (int)(UserSession.CurrentUser?.IDUSER ?? 1);

            try
            {
                decimal manv = (decimal)(_hopDong.MANV ?? 0);
                _luongHieuLuc.RecordPhuLuc(
                    manv,
                    _hopDong.SOHD,
                    soPhuLuc,
                    ngayHieuLuc,
                    denNgay,
                    luongMoi,
                    noiDung,
                    canCu,
                    dictPhuCap,
                    userId
                );

                XtraMessageBox.Show($"Lưu phụ lục {soPhuLuc} và kích hoạt hiệu lực lương mới ({luongMoi:N0} VNĐ) từ ngày {ngayHieuLuc:dd/MM/yyyy} thành công!", 
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                string msg = ErrorHelper.ResolveUserFriendlyMessage(ex, "Lưu phụ lục hợp đồng");
                XtraMessageBox.Show(msg, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDong_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            this.Close();
        }
    }
}
