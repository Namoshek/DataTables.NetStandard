using System;
using DataTables.NetCore.Attributes;
using DataTables.NetCore.Sample.Models;

namespace DataTables.NetCore.Sample.DataTables.ViewModels
{
    public class UserViewModel
    {
        [DataTableColumn("id", "ID", nameof(User.Id), true, true)]
        public long Id { get; set; }

        [DataTableColumn("name", "Name", nameof(User.Name), true, true)]
        public string Name { get; set; }

        [DataTableColumn("email", "Email", nameof(User.Email), true, true)]
        public string Email { get; set; }

        [DataTableColumn("dateOfBirth", "DateOfBirth", nameof(User.DateOfBirth), true, false)]
        public DateTimeOffset DateOfBirth { get; set; }
    }
}
