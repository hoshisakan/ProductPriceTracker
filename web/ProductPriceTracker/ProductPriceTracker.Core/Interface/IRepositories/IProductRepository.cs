using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductPriceTracker.Core.Entities;

namespace ProductPriceTracker.Core.Interface.IRepositories
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);
        Task<Product> GetProductByIdAsync(int id);
        Task<List<Product>> GetProductsByTaskIdAsync(string taskId);
        Task<bool> IsProductExistsAsync(string productName, string productDescription);
        Task<Product> GetByNameAndDescriptionAsync(string productName, string productDescription);
    }
}