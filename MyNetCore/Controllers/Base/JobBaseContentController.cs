using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyNetCore.Business.Jobs;
using Quartz;
using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyNetCore.Controllers
{
    public class JobBaseContentController : BaseController
    {
        public JobBaseContentController()
        {

        }
    }
}