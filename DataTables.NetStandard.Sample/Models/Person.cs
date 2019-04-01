using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataTables.NetStandard.Sample.Models
{
    public class Person
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }

        public Location Location { get; set; }
        public ICollection<EmailAddress> EmailAddresses { get; set; }
    }
}
