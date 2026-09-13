-- ====================================================================
-- HRMS Enterprise Database Migration Script
-- Version: V1_4
-- Description: Khắc phục lỗi Font Tiếng Việt (Mojibake), Chuẩn hóa Triggers & Đồng bộ dữ liệu tự động
-- ====================================================================

-- 1. SỬA TRIGGER TỰ SINH PHỤ CẤP VỚI CHUỖI UNICODE CHUẨN
CREATE OR REPLACE TRIGGER TRG_AUTO_PHUCAP 
AFTER INSERT ON TB_KYCONGCHITIET
FOR EACH ROW
BEGIN
  FOR i IN 1..7 LOOP
    INSERT INTO TB_NHANVIEN_PHUCAP (MANV, IDPC, MAKYCONG, SOTIEN, GHICHU)
    VALUES (:NEW.MANV, i, :NEW.MAKYCONG, 0, N'Khởi tạo tự động');
  END LOOP;
END;
/

-- 2. SỬA TRIGGER TỰ SINH BẢO HIỂM VỚI CHUỖI UNICODE CHUẨN
CREATE OR REPLACE TRIGGER TRG_AUTO_BAOHIEM
AFTER INSERT ON TB_NHANVIEN
FOR EACH ROW
BEGIN
  INSERT INTO TB_BAOHIEM (SOBH, NGAYCAP, NOICAP, NOIKHAMBENH, MANV, LUONG_BHXH)
  VALUES ('BH_' || :NEW.MANV, SYSDATE, N'BHXH Việt Nam', N'Bệnh viện Đa khoa', :NEW.MANV, 0);
END;
/

-- 3. CẬP NHẬT DỮ LIỆU HIỆN CÓ ĐỂ SỬA DỨT ĐIỂM MOJIBAKE
-- 3.1. Bảng phụ cấp nhân viên
UPDATE TB_NHANVIEN_PHUCAP 
SET GHICHU = N'Khởi tạo tự động' 
WHERE GHICHU LIKE '%Khá%' OR GHICHU LIKE '%á»%' OR GHICHU LIKE '%Ä‘%' OR GHICHU IS NULL;

-- 3.2. Bảng bảo hiểm
UPDATE TB_BAOHIEM 
SET NOICAP = N'BHXH Việt Nam' 
WHERE NOICAP LIKE '%Viá»%' OR NOICAP LIKE '%BHXH%';

UPDATE TB_BAOHIEM 
SET NOIKHAMBENH = N'Bệnh viện Đa khoa' 
WHERE NOIKHAMBENH LIKE '%Bá»%' OR NOIKHAMBENH LIKE '%Ä a khoa%';

-- 3.3. Bảng hợp đồng lao động
UPDATE TB_HOPDONG 
SET THOIHAN = N'3 Năm' 
WHERE THOIHAN LIKE '%3 NÄƒm%' OR THOIHAN = '3 Nam';

-- 3.4. Bảng điều chuyển nhân viên
UPDATE TB_DIEUCHUYEN_NHANVIEN 
SET LYDODC = N'Điều động nhân sự hỗ trợ phòng Nhân sự' 
WHERE SOQDDIEUCHUYEN = '00001/2026/QDDD' OR LYDODC LIKE '%Ä iá» u%' OR LYDODC LIKE '%há»— trá»£%';

UPDATE TB_DIEUCHUYEN_NHANVIEN 
SET LYDODC = N'Tăng cường nhân lực cho bộ phận Logistics' 
WHERE SOQDDIEUCHUYEN = '00002/2026/QDDD' OR LYDODC LIKE '%TÄƒng cÆ°á» ng%' OR LYDODC LIKE '%Logistics%';

UPDATE TB_DIEUCHUYEN_NHANVIEN 
SET LYDODC = N'Luân chuyển công tác về tổ IT' 
WHERE SOQDDIEUCHUYEN = '00003/2026/QDDD' OR LYDODC LIKE '%LuÃ¢n chuyá»ƒn%' OR LYDODC LIKE '%tá»• IT%';

UPDATE TB_DIEUCHUYEN_NHANVIEN 
SET GHICHU = N'Quyết định từ Giám đốc' 
WHERE GHICHU LIKE '%Quyáº¿t%';

UPDATE TB_DIEUCHUYEN_NHANVIEN 
SET GHICHU = N'Theo nhu cầu sản xuất kinh doanh' 
WHERE GHICHU LIKE '%nhu cáº§u%';

UPDATE TB_DIEUCHUYEN_NHANVIEN 
SET GHICHU = N'Phát triển dự án mới' 
WHERE GHICHU LIKE '%PhÃ¡t triá»ƒn%';

-- 3.5. Bảng khen thưởng & kỷ luật
UPDATE TB_KHENTHUONG_KYLUAT 
SET NOIDUNG = N'Tuyên dương trước toàn công ty và thưởng 500,000đ' 
WHERE NOIDUNG LIKE '%TuyÃªn%';

UPDATE TB_KHENTHUONG_KYLUAT 
SET NOIDUNG = N'Khiển trách và cảnh báo' 
WHERE NOIDUNG LIKE '%Khiá»ƒn%';

UPDATE TB_KHENTHUONG_KYLUAT 
SET LYDO = N'Hoàn thành xuất sắc tiến độ công việc' 
WHERE LYDO LIKE '%xuáº¥t sáº¯c%';

UPDATE TB_KHENTHUONG_KYLUAT 
SET LYDO = N'Vi phạm quy chế đi làm muộn' 
WHERE LYDO LIKE '%muá»™n%';

-- 3.6. Bảng tạm ứng lương
UPDATE TB_UNGLUONG 
SET GHICHU = N'Tạm ứng lương giữa kỳ' 
WHERE GHICHU LIKE '%á»©ng%' OR GHICHU LIKE '%Táº¡m%';

-- 3.7. Đồng bộ họ tên nhân viên trong kỳ công và bảng công chi tiết từ bảng gốc TB_NHANVIEN
UPDATE TB_KYCONGCHITIET kc 
SET HOTEN = (SELECT nv.HOTEN FROM TB_NHANVIEN nv WHERE nv.MANV = kc.MANV)
WHERE EXISTS (
    SELECT 1 FROM TB_NHANVIEN nv 
    WHERE nv.MANV = kc.MANV 
      AND (kc.HOTEN LIKE '%á»%' OR kc.HOTEN LIKE '%áº%' OR kc.HOTEN LIKE '%Ä‘%')
);

UPDATE TB_BANGCONG_CHITIET bc 
SET HOTEN = (SELECT nv.HOTEN FROM TB_NHANVIEN nv WHERE nv.MANV = bc.MANV)
WHERE EXISTS (
    SELECT 1 FROM TB_NHANVIEN nv 
    WHERE nv.MANV = bc.MANV 
      AND (bc.HOTEN LIKE '%á»%' OR bc.HOTEN LIKE '%áº%' OR bc.HOTEN LIKE '%Ä‘%')
);

COMMIT;
EXIT;
