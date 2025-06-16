using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories;
using ProductPriceTracker.Core.Interface.IServices;

namespace ProductPriceTracker.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task AddProductAsync(Product product)
        {
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveAsync();
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.Products.GetProductByIdAsync(id);
            if (product != null)
            {
                _unitOfWork.Products.Remove(product);
                await _unitOfWork.SaveAsync();
                return true;
            }
            throw new KeyNotFoundException("Product not found");
        }

        public Task<List<Product>> GetAllProductsAsync()
        {
            return _unitOfWork.Products.GetAllAsync();
        }

        public Task<Product> GetProductByIdAsync(int id)
        {
            return _unitOfWork.Products.GetProductByIdAsync(id);
        }

        public async Task<bool> UpdateProductAsync(int id, Product product)
        {
            var existingProduct = _unitOfWork.Products.GetProductByIdAsync(id);
            if (existingProduct != null)
            {
                product.ProductId = id; // Ensure the ID is set correctly
                _unitOfWork.Products.Update(product);
                await _unitOfWork.SaveAsync();
                return true;
            }
            else
            {
                throw new KeyNotFoundException("Product not found");
            }
        }

        public async Task<bool> IsProductExistsAsync(string productName, string productDescription)
        {
            return await _unitOfWork.Products.IsProductExistsAsync(productName, productDescription);
        }

        public async Task<Product> GetByNameAndDescriptionAsync(string productName, string productDescription)
        {
            return await _unitOfWork.Products.GetByNameAndDescriptionAsync(productName, productDescription);
        }
    }
}