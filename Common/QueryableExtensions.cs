/********************************************************************
 * Copyright (C), 2019-2020, APP
 * File Name:       QueryableExtensions.cs
 * Author:          caichao
 * Create Time:     2019.07.12
 * Description:     ��ѯ������
********************************************************************/

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Roim.Common
{
    /// <summary>
    /// ��ѯ������
    /// </summary>
    public static class QueryableExtensions
	{
        /// <summary>
        /// ��ҳ��ѯ
        /// </summary>
        /// <typeparam name="T">���ͱ�ʶ</typeparam>
        /// <param name="query">��ѯ����</param>
        /// <param name="skipCount">������ֵ</param>
        /// <param name="maxResultCount">�������</param>
        /// <returns>IQueryable���Ͷ���</returns>
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
        /// where������ѯ
        /// </summary>
        /// <typeparam name="T">���ͱ�ʶ</typeparam>
        /// <param name="query">��ѯ����</param>
        /// <param name="condition">״̬����</param>
        /// <param name="predicate">ν������</param>
        /// <returns>IQueryable���Ͷ���</returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
		{
			return (condition ? query.Where<T>(predicate) : query);
		}

        /// <summary>
        /// where������ѯ
        /// </summary>
        /// <typeparam name="T">���ͱ�ʶ</typeparam>
        /// <param name="query">��ѯ����</param>
        /// <param name="condition">״̬����</param>
        /// <param name="predicate">ν������</param>
        /// <returns>IQueryable���Ͷ���</returns>
        public static IQueryable<T> WhereIf<T>(this IQueryable<T> query, bool condition, Expression<Func<T, int, bool>> predicate)
		{
			return (condition ? query.Where<T>(predicate) : query);
		}
	}
}