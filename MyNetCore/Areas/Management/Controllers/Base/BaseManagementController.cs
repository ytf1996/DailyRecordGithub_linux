using MyNetCore.Business;
using MyNetCore.Models;
using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace MyNetCore.Areas.Management.Controllers
{
    public abstract class BaseManagementController<T, TBusiness> : BaseManagementWithAuth
        where T : BaseModel
        where TBusiness : BaseBusiness<T>, new()
    {
        protected TBusiness business = null;

        public BaseManagementController()
        {
            business = new TBusiness();
        }

        /// <summary>
        /// 主表列表内容
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public abstract IActionResult GetListData(DataTableParameters param);

        /// <summary>
        /// 从表列表内容
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public virtual IActionResult GetListDataForDetail(DataTableParameters param)
        {
            return NullResult();
        }

        public virtual IActionResult List()
        {
            string errorMsg = string.Empty;
            if (!business.CheckHasSelectRight<T>(out errorMsg))
            {
                Response.Redirect(string.Format("/Management/ErrorPage/Show?errorStr={0}", errorMsg));
            }
            return View();
        }

        public virtual IActionResult Add()
        {
            BaseBusiness<T> business = new BaseBusiness<T>();
            string errorMsg = string.Empty;
            if (!business.CheckHasAddRight<T>(out errorMsg))
            {
                Response.Redirect(string.Format("/Management/ErrorPage/Show?errorStr={0}", errorMsg));
            }
            T model = Activator.CreateInstance<T>();
            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Add(T model)
        {
            business.Add(model);
            return Success("添加成功", null, "List");
        }

        public virtual IActionResult Edit()
        {
            int id = 0;
            
            string idStr = Request.Query["id"];

            if (string.IsNullOrWhiteSpace(idStr))
            {
                business.ThrowErrorInfo("参数错误");
            }

            if (!int.TryParse(idStr, out id))
            {
                business.ThrowErrorInfo("参数错误");
            }

            T model = business.GetById(id, false);

            if (model == null)
            {
                model = System.Activator.CreateInstance<T>();
            }

            if (!business.CheckHasRecordRight<T>(model, GetCurrentUserInfo()))
            {
                Response.Redirect(string.Format("/Management/ErrorPage/Show?errorStr={0}", MessageText.ErrorInfo.您无此数据的操作权限.ToString()));
            }

            return View(model);
        }

        [HttpPost]
        public virtual IActionResult Edit(T model)
        {
            if (model.Id == 0)
            {
                business.ThrowErrorInfo("保存失败:未找到对应的数据");
            }

            business.Edit(model);
            return Success("编辑成功", null, "List");
        }

        [HttpPost]
        public virtual IActionResult Delete(T model)
        {
            business.Delete(model);
            return Success("删除成功", null, "List");
        }

        /// <summary>
        /// 根据ID获取model的json格式
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual IActionResult GetById(int id)
        {
            var model = business.GetById(id);
            if (model == null)
            {
                return NullResult();
            }
            return Success("", model);
        }


        public virtual IActionResult GetListForSelect()
        {
            string search = Request.Query["search"];
            string page = Request.Query["page"];
            int pageInt = 0;
            if (!int.TryParse(page, out pageInt))
            {
                pageInt = 1;
            }

            int count = 0;

            Expression<Func<T, bool>> predicate = null;

            if (!string.IsNullOrWhiteSpace(search))
            {
                predicate = m => m.Name.Contains(search);
            }

            var list = business.GetList(pageInt, 10, out count, predicate, "Name");

            var result = from m
                         in list
                         select new
                         {
                             id = m.Id,
                             text = m.Name
                         };
            return Json(new { items = result, total_count = count });
        }

    }
}