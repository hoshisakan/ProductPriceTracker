using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories;
using ProductPriceTracker.Core.Interface.IServices;

namespace ProductPriceTracker.Infrastructure.Services
{
    public class ProductHistoryService : IProductHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductHistoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddProductHistoryAsync(ProductHistory productHistory)
        {
            await _unitOfWork.ProductHistories.AddAsync(productHistory);
            await _unitOfWork.SaveAsync();
        }

        public async Task<bool> DeleteProductHistoryAsync(int id)
        {
            var productHistory = await _unitOfWork.ProductHistories.GetProductHistoryByIdAsync(id);
            if (productHistory != null)
            {
                _unitOfWork.ProductHistories.Remove(productHistory);
                await _unitOfWork.SaveAsync();
                return true;
            }
            throw new KeyNotFoundException("Product history not found");
        }

        public Task<List<ProductHistory>> GetAllProductHitoriesAsync()
        {
            return _unitOfWork.ProductHistories.GetAllAsync();
        }

        public Task<ProductHistory> GetProductHistoryByIdAsync(int id)
        {
            return _unitOfWork.ProductHistories.GetProductHistoryByIdAsync(id);
        }

        public async Task<bool> UpdateProductHistoryAsync(int id, ProductHistory productHistory)
        {
            var existingProduct = _unitOfWork.ProductHistories.GetProductHistoryByIdAsync(id);
            if (existingProduct != null)
            {
                productHistory.HistoryId = id; // Ensure the ID is set correctly
                _unitOfWork.ProductHistories.Update(productHistory);
                await _unitOfWork.SaveAsync();
                return true;
            }
            else
            {
                throw new KeyNotFoundException("Product history not found");
            }
        }
    }
}