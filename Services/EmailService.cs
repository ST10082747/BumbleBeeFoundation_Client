namespace BumbleBeeFoundation_Client.Services
{
    
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using MimeKit;
    using Microsoft.Extensions.Options;
    using BumbleBeeFoundation_Client.Models;

    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendDonationNotificationAsync(DonationViewModel donation)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            message.To.Add(new MailboxAddress("Admin", _smtpSettings.AdminEmail));
            message.Subject = "New Donation Received";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = $"A new donation has been made.\n\n" +
                           $"Donor Name: {donation.DonorName}\n" +
                           $"Donor Email: {donation.DonorEmail}\n" +
                           $"Donation Amount R: {donation.DonationAmount}\n" +
                           $"Donation Type: {donation.DonationType}\n" +
                           $"Donation ID: {donation.DonationId}\n\n" +
                           $"Log on to the Admin Portal and check the Donation Management page."
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to send email.", ex);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        // Allow admin to Genereate Section 18 document and send it to user
        public async Task SendDonationCertificateAsync(string recipientEmail, string recipientName, byte[] certificatePdf)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = "BumbleBee Foundation - Section 18A Donation Certificate";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = $"Dear {recipientName},\n\n" +
                          "Thank you for your donation to BumbleBee Foundation. " +
                          "Please find attached your Section 18A donation certificate for tax purposes.\n\n" +
                          "Best regards,\nBumbleBee Foundation"
            };

            // Attach the PDF certificate
            bodyBuilder.Attachments.Add("DonationCertificate.pdf", certificatePdf, ContentType.Parse("application/pdf"));

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        // If a password reset request is made, send out an email
        public async Task SendPasswordResetNotificationAsync(string recipientEmail, string recipientName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = "Password Reset Attempt";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = $"Dear {recipientName},\n\n" +
                           "We received a request to reset your password. " +
                           "If this was you, please follow the instructions on the Reset Password page. " +
                           "If you did not request this password reset, please notify us so we can take steps to prevent unauthorized access to your account.\n\n" +
                           "If you have any questions, feel free to contact support.\n\n" +
                           "Best regards,\nBumbleBee Foundation"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        // When the admin deactivates a user account, send the user a notification
        public async Task SendAccountDeletionNotificationAsync(string recipientEmail, string recipientName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            message.To.Add(new MailboxAddress(recipientName, recipientEmail));
            message.Subject = "Account Deactivation Notification";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = $"Dear {recipientName},\n\n" +
                           "We want to inform you that your user profile with BumbleBee Foundation has been deactivated by an administrator. " +
                           "Your profile and user details are no longer valid for logging in.\n\n" +
                           "If you need futher details about why your access was revoked, please contact support.\n\n" +
                           "Best regards,\nBumbleBee Foundation"
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
                await client.SendAsync(message);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }

        // When a company uploads supporting documents, alert the admin
        public async Task SendDocumentUploadNotificationAsync(int requestId, int companyId, string documentName)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_smtpSettings.FromName, _smtpSettings.FromEmail));
            message.To.Add(new MailboxAddress("Admin", _smtpSettings.AdminEmail));
            message.Subject = "New Document Uploaded";

            var bodyBuilder = new BodyBuilder
            {
                TextBody = $"A company has uploaded new supporting documents, regarding their approved project currently in progress.\n\n" +
                           $"Request ID: {requestId}\n" +
                           $"Company ID: {companyId}\n" +
                           $"Document Name: {documentName}\n\n" +
                           "Log in to the Admin Portal to review the document."
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_smtpSettings.UserName, _smtpSettings.Password);
                await client.SendAsync(message);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to send email.", ex);
            }
            finally
            {
                await client.DisconnectAsync(true);
            }
        }


    }

}
