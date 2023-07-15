using System;
using System.Linq;
using MyNetCore.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace MyNetCore.Business
{
    public class BusinessLog : CommonBusiness
    {
        public BusinessLog()
        {

        }

        public IQueryable<ErrorLog> GetList(DataTableParameters param, out int totalCount, Expression<Func<ErrorLog, bool>> predicate = null,
            string orderByExpression = null, bool? isDESC = null)
        {
            int page = 1;
            int pageSize = 1;

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
                        predicate = (m => m.Message.Contains(param.Search) || m.Level == param.Search);
                    }
                }
            }

            totalCount = 0;

            int skip = (page - 1) * pageSize;

            if (string.IsNullOrWhiteSpace(orderByExpression))
            {
                orderByExpression = "id";
            }
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
            }

            IQueryable<ErrorLog> list = DB.Set<ErrorLog>().AsNoTracking();

            if (currentUser.UserType != UserType.Admin)
            {
                list = list.Where(m => m.Id == 0);
            }

            totalCount = list == null ? 0 : list.Count();

            if (list != null && predicate != null)
            {
                list = list.Where(predicate);
            }

            if (page != 0 && list != null)
            {
                var property = typeof(ErrorLog).GetProperty(orderByExpression);
                var parameter = Expression.Parameter(typeof(ErrorLog), "p");
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExp = Expression.Lambda(propertyAccess, parameter);
                string methodName = isDESC.HasValue && isDESC.Value ? "OrderByDescending" : "OrderBy";
                MethodCallExpression resultExp =
                    Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(ErrorLog), property.PropertyType },
                    list.Expression, Expression.Quote(orderByExp));
                list = list.Provider.CreateQuery<ErrorLog>(resultExp);

                list = list.Skip(skip).Take(pageSize);
            }

            return list;
        }


        public IQueryable<ErrorLog> GetList(int page, int pageSize, out int totalCount, Expression<Func<ErrorLog, bool>> predicate = null,
            string orderByExpression = null, bool? isDESC = null)
        {
            DataTableParameters param = new DataTableParameters();
            param.PageNumber = page;
            param.PageSize = pageSize;

            return GetList(param, out totalCount, predicate, orderByExpression, isDESC);
        }


        /// <summary>
        /// 记录信息
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="exception"></param>
        public void Info(string msg, Exception exception = null)
        {
            ErrorLog errorLog = new ErrorLog()
            {
                Application = "MyNetCore",
                CallSite = "MyNetCore",
                Exception = exception?.StackTrace,
                Level = "Info",
                Logged = DateTime.Now,
                Logger = GetCurrentUserInfo()?.Code,
                Message = msg
            };

            DB.Log.Add(errorLog);
            DB.SaveChanges();
        }

        /// <summary>
        /// 记录警告
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="exception"></param>
        public void Warn(string msg, Exception exception = null)
        {
            ErrorLog errorLog = new ErrorLog()
            {
                Application = "MyNetCore",
                CallSite = "MyNetCore",
                Exception = exception?.StackTrace,
                Level = "Warn",
                Logged = DateTime.Now,
                Logger = GetCurrentUserInfo()?.Code,
                Message = msg
            };

            DB.Log.Add(errorLog);
            DB.SaveChanges();
        }

        /// <summary>
        /// 记录错误
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="exception"></param>
        public void Error(string msg, Exception exception = null)
        {
            ErrorLog errorLog = new ErrorLog()
            {
                Application = "MyNetCore",
                CallSite = "MyNetCore",
                Exception = exception?.StackTrace,
                Level = "Error",
                Logged = DateTime.Now,
                Logger = GetCurrentUserInfo()?.Code,
                Message = msg
            };

            DB.Log.Add(errorLog);
            DB.SaveChanges();
        }

    }
}