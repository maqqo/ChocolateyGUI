﻿using Chocolatey.Explorer.Model;
using Chocolatey.Explorer.Powershell;
using Chocolatey.Explorer.Services.SourceService;

namespace Chocolatey.Explorer.Services.PackageVersionService
{
	public class PackageVersionService : IPackageVersionService
	{
		private readonly IRun _powershellAsync;
		private string _package;
		private PackageVersion _packageVersion;
		private readonly ISourceService _sourceService;

		public event Delegates.VersionResult VersionChanged;
	    public event Delegates.StartedDelegate RunStarted;

		public PackageVersionService()
			: this(new RunAsync(), new SourceService.SourceService())
		{
		}

		public PackageVersionService(IRunAsync powershell, ISourceService sourceService)
		{
			_powershellAsync = powershell;
			_sourceService = sourceService;
			_powershellAsync.OutputChanged += VersionHandler;
			_powershellAsync.RunFinished += RunFinished;
		}

		public void PackageVersion(string package)
		{
			this.Log().Info("Getting version of package: " + package);
			_packageVersion = new PackageVersion();
			_package = package;
            OnStarted();
			_powershellAsync.Run("cver " + package + " -source " + _sourceService.Source);
		}

		private void VersionHandler(string version)
		{
			_packageVersion.Name = _package;
			if (version.StartsWith("found") && !version.StartsWith("foundCompare"))
			{
				_packageVersion.CurrentVersion = version.Substring(5).Trim();
			}
			if (version.StartsWith("latest") && !version.StartsWith("latestCopmpare"))
			{
				_packageVersion.Serverversion = version.Substring(6).Trim();
			}
		}

		private void RunFinished()
		{
			OnVersionChanged(_packageVersion);
		}

		private void OnVersionChanged(PackageVersion version)
		{
			var handler = VersionChanged;
			if (handler != null) handler(version);
		}

        private void OnStarted()
        {
            var handler = RunStarted;
            if (handler != null) handler("Getting package " + _package);
        }

	}
}