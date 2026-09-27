namespace E_Kitap_API.Models
{
	public class CreateBookResponse
	{
		public int Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public BookStatus Status { get; set; }
		public List<string> SubmissionFileNames { get; set; } = new(); // Which files are accepted
	}
}