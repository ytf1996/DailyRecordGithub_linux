using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyNetCore.Models;

namespace MyNetCore.Business
{
    public class BusinessAttachment : BaseBusiness<Attachment>
    {
        public override bool NeedCheckNameRepeat => false;

        public override bool CustomValidForSave(Attachment model, out string errorMsg)
        {
            errorMsg = "该路径图片已存在";
            Attachment modelDB = GetByCondition(m => m.Path == model.Path && m.Id != model.Id, false);
            return modelDB == null;
        }

    }
}