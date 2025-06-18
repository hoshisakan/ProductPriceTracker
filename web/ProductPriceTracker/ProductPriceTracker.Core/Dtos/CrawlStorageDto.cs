using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProductPriceTracker.Core.Dtos
{
    public class CrawlStorageDto
    {
        public int UserId { get; set; } = 0; // 用戶ID，默認為0
        public string Mode { get; set; } = string.Empty;   // "pchome" 或 "momo"
        public string Keyword { get; set; } = string.Empty; 
        public int MaxPage { get; set; }
    }
}