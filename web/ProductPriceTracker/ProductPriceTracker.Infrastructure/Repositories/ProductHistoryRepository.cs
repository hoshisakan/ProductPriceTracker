using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories;

namespace ProductPriceTracker.Infrastructure.Data.Repositories
{
    public class ProductHistoryRepository : Repository<ProductHistory>, IProductHistoryRepository
    {
        private readonly ScrapeDbContext _context;

        public ProductHistoryRepository(ScrapeDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ProductHistory> GetProductHistoryByIdAsync(int id)
        {
            return await _context.ProductHistories.FindAsync(id) ?? throw new KeyNotFoundException("Product history not found");
        }

        public async Task<List<ProductHistory>> GetProductHistoriesByTaskIdAsync(string taskId)
        {
            return await _context.ProductHistories
                .Where(ph => ph.TaskId == taskId)
                .ToListAsync();
        }

        public void Update(ProductHistory productHistory)
        {
            _context.ProductHistories.Update(productHistory);
        }
    }
}