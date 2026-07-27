using Microsoft.EntityFrameworkCore;
using OpenAmp.Application.Reporting;
using OpenAmp.Infrastructure.Persistence;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace OpenAmp.Infrastructure.Reporting;

public sealed class ReportingService(OpenAmpDbContext dbContext) : IReportingService
{
    private static readonly string[] UkljuceniStatusi = ["PLACENA", "IZVRSENA"];

    public async Task<PoslovniIzvjestajDto> GenerisiAsync(
        DateTime periodOdUtc,
        DateTime periodDoUtc,
        int? salaId,
        int? zanrId,
        CancellationToken cancellationToken = default)
    {
        ValidirajPeriod(periodOdUtc, periodDoUtc);

        var query = dbContext.Rezervacije
            .AsNoTracking()
            .Where(x =>
                UkljuceniStatusi.Contains(x.Status.Kod)
                && x.TerminOdUtc >= periodOdUtc
                && x.TerminOdUtc < periodDoUtc);

        if (salaId.HasValue)
        {
            query = query.Where(x => x.SalaId == salaId);
        }
        if (zanrId.HasValue)
        {
            query = query.Where(x => x.Bend.ZanrId == zanrId);
        }

        var rezervacije = await query
            .Select(x => new StavkaIzvjestaja(
                x.SalaId,
                x.Sala.Naziv,
                x.Sala.Studio.Naziv,
                x.Bend.ZanrId,
                x.Bend.Zanr.Naziv,
                x.UkupnaCijena - x.RefundiraniIznos,
                EF.Functions.DateDiffMinute(x.TerminOdUtc, x.TerminDoUtc) / 60m))
            .ToArrayAsync(cancellationToken);

        var ukupanPrihod = rezervacije.Sum(x => Math.Max(0, x.Prihod));
        var ukupnoRezervacija = rezervacije.Length;
        var prihodPoSalama = rezervacije
            .GroupBy(x => new { x.SalaId, x.Sala, x.Studio })
            .Select(group =>
            {
                var prihod = group.Sum(x => Math.Max(0, x.Prihod));
                return new PrihodPoSaliDto(
                    group.Key.SalaId,
                    group.Key.Sala,
                    group.Key.Studio,
                    prihod,
                    group.Count(),
                    Procenat(prihod, ukupanPrihod));
            })
            .OrderByDescending(x => x.Prihod)
            .ThenBy(x => x.Sala)
            .ToArray();
        var rezervacijePoZanrovima = rezervacije
            .GroupBy(x => new { x.ZanrId, x.Zanr })
            .Select(group => new RezervacijePoZanruDto(
                group.Key.ZanrId,
                group.Key.Zanr,
                group.Count(),
                Procenat(group.Count(), ukupnoRezervacija)))
            .OrderByDescending(x => x.BrojRezervacija)
            .ThenBy(x => x.Zanr)
            .ToArray();

        return new PoslovniIzvjestajDto(
            periodOdUtc,
            periodDoUtc,
            salaId,
            zanrId,
            ukupanPrihod,
            ukupnoRezervacija,
            ukupnoRezervacija == 0 ? 0 : ukupanPrihod / ukupnoRezervacija,
            decimal.Round(rezervacije.Sum(x => x.BrojSati), 2),
            prihodPoSalama,
            rezervacijePoZanrovima);
    }

    public async Task<byte[]> GenerisiPdfAsync(
        DateTime periodOdUtc,
        DateTime periodDoUtc,
        int? salaId,
        int? zanrId,
        CancellationToken cancellationToken = default)
    {
        var izvjestaj = await GenerisiAsync(
            periodOdUtc,
            periodDoUtc,
            salaId,
            zanrId,
            cancellationToken);

        QuestPDF.Settings.License = LicenseType.Community;
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(36);
            page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(9).FontColor("#25272C"));

            page.Header().Column(header =>
            {
                header.Item().Row(row =>
                {
                    row.RelativeItem().Text(text =>
                    {
                        text.Span("Open").Bold().FontSize(21).FontColor("#111317");
                        text.Span("Amp").Bold().FontSize(21).FontColor("#FF5B3A");
                    });
                    row.ConstantItem(170).AlignRight().Column(meta =>
                    {
                        meta.Item().AlignRight().Text("POSLOVNI IZVJEŠTAJ").Bold().FontSize(11);
                        meta.Item().AlignRight().Text(
                            $"{izvjestaj.PeriodOdUtc:dd.MM.yyyy} – {izvjestaj.PeriodDoUtc.AddTicks(-1):dd.MM.yyyy}")
                            .FontColor("#6B6E76");
                    });
                });
                header.Item().PaddingTop(12).LineHorizontal(2).LineColor("#FF5B3A");
            });

            page.Content().PaddingVertical(22).Column(content =>
            {
                content.Spacing(18);
                content.Item().Element(container => Summary(container, izvjestaj));
                content.Item().Element(container => RevenueTable(container, izvjestaj.PrihodPoSalama));
                content.Item().Element(container => GenreTable(container, izvjestaj.RezervacijePoZanrovima));
            });

            page.Footer().Row(row =>
            {
                row.RelativeItem().Text($"Generisano {DateTime.UtcNow:dd.MM.yyyy HH:mm} UTC")
                    .FontSize(8).FontColor("#7A7D84");
                row.ConstantItem(90).AlignRight().Text(text =>
                {
                    text.Span("Stranica ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        })).GeneratePdf();
    }

    private static void Summary(IContainer container, PoslovniIzvjestajDto report)
    {
        container.Row(row =>
        {
            row.Spacing(10);
            Metric(row, "UKUPAN PRIHOD", $"{report.UkupanPrihod:N2} KM");
            Metric(row, "REZERVACIJE", report.UkupnoRezervacija.ToString(CultureInfo.CurrentCulture));
            Metric(row, "PROSJEČNA VRIJEDNOST", $"{report.ProsjecnaVrijednostRezervacije:N2} KM");
            Metric(row, "REZERVISANI SATI", $"{report.UkupnoSati:N1} h");
        });
    }

    private static void Metric(RowDescriptor row, string label, string value)
    {
        row.RelativeItem().Background("#F3F3F1").Padding(12).Column(column =>
        {
            column.Item().Text(label).FontSize(7).Bold().FontColor("#6B6E76");
            column.Item().PaddingTop(4).Text(value).FontSize(14).Bold().FontColor("#111317");
        });
    }

    private static void RevenueTable(
        IContainer container,
        IReadOnlyCollection<PrihodPoSaliDto> items)
    {
        container.Column(column =>
        {
            SectionTitle(column, "Prihod po salama");
            if (items.Count == 0)
            {
                EmptyState(column);
                return;
            }
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2.7f);
                    columns.RelativeColumn(1.8f);
                    columns.ConstantColumn(82);
                    columns.ConstantColumn(54);
                    columns.ConstantColumn(55);
                });
                TableHeader(table, "Sala", "Studio", "Prihod", "Broj", "Udio");
                foreach (var item in items)
                {
                    Cell(table, item.Sala);
                    Cell(table, item.Studio);
                    Cell(table, $"{item.Prihod:N2} KM", true);
                    Cell(table, item.BrojRezervacija.ToString(CultureInfo.CurrentCulture), true);
                    Cell(table, $"{item.Postotak:N1}%", true);
                }
            });
        });
    }

    private static void GenreTable(
        IContainer container,
        IReadOnlyCollection<RezervacijePoZanruDto> items)
    {
        container.Column(column =>
        {
            SectionTitle(column, "Rezervacije po žanru");
            if (items.Count == 0)
            {
                EmptyState(column);
                return;
            }
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.ConstantColumn(100);
                    columns.ConstantColumn(70);
                });
                TableHeader(table, "Žanr", "Rezervacije", "Udio");
                foreach (var item in items)
                {
                    Cell(table, item.Zanr);
                    Cell(table, item.BrojRezervacija.ToString(CultureInfo.CurrentCulture), true);
                    Cell(table, $"{item.Postotak:N1}%", true);
                }
            });
        });
    }

    private static void SectionTitle(ColumnDescriptor column, string title)
    {
        column.Item().PaddingBottom(8).Text(title).Bold().FontSize(14).FontColor("#111317");
    }

    private static void EmptyState(ColumnDescriptor column)
    {
        column.Item().Background("#F3F3F1").Padding(14)
            .Text("Nema podataka za odabrani period.").FontColor("#6B6E76");
    }

    private static void TableHeader(TableDescriptor table, params string[] labels)
    {
        foreach (var label in labels)
        {
            table.Cell().Background("#111317").PaddingVertical(7).PaddingHorizontal(8)
                .Text(label).Bold().FontColor(Colors.White);
        }
    }

    private static void Cell(TableDescriptor table, string text, bool alignRight = false)
    {
        var cell = table.Cell().BorderBottom(1).BorderColor("#E3E3DF")
            .PaddingVertical(7).PaddingHorizontal(8);
        if (alignRight)
        {
            cell.AlignRight().Text(text);
        }
        else
        {
            cell.Text(text);
        }
    }

    private static decimal Procenat(decimal value, decimal total) =>
        total == 0 ? 0 : decimal.Round(value / total * 100, 1);

    private static void ValidirajPeriod(DateTime od, DateTime @do)
    {
        if (@do <= od)
        {
            throw new ArgumentException("Krajnji datum mora biti nakon početnog datuma.");
        }
        if (@do - od > TimeSpan.FromDays(366 * 5))
        {
            throw new ArgumentException("Izvještaj može obuhvatiti najviše pet godina.");
        }
    }

    private sealed record StavkaIzvjestaja(
        int SalaId,
        string Sala,
        string Studio,
        int ZanrId,
        string Zanr,
        decimal Prihod,
        decimal BrojSati);
}
