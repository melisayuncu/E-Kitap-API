using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace E_Kitap_API.Services
{
	// The document that outlines the actual book
	public class BookDocument : IDocument
	{
		private readonly string _bookName;
		private readonly List<SubmissionContent> _submissions;

		public BookDocument(string bookName, List<SubmissionContent> submissions)
		{
			_bookName = bookName;
			_submissions = submissions;
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
					// Kapak + İçindekiler
					column.Item().Text(_bookName).FontSize(20).Bold();
					column.Item().PaddingTop(15).Text("İçindekiler").FontSize(14).Bold();
					column.Item().PaddingTop(10);

					foreach (var sub in _submissions)
					{
						column.Item().PaddingBottom(4).Row(row =>
						{
							row.RelativeItem().Text(sub.Title).FontSize(11);
							row.ConstantItem(40).AlignRight().Text(sub.StartPage.ToString()).FontSize(11);
						});
					}

					column.Item().PageBreak();

					// Bildirilerin gövdesi
					for (int i = 0; i < _submissions.Count; i++)
					{
						SubmissionContentComposer.Compose(column, _submissions[i]);

						if (i < _submissions.Count - 1)
							column.Item().PageBreak();
					}
				});
			});
		}
	}
}