using System;
using DataTables.NetStandard.Enhanced.Sample.Util;
using Newtonsoft.Json;

namespace DataTables.NetStandard.Enhanced.Sample.DataTables.ViewModels
{
    public class PersonViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string FullAddress { get; set; }
        public string Action { get; set; }
        public string Action2 { get; set; }
        public string Action3 { get; set; }

        [JsonConverter(typeof(LocalDateTimeConverter), "dd.MM.yyyy HH:mm:ss")]
        public DateTimeOffset DateOfBirth { get; set; }
    }
}
