using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RZNU_SimpleRest.Models
{
    public class Photo
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string Url { get; set; }

    }
}