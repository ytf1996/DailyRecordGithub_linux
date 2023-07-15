using System;
using System.Collections.Generic;
using System.Linq;
using MyNetCore.Models;
using System.Reflection;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Html;
using Microsoft.EntityFrameworkCore;
using MyNetCore.Tools;

namespace MyNetCore.Business
{
    public class BusinessWorkflow : BaseBusiness<Workflow>
    {

        public override bool CustomValidForSave(Workflow model, out string errorMsg)
        {
            errorMsg = string.Empty;

            if (DB.Workflow.Any(m => m.WorkflowEntityName == model.WorkflowEntityName && m.Id != model.Id && m.Deleted == false))
            {
                errorMsg = "一个实体只能创建一个工作流";
                return false;
            }

            return true;
        }

        public override void Add(Workflow model, bool needCheckRight = true, bool saveToDBNow = true, MySqlContext db = null)
        {
            if (db == null)
            {
                db = DB;
            }

            var strategy = db.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using (var dbTran = db.Database.BeginTransaction())
                {
                    try
                    {
                        base.Add(model, needCheckRight, true, db);
                        WorkflowStep workflowStep = new WorkflowStep();
                        workflowStep.CreatedById = model.CreatedById;
                        workflowStep.CreatedDate = model.CreatedDate;
                        workflowStep.Name = "开始";
                        workflowStep.WorkflowId = model.Id;
                        workflowStep.LevelPath = 1;
                        new BusinessWorkflowStep().Add(workflowStep, true, true, db);
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


        #region 设置工作流页面
        /// <summary>
        /// 根据工作流ID生成工作流设置页面HTML代码
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public HtmlString GetWorkflowHtmlByWorkflowId(int workflowId)
        {
            string htmlStrAll = string.Empty;

            if (workflowId == 0)
            {
                return null;
            }

            //获得第一个状态
            WorkflowStep modelFirstWorkflowStep = new BusinessWorkflowStep().GetByCondition(m => m.WorkflowId == workflowId && m.LevelPath == 1 && m.Deleted == false);

            if (modelFirstWorkflowStep == null)
            {
                return null;
            }

            var listWorkflowButton = new BusinessWorkflowButton().GetListByCondition(m => m.WorkflowId == workflowId && m.Deleted == false).ToList();

            htmlStrAll += "<div class=\"tree\">";
            htmlStrAll += "<ul>";
            htmlStrAll += "<li>";

            htmlStrAll += "<a class=\"workflowStep\" href=\"EditForWorkflowStep?id=" + modelFirstWorkflowStep.Id + "\">"
                + modelFirstWorkflowStep.Name + "</a><br/>";
            htmlStrAll += "<a class=\"workflowAddButton\" href=\"AddForWorkflowButton?workflowStepId=" + modelFirstWorkflowStep.Id + "\"><i class=\"fa fa-plus\"></i></a>";
            htmlStrAll += GetWorflowHtmlForStepByWorkflowButton(modelFirstWorkflowStep, listWorkflowButton);
            htmlStrAll += "</li>";
            htmlStrAll += "</ul>";
            htmlStrAll += "</div>";

            return new HtmlString(htmlStrAll);
        }

        /// <summary>
        /// 根据工作流步骤生成下一级步骤的html
        /// </summary>
        /// <param name="modelFirstWorkflowStep">工作流按钮</param>
        /// <param name="listWorkflowButton">工作流下的所有按钮</param>
        /// <returns></returns>
        private string GetWorflowHtmlForStepByWorkflowButton(WorkflowStep modelFirstWorkflowStep, List<WorkflowButton> listWorkflowButton)
        {
            string htmlStr = string.Empty;

            if (listWorkflowButton == null)
            {
                return string.Empty;
            }

            IEnumerable<WorkflowButton> listCurrentWorkflowButton = listWorkflowButton.Where
                (m => m.LastWorkflowStepId == modelFirstWorkflowStep.Id);

            if (listCurrentWorkflowButton == null || !listCurrentWorkflowButton.Any())
            {
                return string.Empty;
            }
            htmlStr += "<ul>";
            foreach (var item in listCurrentWorkflowButton)
            {

                htmlStr += "<li>";
                htmlStr += "<a class=\"workflowButton\" href=\"EditForWorkflowButton?id=" + item.Id + "\">" + item.Name + "</a><br/>";
                htmlStr += "<a class=\"workflowStep\" href=\"EditForWorkflowStep?id=" + item.NextWorkflowStep.Id + "\">" + item.NextWorkflowStep.Name + "</a><br/>";
                htmlStr += "<a class=\"workflowAddButton\" href=\"AddForWorkflowButton?workflowStepId=" + item.NextWorkflowStep.Id + "\"><i class=\"fa fa-plus\"></i></a>";
                htmlStr += GetWorflowHtmlForStepByWorkflowButton(item.NextWorkflowStep, listWorkflowButton);
                htmlStr += "</li>";
            }
            htmlStr += "</ul>";

            return htmlStr;
        }
        #endregion

        #region 根据实体记录获得下面的按钮集合
        /// <summary>
        /// 根据实体记录获得下面的按钮集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">实体记录ID</param>
        /// <returns></returns>
        public List<WorkflowButton> GetWorkflowButtonsByRecord<T>(int id)
            where T : BaseModel
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
            }
            List<WorkflowButton> listWorkflowButton = null;
            List<WorkflowButton> listWorkflowButtonFinal = new List<WorkflowButton>();
            BaseBusiness<T> baseBusiness = new BaseBusiness<T>();
            T model = baseBusiness.GetById(id, false);
            if (model == null)
            {
                return listWorkflowButtonFinal;
            }
            BusinessWorkflowButton businessWorkflowButton = new BusinessWorkflowButton();
            BusinessWorkflowStep businessWorkflowStep = new BusinessWorkflowStep();

            WorkflowStep modelCurrentWorkflowStep = GetCurrentStepByRecord<T>(model);

            if (modelCurrentWorkflowStep == null)
            {
                return listWorkflowButtonFinal;
            }

            listWorkflowButton = businessWorkflowButton.GetListByCondition(m => m.LastWorkflowStepId == modelCurrentWorkflowStep.Id, false).ToList();

            #region 筛选当前人员可以看到的按钮
            if (listWorkflowButton != null)
            {
                foreach (var item in listWorkflowButton)
                {
                    //未设置任何控制
                    if (!item.OnlyViewForCreatedBy && !item.OnlyViewForLineManager && !item.OnlyViewForLineManager
                        && string.IsNullOrWhiteSpace(item.UserIds) && string.IsNullOrWhiteSpace(item.ChannelIds))
                    {
                        //listWorkflowButtonFinal.Add(item);
                        continue;
                    }

                    if (!string.IsNullOrWhiteSpace(item.CanViewCondition))
                    {
                        try
                        {
                            string runSql = item.CanViewCondition.Replace("@Id", "@id").Replace("@ID", "@id").Replace("@iD", "@id");

                            var p_name = new MySqlParameter("@id", id.ToString());

                            ToolModel toolModel = null;

                            if (runSql.Contains("@id"))
                            {
                                toolModel = DB.ToolModel.FromSqlRaw<ToolModel>(string.Format("select 1 as Id, count(1) as RecordCount from {0}", runSql), p_name).FirstOrDefault();
                            }
                            else
                            {
                                toolModel = DB.ToolModel.FromSqlRaw<ToolModel>(string.Format("select 1 as Id,count(1) as RecordCount from {0}", runSql)).FirstOrDefault();
                            }

                            if (toolModel == null || toolModel.RecordCount == 0)
                            {
                                continue;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    //创建人可见
                    if (item.OnlyViewForCreatedBy && model.CreatedById == currentUser.Id)
                    {
                        listWorkflowButtonFinal.Add(item);
                        continue;
                    }
                    //上级可见
                    if (item.OnlyViewForLineManager && model.CreatedBy.LineManageId.HasValue && model.CreatedBy.LineManageId.Value == currentUser.Id)
                    {
                        listWorkflowButtonFinal.Add(item);
                        continue;
                    }
                    //可见人员
                    if (("," + item.UserIds + ",").Contains("," + currentUser.Id + ","))
                    {
                        listWorkflowButtonFinal.Add(item);
                        continue;
                    }

                    //可见小组

                    string[] currentUserChannels = currentUser.ChannelIds.Split(',');

                    foreach (var itemChannel in currentUserChannels)
                    {
                        if (string.IsNullOrWhiteSpace(itemChannel))
                        {
                            continue;
                        }

                        if (("," + item.ChannelIds + ",").Contains("," + itemChannel + ","))
                        {
                            listWorkflowButtonFinal.Add(item);
                            break;
                        }
                    }


                }
            }
            #endregion

            return listWorkflowButtonFinal;
        }

        /// <summary>
        /// 获得当前工作流状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public WorkflowStep GetCurrentStepByRecord<T>(T model)
            where T : BaseModel
        {
            WorkflowStep modelCurrentWorkflowStep = null;
            BusinessWorkflowStep businessWorkflowStep = new BusinessWorkflowStep();

            if (model == null)
            {
                return null;
            }

            IWorkflowModel wModel = model as IWorkflowModel;

            if (wModel == null)
            {
                return null;
            }

            if (wModel.WorkflowInstanceId.HasValue)
            {
                BusinessWorkflowInstance businessWorkflowInstance = new BusinessWorkflowInstance();

                var workflowInstanceModel = businessWorkflowInstance.GetByCondition(m => m.Id == wModel.WorkflowInstanceId, false);
                if (workflowInstanceModel != null && workflowInstanceModel.WorkflowStepId.HasValue)
                {
                    modelCurrentWorkflowStep = businessWorkflowStep.GetByCondition(m => m.Id == workflowInstanceModel.WorkflowStepId, false);
                }
            }
            else
            {
                string entityFullName = typeof(T).FullName;
                Workflow modelWorkflow = new BusinessWorkflow().GetByCondition(m => m.WorkflowEntityName == entityFullName && m.Deleted == false);
                if (modelWorkflow != null)
                {
                    modelCurrentWorkflowStep = businessWorkflowStep.GetFirstWorkflowStepByWorkflowId(modelWorkflow.Id);
                }
            }

            return modelCurrentWorkflowStep;
        }

        #endregion

        #region 调用工作流
        public void RunWorkflow<TWorkflowModel, TWorkflowProgressModel>(int recordId, int workflowButtonId, string remark)
            where TWorkflowModel : IWorkflowModel
            where TWorkflowProgressModel : IWorkflowProgressModel
        {
            var currentUser = GetCurrentUserInfo();
            if (currentUser == null)
            {
                ThrowErrorInfo(MessageText.ErrorInfo.您无此操作权限);
            }
            //工作流中设置需要通知的用户
            List<Users> listUsersNotice = null;
            //工作流中设置需要通知的小组
            List<Channel> listChannelNotice = null;
            List<int?> listChannelIdsNotice = new List<int?>();
            //需要通知的最终用户集合
            List<Users> listUserNoticeAll = new List<Users>();
            Users modelUsersLineManager = null;
            //通知
            Notice modelNotice = null;

            BusinessWorkflowButton businessWorkflowButton = new BusinessWorkflowButton();
            BusinessWorkflowInstance businessWorkflowInstance = new BusinessWorkflowInstance();

            TWorkflowModel modelTWorkflowModel = DB.Set<TWorkflowModel>().FirstOrDefault(m => m.Deleted == false && m.Id == recordId);
            WorkflowButton modelWorkflowButton = businessWorkflowButton.GetByCondition(m => m.Deleted == false && m.Id == workflowButtonId, false);
            WorkflowInstance modelWorkflowInstance = modelTWorkflowModel.WorkflowInstanceId.HasValue ?
                businessWorkflowInstance.GetByCondition(m => m.Deleted == false && m.Id == modelTWorkflowModel.WorkflowInstanceId.Value, false)
                : null;
            //点击按钮触发的事件
            List<WorkflowAction> listWorkflowAction = DB.WorkflowAction.Where(m => m.WorkflowButtonId == workflowButtonId && m.Deleted == false)
                .OrderBy(m => m.OrderNum).ToList();

            var db = DB;

            var strategy = db.Database.CreateExecutionStrategy();

            strategy.Execute(() =>
            {
                using (var dbTran = db.Database.BeginTransaction())
                {
                    #region 若没有工作流实例,创建一个工作流实例,然后修改当前状态
                    try
                    {
                        if (modelWorkflowInstance == null)
                        {
                            ThrowErrorInfo("本条记录工作流无法启动，请重新创建记录");
                        }
                        else
                        {
                            if (modelWorkflowInstance.WorkflowStepId != modelWorkflowButton.LastWorkflowStepId)
                            {
                                ThrowErrorInfo("工作流状态已发生改变，请刷新后重试");
                            }
                            modelWorkflowInstance.UpdatedById = currentUser.Id;
                            modelWorkflowInstance.UpdatedDate = DateTime.Now;

                            WorkflowStep workflowStep = DB.Set<WorkflowStep>().Where(x => x.WorkflowId == modelWorkflowInstance.WorkflowId && x.Name == modelWorkflowButton.NextWorkflowStep.Name)
                            .OrderBy(x => x.CreatedDate).FirstOrDefault();

                            if (workflowStep == null)
                            {
                                modelWorkflowInstance.WorkflowStepId = modelWorkflowButton.NextWorkflowStepId;
                            }
                            else
                            {
                                modelWorkflowInstance.WorkflowStepId = workflowStep.Id;
                            }

                            db.Entry(modelWorkflowInstance).State = EntityState.Modified;
                            db.SaveChanges();
                            modelWorkflowInstance.DealUsersIds = GetShenPiRenIdsByInstanceId(modelWorkflowInstance);
                            db.SaveChanges();
                        }

                        EmailHelper emailHeper = new EmailHelper();
                        WeChatWorkHelper weChatWorkHelper = new WeChatWorkHelper();

                        Territory modelUserTerritory = null;
                        BusinessTerritory businessTerritory = new BusinessTerritory();

                        BusinessUsers businessUsers = new BusinessUsers();
                        BusinessChannel businessChannel = new BusinessChannel();
                        BusinessNotice businessNotice = new BusinessNotice();

                        #region 实现Action的操作
                        if (listWorkflowAction.Any())
                        {
                            foreach (WorkflowAction itemWorkflowAction in listWorkflowAction)
                            {
                                switch (itemWorkflowAction.WorkflowActionType)
                                {
                                    case WorkflowActionType.EditColumnValue:
                                        PropertyInfo propotyColumn = modelTWorkflowModel.GetType().GetProperty(itemWorkflowAction.EditColumnName);
                                        if (propotyColumn == null)
                                        {
                                            ThrowErrorInfo(string.Format("修改字段({0})失败：无此字段，请联系管理员。", itemWorkflowAction.EditColumnName));
                                        }
                                        try
                                        {

                                            var propertyType = propotyColumn.PropertyType;
                                            string propertyTypeName = propertyType.Name.ToLower();

                                            if (propertyType.BaseType.Name == "Enum")
                                            {
                                                propertyTypeName = "enum";
                                            }

                                            switch (propertyTypeName)
                                            {
                                                case "nullable`1":
                                                    propertyTypeName = propertyType.GenericTypeArguments[0].Name.ToLower();
                                                    switch (propertyTypeName)
                                                    {
                                                        case "int32":
                                                            propotyColumn.SetValue(modelTWorkflowModel, int.Parse(itemWorkflowAction.EditColumnValue), null);
                                                            break;
                                                        case "enum":
                                                            propotyColumn.SetValue(modelTWorkflowModel, int.Parse(itemWorkflowAction.EditColumnValue), null);
                                                            break;
                                                        case "decimal":
                                                            propotyColumn.SetValue(modelTWorkflowModel, decimal.Parse(itemWorkflowAction.EditColumnValue), null);
                                                            break;
                                                        case "datetime":
                                                            propotyColumn.SetValue(modelTWorkflowModel, DateTime.Parse(itemWorkflowAction.EditColumnValue), null);
                                                            break;
                                                        case "boolean":
                                                            propotyColumn.SetValue(modelTWorkflowModel, bool.Parse(itemWorkflowAction.EditColumnValue), null);
                                                            break;
                                                        default:
                                                            propotyColumn.SetValue(modelTWorkflowModel, itemWorkflowAction.EditColumnValue, null);
                                                            break;
                                                    }
                                                    break;
                                                case "int32":
                                                    propotyColumn.SetValue(modelTWorkflowModel, int.Parse(itemWorkflowAction.EditColumnValue), null);
                                                    break;
                                                case "enum":
                                                    propotyColumn.SetValue(modelTWorkflowModel, int.Parse(itemWorkflowAction.EditColumnValue), null);
                                                    break;
                                                case "decimal":
                                                    propotyColumn.SetValue(modelTWorkflowModel, decimal.Parse(itemWorkflowAction.EditColumnValue), null);
                                                    break;
                                                case "datetime":
                                                    propotyColumn.SetValue(modelTWorkflowModel, DateTime.Parse(itemWorkflowAction.EditColumnValue), null);
                                                    break;
                                                case "boolean":
                                                    propotyColumn.SetValue(modelTWorkflowModel, bool.Parse(itemWorkflowAction.EditColumnValue), null);
                                                    break;
                                                default:
                                                    propotyColumn.SetValue(modelTWorkflowModel, itemWorkflowAction.EditColumnValue, null);
                                                    break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ThrowErrorInfo($"工作流设置有误，修改字段格式不正确，请联系管理员:{ex.Message}");
                                        }
                                        break;
                                    case WorkflowActionType.RunSql:
                                        if (string.IsNullOrWhiteSpace(itemWorkflowAction.RunSqlText))
                                        {
                                            ThrowErrorInfo(string.Format("工作流({0})下的({1})未设置运行脚本", modelWorkflowButton.Workflow.Name, modelWorkflowButton.Name));
                                            break;
                                        }

                                        string runSql = itemWorkflowAction.RunSqlText.Replace("@Id", "@id").Replace("@ID", "@id").Replace("@iD", "@id");
                                        var p_name = new MySqlParameter("@id", modelTWorkflowModel.Id.ToString());
                                        int effectRowCount = 0;
                                        if (runSql.Contains("@id"))
                                        {
                                            effectRowCount = DB.Database.ExecuteSqlRaw(runSql, p_name);
                                        }
                                        else
                                        {
                                            effectRowCount = DB.Database.ExecuteSqlRaw(runSql);
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }

                            if (modelTWorkflowModel.WorkflowStatus == WorkflowStatus.Refuse && string.IsNullOrWhiteSpace(remark))
                            {
                                ThrowErrorInfo("请填写说明");
                            }

                            foreach (WorkflowAction itemWorkflowAction in listWorkflowAction)
                            {
                                listUsersNotice = businessUsers.GetListByIds(itemWorkflowAction.NoticeUserIds, false);
                                listChannelNotice = businessChannel.GetListByIds(itemWorkflowAction.NoticeChannelIds, false);

                                if (listChannelNotice != null)
                                {
                                    foreach (var item in listChannelNotice)
                                    {
                                        listChannelIdsNotice.Add(item.Id);
                                    }
                                }

                                if (listUsersNotice != null)
                                {
                                    listUserNoticeAll.AddRange(listUsersNotice);
                                }

                                if (currentUser.Id != modelTWorkflowModel.CreatedById)
                                {
                                    listUserNoticeAll.Add(currentUser);
                                }

                                if (listChannelIdsNotice.Any())
                                {
                                    listUsersNotice = new List<Users>();

                                    foreach (int? item in listChannelIdsNotice)
                                    {
                                        string itemStr = string.Format(",{0},", item);
                                        var listUsersNoticeTemp = businessUsers.
                                        GetListByCondition(m => ("," + m.ChannelIds + ",").Contains(itemStr), false).ToList();
                                        if (listUsersNoticeTemp != null && listUsersNoticeTemp.Any())
                                        {
                                            listUsersNotice.AddRange(listUsersNoticeTemp);
                                        }
                                    }

                                    if (listUsersNotice != null)
                                    {
                                        if (!modelTWorkflowModel.TerritoryId.HasValue)
                                        {
                                            listUserNoticeAll.AddRange(listUsersNotice);
                                        }
                                        else
                                        {
                                            foreach (var itemUsersNotice in listUsersNotice)
                                            {
                                                if (!itemUsersNotice.TerritoryId.HasValue)
                                                {
                                                    continue;
                                                }
                                                modelUserTerritory = businessTerritory.GetByTerrId(itemUsersNotice.TerritoryId);
                                                if (modelUserTerritory == null)
                                                {
                                                    continue;
                                                }

                                                if (!businessTerritory.CheckHasRecordRight<TWorkflowModel>(modelTWorkflowModel, itemUsersNotice))
                                                {
                                                    continue;
                                                }

                                                listUserNoticeAll.Add(itemUsersNotice);
                                            }
                                        }
                                    }
                                }
                                if (itemWorkflowAction.NoticeLineManager)
                                {
                                    if (!modelWorkflowInstance.CreatedBy.LineManageId.HasValue)
                                    {
                                        ThrowErrorInfo(string.Format("工作流({0})下的({1})通知上级失败：记录ID({2}，原因：未设置上级)"
                                            , modelWorkflowButton.Workflow.Name, modelWorkflowButton.Name, modelTWorkflowModel.Id));
                                    }
                                    else
                                    {
                                        modelUsersLineManager = businessUsers.GetById(modelWorkflowInstance.CreatedBy.LineManageId.Value);
                                        if (modelUsersLineManager == null)
                                        {
                                            ThrowErrorInfo(string.Format("工作流({0})下的({1})通知上级失败：记录ID({2}，原因：未找到上级)"
                                            , modelWorkflowButton.Workflow.Name, modelWorkflowButton.Name, modelTWorkflowModel.Id));
                                        }
                                        else
                                        {
                                            listUserNoticeAll.Add(modelUsersLineManager);
                                        }
                                    }

                                }

                                switch (itemWorkflowAction.WorkflowActionType)
                                {
                                    case WorkflowActionType.WeChatNotice:
                                        if (listUserNoticeAll == null || !listUserNoticeAll.Any())
                                        {
                                            ThrowErrorInfo(string.Format("工作流({0})下的({1})未设置或未找到提醒人员信息,发送工作流提醒消息失败",
                                                modelWorkflowButton.Workflow.Name, modelWorkflowButton.Name));
                                            break;
                                        }

                                        if (listUserNoticeAll.Any(m => string.IsNullOrWhiteSpace(m.WeChatUserId)))
                                        {
                                            ThrowErrorInfo(string.Format("工作流({0})下的({1})未设置提醒人员{2}的企业账号信息,发送工作流提醒消息失败",
                                                modelWorkflowButton.Workflow.Name, modelWorkflowButton.Name, listUserNoticeAll.FirstOrDefault(m => string.IsNullOrWhiteSpace(m.WeChatUserId)).Code));
                                            break;
                                        }

                                        foreach (var itemUserNotice in listUserNoticeAll)
                                        {
                                            modelNotice = new Notice();
                                            modelNotice.CreatedById = currentUser.Id;
                                            modelNotice.CreatedDate = DateTime.Now;
                                            modelNotice.Content = SetNoticeContent(remark, currentUser, modelTWorkflowModel, modelWorkflowButton, itemWorkflowAction);
                                            modelNotice.NoticeManId = itemUserNotice.Id;
                                            modelNotice.EntityFullName = modelWorkflowButton.Workflow.WorkflowEntityName;
                                            modelNotice.RecordId = modelTWorkflowModel.Id;
                                            modelNotice.PreSentTime = DateTime.Now;
                                            modelNotice.IsSend = false;
                                            modelNotice.SendCount = 0;
                                            modelNotice.SendTime = null;
                                            modelNotice.QYWechatUserName = itemUserNotice.WeChatUserId;

                                            modelNotice.IsSendSuccess = false;
                                            businessNotice.Add(modelNotice, false);

                                            businessNotice.SendNotice(modelNotice, weChatWorkHelper, emailHeper);
                                        }

                                        break;
                                    case WorkflowActionType.EmailNotice:
                                        if (listUserNoticeAll == null || !listUserNoticeAll.Any())
                                        {
                                            ThrowErrorInfo(string.Format("工作流({0})下的({1})未设置或未找到提醒人员信息", modelWorkflowButton.Workflow.Name,
                                                modelWorkflowButton.Name));
                                            break;
                                        }
                                        if (string.IsNullOrWhiteSpace(EmailSettingParam.MyConfig.EmailHost) || string.IsNullOrWhiteSpace(EmailSettingParam.MyConfig.EmailPort)
                                            || string.IsNullOrWhiteSpace(EmailSettingParam.MyConfig.EmailSenderAddress) || string.IsNullOrWhiteSpace(EmailSettingParam.MyConfig.EmailPassword))
                                        {
                                            ThrowErrorInfo(string.Format("工作流({0})下的({1})：邮箱服务器参数设置不正确,发送工作流提醒消息失败",
                                                modelWorkflowButton.Workflow.Name, modelWorkflowButton.Name));
                                            break;
                                        }

                                        if (listUserNoticeAll.Any(m => string.IsNullOrWhiteSpace(m.Email)))
                                        {
                                            ThrowErrorInfo(string.Format("工作流({0})下的({1})未设置提醒人员{2}的邮箱信息,发送工作流提醒消息失败",
                                                modelWorkflowButton.Workflow.Name, modelWorkflowButton.Name, listUserNoticeAll.FirstOrDefault(m => string.IsNullOrWhiteSpace(m.WeChatUserId)).Code));
                                            break;
                                        }

                                        foreach (var itemUserNotice in listUserNoticeAll)
                                        {
                                            modelNotice = new Notice();
                                            modelNotice.CreatedById = currentUser.Id;
                                            modelNotice.CreatedDate = DateTime.Now;
                                            modelNotice.Content = SetNoticeContent(remark, currentUser, modelTWorkflowModel, modelWorkflowButton, itemWorkflowAction);
                                            modelNotice.NoticeManId = itemUserNotice.Id;
                                            modelNotice.EntityFullName = modelWorkflowButton.Workflow.WorkflowEntityName;
                                            modelNotice.RecordId = modelTWorkflowModel.Id;
                                            modelNotice.PreSentTime = DateTime.Now;
                                            modelNotice.IsSend = false;
                                            modelNotice.SendCount = 0;
                                            modelNotice.SendTime = null;
                                            modelNotice.EmailAddress = itemUserNotice.Email;

                                            businessNotice.Add(modelNotice, false);

                                            businessNotice.SendNotice(modelNotice, weChatWorkHelper, emailHeper);
                                        }

                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                        #endregion

                        #region 保存审批记录
                        TWorkflowProgressModel modelTWorkflowProgressModel = System.Activator.CreateInstance<TWorkflowProgressModel>();
                        modelTWorkflowProgressModel.CreatedById = currentUser.Id;
                        modelTWorkflowProgressModel.CreatedDate = DateTime.Now;
                        modelTWorkflowProgressModel.UpdatedById = currentUser.Id;
                        modelTWorkflowProgressModel.UpdatedDate = DateTime.Now;
                        modelTWorkflowProgressModel.Name = modelWorkflowButton.Name;
                        modelTWorkflowProgressModel.Remark = remark;
                        modelTWorkflowProgressModel.WorkflowButtonId = modelWorkflowButton.Id;
                        modelTWorkflowProgressModel.RecordId = recordId;
                        db.Entry<TWorkflowProgressModel>(modelTWorkflowProgressModel).State = EntityState.Added;
                        db.SaveChanges();
                        #endregion

                        dbTran.Commit();
                    }
                    catch (Exception dbEx)
                    {
                        dbTran.Rollback();
                        throw dbEx;
                    }
                    #endregion
                }
            });
        }

        /// <summary>
        /// 设置收到的通知消息内容
        /// </summary>
        /// <typeparam name="TWorkflowModel"></typeparam>
        /// <param name="remark"></param>
        /// <param name="currentUser"></param>
        /// <param name="modelTWorkflowModel"></param>
        /// <param name="modelWorkflowButton"></param>
        /// <param name="itemWorkflowAction"></param>
        /// <returns></returns>
        private static string SetNoticeContent<TWorkflowModel>(string remark, Users currentUser, TWorkflowModel modelTWorkflowModel, WorkflowButton modelWorkflowButton, WorkflowAction itemWorkflowAction) where TWorkflowModel : IWorkflowModel
        {
            Type entity = typeof(TWorkflowModel);
            remark = string.IsNullOrWhiteSpace(remark) ? "无" : remark;
            var entityDisplayNameAttribute = entity.GetCustomAttribute<DisplayNameAttribute>();
            string entityDisplayName = entityDisplayNameAttribute == null ? "" : entityDisplayNameAttribute.Name;
            return string.Format("{0};\r\n备注:{1};\r\n数据信息:{2};\r\n操作:{3}点击了{4};\r\n{5};",
                string.IsNullOrWhiteSpace(itemWorkflowAction.NoticeContent)
                ? "您收到了一条通知" : itemWorkflowAction.NoticeContent, remark, modelTWorkflowModel.Name
                , currentUser.Name, modelWorkflowButton.Name, string.IsNullOrWhiteSpace(entityDisplayName) ? "" : string.Format("实体:{0}", entityDisplayName));
        }
        #endregion


    }
}