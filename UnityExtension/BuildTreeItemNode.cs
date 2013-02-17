using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using Microsoft.Practices.ObjectBuilder2;

namespace UnityExtension
{
    internal class BuildTreeItemNode
    {
        public BuildTreeItemNode(NamedTypeBuildKey buildKey, Boolean nodeCreatedByContainer,
                                 BuildTreeItemNode parentNode)
        {
            Contract.Requires<ArgumentNullException>(buildKey != null);

            BuildKey = buildKey;
            NodeCreatedByContainer = nodeCreatedByContainer;
            Parent = parentNode;
            Children = new Collection<BuildTreeItemNode>();
        }

        public NamedTypeBuildKey BuildKey { get; private set; }

        public Collection<BuildTreeItemNode> Children { get; private set; }

        public WeakReference ItemReference { get; private set; }

        public bool NodeCreatedByContainer { get; private set; }

        public BuildTreeItemNode Parent { get; private set; }

        public void AssignInstance(object instance)
        {
            if (ItemReference != null)
            {
                // Instance can be already assigned, since we're reusing build trees
                if (ItemReference.Target == instance)
                    return;

                throw new InvalidOperationException("Instance is already assigned.");
            }

            ItemReference = new WeakReference(instance);
        }
    }
}