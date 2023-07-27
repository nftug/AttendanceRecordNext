using Domain.Entities;

namespace Domain.Interfaces;

public interface IRestTimeRepository
{
    Task CreateAsync(RestTime entity);
    Task UpdateAsync(RestTime entity);
}
