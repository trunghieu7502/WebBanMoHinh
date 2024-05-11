using DoAnCoSo_WebBanMoHinh.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo_WebBanMoHinh.Repositories
{
    public class EFCompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;
        public EFCompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            return await _context.Companies.ToListAsync();
        }
        public async Task<Company> GetByIdAsync(int id)
        {
            return await _context.Companies.FindAsync(id);
        }
        public async Task AddAsync(Company company)
        {
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Company company)
        {
            _context.Companies.Update(company);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();
        }
    }
}
