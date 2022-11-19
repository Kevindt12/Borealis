using System;

using FluentValidation;

using System.Linq;

using Borealis.Domain.Devices;



namespace Borealis.Drivers.Rpi.Udp.Validation;


public class LedstripSettingsValidator : AbstractValidator<LedstripSettings>
{
    public LedstripSettingsValidator()
    {
        RuleForEach(p => p.Ledstrips).SetValidator(new LedstripValidator());
    }



    protected class LedstripValidator : AbstractValidator<Ledstrip>
    {
        public LedstripValidator()
        {
            // Making sure that the length is more then 0.
            RuleFor(p => p.Length).GreaterThan(0);

            RuleFor(p => p.Connection).NotNull().SetValidator(new ConnectionValidator()!);
        }
    }



    protected class ConnectionValidator : AbstractValidator<ConnectionSettings>
    {
        public ConnectionValidator()
        {
            //RuleFor(p => p.Spi).NotNull();
        }
    }
}