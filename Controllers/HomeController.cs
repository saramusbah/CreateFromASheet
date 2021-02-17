using CreateFromASheet.Models;
using CreateFromASheet.Services;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CreateFromASheet.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISheetService _sheetService;

        public HomeController(ILogger<HomeController> logger, ISheetService sheetService)
        {
            _logger = logger;
            _sheetService = sheetService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ImportData(IFormFile file)
        {
            try
            {
                var model = new SheetModel();
                if (file.Length > 0)
                {
                    string _FileName = Path.GetFileName(file.FileName);
                    model = await _sheetService.CreateTableFromCsvAsync(file);
                }

                ViewBag.Message = "File Uploaded Successfully!!";

                return View(model);
            }
            catch(Exception e)
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
        }
    }
}
