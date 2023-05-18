using System;
using System.Linq;



namespace Borealis.Drivers.Rpi.Commands;


public interface IQueryHandler<in TCommand, TQuery>
{
    Task<TQuery> Execute(TCommand command);
}