using QuestPDF.Fluent;
using E_Kitap_API.Models;

namespace E_Kitap_API.Services
{
	public class BookPdfBuilder
	{
		private readonly WordDocumentReader _wordReader;
		private readonly ContactInfoCleaner _cleaner;
		private readonly PdfPageCounter _pageCounter;

		private const int TocPageCount = 1;

		public BookPdfBuilder(WordDocumentReader wordReader, ContactInfoCleaner cleaner, PdfPageCounter pageCounter)
		{
			_wordReader = wordReader;
			_cleaner = cleaner;
			_pageCounter = pageCounter;
		}

		public byte[] Build(string bookName, List<Submission> submissions, string wwwrootPath)
		{
			// Read each submission from Word and remove contact info
			var contents = new List<SubmissionContent>();
			foreach (var sub in submissions.OrderBy(s => s.Order))
			{
				var fullPath = Path.Combine(wwwrootPath, sub.OriginalFilePath);
				var parsed = _wordReader.Read(fullPath);
				var cleanedParagraphs = _cleaner.Clean(parsed.Paragraphs);

				contents.Add(new SubmissionContent
				{
					Order = sub.Order,
					Title = sub.Title,
					Paragraphs = cleanedParagraphs
				});
			}

			// Measure how many pages each individual report takes up
			foreach (var content in contents)
			{
				var measureDoc = new SubmissionPdfDocument(content);
				var bytes = measureDoc.GeneratePdf();
				content.PageCount = _pageCounter.CountPages(bytes);
			}

			// Calculate starting pages for the table of contents
			var currentPage = TocPageCount + 1;
			foreach (var content in contents)
			{
				content.StartPage = currentPage;
				currentPage += content.PageCount;
			}

			// Produce the original book
			var bookDocument = new BookDocument(bookName, contents);
			return bookDocument.GeneratePdf();
		}
	}
}