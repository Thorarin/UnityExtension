using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.Practices.ObjectBuilder2;

namespace UnityExtension
{
    internal class DisposingLifetimeStrategy : BuilderStrategy
    {
        private static readonly object _lock = new object();

        [ThreadStatic]
        private static BuildTreeItemNode _currentBuildNode;

        private static readonly ReferenceCounter _refCounter = new ReferenceCounter();

        private readonly Dictionary<WeakReference, BuildTreeItemNode> _buildTrees =
            new Dictionary<WeakReference, BuildTreeItemNode>(new WeakReferenceComparer());

        public void AssignInstanceToCurrentTreeNode(NamedTypeBuildKey buildKey, object instance)
        {
            if (_currentBuildNode.BuildKey != buildKey)
            {
                throw new InvalidOperationException(
                    String.Format(
                        "Build tree constructed out of order. Build key '{0}' was expected but build key '{1}' was provided.",
                        _currentBuildNode.BuildKey, buildKey));
            }

            _currentBuildNode.AssignInstance(instance);
        }

        public void DisposeAllTrees()
        {
            lock (_lock)
            {
                foreach (BuildTreeItemNode buildTree in _buildTrees.Values.ToArray())
                {
                    DisposeTree(buildTree);
                }
            }
        }

        public BuildTreeItemNode GetBuildTreeForInstance(object instance)
        {
            lock (_lock)
            {
                BuildTreeItemNode node;
                _buildTrees.TryGetValue(new WeakReference(instance), out node);
                return node;
            }
        }

        public override void PreBuildUp(IBuilderContext context)
        {
            base.PreBuildUp(context);

            bool nodeCreatedByContainer = context.Existing == null;
            var newTreeNode = new BuildTreeItemNode(context.BuildKey, nodeCreatedByContainer, _currentBuildNode);

            if (_currentBuildNode != null)
            {
                // This is a child node. Add it to the parent node.
                _currentBuildNode.Children.Add(newTreeNode);
            }

            _currentBuildNode = newTreeNode;
        }

        public override void PostBuildUp(IBuilderContext context)
        {
            Contract.Requires(context != null);

            AssignInstanceToCurrentTreeNode(context.BuildKey, context.Existing);

            BuildTreeItemNode parentNode = _currentBuildNode.Parent;
            bool shouldTrackObject = ShouldTrackObject(context);

            if (shouldTrackObject)
            {
                _refCounter.Increment(context.Existing);
            }
            else if (parentNode != null && _currentBuildNode.Children.Count == 0)
            {
                parentNode.Children.Remove(_currentBuildNode);
            }

            if (parentNode == null && (shouldTrackObject || _currentBuildNode.Children.Count > 0))
            {
                // This is the end of the creation of the root node
                lock (_lock)
                {
                    if (GetBuildTreeForInstance(context.Existing) == null)
                    {
                        _buildTrees.Add(_currentBuildNode.ItemReference, _currentBuildNode);
                    }
                }
            }

            // Move the current node back up to the parent
            // If this is the top level node, this will set the current node back to null
            _currentBuildNode = parentNode;

            base.PostBuildUp(context);
        }

        /// <summary>
        /// Called during the chain of responsibility for a teardown operation. The
        /// PostTearDown method is called when the chain has finished the PreTearDown
        /// phase and executes in reverse order from the PreTearDown calls.
        /// </summary>
        public override void PostTearDown(IBuilderContext context)
        {
            base.PostTearDown(context);

            lock (_lock)
            {
                BuildTreeItemNode buildTree = GetBuildTreeForInstance(context.Existing);
                if (buildTree != null)
                {
                    if (DecreaseTreeRefCount(context, buildTree))
                    {
                        _buildTrees.Remove(buildTree.ItemReference);
                    }
                }
            }

            RemoveDeadTrees(context);
        }

        /// <summary>
        /// When a build tree root object is garbage collected without a call to Teardown,
        /// process all the child nodes of the tree and Dispose if necessary.
        /// </summary>
        private void RemoveDeadTrees(IBuilderContext context)
        {
            lock (_lock)
            {
                KeyValuePair<WeakReference, BuildTreeItemNode>[] deadTrees =
                    _buildTrees.Where(kvp => !kvp.Value.ItemReference.IsAlive).ToArray();

                foreach (var kvp in deadTrees)
                {
                    DecreaseTreeRefCount(context, kvp.Value);
                    _buildTrees.Remove(kvp.Key);
                }
            }
        }

        private void DisposeTree(BuildTreeItemNode buildTree)
        {
            foreach (BuildTreeItemNode child in buildTree.Children)
            {
                DisposeTree(child);
            }

            var disposable = buildTree.ItemReference.Target as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }

            lock (_lock)
            {
                _buildTrees.Remove(buildTree.ItemReference);
            }
        }

        private bool DecreaseTreeRefCount(IBuilderContext context, BuildTreeItemNode buildTree)
        {
            Contract.Requires(context != null);
            Contract.Requires(buildTree != null);

            bool removeBuildTree = false;

            foreach (BuildTreeItemNode child in buildTree.Children)
            {
                removeBuildTree |= DecreaseTreeRefCount(context, child);
            }

            var disposable = buildTree.ItemReference.Target as IDisposable;
            if (disposable == null)
                return removeBuildTree;

            // Object is disposable, so we need to track the reference count
            int count;
            if (_refCounter.TryDecrement(disposable, out count) && count == 0)
            {
                DisposingLifetimeManager lifetimeManager = context.Lifetime.OfType<DisposingLifetimeManager>()
                                                                  .SingleOrDefault(ltm => ltm.AppliesTo(disposable));

                if (lifetimeManager != null)
                {
                    disposable.Dispose();
                    lifetimeManager.RemoveValue(disposable);
                    removeBuildTree = true;
                }
            }

            return removeBuildTree;
        }

        private bool ShouldTrackObject(IBuilderContext context)
        {
            return
                context.Existing is IDisposable &&
                context.Lifetime.OfType<DisposingLifetimeManager>().Any(ltm => ltm.AppliesTo(context.Existing));
        }
    }
}