using ABPmicroservice.Projects.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace ABPmicroservice.Projects;

public abstract class ProjectsController : AbpControllerBase
{
    protected ProjectsController()
    {
        LocalizationResource = typeof(ProjectsResource);
    }
}
