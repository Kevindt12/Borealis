﻿using System.Net.Sockets;



namespace Borealis.Drivers.Rpi.Udp.Connections;


public class TcpClientHandlerFactory
{
    private readonly ILoggerFactory _loggerFactory;


    /// <summary>
    /// Factory for creating tcp client handlers.
    /// </summary>
    /// <param name="loggerFactory"> </param>
    public TcpClientHandlerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }


    /// <summary>
    /// Creates a tcp client handler.
    /// </summary>
    /// <param name="client"> </param>
    /// <returns> </returns>
    public TcpClientConnection CreateHandler(TcpClient client)
    {
        return new TcpClientConnection(_loggerFactory.CreateLogger<TcpClientConnection>(), client);
    }
}