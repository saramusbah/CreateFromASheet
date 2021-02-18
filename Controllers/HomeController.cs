namespace CreateFromASheet.Controllers
{
    using CreateFromASheet.Models;
    using CreateFromASheet.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

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
                var model = new List<SheetModel>();
                if (file != null)
                {
                    model = await _sheetService.CreateTableFromCsvAsync(file);
                }

                ViewBag.Message = "File Uploaded Successfully!!";

                return View(model);
            }
            catch (Exception e)
            {
                ViewBag.Message = "File upload failed!!";
                return View();
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string columnId, string tableName)
        {
            var model = await _sheetService.DeleteColumnAsync(columnId, tableName);
            return View(model);
        }

        [HttpPut]
        public async Task<IActionResult> Edit(string columnId, string tableName)
        {
            var model = await _sheetService.EditColumnAsync(columnId, tableName);
            return View(model);
        }
    }
}
