using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business.Jobs;
using MyNetCore.Business.Tlbb;
using MyNetCore.Tools;
using Quartz;
using Roim.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyNetCore.Controllers
{
    public class TLBBController : BaseController
    {

        public TLBBController()
        {

        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddPoint(string account)
        {
            try
            {
                new AccountService().AddPoint(account);
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }

            return Success();
        }
    }
}