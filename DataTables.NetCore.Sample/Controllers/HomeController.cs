using System.Diagnostics;
using DataTables.NetCore.Sample.DataTables;
using DataTables.NetCore.Sample.Models;
using DataTables.Queryable;
using Microsoft.AspNetCore.Mvc;

namespace DataTables.NetCore.Sample.Controllers
{
    public class HomeController : Controller
    {
        protected UserDataTable _userDataTable;

        public HomeController(UserDataTable userDataTable)
        {
            _userDataTable = userDataTable;
        }

        public IActionResult Index()
        {
            // This is done only for testing because we are not actually using a DataTable yet and this means
            // we need to add proper query string parameters manually.
            if (!Request.QueryString.HasValue)
            {
                Request.QueryString = Request.QueryString.Add(new Microsoft.AspNetCore.Http.QueryString("?start=0&length=10"));
            }

            var request = new DataTablesRequest<User>(Request.QueryString.ToUriComponent());

            return Ok(_userDataTable.RenderResponse(request).AsJsonString());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
