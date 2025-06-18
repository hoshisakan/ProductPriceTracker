using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductPriceTracker.Core.Interface.IRepositories;


namespace ProductPriceTracker.Infrastructure.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ScrapeDbContext _context;

        public UnitOfWork(ScrapeDbContext context)
        {
            _context = context;
            Products = new ProductRepository(context);
            ProductHistories = new ProductHistoryRepository(context);
            CrawlerTasks = new CrawlerTaskRepository(context);
            Users = new UserRepository(context);
        }

        public IProductRepository Products { get; private set; }
        public IProductHistoryRepository ProductHistories { get; private set; }
        public ICrawlerTaskRepository CrawlerTasks { get; private set; }
        public IUserRepository Users { get; private set; }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}