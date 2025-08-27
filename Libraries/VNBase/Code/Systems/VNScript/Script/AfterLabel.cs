using Sandbox;
using System.Collections.Generic;

namespace VNScript;

public partial class Script
{
	/// <summary>
	/// Represents code to execute as well as the new label to direct towards.
	/// </summary>
	public class AfterLabel
	{
		public List<SParen> CodeBlocks { get; set; } = [];

		public bool IsLastLabel { get; set; }

		[FilePath]
		public string? ScriptPath { get; set; }

		public string? TargetLabel { get; set; }
	}
}
