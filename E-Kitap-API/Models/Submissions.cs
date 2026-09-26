using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Kitap_API.Models
{
	public class Submission
	{
		public int Id { get; set; }

		[Required]
		public int BookId { get; set; }

		[ForeignKey(nameof(BookId))]
		public Book? Book { get; set; }

		[Required]
		[MaxLength(300)]
		public string Title { get; set; } = string.Empty;

		[Required]
		[MaxLength(255)]
		public string FileName { get; set; } = string.Empty;

		// Path to the original uploaded docx on the server
		[Required]
		public string OriginalFilePath { get; set; } = string.Empty;

		// Order within the book (1 to 10)
		[Required]
		public int Order { get; set; }

		// Page number where this submission starts in the generated PDF (for table of contents)
		public int? StartPage { get; set; }
	}
}