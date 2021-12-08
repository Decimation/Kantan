using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Kantan.Cli.Controls;

// ReSharper disable UnusedMember.Global

namespace Kantan.Model;

public interface IConsoleComponent
{
	

	public ConsoleOption GetConsoleOption();
}