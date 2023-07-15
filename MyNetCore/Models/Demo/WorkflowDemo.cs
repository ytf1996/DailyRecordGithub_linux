using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyNetCore.Models
{
    [DisplayName(name: "工作流Demo", parentMenuName: "Demo", icons: "fa fa-ticket", orderNum: 920, onlyForAdmin: false, isMenu: true)]
    public class WorkflowDemo : IWorkflowModel
    {
        [Display(Name = "顺序", Order = 10)]
        public int OrderNum { get; set; }
    }
}