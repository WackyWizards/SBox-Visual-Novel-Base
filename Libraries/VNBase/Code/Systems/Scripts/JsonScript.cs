using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Sandbox;
using VNScript.Converters;

namespace VNBase.Assets;

public class JsonScript( string path ) : Script( path )
{
	internal override VNScript.Script Parse()
	{
		var scriptText = FileSystem.Mounted.ReadAllText( Path );
		
		if ( string.IsNullOrEmpty( scriptText ) )
		{
			return new VNScript.Script();
		}
		
		var options = new JsonSerializerOptions();
		options.Converters.Add( new FormattableTextConverter() );
		
		var model = JsonSerializer.Deserialize<Script>( scriptText, options );
		var script = new VNScript.Script();
		
		if ( model is null )
		{
			return new VNScript.Script();
		}
		
		foreach ( var label in model.Labels ?? [] )
		{
			if ( string.IsNullOrEmpty( label.Name ) )
			{
				continue;
			}
			
			var scriptLabel = new VNScript.Script.Label
			{
				Name = label.Name
			};
			
			script.Labels[scriptLabel.Name] = scriptLabel;
			
			foreach ( var line in label.Lines ?? [] )
			{
				if ( string.IsNullOrEmpty( line.Type ) )
				{
					continue;
				}
				
				switch ( line.Type )
				{
					case "dialogue":
						{
							if ( line.Text is null || line.Text.Count == 0 )
							{
								break;
							}
							
							foreach ( var text in line.Text )
							{
								var dialog = new VNScript.Script.Dialogue
								{
									Text = text
								};
								
								scriptLabel.Dialogues.Add( dialog );
							}
							
							break;
						}
					case "choice":
						{
							if ( line.Choices is null || line.Choices.Count == 0 )
							{
								break;
							}
							
							foreach ( var scriptChoice in line.Choices )
							{
								var choice = new VNScript.Script.Choice();
								
								if ( !string.IsNullOrEmpty( scriptChoice.Text ) )
								{
									choice.Text = scriptChoice.Text;
								}
								
								if ( !string.IsNullOrEmpty( scriptChoice.TargetLabel ) )
								{
									choice.TargetLabel = scriptChoice.TargetLabel;
								}
								
								scriptLabel.Choices.Add( choice );
							}
							
							break;
						}
				}
			}
			
			if ( label.After is not null )
			{
				scriptLabel.AfterLabel = label.After;
			}
		}
		
		if ( model.InitialLabel is not null )
		{
			script.InitialLabel = script.Labels[model.InitialLabel];
		}
		
		return script;
	}
	
	private class Script
	{
		[JsonPropertyName( "initialLabel" )]
		public string? InitialLabel { get; set; }
		
		[JsonPropertyName( "labels" )]
		public List<Label>? Labels { get; set; }
	}
	
	private class Label
	{
		[JsonPropertyName( "name" )]
		public string? Name { get; set; }
		
		[JsonPropertyName( "lines" )]
		public List<Line>? Lines { get; set; }
		
		[JsonPropertyName( "after" )]
		public VNScript.Script.After? After { get; set; }
	}
	
	private class Line
	{
		[JsonPropertyName( "type" )]
		public string? Type { get; set; }
		
		[JsonPropertyName( "text" )]
		public List<string>? Text { get; set; }
		
		[JsonPropertyName( "choices" )]
		public List<VNScript.Script.Choice>? Choices { get; set; }
	}
}
