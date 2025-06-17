using System;
using System.Collections.Generic;

namespace ProductPriceTracker.Core.Entities;

public partial class CrawlerTask
{
    public int Id { get; set; }

    public string TaskId { get; set; } = null!;

    public string Source { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
