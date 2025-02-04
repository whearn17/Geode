using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using System.IO;

namespace geode;

public static class GmailAuthService
{
    private static readonly string[] Scopes = { GmailService.Scope.GmailReadonly };
    private const string ApplicationName = "Geode Email Client";

    public static async Task<GmailService> AuthenticateAsync(string credentialsPath)
    {
        if (string.IsNullOrEmpty(credentialsPath) || !File.Exists(credentialsPath))
        {
            throw new FileNotFoundException("Credentials file not found", credentialsPath);
        }

        try
        {
            return await InitializeGmailServiceAsync(credentialsPath);
        }
        catch (Exception ex)
        {
            throw new Exception($"Authentication failed: {ex.Message}", ex);
        }
    }

    private static async Task<GmailService> InitializeGmailServiceAsync(string credentialsPath)
    {
        UserCredential credential;

        using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
        {
            credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.FromStream(stream).Secrets,
                Scopes,
                "user",
                CancellationToken.None,
                new FileDataStore("Geode.OAuth.Store", true)
            );
        }

        return new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName
        });
    }

    public static async Task<string> GetUserEmailAsync(GmailService service)
    {
        try
        {
            var userProfile = await service.Users.GetProfile("me").ExecuteAsync();
            return userProfile.EmailAddress;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get user profile: {ex.Message}", ex);
        }
    }
}