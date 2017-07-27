namespace VSPackageInstaller.SearchProvider
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using System.Net;
    using System.Windows;
    using Microsoft.VisualStudio.Shell.Interop;
    using VSPackageInstaller.Cache;
    

    internal sealed class SearchResult : IVsSearchItemResult
    {
        private readonly IExtensionDataItemView item;

        public SearchResult(
            IVsSearchProvider searchProvider,
            IExtensionDataItemView item)
        {
            // If either of these are null, we end up with very hard to trace exceptions that
            // in Visual Studio that don't really describe the issue. To save us future headaches..
            Debug.Assert(searchProvider != null);
            Debug.Assert(item != null);

            this.SearchProvider = searchProvider;
            this.item = item;
        }

        internal static VSPackageInstaller.PackageInstaller.PackageInstaller packageInstaller = new PackageInstaller.PackageInstaller();

        public void InvokeAction()
        {
            // TODO: use extension manager integration to allow installation while VS is running.
            // Gone for now to the wisps of time...
            // packageInstaller.InstallPackages(item.ExtensionId.ToString(), item.Title);

            Task.Run(InvokeActionAsync);
        }

        private async Task InvokeActionAsync()
        {
            try
            {
                var fileName = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid().ToString()}.vsix");

                using (var webClient = new WebClient())
                {
                    // TODO: a good citizen would keep a list of known temp artifacts (like this one) and delete it on next start up.
                    await webClient.DownloadFileTaskAsync(this.item.Installer, fileName);
                }

                // Use the default windows file associations to invoke VSIXinstaller.exe since we don't know the path.
                Process.Start(new ProcessStartInfo(fileName) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // TODO: perhaps we should handle specific exceptions and give custom error messages.
                MessageBox.Show(ex.Message);
            }
        }

        public IVsSearchProvider SearchProvider { get; }

        public string DisplayText => this.item.Title;

        public string Description => this.item.Description;

        public string Tooltip => this.item.Description;

        public IVsUIObject Icon => null;

        public string PersistenceData => this.item.ExtensionId.ToString();
    }
}
