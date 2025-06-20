﻿using System;
using System.Collections.Generic;

namespace ProductPriceTracker.Core.Entities;

public partial class Product
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string TaskId { get; set; } = null!;

    public int UserId { get; set; }

    public virtual ICollection<ProductHistory> ProductHistories { get; set; } = new List<ProductHistory>();

    public virtual CrawlerTask Task { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
