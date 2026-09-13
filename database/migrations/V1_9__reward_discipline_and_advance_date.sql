-- V1_9__reward_discipline_and_advance_date.sql
-- Thêm số tiền và thời gian áp dụng (tháng/năm) cho bảng khen thưởng & kỷ luật

ALTER TABLE "HR"."TB_KHENTHUONG_KYLUAT" ADD (
    "SOTIEN" NUMBER(15,2) DEFAULT 0,
    "THANG_APDUNG" NUMBER(38,0),
    "NAM_APDUNG" NUMBER(38,0)
);

COMMENT ON COLUMN "HR"."TB_KHENTHUONG_KYLUAT"."SOTIEN" IS 'Số tiền khen thưởng (cộng) hoặc kỷ luật (trừ)';
COMMENT ON COLUMN "HR"."TB_KHENTHUONG_KYLUAT"."THANG_APDUNG" IS 'Tháng áp dụng cộng/trừ vào lương (1-12)';
COMMENT ON COLUMN "HR"."TB_KHENTHUONG_KYLUAT"."NAM_APDUNG" IS 'Năm áp dụng cộng/trừ vào lương (vd: 2026)';

COMMIT;
EXIT;
