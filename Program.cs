using DocumentSearch.Data;
using DocumentSearch.Repository;
using DocumentSearch.Repository.Implementations;
using DocumentSearch.Repository.Interfaces;
using DocumentSearch.Services;
using DocumentSearch.Services.Implementationss;
using DocumentSearch.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// OpenAPI
builder.Services.AddOpenApi();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractorService>();
builder.Services.AddScoped<IOcrService, OcrService>();
builder.Services.AddScoped<IPdfRenderer, PdfRenderer>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();