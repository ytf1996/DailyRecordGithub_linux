using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyNetCore.Models;
using Roim.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace MyNetCore.Business
{
    /// <summary>
    /// 外键类，用来设置下拉框的值用
    /// </summary>
    public class SelectModel
    {
        public string SelectEntityName { get; set; }

        public string SelectText { get; set; }

        public int SelectValue { get; set; }

        public string SelectStringValue { get; set; }
    }

    /// <summary>
    /// 用来把系统用户绑定到微信(公众号)
    /// </summary>
    public class CodeForWechatMPBindUser
    {
        /// <summary>
        /// 微信用户id
        /// </summary>
        public string WechatMPUserId { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpirationDate { get; set; }
    }

    public class BusinessHelper : CommonBusiness
    {
        public static void InitData()
        {
            var db = new CommonBusiness().DB;
            if (db.Users.Any())
            {
                return;
            }
            InitData(db);
        }

        /// <summary>
        /// 初始化数据库数据
        /// </summary>
        /// <param name="context"></param>
        private static void InitData(MySqlContext context)
        {
            var userAdmin = context.Users.Add(
                new Users
                {
                    Name = "系统管理员",
                    Code = "admin",
                    Deleted = false,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    UserType = UserType.Admin,
                    PassWord = Roim.Common.DEncrypt.DEncrypt.DES("admin")
                }
                );

            context.SaveChanges();

            userAdmin.Entity.CreatedById = 1;
            userAdmin.Entity.UpdatedById = 1;

            context.SaveChanges();

            var territoryModel = context.Territory.Add(
                new Territory
                {
                    Name = "共享中心",
                    Depth = 1,
                    TheOrder = 1,
                    TerritoryId = -2147483640,
                    NextRangeStart = -805306364,
                    RangeEnd = 2147483647,
                    RangeIncrement = 268435454,
                    Deleted = false,
                    CreatedById = 1,
                    CreatedDate = DateTime.Now,
                    UpdatedById = 1,
                    UpdatedDate = DateTime.Now
                }
                );

            context.SaveChanges();

            var channelModel = context.Channel.Add(
                new Channel()
                {
                    Name = "默认",
                    CreatedById = 1,
                    CreatedDate = DateTime.Now,
                    Deleted = false,
                    OrderNum = 1,
                    UpdatedById = 1,
                    UpdatedDate = DateTime.Now
                }
                );

            context.SaveChanges();

            var territoryProfilesModel = context.TerritoryProfiles.Add(
                new TerritoryProfiles()
                {
                    Name = "默认",
                    CreatedById = 1,
                    CreatedDate = DateTime.Now,
                    Deleted = false,
                    UpdatedById = 1,
                    UpdatedDate = DateTime.Now
                }
                );

            context.SaveChanges();

            userAdmin.Entity.TerritoryId = territoryModel.Entity.TerritoryId;
            userAdmin.Entity.ChannelIds = channelModel.Entity.Id.ToString();
            userAdmin.Entity.TerritoryProfilesId = territoryProfilesModel.Entity.Id;

            context.SaveChanges();
        }

        public static string DBConfig { get; set; }

        public static string TlbbDBConfig { get; set; }

        private static List<Channel> _ListChannel { get; set; }
        /// <summary>
        /// 小组集合
        /// </summary>
        public static List<Channel> ListChannel
        {
            get
            {
                if (_ListChannel == null)
                {
                    _ListChannel = new BusinessChannel().DB.Channel.Where(m => m.Deleted == false).ToList();
                }
                return _ListChannel;
            }
            set
            {
                _ListChannel = value;
            }
        }

        private static List<Territory> _ListTerritory { get; set; }
        /// <summary>
        /// 区域集合
        /// </summary>
        public static List<Territory> ListTerritory
        {
            get
            {
                if (_ListTerritory == null)
                {
                    _ListTerritory = new BusinessTerritory().DB.Territory.Where(m => m.Deleted == false).ToList();
                }
                return _ListTerritory;
            }
            set
            {
                _ListTerritory = value;
            }
        }

        /// <summary>
        /// 区域列表
        /// </summary>
        public static IEnumerable<Territory> ListTerritoryForSelect
        {
            get
            {
                var business = new CommonBusiness();

                Territory currentTerr = null;
                List<Territory> dblist = null;
                List<Territory> finalList = null;
                int defaultDepth = 1;
                var currentUser = business.GetCurrentUserInfo();
                if (currentUser == null)
                {
                    return finalList;
                }
                if (currentUser != null && currentUser.IsAdmin)
                {
                    dblist = business.DB.Territory.AsNoTracking().Where(m => m.Deleted == false).OrderBy(m => m.Depth).ToList();
                }
                else if (currentUser != null && currentUser.TerritoryId.HasValue)
                {
                    currentTerr = business.DB.Territory.AsNoTracking().FirstOrDefault(m => m.TerritoryId == currentUser.TerritoryId);
                    if (currentTerr != null)
                    {
                        defaultDepth = currentTerr.Depth;
                        dblist = business.DB.Territory.AsNoTracking().Where(m => m.Deleted == false && currentTerr.TerritoryId <= m.TerritoryId &&
                        currentTerr.RangeEnd >= m.TerritoryId).OrderBy(m => m.Depth).ToList();
                    }
                }

                GetTerritorys(dblist, currentTerr, ref finalList, defaultDepth);

                return finalList;
            }
        }

        private static List<TerritoryProfiles> _ListTerritoryProfiles { get; set; }
        /// <summary>
        /// 权限配置文件集合
        /// </summary>
        public static List<TerritoryProfiles> ListTerritoryProfiles
        {
            get
            {
                if (_ListTerritoryProfiles == null)
                {
                    _ListTerritoryProfiles = new BusinessTerritoryProfiles().GetListByCondition(m => m.Deleted == false, false).ToList();
                }
                return _ListTerritoryProfiles;
            }
            set
            {
                _ListTerritoryProfiles = value;
            }
        }


        private static List<EntityModel> _listEntityModels;

        /// <summary>
        /// 实体列表
        /// </summary>
        public static List<EntityModel> ListEntityModels
        {
            get
            {
                if(_listEntityModels?.Any() == true)
                {
                    return _listEntityModels;
                }

                Type[] types = GetChildTypes(typeof(BaseModel));

                Type[] typesParam = GetChildTypes(typeof(BaseParam));

                Type[] typesWorkflow = GetChildTypes(typeof(IWorkflowModel));

                //因为Users,Log,参数类没有继承基类，这里单独添加一下
                if (types != null)
                {
                    types = types.Union(new Type[] { typeof(Users) }).ToArray();
                    types = types.Union(new Type[] { typeof(ErrorLog) }).ToArray();
                    if (typesParam != null)
                    {
                        types = types.Union(typesParam).ToArray();
                    }
                    if (typesWorkflow != null)
                    {
                        types = types.Union(typesWorkflow).ToArray();
                    }
                }

                List<EntityModel> list = new List<EntityModel>();

                EntityModel model = null;

                object[] objs = null;
                object obj = null;
                DisplayNameAttribute displayNameAttr = null;

                CommonBusiness cb = new CommonBusiness();
                Users currentUser = cb.GetCurrentUserInfo();

                if (currentUser == null)
                {
                    return new List<EntityModel>();
                }

                BusinessPurview businessPurview = new BusinessPurview();

                List<Purview> listPurview = businessPurview.
                    GetList(m => currentUser.TerritoryProfilesId != null && m.TerritoryProfilesId == currentUser.TerritoryProfilesId).ToList();

                foreach (var item in types)
                {
                    if (!currentUser.IsAdmin && !listPurview.Any(m => m.FullName == item.FullName && (m.CanSelect == true || m.CanInsert == true)))
                    {
                        continue;
                    }

                    model = new EntityModel();
                    model.FullName = item.FullName;
                    model.Name = item.Name;
                    objs = item.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                    if (objs != null && objs.Any())
                    {
                        obj = objs[0];
                        displayNameAttr = (DisplayNameAttribute)obj;
                        model.DisplayName = displayNameAttr.Name;
                        model.ParentMenuName = displayNameAttr.ParentMenuName;
                        model.Icons = displayNameAttr.Icons;
                        model.OrderNum = displayNameAttr.OrderNum;
                        model.OnlyForAdmin = displayNameAttr.OnlyForAdmin;
                        model.Url = displayNameAttr.Url;
                        model.IsMenu = displayNameAttr.IsMenu;
                        model.NeedAddButton = displayNameAttr.NeedAddButton;
                    }
                    list.Add(model);
                }

                _listEntityModels = list;

                return _listEntityModels;
            }
        }

        private static List<EntityModel> _listWorkflowEntityModels;

        /// <summary>
        /// 带工作流的实体列表
        /// </summary>
        public static List<EntityModel> ListWorkflowEntityModels
        {
            get
            {
                if (_listWorkflowEntityModels?.Any() == true)
                {
                    return _listWorkflowEntityModels;
                }

                Type[] types = GetChildTypes(typeof(IWorkflowModel));

                List<EntityModel> list = new List<EntityModel>();

                EntityModel model = null;

                object[] objs = null;
                object obj = null;
                DisplayNameAttribute displayNameAttr = null;

                foreach (var item in types)
                {
                    model = new EntityModel();
                    model.FullName = item.FullName;
                    model.Name = item.Name;
                    objs = item.GetCustomAttributes(typeof(DisplayNameAttribute), true);
                    if (objs != null && objs.Any())
                    {
                        obj = objs[0];
                        displayNameAttr = (DisplayNameAttribute)obj;
                        model.DisplayName = displayNameAttr.Name;
                        model.ParentMenuName = displayNameAttr.ParentMenuName;
                        model.Icons = displayNameAttr.Icons;
                        model.OrderNum = displayNameAttr.OrderNum;
                        model.OnlyForAdmin = displayNameAttr.OnlyForAdmin;
                        model.Url = displayNameAttr.Url;
                        model.IsMenu = displayNameAttr.IsMenu;
                        model.NeedAddButton = displayNameAttr.NeedAddButton;
                    }
                    list.Add(model);
                }

                _listWorkflowEntityModels = list;

                return _listWorkflowEntityModels;
            }
        }

        /// <summary>
        /// 微信申请绑定系统用户列表
        /// </summary>
        public static List<CodeForWechatMPBindUser> ListWechatMPUserForBindCode { get; set; }

        public static List<CodeForWechatMPBindUser> ListWechatMPUserForLogin { get; set; }

        /// <summary>
        /// 获取父类下所有子类
        /// </summary>
        /// <param name="parentType"></param>
        /// <returns></returns>
        private static Type[] GetChildTypes(Type parentType)
        {

            List<Type> lstType = new List<Type>();
            Assembly assem = Assembly.GetAssembly(parentType);
            foreach (Type tChild in assem.GetTypes())
            {
                if (tChild.BaseType == parentType)
                {
                    lstType.Add(tChild);
                }
            }
            return lstType.ToArray();
        }

        /// <summary>
        /// 根据完全类名获得类的DisplayName
        /// </summary>
        /// <param name="classFullName"></param>
        /// <returns></returns>
        public static string GetDisplayNameByClassFullName(string classFullName)
        {
            if (string.IsNullOrWhiteSpace(classFullName))
            {
                return string.Empty;
            }
            Type type = Type.GetType(classFullName);
            if (type == null)
            {
                return string.Empty;
            }
            var entityDisplayNameAttribute = type.GetCustomAttribute<DisplayNameAttribute>();
            string entityDisplayName = entityDisplayNameAttribute == null ? string.Empty : entityDisplayNameAttribute.Name;
            return entityDisplayName;
        }

        /// <summary>
        /// 根据完全类名获取数据库字段集合
        /// </summary>
        /// <param name="classFullName"></param>
        /// <returns></returns>
        public static List<SelectModel> GetDBColumnsByClassFullName(string classFullName)
        {
            List<SelectModel> list = new List<SelectModel>();

            Type type = Type.GetType(classFullName);

            PropertyInfo[] propertys = type.GetProperties();

            bool isVirtual = false;
            string displayName = string.Empty;

            SelectModel model = null;

            foreach (var itemProperty in propertys)
            {
                if (itemProperty.Name == "Id")
                {
                    continue;
                }
                isVirtual = false;
                foreach (var item in itemProperty.GetAccessors())
                {
                    if (item.IsVirtual)
                    {
                        isVirtual = true;
                    }
                }
                //虚属性找到相对应的属性并放到外键类集合里
                if (isVirtual)
                {
                    continue;
                }
                var displayAttribute = itemProperty.GetCustomAttribute<DisplayAttribute>();

                model = new SelectModel();

                model.SelectStringValue = itemProperty.Name;
                model.SelectText = displayAttribute == null ? itemProperty.Name : displayAttribute.Name;
                list.Add(model);
            }

            return list;
        }

        /// <summary>
        /// 设置区域格式
        /// </summary>
        /// <param name="dblist"></param>
        /// <param name="currentTerr"></param>
        /// <param name="finalList"></param>
        private static void GetTerritorys(List<Territory> dblist, Territory currentTerr, ref List<Territory> finalList, int defaultDepth = 1)
        {
            if (finalList == null)
            {
                finalList = new List<Territory>();
            }

            if (dblist == null)
            {
                return;
            }

            if (currentTerr == null)
            {
                currentTerr = dblist.OrderBy(m => m.Depth).FirstOrDefault();
            }

            //string spaceStr = "|";

            //for (int i = 0; i < currentTerr.Depth - defaultDepth; i++)
            //{
            //    spaceStr += "....";
            //}

            //if (spaceStr == "|")
            //{
            //    spaceStr = "";
            //}

            currentTerr.NameForSelect = GetAllParentTerritorysName(dblist, currentTerr) + currentTerr.Name;

            if (!finalList.Any(m => m.Id == currentTerr.Id))
            {
                finalList.Add(currentTerr);
            }

            List<Territory> children = dblist.Where(m => m.ParentTerrId == currentTerr.TerritoryId).ToList();

            if (children == null || !children.Any())
            {
                return;
            }

            foreach (var item in children)
            {
                GetTerritorys(dblist, item, ref finalList, defaultDepth);
            }

        }

        /// <summary>
        /// 获取所有父区域的名称
        /// </summary>
        /// <param name="dblist"></param>
        /// <param name="currentTerr"></param>
        /// <returns></returns>
        private static string GetAllParentTerritorysName(List<Territory> dblist, Territory currentTerr)
        {
            string parentName = string.Empty;
            List<string> parentNames = new List<string>();

            if (dblist == null || !dblist.Any() || currentTerr == null)
            {
                return parentName;
            }

            int? parentTerr = currentTerr.ParentTerrId;
            Territory territoryParent = null;

            while (dblist.Any(m => m.TerritoryId == parentTerr && m.ParentTerrId != parentTerr))
            {
                territoryParent = dblist.FirstOrDefault(m => m.TerritoryId == parentTerr);
                parentNames.Add(territoryParent.Name);
                parentTerr = territoryParent.ParentTerrId;
            }

            for (int i = parentNames.Count - 1; i >= 0; i--)
            {
                parentName += string.Format("{0}-", parentNames[i]);
            }

            return parentName;
        }

        #region 生成基类为BaseModel类的添加/修改页面
        /// <summary>
        /// 生成基类为BaseModel类的添加/修改页面
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="model">记录</param>
        /// <param name="columnCount">列数</param>
        /// <param name="hideTerritoryId">是否隐藏区域字段</param>
        /// <param name="hideName">是否隐藏Name字段</param>
        /// <param name="backBtnUrl">返回按钮路径(不指定返回上一个页面)</param>
        /// <param name="buttonClass">保存按钮的Class(一般为ajaxSave,明细类为ajaxSaveForDetail)</param>
        /// <returns></returns>
        public static HtmlString BuildCreateOrEditPage<T>(T model, string mdNum = "6", bool hideTerritoryId = false, bool hideName = false,
            string backBtnUrl = null, string buttonClass = "ajaxSave")
            where T : BaseModel
        {
            //页面html
            string htmlStrAll = string.Empty;
            //控件html
            string htmlStr = string.Empty;

            Type entity = typeof(T);

            PropertyInfo[] propertys = entity.GetProperties();

            bool isVirtual = false;
            bool isStatic = false;

            string virtualPropertyName = string.Empty;

            object value = null;

            //实体名称
            var entityDisplayNameAttribute = entity.GetCustomAttribute<DisplayNameAttribute>();
            string entityDisplayName = entityDisplayNameAttribute == null ? "" : entityDisplayNameAttribute.Name;

            string displayName = string.Empty;

            //自定义类,字段排序用
            List<PropertyCustom> listPropertyCustom = new List<PropertyCustom>();
            PropertyCustom modelPropertyCustom = null;
            DisplayAttribute modelDisplayAttribute = null;
            ForeignKeyAttribute modelForeignKeyAttribute = null;

            //外键字段集合
            Dictionary<string, SelectModel> foreignKeyList = new Dictionary<string, SelectModel>();

            foreach (var itemProperty in propertys)
            {
                if (hideName && itemProperty.Name == "Name")
                {
                    continue;
                }

                isStatic = false;
                isVirtual = false;

                foreach (var item in itemProperty.GetAccessors())
                {
                    isStatic = item.IsStatic;
                    if (item.IsVirtual)
                    {
                        isVirtual = true;
                        virtualPropertyName = item.Name;
                        modelForeignKeyAttribute = itemProperty.GetCustomAttribute<ForeignKeyAttribute>();
                    }
                }

                if (isStatic)
                {
                    continue;
                }

                //虚属性找到相对应的属性并放到外键类集合里
                if (isVirtual && modelForeignKeyAttribute != null)
                {
                    SelectModel selectModel = new SelectModel();
                    selectModel.SelectEntityName = itemProperty.PropertyType.Name;
                    BaseModel modelForeign = itemProperty.GetValue(model) as BaseModel;
                    if (modelForeign != null)
                    {
                        selectModel.SelectText = modelForeign.Name;
                        selectModel.SelectValue = modelForeign.Id;
                    }
                    else
                    {
                        //用户类特殊处理，因为users类没有继承基类
                        Users modelForeignUser = itemProperty.GetValue(model) as Users;
                        if (modelForeignUser != null)
                        {
                            selectModel.SelectText = modelForeignUser.Name;
                            selectModel.SelectValue = modelForeignUser.Id;
                        }
                    }

                    foreignKeyList.Add(modelForeignKeyAttribute.Name, selectModel);
                    continue;
                }

                //找到字段顺序设置，并记录
                modelPropertyCustom = new PropertyCustom();
                modelPropertyCustom.PropertyInfo = itemProperty;
                //modelPropertyCustom.IsReadOnly = isVirtual;
                modelDisplayAttribute = itemProperty.GetCustomAttribute<DisplayAttribute>();
                if (modelDisplayAttribute != null)
                {
                    modelPropertyCustom.Order = modelDisplayAttribute.GetOrder().HasValue ? modelDisplayAttribute.GetOrder().Value : 0;
                }
                listPropertyCustom.Add(modelPropertyCustom);
            }



            foreach (var itemPropertyCustom in listPropertyCustom.OrderBy(m => m.Order))
            {
                var itemProperty = itemPropertyCustom.PropertyInfo;

                var propertyType = itemProperty.PropertyType;

                value = itemProperty.GetValue(model);

                var displayAttribute = itemProperty.GetCustomAttribute<DisplayAttribute>();

                var customColumnAttribute = itemProperty.GetCustomAttribute<CustomColumnAttribute>();

                var maxLengthAttribute = itemProperty.GetCustomAttribute<MaxLengthAttribute>();
                int maxLength = maxLengthAttribute == null ? 0 : maxLengthAttribute.Length;

                bool isHide = ((customColumnAttribute != null && customColumnAttribute.IsHide) || (hideTerritoryId && itemProperty.Name == "TerritoryId"));
                bool isRequired = (customColumnAttribute != null && customColumnAttribute.IsRequired);
                itemPropertyCustom.IsReadOnly = itemPropertyCustom.IsReadOnly || (customColumnAttribute != null && customColumnAttribute.IsReadOnly);

                displayName = displayAttribute == null || string.IsNullOrWhiteSpace(displayAttribute.Name) ? itemProperty.Name : displayAttribute.Name;

                string dateClass = customColumnAttribute == null ? "date" : customColumnAttribute.LaydateType.ToString();


                string selectEntityName = string.Empty;
                //若该属性是外键，找到对应的实体，并记录实体名称，下拉框用
                if (foreignKeyList.Any(m => m.Key == itemProperty.Name))
                {
                    selectEntityName = foreignKeyList[itemProperty.Name].SelectEntityName;
                }

                string propertyTypeName = propertyType.Name.ToLower();
                string fullTypeName = propertyType.FullName;

                if (propertyType.BaseType.Name == "Enum")
                {
                    propertyTypeName = "enum";
                }

                if (itemProperty.Name == "TerritoryId")
                {
                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum);
                }
                else
                {
                    switch (propertyTypeName)
                    {
                        //可空类型
                        case "nullable`1":
                            propertyTypeName = propertyType.GenericTypeArguments[0].Name.ToLower();
                            switch (propertyTypeName)
                            {
                                case "string":
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, "", "", "date", maxLength);
                                    break;
                                case "int32":
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, selectEntityName, foreignKeyList[itemProperty.Name].SelectText);
                                    break;
                                case "decimal":
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName);
                                    break;
                                case "datetime":
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, dateClass: dateClass);
                                    break;
                                case "boolean":
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName);
                                    break;
                                case "enum":
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName);
                                    break;
                                default:
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, "", "", "date", maxLength);
                                    break;
                            }
                            break;
                        case "string":
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, "", "", "date", maxLength);
                            break;
                        case "int32":
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName);
                            break;
                        case "decimal":
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName);
                            break;
                        case "datetime":
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, dateClass: dateClass);
                            break;
                        case "bool":
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName);
                            break;
                        case "enum":
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName);
                            break;
                        default:
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, "", "", "date", maxLength);
                            break;
                    }
                }

            }


            htmlStrAll = string.Format(GetCreatePageHtml<T>(entityDisplayName, model, model.Id == 0, backBtnUrl, buttonClass), htmlStr);

            return new HtmlString(htmlStrAll);
        }

        /// <summary>
        /// 生成添加/修改页面
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modelName">页面标题</param>
        /// <param name="model">model数据</param>
        /// <param name="isAddPage">是否为新增页面</param>
        /// <returns></returns>
        private static string GetCreatePageHtml<T>(string modelName, T model, bool isAddPage = true, string backBtnUrl = null, string buttonClass = "ajaxSave")
            where T : BaseModel
        {
            string operationName = isAddPage ? "添加" : "修改";
            string htmlStr = string.Empty;
            string formId = isAddPage ? "addForm" : "editForm";

            var thisModel = model as BaseParam;

            if (thisModel != null)
            {
                operationName = "修改";
                formId = "editForm";
                isAddPage = false;
            }

            htmlStr += "<div class=\"page-title\">";
            htmlStr += "<div>";
            htmlStr += "<h1><i class=\"fa fa-edit\"></i>" + modelName + "</h1>";
            htmlStr += "<p>" + operationName + modelName + "</p>";
            htmlStr += "</div>";
            htmlStr += "<div>";
            htmlStr += "<ul class=\"breadcrumb\">";
            htmlStr += "<li><a href = \"/Management/Home/Index\"><i class=\"fa fa-home fa-lg\"></i></a></li>";
            htmlStr += "<li><a href = \"List\">" + modelName + "</a></li>";
            htmlStr += "<li class=\"active\">" + operationName + modelName + "</li>";
            htmlStr += "</ul>";
            htmlStr += "</div>";
            htmlStr += "</div>";

            htmlStr += "<ul class=\"nav nav-tabs\" role=\"tablist\"><li id=\"tab1Title\" role=\"presentation\" class=\"active\"><a id=\"tabl1A\" href=\"#tab1\" aria-controls=\"tab1\" role=\"tab\" data-toggle=\"tab\">详情</a></li>";
            htmlStr += "<li id=\"tab2Title\" role=\"presentation\"><a id=\"tab2A\" href=\"#tab2\" aria-controls=\"tab2\" role=\"tab\" data-toggle=\"tab\">明细</a></li>";
            htmlStr += "<li id=\"tab3Title\" role=\"presentation\"><a id=\"tab3A\" href=\"#tab3\" aria-controls=\"tab3\" role=\"tab\" data-toggle=\"tab\">备用3</a></li>";
            htmlStr += "<li id=\"tab4Title\" role=\"presentation\"><a id=\"tab4A\" href=\"#tab4\" aria-controls=\"tab4\" role=\"tab\" data-toggle=\"tab\">备用4</a></li>";
            htmlStr += "<li id=\"tab5Title\" role=\"presentation\"><a id=\"tab5A\" href=\"#tab5\" aria-controls=\"tab5\" role=\"tab\" data-toggle=\"tab\">备用5</a></li>";
            htmlStr += "</ul>";

            htmlStr += "<div class=\"tab-content\"><div role =\"tabpanel\" class=\"tab-pane active\" id=\"tab1\">";

            htmlStr += "<form id = \"" + formId + "\" class=\"form-horizontal\" method=\"post\">";
            htmlStr += "<div class=\"card\">";
            htmlStr += "<div class=\"row\">";
            htmlStr += "<div class=\"col-md-12\">";
            htmlStr += "<div class=\"well bs-component\">";

            htmlStr += "<input type=\"hidden\" name=\"entityId\" id=\"EntityId\" value=\"" + model.Id + "\" />";
            htmlStr += "<fieldset>";
            //htmlStr += "<legend></legend>";
            htmlStr += "<div class=\"form-group\">";
            htmlStr += "{0}";

            htmlStr += "<div class=\"col-md-12\"><br/>";
            if (isAddPage)
            {
                htmlStr += ButtonHelper.GetBackButton();
                htmlStr += ButtonHelper.GetSaveButtonForAdd<T>(buttonClass);
            }
            else
            {
                htmlStr += ButtonHelper.GetBackButton(backBtnUrl);
                htmlStr += ButtonHelper.GetDeleteButton<T>(model, buttonClass);
                htmlStr += ButtonHelper.GetSaveButtonForEdit<T>(model, buttonClass);
            }

            htmlStr += "</div></div></fieldset></div></div></div></div>";

            #region 生成工作流按钮

            IWorkflowModel wModel = model as IWorkflowModel;

            if (wModel != null && wModel.Id != 0 && wModel.WorkflowStatus != WorkflowStatus.Approve)
            {
                BusinessWorkflow businessWorkflow = new BusinessWorkflow();
                List<WorkflowButton> listWorkflowButton = businessWorkflow.GetWorkflowButtonsByRecord<T>(wModel.Id);
                WorkflowStep modelWorkflowStep = businessWorkflow.GetCurrentStepByRecord<T>(model);
                WorkflowInstance modelWorkflowInstance = null;
                if (modelWorkflowInstance == null)
                {
                    BusinessWorkflowInstance businessWorkflowInstance = new BusinessWorkflowInstance();
                    modelWorkflowInstance = businessWorkflowInstance.GetById(wModel.WorkflowInstanceId, false);
                }
                else
                {
                    modelWorkflowInstance = wModel.WorkflowInstance;
                }
                string shenPiRen = businessWorkflow.GetShenPiRenNamesByInstance(modelWorkflowInstance);

                if (modelWorkflowStep != null)
                {
                    htmlStr += "<div class=\"card workflowCard\"><div class=\"card-body\"><div class=\"row\"><div class=\"col-md-6\">";
                    htmlStr += "当前状态：" + modelWorkflowStep.Name;
                    htmlStr += "</div>";
                    if (!string.IsNullOrWhiteSpace(shenPiRen))
                    {
                        htmlStr += "<div class=\"col-md-6\">当前操作人：" + shenPiRen + "</div>";
                    }
                    if (listWorkflowButton.Any())
                    {
                        htmlStr += "<div class=\"col-md-12\"><input type=\"text\" class=\"form-control\" placeholder=\"审批意见\" name=\"SPRemark\" id=\"SPRemark\" /></div>";
                        htmlStr += "<div class=\"col-md-12\">";
                        foreach (var itemWorkflowButton in listWorkflowButton)
                        {
                            htmlStr += "<button class=\"btn btn-sm btn-success btn-loading btn-edit ajaxSaveForOther btn-workflow\" type=\"button\" data-loading-text=\"操作中...\"";
                            htmlStr += "name=\"workflowButtons\" actionname=\"WorkflowButtonEvent\" validFor=\"editForm\"";
                            htmlStr += "value=\"" + itemWorkflowButton.Id + "\">";
                            htmlStr += itemWorkflowButton.Name;
                            htmlStr += "</button>&nbsp;";
                        }
                        htmlStr += "</div>";
                    }
                    htmlStr += "</div></div></div>";
                }

            }
            #endregion

            htmlStr += "</form>";
            htmlStr += "</div>";

            htmlStr += "<div role =\"tabpanel\" class=\"tab-pane\" id=\"tab2\"></div>";
            htmlStr += "<div role =\"tabpanel\" class=\"tab-pane\" id=\"tab3\"></div>";
            htmlStr += "<div role =\"tabpanel\" class=\"tab-pane\" id=\"tab4\"></div>";
            htmlStr += "<div role =\"tabpanel\" class=\"tab-pane\" id=\"tab5\"></div>";
            htmlStr += "</div>";

            return htmlStr;
        }

        /// <summary>
        /// 生成字段Html
        /// </summary>
        /// <typeparam name="T">基类为BaseModel的类</typeparam>
        /// <param name="typeStr">字段类型名称</param>
        /// <param name="name">字段名</param>
        /// <param name="displayName">显示名</param>
        /// <param name="value">字段值</param>
        /// <param name="model">实体值</param>
        /// <param name="hide">是否隐藏</param>
        /// <param name="columnCount">列数</param>
        /// <param name="isRequired">是否必填</param>
        /// <param name="isReadonly">是否只读</param>
        /// <param name="fullTypeName">完整字段名</param>
        /// <param name="selectEntityName">下拉框查询的实体名</param>
        /// <param name="selectText">下拉框默认值</param>
        /// <returns></returns>
        private static string GenerateHtmlStrByType<T>(string typeStr, string name, string displayName,
            object value, T model, bool hide = false, string mdNum = "6", bool isRequired = false, bool isReadonly = false,
            string fullTypeName = "", string selectEntityName = "", string selectText = "", string dateClass = "date", int columnLength = 0,
            string imgWidth = "200", string imgHeight = "200", bool isForSearch = false)
            where T : BaseModel
        {
            string htmlStr = string.Empty;

            displayName = (isRequired && !isForSearch) ? ("*" + displayName) : displayName;

            if (hide)
            {
                htmlStr = string.Format("<input class=\"" + name + "\" type=\"hidden\" name=\"{0}\" value=\"{1}\" />", name, value);
            }
            else if (name == "TerritoryId")
            {
                htmlStr =

                                        "<div class=\"col-md-" + mdNum + " " + name + "\">" +
                                        "<label id=\"lbl_TerritoryId\" class=\"control-label\" for=\"TerritoryId\">区域</label>" +
                                            "<select class=\"form-control select2 required\" id=\"TerritoryId\" name=\"TerritoryId\">";

                if (ListTerritoryForSelect != null)
                {
                    List<Territory> currentListTerritoryForSelect = new List<Territory>();
                    currentListTerritoryForSelect.AddRange(ListTerritoryForSelect);
                    var business = new CommonBusiness();
                    var dblist = business.DB.Territory.AsNoTracking().Where(m => m.Deleted == false).OrderBy(m => m.Depth).ToList();

                    var currentUser = business.GetCurrentUserInfo();

                    List<int?> otherTerritoryIds = null;

                    if (model.Id == 0)
                    {
                        otherTerritoryIds = business.DB.Purview.AsNoTracking().Where
                                (m => currentUser.TerritoryProfilesId != null && m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                                && m.PurviewType == PurviewType.OtherTerritory
                                && m.FullName == typeof(T).FullName && m.CanInsert == true
                                ).Select(m => m.OtherTerritoryId).Distinct().ToList();
                    }
                    else
                    {
                        otherTerritoryIds = business.DB.Purview.AsNoTracking().Where
                                (m => currentUser.TerritoryProfilesId != null && m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                                && m.PurviewType == PurviewType.OtherTerritory
                                && m.FullName == typeof(T).FullName && (m.CanUpdate == true || m.CanSelect == true)
                                ).Select(m => m.OtherTerritoryId).Distinct().ToList();
                    }
                    //添加其它区域权限的区域下拉框值
                    if (otherTerritoryIds != null && otherTerritoryIds.Any())
                    {
                        foreach (var itemOtherTerritoryId in otherTerritoryIds)
                        {
                            var tempTerr = dblist.FirstOrDefault(m => m.TerritoryId == itemOtherTerritoryId && m.Deleted == false);
                            GetTerritorys(dblist, tempTerr, ref currentListTerritoryForSelect, 1);
                        }
                    }

                    if (model != null && model.Id != 0 && value == null)
                    {
                        htmlStr += "<option value=\"\" "
                            + "selected=\"selected\"" + ">"
                            + "无</option >";
                    }
                    foreach (var itemTerritory in currentListTerritoryForSelect)
                    {
                        htmlStr += "<option value=\"" + itemTerritory.TerritoryId + "\" orgName=\"" + itemTerritory.Name + "\" "
                            + (model.TerritoryId == itemTerritory.TerritoryId ? "selected=\"selected\"" : "") + ">"
                            + itemTerritory.NameForSelect + "</option >";
                    }


                }
                htmlStr += "</select></div>";
            }
            else
            {
                switch (typeStr)
                {
                    case "int32":
                        htmlStr = GetHtmlForInt(name, displayName, ref value, model, isRequired, isReadonly, selectEntityName, ref selectText, mdNum, imgWidth, imgHeight);
                        break;
                    case "datetime":
                        htmlStr = GetHtmlForDateTime(name, displayName, value, isRequired, dateClass, mdNum);
                        break;
                    case "enum":
                        htmlStr = GetHtmlForEnum(name, displayName, value, model, isRequired, isReadonly, fullTypeName, htmlStr, mdNum);
                        break;
                    case "boolean":
                        htmlStr = GetHtmlForBool(name, displayName, value, isRequired, isReadonly, mdNum);
                        break;
                    case "decimal":
                        htmlStr = GetHtmlForDecimal(name, displayName, value, isRequired, isReadonly, mdNum);
                        break;
                    default:
                        htmlStr = GetHtmlForString(name, displayName, value, isRequired, isReadonly, columnLength, mdNum);
                        break;
                }
            }
            return htmlStr;
        }

        #region 生成String类型的字段HTML
        private static string GetHtmlForString(string name, string displayName, object value, bool isRequired, bool isReadonly, int columnLength, string mdNum)
        {
            string htmlStr;
            if (columnLength >= 100)
            {
                htmlStr = string.Format(
                            "<div class=\"col-md-" + mdNum + " " + name + "\">" +
                            "<label id=\"lbl_" + name + "\" class=\"control-label\" for=\"{0}\">{1}</label>" +
                                "<textarea id=\"{0}\" name=\"{0}\" placeholder=\"{1}\" {2} {4} class=\"form-control\" rows=\"3\">{3}</textarea>" +
                            "</div>", name, displayName, isRequired ? "required" : "", value, isReadonly ? "readonly=\"readonly\"" : "");
            }
            else
            {
                htmlStr = string.Format(
                            "<div class=\"col-md-" + mdNum + " " + name + "\">" +
                            "<label id=\"lbl_" + name + "\" class=\"control-label\" for=\"{0}\">{1}</label>" +
                                "<input class=\"form-control {2}\" id=\"{0}\" name=\"{0}\" type=\"text\" placeholder=\"{1}\" value=\"{3}\" {4}/>" +
                            "</div>", name, displayName, isRequired ? "required" : "", value, isReadonly ? "readonly=\"readonly\"" : "");
            }

            return htmlStr;
        }

        #endregion

        #region 生成Decimal类型的字段HTML
        private static string GetHtmlForDecimal(string name, string displayName, object value, bool isRequired, bool isReadonly, string mdNum)
        {
            return string.Format(
                                                    "<div class=\"col-md-" + mdNum + " " + name + "\">" +
                                                    "<label id=\"lbl_" + name + "\" class=\"control-label\" for=\"{0}\">{1}</label>" +
                                                        "<input class=\"form-control {2}\" id=\"{0}\" name=\"{0}\" type=\"number\" placeholder=\"{1}\" value=\"{3}\" {4}/>" +
                                                    "</div>", name, displayName, isRequired ? "required" : "", value, isReadonly ? "readonly=\"readonly\"" : "");
        }
        #endregion

        #region 生成Bool类型的字段HTML
        private static string GetHtmlForBool(string name, string displayName, object value, bool isRequired, bool isReadonly, string mdNum)
        {
            return string.Format("<div class=\"col-md-{0} {2}\">"
                                        + "<label id=\"lbl_{2}\" class=\"control-label\" for=\"{2}\">{1}</label>"
                                        + "<div class=\"toggle-flip\">"
                                        + "<label id=\"lbl_" + name + "\"><input type=\"checkbox\" value=\"true\" name=\"{2}\" id=\"{2}\" {3} {4} {5}>"
                                        + "<span class=\"flip-indecator\" data-toggle-on=\"是\" data-toggle-off=\"否\"></span></label>"
                                        + "</div></div>", mdNum, displayName, name, isRequired ? "required" : "",
                                        value != null && value.ToString().ToLower() == "true" ? "checked=\"checked\"" : "",
                                        isReadonly ? "readonly=\"readonly\"" : "");
        }
        #endregion

        #region 生成Enum类型的字段HTML
        private static string GetHtmlForEnum<T>(string name, string displayName, object value, T model, bool isRequired,
            bool isReadonly, string fullTypeName, string htmlStr, string mdNum) where T : BaseModel
        {
            if (!string.IsNullOrWhiteSpace(fullTypeName))
            {
                Type type = Type.GetType(fullTypeName);
                List<SelectListItem> list = EnumHelperEx.GetSelectList(type);
                htmlStr = string.Format(
                            "<div class=\"col-md-" + mdNum + " " + name + "\">" +
                            "<label id=\"lbl_" + name + "\" class=\"control-label\" for=\"{0}\">{1}</label>" +
                                "<select class=\"form-control {2} select2WithNoSearchText\" id=\"{0}\" name=\"{0}\" placeholder=\"{1}\">"
                                , name, displayName, isRequired ? "required" : "");

                if (isReadonly)
                {
                    if (model.Id == 0)
                    {
                        foreach (var item in list)
                        {
                            htmlStr += "<option value=\"" + item.Value + "\">" + item.Text + "</option>";
                            break;
                        }
                    }
                    else
                    {
                        foreach (var item in list)
                        {
                            if (value != null && value.ToString() == item.Value)
                            {
                                htmlStr += "<option selected=\"selected\" value=\"" + item.Value + "\">" + item.Text + "</option>";
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var item in list)
                    {
                        if (value != null && value.ToString() == item.Value)
                        {
                            htmlStr += "<option selected=\"selected\" value=\"" + item.Value + "\">" + item.Text + "</option>";
                        }
                        else
                        {
                            htmlStr += "<option value=\"" + item.Value + "\">" + item.Text + "</option>";
                        }
                    }
                }

                htmlStr += "</select></div>";

            }

            return htmlStr;
        }
        #endregion

        #region 生成DateTime类型的字段HTML
        private static string GetHtmlForDateTime(string name, string displayName, object value, bool isRequired, string dateClass, string mdNum)
        {
            string htmlStr;
            DateTime valueTemp = DateTime.MinValue;
            if (value != null)
            {
                DateTime.TryParse(value.ToString(), out valueTemp);
            }
            if (valueTemp == DateTime.MinValue)
            {
                valueTemp = DateTime.Now;
            }
            string finalValue = string.Empty;
            switch (dateClass)
            {
                case "date":
                    finalValue = value == null ? "" : valueTemp.ToString("yyyy-MM-dd");
                    break;
                case "datetime":
                    finalValue = value == null ? "" : valueTemp.ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                case "dateNoChoose":
                    finalValue = value == null ? "" : valueTemp.ToString("yyyy-MM-dd");
                    break;
                case "datetimeNoChoose":
                    finalValue = value == null ? "" : valueTemp.ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                default:
                    finalValue = value == null ? "" : valueTemp.ToString("yyyy-MM-dd");
                    break;
            }

            htmlStr = string.Format(
                            "<div class=\"col-md-" + mdNum + " " + name + "\">" +
                            "<label id=\"lbl_" + name + "\" class=\"control-label\" for=\"{0}\">{1}</label>" +
                                "<input class=\"form-control {4} {2}\" id=\"{0}\" name=\"{0}\" type=\"text\" readonly=\"readonly\" placeholder=\"{1}\" value=\"{3}\" />" +
                            "</div>", name, displayName, isRequired ? "required" : "", finalValue, dateClass);
            return htmlStr;
        }
        #endregion

        #region 生成Int类型的字段HTML
        private static string GetHtmlForInt<T>(string name, string displayName, ref object value, T model, bool isRequired,
            bool isReadonly, string selectEntityName, ref string selectText, string mdNum,
            string imgWidth = "200", string imgHeight = "200") where T : BaseModel
        {
            string htmlStr;
            if (!string.IsNullOrWhiteSpace(selectEntityName))
            {
                //用户外键，新增是默认选择当前用户
                if (selectEntityName == "Users" && model != null)
                {
                    CommonBusiness cb = new CommonBusiness();
                    if (model.Id == 0 && string.IsNullOrWhiteSpace(selectText))
                    {
                        var currentUser = cb.GetCurrentUserInfo();
                        if (currentUser != null)
                        {
                            selectText = string.Format("{0}({1})", currentUser.Name, currentUser.Code);
                            value = currentUser.Id;
                        }
                    }
                    else if (model.Id != 0 && value != null)
                    {
                        int userIdTemp = 0;
                        if (int.TryParse(value.ToString(), out userIdTemp))
                        {
                            var modelUserDB = cb.DB.Users.FirstOrDefault(m => m.Id == userIdTemp);
                            if (modelUserDB == null)
                            {
                                selectText = "人员已删除";
                            }
                            else
                            {
                                selectText = string.Format("{0}({1})", modelUserDB.Name, modelUserDB.Code);
                            }
                        }
                    }
                }

                //图片
                if (selectEntityName == "Attachment")
                {
                    htmlStr = BuildImgHtml(name, displayName, value, mdNum, imgWidth, imgHeight);
                }
                else if (name == "CreatedById")
                {
                    htmlStr = string.Format(
                                                            "<div class=\"col-md-" + mdNum + " " + name + "\">" +
                                                            "<label id=\"lbl_" + name + "\" class=\"control-label\" for=\"{0}\">{1}</label>" +
                                                                "<select class=\"form-control {2} {7}\" id=\"{0}\" name=\"{0}\" placeholder=\"{1}\" {6}>{5}</select>" +
                                                            "</div>",
                                                            name,
                                                            displayName,
                                                            isRequired ? "required" : "",
                                                            value,
                                                            isReadonly && model.Id != 0 ? "" : selectEntityName,
                                                            string.IsNullOrWhiteSpace(selectText) ? "" : "<option selected=\"selected\" value=\"" + value + "\">" + selectText + "</option>",
                                                            isReadonly ? "readonly=\"readonly\"" : "",
                                                            isReadonly && model.Id != 0 ? "select2WithNoSearchText" : "select2WithNoSearchText");
                }
                else
                {
                    htmlStr = string.Format(
                                                            "<div class=\"col-md-" + mdNum + " " + name + "\">" +
                                                            "<label id=\"lbl_" + name + "\" class=\"control-label\" for=\"{0}\">{1}</label>" +
                                                                "<select class=\"form-control {2} {7}\" id=\"{0}\" name=\"{0}\" placeholder=\"{1}\" selectEntityName=\"{4}\" {6}>{5}</select>" +
                                                            "</div>",
                                                            name,
                                                            displayName,
                                                            isRequired ? "required" : "",
                                                            value,
                                                            isReadonly && model.Id != 0 ? "" : selectEntityName,
                                                            string.IsNullOrWhiteSpace(selectText) ? "" : "<option selected=\"selected\" value=\"" + value + "\">" + selectText + "</option>",
                                                            isReadonly ? "readonly=\"readonly\"" : "",
                                                            isReadonly && model.Id != 0 ? "select2WithNoSearchText" : "select2");
                }
            }
            else
            {
                htmlStr = string.Format(
                            "<div class=\"col-md-" + mdNum + " " + name + "\">" +
                            "<label id=\"lbl_" + name + "\" class=\"control-label\" for=\"{0}\">{1}</label>" +
                                "<input class=\"form-control {2}\" id=\"{0}\" name=\"{0}\" type=\"number\" placeholder=\"{1}\" value=\"{3}\" {4} />" +
                            "</div>", name, displayName, isRequired ? "required" : "", value, isReadonly ? "readonly=\"readonly\"" : "");
            }

            return htmlStr;
        }

        /// <summary>
        /// 生成上传图片的HTML
        /// </summary>
        /// <param name="name">name属性</param>
        /// <param name="displayName">标题</param>
        /// <param name="value">附件id</param>
        /// <param name="mdNum">col-md-mdNum</param>
        /// <returns></returns>
        private static string BuildImgHtml(string name, string displayName, object value, string mdNum, string img_width = "200", string img_height = "200")
        {
            string htmlStr;
            string imgUrl = string.Format("/Content/images/imgUp/a11.png?{0}", DateTime.Now.Millisecond);

            int valueInt = 0;
            if (value != null && int.TryParse(value.ToString(), out valueInt))
            {
                BusinessAttachment businessAttachment = new BusinessAttachment();
                Attachment modelAttachment = businessAttachment.GetById(valueInt, false);
                if (modelAttachment != null && !string.IsNullOrWhiteSpace(modelAttachment.Path))
                {
                    imgUrl = string.Format("{0}?{1}", modelAttachment.Path, DateTime.Now.Millisecond);
                }
            }

            htmlStr = string.Format(
                "<div class=\"col-md-" + mdNum + " " + name + "\">" +
                "<div>" +
                    "<label class=\"control-label\" for=\"{0}\">{3}({4}*{5})</label>" +
                    "<div class=\"img-box full\">" +
                        "<input type=\"file\" class=\"img-up hidden\" value=\"选择图片\" />" +
                        "<div style=\"width:{4};height:{5}\">" +
                        "<img class=\"img-init\" src=\"{2}\" width=\"{4}\" height=\"{5}\" />" +
                        "</div>" +
                        "<input type=\"button\" class=\"btn btn-primary btn-img-save hidden\" value=\"上传图片\" />" +
                        "<input name=\"{0}\" id=\"{0}\" value=\"{1}\" class=\"img-base64\" type=\"hidden\" />" +
                    "</div>" +
                "</div></div>", name, value, imgUrl, displayName, img_width, img_height);
            return htmlStr;
        }
        #endregion

        #endregion

        #region 生成基类为BaseModel的查询页面
        /// <summary>
        /// 是否有高级搜索
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="hasGjss">是否需要高级搜索</param>
        /// <returns></returns>
        private static string GetListPageHtml<T>(bool hasGjss)
            where T : BaseModel
        {
            string htmlStr = string.Empty;
            string entityDisplayName = CommonBusiness.GetDisplayName<T>();
            string entityName = typeof(T).Name;

            bool needAddButton = _listEntityModels.FirstOrDefault(m => m.FullName == typeof(T).FullName)?.NeedAddButton ?? false;

            string classDisplay = hasGjss ? "" : "hidden";

            htmlStr += "<div class=\"page-title\"><div>";
            htmlStr += $"<h1>{entityDisplayName}列表</h1>";
            htmlStr += "<ul class=\"breadcrumb side\"><li><a href=\"~/Management/Home/Index\"><i class=\"fa fa-home fa-lg\"></i></a></li><li>管理</li>";
            htmlStr += $"<li class=\"active\">{entityDisplayName}</li></ul></div><div>";
            if(needAddButton)
            {
                htmlStr += ButtonHelper.GetAddButton<T>();
            }
            htmlStr += "</div></div>";

            htmlStr += "<div class=\"row\">";
            htmlStr += "<div class=\"col-md-12\">";
            htmlStr += "<div class=\"card\">";
            htmlStr += "<div class=\"card-body\">";
            htmlStr += string.Format("<div class=\"panel panel-default {0}\">", classDisplay);
            htmlStr += "<div class=\"panel-heading\" role=\"tab\" id=\"collapseListGroupHeading1\">";
            htmlStr += "<h4 class=\"panel-title\">";
            htmlStr += "<a class=\"collapsed\" role=\"button\" data-toggle=\"collapse\" href=\"#collapseListGroup1\" aria-expanded=\"false\" aria-controls=\"collapseListGroup1\">高级搜索</a>";
            htmlStr += "</h4></div>";
            htmlStr += "<div id=\"collapseListGroup1\" class=\"panel-collapse collapse\" role=\"tabpanel\" aria-labelledby=\"collapseListGroupHeading1\">";
            htmlStr += "<form id=\"searchForm\" class=\"form-inline\">";
            htmlStr += "<div class=\"row\">{0}</div>";
            htmlStr += string.Format("<div>&nbsp;</div></form></div></div><div><table id=\"sampleTable\" entity=\"{0}\"></table></div></div></div></div></div>", entityName);
            htmlStr += "<div class=\"row\">";
            htmlStr += "<div class=\"row\">";
            htmlStr += "<div class=\"row\">";
            htmlStr += "<div class=\"row\">";
            htmlStr += "<div class=\"row\">";
            htmlStr += "<div class=\"row\">";
            htmlStr += "<div class=\"row\">";

            return htmlStr;
        }

        public static HtmlString BuildListPage<T>(string mdNum = "5")
            where T : BaseModel
        {
            //页面html
            string htmlStrAll = string.Empty;
            //控件html
            string htmlStr = string.Empty;

            Type entity = typeof(T);

            var model = Activator.CreateInstance<T>();

            PropertyInfo[] propertys = entity.GetProperties();

            string virtualPropertyName = string.Empty;

            object value = null;

            //实体名称
            var entityDisplayNameAttribute = entity.GetCustomAttribute<DisplayNameAttribute>();
            string entityDisplayName = entityDisplayNameAttribute == null ? "" : entityDisplayNameAttribute.Name;

            string displayName = string.Empty;

            //自定义类,字段排序用
            List<PropertyCustom> listPropertyCustom = new List<PropertyCustom>();
            PropertyCustom modelPropertyCustom = null;
            DisplayAttribute modelDisplayAttribute = null;

            //外键字段集合
            Dictionary<string, SelectModel> foreignKeyList = new Dictionary<string, SelectModel>();

            foreach (var itemProperty in propertys)
            {
                var customColumnAttribute = itemProperty.GetCustomAttribute<CustomColumnAttribute>();

                if (customColumnAttribute?.IsSearch == true)
                {
                    modelPropertyCustom = new PropertyCustom();
                    modelPropertyCustom.PropertyInfo = itemProperty;
                    modelPropertyCustom.IsSearch = true;
                    modelDisplayAttribute = itemProperty.GetCustomAttribute<DisplayAttribute>();
                    if (modelDisplayAttribute != null)
                    {
                        modelPropertyCustom.Order = modelDisplayAttribute.GetOrder().HasValue ? modelDisplayAttribute.GetOrder().Value : 0;
                    }
                    listPropertyCustom.Add(modelPropertyCustom);
                }
            }

            foreach (var itemPropertyCustom in listPropertyCustom.OrderBy(m => m.Order))
            {
                var itemProperty = itemPropertyCustom.PropertyInfo;

                var propertyType = itemProperty.PropertyType;

                value = null;

                var displayAttribute = itemProperty.GetCustomAttribute<DisplayAttribute>();

                var customColumnAttribute = itemProperty.GetCustomAttribute<CustomColumnAttribute>();

                var maxLengthAttribute = itemProperty.GetCustomAttribute<MaxLengthAttribute>();
                int maxLength = maxLengthAttribute == null ? 0 : maxLengthAttribute.Length;

                bool isHide = false;

                bool isRequired = (customColumnAttribute != null && customColumnAttribute.IsRequired);
                itemPropertyCustom.IsReadOnly = itemPropertyCustom.IsReadOnly || (customColumnAttribute != null && customColumnAttribute.IsReadOnly);

                displayName = displayAttribute == null || string.IsNullOrWhiteSpace(displayAttribute.Name) ? itemProperty.Name : displayAttribute.Name;

                string dateClass = customColumnAttribute == null ? "date" : customColumnAttribute.LaydateType.ToString();


                string selectEntityName = string.Empty;
                //若该属性是外键，找到对应的实体，并记录实体名称，下拉框用
                if (foreignKeyList.Any(m => m.Key == itemProperty.Name))
                {
                    selectEntityName = foreignKeyList[itemProperty.Name].SelectEntityName;
                }

                string propertyTypeName = propertyType.Name.ToLower();
                string fullTypeName = propertyType.FullName;

                if (propertyType.BaseType.Name == "Enum")
                {
                    propertyTypeName = "enum";
                }

                if (itemProperty.Name == "TerritoryId")
                {
                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum);
                }
                else
                {
                    switch (propertyTypeName)
                    {
                        //可空类型
                        case "nullable`1":
                            propertyTypeName = propertyType.GenericTypeArguments[0].Name.ToLower();
                            switch (propertyTypeName)
                            {
                                case "string":
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, "", "", "date", maxLength, isForSearch: true);
                                    break;
                                case "int32":
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, selectEntityName, null, isForSearch: true);
                                    break;
                                case "decimal":
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, isForSearch: true);
                                    break;
                                case "datetime":
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, dateClass: dateClass, isForSearch: true);
                                    break;
                                case "boolean":
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, isForSearch: true);
                                    break;
                                case "enum":
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, isForSearch: true);
                                    break;
                                default:
                                    htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, "", "", "date", maxLength, isForSearch: true);
                                    break;
                            }
                            break;
                        case "string":
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, "", "", "date", maxLength, isForSearch: true);
                            break;
                        case "int32":
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, isForSearch: true);
                            break;
                        case "decimal":
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, isForSearch: true);
                            break;
                        case "datetime":
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, dateClass: dateClass, isForSearch: true);
                            break;
                        case "bool":
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, isForSearch: true);
                            break;
                        case "enum":
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, isForSearch: true);
                            break;
                        default:
                            htmlStr += GenerateHtmlStrByType<T>(propertyTypeName, itemProperty.Name, displayName, value, model, isHide, mdNum, isRequired, itemPropertyCustom.IsReadOnly, fullTypeName, "", "", "date", maxLength, isForSearch: true);
                            break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(htmlStr))
            {
                htmlStr += "<div class=\"col-md-1\"><br /><input id=\"btn-search\" type=\"button\" class=\"form-control btn btn-info btn-sm btnForSearch\" value=\"查询\" /></div>";
                htmlStr += "<div class=\"col-md-1\"><br /><input type=\"reset\" id=\"resetSearch\" class=\"form-control btn btn-default btn-sm btnForSearch\" value=\"清空条件\" /></div>";
            }


            htmlStrAll = string.Format(GetListPageHtml<T>(listPropertyCustom.Any()), htmlStr);

            return new HtmlString(htmlStrAll);
        }

        #endregion
    }

}