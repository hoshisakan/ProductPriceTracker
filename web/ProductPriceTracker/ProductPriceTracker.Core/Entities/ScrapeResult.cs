using System;
using System.Collections.Generic;

namespace ProductPriceTracker.Core.Entities;

public partial class ScrapeResult
{
    public int Id { get; set; }

    public string Url { get; set; } = null!;

    public string Content { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
