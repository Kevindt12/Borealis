using System;
using System.Linq;



namespace Borealis.Networking.Communication;


/// <summary>
/// The type of packet this is.
/// </summary>
public enum PacketIdentifier : byte
{
	ConnectRequest = 1,

	ConnectReply = 2,


	SetConfigurationRequest = 10,

	SetConfigurationReply = 11,


	GetDriverStatusRequest = 20,

	GetDriverStatusReply = 21,


	StartAnimationRequest = 100,

	StopAnimationRequest = 110,

	AnimationBufferRequest = 120,

	AnimationBufferReply = 121,

	DisplayFrameRequest = 130,


	ClearLedstripRequest = 150,


	SuccessReply = 254,

	ErrorReply = 255
}