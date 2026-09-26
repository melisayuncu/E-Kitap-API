using PdfSharpCore.Pdf.IO;

namespace E_Kitap_API.Services
{
	public class PdfPageCounter
	{
		public int CountPages(byte[] pdfBytes)
		{
			using var stream = new MemoryStream(pdfBytes);
			using var document = PdfReader.Open(stream, PdfDocumentOpenMode.InformationOnly);
			return document.PageCount;
		}
	}
}