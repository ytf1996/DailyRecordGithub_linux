using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using MyNetCore.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Business
{
    public class DataBaseContextConfig : IDesignTimeDbContextFactory<MySqlContext>
    {
        public MySqlContext CreateDbContext(string[] args)
        {
            var optionBuilder = new DbContextOptionsBuilder<MySqlContext>();
            string dbConfig = BusinessHelper.DBConfig;
            if (string.IsNullOrWhiteSpace(dbConfig))
            {
                var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
                var config = builder.Build();
                //读取配置
                dbConfig = config[$"ConnectionStrings:MySqlConnection"];
            }
            optionBuilder.UseMySql(dbConfig,m=> {
                m.EnableRetryOnFailure(maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: new int[] { 2 });
                }).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            return new MySqlContext(optionBuilder.Options);
        }
    }
}
