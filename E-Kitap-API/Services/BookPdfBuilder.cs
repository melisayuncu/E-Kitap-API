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
			// 1. Her bildiriyi Word'den oku ve iletişim bilgilerini temizle
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

			// 2. Her bildirinin tek başına kaç sayfa tuttuğunu ölç
			foreach (var content in contents)
			{
				var measureDoc = new SubmissionPdfDocument(content);
				var bytes = measureDoc.GeneratePdf();
				content.PageCount = _pageCounter.CountPages(bytes);
			}

			// 3. İçindekiler için başlangıç sayfalarını hesapla
			var currentPage = TocPageCount + 1;
			foreach (var content in contents)
			{
				content.StartPage = currentPage;
				currentPage += content.PageCount;
			}

			// 4. Asıl kitabı üret
			var bookDocument = new BookDocument(bookName, contents);
			return bookDocument.GeneratePdf();
		}
	}
}