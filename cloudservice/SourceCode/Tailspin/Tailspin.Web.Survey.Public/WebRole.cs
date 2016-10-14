﻿namespace Tailspin.Web.Survey.Public
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Security.Permissions;
    using Microsoft.Practices.Unity;
    using Microsoft.WindowsAzure.Diagnostics;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Shared.Stores;
    using Tailspin.Web.Survey.Shared;
    using Tailspin.Web.Survey.Shared.Helpers;

    public class WebRole : RoleEntryPoint
    {
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "This is the default code from the project template for the Windows Azure SDK.")]
        public override bool OnStart()
        {
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.
            RoleEnvironment.Changing += RoleEnvironmentChanging;
            RoleEnvironment.Changed += RoleEnvironmentChanged;
            
            StartDiagnosticMonitor();

            using (var container = new UnityContainer())
            {
                ContainerBootstraper.RegisterTypes(container, true);

                container.Resolve<ISurveyStore>().Initialize();
                container.Resolve<ISurveyAnswerStore>().Initialize();
            }

            return base.OnStart();
        }

        private static void RoleEnvironmentChanging(object sender, RoleEnvironmentChangingEventArgs e)
        {
            // for any configuration setting change except TraceEventTypeFilter
            if (e.Changes.OfType<RoleEnvironmentConfigurationSettingChange>().Any(change => change.ConfigurationSettingName != "TraceEventTypeFilter"))
            {
                // Set e.Cancel to true to restart this role instance
                e.Cancel = true;
            }
        }

        private static void RoleEnvironmentChanged(object sender, RoleEnvironmentChangedEventArgs e)
        {
            // configure trace listener for any changes to EnableTableStorageTraceListener 
            if (e.Changes.OfType<RoleEnvironmentConfigurationSettingChange>().Any(change => change.ConfigurationSettingName == "TraceEventTypeFilter"))
            {
                ConfigureTraceListener(RoleEnvironment.GetConfigurationSettingValue("TraceEventTypeFilter"));
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private static void ConfigureTraceListener(string traceEventTypeFilter)
        {
            SourceLevels sourceLevels;
            if (Enum.TryParse(traceEventTypeFilter, true, out sourceLevels))
            {
                TraceHelper.Configure(sourceLevels);
            }
        }

        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        private static void StartDiagnosticMonitor()
        {
            var config = DiagnosticMonitor.GetDefaultInitialConfiguration();
            config.Logs.ScheduledTransferPeriod = TimeSpan.FromMinutes(AzureConstants.LogsConfiguration.ScheduledTransferMinutes);
            ConfigureTraceListener(RoleEnvironment.GetConfigurationSettingValue("TraceEventTypeFilter"));
            DiagnosticMonitor.Start("Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString", config);
        }
    }
}