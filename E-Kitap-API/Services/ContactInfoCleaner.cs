using System.Text.RegularExpressions;

namespace E_Kitap_API.Services
{
	public class ContactInfoCleaner
	{
		private static readonly Regex EmailRegex = new(
			@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b",
			RegexOptions.Compiled);

		// ORCID numarasını koruma altına almak için
		private static readonly Regex OrcidRegex = new(
			@"ORCID[:\s]*[\d\-]{10,25}",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		// Telefon olabilecek aday diziler (rakam + boşluk/parantez/tire/nokta karışımı)
		private static readonly Regex PhoneCandidateRegex = new(
			@"\+?\(?\d[\d\s().\-]{6,18}\d",
			RegexOptions.Compiled);

		// Çok spesifik, tam ifadeler: kolon şartı yok, yanlış eşleşme riski çok düşük
		private static readonly Regex CompoundLabelRegex = new(
			@"\b(E-posta\s*/\s*GSM|Cep\s+telefonu)\b\s*[:./]?\s*",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		// Tekil, belirsiz kelimeler: MUTLAKA kelime sınırı + MUTLAKA arkasında ':' veya '.' olmalı
		// (kolon şartı olmazsa "nitel", "İletişim Fakültesi", "Mobile Applications" gibi
		//  gerçek içerik kelimeleri yanlışlıkla silinir — bunu regresyon testiyle doğruladık)
		private static readonly Regex SimpleLabelRegex = new(
			@"\b(Tel\.?No|Telefon|Tel|E-posta|Eposta|E-mail|Email|Mail|GSM|Cep|Mobile|Mobil|İletişim|İrtibat)\s*[:.]\s*",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);
		// YENİ: satırın TAM SONUNDA, arkasında gerçek içerik kalmamış etiket kelimelerini temizle
		// (örn. "E-mail | Mobile" veya tek başına "Telefon" — telefon numarası zaten silinmiş,
		//  geriye sadece anlamsız etiket kalmış; ama "Mobile Applications" gibi arkasında
		//  gerçek kelime varsa bu regex eşleşmez, dokunmaz)
		private static readonly Regex TrailingLabelRegex = new(
			@"(\b(?:Tel\.?No|Telefon|Tel|E-posta|Eposta|E-mail|Email|Mail|GSM|Cep|Mobile|Mobil|İletişim|İrtibat)\b\s*[|/]?\s*)+$",
			RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public string Clean(string text)
		{
			if (string.IsNullOrEmpty(text))
				return text;

			// 1. ORCID numaralarını geçici olarak koru
			var orcidMatches = new List<string>();
			var t = OrcidRegex.Replace(text, match =>
			{
				orcidMatches.Add(match.Value);
				return $"\uE000{orcidMatches.Count - 1}\uE000";
			});

			// 2. E-posta adreslerini temizle
			t = EmailRegex.Replace(t, "");

			// 3. Telefon numaralarını temizle (gerçek rakam sayısı 10-12 arasındaysa)
			t = PhoneCandidateRegex.Replace(t, match =>
			{
				var digitCount = match.Value.Count(char.IsDigit);
				return (digitCount >= 10 && digitCount <= 12) ? "" : match.Value;
			});

			// 4. İletişim etiketlerini temizle (önce spesifik/bileşik, sonra tekil-ama-kolonlu olanlar)
			t = CompoundLabelRegex.Replace(t, "");
			t = SimpleLabelRegex.Replace(t, "");
			t = TrailingLabelRegex.Replace(t, "");

			// 5. Geriye kalan dağınık ayraçları temizle
			t = Regex.Replace(t, @"^(?:\s*[|/;,\-])+\s*", "");
			t = Regex.Replace(t, @"(?:[|/;,\-]\s*)+$", "");
			t = Regex.Replace(t, @"\s{2,}", " ").Trim();

			// 6. ORCID numaralarını geri yükle
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