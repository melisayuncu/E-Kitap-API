using Microsoft.AspNetCore.Mvc;
using E_Kitap_API.Data;
using E_Kitap_API.Models;
using E_Kitap_API.Services;
using Microsoft.EntityFrameworkCore;

namespace E_Kitap_API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class BooksController : ControllerBase
	{
		private readonly EkitapDbContext _context;
		private readonly IWebHostEnvironment _env;
		private readonly WordDocumentReader _wordReader;
		private readonly BookPdfBuilder _pdfBuilder;

		public BooksController(EkitapDbContext context, IWebHostEnvironment env, WordDocumentReader wordReader, BookPdfBuilder pdfBuilder)
		{
			_context = context;
			_env = env;
			_wordReader = wordReader;
			_pdfBuilder = pdfBuilder;
		}

		[HttpPost]
		[RequestSizeLimit(100_000_000)]
		public async Task<ActionResult<CreateBookResponse>> CreateBook(
			[FromForm] string bookName,
			[FromForm] List<IFormFile> files)
		{
			if (string.IsNullOrWhiteSpace(bookName))
				return BadRequest("Kitap adı boş olamaz.");

			if (files == null || files.Count != 10)
				return BadRequest("Tam olarak 10 adet .docx dosyası yüklenmelidir.");

			foreach (var file in files)
			{
				var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
				if (ext != ".docx")
					return BadRequest($"Yalnızca .docx dosyaları kabul edilir: {file.FileName}");
			}

			var book = new Book
			{
				Name = bookName,
				Status = BookStatus.Pending
			};
			_context.Books.Add(book);
			await _context.SaveChangesAsync();

			var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads", book.Id.ToString());
			Directory.CreateDirectory(uploadsRoot);

			var order = 1;
			foreach (var file in files)
			{
				var savedFileName = $"{order:D2}_{file.FileName}";
				var fullPath = Path.Combine(uploadsRoot, savedFileName);

				using (var stream = new FileStream(fullPath, FileMode.Create))
				{
					await file.CopyToAsync(stream);
				}

				// Gerçek başlığı docx içeriğinden çıkar
				string title;
				try
				{
					var parsed = _wordReader.Read(fullPath);
					title = parsed.Title;
				}
				catch
				{
					// Okuma başarısız olursa dosya adına geri düş, akışı durdurma
					title = Path.GetFileNameWithoutExtension(file.FileName);
				}

				var submission = new Submission
				{
					BookId = book.Id,
					Title = title,
					FileName = file.FileName,
					OriginalFilePath = Path.Combine("uploads", book.Id.ToString(), savedFileName),
					Order = order
				};
				_context.Submissions.Add(submission);
				order++;
			}

			await _context.SaveChangesAsync();

			var response = new CreateBookResponse
			{
				Id = book.Id,
				Name = book.Name,
				Status = book.Status,
				SubmissionFileNames = files.Select(f => f.FileName).ToList()
			};

			return Ok(response);
		}
		[HttpPost("{id}/generate")]
		public async Task<IActionResult> GenerateBook(int id)
		{
			var book = await _context.Books
				.Include(b => b.Submissions)
				.FirstOrDefaultAsync(b => b.Id == id);

			if (book == null)
				return NotFound("Kitap bulunamadı.");

			if (book.Submissions.Count != 10)
				return BadRequest("Kitaba ait tam 10 bildiri bulunamadı.");

			book.Status = BookStatus.Processing;
			await _context.SaveChangesAsync();

			try
			{
				var pdfBytes = _pdfBuilder.Build(book.Name, book.Submissions.ToList(), _env.WebRootPath);

				var generatedDir = Path.Combine(_env.WebRootPath, "generated");
				Directory.CreateDirectory(generatedDir);

				var fileName = $"kitap_{book.Id}.pdf";
				var fullPath = Path.Combine(generatedDir, fileName);
				await System.IO.File.WriteAllBytesAsync(fullPath, pdfBytes);

				book.PdfFilePath = Path.Combine("generated", fileName);
				book.Status = BookStatus.Completed;
				book.ErrorMessage = null;
				await _context.SaveChangesAsync();

				return Ok(new { book.Id, book.Name, Status = book.Status.ToString(), PdfUrl = $"/generated/{fileName}" });
			}
			catch (Exception ex)
			{
				book.Status = BookStatus.Failed;
				book.ErrorMessage = ex.Message;
				await _context.SaveChangesAsync();

				return StatusCode(500, new { message = "PDF oluşturulurken bir hata oluştu.", detail = ex.Message });
			}
		}
	}
}