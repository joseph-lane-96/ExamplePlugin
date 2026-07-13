# Account Follow-up Plugin — Technical Exercise

This repository contains a .NET Framework class library that implements a Microsoft Dataverse (Dynamics 365) plugin. You should assume the current code runs on **Create** of an **account** record, with the goal of creating a linked **task** activity scheduled for **7 days** later.

Your goal is to improve `AccountFollowup` so that it is more suitable for a real CRM solution.

### 1. Make behaviour configurable

The follow-up **delay**, **task subject**, and **task description** should not be hard-coded.

Choose an approach you think is appropriate for a real CRM solution and implement it. There is more than one valid design (for example, plugin step configuration, a settings entity, environment variables, or a combination). Be prepared to explain why you chose that approach over alternatives.

### 2. Send an email notification

When the follow-up task is created, the plugin should also send an **email notification** to:

`hello@christian-aid.org`

The email should be about the task that was created with enough detail for someone to act on it.

(You may invent any SMTP configuration and credentials as required)

### Guidance

- As a rough guide, you should aim for no more than **30 minutes** on this task.
- You may use whatever tools you would normally use when coding (including AI).
- You are **not** expected to register or deploy the plugin into a live Dataverse environment for this exercise.
- You may assume the creation of any dependant entities. Please leave brief comments to explain what you would create or other design choices you have made.
- You may refactor the existing code as you see fit.


The deadline is **10am Wednesday 15th July**. Please submit either:

- A link to a forked version of the repository with your changes.
- An attached text file of your code (e.g. AccountFollowup.cs.txt)


