using System;
using System.Linq;

using Borealis.Domain.Ledstrips;
using Borealis.Portal.Domain.Animations;
using Borealis.Portal.Domain.Common;
using Borealis.Portal.Domain.Connectivity.Connections;
using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Core.Ledstrips.State;


internal class LedstripDisplayState
{
    /// <summary>
    /// The current status of the ledstrip.
    /// </summary>
    public LedstripStatus Status
    {
        get
        {
            if (ColorDisplaying != null)
            {
                return LedstripStatus.DisplayingColor;
            }

            if (AnimationPlayer != null)
            {
                return AnimationPlayer.IsRunning ? LedstripStatus.PlayingAnimation : LedstripStatus.PausedAnimation;
            }

            return LedstripStatus.Idle;
        }
    }

    /// <summary>
    /// The color that we are displaying on the ledstrip.
    /// </summary>
    public PixelColor? ColorDisplaying { get; set; }

    /// <summary>
    /// The connection of the ledstrip.
    /// </summary>
    public ILedstripConnection Connection { get; }


    /// <summary>
    /// The ledstrip that we ware connected to.
    /// </summary>
    public Ledstrip Ledstrip => Connection.Ledstrip;

    /// <summary>
    /// The playing animation on the ledstrip.
    /// </summary>
    public IAnimationPlayer? AnimationPlayer { get; protected set; }


    /// <summary>
    /// A state object that holds the current condition of the ledstrip.
    /// </summary>
    /// <param name="connection"> The <see cref="ILedstripConnection" /> to the ledstrip used to control the ledstrip. </param>
    public LedstripDisplayState(ILedstripConnection connection)
    {
        Connection = connection;
    }


    /// <summary>
    /// Sets the state so we have a animation playing.
    /// </summary>
    /// <param name="animationPlayer"> The <see cref="IAnimationPlayer" /> used for playing the animation on the ledstrip. </param>
    /// <exception cref="InvalidOperationException"> When we try and set a state that the ledstrip already is busy with. </exception>
    public void SetAnimationPlayer(IAnimationPlayer animationPlayer)
    {
        if (Status != LedstripStatus.Idle) throw new InvalidOperationException("Cannot add animation player to a state that already owns an animation player.");

        AnimationPlayer = animationPlayer;
    }


    /// <summary>
    /// Sets up the ledstrip that we ware displaying a state where we show a single color.
    /// </summary>
    /// <param name="color"> The color that we ware displaying. </param>
    /// <exception cref="InvalidOperationException"> When we try and set a state that the ledstrip already is busy with. </exception>
    public void SetDisplayingColor(PixelColor color)
    {
        if (Status != LedstripStatus.Idle) throw new InvalidOperationException("Cannot add displaying color to a state that is already busy with something.");

        ColorDisplaying = color;
    }


    /// <summary>
    /// Sets the state that we cleared the color on the ledstrip.
    /// </summary>
    public void ColorCleared()
    {
        ColorDisplaying = null;
    }


    /// <summary>
    /// Sets the state so we removed the animation player that is connected to the state.
    /// </summary>
    public void RemoveAnimationPlayer()
    {
        AnimationPlayer = null;
        ;
    }
}