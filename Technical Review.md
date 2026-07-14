## Summary
The plugin has been updated to create a configurable follow-up Task when an Account is created and send an email containing details of that Task.

The review focused on meeting the requirements while keeping the solution simple and appropriate for the size of the exercise.
## Main changes

- Made the follow-up delay, Task subject, and Task description configurable.
- Added validation so the plugin only runs for Account creation in post-operation.
- Replaced `OutputParameters["id"]` with `PrimaryEntityId`.
- Replaced `DateTime.Now` with `DateTime.UtcNow`.
- Added plugin tracing.
- Removed the empty exception handler so failures are visible.
- Captured the created Task ID.
- Added a Dataverse Email notification.
- Removed unused imports and unnecessary dependencies.
## Design decisions

Plugin step configuration was used because the settings are specific to this plugin and can be changed without recompiling the assembly.

A native Dataverse Email activity was used instead of direct SMTP so the email is recorded in Dataverse and sent through the configured mailbox and server-side synchronisation. Also, this avoids hard coding SMTP credentials in the code.  

The plugin should be registered as:
- Message: `Create`
- Table: `Account`
- Stage: `Post-operation`
- Mode: `Asynchronous`
## Assumptions
- The delay is measured in calendar days.
- A suitable Dataverse Queue and outgoing mailbox are configured.
- The execution user has permission to create Tasks and Emails.
- The recipient email address is `hello@christian-aid.org`.
## Further improvements

With more time, I would add unit tests, make the sender Queue configurable, and consider environment variables if the settings needed to be shared across multiple components.