using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductPriceTracker.Core.Entities;

namespace ProductPriceTracker.Core.Interface.IServices
{
    public interface IProductHistoryService
    {
        Task<List<ProductHistory>> GetAllProductHitoriesAsync();
        Task<List<ProductHistory>> GetProductHistoriesByTaskIdAsync(string taskId);
        Task<ProductHistory> GetProductHistoryByIdAsync(int id);
        Task AddProductHistoryAsync(ProductHistory productHistory);
        Task<bool> UpdateProductHistoryAsync(int id, ProductHistory productHistory);
        Task<bool> DeleteProductHistoryAsync(int id);
    }
}