﻿namespace BumbleBeeFoundation_Client.Services
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

    }

}
