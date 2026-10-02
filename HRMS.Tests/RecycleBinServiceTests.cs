using Bu;
using NUnit.Framework;

namespace Bu.Tests
{
    [TestFixture]
    public class RecycleBinServiceTests
    {
        [Test]
        public void Restore_InvalidObjectType_ReturnsFailure()
        {
            var service = new RecycleBinService();
            var res = service.Restore("INVALID_TYPE", "123", 1);
            Assert.IsFalse(res.Success);
            StringAssert.Contains("chưa được hỗ trợ", res.Message);
        }

        [Test]
        public void Purge_NonContractObject_ReturnsFailure()
        {
            var service = new RecycleBinService();
            var res = service.Purge("PHUCAP", "1_2", 1);
            Assert.IsFalse(res.Success);
            StringAssert.Contains("Chỉ cho phép xóa vĩnh viễn hợp đồng", res.Message);
        }

        [Test]
        public void Purge_EmptyId_ReturnsFailure()
        {
            var service = new RecycleBinService();
            var res = service.Purge("HOPDONG", "", 1);
            Assert.IsFalse(res.Success);
        }
    }
}
