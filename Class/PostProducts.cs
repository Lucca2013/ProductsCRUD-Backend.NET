using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace task2Backend
{
    public class PostProducts
    {
        public string Name { get; set; } = string.Empty;
        public string Desc { get; set; } = string.Empty;
        public string Price { get; set; } = string.Empty;
        public byte[] ImgUrl { get; set; }
    }
}