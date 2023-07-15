using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MyNetCore.Models;
using Roim.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyNetCore.Business
{
    public class CommonBusiness
    {

        private MySqlContext db { get; set; }
        public MySqlContext DB
        {
            get
            {
                if (db == null)
                {
                    db = new DataBaseContextConfig().CreateDbContext(null);
                    db.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
                }
                return db;
            }
        }

        //private TLBBContext dbTLBB { get; set; }

        //public TLBBContext DBTLBB
        //{
        //    get
        //    {
        //        if (dbTLBB == null)
        //        {
        //            dbTLBB = new DataTLBBContextConfig().CreateDbContext(null);
        //            dbTLBB.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        //        }
        //        return dbTLBB;
        //    }
        //}


        /// <summary>
        /// 是否为定时任务
        /// </summary>
        protected virtual bool IsTask { get; set; }

        public CommonBusiness()
        {

        }

        /// <summary>
        /// API用户有效时间（分钟）
        /// </summary>
        public int ApiUserEffectMinutes { get { return 30; } }

        /// <summary>
        /// 当前用户
        /// </summary>
        public Users GetCurrentUserInfo(string accessToken = null)
        {
            Users currentUser = AppContextMy.Current?.Session.GetObjectFromJson<Users>("CurrentUser") as Users;
            if (currentUser != null)
            {
                return currentUser;
            }
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var apiUserModel = DB.AccessToken.Include(m => m.CurrentUser).AsNoTracking().FirstOrDefault(m => m.Deleted == false && m.Name == accessToken);
                if (apiUserModel != null/* && apiUserModel.EffectDateTime >= DateTime.Now*/ && apiUserModel.CurrentUser != null)
                {
                    currentUser = apiUserModel.CurrentUser;
                    apiUserModel.EffectDateTime = DateTime.Now.AddHours(24); //DateTime.Now.AddMinutes(ApiUserEffectMinutes);
                    BusinessAccessToken businessAccessToken = new BusinessAccessToken();
                    businessAccessToken.Edit(apiUserModel, false);
                    //DB.SaveChanges();
                    AppContextMy.Current?.Session.SetObjectAsJson("CurrentUser", currentUser);
                    return currentUser;
                }
                return GetNoLoginUser();
            }

            string currentUserCode = AppContextMy.Current?.User.Identity.Name;

            if(IsTask && string.IsNullOrWhiteSpace(currentUserCode))
            {
                currentUserCode = "匿名用户";
            }

            if (string.IsNullOrWhiteSpace(currentUserCode))
            {
                return GetNoLoginUser();
            }

            currentUser = DB.Users.AsNoTracking().FirstOrDefault(m => m.Deleted == false && m.Code == currentUserCode);

            if (currentUser == null)
            {
                return GetNoLoginUser();
            }
            AppContextMy.Current?.Session.SetObjectAsJson("CurrentUser", currentUser);
            return currentUser;
        }

        #region 检查权限
        /// <summary>
        /// 检查是否有数据查询权限
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="modelUser"></param>
        /// <returns></returns>
        public bool CheckHasRecordRight<TModel>(TModel model, Users modelUser)
            where TModel : BaseModel
        {
            if (model == null || modelUser == null
                || !modelUser.TerritoryId.HasValue || !modelUser.TerritoryProfilesId.HasValue)
            {
                return false;
            }

            if (modelUser.IsAdmin)
            {
                return true;
            }

            IQueryable<Purview> listPurview = DB.Purview.AsNoTracking().Where
                (m => m.TerritoryProfilesId == modelUser.TerritoryProfilesId
                && m.FullName == typeof(TModel).FullName && m.CanSelect == true
                );

            if (listPurview == null || !listPurview.Any())
            {
                return false;
            }

            if (listPurview.Any(m => m.PurviewType == PurviewType.Owner) && model.CreatedById == modelUser.Id)
            {
                return true;
            }

            Territory modelTerritoryOfUser = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == modelUser.TerritoryId && m.Deleted == false);

            if (modelTerritoryOfUser == null)
            {
                return false;
            }

            if (listPurview.Any(m => m.PurviewType == PurviewType.MyTerritory &&
                model.TerritoryId >= modelTerritoryOfUser.TerritoryId && model.TerritoryId <= modelTerritoryOfUser.RangeEnd))
            {
                return true;
            }

            var listOtherTerritory = listPurview.Where(m => m.PurviewType == PurviewType.OtherTerritory);

            foreach (var itemOtherTerritory in listOtherTerritory)
            {
                Territory modelItemTerritory = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == itemOtherTerritory.OtherTerritoryId && m.Deleted == false);
                if (modelItemTerritory == null)
                {
                    continue;
                }
                if (model.TerritoryId >= modelItemTerritory.TerritoryId && model.TerritoryId <= modelItemTerritory.RangeEnd)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 检查是否有实体的查看权限
        /// </summary>
        /// <typeparam name="Tmodel"></typeparam>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool CheckHasSelectRight<Tmodel>(out string errorMsg)
            where Tmodel : BaseModel
        {
            errorMsg = string.Empty;
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                errorMsg = "用户身份验证失败，请重新登录";
                return false;
            }

            if (currentUser.IsAdmin)
            {
                return true;
            }

            if (currentUser.TerritoryId == null)
            {
                errorMsg = MessageText.ErrorInfo.您无此操作权限.ToString();
                return false;
            }

            Territory modelTerrForCurrentUser = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == currentUser.TerritoryId && m.Deleted == false);

            if (modelTerrForCurrentUser == null || !modelTerrForCurrentUser.TerritoryId.HasValue)
            {
                errorMsg = MessageText.ErrorInfo.您无此操作权限.ToString();
                return false;
            }

            if (currentUser.TerritoryProfilesId == null)
            {
                errorMsg = MessageText.ErrorInfo.您无此操作权限.ToString();
                return false;
            }

            IQueryable<Purview> listPurview = DB.Purview.Where
                            (m => m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                            && m.FullName == typeof(Tmodel).FullName && m.CanSelect == true
                            );

            if (listPurview == null || !listPurview.Any())
            {
                errorMsg = MessageText.ErrorInfo.您无此操作权限.ToString();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查是否有本实体的添加权限
        /// </summary>
        /// <typeparam name="Tmodel"></typeparam>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool CheckHasAddRight<Tmodel>(out string errorMsg)
            where Tmodel : BaseModel
        {
            errorMsg = string.Empty;
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                errorMsg = "用户身份验证失败，请重新登录";
                return false;
            }

            if (currentUser.IsAdmin)
            {
                return true;
            }

            if (currentUser.TerritoryId == null)
            {
                errorMsg = MessageText.ErrorInfo.您无此操作权限.ToString();
                return false;
            }

            Territory modelTerrForCurrentUser = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == currentUser.TerritoryId && m.Deleted == false);

            if (modelTerrForCurrentUser == null || !modelTerrForCurrentUser.TerritoryId.HasValue)
            {
                errorMsg = MessageText.ErrorInfo.您无此操作权限.ToString();
                return false;
            }

            if (currentUser.TerritoryProfilesId == null)
            {
                errorMsg = MessageText.ErrorInfo.您无此操作权限.ToString();
                return false;
            }

            IQueryable<Purview> listPurview = DB.Purview.Where
                            (m => m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                            && m.FullName == typeof(Tmodel).FullName && m.CanInsert == true
                            );

            if (listPurview == null || !listPurview.Any())
            {
                errorMsg = MessageText.ErrorInfo.您无此操作权限.ToString();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查是否有本条数据的添加权限
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="needCheckRight"></param>
        public void CheckAddRight<T>(T model, bool needCheckRight)
            where T : BaseModel
        {
            if (model == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.数据提交失败);
            }

            if (needCheckRight)
            {
                var currentUser = GetCurrentUserInfo();
                if (currentUser == null)
                {
                    ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                }

                if (!currentUser.IsAdmin)
                {
                    Territory modelTerrForCurrentUser = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == currentUser.TerritoryId && m.Deleted == false);

                    if (modelTerrForCurrentUser == null || !modelTerrForCurrentUser.TerritoryId.HasValue || !currentUser.TerritoryProfilesId.HasValue)
                    {
                        ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                    }

                    IQueryable<Purview> listPurview = DB.Purview.Where
                                    (m => m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                                    && m.FullName == typeof(T).FullName && m.CanInsert == true
                                    );

                    if (listPurview == null || !listPurview.Any())
                    {
                        ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                    }
                    else
                    {
                        if (model.TerritoryId != null)
                        {
                            bool hasRight = false;

                            if (listPurview.Any(m => m.PurviewType == PurviewType.Owner || m.PurviewType == PurviewType.MyTerritory))
                            {
                                if (modelTerrForCurrentUser.TerritoryId.Value <= model.TerritoryId.Value &&
                                modelTerrForCurrentUser.RangeEnd >= model.TerritoryId.Value)
                                {
                                    hasRight = true;
                                }
                            }

                            IEnumerable<Territory> listTerritoryAll = BusinessHelper.ListTerritory;

                            foreach (var itemOtherTerritory in listPurview.Where(m => m.PurviewType == PurviewType.OtherTerritory))
                            {

                                Territory modelTerritory = listTerritoryAll.FirstOrDefault(m => m.TerritoryId == itemOtherTerritory.OtherTerritoryId && m.Deleted == false);
                                if (modelTerritory == null)
                                {
                                    continue;
                                }

                                if (modelTerritory.TerritoryId.Value <= model.TerritoryId.Value &&
                                modelTerritory.RangeEnd >= model.TerritoryId.Value)
                                {
                                    hasRight = true;
                                    break;
                                }
                            }

                            if (!hasRight)
                            {
                                ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 检查是否有本条数据的修改权限
        /// </summary>
        /// <typeparam name="Tmodel"></typeparam>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool CheckHasEditRight<Tmodel>(Tmodel model, out string errorMsg)
            where Tmodel : BaseModel
        {
            errorMsg = string.Empty;
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                errorMsg = "用户身份验证失败，请重新登录";
                return false;
            }

            if (currentUser.IsAdmin)
            {
                return true;
            }

            if (currentUser.TerritoryId == null)
            {
                errorMsg = MessageText.ErrorInfo.您无此区域的修改权限.ToString();
                return false;
            }

            Territory modelTerrForCurrentUser = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == currentUser.TerritoryId && m.Deleted == false);

            if (modelTerrForCurrentUser == null || currentUser.TerritoryProfilesId == null)
            {
                errorMsg = MessageText.ErrorInfo.您无此区域的修改权限.ToString();
                return false;
            }

            IQueryable<Purview> listPurview = DB.Purview.Where
                            (m => m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                            && m.FullName == typeof(Tmodel).FullName && m.CanUpdate == true
                            );

            if (listPurview == null || !listPurview.Any())
            {
                errorMsg = MessageText.ErrorInfo.您无此区域的修改权限.ToString();
                return false;
            }

            var listPurviewMyTerritory = listPurview.Where(m => m.PurviewType == PurviewType.MyTerritory);
            var listPurviewOwener = listPurview.Where(m => m.PurviewType == PurviewType.Owner);
            var listPurviewOtherTerritory = listPurview.Where(m => m.PurviewType == PurviewType.OtherTerritory);

            bool hasRight = false;

            if (listPurviewOwener != null && listPurviewOwener.Any())
            {
                if (model.CreatedById == currentUser.Id)
                {
                    hasRight = true;
                }
            }

            if (listPurviewMyTerritory != null && listPurviewMyTerritory.Any())
            {
                if (model.TerritoryId.Value >= modelTerrForCurrentUser.TerritoryId.Value
                        && model.TerritoryId.Value <= modelTerrForCurrentUser.RangeEnd)
                {
                    hasRight = true;
                }
            }

            IEnumerable<Territory> listTerritoryAll = BusinessHelper.ListTerritory;

            foreach (var itemOtherTerritory in listPurviewOtherTerritory)
            {

                Territory modelTerritory = listTerritoryAll.FirstOrDefault(m => m.TerritoryId == itemOtherTerritory.OtherTerritoryId && m.Deleted == false);
                if (modelTerritory == null)
                {
                    continue;
                }

                if (modelTerritory.TerritoryId.Value <= model.TerritoryId.Value &&
                modelTerritory.RangeEnd >= model.TerritoryId.Value)
                {
                    hasRight = true;
                    break;
                }
            }

            if (!hasRight)
            {
                errorMsg = MessageText.ErrorInfo.您无此区域的修改权限.ToString();
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检查是否有本条数据的删除权限
        /// </summary>
        /// <typeparam name="Tmodel"></typeparam>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool CheckHasDeleteRight<Tmodel>(Tmodel model, out string errorMsg)
            where Tmodel : BaseModel
        {
            errorMsg = string.Empty;
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                errorMsg = "用户身份验证失败，请重新登录";
                return false;
            }

            if (currentUser.IsAdmin)
            {
                return true;
            }

            if (currentUser.TerritoryId == null)
            {
                errorMsg = MessageText.ErrorInfo.您无此区域的修改权限.ToString();
                return false;
            }

            Territory modelTerrForCurrentUser = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == currentUser.TerritoryId && m.Deleted == false);

            if (modelTerrForCurrentUser == null || currentUser.TerritoryProfilesId == null)
            {
                errorMsg = MessageText.ErrorInfo.您无此区域的修改权限.ToString();
                return false;
            }

            IQueryable<Purview> listPurview = DB.Purview.Where
                            (m => m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                            && m.FullName == typeof(Tmodel).FullName && m.CanDelete == true
                            );

            if (listPurview == null || !listPurview.Any())
            {
                errorMsg = MessageText.ErrorInfo.您无此区域的修改权限.ToString();
                return false;
            }

            var listPurviewMyTerritory = listPurview.Where(m => m.PurviewType == PurviewType.MyTerritory);
            var listPurviewOwener = listPurview.Where(m => m.PurviewType == PurviewType.Owner);
            var listPurviewOtherTerritory = listPurview.Where(m => m.PurviewType == PurviewType.OtherTerritory);

            bool hasRight = false;

            if (listPurviewOwener != null && listPurviewOwener.Any())
            {
                if (model.CreatedById == currentUser.Id)
                {
                    hasRight = true;
                }
            }

            if (listPurviewMyTerritory != null && listPurviewMyTerritory.Any())
            {
                if (model.TerritoryId.Value >= modelTerrForCurrentUser.TerritoryId.Value
                        && model.TerritoryId.Value <= modelTerrForCurrentUser.RangeEnd)
                {
                    hasRight = true;
                }
            }

            IEnumerable<Territory> listTerritoryAll = BusinessHelper.ListTerritory;

            foreach (var itemOtherTerritory in listPurviewOtherTerritory)
            {

                Territory modelTerritory = listTerritoryAll.FirstOrDefault(m => m.TerritoryId == itemOtherTerritory.OtherTerritoryId && m.Deleted == false);
                if (modelTerritory == null)
                {
                    continue;
                }

                if (modelTerritory.TerritoryId.Value <= model.TerritoryId.Value &&
                modelTerritory.RangeEnd >= model.TerritoryId.Value)
                {
                    hasRight = true;
                    break;
                }
            }

            if (!hasRight)
            {
                errorMsg = MessageText.ErrorInfo.您无此区域的修改权限.ToString();
                return false;
            }

            return true;
        }
        #endregion

        private Users GetNoLoginUser()
        {
            return null;
        }

        /// <summary>
        /// 检查用户是否登录
        /// </summary>
        /// <returns>true为登录,false为未登录</returns>
        public bool CheckUserIsLogin()
        {
            var currentUser = GetCurrentUserInfo();
            return currentUser != null && currentUser.Id > 0;
        }

        public void ThrowErrorInfo(MessageText.ErrorInfo errorInfo)
        {
            throw new LogicException(errorInfo.GetCustomDescription());
        }

        public void ThrowErrorInfo(string errorInfo)
        {
            throw new LogicException(errorInfo);
        }

        /// <summary>
        /// 根据保存的值获取CheckBox是否被选中
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool GetCheckBoxValue(string value)
        {
            return value == "on";
        }

        /// <summary>
        /// 获取当前网站域名url
        /// </summary>
        /// <returns></returns>
        public string GetDomainUrl()
        {
            string url = AppContextMy.Current.Request.GetDomainUri();
            return url;
        }

        /// <summary>
        /// 生成一个新的GUID
        /// </summary>
        /// <returns></returns>
        public string GetNewGuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// 获取DisplayName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetDisplayName<T>()
            where T : BaseModel
        {
            Type type = typeof(T);

            var objs = type.GetCustomAttributes(typeof(DisplayNameAttribute), true);
            if (objs != null && objs.Any())
            {
                var obj = objs[0];
                var displayNameAttr = (DisplayNameAttribute)obj;
                return displayNameAttr?.Name;
            }

            return null;
        }
    }

    public static class ButtonHelper
    {
        /// <summary>
        /// 生成添加页面的保存按钮
        /// </summary>
        /// <returns></returns>
        public static HtmlString GetSaveButtonForAdd<T>(string ajaxClass = "ajaxSave")
            where T : BaseModel
        {
            string actionName = typeof(T).Name;
            string addButtonStr = "<button class=\"btn btn-primary btn-loading needValid btn-add " + ajaxClass + "\" validFor=\"addForm\" type=\"button\" actionName=\"" + actionName + "\">保存</button>&nbsp;";

            if (!new CommonBusiness().CheckHasAddRight<T>(out string s))
            {
                addButtonStr = string.Empty;
            }

            return new HtmlString(addButtonStr);
        }

        /// <summary>
        /// 用于生成未继承BaseModel的添加页面的保存按钮
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ajaxClass"></param>
        /// <returns></returns>
        public static HtmlString GetSaveButtonForAddNoRight<T>(string ajaxClass = "ajaxSave")
        {
            string actionName = typeof(T).Name;
            string addButtonStr = "<button class=\"btn btn-primary btn-loading needValid btn-add " + ajaxClass + "\" validFor=\"addForm\" type=\"button\" actionName=\"" + actionName + "\">保存</button>&nbsp;";
            return new HtmlString(addButtonStr);
        }

        /// <summary>
        /// 生成修改页面的保存按钮
        /// </summary>
        /// <returns></returns>
        public static HtmlString GetSaveButtonForEdit<T>(T model, string ajaxClass = "ajaxSave", string validFor = "editForm")
            where T : BaseModel
        {
            string actionName = typeof(T).Name;
            string addButtonStr = "<button class=\"btn btn-primary btn-loading needValid btn-edit " + ajaxClass + "\" validFor=\"" + validFor + "\" type=\"button\" actionName=\"" + actionName + "\">保存</button>&nbsp;";

            if (!new CommonBusiness().CheckHasEditRight<T>(model, out string s))
            {
                addButtonStr = string.Empty;
            }

            return new HtmlString(addButtonStr);
        }

        /// <summary>
        /// 用于生成未继承BaseModel的修改页面的保存按钮
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <param name="ajaxClass"></param>
        /// <param name="validFor"></param>
        /// <returns></returns>
        public static HtmlString GetSaveButtonForEditNoRight<T>(T model, string ajaxClass = "ajaxSave", string validFor = "editForm")
        {
            string actionName = typeof(T).Name;
            string addButtonStr = "<button class=\"btn btn-primary btn-loading needValid btn-edit " + ajaxClass + "\" validFor=\"" + validFor + "\" type=\"button\" actionName=\"" + actionName + "\">保存</button>&nbsp;";
            return new HtmlString(addButtonStr);
        }

        /// <summary>
        /// 生成返回按钮
        /// </summary>
        /// <returns></returns>
        public static HtmlString GetBackButton(string returnUrl = null)
        {
            string addButtonStr = "<button class=\"btn btn-default btn-loading btn-back\" type=\"button\">返回</button>&nbsp;";
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                addButtonStr = "<a class=\"btn btn-default btn-loading\" href=\"" + returnUrl + "\" type=\"button\">返回</a>&nbsp;";
            }
            return new HtmlString(addButtonStr);
        }

        /// <summary>
        /// 生成删除按钮
        /// </summary>
        /// <returns></returns>
        public static HtmlString GetDeleteButton<T>(T model, string ajaxClass = "ajaxSave")
            where T : BaseModel
        {
            string actionName = typeof(T).Name;
            string addButtonStr = "<button class=\"btn btn-danger btn-loading btn-delete " + ajaxClass + "\" type=\"button\" validFor=\"deleteForm\" actionName=\"" + actionName + "\">删除</button>&nbsp;";

            if (!new CommonBusiness().CheckHasDeleteRight<T>(model, out string s))
            {
                addButtonStr = string.Empty;
            }

            return new HtmlString(addButtonStr);
        }

        /// <summary>
        /// 用于生成未继承BaseModel的修改页面的删除按钮
        /// </summary>
        /// <returns></returns>
        public static HtmlString GetDeleteButtonNoRight<T>(T model, string ajaxClass = "ajaxSave")
        {
            string actionName = typeof(T).Name;
            string addButtonStr = "<button class=\"btn btn-danger btn-loading btn-delete " + ajaxClass + "\" type=\"button\" validFor=\"deleteForm\" actionName=\"" + actionName + "\">删除</button>&nbsp;";
            return new HtmlString(addButtonStr);
        }

        /// <summary>
        /// 生成列表上的添加按钮
        /// </summary>
        /// <returns></returns>
        public static HtmlString GetAddButton<T>(string ajaxClass = "ajaxSave")
            where T : BaseModel
        {
            string actionName = typeof(T).Name;
            string addButtonStr = "<a class=\"btn btn-primary btn-flat\" href=\"Add\"><i class=\"fa fa-lg fa-plus\"></i></a>";

            if (!new CommonBusiness().CheckHasAddRight<T>(out string s))
            {
                addButtonStr = string.Empty;
            }

            return new HtmlString(addButtonStr);
        }
    }


    public static class KuoZhanFangFa
    {
        public static bool IsAjax(this HttpRequest req)
        {
            bool result = false;

            var xreq = req.Headers.ContainsKey("x-requested-with");
            if (xreq)
            {
                result = req.Headers["x-requested-with"] == "XMLHttpRequest";
            }

            return result;
        }
    }

}