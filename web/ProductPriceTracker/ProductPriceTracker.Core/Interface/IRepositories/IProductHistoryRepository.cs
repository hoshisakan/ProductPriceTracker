using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductPriceTracker.Core.Entities;

namespace ProductPriceTracker.Core.Interface.IRepositories
{
    public interface IProductHistoryRepository : IRepository<ProductHistory>
    {
        void Update(ProductHistory product);
        Task<ProductHistory> GetProductHistoryByIdAsync(int id);
        Task<List<ProductHistory>> GetProductHistoriesByTaskIdAsync(string taskId);
    }
}