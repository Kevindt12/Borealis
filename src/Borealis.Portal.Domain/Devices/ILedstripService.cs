using Borealis.Domain.Animations;
using Borealis.Domain.Devices;
using Borealis.Domain.Effects;



namespace Borealis.Portal.Domain.Devices;


public interface ILedstripService
{
    bool IsLedstripBusy(Device device, Ledstrip ledstrip);


    Task StartAnimationOnLedstripAsync(Device device, Ledstrip ledstrip, Animation animation);

    Task StopLedstripAsync(Device device, Ledstrip ledstrip);

    Task TestLedstripAsync(Device device, Ledstrip ledstrip);

    Task SetSolidColorAsync(Device device, Ledstrip ledstrip, PixelColor color, CancellationToken token = default);

    Animation? GetAnimationPlayingOnLedstripOrDefault(Device device, Ledstrip ledstrip);
}