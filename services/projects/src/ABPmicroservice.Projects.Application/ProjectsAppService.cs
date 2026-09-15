using ABPmicroservice.Projects.Localization;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Projects;

public abstract class ProjectsAppService : ApplicationService
{
    protected ProjectsAppService()
    {
        LocalizationResource = typeof(ProjectsResource);
        ObjectMapperContext = typeof(ProjectsApplicationModule);
    }
}
