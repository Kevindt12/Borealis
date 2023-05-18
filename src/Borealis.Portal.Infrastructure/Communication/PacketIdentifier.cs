using System;
using System.Linq;



namespace Borealis.Portal.Infrastructure.Communication;


/// <summary>
/// The type of packet this is.
/// </summary>
internal enum PacketIdentifier : byte
{
    ConnectRequest = 1,

    ConnectReply = 2,

    SetConfigurationRequest = 5,

    SetConfigurationReply = 6,

    StartAnimationRequest = 100,

    PauseAnimationRequest = 105,

    StopAnimationRequest = 110,

    AnimationBufferRequest = 120,

    AnimationBufferReply = 121,

    SetLedstripColorRequest = 150,

    ClearLedstripRequest = 160,

    GetDriverStatusRequest = 250,

    GetDriverStatusReply = 251,

    SuccessReply = 254,

    ErrorReply = 255
}