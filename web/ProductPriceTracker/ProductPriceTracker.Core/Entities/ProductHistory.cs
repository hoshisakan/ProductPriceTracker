using System;
using System.Collections.Generic;

namespace ProductPriceTracker.Core.Entities;

public partial class ProductHistory
{
    public int HistoryId { get; set; }

    public int ProductId { get; set; }

    public string TaskId { get; set; } = null!;

    public int UserId { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public DateTime CapturedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
