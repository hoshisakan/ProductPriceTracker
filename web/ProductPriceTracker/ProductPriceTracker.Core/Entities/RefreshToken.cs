﻿using System;
using System.Collections.Generic;

namespace ProductPriceTracker.Core.Entities;

public partial class RefreshToken
{
    public int Id { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public int UserId { get; set; }

    public virtual User User { get; set; } = null!;
}
