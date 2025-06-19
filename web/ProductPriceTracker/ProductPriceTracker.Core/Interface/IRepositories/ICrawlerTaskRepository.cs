using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductPriceTracker.Core.Entities;

namespace ProductPriceTracker.Core.Interface.IRepositories
{
    public interface ICrawlerTaskRepository : IRepository<CrawlerTask>
    {
        void Update(CrawlerTask crawlerTask);
        Task<CrawlerTask> GetCrawlerTaskByIdAsync(int id);
        Task<List<CrawlerTask>> GetAllByUserIdAsync(int userId);
    }
}