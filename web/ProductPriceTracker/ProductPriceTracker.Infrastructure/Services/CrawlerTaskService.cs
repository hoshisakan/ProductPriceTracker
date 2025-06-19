using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories;
using ProductPriceTracker.Core.Interface.IServices;

namespace ProductPriceTracker.Infrastructure.Services
{
    public class CrawlerTaskService : ICrawlerTaskService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CrawlerTaskService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddCrawlerTaskAsync(CrawlerTask crawlerTask)
        {
            await _unitOfWork.CrawlerTasks.AddAsync(crawlerTask);
            await _unitOfWork.SaveAsync();
        }

        public async Task<bool> DeleteCrawlerTaskAsync(int id)
        {
            var crawlerTask = await _unitOfWork.CrawlerTasks.GetCrawlerTaskByIdAsync(id);
            if (crawlerTask != null)
            {
                _unitOfWork.CrawlerTasks.Remove(crawlerTask);
                await _unitOfWork.SaveAsync();
                return true;
            }
            throw new KeyNotFoundException("Crawler task not found");
        }

        public Task<List<CrawlerTask>> GetAllCrawlerTasksAsync(int userId)
        {
            return _unitOfWork.CrawlerTasks.GetAllByUserIdAsync(userId);
        }

        public Task<CrawlerTask> GetCrawlerTaskByIdAsync(int id)
        {
            return _unitOfWork.CrawlerTasks.GetCrawlerTaskByIdAsync(id);
        }

        public async Task<bool> UpdateCrawlerTaskAsync(int id, CrawlerTask crawlerTask)
        {
            var existingTask = await _unitOfWork.CrawlerTasks.GetCrawlerTaskByIdAsync(id);
            if (existingTask != null)
            {
                crawlerTask.Id = id; // Ensure the ID is set correctly
                _unitOfWork.CrawlerTasks.Update(crawlerTask);
                await _unitOfWork.SaveAsync();
                return true;
            }
            else
            {
                throw new KeyNotFoundException("Crawler task not found");
            }
        }
    }
}