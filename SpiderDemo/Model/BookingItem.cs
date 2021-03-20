using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MisterSpider.Model
{
    public class BookingItem
    {
        public string Title { get; set; }

        public string Location { get; set; }

        public string Stars { get; set; }

        public string Rating { get; set; }

        public string Description { get; set; }

        public string Latitude { get; set; }

        public string Longitude { get; set; }

        public string HotelUrl { get; set; }

        public List<string> Images { get; set; }

        public List<string> Facilities { get; set; }

        public BookingItem()
        {
            Images = new List<string>();
            Facilities = new List<string>();
        }

    }
}
