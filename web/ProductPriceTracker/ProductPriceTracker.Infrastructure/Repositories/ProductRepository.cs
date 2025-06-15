using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProductPriceTracker.Core.Entities;
using ProductPriceTracker.Core.Interface.IRepositories;

namespace ProductPriceTracker.Infrastructure.Data.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ScrapeDbContext _context;

        public ProductRepository(ScrapeDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id) ?? throw new KeyNotFoundException("Product not found");
        }

        public void Update(Product product)
        {
            _context.Products.Update(product);
        }

        public async Task<bool> IsProductExistsAsync(string productName, string productDescription)
        {
            return await _context.Products.AnyAsync(p => p.ProductName == productName && p.Description == productDescription);
        }
    }
}