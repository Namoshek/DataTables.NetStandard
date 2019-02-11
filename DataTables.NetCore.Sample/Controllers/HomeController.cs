﻿using System.Diagnostics;
using DataTables.NetCore.Sample.DataTables;
using DataTables.NetCore.Sample.Models;
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
            ViewBag.DataTableScript = _userDataTable.RenderScript(Url.Action("IndexData", "Home"));
            ViewBag.DataTableHtml = _userDataTable.RenderHtml();

            return View();
        }

        public IActionResult IndexData()
        {
            return Ok(_userDataTable.RenderResponse(Request.QueryString.ToUriComponent()).AsJsonString());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
