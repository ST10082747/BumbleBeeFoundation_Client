using BumbleBeeFoundation_Client.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Colors;
using System.IO;
using Document = iText.Layout.Document;

namespace BumbleBeeFoundation_Client.Services
{
    public class CertificateService
    {
        public byte[] GenerateDonationCertificate(Donation donation)
        {
            using var memoryStream = new MemoryStream();
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4);

            try
            {
                // Set default font
                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                document.SetFont(font);

                // Title
                var title = new Paragraph("SECTION 18A CERTIFICATE")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetBold();
                document.Add(title);

                // Certificate number
                var certNumber = new Paragraph($"Certificate No: {donation.DonationID}")
                    .SetMarginTop(20);
                document.Add(certNumber);

                // Legal text
                var legalText = new Paragraph(
                    "This Tax Certificate is issued in terms of Section 18A(1)(a) of the Income Tax Act of 1962. " +
                    "The donation received will be used exclusively for the objects of BumbleBee Foundation.")
                    .SetMarginTop(20);
                document.Add(legalText);

                // Donor details header
                var donorHeader = new Paragraph("Donor Details:")
                    .SetBold()
                    .SetMarginTop(20);
                document.Add(donorHeader);

                // Donor details list
                var details = new List()
                    .SetListSymbol("•")
                    .SetMarginLeft(30);

                details.Add(new ListItem($"Donor Name [Individual]: {donation.DonorName}"));
                details.Add(new ListItem($"South African ID Number: {donation.DonorIDNumber}"));
                details.Add(new ListItem($"Tax Number: {donation.DonorTaxNumber}"));
                details.Add(new ListItem($"Donor E-mail: {donation.DonorEmail}"));
                details.Add(new ListItem($"Donor Phone: {donation.DonorPhone}"));
                details.Add(new ListItem($"Date of Donation: {donation.DonationDate:dd/MM/yyyy}"));
                details.Add(new ListItem($"Donation Type: {donation.DonationType}"));
                details.Add(new ListItem($"Donation Amount: R {donation.DonationAmount:N2}"));

                document.Add(details);

                // Confirmation text
                var confirmation = new Paragraph("\"I confirm that the above was received by BumbleBee Foundation.\"")
                    .SetMarginTop(20)
                    .SetItalic();
                document.Add(confirmation);

                // Signature section
                var signature = new Paragraph("Signed by:\nBumbleBee Foundation")
                    .SetMarginTop(40);
                document.Add(signature);

                // Issue date
                var issueDate = new Paragraph($"Date of Issue: {DateTime.Now:dd/MM/yyyy}")
                    .SetMarginTop(20);
                document.Add(issueDate);

                // Footer note
                var footerNote = new Paragraph(
                    "Please attach this certificate to your income tax return. " +
                    "This certificate was issued without any alterations or erasures")
                    .SetMarginTop(40)
                    .SetFontSize(8);
                document.Add(footerNote);

                // Trustees
                var trustees = new Paragraph("Trustees: [Trustee Names]")
                    .SetMarginTop(20)
                    .SetFontSize(8);
                document.Add(trustees);

                document.Close();
                return memoryStream.ToArray();
            }
            catch (Exception)
            {
                document?.Close();
                throw;
            }
        }
    }
}
