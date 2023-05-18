namespace Borealis.Shared.Eventing;


public delegate Task AsyncEventHandler(object? sender, EventArgs e);



public delegate Task AsyncCancelableEventHandler(object? sender, EventArgs e, CancellationToken token);



public delegate Task AsyncEventHandler<in TEventArgs>(object? sender, TEventArgs e) where TEventArgs : EventArgs;



public delegate Task AsyncCancelableEventHandler<in TEventArgs>(object? sender, TEventArgs e, CancellationToken token) where TEventArgs : EventArgs;