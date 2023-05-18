using System.Runtime.Serialization;

using Borealis.Communication.Messages;
using Borealis.Portal.Domain.Common;
using Borealis.Portal.Domain.Devices.Models;
using Borealis.Portal.Infrastructure.Communication;
using Borealis.Shared.Extensions;

using Google.FlatBuffers;

using UnitsNet;

using LedstripChip = Borealis.Portal.Domain.Ledstrips.Models.LedstripChip;
using LedstripStatus = Borealis.Portal.Domain.Devices.Models.LedstripStatus;



namespace Borealis.Portal.Infrastructure.Connectivity.Serialization;


internal class MessageSerializer
{
	public MessageSerializer() { }


	public virtual CommunicationPacket SerializeSetConfigurationRequest(string configurationConcurrencyToken, IEnumerable<DevicePort> ports)
	{
		FlatBufferBuilder builder = new FlatBufferBuilder(4096);

		SetConfigurationRequestT request = new SetConfigurationRequestT
		{
			ConcurrencyToken = configurationConcurrencyToken,
			Configuration = new ConfigurationMessageT
			{
				Ledstrips = ports.Where(x => x.Ledstrip != null)
								 .Select(x => new LedstripMessageT
								  {
									  BusId = x.Bus,
									  LedstripId = x.Ledstrip!.Id.ToString(),
									  PixelCount = (UInt16)x.Ledstrip.Length,
									  Chip = x.Ledstrip.Chip switch
									  {
										  LedstripChip.WS2812B => Borealis.Communication.Messages.LedstripChip.WS2812B,
										  LedstripChip.WS2813  => Borealis.Communication.Messages.LedstripChip.WS2813,
										  LedstripChip.WS2815  => Borealis.Communication.Messages.LedstripChip.WS2815,
										  LedstripChip.SK6812  => Borealis.Communication.Messages.LedstripChip.SK6812,
										  LedstripChip.SK9822  => Borealis.Communication.Messages.LedstripChip.SK9822,
										  LedstripChip.APA102  => Borealis.Communication.Messages.LedstripChip.APA102,
										  _                    => Borealis.Communication.Messages.LedstripChip.DEFAULT
									  }
								  })
								 .ToList()
			}
		};

		builder.Finish(SetConfigurationRequest.Pack(builder, request).Value);

		return new CommunicationPacket(PacketIdentifier.SetConfigurationRequest, builder.SizedByteArray());
	}


	public virtual void DeserializeSetConfigurationReplyPacket(CommunicationPacket packet, out bool success, out string? errorMessage)
	{
		if (packet.Identifier != PacketIdentifier.SetConfigurationReply) throw InvalidPacketIdentifierThrowHelper();

		SetConfigurationReply reply = SetConfigurationReply.GetRootAsSetConfigurationReply(packet.GetBuffer());
		success = reply.Success;
		errorMessage = reply.ErrorMessage;
	}


	public virtual void DeserializeErrorReplyPacket(CommunicationPacket packet, out string errorMessage)
	{
		if (packet.Identifier != PacketIdentifier.ErrorReply) throw InvalidPacketIdentifierThrowHelper();

		ErrorReply reply = ErrorReply.GetRootAsErrorReply(packet.GetBuffer());
		errorMessage = reply.Message;
	}


	public virtual void DeserializeAnimationBufferRequestPacket(CommunicationPacket packet, out Guid ledstripId, out int amount)
	{
		if (packet.Identifier != PacketIdentifier.AnimationBufferRequest) throw InvalidPacketIdentifierThrowHelper();

		AnimationBufferRequest request = AnimationBufferRequest.GetRootAsAnimationBufferRequest(packet.GetBuffer());
		ledstripId = request.LedstripId.ToGuid();
		amount = request.FrameCount;
	}


	public virtual CommunicationPacket SerializeGetDriverStatusRequestPacket(Guid? ledstripId = null)
	{
		FlatBufferBuilder builder = new FlatBufferBuilder(128);

		StringOffset ledstripIdOffset = builder.CreateString(ledstripId?.ToString() ?? string.Empty);

		GetDriverStatusRequest.StartGetDriverStatusRequest(builder);

		GetDriverStatusRequest.AddLedstripId(builder, ledstripIdOffset);

		builder.Finish(GetDriverStatusRequest.EndGetDriverStatusRequest(builder).Value);

		return new CommunicationPacket(PacketIdentifier.GetDriverStatusRequest, builder.SizedByteArray());
	}


	public virtual CommunicationPacket SerializeStartAnimationRequest(Guid ledstripId, Frequency frequency, IEnumerable<ReadOnlyMemory<PixelColor>> initialFrameBuffer)
	{
		ReadOnlyMemory<PixelColor>[] frameBuffer = initialFrameBuffer.ToArray();

		FlatBufferBuilder builder = new FlatBufferBuilder(128 + 3072 * frameBuffer.Length);

		// Frames
		Offset<FrameMessage>[] frameMessages = new Offset<FrameMessage>[frameBuffer.Length];
		for (int i = 0; i < frameBuffer.Length; i++)
		{
			VectorOffset offset = builder.CreateVectorOfTables(BuildFrame(builder, frameBuffer[i]));
			frameMessages[i] = FrameMessage.CreateFrameMessage(builder, offset);
		}

		// Ledstrip Id
		StringOffset ledstripIdOffset = builder.CreateString(ledstripId.ToString());

		StartAnimationRequest.StartStartAnimationRequest(builder);

		StartAnimationRequest.AddLedstripId(builder, ledstripIdOffset);
		StartAnimationRequest.AddFrequency(builder, (float)frequency.Hertz);

		VectorOffset initialFrameBufferOffset = StartAnimationRequest.CreateInitialFrameBufferVector(builder, frameMessages);
		StartAnimationRequest.AddInitialFrameBuffer(builder, initialFrameBufferOffset);

		builder.Finish(StartAnimationRequest.EndStartAnimationRequest(builder).Value);

		return new CommunicationPacket(PacketIdentifier.StartAnimationRequest, builder.SizedByteArray());
	}


	public virtual void DeserializeGetDriverStatusRequest(CommunicationPacket packet, out IReadOnlyDictionary<Guid, LedstripStatus> ledstripStatuses)
	{
		if (packet.Identifier != PacketIdentifier.GetDriverStatusReply) throw InvalidPacketIdentifierThrowHelper();

		GetDriverStatusReply reply = GetDriverStatusReply.GetRootAsGetDriverStatusReply(packet.GetBuffer());

		Dictionary<Guid, LedstripStatus> statuses = new Dictionary<Guid, LedstripStatus>();

		for (int i = 0; i < reply.LedstripStatusesLength; i++)
		{
			LedstripStatusMessage message = reply.LedstripStatuses(i) ?? throw new SerializationException("The index was not found.");
			statuses.Add(message.LedstripId.ToGuid(),
						 message.LedstripStatus switch
						 {
							 Borealis.Communication.Messages.LedstripStatus.IDLE             => LedstripStatus.Idle,
							 Borealis.Communication.Messages.LedstripStatus.DISPALYING_FRAME => LedstripStatus.DisplayingColor,
							 Borealis.Communication.Messages.LedstripStatus.PAUSED           => LedstripStatus.PausedAnimation,
							 Borealis.Communication.Messages.LedstripStatus.PLAYING          => LedstripStatus.PlayingAnimation,
							 _                                                               => throw new ArgumentOutOfRangeException(nameof(message.LedstripStatus), "The ledstrip status is out of range.")
						 });
		}

		ledstripStatuses = statuses.AsReadOnly();
	}


	public virtual CommunicationPacket SerializePauseAnimationRequest(Guid ledstripId)
	{
		FlatBufferBuilder builder = new FlatBufferBuilder(128);

		StringOffset ledstripIdOffset = builder.CreateString(ledstripId.ToString());

		builder.Finish(PauseAnimationRequest.CreatePauseAnimationRequest(builder, ledstripIdOffset).Value);

		return new CommunicationPacket(PacketIdentifier.PauseAnimationRequest, builder.SizedByteArray());
	}


	public virtual CommunicationPacket SerializeStopAnimationRequest(Guid ledstripId)
	{
		FlatBufferBuilder builder = new FlatBufferBuilder(128);

		StringOffset ledstripIdOffset = builder.CreateString(ledstripId.ToString());

		builder.Finish(StopAnimationRequest.CreateStopAnimationRequest(builder, ledstripIdOffset).Value);

		return new CommunicationPacket(PacketIdentifier.StopAnimationRequest, builder.SizedByteArray());
	}


	public virtual CommunicationPacket SerializeDisplayFrameRequest(Guid ledstripId, ReadOnlyMemory<PixelColor> frame)
	{
		FlatBufferBuilder builder = new FlatBufferBuilder(3072);

		StringOffset ledstripIdOffset = builder.CreateString(ledstripId.ToString());

		SetLedstripColorRequest.StartSetLedstripColorRequest(builder);

		VectorOffset frameMessageOffset = FrameMessage.CreatePixelsVector(builder, BuildFrame(builder, frame));

		SetLedstripColorRequest.AddLedstripId(builder, ledstripIdOffset);
		SetLedstripColorRequest.AddFrame(builder, FrameMessage.CreateFrameMessage(builder, frameMessageOffset));

		builder.Finish(SetLedstripColorRequest.EndSetLedstripColorRequest(builder).Value);

		return new CommunicationPacket(PacketIdentifier.SetLedstripColorRequest, builder.SizedByteArray());
	}


	public virtual CommunicationPacket SerializeClearLedstripRequest(Guid ledstripId)
	{
		FlatBufferBuilder builder = new FlatBufferBuilder(128);

		StringOffset ledstripIdOffset = builder.CreateString(ledstripId.ToString());

		builder.Finish(ClearLedstripRequest.CreateClearLedstripRequest(builder, ledstripIdOffset).Value);

		return new CommunicationPacket(PacketIdentifier.ClearLedstripRequest, builder.SizedByteArray());
	}


	public virtual CommunicationPacket SerializeAnimationBufferReply(IEnumerable<ReadOnlyMemory<PixelColor>> frames)
	{
		ReadOnlyMemory<PixelColor>[] frameBuffer = frames.ToArray();

		FlatBufferBuilder builder = new FlatBufferBuilder(3072 * frameBuffer.Length);

		// Frames
		Offset<FrameMessage>[] frameMessages = new Offset<FrameMessage>[frameBuffer.Length];
		for (int i = 0; i < frameBuffer.Length; i++)
		{
			VectorOffset offset = builder.CreateVectorOfTables(BuildFrame(builder, frameBuffer[i]));
			frameMessages[i] = FrameMessage.CreateFrameMessage(builder, offset);
		}

		AnimationBufferReply.StartAnimationBufferReply(builder);

		VectorOffset initialFrameBufferOffset = AnimationBufferReply.CreateFrameBufferVector(builder, frameMessages);
		AnimationBufferReply.AddFrameBuffer(builder, initialFrameBufferOffset);

		builder.Finish(AnimationBufferReply.EndAnimationBufferReply(builder).Value);

		return new CommunicationPacket(PacketIdentifier.AnimationBufferReply, builder.SizedByteArray());
	}


	public virtual CommunicationPacket SerializeConnectRequest(string configurationConcurrencyToken)
	{
		FlatBufferBuilder builder = new FlatBufferBuilder(256);

		StringOffset tokenOffset = builder.CreateString(configurationConcurrencyToken);

		builder.Finish(ConnectRequest.CreateConnectRequest(builder, tokenOffset).Value);

		return new CommunicationPacket(PacketIdentifier.ConnectRequest, builder.SizedByteArray());
	}


	public virtual void DeserializeConnectReply(CommunicationPacket packet, out bool configurationConcurrencyTokenChanged)
	{
		if (packet.Identifier != PacketIdentifier.ConnectReply) throw InvalidPacketIdentifierThrowHelper();

		ConnectReply reply = ConnectReply.GetRootAsConnectReply(packet.GetBuffer());

		configurationConcurrencyTokenChanged = reply.ConfigurationConcurrencyTokenValid;
	}


	private static Offset<PixelMessage>[] BuildFrame(FlatBufferBuilder builder, ReadOnlyMemory<PixelColor> frame)
	{
		Offset<PixelMessage>[] frameMessage = new Offset<PixelMessage>[frame.Length];

		for (int pixelIndex = 0; pixelIndex < frame.Length; pixelIndex++)
		{
			PixelColor pixel = frame.Span[pixelIndex];
			frameMessage[pixelIndex] = PixelMessage.CreatePixelMessage(builder, pixel.R, pixel.G, pixel.B, pixel.W);
		}

		return frameMessage;
	}


	private static InvalidOperationException InvalidPacketIdentifierThrowHelper()
	{
		return new InvalidOperationException("Cannot deserialize packet because it has an other identifier was expected.");
	}
}