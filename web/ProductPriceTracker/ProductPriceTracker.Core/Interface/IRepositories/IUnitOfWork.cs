using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ProductPriceTracker.Core.Interface.IRepositories
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }
        Task SaveAsync();
    }
}