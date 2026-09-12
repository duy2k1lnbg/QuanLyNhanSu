-- ====================================================================
-- HRMS Enterprise Database Migration Script
-- Version: V1_2
-- Description: Khởi tạo các View An Toàn dành riêng cho AI Copilot & Hybrid RAG
-- Chỉ bao gồm dữ liệu công khai, không lộ mật khẩu hay các thông tin bảo mật nhạy cảm
-- ====================================================================

-- 1. VIEW NHÂN VIÊN (AI TRA CỨU HỒ SƠ)
CREATE OR REPLACE VIEW V_AI_EMPLOYEE AS
SELECT 
    nv.MANV,
    nv.HOTEN,
    nv.GIOITINH,
    nv.NGAYSINH,
    nv.DIENTHOAI,
    nv.DIACHI,
    pb.TENPB AS TEN_PHONGBAN,
    bp.TENBP AS TEN_BOPHAN,
    cv.TENCV AS TEN_CHUCVU,
    td.TENTD AS TEN_TRINHDO,
    dt.TENDT AS TEN_DANTOC,
    tg.TENTG AS TEN_TONGIAO,
    nv.MACTY
FROM TB_NHANVIEN nv
LEFT JOIN TB_PHONGBAN pb ON nv.IDPB = pb.IDPB
LEFT JOIN TB_BOPHAN bp ON nv.IDBP = bp.IDBP
LEFT JOIN TB_CHUCVU cv ON nv.IDCV = cv.IDCV
LEFT JOIN TB_TRINHDO td ON nv.IDTD = td.IDTD
LEFT JOIN TB_DANTOC dt ON nv.IDDT = dt.IDDT
LEFT JOIN TB_TONGIAO tg ON nv.IDTG = tg.IDTG
WHERE NVL(nv.DATHOIVIEC, 0) = 0
  AND nv.DELETED_DATE IS NULL;

-- 2. VIEW CHẤM CÔNG (AI TRA CỨU NGÀY CÔNG & CHUYÊN CẦN)
CREATE OR REPLACE VIEW V_AI_ATTENDANCE AS
SELECT 
    kc.MAKYCONG,
    kc.THANG,
    kc.NAM,
    ct.MANV,
    nv.HOTEN,
    ct.TONGNGAYCONG,
    ct.NGAYPHEP,
    ct.NGHIKHONGPHEP,
    ct.CONGNGAYLE,
    ct.CONGCHUNHAT,
    pb.TENPB AS TEN_PHONGBAN
FROM TB_KYCONGCHITIET ct
JOIN TB_KYCONG kc ON ct.MAKYCONG = kc.MAKYCONG
JOIN TB_NHANVIEN nv ON ct.MANV = nv.MANV
LEFT JOIN TB_PHONGBAN pb ON nv.IDPB = pb.IDPB;

-- 3. VIEW TĂNG CA (AI TRA CỨU LÀM THÊM GIỜ)
CREATE OR REPLACE VIEW V_AI_OVERTIME AS
SELECT 
    tc.ID,
    tc.MANV,
    nv.HOTEN,
    tc.NGAY,
    tc.THANG,
    tc.NAM,
    tc.SOGIO,
    lc.TENLOAICA,
    lc.HESOLOAICA,
    tc.SOTIEN,
    tc.GHICHU,
    pb.TENPB AS TEN_PHONGBAN
FROM TB_TANGCA tc
JOIN TB_NHANVIEN nv ON tc.MANV = nv.MANV
LEFT JOIN TB_LOAICA lc ON tc.IDLOAICA = lc.IDLOAICA
LEFT JOIN TB_PHONGBAN pb ON nv.IDPB = pb.IDPB;

-- 4. VIEW BẢO HIỂM (AI TRA CỨU BẢO HIỂM XÃ HỘI & Y TẾ)
CREATE OR REPLACE VIEW V_AI_INSURANCE AS
SELECT 
    bh.IDBH,
    bh.MANV,
    nv.HOTEN,
    bh.SOBH,
    bh.NGAYCAP,
    bh.NOICAP,
    bh.NOIKHAMBENH,
    pb.TENPB AS TEN_PHONGBAN
FROM TB_BAOHIEM bh
JOIN TB_NHANVIEN nv ON bh.MANV = nv.MANV
LEFT JOIN TB_PHONGBAN pb ON nv.IDPB = pb.IDPB;

-- 5. VIEW TẠM ỨNG LƯƠNG (AI TRA CỨU ỨNG LƯƠNG)
CREATE OR REPLACE VIEW V_AI_ADVANCE AS
SELECT 
    ul.ID,
    ul.MANV,
    nv.HOTEN,
    ul.NGAY,
    ul.THANG,
    ul.NAM,
    ul.SOTIEN,
    ul.GHICHU,
    pb.TENPB AS TEN_PHONGBAN
FROM TB_UNGLUONG ul
JOIN TB_NHANVIEN nv ON ul.MANV = nv.MANV
LEFT JOIN TB_PHONGBAN pb ON nv.IDPB = pb.IDPB
WHERE NVL(ul.TRANGTHAI, 1) = 1;

-- 6. VIEW PHỤ CẤP NHÂN VIÊN
CREATE OR REPLACE VIEW V_AI_ALLOWANCE AS
SELECT 
    np.ID,
    np.MANV,
    nv.HOTEN,
    np.IDPC,
    pc.TENPC,
    np.SOTIEN,
    np.NOIDUNG,
    np.NGAY,
    pb.TENPB AS TEN_PHONGBAN
FROM TB_NHANVIEN_PHUCAP np
JOIN TB_NHANVIEN nv ON np.MANV = nv.MANV
LEFT JOIN TB_PHUCAP pc ON np.IDPC = pc.IDPC
LEFT JOIN TB_PHONGBAN pb ON nv.IDPB = pb.IDPB;
