using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyNetCore.Areas.Management.Controllers
{
    [Area("Management")]
    public class ErrorPageController : Controller
    {
        public IActionResult Show()
        {
            return View();
        }
    }
}