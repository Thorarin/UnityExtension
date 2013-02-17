using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace UnityExtension
{
    public class DisposableStrategyExtension : UnityContainerExtension, IDisposable
    {
        private readonly DisposingLifetimeStrategy _buildStrategy = new DisposingLifetimeStrategy();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                _buildStrategy.DisposeAllTrees();
            }
        }

        protected override void Initialize()
        {
            Context.Strategies.Add(_buildStrategy, UnityBuildStage.TypeMapping);
        }
    }
}