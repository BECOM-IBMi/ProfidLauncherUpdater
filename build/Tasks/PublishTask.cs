using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Frosting;

namespace Build.Tasks;

[TaskName("Publish")]
[IsDependentOn(typeof(TestTask))]
public sealed class PublishTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context
            .DotNetPublish("../src/ProfidLauncherUpdater/ProfidLauncherUpdater.csproj", new DotNetPublishSettings
            {
                OutputDirectory = "../artifacts",
                SelfContained = true,
                PublishSingleFile = true,
                Runtime = "win-x64",
                PublishReadyToRun = true,
            });

    }
}
