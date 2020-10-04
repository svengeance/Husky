# Table of Contents
- [YAML-Specific DSL](#yaml-specific-dsl)
  * [Workflow](#workflow)
  * [Stage](#stage)
  * [Job](#job)
  * [Task](#task)
- [C#-Specific DSL](#c--specific-dsl)
  * [HuskyStage](#huskystage)
  * [HuskyJob](#huskyjob)
  * [HuskyTask](#huskytask)
  * [HuskyAction](#huskyaction)
# Domain-Specific Language for Husky

Husky uses a variety of terms to describe how it works.
To obviate the learning of a bespoke installer language, Husky prefers to align itself
with the modern established best practices and reuse terminology from Azure and Github pipelines.
Huskies should never be a burden, but a natural companion. 

## YAML-Specific DSL

### Workflow  
A workflow captures the installation process from start to finish.
Comprised of many subsections (see [stages](#stage), [jobs](#job), and [tasks](#task)), Husky operates
by creating, validating, and running a workflow. Your application should depend on a single workflow
to install itself. 

A workflow operates on an optional three-tier structure, described in the following sections. Please note that each tier has a semantic meaning in accordance to its place in the hierarchy, and that only the final tier ([tasks](#task)) may execute installation steps. A workflow must contain one or more of the following:

### Stage
The first tier of a workflow, a stage usually represents a complete installation of a single application comprised of a series of [jobs](#job). Unlike traditional CI/CD pipelines where stages are isolated from each other, Husky will operate on a user's machine, without the privilege of blowing away and instancing a clean slate per stage (at least not without severe disgruntlement from the user). 

A multi-stage workflow for a Husky installation would be preferable when installing a series of _related_ applications, each with an independent install flow. For example, if you were to install a game and a chat application, and you wanted to bundle those within the same installer, you would use a multi-stage workflow.

Stages can demand (require) that a client machine meets certain criteria. It is often preferable to have a client install dependencies first, however Husky is capable of fetching a variety of whitelisted dependencies. 

### Job
The second tier of a workflow, a job represents a series of [tasks](#task). Jobs within a [stage](#stage) can depend on each other, execute in parallel, and relay information to each other.  
You might use several jobs to perform an installation of an application. A series of jobs might look like the following, with each comma-separated value indicating a potential [task](#task).

* (UI) Display Splash Screen, Disaply/Accet Terms of Use
* (UI) Collect Install Directory, Collect Featureset
* Extract Bundled Dist Files, Create Link for Quickstart/Taskbar/Desktop
* Create Necessary System/Registry Entries
* (UI) Thank User, Open Readme, Start Program

### Task
The final tier of a workflow. A task represents a specific action that Husky can perform.
Tasks can be custom code by the user, or pre-defined tasks that Husky is trained to execute.   
Tasks do not execute in parallel, as they are typically linear in nature and usually dependent upon each other.

## C#-Specific DSL 
*Note: The following is still in the discovery phase and subject to change between now and the v1 public release*

### HuskyStage
1:1 Correlation with a [YAML Stage](#stage), this class wrap a collection of [HuskyJobs](#HuskyJob).

### HuskyJob
1:1 Correlation with a [YAML Job](#job), this class wrap a collection of [HuskyTasks](#HuskyTask) and adds the necessary behavior to communicate with other HuskyJobs.

The internal Service Provider for Husky has a "scope" of a HuskyJob; all scoped services will thusly be reinitialized per-job.

### HuskyTask
1:1 Correlation with a [YAML Task](#task), a HuskyTask is a wrapper around a [HuskyAction](#huskyAction) that adds the necessary behavior to communicate with other HuskyTasks. 

### HuskyAction
A generic wrapper around an invokable action. This class defines all of the boilerplate necessary for an action to be friendly to Husky - Execution, progress events, logging/output, rollback behavior, etc.

