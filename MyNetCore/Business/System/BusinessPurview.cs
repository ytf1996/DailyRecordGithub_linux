using System;
using System.Collections.Generic;
using System.Linq;
using MyNetCore.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace MyNetCore.Business
{
    public class BusinessPurview : CommonBusiness
    {
        public BusinessPurview()
        {

        }

        /// <summary>
        /// 根据条件查询权限
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IQueryable<Purview> GetList(Expression<Func<Purview, bool>> predicate)
        {
            return DB.Purview.AsNoTracking().Where(predicate);
        }

        public void Update(List<Purview> list,int territoryProfilesId)
        {
            if (list == null || !list.Any())
            {
                return;
            }

            var currentUser = GetCurrentUserInfo();

            if (currentUser == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
            }

            if (!currentUser.IsAdmin)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
            }

            List<Purview> listForDel = new List<Purview>();

            var db = DB;

            listForDel.AddRange(db.Purview.Where(m => m.TerritoryProfilesId == territoryProfilesId
                && !list.Select(x => x.Id).Contains(m.Id)));

            var strategy = db.Database.CreateExecutionStrategy();

            strategy.Execute(() => {
                using (var dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var item in list)
                        {
                            if (item.TerritoryProfilesId == null)
                            {
                                ThrowErrorInfo(MessageText.ErrorInfo.数据提交失败);
                            }

                            if (item.Id == 0)
                            {
                                item.CreatedById = currentUser.Id;
                                item.CreatedDate = DateTime.Now;
                                item.UpdatedById = currentUser.Id;
                                item.UpdatedDate = DateTime.Now;
                                db.Entry<Purview>(item).State = EntityState.Added;
                                db.SaveChanges();
                                db.Entry<Purview>(item).State = EntityState.Detached;
                            }
                            else
                            {
                                item.UpdatedById = currentUser.Id;
                                item.UpdatedDate = DateTime.Now;
                                db.Entry(item).State = EntityState.Modified;
                                db.SaveChanges();
                                db.Entry<Purview>(item).State = EntityState.Detached;
                            }
                        }

                        if (listForDel.Count > 0)
                        {
                            db.Purview.RemoveRange(listForDel);
                            db.SaveChanges();
                        }

                        dbTran.Commit();
                    }
                    catch (Exception ex)
                    {
                        dbTran.Rollback();
                        throw ex;
                    }
                }
            });
        }

    }
}