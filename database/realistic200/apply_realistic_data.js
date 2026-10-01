const fs = require('fs');
const path = require('path');
const { execSync } = require('child_process');

console.log("=== APPLAYING REALISTIC 200 EMPLOYEE DATASET (DML ONLY) ===");

const manifestPath = path.join(__dirname, 'realistic200_manifest.json');
if (!fs.existsSync(manifestPath)) {
  console.error("FATAL: realistic200_manifest.json not found!");
  process.exit(1);
}
const employees = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));
if (employees.length !== 200) {
  console.error(`FATAL: Expected 200 employees, found ${employees.length}`);
  process.exit(1);
}

// 23 Departments
const DEPARTMENTS = [
  { id: 1, name: "Bộ phận kinh doanh" },
  { id: 2, name: "Bộ phận kỹ thuật" },
  { id: 3, name: "Bộ phận sản xuất" },
  { id: 4, name: "Bộ phận hành chính văn phòng" },
  { id: 5, name: "Bộ phận marketing" },
  { id: 23, name: "Bộ phận Nhân sự" },
  { id: 24, name: "Bộ phận Kế toán" },
  { id: 25, name: "Bộ phận IT" },
  { id: 26, name: "Bộ phận Sản xuất" },
  { id: 27, name: "Bộ phận Phát triển sản phẩm" },
  { id: 28, name: "Bộ phận Dịch vụ khách hàng" },
  { id: 29, name: "Bộ phận Quản lý dự án" },
  { id: 30, name: "Bộ phận Logistics" },
  { id: 31, name: "Bộ phận Thiết kế" },
  { id: 32, name: "Bộ phận Quảng cáo" },
  { id: 33, name: "Bộ phận Tài chính" },
  { id: 34, name: "Bộ phận Nghiên cứu và Phát triển" },
  { id: 35, name: "Bộ phận Đào tạo và Phát triển" },
  { id: 36, name: "Bộ phận Bán hàng" },
  { id: 37, name: "Bộ phận Vận hành" },
  { id: 38, name: "Bộ phận Hỗ trợ kỹ thuật" },
  { id: 39, name: "Bộ phận Chăm sóc khách hàng" },
  { id: 40, name: "Bộ phận Pháp lý" }
];

// 20 Divisions (TB_PHONGBAN)
const DIVISIONS = [
  { id: 1, name: "Phòng Hành chính" },
  { id: 22, name: "Phòng Kinh doanh" },
  { id: 23, name: "Phòng Marketing" },
  { id: 24, name: "Phòng Nhân sự" },
  { id: 25, name: "Phòng Kế toán" },
  { id: 26, name: "Phòng IT" },
  { id: 27, name: "Phòng Sản xuất" },
  { id: 28, name: "Phòng Phát triển sản phẩm" },
  { id: 29, name: "Phòng Dịch vụ khách hàng" },
  { id: 30, name: "Phòng Logistics" },
  { id: 31, name: "Phòng Thiết kế" },
  { id: 32, name: "Phòng Quảng cáo" },
  { id: 33, name: "Phòng Tài chính" },
  { id: 34, name: "Phòng Nghiên cứu và Phát triển" },
  { id: 35, name: "Phòng Đào tạo và Phát triển" },
  { id: 36, name: "Phòng Bán hàng" },
  { id: 37, name: "Phòng Vận hành" },
  { id: 38, name: "Phòng Hỗ trợ kỹ thuật" },
  { id: 39, name: "Phòng Chăm sóc khách hàng" },
  { id: 40, name: "Phòng Pháp lý" }
];

// 35 Positions (TB_CHUCVU)
const POSITIONS = [
  { id: 1, name: "Tổng Giám Đốc" },
  { id: 21, name: "Giám đốc" },
  { id: 22, name: "Phó giám đốc" },
  { id: 23, name: "Trưởng phòng" },
  { id: 24, name: "Phó phòng" },
  { id: 25, name: "Nhân viên" },
  { id: 26, name: "Kế toán" },
  { id: 27, name: "Lễ tân" },
  { id: 28, name: "Marketing" },
  { id: 29, name: "Nhân sự" },
  { id: 30, name: "IT Support" },
  { id: 31, name: "Trưởng bộ phận" },
  { id: 32, name: "Chuyên viên" },
  { id: 33, name: "Nhân viên kinh doanh" },
  { id: 34, name: "Nhân viên kỹ thuật" },
  { id: 35, name: "Quản lý dự án" },
  { id: 36, name: "Giám sát sản xuất" },
  { id: 37, name: "Nhân viên mua hàng" },
  { id: 38, name: "Trưởng phòng marketing" },
  { id: 39, name: "Trưởng phòng nhân sự" },
  { id: 40, name: "Trưởng phòng IT" },
  { id: 41, name: "Trưởng phòng bán hàng" },
  { id: 42, name: "Chuyên viên tư vấn" },
  { id: 43, name: "Chuyên viên phân tích" },
  { id: 44, name: "Nhân viên dịch vụ khách hàng" },
  { id: 45, name: "Nhân viên sản xuất" },
  { id: 46, name: "Nhân viên kho" },
  { id: 47, name: "Nhân viên chăm sóc khách hàng" },
  { id: 48, name: "Nhân viên lập trình" },
  { id: 49, name: "Trưởng phòng phát triển sản phẩm" },
  { id: 50, name: "Chuyên viên thị trường" },
  { id: 51, name: "Nhân viên thiết kế" },
  { id: 52, name: "Quản lý chất lượng" },
  { id: 53, name: "Nhân viên quảng cáo" },
  { id: 54, name: "Nhân viên logistics" }
];

// 13 Allowances (TB_PHUCAP)
const ALLOWANCES = [
  { id: 1, name: "Phụ cấp nhà ở" },
  { id: 2, name: "Phụ cấp đi lại" },
  { id: 3, name: "Phụ cấp gia đình" },
  { id: 4, name: "Phụ cấp người phụ thuộc" },
  { id: 5, name: "Phụ cấp chức vụ" },
  { id: 6, name: "Phụ cấp chứng chỉ" },
  { id: 7, name: "Phụ cấp kỹ năng" },
  { id: 8, name: "Phụ cấp khu vực" },
  { id: 9, name: "Phụ cấp chuyên cần" },
  { id: 10, name: "Phụ cấp thâm niên" },
  { id: 11, name: "Phụ cấp làm việc tại nhà" },
  { id: 12, name: "Phụ cấp đặc biệt" },
  { id: 13, name: "Phụ cấp khác" }
];

// Helper to escape SQL single quotes
function esc(str) {
  if (str === null || str === undefined) return "NULL";
  return "'" + String(str).replace(/'/g, "''") + "'";
}

let sql = [];
sql.push("SET ECHO OFF;");
sql.push("SET FEEDBACK OFF;");
sql.push("SET DEFINE OFF;");
sql.push("SET AUTOCOMMIT OFF;");
sql.push("WHENEVER SQLERROR EXIT SQL.SQLCODE ROLLBACK;");
sql.push("");

// 1. TB_CONGTY
const ceoName = employees.find(e => e.manv === 1).hoten;
sql.push("-- 1. Update TB_CONGTY");
sql.push(`UPDATE HR.TB_CONGTY SET
  TENCTY = 'Công ty Cổ phần Công nghệ & Sản xuất HRMS Enterprise',
  DIENTHOAICTY = '0222 389 6868',
  EMAILCTY = 'contact@hrms-enterprise.vn',
  DIACHICTY = 'KCN Yên Phong, Xã Yên Trung, Huyện Yên Phong, Tỉnh Bắc Ninh',
  MASOTHUECTY = '2301234567',
  DAIDIEN = ${esc(ceoName)}
WHERE IDCTY = 1;`);
sql.push("");

// 2. TB_BOPHAN (23 departments)
sql.push("-- 2. Upsert TB_BOPHAN");
for (const bp of DEPARTMENTS) {
  sql.push(`MERGE INTO HR.TB_BOPHAN t USING (SELECT ${bp.id} AS IDBP, ${esc(bp.name)} AS TENBP FROM DUAL) s
ON (t.IDBP = s.IDBP)
WHEN MATCHED THEN UPDATE SET t.TENBP = s.TENBP
WHEN NOT MATCHED THEN INSERT (IDBP, TENBP) VALUES (s.IDBP, s.TENBP);`);
}
sql.push("");

// 3. TB_PHONGBAN (20 divisions)
sql.push("-- 3. Upsert TB_PHONGBAN");
for (const pb of DIVISIONS) {
  sql.push(`MERGE INTO HR.TB_PHONGBAN t USING (SELECT ${pb.id} AS IDPB, ${esc(pb.name)} AS TENPB FROM DUAL) s
ON (t.IDPB = s.IDPB)
WHEN MATCHED THEN UPDATE SET t.TENPB = s.TENPB
WHEN NOT MATCHED THEN INSERT (IDPB, TENPB) VALUES (s.IDPB, s.TENPB);`);
}
sql.push("");

// 4. TB_CHUCVU (35 positions)
sql.push("-- 4. Upsert TB_CHUCVU");
for (const cv of POSITIONS) {
  sql.push(`MERGE INTO HR.TB_CHUCVU t USING (SELECT ${cv.id} AS IDCV, ${esc(cv.name)} AS TENCV FROM DUAL) s
ON (t.IDCV = s.IDCV)
WHEN MATCHED THEN UPDATE SET t.TENCV = s.TENCV
WHEN NOT MATCHED THEN INSERT (IDCV, TENCV) VALUES (s.IDCV, s.TENCV);`);
}
sql.push("");

// 5. TB_PHUCAP (13 allowances)
sql.push("-- 5. Upsert TB_PHUCAP");
for (const pc of ALLOWANCES) {
  sql.push(`MERGE INTO HR.TB_PHUCAP t USING (SELECT ${pc.id} AS IDPC, ${esc(pc.name)} AS TENPC FROM DUAL) s
ON (t.IDPC = s.IDPC)
WHEN MATCHED THEN UPDATE SET t.TENPC = s.TENPC
WHEN NOT MATCHED THEN INSERT (IDPC, TENPC) VALUES (s.IDPC, s.TENPC);`);
}
sql.push("");

// 6. TB_LOAIHOPDONG
sql.push("-- 6. Update TB_LOAIHOPDONG");
sql.push("UPDATE HR.TB_LOAIHOPDONG SET TENLOAIHD = 'Hợp đồng thử việc' WHERE LOAIHD = 1;");
sql.push("UPDATE HR.TB_LOAIHOPDONG SET TENLOAIHD = 'Hợp đồng xác định thời hạn' WHERE LOAIHD = 2;");
sql.push("UPDATE HR.TB_LOAIHOPDONG SET TENLOAIHD = 'Hợp đồng không xác định thời hạn' WHERE LOAIHD = 3;");
sql.push("");

// 7. TB_NHANVIEN (200 employees)
sql.push("-- 7. Update TB_NHANVIEN");
for (const e of employees) {
  const dathoiviec = e.isResigned ? 1 : 0;
  const loainv = e.isNewbie ? 1 : 0;
  sql.push(`UPDATE HR.TB_NHANVIEN SET
  HOTEN = ${esc(e.hoten)},
  EMPLOYEE_CODE = ${esc(e.code)},
  IDGT = ${e.idgt},
  NGAYSINH = TO_DATE('${e.dob}', 'YYYY-MM-DD'),
  DIENTHOAI = ${esc(e.phone)},
  CCCD = ${esc(e.cccd)},
  DIACHI = ${esc(e.address)},
  IDPB = ${e.idpb},
  IDBP = ${e.idbp},
  IDCV = ${e.idcv},
  IDTD = ${e.idtd},
  IDDT = ${e.iddt},
  IDTG = ${e.idtg},
  IDCTY = 1,
  IDQT = ${e.idqt},
  DATHOIVIEC = ${dathoiviec},
  LOAI_NV = ${loainv}
WHERE MANV = ${e.manv};`);
}
sql.push("");

// 8. Delete demo records from TB_CHUCVU and TB_PHONGBAN if not referenced
sql.push("-- 8. Clean up obsolete demo catalog records if unreferenced");
sql.push("DELETE FROM HR.TB_CHUCVU WHERE IDCV = 2 AND NOT EXISTS (SELECT 1 FROM HR.TB_NHANVIEN WHERE IDCV = 2);");
sql.push("DELETE FROM HR.TB_PHONGBAN WHERE IDPB BETWEEN 2 AND 8 AND NOT EXISTS (SELECT 1 FROM HR.TB_NHANVIEN WHERE IDPB = HR.TB_PHONGBAN.IDPB);");
sql.push("");

// 9. TB_HOPDONG (200 contracts)
sql.push("-- 9. Update TB_HOPDONG");
for (const e of employees) {
  const thoihan = e.contractType === 1 ? '02 tháng' : (e.contractType === 2 ? '24 tháng' : 'Không xác định thời hạn');
  const noidung = `Hợp đồng lao động tiêu chuẩn vị trí ${e.positionTitle}`;
  sql.push(`UPDATE HR.TB_HOPDONG SET
  SOHD = ${esc(e.contractNo)},
  NGAYBATDAU = TO_DATE('${e.contractStart}', 'YYYY-MM-DD'),
  NGAYKETTHUC = TO_DATE('${e.contractEnd}', 'YYYY-MM-DD'),
  NGAYKY = TO_DATE('${e.contractStart}', 'YYYY-MM-DD'),
  LANKY = 1,
  THOIHAN = ${esc(thoihan)},
  HESOLUONG = 1.0,
  LUONG_THOA_THUAN = ${e.salary},
  LOAIHD = ${e.contractType},
  NOIDUNG = ${esc(noidung)}
WHERE MANV = ${e.manv};`);
}
sql.push("");

// 10. TB_LUONG_HIEULUC (200 effective salary records)
sql.push("-- 10. Update TB_LUONG_HIEULUC");
for (const e of employees) {
  const cancu = `Hợp đồng lao động số ${e.contractNo}`;
  // Ensure DEN_NGAY > TU_NGAY
  const denNgayStr = (e.contractType === 3) ? '2030-12-31' : e.contractEnd;
  sql.push(`UPDATE HR.TB_LUONG_HIEULUC SET
  SOHD = ${esc(e.contractNo)},
  TU_NGAY = TO_DATE('${e.contractStart}', 'YYYY-MM-DD'),
  DEN_NGAY = TO_DATE('${denNgayStr}', 'YYYY-MM-DD'),
  LUONG_THANG = ${e.salary},
  CAN_CU = ${esc(cancu)}
WHERE MANV = ${e.manv};`);
}
sql.push("");

// 11. TB_BAOHIEM (200 insurance books)
sql.push("-- 11. Update TB_BAOHIEM");
for (const e of employees) {
  const luongBh = Math.min(e.salary, 36000000);
  sql.push(`UPDATE HR.TB_BAOHIEM SET
  SOBH = ${esc(e.insuranceNo)},
  NGAYCAP = TO_DATE('${e.contractStart}', 'YYYY-MM-DD'),
  NOICAP = 'Bảo hiểm Xã hội tỉnh Bắc Ninh',
  NOIKHAMBENH = ${esc(e.hospital)},
  LUONG_BHXH = ${luongBh}
WHERE MANV = ${e.manv};`);
}
sql.push("");

// 12. TB_NHANVIEN_PHUCAP (Replace 200 with realistic distribution of 463 allowances)
sql.push("-- 12. Refresh TB_NHANVIEN_PHUCAP");
sql.push("DELETE FROM HR.TB_NHANVIEN_PHUCAP WHERE MANV BETWEEN 1 AND 200;");
for (const e of employees) {
  for (const a of e.allowances) {
    const tuNgay = e.contractStart;
    const denNgay = (e.contractType === 3) ? '2030-12-31' : e.contractEnd;
    sql.push(`INSERT INTO HR.TB_NHANVIEN_PHUCAP (MANV, IDPC, SOTIEN, GHICHU, TU_NGAY, DEN_NGAY, CACH_TINH)
VALUES (${e.manv}, ${a.idpc}, ${a.amount}, ${esc(a.reason)}, TO_DATE('${tuNgay}', 'YYYY-MM-DD'), TO_DATE('${denNgay}', 'YYYY-MM-DD'), 'CO_DINH_THANG');`);
  }
}
sql.push("");

// 13. TB_NHANVIEN_THOIVIEC (5 resigned employees MANV 190..194)
sql.push("-- 13. Update TB_NHANVIEN_THOIVIEC");
const TV_REASONS = [
  "Hết hạn hợp đồng lao động và có nguyện vọng cá nhân chuyển đổi định hướng",
  "Chuyển nơi cư trú theo kế hoạch gia đình",
  "Nguyện vọng cá nhân phát triển kinh doanh độc lập",
  "Đi du học nâng cao trình độ chuyên môn",
  "Giải quyết việc gia đình cá nhân dài hạn"
];
for (let manv = 190; manv <= 194; manv++) {
  const idx = manv - 190;
  const sqdtv = `QDTV-2026/${manv}`;
  const lydo = TV_REASONS[idx];
  const ghichu = "Bàn giao công việc và hoàn tất thủ tục ngày 15/09/2026";
  sql.push(`UPDATE HR.TB_NHANVIEN_THOIVIEC SET
  SOQDTV = ${esc(sqdtv)},
  LYDOTV = ${esc(lydo)},
  GHICHUTV = ${esc(ghichu)}
WHERE MANV = ${manv};`);
}
sql.push("");

// 14. TB_BAOHIEM_BIENDONG (11 records: 5 giảm, 6 tăng, retain DRAFT status)
sql.push("-- 14. Update TB_BAOHIEM_BIENDONG");
sql.push("UPDATE HR.TB_BAOHIEM_BIENDONG SET LY_DO = 'Báo giảm lao động do chấm dứt hợp đồng lao động' WHERE MANV BETWEEN 190 AND 194;");
sql.push("UPDATE HR.TB_BAOHIEM_BIENDONG SET LY_DO = 'Báo tăng lao động mới tuyển dụng' WHERE MANV BETWEEN 195 AND 200;");
sql.push("");

// 15. TB_KHENTHUONG_KYLUAT (8 awards)
sql.push("-- 15. Update TB_KHENTHUONG_KYLUAT");
const AWARDS = [
  { manv: 25, title: "Khen thưởng thành tích xuất sắc trong công tác chuyển đổi số quý 3/2026", sqd: "QDKT-2026/025" },
  { manv: 50, title: "Khen thưởng hoàn thành vượt tiến độ triển khai hệ thống thông tin nội bộ", sqd: "QDKT-2026/050" },
  { manv: 75, title: "Khen thưởng sáng kiến cải tiến quy trình công nghệ R&D", sqd: "QDKT-2026/075" },
  { manv: 100, title: "Khen thưởng danh hiệu nhân viên tiêu biểu kinh doanh tháng 8/2026", sqd: "QDKT-2026/100" },
  { manv: 125, title: "Khen thưởng xử lý sự cố kỹ thuật hạ tầng kịp thời", sqd: "QDKT-2026/125" },
  { manv: 150, title: "Khen thưởng quản lý tiến độ chuỗi cung ứng logistics", sqd: "QDKT-2026/150" },
  { manv: 175, title: "Khen thưởng tiết kiệm nguyên vật liệu và nâng cao năng suất chuyền may", sqd: "QDKT-2026/175" },
  { manv: 200, title: "Khen thưởng tinh thần học hỏi và hòa nhập xuất sắc giai đoạn thử việc", sqd: "QDKT-2026/200" }
];
for (const a of AWARDS) {
  sql.push(`UPDATE HR.TB_KHENTHUONG_KYLUAT SET
  SOQUYETDINH = ${esc(a.sqd)},
  LYDO = ${esc(a.title)},
  NOIDUNG = ${esc(a.title)}
WHERE MANV = ${a.manv};`);
}
sql.push("");

// 16. TB_UNGLUONG (10 advances)
sql.push("-- 16. Update TB_UNGLUONG");
for (let manv = 20; manv <= 200; manv += 20) {
  sql.push(`UPDATE HR.TB_UNGLUONG SET
  GHICHU = 'Tạm ứng tiền lương kỳ tháng 09/2026 phục vụ nhu cầu cá nhân'
WHERE MANV = ${manv};`);
}
sql.push("");

// 17. Sync HOTEN in TB_KYCONGCHITIET and TB_BANGCONG_CHITIET
sql.push("-- 17. Synchronize employee names in attendance details");
for (const e of employees) {
  sql.push(`UPDATE HR.TB_KYCONGCHITIET SET HOTEN = ${esc(e.hoten)} WHERE MANV = ${e.manv};`);
  sql.push(`UPDATE HR.TB_BANGCONG_CHITIET SET HOTEN = ${esc(e.hoten)} WHERE MANV = ${e.manv};`);
}
sql.push("");

// 18. Natural descriptions for attendance exceptions & scenarios
sql.push("-- 18. Update descriptions in TB_CHAMCONG_BATTHUONG and TB_UAT_SCENARIO");
sql.push("UPDATE HR.TB_CHAMCONG_BATTHUONG SET MO_TA = 'Nhân viên có giờ vào lúc 08:00:00 nhưng thiếu giờ ra.' WHERE IDBATTHUONG = 1;");
sql.push("UPDATE HR.TB_CHAMCONG_BATTHUONG SET MO_TA = 'Có mặt ngoài ca từ 17:00 đến 21:00 chưa có đơn tăng ca được duyệt.' WHERE IDBATTHUONG = 2;");
sql.push("UPDATE HR.TB_CHAMCONG_BATTHUONG SET MO_TA = 'Có mặt ngoài ca từ 17:00 đến 17:30 chưa có đơn tăng ca được duyệt.' WHERE IDBATTHUONG = 3;");
sql.push("UPDATE HR.TB_CHAMCONG_BATTHUONG SET MO_TA = 'Có mặt ngoài ca từ 20:00 đến 21:00 chưa có đơn tăng ca được duyệt.' WHERE IDBATTHUONG = 4;");
sql.push("UPDATE HR.TB_CHAMCONG_BATTHUONG SET MO_TA = 'Thời gian tăng ca thực tế vượt quá số giây được phê duyệt; cần rà soát phần chênh lệch.' WHERE IDBATTHUONG = 5;");
sql.push("UPDATE HR.TB_CHAMCONG_BATTHUONG SET MO_TA = 'Thiếu lịch làm việc, ca làm việc hoặc chính sách ban hành; không thể xác nhận công.' WHERE IDBATTHUONG = 6;");
sql.push("UPDATE HR.TB_CHAMCONG_BATTHUONG SET MO_TA = 'Nhân viên quẹt thẻ nhưng không có lịch làm việc được phân công vào ngày này.' WHERE IDBATTHUONG = 7;");
sql.push("UPDATE HR.TB_CHAMCONG_BATTHUONG SET MO_TA = 'Vắng mặt trong ca làm việc 420 phút chưa có đơn phép hoặc giải trình hợp lệ.' WHERE IDBATTHUONG = 8;");
sql.push("UPDATE HR.TB_CHAMCONG_BATTHUONG SET MO_TA = 'Giờ ra (08:00:00) nhỏ hơn hoặc bằng giờ vào (17:00:00) trong cùng ngày (giờ đảo ngược).' WHERE IDBATTHUONG = 9;");

const SCENARIOS = [
  { ma: 'STANDARD', desc: 'Ca hành chính chuẩn 8 giờ đủ lượt quẹt thẻ' },
  { ma: 'PAID_LEAVE', desc: 'Nghỉ phép hưởng lương nguyên ngày có đơn duyệt' },
  { ma: 'MORNING_LEAVE', desc: 'Nghỉ phép nửa ngày sáng, đi làm buổi chiều' },
  { ma: 'LATE_20', desc: 'Đi muộn 20 phút (dung sai 5 phút, chỉ tính phần vượt)' },
  { ma: 'MISSING_OUT', desc: 'Thiếu lượt quẹt giờ ra cuối ca (chặn chốt công)' },
  { ma: 'OUT_21_NO_OT', desc: 'Ra muộn lúc 21h nhưng không có đơn tăng ca duyệt' },
  { ma: 'PART_APPROVED_OT', desc: 'Duyệt 2.5 giờ OT, thời gian dôi dư còn lại chờ duyệt' },
  { ma: 'OT_CAP', desc: 'Có mặt 2 giờ ngoài ca nhưng chỉ duyệt 1 giờ OT' },
  { ma: 'NO_SCHEDULE', desc: 'Quẹt thẻ nhưng không có lịch làm việc được phân công' },
  { ma: 'APPROVED_OT', desc: 'Tăng ca 2 giờ đã được phê duyệt hợp lệ' },
  { ma: 'MULTIDAY_LEAVE', desc: 'Ngày thứ 2 trong đợt nghỉ phép nhiều ngày' },
  { ma: 'BHXH_LEAVE', desc: 'Nghỉ ốm đau BHXH chi trả, công ty không tính lương' },
  { ma: 'UNPAID_LEAVE', desc: 'Nghỉ việc riêng không hưởng lương có đơn duyệt' },
  { ma: 'PARTIAL_ABSENCE', desc: 'Nghỉ phép 1 giờ, 7 giờ còn lại vắng mặt chưa xác minh' },
  { ma: 'REVERSED_PUNCH', desc: 'Lượt quẹt bị đảo ngược (giờ ra sớm hơn giờ vào)' },
  { ma: 'EARLY_NOT_PAID', desc: 'Đến sớm trước ca làm việc không tự cộng dồn công' },
  { ma: 'GRACE_4', desc: 'Đi muộn 4 phút nằm trong khoảng dung sai cho phép' },
  { ma: 'NIGHT', desc: 'Ca làm việc ban đêm vắt qua nửa đêm' },
  { ma: 'SPLIT', desc: 'Ca gãy gồm hai khung giờ làm việc tách rời' },
  { ma: 'SHORT', desc: 'Ca làm việc ngắn 4 giờ nửa ngày' },
  { ma: 'PAID_HOLIDAY', desc: 'Nghỉ ngày lễ Quốc khánh 02/09 hưởng nguyên lương' },
  { ma: 'BEFORE_JOIN', desc: 'Thời điểm trước ngày bắt đầu hợp đồng thử việc' },
  { ma: 'AFTER_EXIT', desc: 'Thời điểm sau ngày chấm dứt hợp đồng lao động' }
];
for (const s of SCENARIOS) {
  sql.push(`UPDATE HR.TB_UAT_SCENARIO SET MO_TA = ${esc(s.desc)} WHERE MA = ${esc(s.ma)};`);
}
sql.push("");

// COMMIT
sql.push("COMMIT;");
sql.push("PROMPT ALL REALISTIC DML COMMITTED SUCCESSFULLY;");
sql.push("EXIT;");

const sqlScriptPath = path.join(__dirname, 'apply_realistic_data.sql');
fs.writeFileSync(sqlScriptPath, sql.join('\n'), 'utf8');
console.log(`Generated ${sqlScriptPath} with ${sql.length} lines.`);

// Execute via SQL*Plus
console.log("Executing DML transaction via SQL*Plus (HR schema)...");
try {
  const result = execSync(`sqlplus -s hr/hr@localhost:1521/orcl @${sqlScriptPath}`, {
    encoding: 'utf8',
    env: { ...process.env, NLS_LANG: 'AMERICAN_AMERICA.AL32UTF8' }
  });
  console.log(result);
  console.log("✓ EXECUTION FINISHED.");
} catch (err) {
  console.error("FATAL: SQL*Plus execution failed:", err.stdout || err.message);
  process.exit(1);
}
