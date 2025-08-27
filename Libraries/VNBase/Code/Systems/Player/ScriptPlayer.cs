using Sandbox;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using VNBase.UI;
using Script = VNScript.Script;

namespace VNBase;

/// <summary>
/// Responsible for handling visual novel base scripts.
/// </summary>
[Title( "VN Script Player" )]
[Category( "VNBase" )]
[Icon( "menu_book" )]
public sealed partial class ScriptPlayer : Component
{
	/// <summary>
	/// The currently active script.
	/// </summary>
	public Assets.Script? ActiveScript { get; private set; }

	/// <summary>
	/// The currently active script label.
	/// </summary>
	public Script.Label? ActiveLabel { get; private set; }

	/// <summary>
	/// If there is an active playing script.
	/// </summary>
	[Property]
	public bool IsScriptActive { get; private set; }

	/// <summary>
	/// If not empty, will load the script asset at this path on initial component start.
	/// </summary>
	[Property, Group( "Script" ), FilePath( Extension = "vnscript" )]
	public string? InitialScript { get; set; }

	/// <summary>
	/// The active <see cref="ScriptState"/>.
	/// </summary>
	/// <seealso cref="ScriptState"/>
	[Property, Group( "Script" )]
	public ScriptState State { get; } = new();

    /// <summary>
    /// Automatic mode moves through dialogues without choices automatically.
    /// </summary>
    [Property, Group( "Dialogue" )]
    public bool IsAutomaticMode { get; set; }

    /// <summary>
    /// If automatic mode can be enabled.
    /// </summary>
    [Property, Group( "Dialogue" )]
    public bool IsAutomaticModeAvailable { get; set; } = true;

	[Property, RequireComponent, Group( "Components" )]
	public VNHud? Hud { get; set; }

	[Property]
	public Settings Settings { get; } = new();

	private Script? _activeDialogue;
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

	private bool SkipActionPressed => Settings?.SkipActions.Any( x => x.Pressed ) ?? false;

	protected override void OnUpdate()
	{
		if ( ActiveScript is null || ActiveLabel is null )
		{
			return;
		}

		if ( !Settings?.SkipActionEnabled ?? false )
		{
			return;
		}

		if ( SkipActionPressed )
		{
			AdvanceOrSkipDialogueEffect();
		}
		else if ( IsAutomaticMode )
		{
			if ( State is { IsDialogueFinished: true, Choices.Count: 0 } )
			{
				AdvanceText();
			}
		}
	}

	/// <summary>
	/// Read and load the script at the provided path.
	/// </summary>
	/// <param name="path">Path to the script to load.</param>
	// ReSharper disable once MemberCanBePrivate.Global
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
			Assets.Script script = new( path );
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
	// ReSharper disable once MemberCanBePrivate.Global
	public void LoadScript( Assets.Script script )
	{
		var scriptName = string.Empty;
		if ( script.FromFile )
		{
			scriptName = Path.GetFileNameWithoutExtension( script.Path );
		}

		if ( LoggingEnabled )
		{
			Log.Info( $"Loading script: {scriptName}" );
		}

		if ( Settings?.StopMusicPlaybackOnUnload ?? true )
		{
			// Stop any playing background music.
			State.BackgroundMusic?.Stop();
		}

		ActiveScript = script;
		_activeDialogue = ActiveScript.Parse();

		script.OnLoad();
		OnScriptLoad?.Invoke( script );
		
		SetEnvironment( _activeDialogue );
		SetLabel( _activeDialogue.InitialLabel );
		
		IsScriptActive = true;
	}

	/// <summary>
	/// Unloads the currently active script.
	/// </summary>
	// ReSharper disable once MemberCanBePrivate.Global
	public void UnloadScript()
	{
		if ( ActiveScript is null || ActiveLabel is null )
		{
			return;
		}

		// Safety check. Should hopefully not cause issues.
		if ( ActiveScript.OnChoiceSelected is not null )
		{
			foreach ( var @delegate in ActiveScript.OnChoiceSelected.GetInvocationList() )
			{
				ActiveScript.OnChoiceSelected -= (Action<Script.Choice>)@delegate;
			}
		}

		State.Clear();
		ActiveScript.OnUnload();
		OnScriptUnload?.Invoke( ActiveScript );
		IsScriptActive = false;

		var nextScript = ActiveScript.NextScript;
		if ( nextScript is not null )
		{
			LoadScript( nextScript );
		}
		else
		{
			ActiveScript = null;
		}

		if ( LoggingEnabled )
		{
			Log.Info( $"Unloaded active script." );
		}
	}

	/// <summary>
	/// Skip the currently active text effect.
	/// </summary>
	// ReSharper disable once MemberCanBePrivate.Global
	public void SkipDialogueEffect()
	{
		if ( ActiveScript is null || ActiveLabel is null )
		{
			return;
		}

		if ( IsAutomaticMode )
		{
			return;
		}
		
		_cts?.Cancel();
		_cts?.Dispose();
		_cts = null;
	}

	/// <summary>
	/// If the dialogue isn't finished, skip the effect, otherwise just advance if we can.
	/// </summary>
	public void AdvanceOrSkipDialogueEffect()
	{
		if ( !State.IsDialogueFinished )
		{
			SkipDialogueEffect();
		}
		else if ( State.Choices.Count == 0 )
		{
			AdvanceText();
		}
	}
}
