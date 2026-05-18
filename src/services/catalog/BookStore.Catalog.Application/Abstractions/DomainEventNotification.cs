using BookStore.SharedKernel.Abstractions;
using Mediator;

namespace BookStore.Catalog.Application.Abstractions;

/// <summary>
/// Wraps an <see cref="IDomainEvent"/> as a Mediator notification,
/// bridging the domain layer to the Mediator infrastructure.
/// </summary>
public sealed record DomainEventNotification(IDomainEvent DomainEvent) : INotification;
