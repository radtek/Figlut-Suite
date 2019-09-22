﻿namespace Figlut.Server.Toolkit.Data.DB.SQLite
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Data.SqlTypes;
    using System.Data;

    #endregion //Using Directives

    /// <summary>
    /// http://msdn.microsoft.com/en-us/library/system.data.sqltypes.aspx
    /// </summary>
    public class SqliteTypeConverter : EntityCache<string, SqliteTypeConversionInfo>
    {
        #region Singleton Setup

        private static SqliteTypeConverter _instance;

        public static SqliteTypeConverter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SqliteTypeConverter();
                }
                return _instance;
            }
        }

        #endregion //Singleton Setup

        #region Constructors

        private SqliteTypeConverter()
        {
            Add(new SqliteTypeConversionInfo("bigint", typeof(SqlInt64), DbType.Int64, typeof(Int64)));
            Add(new SqliteTypeConversionInfo("binary", typeof(SqlBytes), DbType.Binary, typeof(Byte[])));
            Add(new SqliteTypeConversionInfo("bit", typeof(SqlBoolean), DbType.Int16, typeof(Boolean)));
            Add(new SqliteTypeConversionInfo("char", typeof(SqlChars), DbType.String, typeof(char))); //this one may need work
            Add(new SqliteTypeConversionInfo("cursor", null, DbType.Object, null));
            Add(new SqliteTypeConversionInfo("date", typeof(SqlDateTime), DbType.Date, typeof(DateTime)));
            Add(new SqliteTypeConversionInfo("datetime", typeof(SqlDateTime), DbType.DateTime, typeof(DateTime)));
            Add(new SqliteTypeConversionInfo("datetime2", typeof(SqlDateTime), DbType.DateTime2, typeof(DateTime)));
            Add(new SqliteTypeConversionInfo("DATETIMEOFFSET", typeof(SqlDateTime), DbType.DateTimeOffset,typeof(DateTimeOffset)));
            Add(new SqliteTypeConversionInfo("decimal", typeof(SqlDecimal), DbType.Decimal, typeof(Decimal)));
            Add(new SqliteTypeConversionInfo("float", typeof(SqlDouble), DbType.Double, typeof(Double)));
            //Add(new SqlTypeConversionInfo("geography", typeof(SqlGeography),typeof(null)));
            //Add(new SqlTypeConversionInfo("geometry", typeof(SqlGeometry),typeof(null)));
            //Add(new SqlTypeConversionInfo("hierarchyid", typeof(SqlHierarchyId),typeof(null)));
            Add(new SqliteTypeConversionInfo("image", typeof(SqlBytes), DbType.Binary, typeof(byte[])));
            Add(new SqliteTypeConversionInfo("int", typeof(SqlInt32), DbType.Int32, typeof(Int32)));
            Add(new SqliteTypeConversionInfo("money", typeof(SqlMoney), DbType.Decimal, typeof(Decimal)));
			Add(new SqliteTypeConversionInfo("varchar", typeof(SqlString), DbType.String, typeof(string))); //this one may need work
			Add(new SqliteTypeConversionInfo("nchar", typeof(SqlChars), DbType.String, typeof(String)));
            Add(new SqliteTypeConversionInfo("ntext", typeof(SqlChars), DbType.String, null));
            Add(new SqliteTypeConversionInfo("numeric", typeof(SqlDecimal), DbType.Decimal, typeof(Decimal)));
            Add(new SqliteTypeConversionInfo("nvarchar", typeof(SqlChars), DbType.String, typeof(String)));
            Add(new SqliteTypeConversionInfo("nvarchar(1)", typeof(SqlChars), DbType.String, typeof(Char)));
            Add(new SqliteTypeConversionInfo("nchar(1)", typeof(SqlChars), DbType.String, typeof(Char)));
            Add(new SqliteTypeConversionInfo("real", typeof(SqlSingle), DbType.Double, typeof(Single)));
            Add(new SqliteTypeConversionInfo("rowversion", null, DbType.Binary, typeof(Byte[])));
            Add(new SqliteTypeConversionInfo("smallint", typeof(SqlInt16), DbType.Int16, typeof(Int16)));
            Add(new SqliteTypeConversionInfo("smallmoney", typeof(SqlMoney), DbType.Decimal,typeof(Decimal)));
            //Add(new SqliteTypeConversionInfo("sql_variant", null, SqlDbType.Variant, typeof(Object)));
            Add(new SqliteTypeConversionInfo("table", null, DbType.Binary, null));
            Add(new SqliteTypeConversionInfo("text", typeof(SqlString), DbType.String,typeof(string))); //this one may need work
            Add(new SqliteTypeConversionInfo("time", null, DbType.Time, typeof(TimeSpan)));
            Add(new SqliteTypeConversionInfo("timestamp", null, DbType.DateTime, null));
            Add(new SqliteTypeConversionInfo("tinyint", typeof(SqlByte), DbType.Int16, typeof(Byte)));
            Add(new SqliteTypeConversionInfo("uniqueidentifier", typeof(SqlGuid), DbType.Guid, typeof(Guid)));
            Add(new SqliteTypeConversionInfo("varbinary", typeof(SqlBytes), DbType.Binary, typeof(Byte[])));
            Add(new SqliteTypeConversionInfo("varbinary(1)", typeof(SqlBytes), DbType.Binary, typeof(byte)));
            Add(new SqliteTypeConversionInfo("binary(1)", typeof(SqlBytes), DbType.Binary, typeof(byte)));
            Add(new SqliteTypeConversionInfo("xml", typeof(SqlXml), DbType.Xml, typeof(string)));
        }

        #endregion //Constructors

        #region Methods

        public Type GetDotNetType(string sqlTypeName, bool isNullable)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqliteTypeConversionInfo).FullName,
                    sqlTypeName));
            }
            Type result = this[sqlTypeName].DotNetType;
            if (result == null)
            {
                throw new NullReferenceException(string.Format(
                    "No matching .NET type for {0}.",
                    sqlTypeName));
            }
            if (isNullable)
            {
                return DataHelper.GetNullableType(result);
            }
            return result;
        }

        public Type GetDotNetType(Type sqlType, bool isNullable)
        {
            foreach (SqliteTypeConversionInfo typeInfo in this)
            {
                if (typeInfo.SqlType == null)
                {
                    continue;
                }
                if (!isNullable && typeInfo.SqlType.IsAssignableFrom(sqlType))
                {
                    if (typeInfo.DotNetType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching .NET type for SQL type {0}.",
                            sqlType.FullName));
                    }
                    return typeInfo.DotNetType;
                }
                else if (DataHelper.GetNullableType(typeInfo.SqlType).IsAssignableFrom(sqlType))
                {
                    if (typeInfo.DotNetType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching .NET type for SQL type {0}.",
                            sqlType.FullName));
                    }
                    return typeInfo.DotNetType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for SQL Type {1}.",
                typeof(SqliteTypeConversionInfo).FullName,
                sqlType.FullName));
        }

        public Type GetSqlType(string sqlTypeName, bool isNullable)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqliteTypeConversionInfo).FullName,
                    sqlTypeName));
            }
            Type result = this[sqlTypeName].SqlType;
            if (result == null)
            {
                throw new NullReferenceException(string.Format(
                    "No matching SQL type for {0}.",
                    sqlTypeName));
            }
            if (isNullable)
            {
                return DataHelper.GetNullableType(result);
            }
            return result;
        }

        public Type GetSqlType(Type dotNetType, bool isNullable)
        {
            foreach (SqliteTypeConversionInfo typeInfo in this)
            {
                if (typeInfo.DotNetType == null)
                {
                    continue;
                }
                if (!isNullable && typeInfo.DotNetType.IsAssignableFrom(dotNetType))
                {
                    if (typeInfo.SqlType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching SQL type for .NET type {0}.",
                            dotNetType.FullName));
                    }
                    return typeInfo.SqlType;
                }
                else if (DataHelper.GetNullableType(typeInfo.DotNetType).IsAssignableFrom(dotNetType))
                {
                    if (typeInfo.SqlType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching SQL type for .NET type {0}.",
                            dotNetType.FullName));
                    }
                    return typeInfo.SqlType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for .NET Type {1}.",
                typeof(SqliteTypeConversionInfo).FullName,
                dotNetType.FullName));
        }

        public string GetSqlTypeNameFromDotNetType(Type dotNetType, bool isNullable)
        {
			if (dotNetType == typeof(Guid)) {
				int stop = 0;
			}
            foreach (SqliteTypeConversionInfo typeInfo in this)
            {
                if (typeInfo.DotNetType == null)
                {
                    continue;
                }
                if (!isNullable && typeInfo.DotNetType.IsAssignableFrom(dotNetType) && typeInfo.DotNetType == dotNetType)
                {
                    return typeInfo.SqlTypeName;
                }
				else if (DataHelper.GetNullableType(typeInfo.DotNetType).IsAssignableFrom(dotNetType) && 
					DataHelper.GetNullableType(typeInfo.DotNetType) == dotNetType)
                {
                    return typeInfo.SqlTypeName;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for .NET Type {1}.",
                typeof(SqliteTypeConversionInfo).FullName,
                dotNetType.FullName));
        }

        public string GetSqlTypeNameFromSqlType(Type sqlType, bool isNullable)
        {
            foreach (SqliteTypeConversionInfo typeInfo in this)
            {
                if (typeInfo.SqlType == null)
                {
                    continue;
                }
                if (!isNullable && typeInfo.SqlType.IsAssignableFrom(sqlType))
                {
                    return typeInfo.SqlTypeName;
                }
                else if (DataHelper.GetNullableType(typeInfo.SqlType).IsAssignableFrom(sqlType))
                {
                    return typeInfo.SqlTypeName;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for SQLs Type {1}.",
                typeof(SqliteTypeConversionInfo).FullName,
                sqlType.FullName));
        }

        public DbType GetSqlDbType(string sqlTypeName)
        {
            if (!Exists(sqlTypeName))
            {
                throw new ArgumentException(string.Format(
                    "Could not find {0} for SQL Type Name {1}.",
                    typeof(SqliteTypeConversionInfo).FullName,
                    sqlTypeName));
            }
            return this[sqlTypeName].DbType;
        }

        public DbType GetSqlDbTypeFromDotNetType(Type dotNetType)
        {
            foreach (SqliteTypeConversionInfo typeInfo in this)
            {
                if (typeInfo.DotNetType == null)
                {
                    continue;
                }
                if (typeInfo.DotNetType.IsAssignableFrom(dotNetType))
                {
                    if (typeInfo.SqlType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching SQL type for .NET type {0}.",
                            dotNetType.FullName));
                    }
                    return typeInfo.DbType;
                }
                else if (DataHelper.GetNullableType(typeInfo.DotNetType).IsAssignableFrom(dotNetType))
                {
                    if (typeInfo.SqlType == null)
                    {
                        throw new NullReferenceException(string.Format(
                            "No matching SQL type for .NET type {0}.",
                            dotNetType.FullName));
                    }
                    return typeInfo.DbType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for .NET Type {1}.",
                typeof(SqliteTypeConversionInfo).FullName,
                dotNetType.FullName));
        }

        public DbType GetSqlDbTypeFromSqlType(Type sqlType)
        {
            foreach (SqliteTypeConversionInfo typeInfo in this)
            {
                if (typeInfo.SqlType == null)
                {
                    continue;
                }
                if (typeInfo.SqlType.IsAssignableFrom(sqlType))
                {
                    return typeInfo.DbType;
                }
                else if (DataHelper.GetNullableType(typeInfo.SqlType).IsAssignableFrom(sqlType))
                {
                    return typeInfo.DbType;
                }
            }
            throw new ArgumentException(string.Format(
                "Could not find {0} for SQLs Type {1}.",
                typeof(SqliteTypeConversionInfo).FullName,
                sqlType.FullName));
        }

        #endregion //Methods
    }
}
