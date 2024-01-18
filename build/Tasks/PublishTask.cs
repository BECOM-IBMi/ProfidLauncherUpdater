using Cake.Common.Tools.GitVersion;
using Cake.Frosting;
using System;

namespace Build.Tasks;

[TaskName("Publish")]
[IsDependentOn(typeof(TestTask))]
public sealed class PublishTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var v = context.GitVersion();

        Console.WriteLine(v.SemVer);

        //context
        //    .DotNetPublish("../src/ProfidLauncherUpdater/ProfidLauncherUpdater.csproj", new DotNetPublishSettings
        //    {
        //        OutputDirectory = "../artifacts",
        //        SelfContained = true,
        //        PublishSingleFile = true,
        //        Runtime = "win-x64",
        //        PublishReadyToRun = true,
        //        MSBuildSettings = new Cake.Common.Tools.DotNet.MSBuild.DotNetMSBuildSettings
        //        {
        //            Version = "1.0.1",
        //            AssemblyVersion = "1.0.1",
        //            FileVersion = "1.0.1",
        //            InformationalVersion = "1.0.1",
        //        }
        //    });

    }
}
