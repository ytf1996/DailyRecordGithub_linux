using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business.Jobs;
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
    public class HomeController : BaseController
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public HomeController(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public IActionResult Index()
        {
            return Redirect("/Management/Home/Index");
        }

        public IActionResult PDFHelper()
        {
            return View();
        }

        [HttpPost]
        public IActionResult PDFMerge()
        {
            //var fileName = file.FileName;
            //var filePath = Server.MapPath(string.Format("~/{0}", "File"));
            //file.SaveAs(Path.Combine(filePath, fileName));
            string dir = "/upload/pdftemp";
            var path = Directory.GetCurrentDirectory();
            //站点文件目录
            string fileDir = string.Format("{0}{1}", path, dir);

            if (!System.IO.Directory.Exists(fileDir))
                System.IO.Directory.CreateDirectory(fileDir);

            #region 若生成过PDF，则先删除上次生成的文件
            string lastFilePath = Request.Query["mergeFilePath"];

            if (!string.IsNullOrWhiteSpace(lastFilePath))
            {
                try
                {
                    if (System.IO.File.Exists(string.Format("{0}{1}", path, lastFilePath)))
                    {
                        System.IO.File.Delete(string.Format("{0}{1}", path, lastFilePath));
                    }
                }
                catch (Exception ex)
                {
                    return Failure(ex.Message);
                }
            }
            #endregion


            foreach (var item in System.IO.Directory.GetFiles(fileDir))
            {
                if (System.IO.File.GetCreationTime(item) < DateTime.Now.AddMinutes(-10))
                {
                    System.IO.File.Delete(item);
                }
            }


            if (Request.Form == null && Request.Form.Files == null || Request.Form.Files.Count == 0)
            {
                return Failure("请选择需要合并的文件");
            }

            List<string> fileList = new List<string>();
            string outMergeFile = string.Empty;
            string zipFilePath = string.Empty;

            try
            {
                for (int i = 0; i < Request.Form.Files.Count; i++)
                {
                    var file = Request.Form.Files[i];
                    string filePath = string.Format("{0}/{1}.pdf", dir, Guid.NewGuid());
                    using (FileStream fs = System.IO.File.Create(filePath))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                        fileList.Add(filePath);
                    }
                }

                if (fileList.Count == 0)
                {
                    return Failure("文件上传失败");
                }
                outMergeFile = string.Format("{0}/{1}.pdf", dir, Guid.NewGuid());

                Roim.Common.PDFHelper.MergePdfFiles(fileList.ToArray(), string.Format("{0}{1}", path, outMergeFile));
                foreach (var item in fileList)
                {
                    if (System.IO.File.Exists(item))
                    {
                        System.IO.File.Delete(item);
                    }
                }

                zipFilePath = Roim.Common.CompressHelper.ZipFile(outMergeFile);

                if (System.IO.File.Exists(string.Format("{0}{1}", path, outMergeFile)))
                {
                    System.IO.File.Delete(string.Format("{0}{1}", path, outMergeFile));
                }
            }
            catch (Exception ex)
            {
                return Failure(ex.Message);
            }
            return Success("文件仅保留10分钟，请抓紧时间下载", data: new { outMergeFile = zipFilePath });
        }

        public IActionResult EmailHelper()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EmailHelperAction(string EmailHost, string EmailPort, string EmailSenderAddress,
            string EmailPassword, string MessageTo, string MessageSubject, string MessageBody)
        {
            EmailHelper emailHelper = new EmailHelper();
            string errorMsg = emailHelper.SendEmail(MessageTo, MessageSubject, MessageBody);
            if (!string.IsNullOrWhiteSpace(errorMsg))
            {
                return Failure(errorMsg);
            }

            return Success();
        }


        /// <summary>
        /// 天气查询api
        /// </summary>
        /// <returns></returns>
        public IActionResult Weather(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                city = "上海";
            }
            string url = string.Format("https://v0.yiketianqi.com/api?version=v61&appid=63399175&appsecret=tDen62PL&city={0}", city);
            string result = new WebClientCustom().CreateHttpResponseForGzip(url);
            result = UnicodeDencode(result);
            return Json(result);
        }

        /// <summary>  
        /// 转换输入字符串中的任何转义字符。如：Unicode 的中文 \u8be5  
        /// </summary>  
        /// <param name="str"></param>  
        /// <returns></returns>  
        public string UnicodeDencode(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            return Regex.Unescape(str);
        }

        /// <summary>
        /// 根据经纬度获得当前地址信息
        /// </summary>
        /// <returns></returns>
        public IActionResult GetInfoByLatAndLng(string lat,string lng)
        {
            if (string.IsNullOrWhiteSpace(lat) || string.IsNullOrWhiteSpace(lng))
            {
                return Json("");
            }
            string url = string.Format("http://api.map.baidu.com/geocoder/v2/?location={0},{1}&output=json&pois=1&ak=tDm05PF7lev0B34wezbfG2GU",
                lat, lng);
            string result = new WebClientCustom().CreateHttpResponse(url);

            return Json(result);
        }
    }
}