﻿using System.Collections.Generic;
using VNBase.Assets;

namespace VNScript;

public partial class Script
{
	public class Label
	{
		public string Name { get; set; } = string.Empty;

		public List<Dialogue> Dialogues { get; set; } = [];

		public List<Character> Characters { get; set; } = [];

		public List<Choice> Choices { get; set; } = [];

		public List<IAsset> Assets { get; set; } = [];

		public Input? ActiveInput { get; set; }

		public After? AfterLabel { get; set; }

		internal IEnvironment Environment { get; set; } = new EnvironmentMap();
	}
}
