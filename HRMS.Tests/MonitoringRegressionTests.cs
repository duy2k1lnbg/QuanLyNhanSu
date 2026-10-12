using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Collections.Specialized;
using DA;
using Bu.CLASS_SYSTEM;
using Bu.CLASS_SECURITY;
using HRMS_API.Services;
using NUnit.Framework;
namespace HRMS.Tests
{
    [TestFixture,NonParallelizable]
    public class MonitoringRegressionTests
    {
        [Test] public async Task ConcurrentUsersDoNotOverwriteEachOthersAuditIdentity()
        {
            var tasks=Enumerable.Range(1,32).Select(async id=> {
                MyEntities.CurrentAuditUserId=id;MyEntities.CurrentAuditUsername="user-"+id;MyEntities.CurrentSessionId="session-"+id;
                await Task.Yield();await Task.Delay(2);
                Assert.That(MyEntities.CurrentAuditUserId,Is.EqualTo(id));
                Assert.That(MyEntities.CurrentAuditUsername,Is.EqualTo("user-"+id));
                Assert.That(MyEntities.CurrentSessionId,Is.EqualTo("session-"+id));
            });
            await Task.WhenAll(tasks);
        }
        [Test] public void ContextKeepsItsActorWhenTheDesktopSessionChanges()
        {
            var previous=MyEntities.AuditIdentityProvider;
            var identity=new AuditIdentity {UserId=80,Username="first",SessionId="original"};
            try {
                MyEntities.AuditIdentityProvider=()=>identity;
                using(var db=new MyEntities()) {
                    identity.UserId=81;identity.Username="second";identity.SessionId="changed";
                    Assert.That(db.AuditContext.UserId,Is.EqualTo(80));
                    Assert.That(db.AuditContext.SessionId,Is.EqualTo("original"));
                }
            } finally {MyEntities.AuditIdentityProvider=previous;}
        }
        [Test] public void NestedLegacySecretsAreRedactedWithoutDiscardingBusinessChanges()
        {
            var value=AuditIdentity.RedactJson("{\"PASSWORD\":\"unsafe\",\"detail\":[{\"DEVICE_TOKEN\":\"private\",\"SALARY\":1200}],\"CCCD\":\"123456\",\"STATUS\":{\"Old\":\"PENDING\",\"New\":\"APPROVED\"}}");
            Assert.That(value,Does.Not.Contain("unsafe").And.Not.Contain("private").And.Not.Contain("123456"));
            Assert.That(value,Does.Contain("APPROVED").And.Contain("1200"));
        }
        [TestCase("PASSWORD")][TestCase("ACCESS_TOKEN")][TestCase("MA_SO_THUE")][TestCase("SO_HO_CHIEU")]
        public void SensitiveFieldsAreNeverIncludedInSnapshots(string name) => Assert.That(AuditIdentity.IsSensitive(name),Is.True);
        [Test] public void UnknownChannelIsNotGuessedFromAnIpOrMachineName()
        {
            Assert.That(MonitoringService.NormalizeChannel("NGUYENDUY"),Is.Null);
            Assert.That(MonitoringService.NormalizeChannel("web"),Is.EqualTo("WEB"));
            Assert.That(MonitoringService.NormalizeChannel("UNKNOWN"),Is.EqualTo("UNKNOWN"));
        }
        [Test] public void ClientCannotChooseAnArbitraryDatabaseSource() => Assert.That(MonitoringService.NormalizeKind("TB_SYS_USER"),Is.EqualTo("activity"));
        [Test] public void ForgedForwardedHeaderFromAnUntrustedPeerIsIgnored()
        {
            Assert.That(AuditRequestContext.ClientIp(new Context()),Is.EqualTo("192.0.2.20"));
        }
        [Test] public void MissingConnectionAddressIsNotInventedAsLoopback() => Assert.That(AuditRequestContext.ClientIp(null),Is.Null);
        [Test] public void MonitoringAndPrintCapabilitiesDoNotGrantOtherPayrollActions()
        {
            Assert.That(ChannelCapabilityRegistry.IsActionSupported("WEB","F_CC_BANGLUONG",ChannelAction.Print),Is.True);
            Assert.That(ChannelCapabilityRegistry.IsActionSupported("WEB","F_CC_BANGLUONG",ChannelAction.Edit),Is.False);
            Assert.That(ChannelCapabilityRegistry.IsActionSupported("WEB","F_SYSTEM_GIAMSAT",ChannelAction.Edit),Is.True);
            Assert.That(ChannelCapabilityRegistry.IsActionSupported("MOBILE","F_SYSTEM_GIAMSAT",ChannelAction.View),Is.False);
        }
        private class Context:HttpContextBase {public override HttpRequestBase Request=>new Request();}
        private class Request:HttpRequestBase {
            public override string UserHostAddress=>"192.0.2.20";
            public override NameValueCollection Headers=>new NameValueCollection {{"X-Forwarded-For","198.51.100.40"}};
        }
    }
}
