using System;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using MSBuildProject = Microsoft.Build.Evaluation.Project;

namespace IceBuilder
{
    class Builder
    {
        public static bool Build(IVsBuildManagerAccessor2 accessor, 
                                 MSBuildProject project, 
                                 BuildCallback buildCallback,
                                 BuildLogger buildLogger)
        {
            if (accessor.ClaimUIThreadForBuild() != VSConstants.S_OK)
            {
                return false;
            }

            try
            {
                BuildManager manager = BuildManager.DefaultBuildManager;

                manager.BeginBuild(new BuildParameters
                {
                    Loggers = new[] 
                    {
                        buildLogger
                    }
                });

                BuildRequestData buildRequest = new BuildRequestData(
                        project.CreateProjectInstance(),
                        new String[] { "IceBuilder_Compile" },
                        null,
                        BuildRequestDataFlags.ReplaceExistingProjectInstance);

                buildCallback.BeginBuild();
                manager.PendBuildRequest(buildRequest).ExecuteAsync((submission) =>
                    {
                        manager.EndBuild();
                        accessor.ReleaseUIThreadForBuild();
                        buildCallback.EndBuild(submission.BuildResult.OverallResult == BuildResultCode.Success);
                    }, 
                    null);
                return true;

            }
            catch(Exception)
            {
                accessor.ReleaseUIThreadForBuild();
                throw;
            }
        }
    }
}