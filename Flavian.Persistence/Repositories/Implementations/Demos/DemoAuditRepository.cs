using Flavian.Domain.Models.Demos;
using Flavian.Persistence.Data;
using Flavian.Persistence.Repositories.Implementations.Generic;
using Flavian.Persistence.Repositories.Interfaces.Demos;

namespace Flavian.Persistence.Repositories.Implementations.Demos;

public class DemoAuditRepository(FlavianDbContext db) : GenericRepository<DemoAudit>(db), IDemoAuditRepository;
