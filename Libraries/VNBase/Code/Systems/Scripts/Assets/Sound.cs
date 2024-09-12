using Sandbox;

namespace VNBase.Assets;

/// <summary>
/// A playable sound asset.
/// </summary>
public class SoundAsset : IAsset
{
	/// <summary>
	/// The name of the <see cref="SoundEvent"/> this asset is tied to.
	/// </summary>
	public string EventName { get; set; } = string.Empty;

	/// <summary>
	/// If this asset is constructed with a SoundEvent, this is that event. Otherwise null.
	/// </summary>
	public SoundEvent? Event { get; private set; }

	/// <summary>
	/// Handle to interface with the playing sound. If this asset isn't playing, this is null.
	/// </summary>
	public SoundHandle? Handle { get; private set; }

	/// <summary>
	/// If this asset is constructed with a SoundEvent, returns the path to the event on disk.
	/// Otherwise, returns an empty string.
	/// </summary>
	public string Path 
	{ 
		get
		{
			if ( Event is not null )
			{
				return Event.ResourcePath;
			}
			else
			{
				return _path;
			}
		}
		set
		{
			_path = value;
		}
	}

	private string _path = string.Empty;

	public SoundAsset( string eventName ) 
	{
		EventName = eventName;
	}

	public SoundAsset( SoundEvent soundEvent ) 
	{
		EventName = soundEvent.ResourceName;
		Event = soundEvent;
	}

	public void Play()
	{
		Handle = Sound.Play( EventName );
	}

	public void Stop( float fadeTime = 0 )
	{
		Handle?.Stop(fadeTime);
	}
}
