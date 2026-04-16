using HotChocolate;
using HotChocolate.Types;
using Flavian.Application.Services.Interfaces.Demos;
using Flavian.Shared.Dto;

namespace Flavian.Application.Resolvers.Demo;

[ExtendObjectType(Name = "Query")]
public class DemoQuery
{
    public async Task<StandardResponse> GetDemos(
        int? pageNumber,
        int? pageSize,
        [Service] IDemoService service) =>
        await service.GetAllAsync(pageNumber, pageSize);

    public async Task<StandardResponse> GetDemoById(
        Guid id,
        [Service] IDemoService service) =>
        await service.GetByIdAsync(id);
}
