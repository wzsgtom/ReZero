﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks; 

namespace ReZero.SuperAPI
{
    internal class UpdateObject : CommonDataService, IDataService
    {
        public async Task<object> ExecuteAction(DataModel dataModel)
        {
            var db = App.Db;
            var type = await EntityGeneratorManager.GetTypeAsync(dataModel.TableId);
            base.InitData(type, db, dataModel);
            await db.UpdateableByObject(dataModel.Data).ExecuteCommandAsync();
            return true;
        }
    }
}