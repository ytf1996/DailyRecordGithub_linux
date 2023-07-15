using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Models
{
    public class ToolModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [CustomColumn(isHide: true)]
        [Column(name: "Id")]
        public int Id { get; set; }

        public int RecordCount { get; set; }
    }
}
