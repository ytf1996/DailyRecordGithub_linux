using MyNetCore.Models;

namespace MyNetCore.Business
{
    public class BusinessTerritoryProfiles : BaseBusiness<TerritoryProfiles>
    {
        public override void Add(TerritoryProfiles model, bool needCheckRight = true, bool saveToDBNow = true, MySqlContext db = null)
        {
            if (db == null)
            {
                db = DB;
            }
            base.Add(model, needCheckRight, saveToDBNow, db);
            BusinessHelper.ListTerritoryProfiles = null;
        }

        public override void Edit(TerritoryProfiles model, bool needCheckRight = true, MySqlContext db = null)
        {
            if(db == null)
            {
                db = DB;
            }
            base.Edit(model, needCheckRight, db);
            BusinessHelper.ListTerritoryProfiles = null;
        }

        public override void Delete(TerritoryProfiles model, bool needCheckRight = true)
        {
            base.Delete(model, needCheckRight);
            BusinessHelper.ListTerritoryProfiles = null;
        }

        public override void Delete(string ids, bool needCheckRight = true)
        {
            base.Delete(ids, needCheckRight);
            BusinessHelper.ListTerritoryProfiles = null;
        }

        public override bool CustomValidForSave(TerritoryProfiles model, out string errorMsg)
        {
            errorMsg = string.Empty;
            if (model.Id == 1)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.系统保留数据不能修改);
            }
            return true;
        }

        public override bool CustomValidForDelete(TerritoryProfiles model, out string errorMsg)
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