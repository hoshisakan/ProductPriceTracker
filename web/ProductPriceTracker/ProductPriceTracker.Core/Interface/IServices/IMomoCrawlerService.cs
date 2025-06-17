using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductPriceTracker.Core.Entities;

namespace ProductPriceTracker.Core.Interface.IServices
{
    public interface IMomoCrawlerService
    {
        Task<List<Product>> GetProductsAsync(string keyword, int maxPages, string taskId);
    }
}