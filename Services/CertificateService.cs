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
using iText.Layout.Borders;

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
                
                var headerColor = new DeviceRgb(30, 78, 120);  
                var borderColor = new DeviceRgb(200, 200, 200); 
                var outerBorderColor = new DeviceRgb(150, 150, 150); 

               
                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                document.SetFont(font);

                // Main container with an outer border
                var mainContainer = new Div()
                    .SetBorder(new SolidBorder(outerBorderColor, 2))
                    .SetPadding(20);

                // Title Section
                var title = new Paragraph("SECTION 18A CERTIFICATE")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(16)
                    .SetBold()
                    .SetFontColor(headerColor);
                mainContainer.Add(title);

                // Certificate Number
                mainContainer.Add(new Paragraph($"Certificate No: {donation.DonationID}")
                    .SetMarginTop(20)
                    .SetFontSize(12)
                    .SetBold());

                // Legal Text
                var legalText = new Paragraph(
                    "This Tax Certificate is issued in terms of Section 18A(1)(a) of the Income Tax Act of 1962. " +
                    "The donation received will be used exclusively for the objects of BumbleBee Foundation.")
                    .SetMarginTop(20)
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.DARK_GRAY);
                mainContainer.Add(legalText);

                // Donor Details Header
                mainContainer.Add(new Paragraph("Donor Details:")
                    .SetBold()
                    .SetFontSize(12)
                    .SetMarginTop(20)
                    .SetFontColor(headerColor));

                // Donor Details List
                var details = new List()
                    .SetListSymbol("•")
                    .SetMarginLeft(30)
                    .SetFontSize(10);

                details.Add(new ListItem($"Donor Name [Individual]: {donation.DonorName}"));
                details.Add(new ListItem($"South African ID Number: {donation.DonorIDNumber}"));
                details.Add(new ListItem($"Tax Number: {donation.DonorTaxNumber}"));
                details.Add(new ListItem($"Donor E-mail: {donation.DonorEmail}"));
                details.Add(new ListItem($"Donor Phone: {donation.DonorPhone}"));
                details.Add(new ListItem($"Date of Donation: {donation.DonationDate:dd/MM/yyyy}"));
                details.Add(new ListItem($"Donation Type: {donation.DonationType}"));
                details.Add(new ListItem($"Donation Amount: R {donation.DonationAmount:N2}"));
                mainContainer.Add(details);

                // Confirmation Text
                var confirmation = new Paragraph("\"We hereby confirm that the above was received by BumbleBee Foundation.\"")
                    .SetMarginTop(20)
                    .SetItalic()
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.DARK_GRAY);
                mainContainer.Add(confirmation);

                // Signature Section
                var signature = new Paragraph("Signed by:\nBumbleBee Foundation")
                    .SetMarginTop(40)
                    .SetFontSize(10)
                    .SetFontColor(headerColor);
                mainContainer.Add(signature);

                // Issue Date
                var issueDate = new Paragraph($"Date of Issue: {DateTime.Now:dd/MM/yyyy}")
                    .SetMarginTop(20)
                    .SetFontSize(10);
                mainContainer.Add(issueDate);

                // Footer Note
                var footerNote = new Paragraph(
                    "Please attach this certificate to your income tax return. " +
                    "This certificate was issued without any alterations or erasures.")
                    .SetMarginTop(40)
                    .SetFontSize(8)
                    .SetFontColor(ColorConstants.DARK_GRAY);
                mainContainer.Add(footerNote);

                // Trustees
                var trustees = new Paragraph("Trustees: [Trustee Names]")
                    .SetMarginTop(20)
                    .SetFontSize(8)
                    .SetFontColor(ColorConstants.DARK_GRAY);
                mainContainer.Add(trustees);

                // Add mainContainer to the document
                document.Add(mainContainer);

                // Close document
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
