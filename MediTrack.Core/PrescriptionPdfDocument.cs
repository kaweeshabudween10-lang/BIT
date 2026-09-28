using System;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using MediTrack.Data.Models;

namespace MediTrack.Core
{
    public class PrescriptionPdfDocument : IDocument
    {
        private readonly Patient _patient;
        private readonly User _doctor;
        private readonly Visit _visit;
        private readonly List<Prescription> _items;

        public PrescriptionPdfDocument(Patient patient, User doctor, Visit visit, List<Prescription> items)
        {
            _patient = patient;
            _doctor = doctor;
            _visit = visit;
            _items = items;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(20);
                page.DefaultTextStyle(style => style.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
        }

        private void ComposeHeader(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().Text("MediTrack Clinic Center").FontSize(18).Bold().FontColor(Colors.Blue.Medium);
                column.Item().Text("123 Healthcare Ave, Colombo | Tel: 011-2345678").FontSize(9).FontColor(Colors.Grey.Medium);
                column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            });
        }

        private void ComposeContent(IContainer container)
        {
            int age = DateTime.Today.Year - _patient.DOB.Year;
            if (_patient.DOB.Date > DateTime.Today.AddYears(-age))
            {
                age--;
            }

            container.PaddingVertical(10).Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(patientColumn =>
                    {
                        patientColumn.Item().Text($"Patient: {_patient.FullName}").Bold();
                        patientColumn.Item().Text($"Age/Gender: {age} yrs / {_patient.Gender}");
                        patientColumn.Item().Text($"Allergies: {_patient.Allergies}");
                    });

                    row.RelativeItem().Column(doctorColumn =>
                    {
                        doctorColumn.Item().Text($"Doctor: Dr. {_doctor.FullName}").Bold();
                        doctorColumn.Item().Text($"Date: {_visit.CreatedDate:dd/MM/yyyy}");
                        doctorColumn.Item().Text($"Diagnosis: {_visit.Diagnosis}");
                    });
                });

                column.Item().PaddingVertical(10).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                column.Item().Text("Rx").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);

                column.Item().PaddingTop(5).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Medicine").Bold();
                        header.Cell().Text("Dosage").Bold();
                        header.Cell().Text("Duration").Bold();
                    });

                    foreach (var item in _items)
                    {
                        table.Cell().Text(item.MedicineName);
                        table.Cell().Text(item.Dosage);
                        table.Cell().Text(item.Duration);
                    }
                });

                if (!string.IsNullOrEmpty(_visit.Notes))
                {
                    column.Item().PaddingTop(15).Text($"Notes / Instructions: {_visit.Notes}").Italic();
                }
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().AlignRight().Text("_______________________");
                column.Item().AlignRight().Text($"Dr. {_doctor.FullName}").Bold();
                column.Item().PaddingTop(5).AlignCenter().Text("Get well soon!").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        }
    }
}