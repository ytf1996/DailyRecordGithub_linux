using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using MyNetCore.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MyNetCore.Areas.DailyRecord.Controllers
{
    public class WorkDiaryController : DailyRecordBaseWithAuthController
    {
        private BusinessWorkDiary _businessWorkDiary = new BusinessWorkDiary();
        private BusinessUsers _businessUsers = new BusinessUsers();
        private BusinessAbsence _businessAbsence = new BusinessAbsence();

        public IActionResult QueryCurrentUserInfo()
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }
            return Success(data: currentUser);
        }

        public IActionResult QueryUserInfoById(int userId)
        {
            var userInfo = _businessUsers.GetList(null, out int userTotalCount, x => x.Id == userId, null, null, false).FirstOrDefault();

            return Success(data: userInfo);
        }

        /// <summary>
        /// 获取某一年月的工作日志列表 （若无则新增空的默认行项）
        /// </summary>
        /// <param name="begDate">一个月的第一天</param>
        /// <returns></returns>
        public IActionResult List(DateTime begDate)
        {
            begDate = new DateTime(begDate.Year, begDate.Month, 1);

            if (DateTime.Today.AddMonths(3) < begDate) throw new Exception("查询日期最多为当前月往后3个月");
            //if (begDate != new DateTime(begDate.Year, begDate.Month, 1))
            //{
            //    throw new LogicException($"{begDate}不为年月格式，需为每月的第一天");
            //}

            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }

            var beflist = _businessWorkDiary.GetList(null, out int beftotalCount, x => x.Dt >= begDate && x.Dt <= begDate.AddMonths(1).AddDays(-1) && x.CreatedById == currentUser.Id, "Dt");

            var befesult = beflist.ToList();

            _businessWorkDiary.AddDefaultItemWhenNotexist(befesult, begDate);//检查并新增空默认行项

            var aftlist = _businessWorkDiary.GetList(null, out int afttotalCount, x => x.Dt >= begDate && x.Dt <= begDate.AddMonths(1).AddDays(-1) && x.CreatedById == currentUser.Id, "Dt");

            var aftresult = aftlist.ToList();

            DiaryShowDto rtnDto = new DiaryShowDto();

            rtnDto.DiaryList = aftresult;
            rtnDto.User = currentUser; //待注释掉
            rtnDto.NormalWorkHourSummary = aftresult.Sum(x => x.NormalWorkHour ?? 0);
            rtnDto.ExtraWorkHourSummary = aftresult.Sum(x => x.ExtraWorkHour ?? 0);
            rtnDto.SubtotalWorkHourSummary = Math.Round((((rtnDto.NormalWorkHourSummary ?? 0) + (rtnDto.ExtraWorkHourSummary ?? 0)) / 8), 2);
            rtnDto.ChargeDayNum = aftresult.Where(x => x.SubtotalWorkHour != null && x.SubtotalWorkHour != 0).Count();
            rtnDto.BusinessTripDayNum = aftresult.Where(x => x.SubtotalWorkHour != null && x.SubtotalWorkHour != 0 && (x.WhetherOnBusinessTrip ?? false)).Count();

            return Success(data: rtnDto);
        }



        public IActionResult SummaryCount(DateTime begDate)
        {
            begDate = new DateTime(begDate.Year, begDate.Month, 1);

            if (DateTime.Today.AddMonths(3) < begDate) throw new Exception("查询日期最多为当前月往后3个月");
            //if (begDate != new DateTime(begDate.Year, begDate.Month, 1))
            //{
            //    throw new LogicException($"{begDate}不为年月格式，需为每月的第一天");
            //}

            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }
            if (!currentUser.IsAdmin)
            {
                throw new Exception("您无此操作权限");
            }

            var userExpList = _businessUsers.GetList(null, out int userTotalCount, x => /*x.IfLeave == 1 &&*/ !x.Disabled, "UserOrder", null, false).ToList();
            var allWorkDiaryList = _businessWorkDiary.GetList(null, out int totalDiaryCount, x => !string.IsNullOrWhiteSpace(x.JobContent) && x.Dt >= begDate && x.Dt <= begDate.AddMonths(1).AddDays(-1), "Dt").ToList();
            List<DiarySummaryDto> list = new List<DiarySummaryDto>();

            foreach (var user in userExpList)
            {
                if (user.IfLeave == 0 && !allWorkDiaryList.Exists(x => x.CreatedById == user.Id))
                {
                    continue;  //如果离职了且无该月日报记录，则跳过
                }

                //var beflist = _businessWorkDiary.GetList(null, out int beftotalCount, x => x.Dt >= begDate && x.Dt <= begDate.AddMonths(1).AddDays(-1) && x.CreatedById == user.Id, "Dt");

                //var befesult = beflist.ToList();

                //_businessWorkDiary.AddDefaultItemWhenNotexist(befesult, begDate);//检查并新增空默认行项

                var aftlist = _businessWorkDiary.GetList(null, out int afttotalCount, x => x.Dt >= begDate && x.Dt <= begDate.AddMonths(1).AddDays(-1) && x.CreatedById == user.Id, "Dt");
                var aftresult = aftlist.ToList();

                //以统计2月份的请假来算
                //请假开始时间<3.1 0:0   && 请假结束时间>2.1 0:0      (则算入当前月份请假部分)    【跨月的两边都显示即可】
                var absencelist = _businessAbsence.GetList(null, out int totalCount, x => !(x.BegAbsenceTime >= begDate.AddMonths(1) || x.EndAbsenceTime <= begDate) && x.CreatedById == user.Id);
                var absenceResult = absencelist.ToList();

                list.Add(new DiarySummaryDto
                {
                    Id = user.Id,
                    UserOrder = user.UserOrder,
                    UserName = user.Name,
                    NormalWorkHourSummary = Math.Round(aftresult.Sum(x => x.NormalWorkHour ?? 0) / 8, 2),        //以人天计
                    ExtraWorkHourSummary = Math.Round(aftresult.Sum(x => x.ExtraWorkHour ?? 0) / 8, 2),
                    SubtotalWorkHourSummary = Math.Round(aftresult.Sum(x => x.NormalWorkHour ?? 0) / 8 + aftresult.Sum(x => x.ExtraWorkHour ?? 0) / 8, 2),
                    SubtotalAbsenceDaySummary = Math.Round(absenceResult.Sum(x => x.AbsenceHours) / 8, 2),
                });
            }

            return Success(data: list);
        }



        /// <summary>
        /// 预览月份+人员  日报信息       (先执行 SummaryCount 方法，后执行本方法)
        /// </summary>
        /// <param name="begDate"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public IActionResult PreviewDiary(DateTime begDate, int userId)
        {
            begDate = new DateTime(begDate.Year, begDate.Month, 1);

            if (DateTime.Today.AddMonths(3) < begDate) throw new Exception("查询日期最多为当前月往后3个月");
            //if (begDate != new DateTime(begDate.Year, begDate.Month, 1))
            //{
            //    throw new LogicException($"{begDate}不为年月格式，需为每月的第一天");
            //}

            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }
            if (!currentUser.IsAdmin)
            {
                throw new Exception("您无此操作权限");
            }
            //var beflist = _businessWorkDiary.GetList(null, out int beftotalCount, x => x.Dt >= begDate && x.Dt <= begDate.AddMonths(1).AddDays(-1) && x.CreatedById == userId, "Dt");

            //var befesult = beflist.ToList();

            //_businessWorkDiary.AddDefaultItemWhenNotexist(befesult, begDate);//检查并新增空默认行项

            var aftlist = _businessWorkDiary.GetList(null, out int afttotalCount, x => x.Dt >= begDate && x.Dt <= begDate.AddMonths(1).AddDays(-1) && x.CreatedById == userId, "Dt");

            var aftresult = aftlist.ToList();

            DiaryShowDto rtnDto = new DiaryShowDto();

            rtnDto.DiaryList = aftresult;
            rtnDto.User = currentUser; //待注释掉
            rtnDto.NormalWorkHourSummary = aftresult.Sum(x => x.NormalWorkHour ?? 0);
            rtnDto.ExtraWorkHourSummary = aftresult.Sum(x => x.ExtraWorkHour ?? 0);
            rtnDto.SubtotalWorkHourSummary = Math.Round((((rtnDto.NormalWorkHourSummary ?? 0) + (rtnDto.ExtraWorkHourSummary ?? 0)) / 8), 2);
            rtnDto.ChargeDayNum = aftresult.Where(x => x.SubtotalWorkHour != null && x.SubtotalWorkHour != 0).Count();
            rtnDto.BusinessTripDayNum = aftresult.Where(x => x.SubtotalWorkHour != null && x.SubtotalWorkHour != 0 && (x.WhetherOnBusinessTrip ?? false)).Count();

            return Success(data: rtnDto);
        }



        /// <summary>
        /// 按公司+年月导出 所有人员的日报 为excel（管理员专用导出功能）
        /// </summary>
        /// <param name="yearMonth">年月</param>
        /// <param name="contractedSupplier">公司</param>
        /// <returns></returns>
        public string ExportDailyReport(DateTime yearMonth, string contractedSupplier)
        {
            yearMonth = new DateTime(yearMonth.Year, yearMonth.Month, 1);

            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }
            if (!currentUser.IsAdmin)
            {
                throw new Exception("您无此操作权限");
            }

            var userExpList = _businessUsers.GetList(null, out int userTotalCount, x => x.ContractedSupplier == contractedSupplier /*&& x.IfLeave == 1*/ && !x.Disabled, "UserOrder", null, false).ToList();
            var userAccounts = userExpList.Select(x => (int?)x.Id).ToList();
            var allWorkDiaryList = _businessWorkDiary.GetList(null, out int totalCount, x => !string.IsNullOrWhiteSpace(x.JobContent) && userAccounts.Contains(x.CreatedById) && x.Dt >= yearMonth && x.Dt <= yearMonth.AddMonths(1).AddDays(-1), "Dt");
            var hasWorkDiaryAcconts = allWorkDiaryList.Select(x => x.CreatedById).Distinct().ToList();
            userExpList = userExpList.Where(x => hasWorkDiaryAcconts.Contains(x.Id)).ToList();
            userAccounts = userExpList.Select(x => (int?)x.Id).ToList();

            var sheetNames = userExpList.Select(x => x.Name).ToList();
            var excelName = contractedSupplier + yearMonth.ToString("yyyyMM") + "日报";
            string path = AppContext.BaseDirectory + @"Excel//";  // @"C:\Users\16273\Desktop\DailyRecord\MyNetCore\";   //return path;

            #region 创建工作簿、克隆sheet页、获取特定单元格位置、赋值单元格日志值
            XSSFWorkbook workbookTemplate;
            XSSFSheet sheetTemplate;
            string templateExcelName = path + "日志导出模板.xlsx";
            Dictionary<string, (int, int)> dic = new Dictionary<string, (int, int)>();
            List<string> speCellNameList_user = new List<string>{
                nameof(Users.ContractedSupplier),
                nameof(Users.Group),
                nameof(Users.ServiceUnit),
                nameof(Users.Name),
                nameof(Users.CounselorPropertyDes),
            };
            List<string> speCellNameList_diary = new List<string>{
                nameof(WorkDiaryInfo.DtExport),
                nameof(WorkDiaryInfo.WhatDayDes),
                nameof(WorkDiaryInfo.WhetherOnBusinessTripExport),
                nameof(WorkDiaryInfo.TravelSite),
                nameof(WorkDiaryInfo.JobClassificationInfoIdExport),
                nameof(WorkDiaryInfo.JobContent),
                nameof(WorkDiaryInfo.BegWorkTimeExport),
                nameof(WorkDiaryInfo.EndWorkTimeExport),
                nameof(WorkDiaryInfo.NormalWorkHour),
                nameof(WorkDiaryInfo.ExtraWorkHour),
                nameof(WorkDiaryInfo.SubtotalWorkHour),
                nameof(WorkDiaryInfo.IsCharged),
                nameof(WorkDiaryInfo.RemarkContent)
            };
            List<string> speCellNameList_summary = new List<string>{
                "yyyyMM",
                "ChargeDayNum",
                "BisTripDayNum"
            };
            using (FileStream stream = new FileStream(templateExcelName, FileMode.Open, FileAccess.Read))
            {
                workbookTemplate = new XSSFWorkbook(stream);
                sheetTemplate = workbookTemplate.GetSheet("XXX") as XSSFSheet;
                //遍历模板excel获得行号列号
                for (var i = 0; i <= sheetTemplate.LastRowNum; i++)  //46行   LastRowNum=45
                {
                    XSSFRow row = (XSSFRow)sheetTemplate.GetRow(i);
                    for (int j = 0; j < (row.LastCellNum); j++) //12列    LastCellNum=12
                    {
                        XSSFCell cell = (XSSFCell)row.GetCell(j);

                        if (speCellNameList_user.Contains(cell.ToString()) || speCellNameList_diary.Contains(cell.ToString()) || speCellNameList_summary.Contains(cell.ToString()))
                        {
                            dic.Add(cell.ToString(), (i, j));
                        }
                    }
                }
            }

            XSSFWorkbook downLoadWorkBook = new XSSFWorkbook();
            foreach (var sheetName in sheetNames)
            {
                sheetTemplate.CopyTo(downLoadWorkBook, sheetName, true, true);
            }
            workbookTemplate.Close();

            var fileName= excelName + ".xlsx";
            var exportExcelName = path + fileName;
            PropertyInfo[] propertyInfos_user = typeof(Users).GetProperties();
            PropertyInfo[] propertyInfos_diary = typeof(WorkDiaryInfo).GetProperties();
            WorkDiaryInfo.projectList = new BusinessJobClassification().GetList(null, out _, null).ToList();
            using (FileStream stream = new FileStream(exportExcelName, FileMode.Create, FileAccess.Write))  // If the file already exists, it will be overwritten. 
            {
                IFont contentFont = downLoadWorkBook.CreateFont();
                contentFont.FontHeightInPoints = 8;
                contentFont.FontName = "微软雅黑";
                float contentCellWidth = GetTextWidth(contentFont, "测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测测");

                foreach (var userId in userAccounts)  //对每个用户循环处理
                {
                    var userInfo = userExpList.Where(x => x.Id == userId).FirstOrDefault();
                    XSSFSheet curUserSheet = (XSSFSheet)downLoadWorkBook.GetSheet(userInfo.Name); //获取该用户对应的sheet页

                    speCellNameList_user.ForEach(cellName =>
                    {
                        ICell cell = curUserSheet.GetRow(dic[cellName].Item1).GetCell(dic[cellName].Item2);

                        var propertyInfo = propertyInfos_user.Where(y => y.Name == cellName).FirstOrDefault();

                        cell.SetCellValue(propertyInfo.GetValue(userInfo)?.ToString());
                    });

                    var curUserWorkDiaryList = allWorkDiaryList.Where(x => x.CreatedById == userId).OrderBy(x => x.Dt).ToList();
                    speCellNameList_diary.ForEach(cellName =>
                    {
                        var propertyInfo = propertyInfos_diary.Where(y => y.Name == cellName).FirstOrDefault();
                        var x = dic[cellName].Item1; var y = dic[cellName].Item2;

                        curUserWorkDiaryList.ForEach(userDiary =>
                        {
                            var ICurRow = curUserSheet.GetRow(x++);

                            ICell cell = ICurRow.GetCell(y);

                            if (cellName == nameof(WorkDiaryInfo.NormalWorkHour) || cellName == nameof(WorkDiaryInfo.ExtraWorkHour) || cellName == nameof(WorkDiaryInfo.SubtotalWorkHour))
                            {
                                var val = propertyInfo.GetValue(userDiary);
                                if (val != null)
                                {
                                    cell.SetCellValue((double)val);
                                }
                                else
                                {
                                    cell.SetCellValue((string)null);
                                }
                            }
                            else if (cellName == nameof(WorkDiaryInfo.JobClassificationInfoIdExport))
                            {
                                cell.SetCellValue((string)null);
                            }
                            else if (cellName == nameof(WorkDiaryInfo.JobContent))
                            {
                                cell.SetCellValue(propertyInfo.GetValue(userDiary)?.ToString());

                                if (cell.CellType == CellType.String && !string.IsNullOrWhiteSpace(cell.StringCellValue))
                                {
                                    var list = cell.StringCellValue.Split('\n', '\r').ToList();
                                    int lineNum = 0;
                                    list.ForEach(item =>
                                    {
                                        float widthTmp = GetTextWidth(contentFont, item);
                                        lineNum += Convert.ToInt32(Math.Ceiling((double)(widthTmp / contentCellWidth)));
                                    });

                                    var height1 = GetTextCommonHeight(contentFont, "测");
                                    //var height2 = GetTextCommonHeight(contentFont, "测\n测");
                                    //var height3 = GetTextCommonHeight(contentFont, "测\n测\n测");
                                    //var height4 = GetTextCommonHeight(contentFont, "测\n测\n测\n测");

                                    var innerGap = height1 / 15 * 1;
                                    var borderGap = height1 / 15 * 1.5f;
                                    var textSize = height1 / 15 * 12.5f;

                                    ICurRow.HeightInPoints = 2 * borderGap + lineNum * textSize + (lineNum - 1) * innerGap + 10;

                                    //if (lineNum == 1)
                                    //{
                                    //    ICurRow.HeightInPoints += 9;
                                    //}
                                    //else if (lineNum == 2)
                                    //{
                                    //    ICurRow.HeightInPoints += 4;
                                    //}
                                    //else if (lineNum >= 3)
                                    //{
                                    //    ICurRow.HeightInPoints -= (lineNum - 1);
                                    //}
                                    //else if (lineNum >= 6)
                                    //{
                                    //    ICurRow.HeightInPoints -= 3 * (lineNum - 2);
                                    //}
                                    //else if (lineNum >= 9)
                                    //{
                                    //    ICurRow.HeightInPoints -= 9 * (lineNum - 2);
                                    //}
                                }
                            }
                            else
                            {
                                cell.SetCellValue(propertyInfo.GetValue(userDiary)?.ToString());
                            }
                        });
                    });

                    speCellNameList_summary.ForEach(cellName =>
                    {
                        ICell cell = curUserSheet.GetRow(dic[cellName].Item1).GetCell(dic[cellName].Item2);

                        switch (cellName)
                        {
                            case "yyyyMM":
                                string valyyyyMM = yearMonth.ToString("yyyy/MM");
                                cell.SetCellValue(valyyyyMM);
                                break;
                            //case "ChargeDayNum":
                            //    double valChargeDayNum = curUserWorkDiaryList.Sum(x => x.SubtotalWorkHour ?? 0);
                            //    cell.SetCellValue(valChargeDayNum);
                            //    break;
                            //case "BisTripDayNum":
                            //    double valBisTripDayNum = curUserWorkDiaryList.Where(x => x.SubtotalWorkHour != null && x.SubtotalWorkHour != 0 && (x.WhetherOnBusinessTrip ?? false)).Count();
                            //    cell.SetCellValue(valBisTripDayNum);
                            //    break;

                            case "ChargeDayNum":
                            case "BisTripDayNum":
                                double valChargeDayNum = curUserWorkDiaryList.Sum(x => x.SubtotalWorkHour ?? 0);
                                cell.SetCellValue(valChargeDayNum);
                                break;
                            default:
                                break;
                        }
                    });
                }
                downLoadWorkBook.Write(stream);
            }
            downLoadWorkBook.Close();
            #endregion

            return fileName;
        }

        public static float GetTextWidth(IFont font, string text)
        {
            using (var bitmap = new Bitmap(1, 1))
            {
                var graphics = Graphics.FromImage(bitmap);
                var size1 = graphics.MeasureString(text, new Font(font.FontName, (float)font.FontHeightInPoints));

                return size1.Width;
            }
        }

        public static float GetTextCommonHeight(IFont font, string text)
        {
            using (var bitmap = new Bitmap(1, 1))
            {
                var graphics = Graphics.FromImage(bitmap);
                var size1 = graphics.MeasureString(text, new Font(font.FontName, (float)font.FontHeightInPoints));

                return size1.Height;
            }
        }

        public IActionResult ExportDailyReport_base64string(DateTime yearMonth, string contractedSupplier)
        {
            var exportExcelName = ExportDailyReport(yearMonth, contractedSupplier);

            byte[] excelBuffer;
            using (FileStream stream = new FileStream(exportExcelName, FileMode.Open, FileAccess.Read))
            {
                excelBuffer = new byte[stream.Length];
                stream.Read(excelBuffer, 0, excelBuffer.Length);
            }
            var base64CodeStr = Convert.ToBase64String(excelBuffer);

            return Success(data: base64CodeStr);
        }

        public IActionResult ExportDailyReport_url(DateTime yearMonth, string contractedSupplier)
        {
            //var exportExcelName = ExportDailyReport(yearMonth, contractedSupplier);
            //var subPath = exportExcelName.Substring(AppContext.BaseDirectory.Length);
            //var excelIpPort = "http://121.5.53.146:6453/";
            ////var excelIpPort = "http://192.168.1.3:8060/";

            //return Success(data: excelIpPort + System.Web.HttpUtility.UrlEncode(subPath.Replace("\\", "/")));

            var exportExcelName = ExportDailyReport(yearMonth, contractedSupplier);
            var excelIpPort = "http://101.34.217.223:2080/";

            return Success(data: excelIpPort + exportExcelName);
        }

        //http://localhost:6789/DailyRecord/WorkDiary/ExportDailyReport_New?yearMonth=2022-02-01&contractedSupplier=%E7%BD%91%E6%96%B0
        //public IActionResult ExportDailyReport_New(DateTime yearMonth, string contractedSupplier)
        //{
        //    yearMonth = new DateTime(yearMonth.Year, yearMonth.Month, 1);

        //    var currentUser = GetCurrentUserInfo();
        //    if (currentUser == null)
        //    {
        //        throw new Exception("用户未登录");
        //    }
        //    if (!currentUser.IsAdmin)
        //    {
        //        throw new Exception("您无此操作权限");
        //    }

        //    var userExpList = _businessUsers.GetList(null, out int userTotalCount, x => x.ContractedSupplier == contractedSupplier && x.IfLeave == 1 && !x.Disabled, "UserOrder", null, false).ToList();
        //    var userAccounts = userExpList.Select(x => (int?)x.Id).ToList();
        //    var allWorkDiaryList = _businessWorkDiary.GetList(null, out int totalCount, x => !string.IsNullOrWhiteSpace(x.JobContent) && userAccounts.Contains(x.CreatedById) && x.Dt >= yearMonth && x.Dt <= yearMonth.AddMonths(1).AddDays(-1), "Dt");
        //    var hasWorkDiaryAcconts = allWorkDiaryList.Select(x => x.CreatedById).Distinct().ToList();
        //    userExpList = userExpList.Where(x => hasWorkDiaryAcconts.Contains(x.Id)).ToList();
        //    userAccounts = userExpList.Select(x => (int?)x.Id).ToList();

        //    var sheetNames = userExpList.Select(x => x.Name).ToList();
        //    var excelName = contractedSupplier + yearMonth.ToString("yyyyMM") + "日报";
        //    string path = AppContext.BaseDirectory + @"Excel\";  // @"C:\Users\16273\Desktop\DailyRecord\MyNetCore\";   //return path;

        //    #region 创建工作簿、克隆sheet页、获取特定单元格位置、赋值单元格日志值
        //    XSSFWorkbook workbookTemplate;
        //    XSSFSheet sheetTemplate;
        //    string templateExcelName = path + "日志导出模板.xlsx";
        //    Dictionary<string, (int, int)> dic = new Dictionary<string, (int, int)>();
        //    List<string> speCellNameList_user = new List<string>{
        //        nameof(Users.ContractedSupplier),
        //        nameof(Users.Group),
        //        nameof(Users.ServiceUnit),
        //        nameof(Users.Name),
        //        nameof(Users.CounselorPropertyDes),
        //    };
        //    List<string> speCellNameList_diary = new List<string>{
        //        nameof(WorkDiaryInfo.DtExport),
        //        nameof(WorkDiaryInfo.WhatDayDes),
        //        nameof(WorkDiaryInfo.WhetherOnBusinessTripExport),
        //        nameof(WorkDiaryInfo.TravelSite),
        //        nameof(WorkDiaryInfo.JobClassificationInfoIdExport),
        //        nameof(WorkDiaryInfo.JobContent),
        //        nameof(WorkDiaryInfo.BegWorkTimeExport),
        //        nameof(WorkDiaryInfo.EndWorkTimeExport),
        //        nameof(WorkDiaryInfo.NormalWorkHour),
        //        nameof(WorkDiaryInfo.ExtraWorkHour),
        //        nameof(WorkDiaryInfo.SubtotalWorkHour),
        //        nameof(WorkDiaryInfo.IsCharged),
        //        nameof(WorkDiaryInfo.RemarkContent)
        //    };
        //    List<string> speCellNameList_summary = new List<string>{
        //        "yyyyMM",
        //        "ChargeDayNum",
        //        "BisTripDayNum"
        //    };
        //    using (FileStream stream = new FileStream(templateExcelName, FileMode.Open, FileAccess.Read))
        //    {
        //        workbookTemplate = new XSSFWorkbook(stream);
        //        sheetTemplate = workbookTemplate.GetSheet("XXX") as XSSFSheet;
        //        //遍历模板excel获得行号列号
        //        for (var i = 0; i <= sheetTemplate.LastRowNum; i++)  //46行   LastRowNum=45
        //        {
        //            XSSFRow row = (XSSFRow)sheetTemplate.GetRow(i);
        //            for (int j = 0; j < (row.LastCellNum); j++) //12列    LastCellNum=12
        //            {
        //                XSSFCell cell = (XSSFCell)row.GetCell(j);

        //                if (speCellNameList_user.Contains(cell.ToString()) || speCellNameList_diary.Contains(cell.ToString()) || speCellNameList_summary.Contains(cell.ToString()))
        //                {
        //                    dic.Add(cell.ToString(), (i, j));
        //                }
        //            }
        //        }
        //    }

        //    XSSFWorkbook downLoadWorkBook = new XSSFWorkbook();
        //    foreach (var sheetName in sheetNames)
        //    {
        //        sheetTemplate.CopyTo(downLoadWorkBook, sheetName, true, true);
        //    }
        //    workbookTemplate.Close();

        //    var exportExcelName = path + excelName + ".xlsx";
        //    PropertyInfo[] propertyInfos_user = typeof(Users).GetProperties();
        //    PropertyInfo[] propertyInfos_diary = typeof(WorkDiaryInfo).GetProperties();
        //    WorkDiaryInfo.projectList = new BusinessJobClassification().GetList(null, out _, null).ToList();
        //    using (FileStream stream = new FileStream(exportExcelName, FileMode.Create, FileAccess.Write))  // If the file already exists, it will be overwritten. 
        //    {
        //        foreach (var userId in userAccounts)  //对每个用户循环处理
        //        {
        //            var userInfo = userExpList.Where(x => x.Id == userId).FirstOrDefault();
        //            XSSFSheet curUserSheet = (XSSFSheet)downLoadWorkBook.GetSheet(userInfo.Name); //获取该用户对应的sheet页

        //            speCellNameList_user.ForEach(cellName =>
        //            {
        //                ICell cell = curUserSheet.GetRow(dic[cellName].Item1).GetCell(dic[cellName].Item2);

        //                var propertyInfo = propertyInfos_user.Where(y => y.Name == cellName).FirstOrDefault();

        //                cell.SetCellValue(propertyInfo.GetValue(userInfo)?.ToString());
        //            });

        //            var curUserWorkDiaryList = allWorkDiaryList.Where(x => x.CreatedById == userId).OrderBy(x => x.Dt).ToList();
        //            speCellNameList_diary.ForEach(cellName =>
        //            {
        //                var propertyInfo = propertyInfos_diary.Where(y => y.Name == cellName).FirstOrDefault();
        //                var x = dic[cellName].Item1; var y = dic[cellName].Item2;

        //                curUserWorkDiaryList.ForEach(userDiary =>
        //                {
        //                    ICell cell = curUserSheet.GetRow(x++).GetCell(y);

        //                    if (cellName == nameof(WorkDiaryInfo.NormalWorkHour) || cellName == nameof(WorkDiaryInfo.ExtraWorkHour) || cellName == nameof(WorkDiaryInfo.SubtotalWorkHour))
        //                    {
        //                        var val = propertyInfo.GetValue(userDiary);
        //                        if (val != null)
        //                        {
        //                            cell.SetCellValue((double)val);
        //                        }
        //                        else
        //                        {
        //                            cell.SetCellValue((string)null);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        cell.SetCellValue(propertyInfo.GetValue(userDiary)?.ToString());
        //                    }
        //                });
        //            });

        //            speCellNameList_summary.ForEach(cellName =>
        //            {
        //                ICell cell = curUserSheet.GetRow(dic[cellName].Item1).GetCell(dic[cellName].Item2);

        //                switch (cellName)
        //                {
        //                    case "yyyyMM":
        //                        string valyyyyMM = yearMonth.ToString("yyyy/MM");
        //                        cell.SetCellValue(valyyyyMM);
        //                        break;
        //                    case "ChargeDayNum":
        //                        double valChargeDayNum = curUserWorkDiaryList.Sum(x => x.SubtotalWorkHour ?? 0);
        //                        cell.SetCellValue(valChargeDayNum);
        //                        break;
        //                    case "BisTripDayNum":
        //                        double valBisTripDayNum = curUserWorkDiaryList.Where(x => x.SubtotalWorkHour != null && x.SubtotalWorkHour != 0 && (x.WhetherOnBusinessTrip ?? false)).Count();
        //                        cell.SetCellValue(valBisTripDayNum);
        //                        break;
        //                    default:
        //                        break;
        //                }
        //            });
        //        }
        //        downLoadWorkBook.Write(stream);
        //    }
        //    downLoadWorkBook.Close();
        //    #endregion

        //    FileStream sr = new FileStream(exportExcelName, FileMode.Open);

        //    if (sr == null)
        //        return NotFound();

        //    return File(sr, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName + ".xlsx");
        //}



        /// <summary>
        /// 获取本机IP
        /// </summary>
        /// <returns></returns>
        //public virtual List<string> GetIps()
        //{
        //    var ips = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
        //    .Select(p => p.GetIPProperties())
        //    .SelectMany(p => p.UnicastAddresses)
        //    .Where(p => p.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(p.Address))
        //    .ToList();

        //    List<string> ipValues = ips.Select(x => x.Address.ToString()).ToList();

        //    return ipValues;
        //}



        /// <summary>
        /// 日期星期不允许人工改动
        /// </summary>
        /// <param name="workDiaryInfo"></param>
        private void EditInner(WorkDiaryInfo workDiaryInfo, Users currentUser)
        {
            var workDiaryInfoDB = _businessWorkDiary.GetById(workDiaryInfo.Id);
            if (workDiaryInfoDB == null)
            {
                throw new LogicException($"不存在主键id为{workDiaryInfo.Id}的日志周报明细记录");
            }

            if (!currentUser.IsAdmin && workDiaryInfoDB.CreatedById != currentUser.Id)
            {
                throw new Exception("非管理员没有权限修改他人的记录");
            }

            if (workDiaryInfoDB.Dt < DateTime.Today)
            {
                if (string.IsNullOrWhiteSpace(workDiaryInfo.JobContent))
                {
                    if (workDiaryInfo.NormalWorkHour != null || workDiaryInfo.ExtraWorkHour != null || workDiaryInfo.BegWorkTime != null || workDiaryInfo.EndWorkTime != null)
                    {
                        throw new Exception("上下班时间、时长、工作内容  需要同时填写或不填写");
                    }
                }
                else
                {
                    if ((workDiaryInfo.NormalWorkHour == null && workDiaryInfo.ExtraWorkHour == null) || workDiaryInfo.BegWorkTime == null || workDiaryInfo.EndWorkTime == null)
                    {
                        throw new Exception("上下班时间、时长、工作内容  需要同时填写或不填写");
                    }
                }
            }

            workDiaryInfoDB.WhetherOnBusinessTrip = workDiaryInfo.WhetherOnBusinessTrip;
            workDiaryInfoDB.TravelSite = workDiaryInfo.TravelSite;
            workDiaryInfoDB.JobClassificationInfoId = workDiaryInfo.JobClassificationInfoId;
            workDiaryInfoDB.JobContent = workDiaryInfo.JobContent;
            workDiaryInfoDB.BegWorkTime = workDiaryInfo.BegWorkTime;
            workDiaryInfoDB.EndWorkTime = workDiaryInfo.EndWorkTime;
            workDiaryInfoDB.NormalWorkHour = workDiaryInfo.NormalWorkHour;
            workDiaryInfoDB.ExtraWorkHour = workDiaryInfo.ExtraWorkHour;
            workDiaryInfoDB.SubtotalWorkHour = Math.Round((((workDiaryInfoDB.NormalWorkHour ?? 0) + (workDiaryInfoDB.ExtraWorkHour ?? 0)) / 8), 2);
            workDiaryInfoDB.RemarkContent = workDiaryInfo.RemarkContent;

            if ((workDiaryInfoDB.BegWorkTime == null && workDiaryInfoDB.EndWorkTime != null) || (workDiaryInfoDB.BegWorkTime != null && workDiaryInfoDB.EndWorkTime == null))
            {
                throw new LogicException("请填写完整上下班时间或均不填写");
            }
            //if (!(workDiaryInfoDB.Dt <= workDiaryInfoDB.BegWorkTime && workDiaryInfoDB.BegWorkTime < workDiaryInfoDB.EndWorkTime && workDiaryInfoDB.EndWorkTime <= workDiaryInfoDB.Dt.AddDays(1)))
            //{
            //    throw new LogicException("上下班时间应在当天之内，请重新填写");
            //}
            if (workDiaryInfoDB.BegWorkTime != null && workDiaryInfoDB.EndWorkTime != null)
            {
                workDiaryInfoDB.BegWorkTime = workDiaryInfoDB.Dt.Date + workDiaryInfoDB.BegWorkTime?.TimeOfDay;
                workDiaryInfoDB.EndWorkTime = workDiaryInfoDB.Dt.Date + workDiaryInfoDB.EndWorkTime?.TimeOfDay;
            }
            if (workDiaryInfoDB.BegWorkTime != null && workDiaryInfoDB.EndWorkTime != null && (workDiaryInfoDB.BegWorkTime >= workDiaryInfoDB.EndWorkTime))
            {
                throw new LogicException("上班时间必须小于下班时间");
            }
            if ((workDiaryInfoDB.NormalWorkHour ?? 0) + (workDiaryInfoDB.ExtraWorkHour ?? 0) > 24)
            {
                throw new LogicException($"日工作总时长大于24小时，请重新填写");
            }

            _businessWorkDiary.Edit(workDiaryInfoDB);
        }


        /// <summary>
        /// 修改工作日志
        /// </summary>
        /// <param name="workDiaryInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(WorkDiaryInfo workDiaryInfo)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }
            EditInner(workDiaryInfo, currentUser);
            return Success();
        }


        /// <summary>
        /// 批量修改工作日志
        /// </summary>
        /// <param name="workDiaryInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Save(List<WorkDiaryInfo> workDiaryInfoList)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }

            foreach (var workDiaryInfo in workDiaryInfoList)
            {
                EditInner(workDiaryInfo, currentUser);
            }
            return Success();
        }
    }
}
