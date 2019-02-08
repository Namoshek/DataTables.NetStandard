using System;
using System.ComponentModel.DataAnnotations;

namespace DataTables.NetCore.Sample.Models
{
    public class User
    {
        [Key]
        public long Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
    }
}
