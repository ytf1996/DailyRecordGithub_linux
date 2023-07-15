using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Business.Tlbb
{
    public class AccountService : CommonBusiness
    {
        public virtual void AddPoint(string name)
        {
            var p_name = new MySqlParameter("@name", $"{name}@game.sohu.com");
            //DBTLBB.Database.ExecuteSqlRaw("update account set point = IFNULL(point,0) + 1000 where name=@name", p_name);
        }
    }
}
