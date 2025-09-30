using System;
using System.Linq;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.ReportGenerator;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    [Solution(SuppressBuildProjectCheck = true)]
    readonly Solution Solution;

    public static int Main () => Execute<Build>(x => x.All);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            DotNetClean(_ => _
                .SetProject(Solution));
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(_ => _
                .SetProjectFile(Solution)
                .EnableLockedMode());
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(_ => _
                .SetProjectFile(Solution)
                .SetNoRestore(FinishedTargets.Contains(Restore)));
        });

    Target Pack => _ => _
        .DependsOn(Compile)
        .Produces("*.nupkg", "*.snupkg")
        .Executes(() =>
        {
            var artifactStagingDirectory = RootDirectory / "dist";

            DotNetPack(_ => _
                .SetProject(Solution)
                .SetConfiguration(Configuration)
                .SetOutputDirectory(artifactStagingDirectory)
                .SetNoBuild(FinishedTargets.Contains(Compile)));
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Produces("*.trx", "*.xml")
        .Executes(() =>
        {
            var testResultDirectory = RootDirectory / "dist" / "TestResults";

            EnsureCleanDirectory(testResultDirectory);

            var testProjects = Solution.GetProjects("*Tests");

            DotNetTest(_ => _
                    .SetConfiguration(Configuration)
                    .SetNoBuild(FinishedTargets.Contains(Compile))
                    .SetNoRestore(FinishedTargets.Contains(Restore))
                    .ResetVerbosity()
                    .SetResultsDirectory(testResultDirectory)
                    .EnableCollectCoverage()
                    .SetDataCollector("XPlat Code Coverage")
                    .SetCoverletOutputFormat(CoverletOutputFormat.cobertura)
                    .CombineWith(testProjects, (_, v) => _
                        .SetProjectFile(v)
                        .SetLoggers($"trx;LogFileName={v.Name}.trx")),
                completeOnFailure: false);
        });

    Target All => _ => _
        .DependsOn(Clean)
        .DependsOn(Test)
        .DependsOn(Pack)
        .Executes(() =>
        {
        });

}
