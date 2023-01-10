namespace Borealis.Drivers.Rpi.Udp.Commands;


public interface IQueryHandler<in TCommand, TQuery>
{
    Task<TQuery> Execute(TCommand command);
}