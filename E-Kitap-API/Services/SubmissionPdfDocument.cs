using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace E_Kitap_API.Services
{
	// Tek bir bildirinin gövde içeriğini (başlık + paragraflar) sayfaya döken parça.
	// Hem "sayfa say" testinde hem asıl kitapta aynı şekilde kullanılır.
	public static class SubmissionContentComposer
	{
		public static void Compose(ColumnDescriptor column, SubmissionContent submission)
		{
			column.Item().Text(submission.Title)
				.FontSize(16).Bold();

			column.Item().PaddingTop(10);

			foreach (var paragraph in submission.Paragraphs.Skip(1)) // 0. paragraf başlıkla aynı, tekrar yazma
			{
				column.Item().PaddingBottom(8).Text(paragraph).FontSize(11).LineHeight(1.3f);
			}
		}
	}

	// Sadece "bu bildiri tek başına kaç sayfa tutuyor?" ölçümü için kullanılan yardımcı belge.
	public class SubmissionPdfDocument : IDocument
	{
		private readonly SubmissionContent _submission;

		public SubmissionPdfDocument(SubmissionContent submission)
		{
			_submission = submission;
		}

		public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

		public void Compose(IDocumentContainer container)
		{
			container.Page(page =>
			{
				page.Size(PageSizes.A4);
				page.Margin(2, Unit.Centimetre);
				page.DefaultTextStyle(x => x.FontSize(11));

				page.Footer().AlignCenter().Text(x =>
				{
					x.CurrentPageNumber();
				});

				page.Content().Column(column =>
				{
					SubmissionContentComposer.Compose(column, _submission);
				});
			});
		}
	}
}