using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Frosting;

namespace Build.Tasks;

[TaskName("Compile")]
[IsDependentOn(typeof(RestoreTask))]
public sealed class CompileTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context
            .DotNetBuild("../ProfidLauncherUpdater.sln", new DotNetBuildSettings
            {
                NoRestore = true
            });
    }
}
