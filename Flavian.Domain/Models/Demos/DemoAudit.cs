using System.Text.Json.Serialization;
using Flavian.Domain.Models.Common;

namespace Flavian.Domain.Models.Demos;

public class DemoAudit : BaseAuditEntity
{
    [JsonIgnore]
    public Demo? Demo { get; set; }
}