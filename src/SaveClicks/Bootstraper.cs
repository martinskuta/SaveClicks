#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SaveClicks.Presentation;
using SaveClicks.Services.CommandService;
using SaveClicks.Services.CommandService.Impl;
using SaveClicks.Services.InteractionService;
using SaveClicks.Services.InteractionService.Impl;
using SaveClicks.Services.LogService;
using SaveClicks.Services.LogService.Impl;
using SaveClicks.Services.ProductivityAdvisor;
using SaveClicks.Services.ProductivityAdvisor.Impl;

#endregion

namespace SaveClicks
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly",
        Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad("ADFC4E64-0397-11D1-9F4E-00A0C911004F")]
    public sealed class Bootstraper : Package, IVsShellPropertyEvents
    {
        public const string PackageGuidString = "e238be6e-e00c-46e4-811a-481fd444e76d";
        private DteInitializer _dteInitializer;
        private uint _propChangeCookie;

        private IWindsorContainer _iocContainer;

        public int OnShellPropertyChange(int propid, object var)
        {
            if (propid == (int)__VSSPROPID4.VSSPROPID_ShellInitialized && Convert.ToBoolean(var))
            {
                InitializeDte();

                // stop listening for shell property changes
                var vsShell = (IVsShell)GetService(typeof(SVsShell));
                vsShell.UnadviseShellPropertyChanges(_propChangeCookie);
                _propChangeCookie = 0;
            }
            return VSConstants.S_OK;
        }

        protected override void Initialize()
        {
            base.Initialize();

            var vsShell = (IVsShell)GetService(typeof(SVsShell));
            vsShell.AdviseShellPropertyChanges(this, out _propChangeCookie);
        }

        private void InitializeDte()
        {
            var dte = GetService(typeof(SDTE)) as DTE2;

            if (dte == null) // The IDE is not yet fully initialized
            {
                _dteInitializer = new DteInitializer((IVsShell)GetService(typeof(SVsShell)), InitializeDte);
                return;
            }

            _dteInitializer = null;
            InitializeIoC(dte);
        }

        private void InitializeIoC(DTE2 dte)
        {
            _iocContainer = new WindsorContainer();

            _iocContainer.Register(
                Component.For<DTE2>().Instance(dte).LifestyleSingleton(),
                Component.For<ILogService>().ImplementedBy<VsOutputWindow>().LifestyleSingleton(),
                Component.For<IKeyConverter>().ImplementedBy<VsKeyConverter>().LifestyleSingleton(),
                Component.For<IModifierKeyConverter>().ImplementedBy<VsModifierKeyConverter>().LifestyleSingleton(),
                Component.For<ICommandBindingParser>().ImplementedBy<VsCommandBindingParser>().LifestyleSingleton(),
                Component.For<IInteractionService>().ImplementedBy<InteractionService>().LifestyleSingleton(),
                Component.For<ICommandService>().ImplementedBy<VSCommandService>().LifestyleSingleton(),
                Component.For<IProductivityAdvisor>().ImplementedBy<ProductivityAdvisor>().LifestyleSingleton()
                );

            var advisor = _iocContainer.Resolve<IProductivityAdvisor>();
            advisor.UseKeyboardShortcut += AdvisorOnUseKeyboardShortcut;
        }

        private void AdvisorOnUseKeyboardShortcut(object sender, CommandArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                (Action)(() =>
                {
                    var toast = new ToastPopup("Save clicks", CreateFormattedText(e.Command));
                    toast.Show();
                }));
        }

        private List<Inline> CreateFormattedText(Command cmd)
        {
            var inlines = new List<Inline>();

            inlines.Add(new Run("Use "));

            var distinctCommands = cmd.KeyboardBindings.Select(c => c.ToString("ns")).Distinct();

            using (var en = distinctCommands.GetEnumerator())
            {
                en.MoveNext();

                inlines.Add(new Run(en.Current) { FontWeight = FontWeights.Bold });

                while (en.MoveNext())
                {
                    inlines.Add(new Run(" or "));
                    inlines.Add(new Run(en.Current) { FontWeight = FontWeights.Bold });
                }
            }

            inlines.Add(new Run(" for "));
            inlines.Add(new Run(cmd.Name) { FontStyle = FontStyles.Italic });
            inlines.Add(new Run(" next time."));
            return inlines;
        }
    }

    internal class DteInitializer : IVsShellPropertyEvents
    {
        private readonly Action _callback;
        private uint _cookie;
        private readonly IVsShell _shellService;

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
            if (propid != (int)__VSSPROPID.VSSPROPID_Zombie) return VSConstants.S_OK;

            var isZombie = (bool)var;
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