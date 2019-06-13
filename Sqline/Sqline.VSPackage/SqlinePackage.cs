// Authors="Daniel Jonas Møller, Anders Eggers-Krag" License="New BSD License http://sqline.codeplex.com/license"
using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE80;
using EnvDTE;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using System.Threading;
using Microsoft.VisualStudio.Shell;

namespace Sqline.VSPackage {
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is a package.
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]
    [Guid(GuidList.guidSqlinePkgString)]
	//[ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
	//[ProvideAutoLoad("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}")]
	public sealed class SqlinePackage : AsyncPackage {
		private AddinContext FContext;
		private DocumentEvents FDocumentEvents;
		private LogWindow FLog;

		public SqlinePackage() {
		}



        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress) {
            FContext = new AddinContext((DTE2)GetService(typeof(SDTE)), this);
            FLog = new LogWindow(this, FContext);
            FDocumentEvents = Context.Application.Events.get_DocumentEvents(null);
            FDocumentEvents.DocumentSaved += OnDocumentSaved;
            FContext.Application.Events.BuildEvents.OnBuildDone += BuildEvents_OnBuildDone;
            FContext.Application.Events.BuildEvents.OnBuildBegin += OnBuildBegin;

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await base.InitializeAsync(cancellationToken, progress);

        }

        private void BuildEvents_OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
        {

        }

        private void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action) {
			FLog.Clear();
			try {
				List<Project> OProjects = new List<Project>();
				FindSqlineProjects(OProjects);
				foreach (Project OProject in OProjects) {
					FLog.SetProject(OProject);
					GenerateDataItems(OProject);
					GenerateProjectHandler(OProject);
				}
			}
			catch (Exception ex) {
				FLog.Add(ex);
			}
			FLog.UpdateView();
		}

		private void GenerateDataItems(Project project) {
			DataItemGenerator OGenerator = new DataItemGenerator(Context, project);
			OGenerator.Generate();
			foreach (string OFile in OGenerator.OutputFiles) {
				ProjectItem OItem = project.ProjectItems.AddFromFile(OFile);
			}
		}

		private void GenerateProjectHandler(Project project) {
			ProjectHandlerGenerator OProjectHandlerGenerator = new ProjectHandlerGenerator(Context, project);
			OProjectHandlerGenerator.Generate();
			foreach (ProjectItem OProjectItem in project.ProjectItems) {
				if (OProjectItem.Name.Equals("sqline.config", StringComparison.OrdinalIgnoreCase)) {
					foreach (string OFile in OProjectHandlerGenerator.OutputFiles) {
						ProjectItem OItem = OProjectItem.ProjectItems.AddFromFile(OFile);
					}
				}
			}
		}

		private void FindSqlineProjects(List<Project> result) {
			foreach (Project OProject in FContext.Application.Solution.Projects) {
				FindSqlineProjects(OProject, result);
			}
		}

		private void FindSqlineProjects(Project project, List<Project> result) {
			if (project == null) {
				return;
			}
			if (project.Kind == EnvDTE.Constants.vsProjectKindSolutionItems) {
				foreach (ProjectItem OProjectItem in project.ProjectItems) {
					FindSqlineProjects(OProjectItem.SubProject, result);
				}
			}
			else {
				if (project.ProjectItems == null) {
					return;
				}
				foreach (ProjectItem OProjectItem in project.ProjectItems) {
					if (OProjectItem.Name.Equals("sqline.config", StringComparison.OrdinalIgnoreCase)) {
						result.Add(project);
					}
				}
			}
		}	

		private void OnDocumentSaved(Document document) {
			FLog.Clear();
			try {
				if (document.FullName.EndsWith(".items")) {
					FLog.SetProject(document.ProjectItem.ContainingProject);
					ItemFileGenerator OGenerator = new ItemFileGenerator(Context, document);
					OGenerator.Generate();
					foreach (string OFile in OGenerator.OutputFiles) {
						ProjectItem OItem = document.ProjectItem.ProjectItems.AddFromFile(OFile);
					}
                    
                    //KAGE temp, BuildEvent er broken i vs 2017, lav dataacess her istedet
				    OnBuildBegin(vsBuildScope.vsBuildScopeProject, vsBuildAction.vsBuildActionBuild);
				}
			}
			catch (Exception ex) {
				FLog.Add(ex);
			}
			FLog.UpdateView();
		}

		public void Dispose() {
			if (FLog != null) {
				FLog.Dispose();
			}
		}

		internal AddinContext Context {
			get {
				return FContext;
			}
		}
	}
}
