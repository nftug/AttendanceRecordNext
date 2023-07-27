namespace Domain.Entities;

public interface IEntity<TSelf>
    where TSelf : IEntity<TSelf>
{
    Guid Id { get; }
    TSelf Recreate();
}