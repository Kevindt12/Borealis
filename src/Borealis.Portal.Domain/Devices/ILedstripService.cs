using Borealis.Domain.Animations;
using Borealis.Domain.Devices;
using Borealis.Domain.Effects;



namespace Borealis.Portal.Domain.Devices;


public interface ILedstripService
{
    bool IsLedstripBusy(Ledstrip ledstrip);


    Task StartAnimationOnLedstripAsync(Ledstrip ledstrip, Animation animation);

    Task StopLedstripAsync(Ledstrip ledstrip);

    Task TestLedstripAsync(Ledstrip ledstrip);

    Task SetSolidColorAsync(Ledstrip ledstrip, PixelColor color, CancellationToken token = default);

    Animation? GetAnimationPlayingOnLedstripOrDefault(Ledstrip ledstrip);
}