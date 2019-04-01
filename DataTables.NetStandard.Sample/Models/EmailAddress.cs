using System.ComponentModel.DataAnnotations;

namespace DataTables.NetStandard.Sample.Models
{
    public class EmailAddress
    {
        [Key]
        public long Id { get; set; }
        public string Address { get; set; }

        public Person Person { get; set; }
    }
}
