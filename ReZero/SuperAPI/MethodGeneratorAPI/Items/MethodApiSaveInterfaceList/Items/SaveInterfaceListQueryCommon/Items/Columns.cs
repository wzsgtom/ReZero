﻿using SqlSugar;
using SqlSugar.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ReZero.SuperAPI
{
    public partial class SaveInterfaceListQueryCommon : BaseSaveInterfaceList, ISaveInterfaceList
    {
        #region Core
        private void SetColumns(SaveInterfaceListModel saveInterfaceListModel, ZeroInterfaceList zeroInterfaceList)
        {
            var anyColumns = saveInterfaceListModel!.Json!.Columns.Any();
            var anyJoin = saveInterfaceListModel!.Json!.ComplexityColumns.Any();
            var tableId = GetTableId(saveInterfaceListModel);
            var columns = GetTableColums(tableId);
            if (IsDefaultColums(anyColumns, anyJoin))
            {
                AddDefaultColumns(zeroInterfaceList, columns);
            }
            else
            {
                AddMasterColumns(saveInterfaceListModel, zeroInterfaceList, anyColumns, columns);
                AddJoinColumns(saveInterfaceListModel, zeroInterfaceList, anyJoin);
            }
        }
        private static void AddJoinColumns(SaveInterfaceListModel saveInterfaceListModel, ZeroInterfaceList zeroInterfaceList, bool anyJoin)
        {
            if (anyJoin)
            {
                var joinColumns = saveInterfaceListModel!.Json!.ComplexityColumns;
                var tableNames = joinColumns.Select(it => it.Json!.JoinInfo!.JoinTable!.ToLower()).ToList();
                var entityInfos = GetJoinEntityInfos(joinColumns, tableNames);
                var index = 0;
                foreach (var item in GetJoinComplexityColumns(joinColumns!))
                {
                    index++;
                    var tableInfo = GetJoinEntity(entityInfos, item);
                    AddJoins(zeroInterfaceList, index, item, tableInfo);
                    AddJoinSelectColumns(zeroInterfaceList, index, item, tableInfo);
                }
                var subIndex = 0;
                foreach (var item in GetSubqueryComplexityColumns(joinColumns!))
                {
                    var tableInfo = GetJoinEntity(entityInfos, item);
                    subIndex++;
                    AddSubquerySelectColums(zeroInterfaceList, subIndex, item, tableInfo);
                }
            }
        }

        private static void AddJoins(ZeroInterfaceList zeroInterfaceList, int index, CommonQueryComplexitycolumn item, ZeroEntityInfo tableInfo)
        {
            if (zeroInterfaceList.DataModel!.JoinParameters == null)
                zeroInterfaceList.DataModel!.JoinParameters = new List<DataModelJoinParameters>();
            zeroInterfaceList!.DataModel!.JoinParameters.Add(new DataModelJoinParameters()
            {
                JoinTableId = tableInfo!.Id,
                JoinType= (item.Json?.JoinInfo?.JoinType== ColumnJoinType.LeftJoin)?JoinType.Left:JoinType.Inner,
                OnList = new List<JoinParameter>()
                        {
                            new JoinParameter()
                            {
                                FieldOperator=FieldOperatorType.Equal,
                                LeftIndex=0,
                                LeftPropertyName=item.Json!.JoinInfo!.MasterField,
                                RightPropertyName=item.Json!.JoinInfo!.JoinField,
                                RightIndex=index
                            }
                        }
            });
        }
        private void AddMasterColumns(SaveInterfaceListModel saveInterfaceListModel, ZeroInterfaceList zeroInterfaceList, bool anyColumns, List<ZeroEntityColumnInfo> columns)
        {
            if (anyColumns)
            {
                zeroInterfaceList.DataModel!.Columns = columns
                .Where(it => saveInterfaceListModel!.Json!.Columns.Any(z => z.PropertyName == it.PropertyName)).Select(it => new DataColumnParameter()
                {
                    Description = saveInterfaceListModel!.Json!.Columns.FirstOrDefault(z => z.PropertyName == it.PropertyName).DbColumnName,
                    PropertyName = it.PropertyName
                }).ToList(); 
                var isPage = saveInterfaceListModel.PageSize;
                if (isPage) 
                {
                    foreach (var item in zeroInterfaceList.DataModel!.Columns)
                    {
                        if (item.PropertyName == item.Description || string.IsNullOrEmpty(item.Description))
                        {
                            item.Description = columns.FirstOrDefault(it => it.PropertyName == item.PropertyName).Description;
                        }
                    }
                }
                zeroInterfaceList.DataModel!.SelectParameters = saveInterfaceListModel!.Json!.Columns
                  .Select(it => new DataModelSelectParameters()
                  {
                      AsName = isPage ? it.PropertyName : it.DbColumnName,
                      TableIndex = 0,
                      Name = it.PropertyName,
                  }).ToList();
            }
        }
        private void AddDefaultColumns(ZeroInterfaceList zeroInterfaceList, List<ZeroEntityColumnInfo> columns)
        {
            if (this.isPage)
            {
                zeroInterfaceList.DataModel!.Columns = columns.Select(it => new DataColumnParameter()
                {
                    Description = it.PropertyName,
                    PropertyName = it.PropertyName,
                    AsName = it.PropertyName

                }).ToList();

                foreach (var item in zeroInterfaceList.DataModel!.Columns)
                {
                    if (item.PropertyName == item.Description || string.IsNullOrEmpty(item.Description))
                    {
                        item.Description = columns.FirstOrDefault(it => it.PropertyName == item.PropertyName).Description;
                    }
                }
            }
            else
            {
                zeroInterfaceList.DataModel!.Columns = columns.Select(it => new DataColumnParameter()
                {
                    Description = it.DbColumnName,
                    PropertyName = it.PropertyName,
                    AsName = it.DbColumnName

                }).ToList();
            }
        }
        private static void AddSubquerySelectColums(ZeroInterfaceList zeroInterfaceList, int subIndex, CommonQueryComplexitycolumn item, ZeroEntityInfo tableInfo)
        {
            var columnsInfo = tableInfo!.ZeroEntityColumnInfos!
                                 .Where(it => it.PropertyName == item.Json!.JoinInfo!.ShowField).First();
            var joinField = item.Json!.JoinInfo!.JoinField;
            var materField = item.Json!.JoinInfo!.MasterField;
            var asName = item.Json!.JoinInfo!.Name;
            var showField = item.Json!.JoinInfo!.ShowField;
            var subQueryable = App.Db.Queryable<object>();
            var builder = subQueryable.QueryBuilder.Builder;
            var subquerySql = subQueryable
                .Take(1)
                .AS(tableInfo.DbTableName)
                .Where($"{builder.GetTranslationColumnName(joinField)}={builder.GetTranslationColumnName(PubConst.Orm_TableDefaultPreName + 0)}.{builder.GetTranslationColumnName(materField)}")
                .Select(SelectModel.Create(new SelectModel()
                {
                    AsName = asName,
                    FieldName = showField
                })).ToSql().Key;
            DataModelSelectParameters addColumnItem = new DataModelSelectParameters()
            {
                Name = PubConst.Orm_SubqueryKey,
                SubquerySQL = $"({subquerySql}) AS {builder.GetTranslationColumnName(asName)} ",
                AsName = asName,
            };
            zeroInterfaceList.DataModel!.SelectParameters!.Add(addColumnItem);
            zeroInterfaceList.DataModel!.Columns!.Add(new DataColumnParameter()
            {
                Description = addColumnItem.AsName,
                PropertyName = addColumnItem.AsName
            });
        }
        private static void AddJoinSelectColumns(ZeroInterfaceList zeroInterfaceList, int index, CommonQueryComplexitycolumn item, ZeroEntityInfo tableInfo)
        {
            var columnsInfo = tableInfo!.ZeroEntityColumnInfos!
                                    .Where(it => it.PropertyName == item.Json!.JoinInfo!.ShowField).First();
            DataModelSelectParameters addColumnItem = new DataModelSelectParameters()
            {
                Name = columnsInfo.PropertyName,
                TableIndex = index,
                AsName = string.IsNullOrEmpty(item.Json!.JoinInfo!.Name) ? columnsInfo.PropertyName : item.Json!.JoinInfo!.Name
            };
            zeroInterfaceList.DataModel!.SelectParameters!.Add(addColumnItem);
            zeroInterfaceList.DataModel!.Columns!.Add(new DataColumnParameter()
            {
                Description = addColumnItem.AsName,
                PropertyName = addColumnItem.AsName
            });
        }

        #endregion

        #region Helper 
        private static ZeroEntityInfo GetJoinEntity(List<ZeroEntityInfo> entityInfos, CommonQueryComplexitycolumn item)
        {
            return entityInfos.FirstOrDefault(it => it.DbTableName!.ToLower() == item!.Json!.JoinInfo!.JoinTable!.ToLower() ||
                                                                                 it.ClassName!.ToLower() == item!.Json!.JoinInfo!.JoinTable!.ToLower());
        }
        private static IEnumerable<CommonQueryComplexitycolumn> GetJoinComplexityColumns(CommonQueryComplexitycolumn[] joinColumns)
        {
            return joinColumns!.Where(it => it.Json!.JoinInfo!.JoinType != ColumnJoinType.SubqueryJoin);
        }
        private static IEnumerable<CommonQueryComplexitycolumn> GetSubqueryComplexityColumns(CommonQueryComplexitycolumn[] joinColumns)
        {
            return joinColumns!.Where(it => it.Json!.JoinInfo!.JoinType == ColumnJoinType.SubqueryJoin);
        }

        private static List<ZeroEntityInfo> GetJoinEntityInfos(CommonQueryComplexitycolumn[]? joinColumns, List<string> tableNames)
        {
            return App.Db.Queryable<ZeroEntityInfo>()
                                .Includes(s => s.ZeroEntityColumnInfos)
                                .Where(s =>
                                                  joinColumns.Any(it => tableNames.Contains(s.DbTableName!.ToLower())) ||
                                                  joinColumns.Any(it => tableNames.Contains(s.ClassName!.ToLower()))
                                          )
                                .ToList();
        }
        private static bool IsDefaultColums(bool anyColumns, bool anyJoin)
        {
            return !anyJoin && !anyColumns;
        }

        private static List<ZeroEntityColumnInfo> GetTableColums(long tableId)
        {
            return App.Db.Queryable<ZeroEntityColumnInfo>().Where(it => it.TableId == tableId).ToList();
        }

        private static long GetTableId(SaveInterfaceListModel saveInterfaceListModel)
        {
            return App.Db.Queryable<ZeroEntityInfo>().Where(it => it.ClassName == saveInterfaceListModel.TableId).First().Id;
        }
        #endregion

    }
}
