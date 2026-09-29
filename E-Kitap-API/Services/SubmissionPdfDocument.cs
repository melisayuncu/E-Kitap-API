using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace E_Kitap_API.Services
{
	// A component that renders the body content (title + paragraphs) of a single declaration onto the page. It is used in the same way for both page counting and the actual book.
	public static class SubmissionContentComposer
	{
		public static void Compose(ColumnDescriptor column, SubmissionContent submission)
		{
			column.Item().Text(submission.Title)
				.FontSize(16).Bold();

			column.Item().PaddingTop(10);

			foreach (var paragraph in submission.Paragraphs.Skip(1)) //The first paragraph is same with title
			{
				column.Item().PaddingBottom(8).Text(paragraph).FontSize(11).LineHeight(1.3f);
			}
		}
	}
	// A supplementary document used solely to measure how many pages this paper spans on its own.
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

				//page number
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