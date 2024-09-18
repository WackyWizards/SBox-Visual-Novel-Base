using Sandbox;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using VNBase.Assets;
using VNBase.UI;
using SandLang;

namespace VNBase;

/// <summary>
/// Responsible for handling visual novel base scripts.
/// </summary>
[Title( "VN Script Player" )]
[Category( "VNBase" )]
public sealed partial class ScriptPlayer : Component
{
	/// <summary>
	/// If not empty, will load the script at this path on initial component start.
	/// </summary>
	[Property, Group( "Script" )] public string? InitialScript { get; set; }

	/// <summary>
	/// The currently active script.
	/// </summary>
	[Property, Group( "Script" ), ReadOnly] public Script? ActiveScript { get; private set; }

	/// <summary>
	/// The currently active script label.
	/// </summary>
	[Property, Group( "Script" ), ReadOnly] public Dialogue.Label? ActiveLabel { get; private set; }

	/// <summary>
	/// The currently active script label text.
	/// </summary>
	[Property, Group( "Dialogue" )] public string? DialogueText { get; set; }

	/// <summary>
	/// Path to the currently active background image.
	/// </summary>
	[Property, Group( "Dialogue" )] public string? Background { get; set; }

	/// <summary>
	/// If the dialogue has finished writing text.
	/// </summary>
	[Property, Group( "Dialogue" )] public bool DialogueFinished { get; set; }

	/// <summary>
	/// The currently active speaking character.
	/// </summary>
	[Property, Group( "Characters" )] public Character? SpeakingCharacter { get; set; }

	/// <summary>
	/// Characters to display for this label.
	/// </summary>
	[Property, Group( "Characters" )] public List<Character> Characters { get; set; } = new();

	[Property, RequireComponent] public Settings? Settings { get; set; }

	[Property, RequireComponent] public VNHud? Hud { get; set; }

	private Dialogue? _activeDialogue;

	/// <summary>
	/// The script environment.
	/// Will be empty if there is no active dialogue.
	/// </summary>
	private IEnvironment _environment = new EnvironmentMap();

	private CancellationTokenSource? _cts;

	protected override void OnStart()
	{
		if ( !string.IsNullOrEmpty( InitialScript ) )
		{
			LoadScript( InitialScript );
		}

		if ( !Scene.GetAllComponents<VNHud>().Any() )
		{
			Log.Warning( "No VNHud Component found, ScriptPlayer will not be immediately visible!" );
		}
	}

	private bool SkipActionPressed => Settings?.SkipActions.Any( x => Input.Pressed( x.Action ) ) ?? false;

	protected override void OnUpdate()
	{
		if ( !Settings?.SkipActionEnabled ?? false )
		{
			return;
		}

		if ( SkipActionPressed )
		{
			if ( ActiveScript is null || ActiveLabel is null )
			{
				return;
			}

			if ( !DialogueFinished )
			{
				SkipDialogue();
			}
			else if ( !DialogueChoices.Any() )
			{
				ExecuteAfterLabel();
			}
		}
	}

	/// <summary>
	/// Read and load the script at the provided path.
	/// </summary>
	/// <param name="path">Path to the script to load.</param>
	public void LoadScript( string path )
	{
		var dialogue = FileSystem.Mounted.ReadAllText( path );

		if ( dialogue is null )
		{
			Log.Error( $"Unable to load script! Script file couldn't be found by path: {path}" );
			return;
		}

		if ( !string.IsNullOrEmpty( dialogue ) )
		{
			Script script = new( path );
			LoadScript( script );
		}
		else
		{
			Log.Error( "Unable to load script! The script file is empty." );
		}
	}

	/// <summary>
	/// Load the provided Script object.
	/// </summary>
	/// <param name="script">Script to load.</param>
	public void LoadScript( Script script )
	{
		if ( script is null )
		{
			Log.Error( "Unable to load script! Script is null!" );
			return;
		}

		string? scriptName = string.Empty;
		if ( script.FromFile )
		{
			scriptName = Path.GetFileNameWithoutExtension( script.Path );
		}

		Log.Info( $"Loading script: {scriptName}" );

		ActiveScript = script;
		script.OnLoad();

		List<SParen> codeblocks = SParen.ParseText( ActiveScript.Dialogue ).ToList();
		_activeDialogue = Dialogue.ParseDialogue( codeblocks );

		SetEnvironment( _activeDialogue );
		SetLabel( _activeDialogue.InitialLabel );
	}

	/// <summary>
	/// Unloads the currently active script.
	/// </summary>
	public void UnloadScript()
	{
		if ( ActiveScript is null )
		{
			return;
		}

		_activeDialogue = null;
		DialogueText = null;
		SpeakingCharacter = null;
		Background = null;
		DialogueChoices.Clear();

		ActiveScript.OnUnload();
		if ( ActiveScript.NextScript is not null )
		{
			LoadScript( ActiveScript.NextScript );
		}
		else
		{
			ActiveScript = null;
		}

		Log.Info( $"Unloaded active script." );
	}

	/// <summary>
	/// Skip the currently active text effect.
	/// </summary>
	public void SkipDialogue()
	{
		if ( !DialogueFinished && _cts is not null )
		{
			_cts.Cancel();
		}
	}
}
