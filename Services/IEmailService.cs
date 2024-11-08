﻿using BumbleBeeFoundation_Client.Models;

namespace BumbleBeeFoundation_Client.Services
{
    public interface IEmailService
    {
        Task SendDonationNotificationAsync(DonationViewModel donation);

        Task SendDonationCertificateAsync(string recipientEmail, string recipientName, byte[] certificatePdf);
    }
}
