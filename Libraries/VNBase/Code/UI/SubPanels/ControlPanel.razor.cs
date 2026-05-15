using System;
using System.Linq;
using Sandbox;
using Sandbox.UI;

namespace VNBase.UI;

public partial class ControlPanel
{
	private Panel? ControlButtons { get; set; }

	private bool UIVisible { get; set; } = true;

    private bool IsAutomaticMode => Player?.IsAutomaticMode ?? false;
    private bool IsInputPressed => Settings?.HideUIInputs.Any( x => x.Pressed ) ?? false;
    
    private SubPanel? _activeSubPanel;
    
    public override void Tick()
    {
        if ( IsInputPressed )
        {
            ToggleUI();
        }
    }

    private void Skip()
    {
        Player?.Skip();
    }

    private void ToggleAutomaticMode()
    {
        if ( !Player.IsValid() )
        {
            return;
        }

        Player.IsAutomaticMode = !Player.IsAutomaticMode;
    }

    private void ToggleUI()
    {
        // Panels that should not be hidden
        var ignoredPanels = new[]
        {
            typeof( CharacterPortraits )
        };

        var panelsToHide = Parent.ChildrenOfType<SubPanel>()
            .Where( x => !ignoredPanels.Contains( x.GetType() ) )
            .Skip( 1 );

        foreach ( var panel in panelsToHide )
        {
            panel.ToggleVisibility();
        }

        UIVisible = !UIVisible;
    }

    private void ToggleHudSubPanel( string id )
    {
        var panel = GetHudSubPanelFromId( id );

        if ( !panel.IsValid() )
        {
            Log.Warning( $"Unable to find sub panel with id: {id}" );
            return;
        }

        if ( _activeSubPanel.IsValid() && _activeSubPanel != panel )
        {
            _activeSubPanel.Hide();
        }

        _activeSubPanel = panel;
        panel.ToggleVisibility();
    }

    private SubPanel? GetHudSubPanelFromId( string id )
    {
        if ( !Hud.IsValid() )
        {
            return null;
        }
        
        return Hud.GetSubPanels().SingleOrDefault( x => x.ElementName.Equals( id, StringComparison.CurrentCultureIgnoreCase ) );
    }

    protected override int BuildHash()
    {
        return HashCode.Combine( UIVisible, _activeSubPanel, Player?.CanSkip(), Player?.IsAutomaticModeAvailable, Player?.State.GetHashCode() );
    }
}
