# Danh mục thuộc tính model và phạm vi đối chiếu

Cập nhật nội dung: 01/10/2026.

**Phạm vi:** Danh mục đọc từ model C#; không khẳng định độ phủ Database → UI.

## Cách đọc

Danh mục được lấy từ các khai báo thuộc tính scalar của 87 file model trong `HRMS.DataAccess` ở workspace ngày 01/10/2026. Tổng cộng có 1041 khai báo được ghi lại. Đây không phải số bảng/cột đã xác nhận trên Oracle.

Bản trước suy nhiều quan hệ, đường DTO/API/UI và tình trạng kiểm thử mà không có bằng chứng riêng cho từng dòng. Bản này chỉ ghi những gì đọc được từ model. Không suy khóa chính từ tên cột, không suy `NOT NULL` trong database từ kiểu CLR và không đánh dấu một trường đã được kiểm thử chỉ vì có tên trong danh mục.

Các navigation property `virtual` không nằm trong bảng scalar này. Những cột được truy vấn bằng SQL nhưng chưa có trong model cũng có thể chưa xuất hiện. Khi đối chiếu từng trường, bổ sung bằng chứng từ migration/metadata DB, DTO, endpoint, UI và test thực chạy.

## Các lớp cần đối chiếu tiếp

| Lớp | Nguồn bằng chứng cần có |
| --- | --- |
| Schema | Cột, kiểu Oracle, null/default, constraint và migration đã áp dụng |
| Model/DTO | Tên thuộc tính, chuyển đổi kiểu và cách map |
| Service/API | Đọc/ghi, validate, quyền và phạm vi dữ liệu |
| Giao diện | Trường hiển thị/nhập, format, giới hạn theo vai trò |
| Test | Fixture, assertion, lệnh/log và kết quả thực chạy |

Xem [từ điển lương](payroll.md) và [hiện trạng](current-status.md).

## SCHEMA_VERSION

Nguồn: [SCHEMA_VERSION.cs](../HRMS.DataAccess/SCHEMA_VERSION.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `INSTALLED_RANK` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `VERSION` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DESCRIPTION` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TYPE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SCRIPT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CHECKSUM` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `INSTALLED_BY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `INSTALLED_ON` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `EXECUTION_TIME_MS` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `SUCCESS` | `Nullable<bool>` | Cho phép null ở kiểu CLR |

## TB_AUTH_AUDIT

Nguồn: [TB_AUTH_AUDIT.cs](../HRMS.DataAccess/TB_AUTH_AUDIT.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `AUDIT_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `USER_ID` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `ACTOR_USER_ID` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `SESSION_ID` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `EVENT_TYPE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `RESULT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `REASON` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CLIENT_TYPE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DEVICE_ID_HASH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IP_ADDRESS` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `USER_AGENT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `OCCURRED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `CORRELATION_ID` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `METADATA_JSON` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_AUTH_LOGIN_ATTEMPT

Nguồn: [TB_AUTH_LOGIN_ATTEMPT.cs](../HRMS.DataAccess/TB_AUTH_LOGIN_ATTEMPT.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ATTEMPT_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `USER_ID` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `LOGIN_IDENTIFIER` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CLIENT_TYPE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DEVICE_ID_HASH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IP_ADDRESS` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `USER_AGENT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `ATTEMPTED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `SUCCESS_FLAG` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `FAILURE_REASON` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CORRELATION_ID` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_AUTH_POLICY

Nguồn: [TB_AUTH_POLICY.cs](../HRMS.DataAccess/TB_AUTH_POLICY.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `POLICY_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `SCOPE_TYPE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SCOPE_ID` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MAX_ACTIVE_SESSIONS` | `byte` | Kiểu giá trị không nullable trong khai báo |
| `SESSION_LIMIT_STRATEGY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `ACCESS_TOKEN_MINUTES` | `int` | Kiểu giá trị không nullable trong khai báo |
| `IDLE_TIMEOUT_MINUTES` | `int` | Kiểu giá trị không nullable trong khai báo |
| `ABSOLUTE_TIMEOUT_MINUTES` | `int` | Kiểu giá trị không nullable trong khai báo |
| `MAX_FAILED_LOGIN_ATTEMPTS` | `byte` | Kiểu giá trị không nullable trong khai báo |
| `FAILED_ATTEMPT_WINDOW_MINUTES` | `short` | Kiểu giá trị không nullable trong khai báo |
| `LOCKOUT_DURATION_MINUTES` | `short` | Kiểu giá trị không nullable trong khai báo |
| `ENABLED` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `UPDATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_AUTH_SESSION

Nguồn: [TB_AUTH_SESSION.cs](../HRMS.DataAccess/TB_AUTH_SESSION.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `SESSION_ID` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `USER_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `JTI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CLIENT_TYPE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `PLATFORM` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DEVICE_ID_HASH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DEVICE_NAME` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IP_ADDRESS` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `USER_AGENT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `LAST_USED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `EXPIRES_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `REVOKED_AT` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `REVOKE_REASON` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_BANGCONG

Nguồn: [TB_BANGCONG.cs](../HRMS.DataAccess/TB_BANGCONG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `MABC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `NAM` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THANG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGAY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `GIOVAO` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `PHUTVAO` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `GIORA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `PHUTRA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `MANV` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDLOAICONG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THOIDIEM_VAO` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `THOIDIEM_RA` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `NGUON_CHAM` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MA_SU_KIEN_NGUON` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MA_THIET_BI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TIEPNHAN_LUC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `NGUON_REV` | `Nullable<long>` | Cho phép null ở kiểu CLR |

## TB_BANGCONG_CHITIET

Nguồn: [TB_BANGCONG_CHITIET.cs](../HRMS.DataAccess/TB_BANGCONG_CHITIET.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDBANGCONGCT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MAKYCONG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDCTY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `MANV` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `HOTEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAY` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `THU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `GIOVAO` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `GIORA` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAYPHEP` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CONGNGAYLE` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CONGCHUNHAT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `KYHIEU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `GHICHU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `NGAYCONG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TRANGTHAI_CONG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DU_DIEUKIEN_CHOT` | `Nullable<bool>` | Cho phép null ở kiểu CLR |
| `LANTINH_ID_HIENHANH` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `GIAY_THUC_TE` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `GIAY_HUONG_CONG_THUONG` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `GIAY_OT_XAC_NHAN` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `GIAY_DEM_TRONG_GIO_THUONG` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `GIAY_DEM_OT` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `GIAY_DI_MUON_THUC_TE` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `GIAY_VE_SOM_THUC_TE` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `GIAY_DI_MUON_VIPHAM` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `GIAY_VE_SOM_VIPHAM` | `Nullable<long>` | Cho phép null ở kiểu CLR |

## TB_BANGLUONG

Nguồn: [TB_BANGLUONG.cs](../HRMS.DataAccess/TB_BANGLUONG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDBL` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MAKYCONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `THANG` | `byte` | Kiểu giá trị không nullable trong khai báo |
| `NAM` | `short` | Kiểu giá trị không nullable trong khai báo |
| `CONG_CHUAN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CONG_THUCTE` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CONG_LAMDEM` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DAILY_RATE` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DAILY_ALLOWANCE` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `LUONG_CONG_THUCTE` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `PHUCAP_CONG_THUCTE` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TIEN_TANGCA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TIEN_CHUYENCAN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TIEN_AN_CA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `KHOAN_CONG_KHAC` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TIEN_BHXH_TRICH` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TIEN_TAMUNG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `KHOAN_TRU_KHAC` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THUC_LINH` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CONG_LAMNGAY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TONG_CONG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `LUONG_BHXH` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TIEN_BHXH` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TIEN_BHYT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TIEN_BHTN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TIEN_CONG_DOAN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THUE_TNCN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `HOAN_THUE` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |

## TB_BANGLUONG_BAOHIEM

Nguồn: [TB_BANGLUONG_BAOHIEM.cs](../HRMS.DataAccess/TB_BANGLUONG_BAOHIEM.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `IDBL` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MAKYCONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `POLICY_BHXH_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `PROFILE_BH_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MUC_THAM_CHIEU` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `VUNG_LUONG` | `short` | Kiểu giá trị không nullable trong khai báo |
| `LUONG_TOI_THIEU_VUNG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `LUONG_DONG_BHXH_GOC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `LUONG_DONG_BHXH_AP_DUNG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `LUONG_DONG_BHTN_AP_DUNG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_BHXH_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TIEN_BHXH_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_BHYT_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TIEN_BHYT_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_BHTN_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TIEN_BHTN_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TONG_BH_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_BHXH_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TIEN_BHXH_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_BHYT_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TIEN_BHYT_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_BHTN_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TIEN_BHTN_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_TNLD_BNN_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TIEN_TNLD_BNN_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TONG_BH_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_BANGLUONG_CONG_DOAN

Nguồn: [TB_BANGLUONG_CONG_DOAN.cs](../HRMS.DataAccess/TB_BANGLUONG_CONG_DOAN.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `IDBL` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MAKYCONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `POLICY_CD_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `PROFILE_CD_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `LA_DOAN_VIEN` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `LUONG_CAN_CU_DONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_DOAN_PHI_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MUC_TRAN_DOAN_PHI` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TIEN_DOAN_PHI_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_KPCD_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TIEN_KPCD_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_BANGLUONG_CT

Nguồn: [TB_BANGLUONG_CT.cs](../HRMS.DataAccess/TB_BANGLUONG_CT.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDBLCT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `IDBL` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MAKYCONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `NHOM_KHOAN_MUC` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MA_KHOAN_MUC` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_KHOAN_MUC` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SO_LUONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `DON_GIA` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HE_SO` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `THANH_TIEN` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TINH_VAO_DONG_BHXH` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `TINH_THUE_TNCN` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `SO_TIEN_MIEN_THUE` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `SO_TIEN_CHIU_THUE` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CONG_THUC_DIEN_GIAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IS_LEGACY` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_BANGLUONG_CT_SOURCE

Nguồn: [TB_BANGLUONG_CT_SOURCE.cs](../HRMS.DataAccess/TB_BANGLUONG_CT_SOURCE.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `IDBLCT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `SOURCE_TYPE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SOURCE_BCCT_ID` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `SOURCE_TC_ID` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `SOURCE_NVPC_ID` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `SOURCE_KTKL_SOQD` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SOURCE_UL_ID` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `WEIGHT_QUANTITY` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `AMOUNT_CONTRIBUTED` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_BANGLUONG_OT_COMPLIANCE

Nguồn: [TB_BANGLUONG_OT_COMPLIANCE.cs](../HRMS.DataAccess/TB_BANGLUONG_OT_COMPLIANCE.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `IDBL` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MAKYCONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MONTHLY_STANDARD_LIMIT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `ANNUAL_STANDARD_LIMIT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `ANNUAL_EXTENDED_LIMIT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `ACTUAL_HOURS_MONTH` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `ACTUAL_HOURS_YTD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `LEGAL_HOURS_MONTH` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `EXCESS_HOURS_MONTH` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `EXTENDED_ELIGIBLE` | `Nullable<bool>` | Cho phép null ở kiểu CLR |
| `EMPLOYEE_CONSENT` | `Nullable<bool>` | Cho phép null ở kiểu CLR |
| `NOTIFICATION_FILED` | `Nullable<bool>` | Cho phép null ở kiểu CLR |
| `COMPLIANCE_STATUS` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TOTAL_OT_PAYMENT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `LEGAL_ALLOWED_PAYMENT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `EXEMPT_OT_PAYMENT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TAXABLE_EXCESS_PAYMENT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_BANGLUONG_THUE_CT

Nguồn: [TB_BANGLUONG_THUE_CT.cs](../HRMS.DataAccess/TB_BANGLUONG_THUE_CT.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `IDBL` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MAKYCONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `POLICY_THUE_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `BAC_THUE` | `byte` | Kiểu giá trị không nullable trong khai báo |
| `CAN_DUOI` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `CAN_TREN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THU_NHAP_CHIU_THUE_BAC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `THUE_SUAT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TIEN_THUE_BAC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_BAOHIEM

Nguồn: [TB_BAOHIEM.cs](../HRMS.DataAccess/TB_BAOHIEM.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDBH` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `SOBH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAYCAP` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `NOICAP` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NOIKHAMBENH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MANV` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `LUONG_BHXH` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |

## TB_BOPHAN

Nguồn: [TB_BOPHAN.cs](../HRMS.DataAccess/TB_BOPHAN.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDBP` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENBP` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_CA_KHUNGGIO

Nguồn: [TB_CA_KHUNGGIO.cs](../HRMS.DataAccess/TB_CA_KHUNGGIO.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDKHUNGGIO` | `long` | Kiểu giá trị không nullable trong khai báo |
| `IDCAPHIENBAN` | `long` | Kiểu giá trị không nullable trong khai báo |
| `STT` | `int` | Kiểu giá trị không nullable trong khai báo |
| `BATDAU_PHUT` | `int` | Kiểu giá trị không nullable trong khai báo |
| `KETTHUC_PHUT` | `int` | Kiểu giá trị không nullable trong khai báo |
| `LOAI_KHUNGGIO` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `BAT_BUOC_QUET_THE` | `bool` | Kiểu giá trị không nullable trong khai báo |

## TB_CA_PHIENBAN

Nguồn: [TB_CA_PHIENBAN.cs](../HRMS.DataAccess/TB_CA_PHIENBAN.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDCAPHIENBAN` | `long` | Kiểu giá trị không nullable trong khai báo |
| `IDLOAICA` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `SO_PHIENBAN` | `int` | Kiểu giá trị không nullable trong khai báo |
| `TEN_PHIENBAN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TU_NGAY` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `DEN_NGAY` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `TONG_GIAY_CHUAN` | `long` | Kiểu giá trị không nullable trong khai báo |
| `CONG_QUY_DOI` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TAO_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `TAO_BOI` | `decimal` | Kiểu giá trị không nullable trong khai báo |

## TB_CHAMCONG_BATTHUONG

Nguồn: [TB_CHAMCONG_BATTHUONG.cs](../HRMS.DataAccess/TB_CHAMCONG_BATTHUONG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDBATTHUONG` | `long` | Kiểu giá trị không nullable trong khai báo |
| `MA_SU_VIEC_HASH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IDLANTINH_PHATHIEN` | `long` | Kiểu giá trị không nullable trong khai báo |
| `IDBANGCONGCT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `IDLICH` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `NGAY` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `MA_LOI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `BATDAU_LUC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `KETTHUC_LUC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `MUC_DO` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CHAN_CHOT` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `INPUT_HASH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `INPUT_SNAPSHOT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IDYEUCAU_NGHIPHEP` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDYEUCAU_TANGCA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDYEUCAU_DIEUCHINH` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THAY_THE_SU_VIEC_ID` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `MO_TA` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TAO_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_CHAMCONG_BT_LICHSU

Nguồn: [TB_CHAMCONG_BT_LICHSU.cs](../HRMS.DataAccess/TB_CHAMCONG_BT_LICHSU.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `long` | Kiểu giá trị không nullable trong khai báo |
| `IDBATTHUONG` | `long` | Kiểu giá trị không nullable trong khai báo |
| `THUTU` | `int` | Kiểu giá trị không nullable trong khai báo |
| `TU_TRANGTHAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DEN_TRANGTHAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `INPUT_HASH_DUYET` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `QUYETDINH_SNAPSHOT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `LY_DO` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGUOI_XULY` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `XULY_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_CHAMCONG_KQ_NGAY

Nguồn: [TB_CHAMCONG_KQ_NGAY.cs](../HRMS.DataAccess/TB_CHAMCONG_KQ_NGAY.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDLANTINH` | `long` | Kiểu giá trị không nullable trong khai báo |
| `IDBANGCONGCT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MAKYCONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `NGAY` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `INPUT_HASH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `INPUT_SNAPSHOT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DU_DIEUKIEN_CHOT` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_THUC_TE` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_HUONG_CONG_THUONG` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_OT_XAC_NHAN` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_DEM_TRONG_GIO_THUONG` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_DEM_OT` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_DI_MUON_THUC_TE` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_VE_SOM_THUC_TE` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_DI_MUON_VIPHAM` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_VE_SOM_VIPHAM` | `long` | Kiểu giá trị không nullable trong khai báo |
| `CONG_THUONG_QUYDOI` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TAO_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_CHAMCONG_LANTINH

Nguồn: [TB_CHAMCONG_LANTINH.cs](../HRMS.DataAccess/TB_CHAMCONG_LANTINH.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDLANTINH` | `long` | Kiểu giá trị không nullable trong khai báo |
| `MA_YEU_CAU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `REQUEST_HASH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MAKYCONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TU_NGAY` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `DEN_NGAY` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `INPUT_REV` | `long` | Kiểu giá trị không nullable trong khai báo |
| `EXPECTED_PUBLISH_REV` | `long` | Kiểu giá trị không nullable trong khai báo |
| `INPUT_DATA_HASH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `PHIENBAN_ENGINE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MUI_GIO` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `BATDAU_TINH` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `KETTHUC_TINH` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `CONGBO_LUC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `NGUOI_THUC_HIEN` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `GHI_CHU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_CHINH_SACH_BHXH

Nguồn: [TB_CHINH_SACH_BHXH.cs](../HRMS.DataAccess/TB_CHINH_SACH_BHXH.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MA_CHINH_SACH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_CHINH_SACH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAY_HIEU_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `NGAY_HET_HIEU_LUC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `MUC_THAM_CHIEU` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_BHXH_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_BHYT_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_BHTN_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_BHXH_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_BHYT_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_BHTN_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_TNLD_BNN_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TY_LE_TNLD_BNN_UU_DAI` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `AP_DUNG_TRAN_BHXH_BHYT` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `AP_DUNG_TRAN_BHTN` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_CHINH_SACH_BHXH_VUNG

Nguồn: [TB_CHINH_SACH_BHXH_VUNG.cs](../HRMS.DataAccess/TB_CHINH_SACH_BHXH_VUNG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `POLICY_BHXH_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `VUNG_LUONG` | `short` | Kiểu giá trị không nullable trong khai báo |
| `LUONG_TOI_THIEU_THANG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `LUONG_TOI_THIEU_GIO` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HE_SO_SAN_DOANH_NGHIEP` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_CHINH_SACH_CONG_DOAN

Nguồn: [TB_CHINH_SACH_CONG_DOAN.cs](../HRMS.DataAccess/TB_CHINH_SACH_CONG_DOAN.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MA_CHINH_SACH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_CHINH_SACH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAY_HIEU_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `NGAY_HET_HIEU_LUC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `TY_LE_DOAN_PHI_NLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `CAP_PERCENT_STATUTORY_BASE_SALARY` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `KINH_PHI_CONG_DOAN_NSDLD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_CHINH_SACH_LUONG

Nguồn: [TB_CHINH_SACH_LUONG.cs](../HRMS.DataAccess/TB_CHINH_SACH_LUONG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MA_CHINH_SACH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_CHINH_SACH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAY_HIEU_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `NGAY_HET_HIEU_LUC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `SO_CONG_CHUAN_THANG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `SO_GIO_CHUAN_NGAY` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HE_SO_LAM_DEM` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_CHUCVU

Nguồn: [TB_CHUCVU.cs](../HRMS.DataAccess/TB_CHUCVU.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDCV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENCV` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_CONFIG

Nguồn: [TB_CONFIG.cs](../HRMS.DataAccess/TB_CONFIG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID_CF` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `NAME` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `VALUE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_CONG_PD_NGUON

Nguồn: [TB_CONG_PD_NGUON.cs](../HRMS.DataAccess/TB_CONG_PD_NGUON.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `long` | Kiểu giá trị không nullable trong khai báo |
| `IDPHANDOAN` | `long` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MABC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `SOURCE_HASH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SOURCE_SNAPSHOT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_CONG_PHANDOAN

Nguồn: [TB_CONG_PHANDOAN.cs](../HRMS.DataAccess/TB_CONG_PHANDOAN.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDPHANDOAN` | `long` | Kiểu giá trị không nullable trong khai báo |
| `IDLANTINH` | `long` | Kiểu giá trị không nullable trong khai báo |
| `IDBANGCONGCT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `IDLICH` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `IDQUYDINH` | `long` | Kiểu giá trị không nullable trong khai báo |
| `BATDAU_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `KETTHUC_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `THOILUONG_GIAY` | `long` | Kiểu giá trị không nullable trong khai báo |
| `LOAI_THOIGIAN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `LOAI_NGAY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `LA_BAN_DEM` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `CO_OT_BAN_NGAY_TRUOC` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `TRANGTHAI_XACNHAN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `GIAY_THUC_TE` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_HUONG_CONG_THUONG` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_OT_XAC_NHAN` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_DEM_TRONG_GIO_THUONG` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_DEM_OT` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_DI_MUON_THUC_TE` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_VE_SOM_THUC_TE` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_DI_MUON_VIPHAM` | `long` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_VE_SOM_VIPHAM` | `long` | Kiểu giá trị không nullable trong khai báo |
| `IDYEUCAU_TANGCA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDYEUCAU_NGHIPHEP` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDYEUCAU_DIEUCHINH` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |

## TB_CONGTY

Nguồn: [TB_CONGTY.cs](../HRMS.DataAccess/TB_CONGTY.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDCTY` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENCTY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DIENTHOAICTY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `EMAILCTY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DIACHICTY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MASOTHUECTY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DAIDIEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_DANTOC

Nguồn: [TB_DANTOC.cs](../HRMS.DataAccess/TB_DANTOC.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDDT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENDT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_DIEUCHUYEN_NHANVIEN

Nguồn: [TB_DIEUCHUYEN_NHANVIEN.cs](../HRMS.DataAccess/TB_DIEUCHUYEN_NHANVIEN.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `SOQDDIEUCHUYEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAYDC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `MANV` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `MAPB` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `MAPB2` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `LYDODC` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `GHICHU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |

## TB_GIOITINH

Nguồn: [TB_GIOITINH.cs](../HRMS.DataAccess/TB_GIOITINH.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDGT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENGT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_HESO_TANGCA

Nguồn: [TB_HESO_TANGCA.cs](../HRMS.DataAccess/TB_HESO_TANGCA.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TEN_QUYDINH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `LOAICONG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IDLOAICONG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `LOAICA` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IDLOAICA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `GIO_BATDAU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `GIO_KETTHUC` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `HESO_CHINHTHUC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HESO_THUVIEC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `THUTU_UT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `GHICHU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_HOPDONG

Nguồn: [TB_HOPDONG.cs](../HRMS.DataAccess/TB_HOPDONG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `NGAYBATDAU` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `NGAYKETTHUC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `NGAYKY` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `LANKY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THOIHAN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `HESOLUONG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `MANV` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `SOHD` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IDCTY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DEL_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DEL_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATE_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATE_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `NOIDUNG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `LUONG_THOA_THUAN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `LOAIHD` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |

## TB_KHENTHUONG_KYLUAT

Nguồn: [TB_KHENTHUONG_KYLUAT.cs](../HRMS.DataAccess/TB_KHENTHUONG_KYLUAT.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `NOIDUNG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAY` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `MANV` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `LOAI` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `LYDO` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TUNGAY` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DENNGAY` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `SOQUYETDINH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SOTIEN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THANG_APDUNG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NAM_APDUNG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |

## TB_KYCONG

Nguồn: [TB_KYCONG.cs](../HRMS.DataAccess/TB_KYCONG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `MAKYCONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `THANG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NAM` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `KHOA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGAYTINHCONG` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `NGAYCONGTRONGTHANG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDCTY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TRANGTHAI` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `CONG_INPUT_REV` | `long` | Kiểu giá trị không nullable trong khai báo |
| `CONG_PUBLISH_REV` | `long` | Kiểu giá trị không nullable trong khai báo |

## TB_KYCONGCHITIET

Nguồn: [TB_KYCONGCHITIET.cs](../HRMS.DataAccess/TB_KYCONGCHITIET.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `MAKYCONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HOTEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D1` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D2` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D3` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D4` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D5` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D6` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D7` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D8` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D9` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D10` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D11` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D12` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D13` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D14` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D15` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D16` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D17` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D18` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D19` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D20` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D21` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D22` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D23` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D24` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D25` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D26` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D27` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D28` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D29` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D30` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `D31` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAYCONG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGAYPHEP` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGHIKHONGPHEP` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CONGNGAYLE` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CONGCHUNHAT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TONGNGAYCONG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `IDCTY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |

## TB_LANGUAGES

Nguồn: [TB_LANGUAGES.cs](../HRMS.DataAccess/TB_LANGUAGES.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `CODE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NAME` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IS_ACTIVE` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `CREATED_AT` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |

## TB_LICH_LAMVIEC

Nguồn: [TB_LICH_LAMVIEC.cs](../HRMS.DataAccess/TB_LICH_LAMVIEC.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDLICH` | `long` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `NGAY` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `MA_PHANCONG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SO_PHIENBAN` | `int` | Kiểu giá trị không nullable trong khai báo |
| `THAY_THE_ID` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `IDCAPHIENBAN` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `IDQUYDINH` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `BATDAU_KEHOACH` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `KETTHUC_KEHOACH` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `TRANG_THAI_PHAN_CONG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `LOAI_NGAY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGUON_PHAN_CONG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TAO_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `TAO_BOI` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `LY_DO` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_LOAICA

Nguồn: [TB_LOAICA.cs](../HRMS.DataAccess/TB_LOAICA.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDLOAICA` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENLOAICA` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `HESOLOAICA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |

## TB_LOAICONG

Nguồn: [TB_LOAICONG.cs](../HRMS.DataAccess/TB_LOAICONG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDLOAICONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENLC` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `HESOLOAICONG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |

## TB_LOAIHOPDONG

Nguồn: [TB_LOAIHOPDONG.cs](../HRMS.DataAccess/TB_LOAIHOPDONG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `LOAIHD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENLOAIHD` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |

## TB_NANGLUONG_NHANVIEN

Nguồn: [TB_NANGLUONG_NHANVIEN.cs](../HRMS.DataAccess/TB_NANGLUONG_NHANVIEN.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `SOQDNL` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SOHD` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MANV` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `HESOLUONG_NOW` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `HESOLUONG_NEW` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGAYLENLUONG` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `NGAYKYNL` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `GHICHUNL` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |

## TB_NGAYLE

Nguồn: [TB_NGAYLE.cs](../HRMS.DataAccess/TB_NGAYLE.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDLE` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENLE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAY` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `NAM` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `HESO` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |

## TB_NGUOI_PHU_THUOC

Nguồn: [TB_NGUOI_PHU_THUOC.cs](../HRMS.DataAccess/TB_NGUOI_PHU_THUOC.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HO_TEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MOI_QUAN_HE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CCCD` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MA_SO_THUE_NPT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAY_SINH` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `THANG_BAT_DAU_GIAM_TRU` | `int` | Kiểu giá trị không nullable trong khai báo |
| `THANG_KET_THUC_GIAM_TRU` | `Nullable<int>` | Cho phép null ở kiểu CLR |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_NHANVIEN

Nguồn: [TB_NHANVIEN.cs](../HRMS.DataAccess/TB_NHANVIEN.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HOTEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IDGT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGAYSINH` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DIENTHOAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CCCD` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DIACHI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `HINHANH` | `byte[]` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IDPB` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDBP` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDCV` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDTD` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDDT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDTG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDCTY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDQT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DATHOIVIEC` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `LOAI_NV` | `Nullable<int>` | Cho phép null ở kiểu CLR |
| `EMPLOYEE_CODE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_NHANVIEN_BAOHIEM_THAM_GIA

Nguồn: [TB_NHANVIEN_BAOHIEM_THAM_GIA.cs](../HRMS.DataAccess/TB_NHANVIEN_BAOHIEM_THAM_GIA.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `VUNG_LUONG` | `short` | Kiểu giá trị không nullable trong khai báo |
| `THAM_GIA_BHXH` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `THAM_GIA_BHYT` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `THAM_GIA_BHTN` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `THAM_GIA_TNLD_BNN` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `HUONG_TY_LE_TNLD_UU_DAI` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `LUONG_DONG_BHXH_RIENG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGAY_BAT_DAU` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `NGAY_KET_THUC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_NHANVIEN_CONG_DOAN_THAM_GIA

Nguồn: [TB_NHANVIEN_CONG_DOAN_THAM_GIA.cs](../HRMS.DataAccess/TB_NHANVIEN_CONG_DOAN_THAM_GIA.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `LA_DOAN_VIEN` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `NGAY_GIA_NHAP` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `NGAY_KET_THUC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_NHANVIEN_PHUCAP

Nguồn: [TB_NHANVIEN_PHUCAP.cs](../HRMS.DataAccess/TB_NHANVIEN_PHUCAP.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `IDPC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `GHICHU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SOTIEN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |

## TB_NHANVIEN_THOIVIEC

Nguồn: [TB_NHANVIEN_THOIVIEC.cs](../HRMS.DataAccess/TB_NHANVIEN_THOIVIEC.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `SOQDTV` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MANV` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGAYNOPDON` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `NGAYNGHIVIEC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `LYDOTV` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `GHICHUTV` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |

## TB_NHANVIEN_THUE

Nguồn: [TB_NHANVIEN_THUE.cs](../HRMS.DataAccess/TB_NHANVIEN_THUE.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MA_SO_THUE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IS_CU_TRU` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `CO_UY_QUYEN_QUYET_TOAN` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `NGAY_HIEU_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `NGAY_HET_HIEU_LUC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_PAYROLL_CALCULATION_RUN

Nguồn: [TB_PAYROLL_CALCULATION_RUN.cs](../HRMS.DataAccess/TB_PAYROLL_CALCULATION_RUN.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `RUN_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MAKYCONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `NAM` | `short` | Kiểu giá trị không nullable trong khai báo |
| `THANG` | `byte` | Kiểu giá trị không nullable trong khai báo |
| `EXECUTION_TYPE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TOTAL_EMPLOYEES` | `int` | Kiểu giá trị không nullable trong khai báo |
| `SUCCESS_COUNT` | `int` | Kiểu giá trị không nullable trong khai báo |
| `WARNING_COUNT` | `int` | Kiểu giá trị không nullable trong khai báo |
| `ERROR_COUNT` | `int` | Kiểu giá trị không nullable trong khai báo |
| `STATUS` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `ERROR_SUMMARY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `STARTED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `FINISHED_AT` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `EXECUTED_BY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_PHONGBAN

Nguồn: [TB_PHONGBAN.cs](../HRMS.DataAccess/TB_PHONGBAN.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDPB` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENPB` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_PHUCAP

Nguồn: [TB_PHUCAP.cs](../HRMS.DataAccess/TB_PHUCAP.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDPC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENPC` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_QUOCTICH

Nguồn: [TB_QUOCTICH.cs](../HRMS.DataAccess/TB_QUOCTICH.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDQT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENQT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_QUYDINH_CHAMCONG

Nguồn: [TB_QUYDINH_CHAMCONG.cs](../HRMS.DataAccess/TB_QUYDINH_CHAMCONG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDQUYDINH` | `long` | Kiểu giá trị không nullable trong khai báo |
| `MA_QUYDINH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SO_PHIENBAN` | `int` | Kiểu giá trị không nullable trong khai báo |
| `TEN_QUYDINH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TU_NGAY` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `DEN_NGAY` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `GIAY_MIEN_VIPHAM_MUON` | `int` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_MIEN_VIPHAM_SOM` | `int` | Kiểu giá trị không nullable trong khai báo |
| `GIAY_DUNG_SAI_HUONG_CONG` | `int` | Kiểu giá trị không nullable trong khai báo |
| `CACH_HUONG_DUNG_SAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CACH_DEM_MUON` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CUA_SO_VAO_TRUOC_GIAY` | `int` | Kiểu giá trị không nullable trong khai báo |
| `CUA_SO_RA_SAU_GIAY` | `int` | Kiểu giá trị không nullable trong khai báo |
| `GIOI_HAN_GHEP_GIAY` | `int` | Kiểu giá trị không nullable trong khai báo |
| `MUI_GIO` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DEM_BATDAU_PHUT` | `short` | Kiểu giá trị không nullable trong khai báo |
| `DEM_KETTHUC_PHUT` | `short` | Kiểu giá trị không nullable trong khai báo |
| `KIEU_LAM_TRON` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `BUOC_LAM_TRON_GIAY` | `int` | Kiểu giá trị không nullable trong khai báo |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TAO_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `TAO_BOI` | `decimal` | Kiểu giá trị không nullable trong khai báo |

## TB_QUYET_TOAN_THUE_NAM

Nguồn: [TB_QUYET_TOAN_THUE_NAM.cs](../HRMS.DataAccess/TB_QUYET_TOAN_THUE_NAM.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TAX_YEAR` | `short` | Kiểu giá trị không nullable trong khai báo |
| `POLICY_THUE_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `PROFILE_THUE_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `SO_THANG_LAM_VIEC` | `byte` | Kiểu giá trị không nullable trong khai báo |
| `TONG_THU_NHAP_CHIU_THUE` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `GIAM_TRU_BAN_THAN` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `GIAM_TRU_PHU_THUOC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `GIAM_TRU_BAO_HIEM` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `GIAM_TRU_KHAC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TONG_GIAM_TRU` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `THU_NHAP_TINH_THUE` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TONG_THUE_PHAI_NOP` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `THUE_DA_KHAU_TRU` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `THUE_CON_PHAI_NOP` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `THUE_NOP_THUA` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_QUYET_TOAN_THUE_NAM_CT

Nguồn: [TB_QUYET_TOAN_THUE_NAM_CT.cs](../HRMS.DataAccess/TB_QUYET_TOAN_THUE_NAM_CT.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `QUYET_TOAN_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TAX_YEAR` | `short` | Kiểu giá trị không nullable trong khai báo |
| `BAC_THUE` | `byte` | Kiểu giá trị không nullable trong khai báo |
| `CAN_DUOI` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `CAN_TREN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THU_NHAP_CHIU_THUE_BAC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `THUE_SUAT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TIEN_THUE_BAC` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_SYS_FUNCTION

Nguồn: [TB_SYS_FUNCTION.cs](../HRMS.DataAccess/TB_SYS_FUNCTION.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `FUNCTION_CODE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SORT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `DESCRIPTION` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `ISGROUP` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `PARENT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MENU` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TIPS` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_SYS_GROUP

Nguồn: [TB_SYS_GROUP.cs](../HRMS.DataAccess/TB_SYS_GROUP.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID_GROUP` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MEMBER` | `decimal` | Kiểu giá trị không nullable trong khai báo |

## TB_SYS_LOG

Nguồn: [TB_SYS_LOG.cs](../HRMS.DataAccess/TB_SYS_LOG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID_LOG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV_THUCHIEN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TEN_THUCHIEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `HANHDONG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_BANG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `ID_BAN_GHI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DU_LIEU_CU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DU_LIEU_MOI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IP_ADDRESS` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MAC_ADDRESS` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_MAY_TINH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `THOIGIAN` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `SESSION_ID` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MODULE_NAME` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `APP_VERSION` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CHANGED_FIELDS` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_SYS_LOGIN_HISTORY

Nguồn: [TB_SYS_LOGIN_HISTORY.cs](../HRMS.DataAccess/TB_SYS_LOGIN_HISTORY.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID_LOGIN` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `ID_USER` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THOIGIAN` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `IP_ADDRESS` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MAC_ADDRESS` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_MAY_TINH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TRANGTHAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `THOIGIAN_DANGXUAT` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |

## TB_SYS_REPORT

Nguồn: [TB_SYS_REPORT.cs](../HRMS.DataAccess/TB_SYS_REPORT.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `REP_CODE` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `DESCRIPTION` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `REP_NAME` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `VISIBLED` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TUNGAY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THANG_NAM` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `MACTY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `MADVI` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |

## TB_SYS_RIGHT

Nguồn: [TB_SYS_RIGHT.cs](../HRMS.DataAccess/TB_SYS_RIGHT.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `FUNCTION_CODE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IDUSER` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `USER_RIGHT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CAN_VIEW` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CAN_ADD` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CAN_EDIT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CAN_DELETE` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CAN_PRINT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |

## TB_SYS_RIGHT_BACKUP

Nguồn: [TB_SYS_RIGHT_BACKUP.cs](../HRMS.DataAccess/TB_SYS_RIGHT_BACKUP.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `FUNCTION_CODE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IDUSER` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `USER_RIGHT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |

## TB_SYS_RIGHT_REPORT

Nguồn: [TB_SYS_RIGHT_REPORT.cs](../HRMS.DataAccess/TB_SYS_RIGHT_REPORT.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `REP_CODE` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `IDUSER` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `USER_RIGHT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |

## TB_SYS_USER

Nguồn: [TB_SYS_USER.cs](../HRMS.DataAccess/TB_SYS_USER.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDUSER` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `USERNAME` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `PASSWORD` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `FULLNAME` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MACTY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MADVI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `LAST_PWD_CHANGED` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DISABLED` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `ISGROUP` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `ALLOWED_IPS` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `FAILED_LOGIN_COUNT` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `LOCKOUT_END` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `MANV` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CLIENT_TYPE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TOKEN_VERSION` | `int` | Kiểu giá trị không nullable trong khai báo |
| `FIRST_FAILED_LOGIN_AT` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `LAST_FAILED_LOGIN_AT` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `LAST_SUCCESS_LOGIN_AT` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `LOCK_REASON` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_TANGCA

Nguồn: [TB_TANGCA.cs](../HRMS.DataAccess/TB_TANGCA.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDTCA` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `NAM` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THANG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGAY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `SOGIO` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `MANV` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDLOAICA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `GHICHU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SOTIENTC` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `GIOBATDAU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `GIOKETTHUC` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IDLOAICONG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `HESOTC` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DONGIATC` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IS_THUVIEC` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `IDLOAITANGCA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `OT_REQUEST_ID` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |

## TB_THONGBAO

Nguồn: [TB_THONGBAO.cs](../HRMS.DataAccess/TB_THONGBAO.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TIEUDE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NOIDUNG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGUOIDANG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAYDANG` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `LOAI_TB` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `IS_PINNED` | `Nullable<bool>` | Cho phép null ở kiểu CLR |
| `TRANGTHAI` | `Nullable<bool>` | Cho phép null ở kiểu CLR |
| `NGAY_HETHAN` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `FILE_DINHKEM` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MACTY` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `MAPB` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_THUE_TNCN_BAC

Nguồn: [TB_THUE_TNCN_BAC.cs](../HRMS.DataAccess/TB_THUE_TNCN_BAC.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `POLICY_THUE_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TAX_YEAR` | `short` | Kiểu giá trị không nullable trong khai báo |
| `PERIOD_TYPE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `BAC_THUE` | `byte` | Kiểu giá trị không nullable trong khai báo |
| `CAN_DUOI` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `CAN_TREN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THUE_SUAT` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_THUE_TNCN_CHINH_SACH

Nguồn: [TB_THUE_TNCN_CHINH_SACH.cs](../HRMS.DataAccess/TB_THUE_TNCN_CHINH_SACH.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MA_CHINH_SACH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_CHINH_SACH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TAX_YEAR` | `short` | Kiểu giá trị không nullable trong khai báo |
| `NGAY_HIEU_LUC` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `NGAY_HET_HIEU_LUC` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `GIAM_TRU_BAN_THAN_THANG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `GIAM_TRU_PHU_THUOC_THANG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `GIAM_TRU_BAN_THAN_NAM` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `GIAM_TRU_PHU_THUOC_NAM` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TRANG_THAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_AT` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |

## TB_TONGIAO

Nguồn: [TB_TONGIAO.cs](../HRMS.DataAccess/TB_TONGIAO.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDTG` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENTG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_TRANSLATIONS

Nguồn: [TB_TRANSLATIONS.cs](../HRMS.DataAccess/TB_TRANSLATIONS.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `TABLE_NAME` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `RECORD_ID` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `COLUMN_NAME` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `LANGUAGE_CODE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `VALUE` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DESCRIPTION` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_TRINHDO

Nguồn: [TB_TRINHDO.cs](../HRMS.DataAccess/TB_TRINHDO.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDTD` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `TENTD` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_UNGLUONG

Nguồn: [TB_UNGLUONG.cs](../HRMS.DataAccess/TB_UNGLUONG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `IDUL` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `NAM` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THANG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGAY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `SOTIENUNG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `MANV` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `GHICHU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `UPDATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DELETED_BY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `DELETED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |

## TB_USER_EMPLOYEE_MAPPING

Nguồn: [TB_USER_EMPLOYEE_MAPPING.cs](../HRMS.DataAccess/TB_USER_EMPLOYEE_MAPPING.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `USER_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `EMPLOYEE_ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `IS_MOBILE_ENABLED` | `bool` | Kiểu giá trị không nullable trong khai báo |
| `CREATED_AT` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `UPDATED_AT` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |

## TB_YEUCAU_DIEUCHINHCONG

Nguồn: [TB_YEUCAU_DIEUCHINHCONG.cs](../HRMS.DataAccess/TB_YEUCAU_DIEUCHINHCONG.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `NGAY` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `LOAIDIEUCHINH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `GIO_VAO` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `GIO_RA` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `LYDO` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TRANGTHAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGUOIDUYET` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGAYDUYET` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `GHICHUDUYET` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `THOIDIEM_VAO_CU` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `THOIDIEM_RA_CU` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `THOIDIEM_VAO_MOI` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `THOIDIEM_RA_MOI` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `IDBANGCONG_GOC` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGUON_HASH_CU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_YEUCAU_NGHIPHEP

Nguồn: [TB_YEUCAU_NGHIPHEP.cs](../HRMS.DataAccess/TB_YEUCAU_NGHIPHEP.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `LOAIPHEP` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TUNGAY` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `DENNGAY` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `SONGAY` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `LYDO` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TRANGTHAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGUOIDUYET` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGAYDUYET` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `GHICHUDUYET` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `BATDAU_NGHI` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `KETTHUC_NGHI` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `GIAY_NGHI` | `Nullable<long>` | Cho phép null ở kiểu CLR |
| `NGUON_CHI_TRA` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `LOAI_HUONG_CONG` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## TB_YEUCAU_TANGCA

Nguồn: [TB_YEUCAU_TANGCA.cs](../HRMS.DataAccess/TB_YEUCAU_TANGCA.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `ID` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `NGAY` | `System.DateTime` | Kiểu giá trị không nullable trong khai báo |
| `GIOTANGCA` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `IDCA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `LYDO` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TRANGTHAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGUOIDUYET` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NGAYDUYET` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `GHICHUDUYET` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `CREATED_DATE` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `BATDAU_DANGKY` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `KETTHUC_DANGKY` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `BATDAU_DUYET` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `KETTHUC_DUYET` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `GIAY_OT_DUYET` | `Nullable<long>` | Cho phép null ở kiểu CLR |

## V_AI_ADVANCE

Nguồn: [V_AI_ADVANCE.cs](../HRMS.DataAccess/V_AI_ADVANCE.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HOTEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THANG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NAM` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `SOTIEN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |

## V_AI_ALLOWANCE

Nguồn: [V_AI_ALLOWANCE.cs](../HRMS.DataAccess/V_AI_ALLOWANCE.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HOTEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TENPC` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SOTIEN` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `KYCONG` | `decimal` | Kiểu giá trị không nullable trong khai báo |

## V_AI_ATTENDANCE

Nguồn: [V_AI_ATTENDANCE.cs](../HRMS.DataAccess/V_AI_ATTENDANCE.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HOTEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_PHONGBAN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THANG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NAM` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `GIOVAO` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `PHUTVAO` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `GIORA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `PHUTRA` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `TIME_IN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TIME_OUT` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## V_AI_EMPLOYEE

Nguồn: [V_AI_EMPLOYEE.cs](../HRMS.DataAccess/V_AI_EMPLOYEE.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HOTEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAYSINH` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `DIENTHOAI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `DIACHI` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_PHONGBAN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_BOPHAN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_CHUCVU` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## V_AI_INSURANCE

Nguồn: [V_AI_INSURANCE.cs](../HRMS.DataAccess/V_AI_INSURANCE.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HOTEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `SOBH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAYCAP` | `Nullable<System.DateTime>` | Cho phép null ở kiểu CLR |
| `NOICAP` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NOIKHAMBENH` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |

## V_AI_OVERTIME

Nguồn: [V_AI_OVERTIME.cs](../HRMS.DataAccess/V_AI_OVERTIME.cs).

| Thuộc tính | Kiểu CLR trong model | Ghi chú |
| --- | --- | --- |
| `MANV` | `decimal` | Kiểu giá trị không nullable trong khai báo |
| `HOTEN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `TEN_PHONGBAN` | `string` | Kiểu tham chiếu; chưa suy ràng buộc DB |
| `NGAY` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `THANG` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `NAM` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
| `SOGIO` | `Nullable<decimal>` | Cho phép null ở kiểu CLR |
