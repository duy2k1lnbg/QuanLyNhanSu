using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Bu.CLASS_SECURITY;
using Bu.CLASS_SYSTEM;
using DA;
using NUnit.Framework;
using QLyNSu.Functions;
using QLyNSu.FORM_SYSTEM;

[TestFixture, NonParallelizable]
public class DesktopAuthRegressionTests
{
    readonly List<AiApiClient> clients = new List<AiApiClient>();
    void Login()
    {
        UserSession.Clear();
        UserSession.CurrentUser = new TB_SYS_USER { IDUSER = 42, USERNAME = "employee", PASSWORD = null };
        UserSession.CurrentSessionId = "oracle-session-42";
        UserSession.CurrentJti = "oracle-jti-42";
    }
    [SetUp] public void SetUp() { Login(); }
    [TearDown] public void TearDown()
    {
        UserSession.Clear();
        foreach (var client in clients) UserSession.SessionCleared -= client.ClearSession;
        clients.Clear();
    }
    class Handler : HttpMessageHandler
    {
        public int AuthCalls, AiCalls;
        public string AuthBody;
        public bool Reject;
        public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>();
        public TaskCompletionSource<bool> Release;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (request.RequestUri.AbsolutePath.EndsWith("/desktop-token"))
            {
                Interlocked.Increment(ref AuthCalls);
                AuthBody = await request.Content.ReadAsStringAsync();
                Started.TrySetResult(true);
                if (Release != null) await Release.Task;
                return new HttpResponseMessage(Reject ? HttpStatusCode.Unauthorized : HttpStatusCode.OK)
                    { Content = new StringContent("{\"token\":\"mock-token\"}") };
            }
            Assert.That(request.RequestUri.AbsolutePath, Does.Not.Contain("desktop-login"), "Must reuse the Oracle session, not create a second login.");
            Assert.That(request.Headers.Authorization?.Parameter, Is.EqualTo("mock-token"));
            Interlocked.Increment(ref AiCalls);
            return new HttpResponseMessage(HttpStatusCode.OK)
                { Content = new StringContent("{\"isOnline\":true,\"llmAvailable\":true,\"queryReady\":true}") };
        }
    }
    AiApiClient Client(Handler handler, bool prepare = true)
    {
        var client = new AiApiClient(new HttpClient(handler));
        clients.Add(client);
        if (prepare) client.SetDesktopCredentials("employee", "correct-password", UserSession.CurrentSessionId, UserSession.CurrentJti);
        return client;
    }
    [Test] public async Task LoginPreparationHasNoHttp_FirstAiCallReusesOracleSession()
    {
        var handler = new Handler(); var client = Client(handler);
        Assert.That(handler.AuthCalls + handler.AiCalls, Is.Zero);
        Assert.That(client.HasToken, Is.False);
        Assert.That((await client.GetStatusAsync()).Connected, Is.True);
        Assert.That(handler.AuthCalls, Is.EqualTo(1)); Assert.That(handler.AiCalls, Is.EqualTo(1));
        var body = Newtonsoft.Json.Linq.JObject.Parse(handler.AuthBody);
        Assert.That((string)body["sessionId"], Is.EqualTo("oracle-session-42"));
        Assert.That((string)body["jti"], Is.EqualTo("oracle-jti-42"));
        Assert.That((string)body["username"], Is.EqualTo("employee"));
    }
    [Test] public async Task ConcurrentAiOperationsExchangeOneToken()
    {
        var handler = new Handler(); var client = Client(handler);
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 20; i++) tasks.Add(client.EnsureAuthenticatedAsync());
        Assert.That(await Task.WhenAll(tasks), Is.All.True);
        Assert.That(handler.AuthCalls, Is.EqualTo(1));
        await client.GetStatusAsync(); await client.GetStatusAsync();
        Assert.That(handler.AuthCalls, Is.EqualTo(1));
    }
    [Test] public async Task LogoutDiscardsAnInFlightTokenAndCredentials()
    {
        var handler = new Handler { Release = new TaskCompletionSource<bool>() }; var client = Client(handler);
        var pending = client.EnsureAuthenticatedAsync();
        await handler.Started.Task;
        UserSession.Clear(); handler.Release.SetResult(true);
        Assert.That(await pending, Is.False);
        Assert.That(client.HasToken, Is.False); Assert.That(UserSession.CurrentUser, Is.Null);
        Assert.That(await client.EnsureAuthenticatedAsync(), Is.False);
        Assert.That(handler.AuthCalls, Is.EqualTo(1));
    }
    [Test] public async Task DeniedExchangeDoesNotSendAnonymousAiOrCreateNewSession()
    {
        var handler = new Handler { Reject = true }; var client = Client(handler);
        Assert.That((await client.GetStatusAsync()).Connected, Is.False);
        Assert.That(handler.AuthCalls, Is.EqualTo(1)); Assert.That(handler.AiCalls, Is.Zero);
        Assert.That(UserSession.CurrentSessionId, Is.EqualTo("oracle-session-42"));
        Assert.That(client.HasToken, Is.False);
    }
    [Test] public async Task MissingCredentialsDoNotCallApi()
    {
        var handler = new Handler(); var client = Client(handler, false);
        Assert.That(await client.EnsureAuthenticatedAsync(), Is.False);
        Assert.That(handler.AuthCalls + handler.AiCalls, Is.Zero);
    }
    [Test] public async Task ReplacedOracleSessionCannotReuseDeferredCredential()
    {
        var handler = new Handler(); var client = Client(handler);
        UserSession.CurrentSessionId = "different-session";
        Assert.That(await client.EnsureAuthenticatedAsync(), Is.False);
        Assert.That(handler.AuthCalls, Is.Zero);
    }
    class PasswordService : IAuthSecurityService
    {
        public int Calls; public decimal UserId; public string OldPassword, NewPassword;
        readonly string databaseHash = PasswordHasher.HashPassword("correct-password");
        public Task<ChangePasswordResultDto> ChangePasswordWithRevocationAsync(decimal id, string oldPwd, string newPwd, string ip, string agent, string correlation)
        {
            Calls++; UserId = id; OldPassword = oldPwd; NewPassword = newPwd;
            Assert.That(agent, Is.EqualTo("DESKTOP_WINFORMS")); Assert.That(correlation, Is.Not.Empty);
            return Task.FromResult(new ChangePasswordResultDto { Success = PasswordHasher.VerifyPassword(oldPwd, databaseHash) });
        }
        public Task<LoginResultDto> AuthenticateAsync(string a,string b,string c,string d,string e,string f,string g,string h,string i) { throw new NotSupportedException(); }
        public Task<bool> ValidateSessionAsync(string a,long b,decimal c) { throw new NotSupportedException(); }
        public Task<bool> ValidateSessionAsync(string a,long b,decimal c,string d) { throw new NotSupportedException(); }
        public bool ValidateSession(string a,long b,decimal c) { throw new NotSupportedException(); }
        public bool ValidateSession(string a,long b,decimal c,string d) { throw new NotSupportedException(); }
        public Task<bool> RevokeSessionAsync(string a,string b,decimal c,string d) { throw new NotSupportedException(); }
        public Task<int> RevokeAllSessionsAsync(decimal a,string b,bool c,decimal d,string e) { throw new NotSupportedException(); }
        public Task<int> RevokePlatformSessionsAsync(decimal a,string b,string c,decimal d,string e) { throw new NotSupportedException(); }
        public Task<List<SessionInfoDto>> GetUserSessionsAsync(decimal a,string b) { throw new NotSupportedException(); }
        public Task<bool> UnlockUserAsync(decimal a,decimal b,string c) { throw new NotSupportedException(); }
    }
    static ChangePasswordResultDto Submit(FrmChangePassword form, string password)
    {
        var method = typeof(FrmChangePassword).GetMethod("ChangePasswordAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        return ((Task<ChangePasswordResultDto>)method.Invoke(form, new object[] { password, "new-password" })).GetAwaiter().GetResult();
    }
    [Test, Apartment(ApartmentState.STA)] public void CorrectPasswordWorksWithPasswordFreeLoginDto()
    {
        var service = new PasswordService();
        using (var form = new FrmChangePassword(service))
        {
            Assert.That(UserSession.CurrentUser.PASSWORD, Is.Null);
            Assert.That(Submit(form, "correct-password").Success, Is.True);
            Assert.That(service.UserId, Is.EqualTo(42)); Assert.That(service.Calls, Is.EqualTo(1));
            Assert.That(service.OldPassword, Is.EqualTo("correct-password"));
            Assert.That(service.NewPassword, Is.EqualTo("new-password"));
            Assert.That(UserSession.CurrentUser.PASSWORD, Is.Null, "Never overwrite a partial session entity in Oracle.");
        }
    }
    [Test, Apartment(ApartmentState.STA)] public void IncorrectPasswordStillRejectedByPasswordService()
    {
        var service = new PasswordService();
        using (var form = new FrmChangePassword(service)) Assert.That(Submit(form, "incorrect-password").Success, Is.False);
        Assert.That(service.Calls, Is.EqualTo(1));
    }
    [Test, Apartment(ApartmentState.STA)] public void LoggedOutPasswordChangeDoesNotTouchDatabase()
    {
        UserSession.Clear(); var service = new PasswordService();
        using (var form = new FrmChangePassword(service)) Assert.That(Submit(form, "correct-password").Success, Is.False);
        Assert.That(service.Calls, Is.Zero);
    }
}
