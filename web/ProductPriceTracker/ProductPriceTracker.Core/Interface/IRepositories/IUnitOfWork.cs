using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ProductPriceTracker.Core.Interface.IRepositories
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }
        IProductHistoryRepository ProductHistories { get; }
        ICrawlerTaskRepository CrawlerTasks { get; }
        IUserRepository Users { get; }
        Task SaveAsync();
    }
}