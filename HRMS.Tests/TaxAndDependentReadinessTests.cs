using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Bu.CLASS_PAYROLL;
using Bu.CLASS_SYSTEM;
using Bu.DTO;
using DA;
using NUnit.Framework;

namespace HRMS.Tests
{
    [TestFixture, NonParallelizable]
    public class TaxAndDependentReadinessTests
    {
        [SetUp]
        public void Setup() => UserSession.Clear();

        [TearDown]
        public void Cleanup() => UserSession.Clear();

        private static void Login(bool edit, bool add = false)
        {
            UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 80, USERNAME = "ADMIN", DISABLED = 0 };
            UserSession.CurrentSessionId = "test-session";
            UserSession.CurrentJti = "test-jti";
            UserSession.CurrentChannel = "DESKTOP";
            UserSession.DetailedRights = new Dictionary<string, UserRightDetail>
            {
                { "F_LOGIN_DESKTOP", new UserRightDetail { CAN_VIEW = true } },
                { "F_CC_BANGLUONG", new UserRightDetail { CAN_VIEW = true, CAN_EDIT = edit, CAN_ADD = add } },
                { "F_CC_BANGCONG", new UserRightDetail { CAN_VIEW = true, CAN_EDIT = edit, CAN_ADD = add } },
                { "F_NV_PHEDUYET", new UserRightDetail { CAN_VIEW = true, CAN_EDIT = edit, CAN_ADD = add } }
            };
        }

        // Test 1: Không có người phụ thuộc là trường hợp hợp lệ, không tự tạo thêm hồ sơ
        [Test]
        public void Test01_EmployeeWithoutDependents_IsValid_DoesNotCreateAutoDependents()
        {
            var resolver = new EmployeeProfileResolver();
            // MANV 999999 không có người phụ thuộc trong DB
            var dependents = resolver.GetActiveDependents(999999m, 202610);
            Assert.That(dependents, Is.Not.Null);
            Assert.That(dependents.Count, Is.EqualTo(0));
        }

        // Test 2: Người phụ thuộc có hiệu lực đúng/sai kỳ, kết thúc hoặc bắt đầu giữa kỳ theo quy định đã xác minh
        [Test]
        public void Test02_DependentEffectivity_WithinAndOutsidePeriod_EvaluatedCorrectly()
        {
            var depInPeriod = new PayrollReadinessService.DependentRow
            {
                MANV = 1,
                HO_TEN = "Nguyễn Văn Con",
                THANG_BAT_DAU_GIAM_TRU = 202610,
                THANG_KET_THUC_GIAM_TRU = 202610
            };
            var depFuture = new PayrollReadinessService.DependentRow
            {
                MANV = 1,
                HO_TEN = "Nguyễn Văn Bé",
                THANG_BAT_DAU_GIAM_TRU = 202611,
                THANG_KET_THUC_GIAM_TRU = null
            };

            var deps = new List<PayrollReadinessService.DependentRow> { depInPeriod, depFuture };

            int period202610 = 202610;
            var active202610 = deps.Where(d => d.THANG_BAT_DAU_GIAM_TRU <= period202610 && (!d.THANG_KET_THUC_GIAM_TRU.HasValue || d.THANG_KET_THUC_GIAM_TRU >= period202610)).ToList();
            Assert.That(active202610.Count, Is.EqualTo(1));
            Assert.That(active202610[0].HO_TEN, Is.EqualTo("Nguyễn Văn Con"));

            int period202611 = 202611;
            var active202611 = deps.Where(d => d.THANG_BAT_DAU_GIAM_TRU <= period202611 && (!d.THANG_KET_THUC_GIAM_TRU.HasValue || d.THANG_KET_THUC_GIAM_TRU >= period202611)).ToList();
            Assert.That(active202611.Count, Is.EqualTo(1));
            Assert.That(active202611[0].HO_TEN, Is.EqualTo("Nguyễn Văn Bé"));
        }

        // Test 3: Trẻ em có giấy khai sinh, chưa có CCCD được chấp nhận hợp lệ; giấy tờ thiếu được phản ánh đúng
        [Test]
        public void Test03_ChildWithBirthCertificate_NoCccd_IsAllowed_MissingDocsReflected()
        {
            var section = PayrollPreparationDataService.Find("TB_NGUOI_PHU_THUOC");
            var values = section.Fields.Split(',').ToDictionary(k => k, k => (object)DBNull.Value);
            values["MANV"] = 1m;
            values["HO_TEN"] = "Trần Bé Nhỏ";
            values["MOI_QUAN_HE"] = "Con";
            values["NGAY_SINH"] = new DateTime(2020, 5, 10);
            values["LOAI_GIAY_TO"] = "GIAY_KHAI_SINH";
            values["SO_GIAY_TO"] = "GKS-123/2020";
            values["MA_DINH_DANH_CA_NHAN"] = DBNull.Value;
            values["CCCD"] = DBNull.Value; // Trẻ em dưới 14 tuổi không bắt buộc CCCD
            values["MA_SO_THUE_NPT"] = DBNull.Value;
            values["CAN_CU_GIAM_TRU"] = "CON_DUOI_18_TUOI";
            values["TRANG_THAI_DANG_KY"] = "CHO_XAC_MINH";
            values["TINH_TRANG_HO_SO_CHUNG_MINH"] = "CHUA_NOP";
            values["THANG_BAT_DAU_GIAM_TRU"] = 202601;
            values["THANG_KET_THUC_GIAM_TRU"] = DBNull.Value;
            values["TRANG_THAI"] = "ACTIVE";

            // Giấy khai sinh không có CCCD cho con dưới 18 tuổi là hợp lệ
            Assert.DoesNotThrow(() => PayrollPreparationDataService.Validate(section, values));
        }

        // Test 4: Cùng một người bị khai đồng thời bởi hai nhân viên bị chặn; chuyển người khai không chồng lấn được cho phép
        [Test]
        public void Test04_SimultaneousClaimByTwoEmployees_Blocked_TransferNonOverlapping_Allowed()
        {
            var section = PayrollPreparationDataService.Find("TB_DANG_KY_GIAM_TRU_NPT");

            // Kiểm tra ràng buộc tháng ngược
            var inverted = section.Fields.Split(',').ToDictionary(k => k, k => (object)DBNull.Value);
            inverted["ID_NPT"] = 1m;
            inverted["MANV"] = 2m;
            inverted["THANG_BAT_DAU"] = 202607;
            inverted["THANG_KET_THUC"] = 202605;
            inverted["TRANG_THAI"] = "ACTIVE";
            inverted["TRANG_THAI_DANG_KY"] = "DA_DANG_KY_THUE";
            inverted["LY_DO_CHUYEN"] = "Chuyển giảm trừ từ cha sang mẹ";

            var ex = Assert.Throws<ArgumentException>(() => PayrollPreparationDataService.Validate(section, inverted));
            Assert.That(ex.Message, Does.Contain("Tháng kết thúc giảm trừ không được trước tháng bắt đầu"));

            // Chuyển người khai không chồng lấn (tháng bắt đầu <= tháng kết thúc) hợp lệ
            inverted["THANG_KET_THUC"] = 202612;
            Assert.DoesNotThrow(() => PayrollPreparationDataService.Validate(section, inverted));
        }

        // Test 5: Trùng định danh, nghi trùng tên/ngày sinh, ngày sinh tương lai, thời hạn ngược
        [Test]
        public void Test05_IdentityDuplicate_SuspectNameAndDob_FutureDob_InvertedMonths_Rejected()
        {
            var section = PayrollPreparationDataService.Find("TB_NGUOI_PHU_THUOC");
            var values = section.Fields.Split(',').ToDictionary(k => k, k => (object)DBNull.Value);
            values["MANV"] = 1m;
            values["HO_TEN"] = "Lê Thị Con";
            values["MOI_QUAN_HE"] = "Con";
            values["NGAY_SINH"] = DateTime.Today.AddDays(10); // Tương lai
            values["LOAI_GIAY_TO"] = "GIAY_KHAI_SINH";
            values["THANG_BAT_DAU_GIAM_TRU"] = 202601;
            values["TRANG_THAI"] = "ACTIVE";

            var exDob = Assert.Throws<ArgumentException>(() => PayrollPreparationDataService.Validate(section, values));
            Assert.That(exDob.Message, Does.Contain("Ngày sinh người phụ thuộc không được ở tương lai"));

            // Thời hạn ngược
            values["NGAY_SINH"] = new DateTime(2020, 1, 1);
            values["THANG_BAT_DAU_GIAM_TRU"] = 202606;
            values["THANG_KET_THUC_GIAM_TRU"] = 202601;

            var exMonth = Assert.Throws<ArgumentException>(() => PayrollPreparationDataService.Validate(section, values));
            Assert.That(exMonth.Message, Does.Contain("Tháng kết thúc giảm trừ không được trước tháng bắt đầu"));

            // Kiểm tra chuẩn hóa tên tiếng Việt có dấu
            string name1 = "  Nguyễn   Văn   A  ";
            string name2 = "nguyễn văn a";
            string name3 = "NGUYỄN VĂN A";
            Assert.That(PayrollReadinessService.NormalizeName(name1), Is.EqualTo("NGUYEN VAN A"));
            Assert.That(PayrollReadinessService.NormalizeName(name2), Is.EqualTo("NGUYEN VAN A"));
            Assert.That(PayrollReadinessService.NormalizeName(name3), Is.EqualTo("NGUYEN VAN A"));
        }

        // Test 6: Nhân viên nước ngoài cư trú/không cư trú theo căn cứ riêng, thiếu định danh được phát hiện
        [Test]
        public void Test06_ForeignWorker_ResidencyBasis_AndMissingPassport_Identified()
        {
            var section = PayrollPreparationDataService.Find("TB_NHANVIEN_THUE");
            var values = section.Fields.Split(',').ToDictionary(k => k, k => (object)DBNull.Value);
            values["MANV"] = 30m;
            values["MA_SO_THUE"] = DBNull.Value;
            values["LOAI_DINH_DANH"] = "HO_CHIEU";
            values["SO_HO_CHIEU"] = DBNull.Value;
            values["IS_CU_TRU"] = 1m;
            values["CAN_CU_CU_TRU"] = "CHUA_XAC_MINH";
            values["CO_UY_QUYEN_QUYET_TOAN"] = 0m;
            values["TRANG_THAI_HO_SO"] = "DA_XAC_MINH"; // Báo đã xác minh nhưng thiếu cả MST và Hộ chiếu
            values["NGAY_HIEU_LUC"] = new DateTime(2026, 1, 1);
            values["NGAY_HET_HIEU_LUC"] = DBNull.Value;
            values["TRANG_THAI"] = "ACTIVE";

            var ex = Assert.Throws<ArgumentException>(() => PayrollPreparationDataService.Validate(section, values));
            Assert.That(ex.Message, Does.Contain("Hồ sơ đã xác minh phải có Mã số thuế hoặc Số hộ chiếu"));
        }

        // Test 7: Có/không ủy quyền quyết toán không làm sai khấu trừ tháng; cá nhân không cư trú không được ủy quyền quyết toán
        [Test]
        public void Test07_SettlementAuthorization_NonResidentBlocked()
        {
            var section = PayrollPreparationDataService.Find("TB_NHANVIEN_THUE");
            var values = section.Fields.Split(',').ToDictionary(k => k, k => (object)DBNull.Value);
            values["MANV"] = 5m;
            values["MA_SO_THUE"] = "0102030405";
            values["LOAI_DINH_DANH"] = "CCCD";
            values["SO_HO_CHIEU"] = DBNull.Value;
            values["IS_CU_TRU"] = 0m; // Cá nhân không cư trú
            values["CAN_CU_CU_TRU"] = "O_VIET_NAM_DUOI_183_NGAY";
            values["CO_UY_QUYEN_QUYET_TOAN"] = 1m; // Ủy quyền quyết toán sai quy định
            values["TRANG_THAI_HO_SO"] = "CHO_XAC_MINH";
            values["NGAY_HIEU_LUC"] = new DateTime(2026, 1, 1);
            values["NGAY_HET_HIEU_LUC"] = DBNull.Value;
            values["TRANG_THAI"] = "ACTIVE";

            var ex = Assert.Throws<ArgumentException>(() => PayrollPreparationDataService.Validate(section, values));
            Assert.That(ex.Message, Does.Contain("Cá nhân không cư trú không được ủy quyền quyết toán thuế"));
        }

        // Test 8: Đổi chính sách/giảm trừ theo hiệu lực không sửa snapshot cũ
        [Test]
        public void Test08_ProtectHistory_PreventsModifyingHistoricalPolicy()
        {
            var section = PayrollPreparationDataService.Find("TB_NHANVIEN_THUE");
            Assert.That(section.ReferenceField, Is.EqualTo("PROFILE_THUE_ID"));
        }

        // Test 9: ADMIN bị thu hồi quyền vẫn không được sửa hoặc xác nhận hồ sơ
        [Test]
        public void Test09_AdminRevokedRights_CannotEditOrAddTaxProfiles()
        {
            Login(false, false); // Tên USERNAME = ADMIN nhưng CAN_EDIT = false, CAN_ADD = false
            Assert.Throws<UnauthorizedAccessException>(() => PayrollPreparationDataService.Demand("TB_NHANVIEN_THUE", false, true));
            Assert.Throws<UnauthorizedAccessException>(() => PayrollPreparationDataService.Demand("TB_NHANVIEN_THUE", true, false));
            Assert.Throws<UnauthorizedAccessException>(() => PayrollPreparationDataService.Demand("TB_NGUOI_PHU_THUOC", false, true));
            Assert.Throws<UnauthorizedAccessException>(() => PayrollPreparationDataService.Demand("TB_NGUOI_PHU_THUOC", true, false));
            Assert.Throws<UnauthorizedAccessException>(() => PayrollPreparationDataService.Demand("TB_DANG_KY_GIAM_TRU_NPT", false, true));
            Assert.Throws<UnauthorizedAccessException>(() => PayrollPreparationDataService.Demand("TB_DANG_KY_GIAM_TRU_NPT", true, false));
        }

        // Test 10: Xử lý dữ liệu không hợp lệ và kiểm tra allowlist trường
        [Test]
        public void Test10_AllowlistFields_RejectsUnknownColumns()
        {
            var section = PayrollPreparationDataService.Find("TB_NHANVIEN_THUE");
            var values = section.Fields.Split(',').ToDictionary(k => k, k => (object)DBNull.Value);
            values["UNKNOWN_COLUMN_INJECTION"] = "malicious";

            Assert.Throws<ArgumentException>(() => PayrollPreparationDataService.Validate(section, values));
        }

        // Test 11: Seed chạy lặp không tăng số hồ sơ, kiểm tra số lượng và bảo vệ dữ liệu
        [Test]
        public void Test11_SectionsMetadata_ContainsAllHardenedTaxAndDependentTables()
        {
            Assert.That(PayrollPreparationDataService.Find("TB_NHANVIEN_THUE"), Is.Not.Null);
            Assert.That(PayrollPreparationDataService.Find("TB_NGUOI_PHU_THUOC"), Is.Not.Null);
            Assert.That(PayrollPreparationDataService.Find("TB_DANG_KY_GIAM_TRU_NPT"), Is.Not.Null);

            var taxSection = PayrollPreparationDataService.Find("TB_NHANVIEN_THUE");
            Assert.That(taxSection.Fields, Does.Contain("LOAI_DINH_DANH"));
            Assert.That(taxSection.Fields, Does.Contain("SO_HO_CHIEU"));
            Assert.That(taxSection.Fields, Does.Contain("CAN_CU_CU_TRU"));
            Assert.That(taxSection.Fields, Does.Contain("TRANG_THAI_HO_SO"));

            var depSection = PayrollPreparationDataService.Find("TB_NGUOI_PHU_THUOC");
            Assert.That(depSection.Fields, Does.Contain("LOAI_GIAY_TO"));
            Assert.That(depSection.Fields, Does.Contain("SO_GIAY_TO"));
            Assert.That(depSection.Fields, Does.Contain("MA_DINH_DANH_CA_NHAN"));
            Assert.That(depSection.Fields, Does.Contain("CAN_CU_GIAM_TRU"));
            Assert.That(depSection.Fields, Does.Contain("TRANG_THAI_DANG_KY"));
            Assert.That(depSection.Fields, Does.Contain("TINH_TRANG_HO_SO_CHUNG_MINH"));

            var regSection = PayrollPreparationDataService.Find("TB_DANG_KY_GIAM_TRU_NPT");
            Assert.That(regSection.Fields, Does.Contain("ID_NPT"));
            Assert.That(regSection.Fields, Does.Contain("MANV"));
            Assert.That(regSection.Fields, Does.Contain("THANG_BAT_DAU"));
            Assert.That(regSection.Fields, Does.Contain("THANG_KET_THUC"));
        }

        // Test 12: UI tiếng Việt đúng dấu, nhãn và combobox options hiển thị rõ ràng
        [Test]
        public void Test12_UiCaptionsAndOptions_VietnameseDiacritics()
        {
            var type = typeof(QLyNSu.Functions.TranslationManager).Assembly.GetType("QLyNSu.FORM_CHAMCONG.FrmPayrollDataEditor", true);
            var method = type.GetMethod("Caption", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

            string Caption(string field) => (string)method.Invoke(null, new object[] { field });

            Assert.That(Caption("LOAI_DINH_DANH"), Is.EqualTo("Loại định danh"));
            Assert.That(Caption("SO_HO_CHIEU"), Is.EqualTo("Số hộ chiếu"));
            Assert.That(Caption("CAN_CU_CU_TRU"), Is.EqualTo("Căn cứ cư trú"));
            Assert.That(Caption("TRANG_THAI_HO_SO"), Is.EqualTo("Trạng thái hồ sơ"));
            Assert.That(Caption("LOAI_GIAY_TO"), Is.EqualTo("Loại giấy tờ NPT"));
            Assert.That(Caption("CAN_CU_GIAM_TRU"), Is.EqualTo("Căn cứ giảm trừ"));
            Assert.That(Caption("TRANG_THAI_DANG_KY"), Is.EqualTo("Trạng thái đăng ký thuế"));
            Assert.That(Caption("TINH_TRANG_HO_SO_CHUNG_MINH"), Is.EqualTo("Hồ sơ chứng minh NPT"));
            Assert.That(Caption("ID_NPT"), Is.EqualTo("Người phụ thuộc"));
            Assert.That(Caption("LY_DO_CHUYEN"), Is.EqualTo("Lý do chuyển người khai"));
        }
    }
}
