using Ekiva.Application.Services;
using Ekiva.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Ekiva.Infrastructure.Services;

public class PdfGenerationService : IPdfGenerationService
{
    private readonly EkivaDbContext _context;

    public PdfGenerationService(EkivaDbContext context)
    {
        _context = context;
        // Configuration de la licence QuestPDF (Community)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateQuotePdfAsync(Guid quoteId)
    {
        var quote = await _context.MotorQuotes
            .Include(q => q.Client)
            .Include(q => q.MotorProduct)
            .Include(q => q.VehicleCategory)
            .Include(q => q.VehicleMake)
            .Include(q => q.VehicleModel)
            .Include(q => q.Currency)
            .Include(q => q.SelectedCoverages)
                .ThenInclude(c => c.MotorCoverage)
            .FirstOrDefaultAsync(q => q.Id == quoteId);

        if (quote == null)
            throw new Exception($"Devis {quoteId} non trouvé");

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(Header);
                page.Content().Element(c => QuoteContent(c, quote));
                page.Footer().Element(Footer);
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GeneratePolicyPdfAsync(Guid policyId)
    {
        var policy = await _context.MotorPolicies
            .Include(p => p.Client)
            .Include(p => p.MotorProduct)
            .Include(p => p.VehicleCategory)
            .Include(p => p.VehicleMake)
            .Include(p => p.VehicleModel)
            .Include(p => p.Currency)
            .Include(p => p.Coverages)
                .ThenInclude(c => c.MotorCoverage)
            .FirstOrDefaultAsync(p => p.Id == policyId);

        if (policy == null)
            throw new Exception($"Police {policyId} non trouvée");

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(Header);
                page.Content().Element(c => PolicyContent(c, policy));
                page.Footer().Element(Footer);
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateInsuranceCertificateAsync(Guid policyId)
    {
        var policy = await _context.MotorPolicies
            .Include(p => p.Client)
            .Include(p => p.MotorProduct)
            .Include(p => p.VehicleMake)
            .Include(p => p.VehicleModel)
            .Include(p => p.Coverages)
                .ThenInclude(c => c.MotorCoverage)
            .FirstOrDefaultAsync(p => p.Id == policyId);

        if (policy == null)
            throw new Exception($"Police {policyId} non trouvée");

        if (policy.Status != Core.Entities.PolicyStatus.Active)
            throw new Exception("L'attestation ne peut être générée que pour une police active");

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Element(Header);
                page.Content().Element(c => CertificateContent(c, policy));
                page.Footer().Element(c => CertificateFooter(c, policy));
            });
        }).GeneratePdf();
    }

    public async Task<byte[]> GenerateEndorsementPdfAsync(Guid endorsementId)
    {
        var endorsement = await _context.MotorPolicyEndorsements
            .Include(e => e.MotorPolicy)
                .ThenInclude(p => p.Client)
            .Include(e => e.MotorPolicy)
                .ThenInclude(p => p.VehicleMake)
            .Include(e => e.MotorPolicy)
                .ThenInclude(p => p.VehicleModel)
            .FirstOrDefaultAsync(e => e.Id == endorsementId);

        if (endorsement == null)
            throw new Exception($"Avenant {endorsementId} non trouvé");

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                page.Header().Element(Header);
                page.Content().Element(c => EndorsementContent(c, endorsement));
                page.Footer().Element(Footer);
            });
        }).GeneratePdf();
    }

    // Méthodes privées pour la mise en page

    private void Header(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("EKIVA INSURANCE").Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                column.Item().Text("Assurance Automobile").FontSize(12).FontColor(Colors.Grey.Darken1);
                column.Item().PaddingTop(5).Text("123 Avenue des Assurances, Libreville, Gabon").FontSize(9);
                column.Item().Text("Tél: +241 01 XX XX XX | Email: contact@ekiva.ga").FontSize(9);
            });

            row.ConstantItem(100).Height(60).Placeholder(); // Logo placeholder
        });
    }

    private void Footer(IContainer container)
    {
        container.AlignCenter().Column(column =>
        {
            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            column.Item().Text(text =>
            {
                text.Span("EKIVA Insurance - RC: XXXX - CC: XXXXXXXXX").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
            column.Item().Text(text =>
            {
                text.Span("Page ").FontSize(8);
                text.CurrentPageNumber().FontSize(8);
                text.Span(" / ").FontSize(8);
                text.TotalPages().FontSize(8);
            });
        });
    }

    private void QuoteContent(IContainer container, Core.Entities.MotorQuote quote)
    {
        container.Column(column =>
        {
            // En-tête du devis
            column.Item().PaddingBottom(20).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("DEVIS D'ASSURANCE AUTOMOBILE").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(5).Text($"N° {quote.QuoteNumber}").FontSize(12).Bold();
                    col.Item().Text($"Date: {quote.QuoteDate:dd/MM/yyyy}").FontSize(10);
                    col.Item().Text($"Validité: {quote.ExpiryDate:dd/MM/yyyy}").FontSize(10);
                });

                row.ConstantItem(150).Column(col =>
                {
                    col.Item().Text($"Statut: {GetQuoteStatusText(quote.Status)}").FontSize(10).Bold();
                });
            });

            // Informations client
            column.Item().PaddingBottom(15).Element(c => ClientSection(c, quote.Client));

            // Informations véhicule
            column.Item().PaddingBottom(15).Element(c => VehicleSection(c, quote));

            // Garanties
            column.Item().PaddingBottom(15).Element(c => CoveragesSection(c, quote.SelectedCoverages));

            // Calcul de la prime
            column.Item().Element(c => PremiumCalculationSection(c, quote));
        });
    }

    private void PolicyContent(IContainer container, Core.Entities.MotorPolicy policy)
    {
        container.Column(column =>
        {
            // En-tête de la police
            column.Item().PaddingBottom(20).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("POLICE D'ASSURANCE AUTOMOBILE").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                    col.Item().PaddingTop(5).Text($"N° {policy.PolicyNumber}").FontSize(14).Bold();
                    col.Item().Text($"Date d'émission: {policy.IssueDate:dd/MM/yyyy}").FontSize(10);
                    col.Item().Text($"Période: du {policy.PolicyStartDate:dd/MM/yyyy} au {policy.PolicyEndDate:dd/MM/yyyy}").FontSize(10);
                });

                row.ConstantItem(150).Column(col =>
                {
                    col.Item().Border(1).BorderColor(Colors.Green.Medium).Padding(10).Column(c =>
                    {
                        c.Item().Text($"Statut: {policy.Status}").FontSize(10).Bold();
                        c.Item().Text(policy.IsPaid ? "✓ PAYÉE" : "⚠ NON PAYÉE").FontSize(9)
                            .FontColor(policy.IsPaid ? Colors.Green.Medium : Colors.Red.Medium);
                    });
                });
            });

            // Informations client
            column.Item().PaddingBottom(15).Element(c => ClientSection(c, policy.Client));

            // Informations véhicule
            column.Item().PaddingBottom(15).Element(c => VehicleSectionPolicy(c, policy));

            // Garanties
            column.Item().PaddingBottom(15).Element(c => CoveragesSectionPolicy(c, policy.Coverages));

            // Calcul de la prime
            column.Item().Element(c => PremiumCalculationSectionPolicy(c, policy));

            // Conditions générales
            column.Item().PaddingTop(20).Element(GeneralConditions);
        });
    }

    private void CertificateContent(IContainer container, Core.Entities.MotorPolicy policy)
    {
        container.Column(column =>
        {
            // Titre
            column.Item().PaddingBottom(20).AlignCenter().Column(col =>
            {
                col.Item().Text("ATTESTATION D'ASSURANCE").Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                col.Item().Text("RESPONSABILITÉ CIVILE AUTOMOBILE").FontSize(12).Bold();
            });

            // Informations de la police
            column.Item().PaddingBottom(15).Border(2).BorderColor(Colors.Blue.Darken2).Padding(15).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text($"Police N°: {policy.PolicyNumber}").Bold().FontSize(12);
                    row.RelativeItem().AlignRight().Text($"Du {policy.PolicyStartDate:dd/MM/yyyy} au {policy.PolicyEndDate:dd/MM/yyyy}").FontSize(11);
                });
            });

            // Assuré
            column.Item().PaddingBottom(15).Column(col =>
            {
                col.Item().Text("ASSURÉ").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                col.Item().PaddingTop(5).Text($"Nom: {GetClientName(policy.Client)}").FontSize(11);
                col.Item().Text($"Adresse: {policy.Client.Address}, {policy.Client.City}").FontSize(10);
            });

            // Véhicule assuré
            column.Item().PaddingBottom(15).Column(col =>
            {
                col.Item().Text("VÉHICULE ASSURÉ").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                col.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Marque/Modèle: {policy.VehicleMake.Name} {policy.VehicleModel.Name}").FontSize(11);
                    row.RelativeItem().Text($"Année: {policy.YearOfManufacture}").FontSize(11);
                });
                col.Item().Text($"Immatriculation: {policy.RegistrationNumber}").FontSize(11).Bold();
                col.Item().Text($"N° de châssis: {policy.ChassisNumber}").FontSize(10);
            });

            // Garanties couvertes
            column.Item().PaddingBottom(15).Column(col =>
            {
                col.Item().Text("GARANTIES EN VIGUEUR").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
                col.Item().PaddingTop(5).Column(c =>
                {
                    foreach (var coverage in policy.Coverages.Where(cov => cov.IsActive))
                    {
                        c.Item().Text($"✓ Section {coverage.MotorCoverage.SectionLetter} - {coverage.MotorCoverage.CoverageName}")
                            .FontSize(10);
                    }
                });
            });

            // Avertissement
            column.Item().PaddingTop(20).Background(Colors.Grey.Lighten3).Padding(10).Column(col =>
            {
                col.Item().Text("IMPORTANT").Bold().FontSize(10);
                col.Item().PaddingTop(5).Text("Cette attestation doit être conservée dans le véhicule et présentée aux autorités en cas de contrôle. Elle atteste que le véhicule est couvert par une assurance responsabilité civile conformément à la réglementation en vigueur.")
                    .FontSize(9).Italic();
            });
        });
    }

    private void CertificateFooter(IContainer container, Core.Entities.MotorPolicy policy)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(30).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Fait à Libreville").FontSize(9);
                    col.Item().Text($"Le {DateTime.Now:dd/MM/yyyy}").FontSize(9);
                });

                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().Text("Signature et cachet").FontSize(9).Bold();
                    col.Item().PaddingTop(30).LineHorizontal(1).LineColor(Colors.Black);
                });
            });

            column.Item().PaddingTop(20).AlignCenter().Text(text =>
            {
                text.Span("EKIVA Insurance - Agrément N° XXXX/MEFBP").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void EndorsementContent(IContainer container, Core.Entities.MotorPolicyEndorsement endorsement)
    {
        container.Column(column =>
        {
            // En-tête de l'avenant
            column.Item().PaddingBottom(20).Column(col =>
            {
                col.Item().Text("AVENANT À LA POLICE D'ASSURANCE").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                col.Item().PaddingTop(5).Text($"Avenant N° {endorsement.EndorsementNumber}").FontSize(14).Bold();
                col.Item().Text($"Police N° {endorsement.MotorPolicy.PolicyNumber}").FontSize(12);
                col.Item().Text($"Date: {endorsement.EndorsementDate:dd/MM/yyyy}").FontSize(10);
                col.Item().Text($"Effet au: {endorsement.EffectiveDate:dd/MM/yyyy}").FontSize(10);
            });

            // Type d'avenant
            column.Item().PaddingBottom(15).Background(Colors.Blue.Lighten4).Padding(10).Column(col =>
            {
                col.Item().Text($"Type: {GetEndorsementTypeText(endorsement.EndorsementType)}").Bold().FontSize(12);
            });

            // Description
            column.Item().PaddingBottom(15).Column(col =>
            {
                col.Item().Text("Description des modifications:").Bold().FontSize(11);
                col.Item().PaddingTop(5).Text(endorsement.Description).FontSize(10);
                if (!string.IsNullOrEmpty(endorsement.Reason))
                {
                    col.Item().PaddingTop(5).Text($"Motif: {endorsement.Reason}").FontSize(10).Italic();
                }
            });

            // Impact financier
            column.Item().PaddingBottom(15).Border(1).BorderColor(Colors.Grey.Medium).Padding(10).Column(col =>
            {
                col.Item().Text("IMPACT FINANCIER").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                col.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text("Ajustement de prime:");
                    row.ConstantItem(150).AlignRight().Text($"{FormatCurrency(endorsement.PremiumAdjustment)}")
                        .FontColor(endorsement.PremiumAdjustment >= 0 ? Colors.Green.Medium : Colors.Red.Medium);
                });
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Nouvelle prime totale:").Bold();
                    row.ConstantItem(150).AlignRight().Text($"{FormatCurrency(endorsement.NewTotalPremium)}").Bold();
                });
            });

            // Conditions
            column.Item().PaddingTop(20).Column(col =>
            {
                col.Item().Text("Toutes les autres conditions de la police restent inchangées.").FontSize(9).Italic();
            });
        });
    }

    // Sections réutilisables

    private void ClientSection(IContainer container, Core.Entities.Client client)
    {
        container.Background(Colors.Grey.Lighten4).Padding(10).Column(column =>
        {
            column.Item().Text("INFORMATIONS CLIENT").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Nom: {GetClientName(client)}").FontSize(10);
                    col.Item().Text($"Email: {client.Email}").FontSize(10);
                });
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Téléphone: {client.PhoneNumber}").FontSize(10);
                    col.Item().Text($"Adresse: {client.Address}, {client.City}").FontSize(10);
                });
            });
        });
    }

    private void VehicleSection(IContainer container, Core.Entities.MotorQuote quote)
    {
        container.Background(Colors.Blue.Lighten5).Padding(10).Column(column =>
        {
            column.Item().Text("INFORMATIONS VÉHICULE").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Marque: {quote.VehicleMake.Name}").FontSize(10);
                    col.Item().Text($"Modèle: {quote.VehicleModel.Name}").FontSize(10);
                    col.Item().Text($"Catégorie: {quote.VehicleCategory.Name}").FontSize(10);
                    col.Item().Text($"Année: {quote.YearOfManufacture}").FontSize(10);
                });
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Immatriculation: {quote.RegistrationNumber}").FontSize(10).Bold();
                    col.Item().Text($"Châssis: {quote.ChassisNumber}").FontSize(10);
                    col.Item().Text($"Puissance: {quote.Horsepower} CV").FontSize(10);
                    col.Item().Text($"Carburant: {quote.FuelType}").FontSize(10);
                });
                row.ConstantItem(120).Column(col =>
                {
                    col.Item().Text($"Valeur: {FormatCurrency(quote.VehicleValue)}").FontSize(10).Bold();
                });
            });
        });
    }

    private void VehicleSectionPolicy(IContainer container, Core.Entities.MotorPolicy policy)
    {
        container.Background(Colors.Blue.Lighten5).Padding(10).Column(column =>
        {
            column.Item().Text("INFORMATIONS VÉHICULE").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Marque: {policy.VehicleMake.Name}").FontSize(10);
                    col.Item().Text($"Modèle: {policy.VehicleModel.Name}").FontSize(10);
                    col.Item().Text($"Catégorie: {policy.VehicleCategory.Name}").FontSize(10);
                    col.Item().Text($"Année: {policy.YearOfManufacture}").FontSize(10);
                });
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Immatriculation: {policy.RegistrationNumber}").FontSize(10).Bold();
                    col.Item().Text($"Châssis: {policy.ChassisNumber}").FontSize(10);
                    col.Item().Text($"Puissance: {policy.Horsepower} CV").FontSize(10);
                    col.Item().Text($"Carburant: {policy.FuelType}").FontSize(10);
                });
                row.ConstantItem(120).Column(col =>
                {
                    col.Item().Text($"Valeur: {FormatCurrency(policy.VehicleValue)}").FontSize(10).Bold();
                });
            });
        });
    }

    private void CoveragesSection(IContainer container, ICollection<Core.Entities.MotorQuoteCoverage> coverages)
    {
        container.Column(column =>
        {
            column.Item().Text("GARANTIES SOUSCRITES").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);
                    columns.RelativeColumn();
                    columns.ConstantColumn(100);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Section").FontColor(Colors.White).FontSize(9).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Garantie").FontColor(Colors.White).FontSize(9).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Prime").FontColor(Colors.White).FontSize(9).Bold();
                });

                foreach (var coverage in coverages)
                {
                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(coverage.MotorCoverage.SectionLetter).FontSize(9);
                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(coverage.MotorCoverage.CoverageName).FontSize(9);
                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(FormatCurrency(coverage.PremiumAmount)).FontSize(9);
                }
            });
        });
    }

    private void CoveragesSectionPolicy(IContainer container, ICollection<Core.Entities.MotorPolicyCoverage> coverages)
    {
        container.Column(column =>
        {
            column.Item().Text("GARANTIES EN VIGUEUR").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(60);
                    columns.RelativeColumn();
                    columns.ConstantColumn(80);
                    columns.ConstantColumn(100);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Section").FontColor(Colors.White).FontSize(9).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Garantie").FontColor(Colors.White).FontSize(9).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("Statut").FontColor(Colors.White).FontSize(9).Bold();
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Prime").FontColor(Colors.White).FontSize(9).Bold();
                });

                foreach (var coverage in coverages)
                {
                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(coverage.MotorCoverage.SectionLetter).FontSize(9);
                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(coverage.MotorCoverage.CoverageName).FontSize(9);
                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text(coverage.IsActive ? "✓" : "✗")
                        .FontColor(coverage.IsActive ? Colors.Green.Medium : Colors.Red.Medium).FontSize(9);
                    table.Cell().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5).AlignRight().Text(FormatCurrency(coverage.PremiumAmount)).FontSize(9);
                }
            });
        });
    }

    private void PremiumCalculationSection(IContainer container, Core.Entities.MotorQuote quote)
    {
        container.Background(Colors.Blue.Lighten5).Padding(15).Column(column =>
        {
            column.Item().Text("CALCUL DE LA PRIME").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
            
            column.Item().PaddingTop(10).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Prime de base:");
                    row.ConstantItem(150).AlignRight().Text(FormatCurrency(quote.BasePremium));
                });
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Prime des sections:");
                    row.ConstantItem(150).AlignRight().Text(FormatCurrency(quote.SectionsPremium));
                });
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Sous-total:");
                    row.ConstantItem(150).AlignRight().Text(FormatCurrency(quote.Subtotal));
                });

                if (quote.TotalDiscount > 0)
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Remise ({quote.ProfessionalDiscountPercent + quote.CommercialDiscountPercent}%):")
                            .FontColor(Colors.Green.Medium);
                        row.ConstantItem(150).AlignRight().Text($"- {FormatCurrency(quote.TotalDiscount)}")
                            .FontColor(Colors.Green.Medium);
                    });
                }

                if (quote.ShortTermCoefficient < 1)
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Coefficient court terme (×{quote.ShortTermCoefficient:F2}):");
                        row.ConstantItem(150).AlignRight().Text(FormatCurrency(quote.NetPremium));
                    });
                }

                col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Prime nette:").Bold();
                    row.ConstantItem(150).AlignRight().Text(FormatCurrency(quote.NetPremium)).Bold();
                });
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Taxes (14.5%):");
                    row.ConstantItem(150).AlignRight().Text(FormatCurrency(quote.TaxAmount));
                });
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Frais de police:");
                    row.ConstantItem(150).AlignRight().Text(FormatCurrency(quote.PolicyCostAmount));
                });

                col.Item().PaddingVertical(5).LineHorizontal(2).LineColor(Colors.Blue.Darken2);

                col.Item().Background(Colors.Blue.Darken2).Padding(8).Row(row =>
                {
                    row.RelativeItem().Text("PRIME TOTALE").Bold().FontSize(12).FontColor(Colors.White);
                    row.ConstantItem(150).AlignRight().Text(FormatCurrency(quote.TotalPremium)).Bold().FontSize(14).FontColor(Colors.White);
                });
            });
        });
    }

    private void PremiumCalculationSectionPolicy(IContainer container, Core.Entities.MotorPolicy policy)
    {
        container.Background(Colors.Blue.Lighten5).Padding(15).Column(column =>
        {
            column.Item().Text("DÉTAIL DE LA PRIME").Bold().FontSize(12).FontColor(Colors.Blue.Darken2);
            
            column.Item().PaddingTop(10).Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Prime nette:").Bold();
                    row.ConstantItem(150).AlignRight().Text(FormatCurrency(policy.NetPremium)).Bold();
                });
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Taxes:");
                    row.ConstantItem(150).AlignRight().Text(FormatCurrency(policy.TaxAmount));
                });
                col.Item().Row(row =>
                {
                    row.RelativeItem().Text("Frais de police:");
                    row.ConstantItem(150).AlignRight().Text(FormatCurrency(policy.PolicyCostAmount));
                });

                col.Item().PaddingVertical(5).LineHorizontal(2).LineColor(Colors.Blue.Darken2);

                col.Item().Background(Colors.Blue.Darken2).Padding(8).Row(row =>
                {
                    row.RelativeItem().Text("PRIME TOTALE").Bold().FontSize(12).FontColor(Colors.White);
                    row.ConstantItem(150).AlignRight().Text(FormatCurrency(policy.TotalPremium)).Bold().FontSize(14).FontColor(Colors.White);
                });
            });
        });
    }

    private void GeneralConditions(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text("CONDITIONS GÉNÉRALES").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
            column.Item().PaddingTop(5).Text(text =>
            {
                text.Span("1. ").Bold();
                text.Span("La présente police est soumise aux conditions générales d'assurance automobile en vigueur.").FontSize(8);
            });
            column.Item().Text(text =>
            {
                text.Span("2. ").Bold();
                text.Span("L'assuré s'engage à déclarer tout sinistre dans les 5 jours ouvrables.").FontSize(8);
            });
            column.Item().Text(text =>
            {
                text.Span("3. ").Bold();
                text.Span("Le paiement de la prime est requis pour la prise d'effet de la garantie.").FontSize(8);
            });
        });
    }

    // Méthodes utilitaires

    private string GetClientName(Core.Entities.Client client)
    {
        return client.Type == Core.Entities.ClientType.Individual 
            ? $"{client.FirstName} {client.LastName}"
            : client.CompanyName ?? string.Empty;
    }

    private string GetQuoteStatusText(Core.Entities.QuoteStatus status)
    {
        return status switch
        {
            Core.Entities.QuoteStatus.Draft => "Brouillon",
            Core.Entities.QuoteStatus.Generated => "Généré",
            Core.Entities.QuoteStatus.Accepted => "Accepté",
            Core.Entities.QuoteStatus.Rejected => "Rejeté",
            Core.Entities.QuoteStatus.Expired => "Expiré",
            _ => status.ToString()
        };
    }

    private string GetEndorsementTypeText(string type)
    {
        return type switch
        {
            "AddCoverage" => "Ajout de garantie",
            "RemoveCoverage" => "Retrait de garantie",
            "ChangeVehicleValue" => "Modification de la valeur du véhicule",
            "Suspension" => "Suspension de la police",
            "Cancellation" => "Annulation de la police",
            _ => type
        };
    }

    private string FormatCurrency(decimal amount)
    {
        return $"{amount:N0} FCFA";
    }
}
