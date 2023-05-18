using System;
using System.Linq;



namespace Borealis.Drivers.Rpi.Commands;


public interface ICommandHandler<in TCommand>
{
    Task ExecuteAsync(TCommand command);
}