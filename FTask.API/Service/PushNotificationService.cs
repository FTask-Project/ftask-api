using Azure;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using FTask.Repository.Data;
using FTask.Service.Common;
using FTask.Service.IService;
using Google.Apis.Auth.OAuth2;

namespace FTask.API.Service
{
    public interface IPushNotificationService
    {
        Task SendNotifications(string title, string content, List<string> tokens);
    }

    public class PushNotificationService : IPushNotificationService
    {
        private readonly FirebaseMessaging _firebaseMessaging;
        private ILogger<PushNotificationService> logger;

        public PushNotificationService(ILogger<PushNotificationService> logger)
        {
            if (FirebaseApp.GetInstance("pushNotificationInstance") is null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("keys/mobileServiceAccountKey.json"),
                }, "pushNotificationInstance");
            }
            FirebaseApp app = FirebaseApp.GetInstance("pushNotificationInstance");
            _firebaseMessaging = FirebaseMessaging.GetMessaging(app);
            this.logger = logger;
        }

        public async Task SendNotifications(string title, string content, List<string> tokens)
        {
            if (tokens.Count() > 0)
            {
                var message = new MulticastMessage()
                {
                    Tokens = tokens.ToList(),
                    Notification = new Notification
                    {
                        Title = title,
                        Body = content
                    }
                };

                var response = await _firebaseMessaging.SendMulticastAsync(message);

                Console.WriteLine("Send notification: " + response.SuccessCount + "/" + tokens.Count() + " success");
                logger.LogInformation("Send notification: " + response.SuccessCount + "/" + tokens.Count() + " success");
            }
            else
            {
                Console.WriteLine("No notification sent");
                logger.LogInformation("No notification sent");
            }
        }
    }
}
