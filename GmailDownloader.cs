using System.IO;
using System.Text;
using Google.Apis.Gmail.v1;

namespace geode;

public static class GmailDownloadService
{
    public static async Task DownloadEmailsBulkAsync(GmailService gmailService, string userEmail, string[] rfcMessageIds, string outputPath)
    {
        foreach (string rfcMessageId in rfcMessageIds)
        {
            await DownloadSingleEmailAsync(gmailService, userEmail, rfcMessageId, outputPath);
        }
    }

    public static async Task DownloadSingleEmailAsync(GmailService gmailService, string userEmail, string rfcMessageId, string outputPath)
    {

        var gmailMessageId = await GetGmailMessageIdAsync(gmailService, userEmail, rfcMessageId);

        var request = gmailService.Users.Messages.Get(userEmail, gmailMessageId);
        request.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Raw;
        var message = await request.ExecuteAsync();

        var rawMessage = b64.UrlDecode(message.Raw);

        await SaveRawMessage(rawMessage, rfcMessageId, outputPath);
    }

    private static async Task<string> GetGmailMessageIdAsync(GmailService gmailService, string userEmail, string rfcMessageId)
    {

        var query = $"rfc822msgid:{rfcMessageId}";

        var listRequest = gmailService.Users.Messages.List(userEmail);
        listRequest.Q = query;
        listRequest.MaxResults = 1; // InternetMessageIDs should be globally unique

        var response = await listRequest.ExecuteAsync();

        if (response.Messages == null || !response.Messages.Any())
        {
            throw new Exception($"Message with RFC ID {rfcMessageId} not found.");
        }

        return response.Messages.First().Id;
    }

    private static async Task SaveRawMessage(string rawMessage, string rfcMessageId, string outputPath)
    {
        Directory.CreateDirectory(outputPath);

        string cleanEmailFileName = MakeValidFileName(rfcMessageId);

        string emailPath = Path.Combine(outputPath, cleanEmailFileName, ".eml");

        await File.WriteAllTextAsync(emailPath, rawMessage, Encoding.UTF8);
    }

    private static string MakeValidFileName(string name)
    {
        string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
    }
}