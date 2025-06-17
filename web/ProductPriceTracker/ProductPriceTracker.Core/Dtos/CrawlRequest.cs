using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductPriceTracker.Core.Dtos
{
    public class CrawlRequest
    {
        public string Mode { get; set; } = string.Empty;   // "pchome" 或 "momo"
        public string Keyword { get; set; } = string.Empty; 
        public int MaxPage { get; set; }
    }
}