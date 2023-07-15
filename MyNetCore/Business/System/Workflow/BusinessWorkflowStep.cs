using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyNetCore.Models;

namespace MyNetCore.Business
{
    public class BusinessWorkflowStep : BaseBusiness<WorkflowStep>
    {
        public override bool NeedCheckNameRepeat
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 根据工作流ID获得第一个状态
        /// </summary>
        /// <param name="workflowId"></param>
        /// <returns></returns>
        public WorkflowStep GetFirstWorkflowStepByWorkflowId(int workflowId)
        {
            return GetByCondition(m => m.WorkflowId == workflowId && m.LevelPath == 1 && m.Deleted == false);
        }

    }
}