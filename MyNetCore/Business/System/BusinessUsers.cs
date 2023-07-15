using System;
using System.Collections.Generic;
using System.Linq;
using MyNetCore.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Roim.Common;

namespace MyNetCore.Business
{
    public class BusinessUsers : CommonBusiness
    {
        public BusinessUsers()
        {

        }

        /// <summary>
        /// 获得所有数据(前10000条)
        /// </summary>
        /// <returns></returns>
        public IQueryable<Users> GetList(int length = 10000, bool needCheckRight = true)
        {
            int totalCount = 0;
            DataTableParameters param = new DataTableParameters();
            param.PageSize = length;
            param.PageNumber = 1;
            return GetList(param, out totalCount, null, null, null, needCheckRight);
        }

        /// <summary>
        /// 根据条件获取用户
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="needCheckRight"></param>
        /// <returns></returns>
        public Users GetByCondition(Expression<Func<Users, bool>> predicate, bool needCheckRight = true)
        {
            int totalCount = 0;
            DataTableParameters param = new DataTableParameters();
            param.PageSize = 1;
            param.PageNumber = 1;
            var list = GetList(param, out totalCount, predicate, null, null, needCheckRight);
            if (list == null || !list.Any())
            {
                return null;
            }

            return list.FirstOrDefault();
        }

        /// <summary>
        /// 获取无权限时的数据(区域为空或创建人为自己)
        /// </summary>
        /// <returns></returns>
        private IQueryable<Users> GetNoPurviewData(Territory modelTerrForCurrentUser)
        {
            if (modelTerrForCurrentUser == null || !modelTerrForCurrentUser.TerritoryId.HasValue)
            {
                return DB.Set<Users>().Where(m => m.Id < -10000);
            }
            var currentUser = GetCurrentUserInfo();
            return DB.Set<Users>().AsNoTracking().Where(m => m.Deleted == false &&
            (m.TerritoryId == null
            ||
            (m.TerritoryId >= modelTerrForCurrentUser.TerritoryId && m.TerritoryId <= modelTerrForCurrentUser.RangeEnd && m.CreatedById == currentUser.Id)
            ));
        }

        /// <summary>
        /// 根据ID获取用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="needCheckRight"></param>
        /// <returns></returns>
        public Users GetById(int id, bool needCheckRight = true)
        {
            int totalCount = 0;
            DataTableParameters param = new DataTableParameters();
            param.PageSize = 1;
            param.PageNumber = 1;
            var list = GetList(param, out totalCount, x => x.Id == id, null, null, needCheckRight);
            if (list != null && list.Any())
            {
                return list.FirstOrDefault();
            }
            return null;

        }

        /// <summary>
        /// 查询用户
        /// </summary>
        /// <param name="param"></param>
        /// <param name="totalCount"></param>
        /// <param name="predicate"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="isDESC"></param>
        /// <param name="needCheckRight"></param>
        /// <param name="asNoTracking"></param>
        /// <returns></returns>
        public IQueryable<Users> GetList(DataTableParameters param, out int totalCount, Expression<Func<Users, bool>> predicate = null,
            string orderByExpression = null, bool? isDESC = null, bool needCheckRight = true, bool asNoTracking = true)
        {
            int page = 1;
            int pageSize = 100000;
            var currentUser = GetCurrentUserInfo();
            if (param != null)
            {
                pageSize = param.PageSize;
                page = param.PageNumber;

                if (string.IsNullOrWhiteSpace(orderByExpression))
                {
                    orderByExpression = string.IsNullOrWhiteSpace(param.Sort) ? "Id" : param.Sort;
                }

                orderByExpression = orderByExpression.Substring(0, 1).ToUpper() + orderByExpression.Substring(1);

                if (!isDESC.HasValue)
                {
                    isDESC = (param.SortOrder == "desc");
                }

                if (!string.IsNullOrWhiteSpace(param.Search))
                {
                    if (predicate == null)
                    {
                        predicate = (m => m.Name.Contains(param.Search));
                    }
                }
            }

            totalCount = 0;

            int skip = (page - 1) * pageSize;

            if (string.IsNullOrWhiteSpace(orderByExpression))
            {
                orderByExpression = "Id";
            }

            IQueryable<Users> list = null;

            Expression<Func<Users, bool>> predicateTemp = m => m.Deleted == false;

            if (asNoTracking)
            {
                list = DB.Set<Users>().Where(predicateTemp).AsNoTracking();
            }
            else
            {
                list = DB.Set<Users>().Where(predicateTemp);
            }

            if (needCheckRight)
            {
                if (currentUser == null)
                {
                    ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                }

                if (currentUser.UserType != UserType.Admin)
                {
                    BusinessTerritory bt = new BusinessTerritory();

                    if (currentUser.TerritoryId == null)
                    {
                        //若没权限，则只能看到自己创建的数据
                        list = GetNoPurviewData(null);
                    }
                    else
                    {
                        IEnumerable<Territory> listTerritoryAll = BusinessHelper.ListTerritory;
                        //用户所在区域
                        Territory modelTerrForCurrentUser = listTerritoryAll.FirstOrDefault(m => m.TerritoryId == currentUser.TerritoryId && m.Deleted == false);

                        if (modelTerrForCurrentUser == null)
                        {
                            //若没权限，则只能看到自己创建的数据
                            list = GetNoPurviewData(modelTerrForCurrentUser);
                        }
                        else
                        {
                            //根据登录人权限筛选数据
                            #region 根据登录人权限筛选数据

                            IQueryable<Purview> listPurview = DB.Purview.AsNoTracking().Where
                                (m => currentUser.TerritoryProfilesId != null && m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                                && m.FullName == typeof(Users).FullName && m.CanSelect == true
                                );

                            if (listPurview == null || !listPurview.Any() || !modelTerrForCurrentUser.TerritoryId.HasValue)
                            {
                                list = GetNoPurviewData(modelTerrForCurrentUser);
                            }
                            else
                            {
                                var listPurviewMyTerritory = listPurview.Where(m => m.PurviewType == PurviewType.MyTerritory);
                                var listPurviewOwener = listPurview.Where(m => m.PurviewType == PurviewType.Owner);
                                var listPurviewOtherTerritorys = listPurview.Where(m => m.PurviewType == PurviewType.OtherTerritory);



                                Expression<Func<Users, bool>> predicateTemp2 = m => false;

                                if (listPurviewMyTerritory != null && listPurviewMyTerritory.Any())
                                {
                                    predicateTemp2 = predicateTemp2.
                                        Or(m => m.Deleted == false && m.TerritoryId >= modelTerrForCurrentUser.TerritoryId
                                        && m.TerritoryId <= modelTerrForCurrentUser.RangeEnd);
                                }

                                if (listPurviewOwener != null && listPurviewOwener.Any())
                                {
                                    predicateTemp2 = predicateTemp2.
                                        Or(m => m.Deleted == false && m.CreatedById == currentUser.Id);
                                }

                                if (listPurviewOtherTerritorys != null && listPurviewOtherTerritorys.Any())
                                {
                                    var listOtherTerritoryId = listPurviewOtherTerritorys.Select(m => m.OtherTerritoryId).Distinct();
                                    foreach (var itemOtherTerritoryId in listOtherTerritoryId)
                                    {
                                        if (!itemOtherTerritoryId.HasValue)
                                        {
                                            continue;
                                        }
                                        Territory modelTerritory = listTerritoryAll.FirstOrDefault(m => m.TerritoryId == itemOtherTerritoryId && m.Deleted == false);
                                        if (modelTerritory == null)
                                        {
                                            continue;
                                        }
                                        predicateTemp2 = predicateTemp2.
                                        Or(m => m.Deleted == false && m.TerritoryId >= modelTerritory.TerritoryId
                                        && m.TerritoryId <= modelTerritory.RangeEnd);
                                    }
                                }

                                predicateTemp = predicateTemp.And(predicateTemp2);

                                if (asNoTracking)
                                {
                                    list = DB.Set<Users>().AsNoTracking().Where(predicateTemp);
                                }
                                else
                                {
                                    list = DB.Set<Users>().Where(predicateTemp);
                                }

                            }
                            #endregion
                        }

                    }
                }
            }

            if (list != null && predicate != null)
            {
                list = list.Where(predicate);
            }

            totalCount = list == null ? 0 : list.Count();

            if (page != 0 && list != null)
            {
                var property = typeof(Users).GetProperty(orderByExpression);
                var parameter = Expression.Parameter(typeof(Users), "p");
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExp = Expression.Lambda(propertyAccess, parameter);
                string methodName = isDESC.HasValue && isDESC.Value ? "OrderByDescending" : "OrderBy";
                MethodCallExpression resultExp =
                    Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(Users), property.PropertyType },
                    list.Expression, Expression.Quote(orderByExp));
                list = list.Provider.CreateQuery<Users>(resultExp);

                list = list.Skip(skip).Take(pageSize);
            }

            return list;
        }

        /// <summary>
        /// 查询用户
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount"></param>
        /// <param name="predicate"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="isDESC"></param>
        /// <param name="needCheckRight"></param>
        /// <returns></returns>
        public IQueryable<Users> GetList(int page, int pageSize, out int totalCount, Expression<Func<Users, bool>> predicate = null,
            string orderByExpression = null, bool? isDESC = null, bool needCheckRight = true)
        {
            DataTableParameters param = new DataTableParameters();
            param.PageNumber = page;
            param.PageSize = pageSize;

            return GetList(param, out totalCount, predicate, orderByExpression, isDESC, needCheckRight);
        }

        /// <summary>
        /// 根据code获取用户
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Users GetUserByCode(string code)
        {
            return DB.Users.AsNoTracking().FirstOrDefault(m => m.Code == code && m.Deleted == false);
        }

        public Users Add(Users model, bool needCheckRight = true)
        {
            if (model == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.数据提交失败);
            }

            var currentUser = GetCurrentUserInfo();

            if (needCheckRight)
            {
                if (currentUser == null)
                {
                    ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                }

                if (!currentUser.IsAdmin)
                {
                    Territory modelTerrForCurrentUser = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == currentUser.TerritoryId && m.Deleted == false);

                    if (modelTerrForCurrentUser == null || !modelTerrForCurrentUser.TerritoryId.HasValue)
                    {
                        ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                    }

                    IQueryable<Purview> listPurview = DB.Purview.Where
                                    (m => currentUser.TerritoryProfilesId != null && m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                                    && m.FullName == typeof(Users).FullName && m.CanInsert == true
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

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未填写名称);
            }

            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未填写账号);
            }

            if (model.Code.Length > 30)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.账号最大长度为30);
            }

            if (string.IsNullOrWhiteSpace(model.PassWord))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未填写密码);
            }

            if (DB.Users.Any(m => m.Deleted == false && m.Code == model.Code))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.账号已存在);
            }

            model.PassWord = Roim.Common.DEncrypt.DEncrypt.DES(model.PassWord);
            if (model.CreatedById == 0)
            {
                model.CreatedById = currentUser.Id;
            }
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;
            model.UpdatedById = model.CreatedById;
            DB.Entry<Users>(model).State = EntityState.Added;
            DB.SaveChanges();
            DB.Entry(model).State = EntityState.Detached;
            return model;
        }

        /// <summary>
        /// 企业号或公众号自动注册
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Users AddForAuto(Users model)
        {
            if (model == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.数据提交失败);
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未填写名称);
            }

            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未填写账号);
            }

            if (string.IsNullOrWhiteSpace(model.PassWord))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未填写密码);
            }

            if (DB.Users.Any(m => m.Deleted == false && m.Code == model.Code))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.账号已存在);
            }

            model.PassWord = Roim.Common.DEncrypt.DEncrypt.DES(model.PassWord);
            if (model.CreatedById == 0)
            {
                model.CreatedById = 1;
            }
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = DateTime.Now;
            model.UpdatedById = model.CreatedById;

            if (WeChatMiniSettingParam.MyConfig.TerritoryProfilesId == 0)
            {
                ThrowErrorInfo("尚未开放注册");
            }

            model.ChannelIds = WeChatMiniSettingParam.MyConfig.ChannelIds;
            model.TerritoryId = WeChatMiniSettingParam.MyConfig.DefaultTerritoryId;
            model.TerritoryProfilesId = WeChatMiniSettingParam.MyConfig.TerritoryProfilesId;
            DB.Entry<Users>(model).State = EntityState.Added;
            DB.SaveChanges();
            return model;
        }

        /// <summary>
        /// 编辑用户
        /// </summary>
        /// <param name="model"></param>
        /// <param name="needCheckRight"></param>
        /// <param name="needUpdatePwd">要修改密码</param>
        /// <returns></returns>
        public Users Edit(Users model, bool needCheckRight = true, bool needUpdatePwd = false)
        {
            if (model == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.数据提交失败);
            }
            var currentUser = GetCurrentUserInfo();
            if (needCheckRight)
            {
                if (currentUser == null)
                {
                    ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                }

                if (!currentUser.IsAdmin)
                {
                    Territory modelTerrForCurrentUser = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == currentUser.TerritoryId && m.Deleted == false);

                    if (modelTerrForCurrentUser == null)
                    {
                        ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                    }

                    IQueryable<Purview> listPurview = DB.Purview.Where
                                    (m => currentUser.TerritoryProfilesId != null && m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                                    && m.FullName == typeof(Users).FullName && m.CanUpdate == true
                                    );

                    if (listPurview == null || !listPurview.Any(m => m.CanUpdate == true))
                    {
                        ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
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
                        ThrowErrorInfo(MessageText.ErrorInfo.您无此区域的修改权限);
                    }

                }
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未填写名称);
            }

            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未填写账号);
            }

            if ((model.UserType != UserType.Admin || model.Disabled == true)
                && !DB.Users.Any(m => m.Id != model.Id && m.UserType == UserType.Admin && m.Deleted == false
            && m.Disabled == false))
            {
                ThrowErrorInfo("至少要保留一个系统管理员用户");
            }

            if (!string.IsNullOrWhiteSpace(model.PassWord) && needUpdatePwd)
            {
                model.PassWord = Roim.Common.DEncrypt.DEncrypt.DES(model.PassWord);
            }
            else
            {
                Users dbModel = GetByCondition(m => m.Id == model.Id, false);
                model.PassWord = dbModel == null ? null : dbModel.PassWord;
            }

            DB.Entry(model).State = EntityState.Modified;

            DB.SaveChanges();

            return model;
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="needCheckRight"></param>
        public void Delete(int id, bool needCheckRight = true)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
            }

            if (currentUser.Id == id)
            {
                ThrowErrorInfo("不能删除自己");
            }

            if (id == 1)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.系统保留数据不能删除);
            }

            Users model = DB.Users.FirstOrDefault(m => m.Id == id);

            if (model == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未找到要删除的数据);
            }

            if (!DB.Users.Any(m => m.Id != model.Id && m.UserType == UserType.Admin && m.Deleted == false
            && m.Disabled == false))
            {
                ThrowErrorInfo("至少要保留一个系统管理员用户");
            }

            if (needCheckRight)
            {
                if (currentUser == null)
                {
                    ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                }

                if (!currentUser.IsAdmin)
                {
                    Territory modelTerrForCurrentUser = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == currentUser.TerritoryId && m.Deleted == false);

                    if (modelTerrForCurrentUser == null)
                    {
                        ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                    }

                    IQueryable<Purview> listPurview = DB.Purview.Where
                                    (m => currentUser.TerritoryProfilesId != null && m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                                    && m.FullName == typeof(Users).FullName && m.CanDelete == true
                                    );

                    if (listPurview == null || !listPurview.Any(m => m.CanDelete == true))
                    {
                        ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
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
                        ThrowErrorInfo(MessageText.ErrorInfo.您无此数据的删除权限);
                    }

                }
            }

            model.UpdatedById = currentUser.Id;
            model.Deleted = true;
            model.UpdatedDate = DateTime.Now;
            DB.Entry(model).State = EntityState.Modified;
            DB.SaveChanges();
            DB.Entry(model).State = EntityState.Detached;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Users UpdatePassword(string code, string password, int updatedBy = 1)
        {
            Users model = DB.Users.FirstOrDefault(m => m.Code == code && m.Deleted == false);
            if (model == null)
            {
                ThrowErrorInfo("无此用户信息");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ThrowErrorInfo("密码为必填项");
            }

            model.PassWord = Roim.Common.DEncrypt.DEncrypt.DES(password);
            model.UpdatedDate = DateTime.Now;
            model.UpdatedById = updatedBy;
            DB.Entry(model).State = EntityState.Modified;
            DB.SaveChanges();
            DB.Entry(model).State = EntityState.Detached;
            return model;
        }

        /// <summary>
        /// 根据id集合获取用户
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="needCheckRight"></param>
        /// <returns></returns>
        public IQueryable<Users> GetListByCondition(Expression<Func<Users, bool>> predicate, bool needCheckRight = true)
        {
            int total = 0;
            return GetList(null, out total, predicate, null, null, needCheckRight);
        }

        /// <summary>
        /// 根据id集合获取数据集合
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<Users> GetListByIds(string ids, bool needCheckRight = true)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return null;
            }
            ids = "," + ids + ",";
            return GetListByCondition(m => ids.Contains("," + m.Id + ","), needCheckRight).ToList();
        }

        /// <summary>
        /// 获得小组下所有用户
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public List<Users> GetListByChannelId(int channelId, bool needCheckRight = true)
        {
            if (channelId == 0)
            {
                return null;
            }

            string channelIdStr = string.Format(",{0},", channelId);
            var listUsersAllInChannel = GetListByCondition(m => ("," + m.ChannelIds + ",").Contains(channelIdStr), needCheckRight);
            if (listUsersAllInChannel != null && listUsersAllInChannel.Any())
            {
                return listUsersAllInChannel.ToList();
            }

            return null;

        }

        /// <summary>
        /// 根据用户ID获得上级用户信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Users GetShangJiByUserId(int? userId)
        {
            if (!userId.HasValue)
            {
                return null;
            }

            Users model = GetById(userId.Value, false);
            if (model == null || !model.LineManageId.HasValue)
            {
                return null;
            }

            return GetByCondition(m => m.Id == model.LineManageId, false);

        }

        /// <summary>
        /// 修改当前用户的用户名和密码
        /// </summary>
        /// <param name="code">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="phoneNum">手机号</param>
        public void UpdateUserInfo(string code, string password, string phoneNum)
        {
            Users currentUser = GetCurrentUserInfo();

            if (currentUser == null || currentUser.Id == 0)
            {
                ThrowErrorInfo("未获得当前用户身份,请重新登录");
            }

            if (currentUser.Code != code && currentUser.Code?.Length <= 20)
            {
                ThrowErrorInfo("用户名只能修改一次");
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                ThrowErrorInfo("用户名为必填项");
            }

            if (code.Length > 30)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.账号最大长度为30);
            }

            if (DB.Users.Any(m => m.Id != currentUser.Id && m.Code == code && m.Deleted == false))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.账号已存在);
            }

            if (!string.IsNullOrWhiteSpace(password))
            {
                currentUser.PassWord = Roim.Common.DEncrypt.DEncrypt.DES(password);
            }

            currentUser.Code = code;
            currentUser.PhoneNum = phoneNum;
            currentUser.UpdatedDate = DateTime.Now;
            currentUser.UpdatedById = currentUser.Id;
            DB.Entry(currentUser).State = EntityState.Modified;
            DB.SaveChanges();
            DB.Entry(currentUser).State = EntityState.Detached;
        }

        #region 检查权限
        /// <summary>
        /// 检查是否有数据查询权限
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <param name="modelUser"></param>
        /// <returns></returns>
        public bool CheckHasRecordRight(Users model)
        {
            var currentUser = GetCurrentUserInfo();

            if (model == null || currentUser == null
                || !currentUser.TerritoryId.HasValue || !currentUser.TerritoryProfilesId.HasValue)
            {
                return false;
            }

            if (currentUser.IsAdmin)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 检查是否有添加权限
        /// </summary>
        /// <typeparam name="Tmodel"></typeparam>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool CheckHasAddRight(out string errorMsg)
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
            else
            {
                errorMsg = MessageText.ErrorInfo.您无此操作权限.ToString();
                return false;
            }
        }

        /// <summary>
        /// 检查是否有列表查看权限
        /// </summary>
        /// <typeparam name="Tmodel"></typeparam>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool CheckHasSelectRight(out string errorMsg)
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
            else
            {
                errorMsg = MessageText.ErrorInfo.您无此操作权限.ToString();
                return false;
            }
        }

        /// <summary>
        /// 检查是否有修改权限
        /// </summary>
        /// <typeparam name="Tmodel"></typeparam>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool CheckHasEditRight(Users model, out string errorMsg)
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
            else
            {
                errorMsg = MessageText.ErrorInfo.您无此操作权限.ToString();
                return false;
            }
        }

        /// <summary>
        /// 检查是否有删除权限
        /// </summary>
        /// <typeparam name="Tmodel"></typeparam>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public bool CheckHasDeleteRight(Users model, out string errorMsg)
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
            else
            {
                errorMsg = MessageText.ErrorInfo.您无此操作权限.ToString();
                return false;
            }
        }

        /// <summary>
        /// 获得小组下对某实体有查看权限的人员集合
        /// </summary>
        /// <param name="channelId"></param>
        /// <param name="entityFullNme"></param>
        /// <returns></returns>
        public List<Users> GetUsersHasRightWithEntityInChannel(int channelId, WorkflowInstance modelWorkflowInstance)
        {
            List<Users> listUsers = new List<Users>();

            if (channelId == 0)
            {
                return null;
            }

            if (modelWorkflowInstance == null || modelWorkflowInstance.RecordId == 0 || modelWorkflowInstance.Workflow == null
                || modelWorkflowInstance.WorkflowId == 0 || modelWorkflowInstance.TerritoryId == null)
            {
                return null;
            }


            List<Users> listUsersAllInChannel = GetListByChannelId(channelId, false);

            if (listUsersAllInChannel == null)
            {
                return null;
            }

            foreach (Users itemUser in listUsersAllInChannel)
            {
                if (listUsers.Any(m => m.Id == itemUser.Id))
                {
                    continue;
                }

                if (itemUser.IsAdmin)
                {
                    listUsers.Add(itemUser);
                    continue;
                }

                IQueryable<Purview> listPurview = DB.Purview.AsNoTracking().Where
                    (m => m.TerritoryProfilesId == itemUser.TerritoryProfilesId
                    && m.FullName == modelWorkflowInstance.Workflow.WorkflowEntityName && m.CanSelect == true
                    );

                if (listPurview == null || !listPurview.Any())
                {
                    continue;
                }

                if (listPurview.Any(m => m.PurviewType == PurviewType.Owner) && modelWorkflowInstance.CreatedById == itemUser.Id)
                {
                    listUsers.Add(itemUser);
                    continue;
                }

                Territory modelTerritoryOfUser = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == itemUser.TerritoryId && m.Deleted == false);

                if (modelTerritoryOfUser == null)
                {
                    continue;
                }

                if (listPurview.Any(m => m.PurviewType == PurviewType.MyTerritory &&
                    modelWorkflowInstance.TerritoryId >= modelTerritoryOfUser.TerritoryId &&
                    modelWorkflowInstance.TerritoryId <= modelTerritoryOfUser.RangeEnd))
                {
                    listUsers.Add(itemUser);
                    continue;
                }

                var listOtherTerritory = listPurview.Where(m => m.PurviewType == PurviewType.OtherTerritory);

                foreach (var itemOtherTerritory in listOtherTerritory)
                {
                    Territory modelItemTerritory = BusinessHelper.ListTerritory.FirstOrDefault(m => m.TerritoryId == itemOtherTerritory.OtherTerritoryId && m.Deleted == false);
                    if (modelItemTerritory == null)
                    {
                        continue;
                    }
                    if (modelWorkflowInstance.TerritoryId >= modelItemTerritory.TerritoryId && modelWorkflowInstance.TerritoryId <= modelItemTerritory.RangeEnd)
                    {
                        listUsers.Add(itemUser);
                        continue;
                    }
                }
            }

            return listUsers;
        }

        #endregion
    }
}