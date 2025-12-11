using Sandbox;
using Sandbox.Diagnostics;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using VNScript;

// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable VirtualMemberCallInConstructor

namespace VNBase.Assets;

/// <summary>
/// Defines a VNBase script asset.
/// </summary>
public class Script : IAsset
{
	// ReSharper disable once MemberCanBeProtected.Global
	public virtual string Code { get; set; } = string.Empty;
	
	/// <summary>
	/// The script to run after this one has finished.
	/// </summary>
	public virtual Script? NextScript { get; set; }
	
	/// <summary>
	/// If this script is initialized from a file,
	/// this is the path to that script file.
	/// </summary>
	[FilePath]
	public string Path { get; set; } = string.Empty;
	
	/// <summary>
	/// If this script was initialized from a file or not.
	/// </summary>
	public bool FromFile => !string.IsNullOrEmpty( Path );
	
	/// <summary>
	/// Called when a choice is selected from this script.
	/// </summary>
	[Hide]
	public Action<VNScript.Script.Choice>? OnChoiceSelected { get; set; }
	
	[Hide]
	private IEnvironment? _environment;
	
	[JsonIgnore, Hide]
	private static readonly Logger Log = new( "Script" );
	
	/// <summary>
	/// Create a new empty script.
	/// </summary>
	public Script() { }
	
	/// <summary>
	/// Create a new script from a file.
	/// </summary>
	/// <param name="path">The path to the script file.</param>
	public Script( string path )
	{
		if ( !FileSystem.Mounted.FileExists( path ) )
		{
			Log.Error( $"Unable to load script! Script file couldn't be found by path: {path}" );
			
			return;
		}
		
		Code = FileSystem.Mounted.ReadAllText( path );
		Path = path;
	}
	
	/// <summary>
	/// Create a new script from a file.
	/// </summary>
	/// <param name="path">The path to the script file.</param>
	/// <param name="nextScript">The next script to run after this one has finished.</param>
	public Script( string path, Script nextScript ) : this( path )
	{
		NextScript = nextScript;
	}
	
	/// <summary>
	/// Called when the script is loaded by the <see cref="ScriptPlayer"/>
	/// </summary>
	public virtual void OnLoad() { }
	
	/// <summary>
	/// Called after the script has finished executing by the <see cref="ScriptPlayer"/>
	/// </summary>
	public virtual void OnUnload() { }
	
	/// <summary>
	/// Get this scripts local environment map.
	/// </summary>
	public virtual IEnvironment GetEnvironment()
	{
		return _environment ??= new EnvironmentMap( new Dictionary<string, Value>() );
	}
	
	internal VNScript.Script Parse()
	{
		var codeBlocks = SParen.ParseText( Code ).ToList();
		return VNScript.Script.ParseScript( codeBlocks );
	}
}
