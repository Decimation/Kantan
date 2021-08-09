using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kantan.Diagnostics;
using Kantan.Utilities;
using static Kantan.Diagnostics.LogCategories;

namespace Kantan.Cli
{
	// todo: WIP

	public class CliHandler
	{
		public class CliParameter
		{
			public string ParameterId { get; init; }

			public Func<string[], object> Function { get; init; }

			public int ArgumentCount { get; init; }
		}


		public CliHandler()
		{
			Parameters = new List<CliParameter>();
		}

		public List<CliParameter> Parameters { get; }


		public void Run() => Run(Environment.GetCommandLineArgs());

		public void Run(string[] args)
		{
			Trace.WriteLine($"{nameof(CliHandler)}: cli arguments: {args.QuickJoin()}", C_INFO);

			var argEnum = args.GetEnumerator().Cast<string>();

			while (argEnum.MoveNext()) {

				var arg = Parameters.FirstOrDefault(x => x.ParameterId == argEnum.Current);

				if (arg != null) {
					var count = arg.ArgumentCount;

					Guard.AssertNonNegative(count);

					if (count == 0) {
						// switch
						arg.Function(new[] {true.ToString()});
						return;
					}

					var argValues = new string[count];

					for (int i = 0; i < count; i++) {
						argEnum.MoveNext();
						argValues[i] = argEnum.Current;
					}


					arg.Function(argValues);

					//((FieldInfo)((MemberExpression)reg.fx.Body).Member).SetValue();
				}

			}


		}
	}
}