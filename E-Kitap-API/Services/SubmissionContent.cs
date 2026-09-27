namespace E_Kitap_API.Services
{
	// the content of the submissions 
	public class SubmissionContent
	{
		public int Order { get; set; }
		public string Title { get; set; } = string.Empty;
		public List<string> Paragraphs { get; set; } = new();
		public int PageCount { get; set; }
		public int StartPage { get; set; }
	}
}