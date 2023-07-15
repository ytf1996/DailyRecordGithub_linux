using MyNetCore.Controllers;
using MyNetCore.Models;
using Roim.Common;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using System.Collections.Generic;
using System;
using System.IO;

namespace MyNetCore.Areas.Management.Controllers
{
    public class AttachmentController : BaseManagementController<Attachment, BusinessAttachment>
    {
        public override IActionResult GetListData(DataTableParameters param)
        {
            int total = 0;

            IQueryable<Attachment> list = business.GetList(param, out total);

            object result = null;
            List<Attachment> finalList = null;


            if (list == null)
            {
                finalList = new List<Attachment>();
            }
            else
            {
                finalList = list.ToList();
            }


            result = from m in finalList
                     select new
                     {
                         m.Id,
                         m.Name,
                         m.SuffixName,
                         m.Size,
                         m.Path,
                         m.ContentType,
                         CreatedDate = m.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                     };

            return JsonListResult(param, total, result);

        }

        [HttpPost]
        public IActionResult SaveImg()
        {
            string attrId = Request.Form["attrId"];
            string base64Url = Request.Form["base64Url"];
            string filename = Request.Form["filename"];
            string filetype = Request.Form["filetype"];

            int attrIdInt = 0;

            Attachment modelAttachment = null;

            if (!string.IsNullOrWhiteSpace(attrId) && int.TryParse(attrId, out attrIdInt))
            {
                modelAttachment = business.GetById(attrIdInt, false);
            }

            //附件已存在,做修改操作
            if (modelAttachment == null)
            {
                modelAttachment = new Attachment();
            }
            modelAttachment.IsPicture = true;
            modelAttachment.Name = filename;
            modelAttachment.ContentType = filetype;
            modelAttachment.SuffixName = filename.Split('.').Length > 1 ? filename.Split('.')[filename.Split('.').Length - 1] : "";

            string dir = string.Format("/upload/img/{0}/{1}", DateTime.Now.Year, DateTime.Now.Month.ToString().PadLeft(2, '0'));

            string pathRoot = Directory.GetCurrentDirectory();
            //站点文件目录
            string fileDir = Path.Combine(pathRoot, "wwwroot" + dir);
            if (!System.IO.Directory.Exists(fileDir))
                System.IO.Directory.CreateDirectory(fileDir);

            if (string.IsNullOrWhiteSpace(modelAttachment.Path))
            {
                modelAttachment.Path = string.Format("{0}/{1}.{2}", dir, Guid.NewGuid(), modelAttachment.SuffixName);
            }
            string[] img_array = base64Url.Split(',');
            byte[] arr2 = Convert.FromBase64String(img_array[1]);
            
            using (MemoryStream ms2 = new MemoryStream(arr2))
            {
                System.Drawing.Bitmap bmp2 = new System.Drawing.Bitmap(ms2);
                bmp2.Save(Path.Combine(pathRoot, "wwwroot" + modelAttachment.Path),
                    modelAttachment.SuffixName.ToLower() == "png" ? System.Drawing.Imaging.ImageFormat.Png : System.Drawing.Imaging.ImageFormat.Jpeg);
                modelAttachment.Size = ms2.Length;
                bmp2.Dispose();
            }

            if (modelAttachment.Id == 0)
            {
                business.Add(modelAttachment);
            }
            else
            {
                business.Edit(modelAttachment);
            }

            attrId = modelAttachment.Id.ToString();
            return Success("图片保存成功", new { fileId = attrId });

        }
    }
}