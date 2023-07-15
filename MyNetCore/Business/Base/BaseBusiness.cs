using System.Linq;
using MyNetCore.Models;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Roim.Common;

namespace MyNetCore.Business
{
    public class BaseBusiness<T> : CommonBusiness
        where T : BaseModel
    {

        protected T Proxy
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// 是否验证Name字段重复
        /// </summary>
        public virtual bool NeedCheckNameRepeat
        {
            get
            {
                return true;
            }
        }

        public BaseBusiness()
        {

        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="param">bootstrap-table控件的参数</param>
        /// <param name="totalCount">总数</param>
        /// <param name="predicate">查询条件</param>
        /// <param name="orderByExpression">排序字段</param>
        /// <param name="isDESC">是否倒序</param>
        /// <param name="needCheckRight">是否需要验证权限,默认为true</param>
        /// <param name="asNoTracking">默认为true，查询出来的数据不保存在EF的session中，不能用作修改，不会造成session id重复报错</param>
        /// <returns></returns>
        public virtual IQueryable<T> GetList(DataTableParameters param, out int totalCount, Expression<Func<T, bool>> predicate = null,
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
                        predicate = (m => m.Name != null && m.Name.Contains(param.Search));
                    }
                }
            }

            totalCount = 0;

            int skip = (page - 1) * pageSize;

            if (string.IsNullOrWhiteSpace(orderByExpression))
            {
                orderByExpression = "Id";
            }

            IQueryable<T> list = null;

            Expression<Func<T, bool>> predicateTemp = m => m.Deleted == false;

            if (asNoTracking)
            {
                list = DB.Set<T>().Where(predicateTemp).AsNoTracking();
            }
            else
            {
                list = DB.Set<T>().Where(predicateTemp);
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
                        //若没权限，则只能看到区域为Null的数据
                        list = GetNoPurviewData(null);
                    }
                    else
                    {
                        list = GetDataByUser(currentUser, ref predicateTemp, asNoTracking);
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
                var property = typeof(T).GetProperty(orderByExpression);
                var parameter = Expression.Parameter(typeof(T), "p");
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExp = Expression.Lambda(propertyAccess, parameter);
                string methodName = isDESC.HasValue && isDESC.Value ? "OrderByDescending" : "OrderBy";
                MethodCallExpression resultExp =
                    Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(T), property.PropertyType },
                    list.Expression, Expression.Quote(orderByExp));
                list = list.Provider.CreateQuery<T>(resultExp);
                if (pageSize != 0)
                {
                    list = list.Skip(skip).Take(pageSize);
                }
            }

            if (list != null)
            {
                list = list.Include(m => m.CreatedBy);
            }

            return list;
        }

        /// <summary>
        /// 根据用户获取用户可查看的数据
        /// </summary>
        /// <param name="modelUser">用户</param>
        /// <param name="predicateTemp">筛选条件</param>
        /// <param name="listTerritoryAll"></param>
        /// <param name="modelTerrForCurrentUser"></param>
        /// <param name="asNoTracking"></param>
        /// <returns></returns>
        public IQueryable<T> GetDataByUser(Users modelUser, ref Expression<Func<T, bool>> predicateTemp, bool asNoTracking = true)
        {
            IEnumerable<Territory> listTerritoryAll = BusinessHelper.ListTerritory;
            Territory modelTerrForCurrentUser = listTerritoryAll.FirstOrDefault(m => m.TerritoryId == modelUser.TerritoryId && m.Deleted == false);

            if (modelTerrForCurrentUser == null)
            {
                //若没权限，则只能看到区域为Null的数据
                return GetNoPurviewData(modelTerrForCurrentUser);
            }

            IQueryable<T> list;
            if (predicateTemp == null)
            {
                predicateTemp = m => m.Deleted == false;
            }
            //根据登录人权限筛选数据
            #region 根据登录人权限筛选数据

            List<Purview> listPurview = DB.Purview.AsNoTracking().Where
                (m => modelUser.TerritoryProfilesId != null && m.TerritoryProfilesId == modelUser.TerritoryProfilesId
                && m.FullName == typeof(T).FullName && m.CanSelect == true
                ).ToList();

            if (listPurview == null || !listPurview.Any() || !modelTerrForCurrentUser.TerritoryId.HasValue)
            {
                //若没权限，则只能看到区域为Null的数据
                list = GetNoPurviewData(modelTerrForCurrentUser);
            }
            else
            {
                var listPurviewMyTerritory = listPurview.Where(m => m.PurviewType == PurviewType.MyTerritory);
                var listPurviewOwener = listPurview.Where(m => m.PurviewType == PurviewType.Owner);
                var listPurviewOtherTerritorys = listPurview.Where(m => m.PurviewType == PurviewType.OtherTerritory);

                Expression<Func<T, bool>> predicateTemp2 = m => false;

                if (listPurviewMyTerritory != null && listPurviewMyTerritory.Any())
                {
                    predicateTemp2 = predicateTemp2.
                        Or(m => m.Deleted == false && m.TerritoryId >= modelTerrForCurrentUser.TerritoryId
                        && m.TerritoryId <= modelTerrForCurrentUser.RangeEnd);
                }

                if (listPurviewOwener != null && listPurviewOwener.Any())
                {
                    predicateTemp2 = predicateTemp2.
                        Or(m => m.Deleted == false && m.CreatedById == modelUser.Id);
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

                predicateTemp2 = predicateTemp2.Or(m => m.Deleted == false && m.TerritoryId == null);

                predicateTemp = predicateTemp.And(predicateTemp2);

                if (asNoTracking)
                {
                    list = DB.Set<T>().AsNoTracking().Where(predicateTemp);
                }
                else
                {
                    list = DB.Set<T>().Where(predicateTemp);
                }

            }
            #endregion
            return list;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="page">页码</param>
        /// <param name="pageSize">每页显示条数</param>
        /// <param name="totalCount">总数</param>
        /// <param name="predicate">筛选条件</param>
        /// <param name="orderByExpression">排序字段</param>
        /// <param name="isDESC">是否倒序</param>
        /// <param name="needCheckRight">是否需要验证权限,默认为true</param>
        /// <returns></returns>
        public IQueryable<T> GetList(int page, int pageSize, out int totalCount, Expression<Func<T, bool>> predicate = null,
            string orderByExpression = null, bool? isDESC = null, bool needCheckRight = true, bool asNoTracking = true)
        {
            DataTableParameters param = new DataTableParameters();
            param.PageNumber = page;
            param.PageSize = pageSize;

            return GetList(param, out totalCount, predicate, orderByExpression, isDESC, needCheckRight, asNoTracking);
        }

        /// <summary>
        /// 根据id集合获取数据集合
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<T> GetListByIds(string ids, bool needCheckRight = true, bool asNoTracking = true)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return null;
            }
            ids = "," + ids + ",";
            return GetListByCondition(m => ids.Contains("," + m.Id + ","), needCheckRight, asNoTracking).ToList();
        }

        /// <summary>
        /// 获取无权限时的数据(区域为空或创建人为自己)
        /// </summary>
        /// <returns></returns>
        private IQueryable<T> GetNoPurviewData(Territory modelTerrForCurrentUser)
        {
            if (modelTerrForCurrentUser == null || !modelTerrForCurrentUser.TerritoryId.HasValue)
            {
                return DB.Set<T>().Where(m => m.Id < -10000);
            }
            var currentUser = GetCurrentUserInfo();
            return DB.Set<T>().AsNoTracking().Where(m => m.Deleted == false && m.TerritoryId == null);
        }

        /// <summary>
        /// 获得所有数据(前10000条)
        /// </summary>
        /// <param name="orderBy">排序字段</param>
        /// <param name="isDESC">是否倒序</param>
        /// <param name="length">查询长度(默认10000条)</param>
        /// <param name="needCheckRight">是否需要验证权限,默认为true</param>
        /// <returns></returns>
        public virtual IQueryable<T> GetAllList(string orderBy = "Id", bool? isDESC = true, int length = 0, bool needCheckRight = true, bool asNoTracking = true)
        {
            int totalCount = 0;
            DataTableParameters param = new DataTableParameters();
            param.PageSize = length;
            param.PageNumber = 1;

            return GetList(param, out totalCount, null, orderBy, isDESC, needCheckRight, asNoTracking);
        }

        /// <summary>
        /// 根据条件获取第一个值
        /// </summary>
        /// <param name="predicate">查询条件</param>
        /// <param name="needCheckRight">是否需要验证权限,默认为true</param>
        /// <returns></returns>
        public T GetByCondition(Expression<Func<T, bool>> predicate, bool needCheckRight = true, bool asNoTracking = true)
        {
            int total = 0;
            var list = GetList(null, out total, predicate, null, null, needCheckRight, asNoTracking);

            if (list != null && list.Any())
            {
                return list.FirstOrDefault();
            }
            return null;
        }

        public IQueryable<T> GetListByCondition(Expression<Func<T, bool>> predicate, bool needCheckRight = true, bool asNoTracking = true)
        {
            int total = 0;
            return GetList(null, out total, predicate, null, null, needCheckRight, asNoTracking);
        }

        public IQueryable<T> GetListByCondition(Expression<Func<T, bool>> predicate, int page, int length = 10,
            bool needCheckRight = true, bool asNoTracking = true)
        {
            int total = 0;
            DataTableParameters param = new DataTableParameters();
            param.PageSize = length;
            param.PageNumber = page;
            return GetList(param, out total, predicate, null, null, needCheckRight, asNoTracking);
        }


        /// <summary>
        /// 根据ID查询
        /// </summary>
        /// <param name="id"></param>
        /// <param name="needCheckRight">是否需要验证权限,默认为true</param>
        /// <returns></returns>
        public T GetById(int? id, bool needCheckRight = true, bool asNoTracking = true)
        {
            if (!id.HasValue)
            {
                return null;
            }
            int totalCount = 0;
            DataTableParameters param = new DataTableParameters();
            param.PageNumber = 1;
            param.PageSize = 1;
            var list = GetList(param, out totalCount, x => x.Id == id, "Id", true, needCheckRight, asNoTracking);
            if (list != null && list.Any())
            {
                var model = list.FirstOrDefault();
                return model;
            }
            return null;
        }

        public virtual void Add(T model, bool needCheckRight = true, bool saveToDBNow = true, MySqlContext db = null)
        {
            string errorMsg = string.Empty;

            if (db == null)
            {
                db = DB;
            }

            CheckAddRight(model, needCheckRight);

            if (NeedCheckNameRepeat)
            {
                var checkRepeatModel = GetByCondition(m => m.Name == model.Name.Trim(), needCheckRight);
                if (checkRepeatModel != null)
                {
                    ThrowErrorInfo(MessageText.ErrorInfo.名称已存在);
                }
            }

            if (!CustomValidForSave(model, out errorMsg))
            {
                ThrowErrorInfo(errorMsg);
            }

            model.CreatedById = GetCurrentUserId();
            model.CreatedDate = DateTime.Now;
            model.UpdatedDate = model.CreatedDate;
            model.UpdatedById = model.CreatedById;
            if (!model.TerritoryId.HasValue)
            {
                model.TerritoryId = GetCurrentUserInfo()?.TerritoryId;
            }
            db.Entry<T>(model).State = EntityState.Added;
            if (saveToDBNow)
            {
                db.SaveChanges();
            }
        }

        public virtual void Edit(T model, bool needCheckRight = true, MySqlContext db = null)
        {
            string errorMsg = string.Empty;

            if (db == null)
            {
                db = DB;
            }

            var modelDB = GetById(model.Id, false);

            if (modelDB == null)
            {
                ThrowErrorInfo("未在数据库中找到相应数据，修改失败");
            }

            CheckEditRight(modelDB, needCheckRight);
            CheckEditRight(model, needCheckRight);

            modelDB = null;

            if (NeedCheckNameRepeat)
            {
                var checkRepeatModel = GetByCondition(m => m.Name == model.Name && m.Id != model.Id, needCheckRight);
                if (checkRepeatModel != null)
                {
                    ThrowErrorInfo(MessageText.ErrorInfo.名称已存在);
                }
            }

            if (!CustomValidForSave(model, out errorMsg))
            {
                ThrowErrorInfo(errorMsg);
            }

            model.UpdatedById = GetCurrentUserId();
            model.UpdatedDate = DateTime.Now;
            db.Entry(model).State = EntityState.Modified;//解决循环修改数据时报Id已attached的错误
            //db.Update(model);
            db.SaveChanges();
            db.Entry(model).State = EntityState.Detached;
        }

        /// <summary>
        /// 检查是否有修改权限
        /// </summary>
        /// <param name="model"></param>
        /// <param name="needCheckRight"></param>
        protected void CheckEditRight(T model, bool needCheckRight)
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

                    if (modelTerrForCurrentUser == null || currentUser.TerritoryProfilesId == null)
                    {
                        ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                    }

                    IQueryable<Purview> listPurview = DB.Purview.Where
                                    (m => m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                                    && m.FullName == typeof(T).FullName && m.CanUpdate == true
                                    );

                    if (listPurview == null || !listPurview.Any())
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
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="model"></param>
        /// <param name="needCheckRight">是否需要验证权限,默认为true</param>
        public virtual void Delete(T model, bool needCheckRight = true)
        {
            string errorMsg = string.Empty;

            CheckDeleteRight(model, needCheckRight);

            if (!CustomValidForDelete(model, out errorMsg))
            {
                ThrowErrorInfo(errorMsg);
            }

            model.UpdatedById = GetCurrentUserId();
            model.UpdatedDate = DateTime.Now;
            model.Deleted = true;
            DB.Entry(model).State = EntityState.Modified;
            DB.SaveChanges();
            DB.Entry(model).State = EntityState.Detached;
        }

        public virtual void DeleteById(int id, bool needCheckRight = true)
        {
            var model = GetById(id, needCheckRight);
            Delete(model, needCheckRight);
        }

        /// <summary>
        /// 检查是否有删除权限
        /// </summary>
        /// <param name="model"></param>
        /// <param name="needCheckRight"></param>
        protected void CheckDeleteRight(T model, bool needCheckRight)
        {
            if (model == null || model.Id == 0)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未找到要删除的数据);
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

                    if (modelTerrForCurrentUser == null)
                    {
                        ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
                    }

                    IQueryable<Purview> listPurview = DB.Purview.Where
                                    (m => currentUser.TerritoryProfilesId != null && m.TerritoryProfilesId == currentUser.TerritoryProfilesId
                                    && m.FullName == typeof(T).FullName && m.CanDelete == true
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


        }

        /// <summary>
        /// 逻辑删除
        /// </summary>
        /// <param name="ids"></param>
        public virtual void Delete(string ids, bool needCheckRight = true)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
            }

            if (string.IsNullOrWhiteSpace(ids))
            {
                ThrowErrorInfo(MessageText.ErrorInfo.未找到要删除的数据);
            }

            int id = 0;

            T model = null;

            foreach (string item in ids.Split(','))
            {
                if (string.IsNullOrWhiteSpace(item) || !int.TryParse(item, out id))
                {
                    continue;
                }
                model = GetById(id, needCheckRight);

                Delete(model, needCheckRight);
            }

        }

        /// <summary>
        /// 保存数据时的自定义验证
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual bool CustomValidForSave(T model, out string errorMsg)
        {
            errorMsg = string.Empty;
            return true;
        }

        /// <summary>
        /// 删除数据时的自定义验证
        /// </summary>
        /// <param name="model"></param>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public virtual bool CustomValidForDelete(T model, out string errorMsg)
        {
            errorMsg = string.Empty;
            return true;
        }

        protected int GetCurrentUserId()
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser != null)
            {
                return currentUser.Id;
            }
            return 1;
        }

        /// <summary>
        /// 根据workflowInstanceId获得审批人集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<Users> GetShenPiRenByInstanceId(WorkflowInstance modelWorkflowInstance)
        {
            if (modelWorkflowInstance == null)
            {
                return null;
            }

            if (modelWorkflowInstance == null || modelWorkflowInstance.RecordId == 0
                || modelWorkflowInstance.WorkflowId == 0)
            {
                return null;
            }

            Workflow modelWorkflow = null;

            if (modelWorkflowInstance.Workflow == null)
            {
                modelWorkflow = DB.Workflow.FirstOrDefault(m => m.Id == modelWorkflowInstance.WorkflowId);
            }
            else
            {
                modelWorkflow = modelWorkflowInstance.Workflow;
            }

            if (modelWorkflow == null)
            {
                return null;
            }

            BusinessWorkflowButton businessWorkflowButton = new BusinessWorkflowButton();
            var listWorkflowButtons = businessWorkflowButton.GetListByCondition(m => m.LastWorkflowStepId == modelWorkflowInstance.WorkflowStepId, false);
            if (listWorkflowButtons == null || !listWorkflowButtons.Any())
            {
                return null;
            }

            List<Users> listShenPiRen = new List<Users>();

            BusinessUsers businessUsers = new BusinessUsers();
            Users modelUsers = null;


            foreach (WorkflowButton itemWorkflowButton in listWorkflowButtons)
            {
                if (modelWorkflowInstance.CreatedById.HasValue)
                {
                    //申请人可见
                    if (itemWorkflowButton.OnlyViewForCreatedBy)
                    {
                        if (!listShenPiRen.Any(m => m.Id == modelWorkflowInstance.CreatedById))
                        {
                            modelUsers = businessUsers.GetById(modelWorkflowInstance.CreatedById.Value, false);
                            if (modelUsers != null)
                            {
                                listShenPiRen.Add(modelUsers);
                            }
                        }
                    }

                    //上级可见
                    if (itemWorkflowButton.OnlyViewForLineManager)
                    {
                        Users modelUserLineManager = businessUsers.GetShangJiByUserId(modelWorkflowInstance.CreatedById);
                        if (modelUserLineManager != null && !listShenPiRen.Any(m => m.Id == modelUserLineManager.Id))
                        {
                            listShenPiRen.Add(modelUserLineManager);
                        }
                    }

                    //用户可见
                    if (!string.IsNullOrWhiteSpace(itemWorkflowButton.UserIds))
                    {
                        string[] listUserIds = itemWorkflowButton.UserIds.Split(',');
                        int itemUserIdInt = 0;
                        foreach (string itemUserId in listUserIds)
                        {
                            if (string.IsNullOrWhiteSpace(itemUserId))
                            {
                                continue;
                            }
                            if (!int.TryParse(itemUserId, out itemUserIdInt))
                            {
                                continue;
                            }
                            if (listShenPiRen.Any(m => m.Id == itemUserIdInt))
                            {
                                continue;
                            }
                            modelUsers = businessUsers.GetById(itemUserIdInt, false);
                            if (modelUsers != null)
                            {
                                listShenPiRen.Add(modelUsers);
                            }
                        }
                    }

                    //小组可见
                    if (!string.IsNullOrWhiteSpace(itemWorkflowButton.ChannelIds))
                    {
                        string[] listChannelIds = itemWorkflowButton.ChannelIds.Split(',');
                        int itemChannelIdInt = 0;
                        foreach (string itemChannelId in listChannelIds)
                        {
                            if (string.IsNullOrWhiteSpace(itemChannelId))
                            {
                                continue;
                            }
                            if (!int.TryParse(itemChannelId, out itemChannelIdInt))
                            {
                                continue;
                            }

                            List<Users> listUsersInChannelWhichHasRight = businessUsers.GetUsersHasRightWithEntityInChannel(itemChannelIdInt, modelWorkflowInstance);

                            if (listUsersInChannelWhichHasRight == null || !listUsersInChannelWhichHasRight.Any())
                            {
                                continue;
                            }

                            foreach (var item in listUsersInChannelWhichHasRight)
                            {
                                if (listShenPiRen.Any(m => m.Id == item.Id))
                                {
                                    continue;
                                }
                                listShenPiRen.Add(item);
                            }

                        }
                    }

                }

            }

            return listShenPiRen;
        }

        /// <summary>
        /// 根据workflowInstance获得审批人名称集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetShenPiRenNamesByInstance(WorkflowInstance modelWorkflowInstance)
        {
            string shenPiRenNames = string.Empty;

            List<Users> list = GetShenPiRenByInstanceId(modelWorkflowInstance);

            if (list == null || !list.Any())
            {
                return shenPiRenNames;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (i == list.Count - 1)
                {
                    shenPiRenNames += list[i].Name;
                }
                else
                {
                    shenPiRenNames += list[i].Name + ";";
                }
            }

            return shenPiRenNames;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">根据workflowInstanceId获得审批人名称集合</param>
        /// <returns></returns>
        public string GetShenPiRenNamesByInstanceId(int? id)
        {
            string shenPiRenNames = string.Empty;
            if (id == null)
            {
                return shenPiRenNames;
            }
            WorkflowInstance modelWorkflowInstance = DB.WorkflowInstance.FirstOrDefault(m => m.Id == id);
            return GetShenPiRenNamesByInstance(modelWorkflowInstance);
        }

        /// <summary>
        /// 根据workflowInstanceId获得审批人Id集合
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetShenPiRenIdsByInstanceId(WorkflowInstance modelWorkflowInstance)
        {
            string shenPiRenIds = string.Empty;

            List<Users> list = GetShenPiRenByInstanceId(modelWorkflowInstance);

            if (list == null || !list.Any())
            {
                return null;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (i == list.Count - 1)
                {
                    shenPiRenIds += list[i].Id;
                }
                else
                {
                    shenPiRenIds += list[i].Id + ",";
                }
            }

            return shenPiRenIds;
        }
    }


}