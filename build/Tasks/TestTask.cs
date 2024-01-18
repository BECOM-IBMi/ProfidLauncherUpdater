using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Test;
using Cake.Frosting;

namespace Build.Tasks;

[TaskName("Test")]
[IsDependentOn(typeof(CompileTask))]
public sealed class TestTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetTest("../ProfidLauncherUpdater.sln", new DotNetTestSettings { NoRestore = true });
    }
}
