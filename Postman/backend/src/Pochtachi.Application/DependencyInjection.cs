using Microsoft.Extensions.DependencyInjection;
using Pochtachi.Application.Collections;
using Pochtachi.Application.Folders;
using Pochtachi.Application.History;
using Pochtachi.Application.ImportExport;
using Pochtachi.Application.Requests;
using Pochtachi.Application.Switches;
using Pochtachi.Application.Workspaces;

namespace Pochtachi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IWorkspaceService, WorkspaceService>();
        services.AddScoped<ICollectionService, CollectionService>();
        services.AddScoped<IFolderService, FolderService>();
        services.AddScoped<IRequestService, RequestService>();
        services.AddScoped<ISwitchDimensionService, SwitchDimensionService>();
        services.AddScoped<IVariableService, VariableService>();
        services.AddScoped<IHistoryService, HistoryService>();
        services.AddScoped<IPostmanImportExportService, PostmanImportExportService>();

        return services;
    }
}
