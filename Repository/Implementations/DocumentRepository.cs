using Microsoft.EntityFrameworkCore;
using DocumentSearch.Data;
using DocumentSearch.Models;
using DocumentSearch.Repository.Interfaces;

namespace DocumentSearch.Repository.Implementations
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly ApplicationDbContext _context;
        public DocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Document>> GetAllAsync(int employeeId)
        {
            return await _context.Documents
                .AsNoTracking()
                .Where(d => d.UploadedByEmployeeId == employeeId)
                .ToListAsync();
        }

        public async Task<Document?> GetByIdAsync(int id, int employeeId)
        {
            return await _context.Documents
                .FirstOrDefaultAsync(d => d.Id == id && d.UploadedByEmployeeId == employeeId);
        }

        public async Task<Document> AddAsync(Document document)
        {
            await _context.Documents.AddAsync(document);
            await _context.SaveChangesAsync();

            return document;
        }

        public async Task UpdateAsync(Document document)
        {
            _context.Documents.Update(document);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Document document)
        {
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Document>> SearchAsync(string query, int employeeId)
        {
            return await _context.Documents
                .AsNoTracking()
                .Where(d => d.UploadedByEmployeeId == employeeId &&
                            d.ExtractedText != null &&
                            d.ExtractedText.Contains(query))
                .ToListAsync();
        }
    }
}
