#region Usings

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

#endregion

namespace SaveClicks
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]
    public sealed class Bootstraper : Package
    {
        public const string PackageGuidString = "e238be6e-e00c-46e4-811a-481fd444e76d";

        private DTE2 _dte;
        private DteInitializer _dteInitializer;

        protected override void Initialize()
        {
            base.Initialize();
            InitializeDte();
        }

        private void InitializeDte()
        {
            _dte = GetService(typeof (SDTE)) as DTE2;

            if (_dte == null) // The IDE is not yet fully initialized
            {
                var shellService = GetService(typeof (SVsShell)) as IVsShell;
                _dteInitializer = new DteInitializer(shellService, InitializeDte);
            }
            else
            {
                _dteInitializer = null;
            }
        }
    }

    internal class DteInitializer : IVsShellPropertyEvents
    {
        private readonly Action _callback;
        private readonly IVsShell _shellService;
        private uint _cookie;

        internal DteInitializer(IVsShell shellService, Action callback)
        {
            _shellService = shellService;
            _callback = callback;

            // Set an event handler to detect when the IDE is fully initialized
            var hr = _shellService.AdviseShellPropertyChanges(this, out _cookie);
            ErrorHandler.ThrowOnFailure(hr);
        }

        int IVsShellPropertyEvents.OnShellPropertyChange(int propid, object var)
        {
            if (propid != (int) __VSSPROPID.VSSPROPID_Zombie) return VSConstants.S_OK;

            var isZombie = (bool) var;
            if (isZombie) return VSConstants.S_OK;

            // Release the event handler to detect when the IDE is fully initialized
            var hr = _shellService.UnadviseShellPropertyChanges(_cookie);
            ErrorHandler.ThrowOnFailure(hr);

            _cookie = 0;
            _callback();
            return VSConstants.S_OK;
        }
    }
}