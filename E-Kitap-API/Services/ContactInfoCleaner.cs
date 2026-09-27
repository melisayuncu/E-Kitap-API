using System.Text.RegularExpressions;

namespace E_Kitap_API.Services
{
	public class ContactInfoCleaner
	{
		private static readonly Regex EmailRegex = new(
			@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b",
			RegexOptions.Compiled);

		// to protect ORCID number
		private static readonly Regex OrcidRegex = new(
			@"ORCID[:\s]*[\d\-]{10,25}",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		// Candidate strings that could be phone numbers (a mix of digits and spaces/parentheses/hyphens/dots)
		private static readonly Regex PhoneCandidateRegex = new(
			@"\+?\(?\d[\d\s().\-]{6,18}\d",
			RegexOptions.Compiled);

		// Very specific, precise expressions
		private static readonly Regex CompoundLabelRegex = new(
			@"\b(E-posta\s*/\s*GSM|Cep\s+telefonu)\b\s*[:./]?\s*",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		// Singular indefinite words: must have a word boundary + must be followed by ':' or '.'
		private static readonly Regex SimpleLabelRegex = new(
			@"\b(Tel\.?No|Telefon|Tel|E-posta|Eposta|E-mail|Email|Mail|GSM|Cep|Mobile|Mobil|İletişim|İrtibat)\s*[:.]\s*",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);
		// Clean up label words at the very end of the line that have no actual content following them (for ex: "E-mail | Mobile" or just "Phone" — the phone number has already been deleted,
		//  leaving only a meaningless label; however, if there is an actual word following it, such as "Mobile Applications," this regex will not match and will leave it untouched)
		private static readonly Regex TrailingLabelRegex = new(
			@"(\b(?:Tel\.?No|Telefon|Tel|E-posta|Eposta|E-mail|Email|Mail|GSM|Cep|Mobile|Mobil|İletişim|İrtibat)\b\s*[|/]?\s*)+$",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public string Clean(string text)
		{
			if (string.IsNullOrEmpty(text))
				return text;

			// Temporarily protect ORCID numbers
			var orcidMatches = new List<string>();
			var t = OrcidRegex.Replace(text, match =>
			{
				orcidMatches.Add(match.Value);
				return $"\uE000{orcidMatches.Count - 1}\uE000";
			});

			// Clean the email addresses
			t = EmailRegex.Replace(t, "");

			// clear the phone numbers
			t = PhoneCandidateRegex.Replace(t, match =>
			{
				var digitCount = match.Value.Count(char.IsDigit);
				return (digitCount >= 10 && digitCount <= 12) ? "" : match.Value;
			});

			// Clear communication tags
			t = CompoundLabelRegex.Replace(t, "");
			t = SimpleLabelRegex.Replace(t, "");
			t = TrailingLabelRegex.Replace(t, "");

			// Clean up any remaining messy brackets.
			t = Regex.Replace(t, @"^(?:\s*[|/;,\-])+\s*", "");
			t = Regex.Replace(t, @"(?:[|/;,\-]\s*)+$", "");
			t = Regex.Replace(t, @"\s{2,}", " ").Trim();

			// Reload the ORCID numbers
			for (int i = 0; i < orcidMatches.Count; i++)
			{
				t = t.Replace($"\uE000{i}\uE000", orcidMatches[i]);
			}

			return t;
		}

		public List<string> Clean(List<string> paragraphs)
		{
			return paragraphs.Select(Clean).ToList();
		}
	}
}