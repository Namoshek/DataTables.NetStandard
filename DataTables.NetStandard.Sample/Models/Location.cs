﻿using System.ComponentModel.DataAnnotations;

namespace DataTables.NetStandard.Sample.Models
{
    public class Location
    {
        [Key]
        public long Id { get; set; }
        public string Street { get; set; }
        public string HouseNumber { get; set; }
        public string PostCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public long PersonId { get; set; }
        public Person Person { get; set; }
    }
}
