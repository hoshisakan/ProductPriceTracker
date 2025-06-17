using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories;

namespace ProductPriceTracker.Infrastructure.Data.Repositories
{
    public class CrawlerTaskRepository : Repository<CrawlerTask>, ICrawlerTaskRepository
    {
        private readonly ScrapeDbContext _context;

        public CrawlerTaskRepository(ScrapeDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CrawlerTask> GetCrawlerTaskByIdAsync(int id)
        {
            return await _context.CrawlerTasks.FindAsync(id) ?? throw new KeyNotFoundException("Product not found");
        }

        public void Update(CrawlerTask crawlerTask)
        {
            _context.CrawlerTasks.Update(crawlerTask);
        }
    }
}