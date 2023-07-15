/********************************************************************
 * Copyright (C), 2019-2020, APP
 * File Name:       QueryableExtensions.cs
 * Author:          caichao
 * Create Time:     2019.07.12
 * Description:     查询工具类
********************************************************************/

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Roim.Common
{
    /// <summary>
    /// 查询工具类
    /// </summary>
    public static class QueryableExtensions
	{
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型标识</typeparam>
        /// <param name="query">查询对象</param>
        /// <param name="skipCount">跳过数值</param>
        /// <param name="maxResultCount">最大结果数</param>
        /// <returns>IQueryable泛型对象</returns>
        public static IQueryable<T> PageBy<T>(this IQueryable<T> query, int skipCount, int maxResultCount)
		{
			if (query == null)
			{
				throw new ArgumentNullException("query");
			}
			IQueryable<T> ts = query.Skip<T>(skipCount).Take<T>(maxResultCount);
			return ts;
		}

        /// <summary>
        /// where条件查询
        /// </summary>
        /// <typeparam name="T">泛型标识</typeparam>
        /// <param name="query">查询对象</param>
        /// <param name="condition">状态条件</param>
        /// <param name="predicate">谓词条件</param>
        /// <returns>IQueryable泛型对象</returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
		{
			return (condition ? query.Where<T>(predicate) : query);
		}

        /// <summary>
        /// where条件查询
        /// </summary>
        /// <typeparam name="T">泛型标识</typeparam>
        /// <param name="query">查询对象</param>
        /// <param name="condition">状态条件</param>
        /// <param name="predicate">谓词条件</param>
        /// <returns>IQueryable泛型对象</returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, int, bool>> predicate)
		{
			return (condition ? query.Where<T>(predicate) : query);
		}
	}
}