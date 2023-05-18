using System;
using System.Linq;

using Borealis.Domain.Ledstrips;
using Borealis.Portal.Domain.Devices.Models;



namespace Borealis.Portal.Domain.Ledstrips.Managers;


/// <summary>
/// The managers the <see cref="Ledstrip" /> in the application.
/// </summary>
public interface ILedstripManager
{
    /// <summary>
    /// Gets all the ledstrips known to the application.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="IEnumerable{Ledstrip}" /> of all the ledstrips. </returns>
    Task<IEnumerable<Ledstrip>> GetLedstripsAsync(CancellationToken token = default);


    /// <summary>
    /// Gets all the ledstrips that are not connected to a <see cref="Device" />.
    /// </summary>
    /// <param name="token"> A token to cancel the current operation. </param>
    /// <returns> A <see cref="IEnumerable{Ledstrip}" /> of all the ledstrips that are not connected to anything. </returns>
    Task<IEnumerable<Ledstrip>> GetUnoccupiedLedstripsAsync(CancellationToken token = default);


    /// <summary>
    /// Saves the ledstrip.
    /// </summary>
    /// <param name="ledstrip"> The <see cref="ledstrip" /> that we want to save. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task SaveAsync(Ledstrip ledstrip, CancellationToken token = default);


    /// <summary>
    /// Deletes a ledstrip from the system.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip that we want to remove. </param>
    /// <param name="token"> A token to cancel the current operation. </param>
    Task DeleteAsync(Ledstrip ledstrip, CancellationToken token = default);
}