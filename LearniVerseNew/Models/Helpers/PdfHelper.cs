using LearniVerseNew.Models.ApplicationModels;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace LearniVerseNew.Models.Helpers
{
    public class PdfHelper
    {
        private static readonly ApplicationDbContext db = new ApplicationDbContext();
        private static byte[] GenerateInvoice(Student student, List<string> courseIdsList, decimal totalAmount, string paymentReference)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            byte[] bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);


                    page.Header().Text("LearniVerse Invoice")
                    .Bold().FontSize(32)
                    .FontColor(Colors.Black);


                    page.Content().Column(column =>
                    {
                        column.Item().Text($"Faculty: {student.Faculty.FacultyName}, Qualification: {student.Qualification.QualificationID}")
                        .AlignLeft()
                        .FontColor(Colors.Black)
                        .Underline()
                        .LineHeight(10);

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Course")
                                .Bold();
                                header.Cell().Text("Price")
                                .Bold();
                            });

                            foreach (var courseId in courseIdsList)
                            {
                                var course = db.Courses.FirstOrDefault(c => c.CourseID == courseId);
                                table.Cell().Text($"{course.CourseName}").FontColor(Colors.Black);
                                table.Cell().Text($"{course.Price}").FontColor(Colors.Black);
                            }
                        });

                        column.Item().Row(row =>
                        {
                            row.RelativeItem(1).Text($"Total Amount: {totalAmount}")
                            .Bold();
                            row.RelativeItem(1).Text($"Payment Reference: {paymentReference}")
                            .Bold();
                        });

                        column.Item().Text($"Thank you for your payment. {student.StudentFirstName}!")
                        .AlignCenter()
                        .FontSize(10)
                        .Italic();

                    });

                });
            }).GeneratePdf();

            return bytes;
        }

        public static void GenerateAndEmailInvoice(Student student, List<string> courseIdsList, decimal totalAmount, string paymentReference)
        {
            EmailHelper mailer = new EmailHelper();

            byte[] bytes = GenerateInvoice(student, courseIdsList, totalAmount, paymentReference);

            string tempFilePath = Path.Combine(Path.GetTempPath(), $"{student.StudentFirstName}_{student.StudentLastName}_Invoice_{DateTime.Now.ToString("yyyy-MM-dd")}.pdf");

            File.WriteAllBytes(tempFilePath, bytes);

            mailer.SendEmailWithAttachment(student.StudentEmail, tempFilePath, $"{student.StudentFirstName} {student.StudentLastName}-Invoice-{DateTime.Now.ToString("yyyy-MM-dd")}.pdf");

            File.Delete(tempFilePath);
        }
    }
}