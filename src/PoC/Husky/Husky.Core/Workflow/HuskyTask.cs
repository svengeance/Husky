namespace Husky.Core.Workflow
{
    public abstract class HuskyTask<T> where T : HuskyTaskConfiguration
    {
        protected T Configuration { get; }

        protected HuskyTask(T configuration)
        {
            Configuration = configuration;
        }

        protected abstract void EnsureConfigured();

        protected abstract void Execute();

        protected abstract void Rollback();
    }
}