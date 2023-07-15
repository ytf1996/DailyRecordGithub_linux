using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyNetCore.Models;
using Roim.Common;

namespace MyNetCore.Business
{
    public class BusinessTaskHistory : BaseBusiness<TaskHistory>
    {
        public override bool NeedCheckNameRepeat => false;

        protected override bool IsTask
        {
            get
            {
                return true;
            }
        }
    }
}