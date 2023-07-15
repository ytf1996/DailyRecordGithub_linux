using MesMessagePlat;
using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using MyNetCore.Models;
using Roim.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MyNetCore.Areas.DailyRecord.Controllers
{
    public class PlanNextWeekController : DailyRecordBaseWithAuthController
    {
        private BusinessPlanNextWeek _businessPlanNextWeek = new BusinessPlanNextWeek();
        private BusinessProjectClassification _businessProjectClassification = new BusinessProjectClassification();
        private BusinessUsers _businessUsers = new BusinessUsers();

        /// <summary>
        /// 获取自然周开始时间的工作计划安排
        /// </summary>
        /// <returns></returns>
        public IActionResult List(DateTime begDate, DateTime endDate)
        {
            begDate = new DateTime(begDate.Year, begDate.Month, begDate.Day);
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);
            //_businessPlanNextWeek.CheckDate(begDate);
            //_businessPlanNextWeek.CheckDate(endDate);
            if (begDate > endDate)
            {
                throw new Exception($"开始时间{begDate}大于结束时间{endDate}");
            }

            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }

            PlanShowDto rtnDto = new PlanShowDto();

            var projectList = _businessProjectClassification.GetList(null, out int totalCount, null, "Order");
            rtnDto.WeeklyProjects = projectList.Select(x => new WeeklyProject
            {
                ProjectClassificationInfoId = x.Id.ToString(),
                ClassificationName = x.ClassificationName
            }).ToList();

            DataTable table = new DataTable();

            //DataRow firstDr = table.NewRow();
            table.Columns.Add("BegDate", typeof(string));
            //firstDr["BegDate"] = null;
            rtnDto.WeeklyProjects.ForEach(x =>
            {
                table.Columns.Add(x.ProjectClassificationInfoId.ToString(), typeof(string));
                //firstDr[x.ProjectClassificationInfoId.ToString()] = x.ClassificationName;
            });
            //table.Rows.Add(firstDr);

            var dataList = _businessPlanNextWeek.GetList(null, out int beftotalCount, x => x.BegDate >= begDate && x.BegDate <= endDate && x.CreatedById == currentUser.Id).ToList();

            for (var dt = endDate; dt >= begDate; dt = dt.AddDays(-1))
            {
                var dataList_dt = dataList.Where(x => x.BegDate == dt).ToList();
                if (dataList_dt.Count == 0) continue;
                DataRow dr = table.NewRow();
                dr["BegDate"] = dt;
                rtnDto.WeeklyProjects.ForEach(project =>
                {
                    var pPlanNextWeekInfo = dataList_dt.Where(x => x.ProjectClassificationInfoId.ToString() == project.ProjectClassificationInfoId.ToString()).FirstOrDefault();

                    dr[project.ProjectClassificationInfoId.ToString()] = new CellDto { Id = pPlanNextWeekInfo?.Id, JobContent = pPlanNextWeekInfo?.JobContent }.ToJsonString();
                });
                table.Rows.Add(dr);
            }

            rtnDto.WeeklyData = table;

            return Success(data: rtnDto.ToJsonString());
        }

        /// <summary>
        /// 获取自然周开始时间的工作计划安排
        /// </summary>
        /// <returns></returns>
        public IActionResult ListAll(DateTime begDate, DateTime endDate)
        {
            begDate = new DateTime(begDate.Year, begDate.Month, begDate.Day);
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);
            if (begDate > endDate)
            {
                throw new Exception($"开始时间{begDate}大于结束时间{endDate}");
            }

            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }
            if (!currentUser.IsAdmin)
            {
                throw new Exception("您无此操作权限");
            }
            PlanShowDto rtnDto = new PlanShowDto();
            var projectList = _businessProjectClassification.GetList(null, out int totalCount, null, "Order");
            rtnDto.WeeklyProjects = projectList.Select(x => new WeeklyProject
            {
                ProjectClassificationInfoId = x.Id.ToString(),
                ClassificationName = x.ClassificationName
            }).ToList();

            DataTable table = new DataTable();
            table.Columns.Add("userOrder", typeof(string));
            table.Columns.Add("company", typeof(string));
            table.Columns.Add("duty", typeof(string));
            table.Columns.Add("userName", typeof(string));

            rtnDto.WeeklyProjects.ForEach(x =>
            {
                table.Columns.Add(x.ProjectClassificationInfoId.ToString(), typeof(string));
            });

            var userExpList = _businessUsers.GetList(null, out int userTotalCount, x => /*x.IfLeave == 1 &&*/ !x.Disabled, "UserOrder", null, false).ToList();

            var users = userExpList.Select(_ =>
            new
            {
                IfLeave = _.IfLeave,
                UserId = _.Id,
                Company = _.ContractedSupplier,
                Duty = _.Duty,
                UserName = _.Name,
                UserOrder = _.UserOrder
            }
            ).Distinct().OrderBy(_ => _.UserOrder).ToList();

            //本周所有人的数据
            var dataList = _businessPlanNextWeek.GetList(null, out int beftotalCount, x => x.BegDate >= begDate && x.BegDate <= endDate /*&& x.CreatedById == currentUser.Id*/).ToList();

            foreach (var item in users)
            {
                var row = dataList.Where(_ => _.CreatedById == item.UserId).ToList();

                if (item.IfLeave == 0 && row.Count == 0)
                {
                    continue;  //如果离职了且无该周计划，则跳过
                }
                DataRow dr = table.NewRow();
                dr["userOrder"] = item.UserOrder;
                dr["company"] = item.Company;
                dr["duty"] = item.Duty;
                dr["userName"] = item.UserName;
                rtnDto.WeeklyProjects.ForEach(project =>
                {
                    var pPlanNextWeekInfo = row?.Where(x => x.ProjectClassificationInfoId.ToString() == project.ProjectClassificationInfoId.ToString()).FirstOrDefault();
                    dr[project.ProjectClassificationInfoId.ToString()] = new CellDto { Id = pPlanNextWeekInfo?.Id, JobContent = pPlanNextWeekInfo?.JobContent }.ToJsonString();
                });
                table.Rows.Add(dr);

            }

            rtnDto.WeeklyData = table;
            return Success(data: rtnDto.ToJsonString());
        }

        /// <summary>
        /// 获取自然周开始时间的工作计划安排(管理员)
        /// </summary>
        /// <param name="begDate"></param>
        /// <returns></returns>
        public IActionResult List_ShowAll_ForAdministrator(DateTime begDate, DateTime endDate)
        {
            //_businessPlanNextWeek.CheckDate(begDate);

            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }
            if (!currentUser.IsAdmin)
            {
                throw new Exception("您无此操作权限");
            }

            var list = _businessPlanNextWeek.GetList(null, out int totalCount, x => x.BegDate >= begDate && x.BegDate <= endDate);

            var result = list.OrderBy(x => x.CreatedById).ThenBy(x => x.ProjectClassificationInfoId).ToList();

            return Success(data: result);
        }


        /// <summary>
        /// 添加工作计划安排
        /// </summary>
        /// <param name="pPlanNextWeekInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Add(PlanDto planDto)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }

            //_businessPlanNextWeek.CheckDate(planDto.BegDate);

            foreach (var item in planDto.ItemList)
            {
                if (string.IsNullOrEmpty(item.ProjectClassificationInfoId))
                {
                    throw new LogicException("项目分类不能为空");
                }
                //if (string.IsNullOrWhiteSpace(item.JobContent))
                //{
                //    throw new LogicException("工作计划安排内容不能为空");
                //}
                planDto.BegDate = new DateTime(planDto.BegDate.Year, planDto.BegDate.Month, planDto.BegDate.Day);
                _businessPlanNextWeek.CheckRepeat(planDto.BegDate, Convert.ToInt32(item.ProjectClassificationInfoId), currentUser);
                var pPlanNextWeekInfo = new PlanNextWeekInfo
                {
                    BegDate = planDto.BegDate,
                    ProjectClassificationInfoId = Convert.ToInt32(item.ProjectClassificationInfoId),
                    JobContent = item.JobContent
                };

                _businessPlanNextWeek.Add(pPlanNextWeekInfo);
            }
            return Success();
        }


        /// <summary>
        /// 修改工作计划安排
        /// </summary>
        /// <param name="pPlanNextWeekInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(List<PlanNextWeekInfo> pPlanNextWeekInfoList)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }

            foreach (var pPlanNextWeekInfo in pPlanNextWeekInfoList)
            {
                var pPlanNextWeekInfoDB = _businessPlanNextWeek.GetById(pPlanNextWeekInfo.Id);
                if (pPlanNextWeekInfoDB == null)
                {
                    var pPlanNextWeekInfo_add = new PlanNextWeekInfo
                    {
                        BegDate = new DateTime(pPlanNextWeekInfo.BegDate.Year, pPlanNextWeekInfo.BegDate.Month, pPlanNextWeekInfo.BegDate.Day),
                        ProjectClassificationInfoId = Convert.ToInt32(pPlanNextWeekInfo.ProjectClassificationInfoId),
                        JobContent = pPlanNextWeekInfo.JobContent
                    };

                    _businessPlanNextWeek.Add(pPlanNextWeekInfo_add);  //若新增了项目，每次修改时，改为该条执行新增
                    //throw new LogicException($"不存在主键id为{pPlanNextWeekInfo.Id}的下周计划记录");
                }
                else
                {
                    if (!currentUser.IsAdmin && pPlanNextWeekInfoDB.CreatedById != currentUser.Id)
                    {
                        throw new Exception("非管理员没有权限修改他人的记录");
                    }
                    pPlanNextWeekInfo.BegDate = new DateTime(pPlanNextWeekInfo.BegDate.Year, pPlanNextWeekInfo.BegDate.Month, pPlanNextWeekInfo.BegDate.Day);
                    //_businessPlanNextWeek.CheckDate(pPlanNextWeekInfo.BegDate);

                    _businessPlanNextWeek.CheckRepeat(pPlanNextWeekInfo.BegDate, pPlanNextWeekInfo.ProjectClassificationInfoId, currentUser, isUpdate: true, pPlanNextWeekInfo.Id);

                    pPlanNextWeekInfoDB.BegDate = pPlanNextWeekInfo.BegDate;
                    //pPlanNextWeekInfoDB.ProjectClassificationInfoId = pPlanNextWeekInfo.ProjectClassificationInfoId;
                    pPlanNextWeekInfoDB.JobContent = pPlanNextWeekInfo.JobContent;

                    //if (pPlanNextWeekInfoDB.BegDate == DateTime.MinValue)
                    //{
                    //    throw new LogicException("自然周日期不能为空");
                    //}
                    //if (pPlanNextWeekInfoDB.ProjectClassificationInfoId == 0)
                    //{
                    //    throw new LogicException("项目分类不能为空");
                    //}
                    //if (string.IsNullOrWhiteSpace(pPlanNextWeekInfoDB.JobContent))
                    //{
                    //    throw new LogicException("工作计划安排内容不能为空");
                    //}

                    _businessPlanNextWeek.Edit(pPlanNextWeekInfoDB);
                }
            }
            return Success();
        }


        /// <summary>
        /// 删除工作计划安排
        /// </summary>
        /// <param name="pPlanNextWeekInfo"></param>
        /// <returns></returns>
        [HttpDelete]
        public IActionResult Delete(List<int> idList)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }

            foreach (var id in idList)
            {
                var pPlanNextWeekInfoDB = _businessPlanNextWeek.GetById(id);
                if (pPlanNextWeekInfoDB == null)
                {
                    throw new LogicException($"不存在主键id为{id}的下周计划记录");
                }
                if (!currentUser.IsAdmin && pPlanNextWeekInfoDB.CreatedById != currentUser.Id)
                {
                    throw new Exception("非管理员没有权限删除他人的记录");
                }

                _businessPlanNextWeek.Delete(pPlanNextWeekInfoDB);
            }

            return Success();
        }



        //用最少改动，不用管最优
        public string ListExport(DateTime begDate, DateTime endDate)
        {
            begDate = new DateTime(begDate.Year, begDate.Month, begDate.Day);
            endDate = new DateTime(endDate.Year, endDate.Month, endDate.Day);
            if (begDate > endDate)
            {
                throw new Exception($"开始时间{begDate}大于结束时间{endDate}");
            }

            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }
            if (!currentUser.IsAdmin)
            {
                throw new Exception("您无此操作权限");
            }
            //PlanShowDto rtnDto = new PlanShowDto();
            var projectList = _businessProjectClassification.GetList(null, out int totalCount, null, "Order");
            var WeeklyProjects = projectList.Select(x => x.ClassificationName).ToList();

            DataTable table = new DataTable();
            table.Columns.Add("编号", typeof(string));
            table.Columns.Add("公司", typeof(string));
            table.Columns.Add("职责", typeof(string));
            table.Columns.Add("工程师", typeof(string));

            WeeklyProjects.ForEach(x =>
            {
                table.Columns.Add(x, typeof(string)); //直接用描述作为列名
            });

            var userExpList = _businessUsers.GetList(null, out int userTotalCount, x => /*x.IfLeave == 1 &&*/ !x.Disabled, "UserOrder", null, false).ToList();

            var users = userExpList.Select(_ =>
            new
            {
                IfLeave = _.IfLeave,
                UserId = _.Id,
                Company = _.ContractedSupplier,
                Duty = _.Duty,
                UserName = _.Name,
                UserOrder = _.UserOrder
            }
            ).Distinct().OrderBy(_ => _.UserOrder).ToList();

            //本周所有人的数据
            var dataList = _businessPlanNextWeek.GetList(null, out int beftotalCount, x => x.BegDate >= begDate && x.BegDate <= endDate /*&& x.CreatedById == currentUser.Id*/).ToList();

            foreach (var item in users)
            {
                var row = dataList.Where(_ => _.CreatedById == item.UserId).ToList();

                if (item.IfLeave == 0 && row.Count == 0)
                {
                    continue;  //如果离职了且无该周计划，则跳过
                }
                DataRow dr = table.NewRow();
                dr["编号"] = item.UserOrder;
                dr["公司"] = item.Company;
                dr["职责"] = item.Duty;
                dr["工程师"] = item.UserName;
                WeeklyProjects.ForEach(project =>
                {
                    var pPlanNextWeekInfo = row?.Where(x => x.ProjectClassificationInfo.ClassificationName == project).FirstOrDefault();
                    dr[project] = pPlanNextWeekInfo?.JobContent;
                });
                table.Rows.Add(dr);

            }

            string basePath = AppContext.BaseDirectory + @"Excel//";

            string fromPath = basePath + "周报导出模板.xlsx";
            string fileName = begDate.ToString("yyyyMMdd") + "-" + endDate.ToString("yyyyMMdd") + "周报.xlsx";
            var toPath = basePath + fileName;

            new ExcelHelper().WriteExcel_Dy(table, "sheet1", 2, 1, fromPath, toPath);


            return fileName;
        }


        public IActionResult WeekReportListExport_url(DateTime begDate, DateTime endDate)
        {
            //var exportExcelName = ListExport(begDate, endDate);
            //var subPath = exportExcelName.Substring(AppContext.BaseDirectory.Length);
            //var excelIpPort = "http://101.34.217.223:2080/";
            ////var excelIpPort = "http://192.168.1.3:8060/";

            //return Success(data: excelIpPort + System.Web.HttpUtility.UrlEncode(subPath.Replace("\\", "/")));

            var exportExcelName = ListExport(begDate, endDate);
            var excelIpPort = "http://101.34.217.223:2080/";

            return Success(data: excelIpPort + exportExcelName);
        }
    }
}
