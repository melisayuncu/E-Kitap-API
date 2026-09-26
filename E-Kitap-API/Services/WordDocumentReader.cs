using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace E_Kitap_API.Services
{
	public class ParsedDocument
	{
		public string Title { get; set; } = string.Empty;
		public List<string> Paragraphs { get; set; } = new();
	}

	public class WordDocumentReader
	{
		public ParsedDocument Read(string filePath)
		{
			var result = new ParsedDocument();

			using (var wordDoc = WordprocessingDocument.Open(filePath, false))
			{
				var body = wordDoc.MainDocumentPart?.Document?.Body;
				if (body == null)
					return result;

				string? detectedTitle = null;

				foreach (var para in body.Elements<Paragraph>())
				{
					var text = string.Concat(para.Descendants<Text>().Select(t => t.Text)).Trim();
					if (string.IsNullOrWhiteSpace(text))
						continue;

					result.Paragraphs.Add(text);

					// Başlığı, "Title" stiline sahip ilk paragraftan belirle
					var styleId = para.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
					if (detectedTitle == null && styleId != null &&
						styleId.Equals("Title", StringComparison.OrdinalIgnoreCase))
					{
						detectedTitle = text;
					}
				}

				// Title stili bulunamazsa, ilk dolu paragrafı başlık say
				result.Title = detectedTitle ?? result.Paragraphs.FirstOrDefault() ?? "Başlıksız";
			}

			return result;
		}
	}
}