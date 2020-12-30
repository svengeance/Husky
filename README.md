# Husky - The Most Loyal and Reliable Installer, Powered by .NET Core
Husky is the world's first open-source x-plat native installer that aims to be as simple as possible to use. Whether you're developing applications in .NET, Python, Java, or anything in-between, Husky can deliver the application to your clients.

## What Makes Husky Different?
Husky is the first installer that looks at an installation process not as a program, but as a pipeline. Compared to the CI/CD pipelines that we all know and love, why is an installer any different?

### Yaml-First
Most modern pipelines are built in [YAML](https://yaml.org/). These pipelines allow users to define configuration files of various complexity, and ship it off to a runner/execution environment. Husky provides users the same capability to write a configuration-first installer, with pre-defined tasks (HuskyTasks) that facilitates execution.

### Extensibility
It's a common trend for any given library that its out-of-box functionality works well, but once one tries to deviate from the beaten path, troubles arise.  
Using the modern development paradigms of Dependency Injection and Inversion of Control, we can provide clients and developers with the option of **extending** or **overriding** various aspects of Husky's internal *Tasks* and *Services*. Changing one aspect of Husky is as simple as identifying the piece you want to change, overriding the class in-code, and registering your `Assembly` as contributing to the Husky Dependency Injection System. Users even have the capability to use Husky's pre-existing service layer to write their own HuskyTasks!

### Reliability
Client machines come in all shapes and sizes, and theres always a risk of failure when doing something potentially complex as a product installation. Husky tries its best to execute each task, and in the event it encounters an issue, will reliably report failures, and attempt to rollback the installation.

### First-Class Support for Dockerized Testing
Testing an installer is typically not an enjoyable experience. Developers & QA must either repeatedly install/uninstall on their machine, or go through the process of setting up virtual hardware to spin up & tear down. Husky makes this process easier by allowing developers to write integration tests against their installer, setting up expectatiosn for what the user's machine should look like post-install. Husky then facilitates the creation of a docker container with the desired OS, running the installer, running the users' tests, and then reporting back the success or failure. This will give users the confidence they need to deploy their changes and make continual improvements.

## Getting Started
// Todo

### Install Packages
// Todo

### Browse Documentation for Predefined Tasks & Services
// Todo

### Write Integration Tests
// Todo

### Distribute!
// Todo

## Repository & Contribution Guidelines

### Formatting
// Todo

### Testing
// Todo

#### Unit Tests
// Todo

#### Integration Tests
// Todo