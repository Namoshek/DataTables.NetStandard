using System;
using DataTables.NetCore.Sample.Util;
using Newtonsoft.Json;

namespace DataTables.NetCore.Sample.DataTables.ViewModels
{
    public class UserViewModel
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Action { get; set; }

        [JsonConverter(typeof(LocalDateTimeConverter), "dd.MM.yyyy HH:mm:ss")]
        public DateTimeOffset DateOfBirth { get; set; }
    }
}
