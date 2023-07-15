using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyNetCore.Models;

namespace MyNetCore.Business
{
    public class BusinessChannel : BaseBusiness<Channel>
    {
        public override void Add(Channel model, bool needCheckRight = true, bool saveToDBNow = true, MySqlContext db = null)
        {
            if (db == null)
            {
                db = DB;
            }
            base.Add(model, needCheckRight, saveToDBNow, db);
            BusinessHelper.ListChannel = null;
        }

        public override void Edit(Channel model, bool needCheckRight = true, MySqlContext db = null)
        {
            if (db == null)
            {
                db = DB;
            }
            base.Edit(model, needCheckRight, db);
            BusinessHelper.ListChannel = null;
        }

        public override void Delete(Channel model, bool needCheckRight = true)
        {
            base.Delete(model, needCheckRight);
            BusinessHelper.ListChannel = null;
        }

        public override void Delete(string ids, bool needCheckRight = true)
        {
            base.Delete(ids, needCheckRight);
            BusinessHelper.ListChannel = null;
        }

        public override bool CustomValidForSave(Channel model, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (model.Id == 1)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.系统保留数据不能修改);
            }
            return true;
        }

        public override bool CustomValidForDelete(Channel model, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (model.Id == 1)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.系统保留数据不能删除);
            }
            return true;
        }
    }
}