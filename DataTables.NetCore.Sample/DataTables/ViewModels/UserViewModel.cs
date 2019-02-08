using System;
using DataTables.NetCore.Attributes;
using DataTables.NetCore.Sample.Models;

namespace DataTables.NetCore.Sample.DataTables.ViewModels
{
    [DTDataSource(typeof(User))]
    public class UserViewModel
    {
        [DTColumn("id", "ID", nameof(User.Id), true, true)]
        public long Id { get; set; }

        [DTColumn("name", "Name", nameof(User.Name), true, true)]
        public string Name { get; set; }

        [DTColumn("email", "Email", nameof(User.Email), true, true)]
        public string Email { get; set; }

        [DTColumn("dateOfBirth", "DateOfBirth", nameof(User.DateOfBirth), true, false)]
        public DateTimeOffset DateOfBirth { get; set; }
    }
}
