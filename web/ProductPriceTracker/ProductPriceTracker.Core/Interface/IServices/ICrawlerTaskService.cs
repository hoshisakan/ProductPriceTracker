using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductPriceTracker.Core.Entities;

namespace ProductPriceTracker.Core.Interface.IServices
{
    public interface ICrawlerTaskService
    {
        Task<List<CrawlerTask>> GetAllCrawlerTasksAsync();
        Task<CrawlerTask> GetCrawlerTaskByIdAsync(int id);
        Task AddCrawlerTaskAsync(CrawlerTask crawlerTask);
        Task<bool> UpdateCrawlerTaskAsync(int id, CrawlerTask crawlerTask);
        Task<bool> DeleteCrawlerTaskAsync(int id);
    }
}