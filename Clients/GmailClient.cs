using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Net.Mail;
using System.Text;

namespace AWS_QA_Course_Test_Project.Clients
{
    public class GmailClient
    {
        private readonly string[] Scopes = { GmailService.Scope.GmailReadonly };
        private readonly string ApplicationName = "aws-qa-course";
        private readonly GmailService _service;

        public GmailClient(string user)
        {
            UserCredential credential;

            using (var stream = new FileStream("C:\\Users\\origami\\Desktop\\CloudX Associate AWS for Testers\\AWS_QA_Course_Test_Project\\Config\\client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    user,
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            _service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        public async Task<MailMessage> GetLastMessageAsync()
        {
            return (await GetMessagesAsync()).FirstOrDefault();
        }

        public async Task<List<MailMessage>> GetMessagesAsync()
        {
            var request = _service.Users.Messages.List("me");
            request.LabelIds = "INBOX";
            request.IncludeSpamTrash = false;

            var messages = new List<MailMessage>();
            ListMessagesResponse response = null;

            do
            {
                response = await request.ExecuteAsync();

                if (response.Messages != null)
                {
                    foreach (var messageItem in response.Messages)
                    {
                        var message = await _service.Users.Messages.Get("me", messageItem.Id).ExecuteAsync();
                        var mailMessage = new MailMessage
                        {
                            Subject = message.Payload.Headers.FirstOrDefault(h => h.Name == "Subject")?.Value,
                            Body = GetMessageBody(message.Payload)
                        };
                        mailMessage.From = new MailAddress(message.Payload.Headers.FirstOrDefault(h => h.Name == "From")?.Value);
                        mailMessage.To.Add(new MailAddress(message.Payload.Headers.FirstOrDefault(h => h.Name == "To")?.Value));
                        messages.Add(mailMessage);
                    }
                }

                request.PageToken = response.NextPageToken;
            } while (!string.IsNullOrEmpty(response.NextPageToken));

            return messages;
        }

        private string GetMessageBody(MessagePart payload)
        {
            if (payload.Parts == null && payload.Body != null)
            {
                return DecodeBase64String(payload.Body.Data);
            }

            var body = string.Empty;
            foreach (var part in payload.Parts)
            {
                if (part.MimeType == "text/plain")
                {
                    body += DecodeBase64String(part.Body.Data);
                }
                else if (part.MimeType == "multipart/alternative" || part.MimeType == "multipart/mixed")
                {
                    body += GetMessageBody(part);
                }
            }

            return body;
        }

        private string DecodeBase64String(string base64String)
        {
            var data = Convert.FromBase64String(base64String.Replace('-', '+').Replace('_', '/'));
            return Encoding.UTF8.GetString(data);
        }
    }
}
