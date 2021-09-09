using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Kantan.Collections;
using Kantan.Diagnostics;
using Kantan.Text;
using static Kantan.Diagnostics.LogCategories;
// ReSharper disable EmptyConstructor

// ReSharper disable UnusedMember.Global

namespace Kantan.Utilities
{
	public class CliHandler
	{
		public CliHandler() { }

		public List<CliParameter> Parameters { get; } = new();

		[CanBeNull]
		public CliParameter Default { get; set; }


		public void Run() => Run(Environment.GetCommandLineArgs());

		public void Run(string[] args)
		{
			Trace.WriteLine($"{nameof(CliHandler)}: arguments: {args.QuickJoin(" ")}", C_DEBUG);

			var argEnum = args.GetEnumerator().Cast<string>();

			while (argEnum.MoveNext()) {

				var current = argEnum.Current;

				var cliParam = Parameters.FirstOrDefault(p => p.ParameterId == current);

				if (cliParam == null) {
					if (Default == null) {
						throw new InvalidOperationException();
					}

					Default.Handle(argEnum);
				}
				else {
					cliParam.Handle(argEnum);
				}

			}


		}
	}


	public record CliParameter
	{
		public string ParameterId { get; init; }

		public Func<string[], object> Function { get; init; }

		/// <remarks><c>0</c> for switch parameter</remarks>
		public int ArgumentCount { get; init; }


		internal void Handle(IEnumerator<string> argEnum)
		{

			Guard.AssertNonNegative(ArgumentCount);

			if (ArgumentCount == 0) {
				// switch
				Function(new[] {true.ToString()});
				return;
			}

			var argValues = new string[ArgumentCount];

			for (int i = 0; i < ArgumentCount; i++) {
				argEnum.MoveNext();
				argValues[i] = argEnum.Current;
			}

			Trace.WriteLine($"{nameof(CliHandler)}: invoking {ParameterId} with {argValues.QuickJoin()}",
			                C_VERBOSE);

			Function(argValues);

			//((FieldInfo)((MemberExpression)reg.fx.Body).Member).SetValue();
		}
	}
}