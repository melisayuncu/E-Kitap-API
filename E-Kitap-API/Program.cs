using Microsoft.EntityFrameworkCore;
using E_Kitap_API.Data;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community; // to use QuestPDF freely, we need to indicate "community" license

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<EkitapDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<E_Kitap_API.Services.WordDocumentReader>();

builder.Services.AddScoped<E_Kitap_API.Services.ContactInfoCleaner>();
builder.Services.AddScoped<E_Kitap_API.Services.PdfPageCounter>();
builder.Services.AddScoped<E_Kitap_API.Services.BookPdfBuilder>();

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
