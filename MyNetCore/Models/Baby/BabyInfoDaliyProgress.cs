using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "婴儿管理工作流", parentMenuName: "婴儿管理", icons: "fa fa-ticket", orderNum: 212, onlyForAdmin: false, isMenu: false)]
    public class BabyInfoDaliyProgress : IWorkflowProgressModel
    {

    }
}