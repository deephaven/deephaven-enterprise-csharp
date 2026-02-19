/*
 * Copyright (c) 2016-2020 Deephaven Data Labs and Patent Pending
 */
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Deephaven.OpenAPI.Core.API.Util
{
	public static class ExceptionUtil
	{
		public static void CheckState(bool condition, string errorMessage)
		{
			if (!condition)
			{
				throw new InvalidOperationException(errorMessage);
			}
		}

		public static T ResultOrUnwrappedException<T>(Task<T> task)
		{
			try
			{
				return task.Result;
			}
			catch (AggregateException ae)
			{
				ExceptionDispatchInfo.Capture(ae.InnerException).Throw();
				throw new Exception("notreached");
			}
		}

		public static void WaitOrUnwrappedException(Task task)
		{
			try
			{
				task.Wait();
			}
			catch (AggregateException ae)
			{
				ExceptionDispatchInfo.Capture(ae.InnerException).Throw();
				throw new Exception("notreached");
			}
		}
	}
}
