namespace Borealis.Drivers.Rpi.Udp.Commands;


public interface ICommandHandler<in TCommand>
{
    Task ExecuteAsync(TCommand command);
}