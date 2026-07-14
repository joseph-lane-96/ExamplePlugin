using System;

// Required for SendEmailRequest, which submits the Dataverse Email activity for sending.
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace ExamplePlugin
{
    public sealed class AccountFollowup : IPlugin
    {
        // These fields hold the configurable Task values.
        private readonly int _followUpDays;
        private readonly string _taskSubject;
        private readonly string _taskDescription;

        // The constructor sets the default values and reads any configuration supplied through the plugin step.
        public AccountFollowup(
            string unsecureConfiguration,
            string secureConfiguration)
        {
            _followUpDays = 7;
            _taskSubject = "Send e-mail to the new customer.";
            _taskDescription =
                "Follow up with the customer and check whether they need further support.";

            if (string.IsNullOrWhiteSpace(unsecureConfiguration))
            {
                return;
            }

            string[] settings = unsecureConfiguration.Split('|');

            if (settings.Length > 0 &&
                int.TryParse(settings[0], out int configuredDays) &&
                configuredDays >= 0)
            {
                _followUpDays = configuredDays;
            }

            if (settings.Length > 1 &&
                !string.IsNullOrWhiteSpace(settings[1]))
            {
                _taskSubject = settings[1];
            }

            if (settings.Length > 2 &&
                !string.IsNullOrWhiteSpace(settings[2]))
            {
                _taskDescription = settings[2];
            }
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            // Provides information about the event that triggered the plugin.
            IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(
                    typeof(IPluginExecutionContext));

            // Used to record useful diagnostic information in the Plugin Trace Log.
            ITracingService tracing =
                (ITracingService)serviceProvider.GetService(
                    typeof(ITracingService));

            // Only continue when the plugin is running after an Account has been created.
            if (context.MessageName != "Create" ||
                context.PrimaryEntityName != "account" ||
                context.Stage != 40)
            {
                return;
            }

            // Confirm that Target contains an Account record.
            if (!context.InputParameters.Contains("Target") ||
                !(context.InputParameters["Target"] is Entity account))
            {
                return;
            }

            if (account.LogicalName != "account")
            {
                return;
            }

            IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(
                    typeof(IOrganizationServiceFactory));

            IOrganizationService service =
                serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                Guid accountId = context.PrimaryEntityId;

                // Read the Account name from the Target record for the email.
                string accountName =
                    account.GetAttributeValue<string>("name") ??
                    "New account";

                // Calculate once so the Task and Email use the same date.
                DateTime followUpDate =
                    DateTime.UtcNow.AddDays(_followUpDays);

                Entity followup = new Entity("task");

                // Use the configurable values loaded by the constructor.
                followup["subject"] = _taskSubject;
                followup["description"] = _taskDescription;
                followup["scheduledend"] = followUpDate;

                // Link the Task to the newly created Account.
                followup["regardingobjectid"] =
                    new EntityReference("account", accountId);

                // Capture the Task ID for use in the email.
                Guid taskId = service.Create(followup);

                // This must be replaced with the ID of a Dataverse Queue that has an approved and enabled outgoing mailbox.
                Guid senderQueueId =
                    new Guid("11111111-1111-1111-1111-111111111111");

                Entity sender = new Entity("activityparty");
                sender["partyid"] =
                    new EntityReference("queue", senderQueueId);

                // The environment must allow unresolved email recipients for an addressused-only Activity Party.
                Entity recipient = new Entity("activityparty");
                recipient["addressused"] =
                    "hello@christian-aid.org";

                Entity email = new Entity("email");

                email["subject"] =
                    "New Account follow-up Task created";

                email["description"] =
                    $"Account: {accountName}<br/>" +
                    $"Task subject: {_taskSubject}<br/>" +
                    $"Task description: {_taskDescription}<br/>" +
                    $"Follow-up date: {followUpDate:dd MMMM yyyy}<br/>" +
                    $"Account ID: {accountId}<br/>" +
                    $"Task ID: {taskId}";

                email["from"] = new[] { sender };
                email["to"] = new[] { recipient };

                // Link the Email to the Account that triggered the plugin.
                email["regardingobjectid"] =
                    new EntityReference("account", accountId);

                Guid emailId = service.Create(email);

                // Submit the Email for delivery through server-side synchronisation.
                service.Execute(new SendEmailRequest
                {
                    EmailId = emailId,
                    IssueSend = true,
                    TrackingToken = string.Empty
                });

                tracing.Trace(
                    "Created Task {0} and submitted Email {1} for Account {2}.",
                    taskId,
                    emailId,
                    accountId);
            }
            catch (Exception ex)
            {
                // Record the full error in the Dataverse Plugin Trace Log.
                tracing.Trace(
                    "AccountFollowup failed: {0}",
                    ex);

                // Surface the failure so Dataverse records the execution as failed.
                throw new InvalidPluginExecutionException(
                    "The Account follow-up process failed.",
                    ex);
            }
        }
    }
}
