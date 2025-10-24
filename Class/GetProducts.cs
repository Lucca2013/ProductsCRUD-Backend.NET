using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace task2Backend
{
    public class GetProducts
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string Price { get; set; }
        public string ImgUrl { get; set; }
        public string CreatedAt { get; set; }
    }
}