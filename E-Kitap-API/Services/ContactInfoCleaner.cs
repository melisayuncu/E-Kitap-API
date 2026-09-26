using System.Text.RegularExpressions;

namespace E_Kitap_API.Services
{
	public class ContactInfoCleaner
	{
		private static readonly Regex EmailRegex = new(
			@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b",
			RegexOptions.Compiled);

		private static readonly Regex PhoneRegex = new(
			@"\b(?:\+90[\s.-]?)?0?\d{3}[\s.-]?\d{3}[\s.-]?\d{2}[\s.-]?\d{2}\b",
			RegexOptions.Compiled);

		public string Clean(string text)
		{
			if (string.IsNullOrEmpty(text))
				return text;

			var cleaned = EmailRegex.Replace(text, "");
			cleaned = PhoneRegex.Replace(cleaned, "");
			return cleaned;
		}

		public List<string> Clean(List<string> paragraphs)
		{
			return paragraphs.Select(Clean).ToList();
		}
	}
}