using System.ComponentModel.DataAnnotations;

namespace DataTables.NetCore.Sample.Models
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

        public long UserId { get; set; }
        public User User { get; set; }
    }
}
