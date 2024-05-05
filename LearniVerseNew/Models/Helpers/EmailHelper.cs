using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;

namespace LearniVerseNew.Models.Helpers
{
    public class EmailHelper
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailHelper()
        {
            _smtpServer = ConfigurationManager.AppSettings["SmtpServer"];
            _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"]);
            _smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];
            _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
        }


        public void SendEmailWithAttachment(string recipientEmail, string filePath, string attachmentName)
        {
            using (MailMessage mailMessage = new MailMessage())
            {
                mailMessage.From = new MailAddress(_smtpUsername);
                mailMessage.To.Add(recipientEmail);
                mailMessage.Subject = "LearniVerse Invoice";
                mailMessage.Body = "Attached is your Invoice for your Enrollment";
                mailMessage.IsBodyHtml = true; // Set to false if you're sending plain text

                // Attach the PDF to the email
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    Attachment attachment = new Attachment(fileStream, attachmentName, "application/pdf");
                    mailMessage.Attachments.Add(attachment);

                    // Send the email
                    using (SmtpClient smtpClient = new SmtpClient(_smtpServer, _smtpPort))
                    {
                        smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                        smtpClient.EnableSsl = true; // Use SSL
                        smtpClient.Send(mailMessage);
                    }
                }
            }
        }

        public void SendEmailApprovalPending(string recipientEmail, string studentName)
        {
            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress(_smtpUsername);
                msg.To.Add(recipientEmail);
                msg.Subject = "Enrollment Approval Pending";
                msg.Body = $"Dear {studentName},<br><br>Your enrollment is pending approval. You will receive further instructions via email.<br><br>Thank you,<br>LearniVerse";
                msg.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient(_smtpServer, _smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(msg);
                }

            }
        }

        public void SendEmailApproved(string recipientEmail, string studentName)
        {
            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress(_smtpUsername);
                msg.To.Add(recipientEmail);
                msg.Subject = "Enrollment Approved";
                msg.Body = $"Dear {studentName},<br><br>Your enrollment has been approved. You can now access your courses.<br><br>Thank you,<br>LearniVerse";
                msg.IsBodyHtml = true;

                using (SmtpClient smtpClient = new SmtpClient(_smtpServer, _smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.Send(msg);
                }
            }
        }
    }
}