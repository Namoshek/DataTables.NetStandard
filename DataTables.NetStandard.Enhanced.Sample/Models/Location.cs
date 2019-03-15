using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataTables.NetStandard.Enhanced.Sample.Models
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

        [NotMapped]
        public string FullAddress => Street + " " + HouseNumber + ", " + PostCode + " " + City + " (" + Country + ")";

        public long PersonId { get; set; }
        public Person Person { get; set; }
    }
}
