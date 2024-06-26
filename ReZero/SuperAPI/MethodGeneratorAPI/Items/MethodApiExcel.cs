﻿ using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ReZero.Excel;

namespace ReZero.SuperAPI 
{
    public partial class MethodApi
    {
        public byte[] ExportEntities(long databaseId,long[] tableIds)
        {
            List<ExcelData> datatables = new List<ExcelData>();
            var db = App.Db;
            var datas = db.Queryable<ZeroEntityInfo>()
                .OrderBy(it=>it.DbTableName)
                .Where(it=>it.DataBaseId==databaseId)
                .WhereIF(tableIds.Any(), it => tableIds.Contains(it.Id))
                .Includes(it => it.ZeroEntityColumnInfos).ToList();
            foreach (var item in datas)
            {
                var currentDb = App.GetDbById(databaseId)!;
                var columnInfos = currentDb.DbMaintenance.GetColumnInfosByTableName(item.DbTableName, false);
                DataTable dt = new DataTable();
                dt.Columns.Add(TextHandler.GetCommonText("列名", "Field name"));
                dt.Columns.Add(TextHandler.GetCommonText("列描述", "Column description"));
                dt.Columns.Add(TextHandler.GetCommonText("列类型", "Column type"));
                dt.Columns.Add(TextHandler.GetCommonText("实体类型", "Entity type"));
                dt.Columns.Add(TextHandler.GetCommonText("主键", "Primary key"));
                dt.Columns.Add(TextHandler.GetCommonText("自增", "Auto increment"));
                dt.Columns.Add(TextHandler.GetCommonText("可空", "Nullable"));
                dt.Columns.Add(TextHandler.GetCommonText("长度", "Length"));
                dt.Columns.Add(TextHandler.GetCommonText("精度", "Precision"));
                dt.Columns.Add(TextHandler.GetCommonText("默认值", "Default value"));
                dt.Columns.Add(TextHandler.GetCommonText("表名", "Table name"));
                dt.Columns.Add(TextHandler.GetCommonText("表描述", "Table description"));
                foreach (var it in columnInfos!)
                {
                    var dr = dt.NewRow();
                    dr[TextHandler.GetCommonText("列名", "Field name")] = it.DbColumnName;
                    dr[TextHandler.GetCommonText("列描述", "Column description")] = it.ColumnDescription?? item.ZeroEntityColumnInfos.FirstOrDefault(x => x.DbColumnName!.EqualsCase(it.DbColumnName!))?.Description; ;
                    dr[TextHandler.GetCommonText("列类型", "Column type")] = it.DataType;
                    if (db.CurrentConnectionConfig.DbType == SqlSugar.DbType.Oracle)
                    {
                        dr[TextHandler.GetCommonText("列类型", "Column type")] = it.OracleDataType;
                    }
                    dr[TextHandler.GetCommonText("实体类型", "Entity type")] = it.PropertyType;
                    dr[TextHandler.GetCommonText("表名", "Table name")] = item.DbTableName;
                    dr[TextHandler.GetCommonText("表描述", "Table description")] = item.Description ?? item.Description; 
                    dr[TextHandler.GetCommonText("主键", "Primary key")] = it.IsPrimarykey ? "yes" : "";
                    dr[TextHandler.GetCommonText("自增", "Auto increment")] = it.IsIdentity ? "yes" : "";
                    dr[TextHandler.GetCommonText("可空", "Nullable")] = it.IsNullable ? "yes" : "";
                    dr[TextHandler.GetCommonText("长度", "Length")] = it.Length;
                    dr[TextHandler.GetCommonText("精度", "Precision")] = it.DecimalDigits;

                    dt.Rows.Add(dr);
                }
                dt.TableName = item.DbTableName;
                if (dt.Rows.Count == 0) 
                {
                    var dr = dt.NewRow();
                    dr[TextHandler.GetCommonText("列名", "Field name")]= TextHandler.GetCommonText("表还没有创建需要到实体管理点同步", "The table has not yet been created and needs to be synchronized to the entity management point");
                    dt.Rows.Add(dr);
                }
                datatables.Add(new ExcelData() { DataTable=dt, TableDescrpition=item.Description??"-" });
            }
            return ReZero.Excel.DataTableToExcel.ExportExcel(datatables.ToArray(), $"{DateTime.Now.ToString("实体文档.xlsx")}",navName:TextHandler.GetCommonText("表名","Table name"));
        } 
    }
}
