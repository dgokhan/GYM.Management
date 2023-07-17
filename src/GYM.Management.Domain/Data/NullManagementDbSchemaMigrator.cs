﻿using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace GYM.Management.Data;

/* This is used if database provider does't define
 * IManagementDbSchemaMigrator implementation.
 */
public class NullManagementDbSchemaMigrator : IManagementDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
