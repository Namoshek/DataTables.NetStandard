﻿using System.Diagnostics;
using DataTables.NetStandard.Sample.DataTables;
using DataTables.NetStandard.Sample.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataTables.NetStandard.Sample.Controllers
{
    public class PersonsController : Controller
    {
        protected PersonDataTable _personDataTable;

        public PersonsController(PersonDataTable personDataTable)
        {
            _personDataTable = personDataTable;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TableData()
        {
            return Ok(_personDataTable.RenderResponse(Request.QueryString.ToUriComponent()).AsJsonString());
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
