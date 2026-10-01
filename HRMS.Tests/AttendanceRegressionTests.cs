using System;
using System.Collections.Generic;
using System.Linq;
using Bu.CLASS_CHAMCONG;
using NUnit.Framework;
namespace HRMS.Tests {
 [TestFixture]
 public class AttendanceRegressionTests {
  private DayCalculationInput Input() {
   var day=new DateTime(2026,9,21);
   return new DayCalculationInput {
    Ngay=day,MaNV=1,MaKyCong=202609,IdBangCongCt=1,
    Schedule=new ScheduleDto {IdLich=1,Ngay=day,MaNV=1,BatDauKeHoach=day.AddHours(8),KetThucKeHoach=day.AddHours(17)},
    Shift=new ShiftVersionDto {TongGiayChuan=28800,CongQuyDoi=1,KhungGios=new List<ShiftFrameDto>{
     new ShiftFrameDto{Stt=1,BatDauPhut=480,KetThucPhut=720,LoaiKhungGio="LAM_VIEC"},
     new ShiftFrameDto{Stt=2,BatDauPhut=720,KetThucPhut=780,LoaiKhungGio="NGHI_KHONG_LUONG"},
     new ShiftFrameDto{Stt=3,BatDauPhut=780,KetThucPhut=1020,LoaiKhungGio="LAM_VIEC"}}},
    Policy=new AttendancePolicyDto{IdQuyDinh=1,GiayMienViPhamMuon=300,CachDemMuon="PHAN_VUOT_NGUONG"},
    RawPunch=new RawPunchDto{Mabc=1,ThoiDiemVao=day.AddHours(8),ThoiDiemRa=day.AddHours(17)}
   };
  }
  [Test] public void MissingScheduleIsNotConfirmedRest() {
   var i=Input();i.Schedule=null;i.RawPunch=null;
   var r=new TimeSegmentationEngine().ProcessDay(i);
   Assert.AreEqual(0,r.DuDieuKienChot);Assert.IsTrue(r.Anomalies.Any(x=>x.MaLoi=="THIEU_CAU_HINH"));
  }
  [Test] public void MissingShiftCannotPublishConfirmedZero() {
   var i=Input();i.Shift=null;Assert.AreEqual(0,new TimeSegmentationEngine().ProcessDay(i).DuDieuKienChot);
  }
  [Test] public void ExcessOnlyLatePenaltyExcludesGrace() {
   var i=Input();i.RawPunch.ThoiDiemVao=i.Ngay.AddHours(8).AddMinutes(20);
   var r=new TimeSegmentationEngine().ProcessDay(i);
   Assert.AreEqual(1200,r.GiayDiMuonThucTe);Assert.AreEqual(900,r.GiayDiMuonViPham);
  }
  [Test] public void BhxhPayerDoesNotBecomeCompanyPaidLeave() {
   var i=Input();i.RawPunch=null;
   i.ApprovedLeaves.Add(new ApprovedLeaveDto{Id=1,BatDauNghi=i.Ngay.AddHours(8),KetThucNghi=i.Ngay.AddHours(17),CoHuongLuong=1,LoaiHuongCong="NGHI_BHXH",NguonChiTra="BHXH"});
   var r=new TimeSegmentationEngine().ProcessDay(i);
   Assert.AreEqual(0,r.GiayHuongCongThuong);Assert.IsTrue(r.Segments.Any(x=>x.LoaiThoiGian=="NGHI_BHXH"));
  }
  [Test] public void MultiDayLeaveOnlyCreditsThisDaysFrames() {
   var i=Input();i.RawPunch=null;i.ApprovedLeaves.Add(new ApprovedLeaveDto{Id=1,BatDauNghi=i.Ngay.AddDays(-1).AddHours(8),KetThucNghi=i.Ngay.AddDays(1).AddHours(17)});
   var r=new TimeSegmentationEngine().ProcessDay(i);
   Assert.AreEqual(28800,r.GiayHuongCongThuong);Assert.IsTrue(r.Segments.All(x=>x.BatDauLuc>=i.Ngay && x.KetThucLuc<=i.Ngay.AddDays(1)));
  }
  [Test] public void ApprovedOtCapSplitsConfirmedAndUnresolvedTime() {
   var i=Input();i.RawPunch.ThoiDiemRa=i.Ngay.AddHours(19);
   i.ApprovedOvertimes.Add(new ApprovedOvertimeDto{Id=1,BatDauDuyet=i.Ngay.AddHours(17),KetThucDuyet=i.Ngay.AddHours(19),GiayOtDuyet=3600});
   var r=new TimeSegmentationEngine().ProcessDay(i);
   Assert.AreEqual(3600,r.GiayOtXacNhan);Assert.AreEqual(0,r.DuDieuKienChot);
   foreach(var s in r.Segments){
    Assert.AreEqual((long)(s.KetThucLuc-s.BatDauLuc).TotalSeconds,s.ThoiLuongGiay);
    if(s.TrangThaiXacNhan!="DA_XAC_NHAN") Assert.AreEqual(0,s.GiayOtXacNhan+s.GiayHuongCongThuong+s.GiayDemOt);
   }
  }
  [Test] public void InvalidPunchAnomalyStillSatisfiesOracleIntervalCheck() {
   var i=Input();i.RawPunch.ThoiDiemRa=i.Ngay.AddHours(7);
   var r=new TimeSegmentationEngine().ProcessDay(i);
   Assert.AreEqual(0,r.DuDieuKienChot);
   Assert.IsTrue(r.Anomalies.All(x=>(!x.BatDauLuc.HasValue&&!x.KetThucLuc.HasValue)||(x.BatDauLuc.HasValue&&x.KetThucLuc.HasValue&&x.KetThucLuc>x.BatDauLuc)));
  }
  [Test] public void UnsupportedRoundingIsBlockedExplicitly() {
   var i=Input();i.Policy.KieuLamTron="CEILING";i.Policy.BuocLamTronGiay=900;
   Assert.AreEqual(0,new TimeSegmentationEngine().ProcessDay(i).DuDieuKienChot);
  }
  [Test] public void PublishAttendance_202601_VerifiesNoQuestionMarks() {
   var svc = new AttendancePublishingService();
   var res = svc.PublishAttendance(new AttendancePublishRequest {
    MaKyCong = 202601,
    NguoiThucHien = 1,
    GhiChu = "UAT test 202601 no question mark",
    ForceRecalculate = true
   });
   Assert.IsTrue(res.Success, res.ErrorMessage);
   Assert.AreEqual(194, res.SoNhanVien);
  }
  [Test] public void CalculatePayroll_202601_VerifiesSuccess() {
   var blBus = new BANGLUONG();
   var run = blBus.TinhLuongKyCong(202601, 1);
   Assert.IsNotNull(run);
   Assert.AreEqual("SUCCESS", run.STATUS);
   var list = blBus.getList(202601);
   Assert.IsNotNull(list);
   Assert.AreEqual(194, list.Count);
   Assert.IsTrue(list.All(x => x.THUC_LINH > 0));
  }
  [Test] public void PhatSinhToanBoKyCongVaBangCong_202603_Succeeds() {
   var kcctBus = new KYCONGCHITIET();
   kcctBus.PhatSinhToanBoKyCongVaBangCong(1, 3, 2026, 1, null);
   var list = kcctBus.getList(202603);
   Assert.IsNotNull(list);
   Assert.AreEqual(194, list.Count);
   Assert.IsTrue(list.All(x => x.D1 != "?"));
  }
 }
}