using System.ComponentModel.DataAnnotations;

namespace E_Kitap_API.Models
{
	public class Book
	{
		public int Id { get; set; } //identity

		[Required]
		[MaxLength(200)] 
		public string Name { get; set; } = string.Empty;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public BookStatus Status { get; set; } = BookStatus.Pending;

		// Path to the generated PDF on the server (filled once processing completes)
		public string? PdfFilePath { get; set; }

		// Short error description if generation fails
		public string? ErrorMessage { get; set; }

		// A book has many submissions
		public List<Submission> Submissions { get; set; } = new();
	}
}