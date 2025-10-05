using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRN232.Lab1.CoffeeStore.Services.Models.Requests
{
    public class ProductQueryRequest
    {
        public string? Search { get; set; }        
        public string? Sort { get; set; }        // e.g. "name,-price"
        public int Page { get; set; } = 1;          
        public int PageSize { get; set; } = 10;    
        public string? Select { get; set; }        

        public bool? IsActive { get; set; }
    }
}
