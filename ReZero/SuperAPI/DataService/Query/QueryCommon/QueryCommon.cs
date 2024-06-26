﻿using SqlSugar;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Data;
using System.Text.RegularExpressions;
namespace ReZero.SuperAPI 
{
    public partial class QueryCommon :CommonDataService, IDataService
    {
        public ISqlSugarClient? _sqlSugarClient;
        private ISqlBuilder? _sqlBuilder;
        public async Task<object> ExecuteAction(DataModel dataModel)
        {
            try
            {
                RefAsync<int> count = 0;
                _sqlSugarClient = App.GetDbTableId(dataModel.TableId) ?? App.Db;
                _sqlBuilder = _sqlSugarClient.Queryable<object>().SqlBuilder;
                var type = await EntityGeneratorManager.GetTypeAsync(dataModel.TableId);
                base.InitDb(type,_sqlSugarClient);
                var queryObject = _sqlSugarClient.QueryableByObject(type, PubConst.Orm_TableDefaultMasterTableShortName);
                queryObject = Join(type, dataModel, queryObject);
                queryObject = Where(type, dataModel, queryObject);
                queryObject = OrderBySelectBefore(type, dataModel, queryObject);
                queryObject = GroupBy(type, dataModel, queryObject);
                queryObject = Select(type, dataModel, queryObject); 
                queryObject = MergeTable(type,dataModel,queryObject);
                object? result = await ToList(dataModel, count, type, queryObject);
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    } 
}
