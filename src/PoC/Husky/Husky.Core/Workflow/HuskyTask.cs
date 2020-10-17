using System;

namespace Husky.Core.Workflow
{
    public abstract class HuskyTask
    {
        protected abstract void EnsureConfigured();

        protected abstract void Execute();

        protected abstract void Rollback();
    }
}