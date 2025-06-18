using System;
using System.Collections.Generic;

namespace ProductPriceTracker.Core.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CrawlerTask> CrawlerTasks { get; set; } = new List<CrawlerTask>();

    public virtual ICollection<ProductHistory> ProductHistories { get; set; } = new List<ProductHistory>();

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
