using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using MyNetCore.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MyNetCore.Areas.DailyRecord.Controllers
{
    public class AbsenceController : DailyRecordBaseWithAuthController
    {
        private BusinessAbsence _businessAbsence = new BusinessAbsence();
        private BusinessUsers _businessUsers = new BusinessUsers();

        /// <summary>
        /// 获取自然周开始时间的请假信息
        /// </summary>
        /// <returns></returns>
        public IActionResult List(DateTime begDate, DateTime endDate, bool isOnlyNotDealed = true)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }

            if (begDate == DateTime.MinValue || endDate == DateTime.MinValue || begDate >= endDate)
            {
                throw new Exception("请填写正确的查询开始和结束时间");
            }

            var list = _businessAbsence.GetList(null, out int totalCount, x => x.BegAbsenceTime.Date >= begDate.Date && x.EndAbsenceTime.Date <= endDate.Date && x.CreatedById == currentUser.Id);
            if (isOnlyNotDealed)
            {
                list = list.Where(x => x.WhetherApprove == null);
            }

            var result = list.OrderByDescending(x => x.BegAbsenceTime).ToList(); //排序（申请人、批复状态、请假开始时间）

            return Success(data: result);
        }

        /// <summary>
        /// 获取所有请假信息(管理员)      可筛选仅未批复的记录
        /// </summary>
        /// <param name="begDate"></param>
        /// <returns></returns>
        public IActionResult List_ShowAll_ForAdministrator(DateTime begDate, DateTime endDate, bool isOnlyNotDealed = true)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }
            if (!currentUser.IsAdmin)
            {
                throw new Exception("您无此操作权限");
            }

            if (begDate == DateTime.MinValue || endDate == DateTime.MinValue || begDate >= endDate)
            {
                throw new Exception("请填写正确的查询开始和结束时间");
            }

            var list = _businessAbsence.GetList(null, out int totalCount, x => x.BegAbsenceTime.Date >= begDate.Date && x.EndAbsenceTime.Date <= endDate.Date);
            if (isOnlyNotDealed)
            {
                list = list.Where(x => x.WhetherApprove == null);
            }

            var result = list.OrderBy(x => x.CreatedById).ThenBy(x => x.WhetherApprove).ThenByDescending(x => x.BegAbsenceTime).ToList(); //排序（申请人、批复状态、请假开始时间）

            return Success(data: result);
        }


        /// <summary>
        /// 添加请假信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Add(AbsenceInfo input)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }

            if (input.BegAbsenceTime == DateTime.MinValue || input.EndAbsenceTime == DateTime.MinValue || input.BegAbsenceTime >= input.EndAbsenceTime)
            {
                throw new Exception("请填写正确的请假开始和结束时间");
            }
            if (input.AbsenceHours <= 0)  //是否改为自动计算？？？
            {
                throw new Exception("请假时长填写不正确");
            }

            _businessAbsence.CheckRepeat(input.BegAbsenceTime, input.EndAbsenceTime, currentUser);

            var inputForDb = new AbsenceInfo   //保险起见，重新构造
            {
                BegAbsenceTime = input.BegAbsenceTime,
                EndAbsenceTime = input.EndAbsenceTime,
                AbsenceHours = input.AbsenceHours,
                AbsenceReason = input.AbsenceReason,
                AbsenceRemark = input.AbsenceRemark
            };

            _businessAbsence.Add(inputForDb);

            return Success();
        }


        /// <summary>
        /// 修改请假信息
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Edit(AbsenceInfo input)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }

            var absenceInfoDB = _businessAbsence.GetById(input.Id);
            if (absenceInfoDB == null)
            {
                throw new LogicException($"不存在主键id为{input.Id}的请假填写记录");
            }
            if (!currentUser.IsAdmin && absenceInfoDB.CreatedById != currentUser.Id)
            {
                throw new Exception("非管理员没有权限修改他人的记录");
            }

            if (input.BegAbsenceTime == DateTime.MinValue || input.EndAbsenceTime == DateTime.MinValue || input.BegAbsenceTime >= input.EndAbsenceTime)
            {
                throw new Exception("请填写正确的请假开始和结束时间");
            }
            if (input.AbsenceHours <= 0)  //是否改为自动计算？？？
            {
                throw new Exception("请假时长填写不正确");
            }

            _businessAbsence.CheckRepeat(input.BegAbsenceTime, input.EndAbsenceTime, currentUser, isUpdate: true, input.Id);

            absenceInfoDB.BegAbsenceTime = input.BegAbsenceTime;
            absenceInfoDB.EndAbsenceTime = input.EndAbsenceTime;
            absenceInfoDB.AbsenceHours = input.AbsenceHours;
            absenceInfoDB.AbsenceReason = input.AbsenceReason;
            absenceInfoDB.AbsenceRemark = input.AbsenceRemark;

            _businessAbsence.Edit(absenceInfoDB);

            return Success();
        }


        /// <summary>
        /// 删除请假信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }

            var absenceInfoDB = _businessAbsence.GetById(id);
            if (absenceInfoDB == null)
            {
                throw new LogicException($"不存在主键id为{id}的请假填写记录");
            }
            if (!currentUser.IsAdmin && absenceInfoDB.CreatedById != currentUser.Id)
            {
                throw new Exception("非管理员没有权限修改他人的记录");
            }

            _businessAbsence.Delete(absenceInfoDB);

            return Success();
        }



        /// <summary>
        /// 批复请假信息 (可多次改动)
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Reply(AbsenceInfo input)
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                throw new Exception("用户未登录");
            }

            var absenceInfoDB = _businessAbsence.GetById(input.Id);
            if (absenceInfoDB == null)
            {
                throw new LogicException($"不存在主键id为{input.Id}的请假填写记录");
            }
            if (!currentUser.IsAdmin && absenceInfoDB.CreatedById != currentUser.Id)
            {
                throw new Exception("非管理员没有权限批复请假记录");
            }

            if (input.WhetherApprove == null)
            {
                throw new Exception("请填写是否批准");
            }

            absenceInfoDB.WhetherApprove = input.WhetherApprove;
            absenceInfoDB.ReplyRemark = input.ReplyRemark;

            _businessAbsence.Edit(absenceInfoDB);

            return Success();
        }
    }
}
