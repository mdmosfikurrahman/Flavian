using HotChocolate;
using HotChocolate.Types;
using Flavian.Application.Dto.Request.Demos;
using Flavian.Application.Services.Interfaces.Demos;
using Flavian.Shared.Dto;

namespace Flavian.Application.Resolvers.Demo;

[ExtendObjectType(Name = "Mutation")]
public class DemoMutation
{
    public async Task<StandardResponse> CreateDemo(
        DemoRequest input,
        [Service] IDemoService service) =>
        await service.AddAsync(input);

    public async Task<StandardResponse> UpdateDemo(
        Guid id,
        DemoRequest input,
        [Service] IDemoService service) =>
        await service.UpdateAsync(id, input);

    public async Task<StandardResponse> DeleteDemo(
        Guid id,
        [Service] IDemoService service) =>
        await service.DeleteAsync(id);
}
