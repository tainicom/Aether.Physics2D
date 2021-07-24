// Copyright (c) 2021 Kastellanos Nikolaos

/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using tainicom.Aether.Physics2D.Common;
#if XNAAPI
using Vector2 = Microsoft.Xna.Framework.Vector2;
#endif

namespace tainicom.Aether.Physics2D.Collision
{
    /// <summary>
    /// A node in the dynamic tree. The client does not interact with this directly.
    /// </summary>
    internal struct TreeNode<T>
    {
        /// <summary>
        /// Enlarged AABB
        /// </summary>
        internal AABB[] AABB;

        internal int[] Child1;
        internal int[] Child2;

        internal int[] Height; // leaf = 0, free node = -1
        internal int[] Parent;

        // to reduce struct size we use Parent for the Free linked-list
        /// <summary>
        /// Next free node
        /// </summary>
        internal int[] Next
        {
            get { return Parent; }
        }

        internal T[] UserData;

        public TreeNode(int nodeCapacity) : this()
        {
            AABB = new AABB[nodeCapacity];
            
            Height = new int[nodeCapacity];
            Parent = new int[nodeCapacity];

            Child1 = new int[nodeCapacity];
            Child2 = new int[nodeCapacity];

            UserData = new T[nodeCapacity];
        }

        internal void Resize(int nodeCapacity)
        {
            AABB[] oldNodes = AABB;
            int[] oldHeight = Height;
            int[] oldParent = Parent;
            int[] oldChild1 = Child1;
            int[] oldChild2 = Child2;
            T[] oldUserData = UserData;
            AABB = new AABB[nodeCapacity];
            Height = new int[nodeCapacity];
            Parent = new int[nodeCapacity];
            Child1 = new int[nodeCapacity];
            Child2 = new int[nodeCapacity];
            UserData = new T[nodeCapacity];
            Array.Copy(oldNodes, AABB, oldNodes.Length);
            Array.Copy(oldHeight, Height, oldHeight.Length);
            Array.Copy(oldParent, Parent, oldParent.Length);
            Array.Copy(oldChild1, Child1, oldChild1.Length);
            Array.Copy(oldChild2, Child2, oldChild2.Length);
            Array.Copy(oldUserData, UserData, oldUserData.Length);

        }

        internal bool IsLeaf(int index)
        {
            return Child1[index] == DynamicTree<T>.NullNode;
        }

    }

    /// <summary>
    /// A dynamic tree arranges data in a binary tree to accelerate
    /// queries such as volume queries and ray casts. Leafs are proxies
    /// with an AABB. In the tree we expand the proxy AABB by Settings.b2_fatAABBFactor
    /// so that the proxy AABB is bigger than the client object. This allows the client
    /// object to move by small amounts without triggering a tree update.
    ///
    /// Nodes are pooled and relocatable, so we use node indices rather than pointers.
    /// </summary>
    public class DynamicTree<T>
    {
        private Stack<int> _raycastStack = new Stack<int>(256);
        private Stack<int> _queryStack = new Stack<int>(256);
        private int _freeList;
        private int _nodeCapacity;
        private int _root;
        internal const int NullNode = -1;
        private int _nodeCount;
        private TreeNode<T> _nodes;

        /// <summary>
        /// Constructing the tree initializes the node pool.
        /// </summary>
        public DynamicTree()
        {
            _root = NullNode;

            _nodeCapacity = 16;
            _nodeCount = 0;
            _nodes = new TreeNode<T>(_nodeCapacity);

            // Build a linked list for the free list.
            for (int i = 0; i < _nodeCapacity - 1; ++i)
            {
                _nodes.Next[i] = i + 1;
                _nodes.Height[i] = -1;
            }
            // build last node
            _nodes.Next[_nodeCapacity - 1] = NullNode;
            _nodes.Height[_nodeCapacity - 1] = -1;
            _freeList = 0;
        }

        /// <summary>
        /// Compute the height of the binary tree in O(N) time. Should not be called often.
        /// </summary>
        public int Height
        {
            get
            {
                if (_root == NullNode)
                {
                    return 0;
                }

                return _nodes.Height[_root];
            }
        }

        /// <summary>
        /// Get the ratio of the sum of the node areas to the root area.
        /// </summary>
        public float AreaRatio
        {
            get
            {
                if (_root == NullNode)
                {
                    return 0.0f;
                }

                //TreeNode<T>* root = &_nodes[_root];
                float rootArea = _nodes.AABB[_root].Perimeter;

                float totalArea = 0.0f;
                for (int i = 0; i < _nodeCapacity; ++i)
                {
                    //TreeNode<T>* node = &_nodes[i];
                    if (_nodes.Height[i] < 0)
                    {
                        // Free node in pool
                        continue;
                    }

                    totalArea += _nodes.AABB[i].Perimeter;
                }

                return totalArea / rootArea;
            }
        }

        /// <summary>
        /// Get the maximum balance of an node in the tree. The balance is the difference
        /// in height of the two children of a node.
        /// </summary>
        public int MaxBalance
        {
            get
            {
                int maxBalance = 0;
                for (int i = 0; i < _nodeCapacity; ++i)
                {
                    //TreeNode<T>* node = &_nodes[i];
                    if (_nodes.Height[i] <= 1)
                    {
                        continue;
                    }

                    Debug.Assert(_nodes.IsLeaf(i) == false);

                    int child1 = _nodes.Child1[i];
                    int child2 = _nodes.Child2[i];
                    int balance = Math.Abs(_nodes.Height[child2] - _nodes.Height[child1]);
                    maxBalance = Math.Max(maxBalance, balance);
                }

                return maxBalance;
            }
        }

        /// <summary>
        /// Create a proxy in the tree as a leaf node. We return the index
        /// of the node instead of a pointer so that we can grow
        /// the node pool.        
        /// /// </summary>
        /// <param name="aabb">The aabb.</param>
        /// <param name="userData">The user data.</param>
        /// <returns>Index of the created proxy</returns>
        public int AddProxy(ref AABB aabb)
        {
            int proxyId = AllocateNode();

            // Fatten the aabb.
            Vector2 r = new Vector2(Settings.AABBExtension, Settings.AABBExtension);
            _nodes.AABB[proxyId].LowerBound = aabb.LowerBound - r;
            _nodes.AABB[proxyId].UpperBound = aabb.UpperBound + r;
            _nodes.Height[proxyId] = 0;

            InsertLeaf(proxyId);

            return proxyId;
        }

        /// <summary>
        /// Destroy a proxy. This asserts if the id is invalid.
        /// </summary>
        /// <param name="proxyId">The proxy id.</param>
        public void RemoveProxy(int proxyId)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            Debug.Assert(_nodes.IsLeaf(proxyId));

            RemoveLeaf(proxyId);
            FreeNode(proxyId);
        }

        /// <summary>
        /// Move a proxy with a swepted AABB. If the proxy has moved outside of its fattened AABB,
        /// then the proxy is removed from the tree and re-inserted. Otherwise
        /// the function returns immediately.
        /// </summary>
        /// <param name="proxyId">The proxy id.</param>
        /// <param name="aabb">The aabb.</param>
        /// <param name="displacement">The displacement.</param>
        /// <returns>true if the proxy was re-inserted.</returns>
        public bool MoveProxy(int proxyId, ref AABB aabb, Vector2 displacement)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);

            Debug.Assert(_nodes.IsLeaf(proxyId));

            if (_nodes.AABB[proxyId].Contains(ref aabb))
            {
                return false;
            }

            RemoveLeaf(proxyId);

            // Extend AABB.
            AABB b = aabb;
            Vector2 r = new Vector2(Settings.AABBExtension, Settings.AABBExtension);
            b.LowerBound = b.LowerBound - r;
            b.UpperBound = b.UpperBound + r;

            // Predict AABB displacement.
            Vector2 d = Settings.AABBMultiplier * displacement;

            if (d.X < 0.0f)
            {
                b.LowerBound.X += d.X;
            }
            else
            {
                b.UpperBound.X += d.X;
            }

            if (d.Y < 0.0f)
            {
                b.LowerBound.Y += d.Y;
            }
            else
            {
                b.UpperBound.Y += d.Y;
            }

            _nodes.AABB[proxyId] = b;

            InsertLeaf(proxyId);
            return true;
        }

        /// <summary>
        /// Set proxy user data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proxyId">The proxy id.</param>
        /// <param name="userData">The proxy user data.</param>
        public void SetUserData(int proxyId, T userData)
        {
            _nodes.UserData[proxyId] = userData;
        }

        /// <summary>
        /// Get proxy user data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="proxyId">The proxy id.</param>
        /// <returns>the proxy user data or 0 if the id is invalid.</returns>
        public T GetUserData(int proxyId)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            return _nodes.UserData[proxyId];
        }

        /// <summary>
        /// Get the fat AABB for a proxy.
        /// </summary>
        /// <param name="proxyId">The proxy id.</param>
        /// <param name="fatAABB">The fat AABB.</param>
        public void GetFatAABB(int proxyId, out AABB fatAABB)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            fatAABB = _nodes.AABB[proxyId];
        }

        /// <summary>
        /// Get the fat AABB for a proxy.
        /// </summary>
        /// <param name="proxyId">The proxy id.</param>
        /// <returns>The fat AABB.</returns>
        public AABB GetFatAABB(int proxyId)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            return _nodes.AABB[proxyId];
        }

        /// <summary>
        /// Test overlap of fat AABBs.
        /// </summary>
        /// <param name="proxyIdA">The proxy id A.</param>
        /// <param name="proxyIdB">The proxy id B.</param>
        public bool TestFatAABBOverlap(int proxyIdA, int proxyIdB)
        {
            Debug.Assert(0 <= proxyIdA && proxyIdA < _nodeCapacity);
            Debug.Assert(0 <= proxyIdB && proxyIdB < _nodeCapacity);
            return Physics2D.Collision.AABB.TestOverlap(ref _nodes.AABB[proxyIdA], ref _nodes.AABB[proxyIdB]);
        }

        /// <summary>
        /// Query an AABB for overlapping proxies. The callback class
        /// is called for each proxy that overlaps the supplied AABB.
        /// </summary>
        /// <param name="callback">The callback.</param>
        /// <param name="aabb">The aabb.</param>
        public void Query(BroadPhaseQueryCallback callback, ref AABB aabb)
        {
            _queryStack.Clear();
            _queryStack.Push(_root);

            while (_queryStack.Count > 0)
            {
                int nodeId = _queryStack.Pop();
                if (nodeId == NullNode)
                {
                    continue;
                }

                //TreeNode<T>* node = &_nodes[nodeId];

                if (Physics2D.Collision.AABB.TestOverlap(ref _nodes.AABB[nodeId], ref aabb))
                {
                    if (_nodes.IsLeaf(nodeId))
                    {
                        bool proceed = callback(nodeId);
                        if (proceed == false)
                        {
                            return;
                        }
                    }
                    else
                    {
                        _queryStack.Push(_nodes.Child1[nodeId]);
                        _queryStack.Push(_nodes.Child2[nodeId]);
                    }
                }
            }
        }

        /// <summary>
        /// Ray-cast against the proxies in the tree. This relies on the callback
        /// to perform a exact ray-cast in the case were the proxy contains a Shape.
        /// The callback also performs the any collision filtering. This has performance
        /// roughly equal to k * log(n), where k is the number of collisions and n is the
        /// number of proxies in the tree.
        /// </summary>
        /// <param name="callback">A callback class that is called for each proxy that is hit by the ray.</param>
        /// <param name="input">The ray-cast input data. The ray extends from p1 to p1 + maxFraction * (p2 - p1).</param>
        public void RayCast(BroadPhaseRayCastCallback callback, ref RayCastInput input)
        {
            Vector2 p1 = input.Point1;
            Vector2 p2 = input.Point2;
            Vector2 r = p2 - p1;
            Debug.Assert(r.LengthSquared() > 0.0f);
            r.Normalize();

            // v is perpendicular to the segment.
            Vector2 absV = MathUtils.Abs(new Vector2(-r.Y, r.X)); //FPE: Inlined the 'v' variable

            // Separating axis for segment (Gino, p80).
            // |dot(v, p1 - c)| > dot(|v|, h)

            float maxFraction = input.MaxFraction;

            // Build a bounding box for the segment.
            AABB segmentAABB = new AABB();
            {
                Vector2 t = p1 + maxFraction * (p2 - p1);
                Vector2.Min(ref p1, ref t, out segmentAABB.LowerBound);
                Vector2.Max(ref p1, ref t, out segmentAABB.UpperBound);
            }

            _raycastStack.Clear();
            _raycastStack.Push(_root);

            while (_raycastStack.Count > 0)
            {
                int nodeId = _raycastStack.Pop();
                if (nodeId == NullNode)
                {
                    continue;
                }

                //TreeNode<T>* node = &_nodes[nodeId];

                if (Physics2D.Collision.AABB.TestOverlap(ref _nodes.AABB[nodeId], ref segmentAABB) == false)
                {
                    continue;
                }

                // Separating axis for segment (Gino, p80).
                // |dot(v, p1 - c)| > dot(|v|, h)
                Vector2 c = _nodes.AABB[nodeId].Center;
                Vector2 h = _nodes.AABB[nodeId].Extents;
                float separation = Math.Abs(Vector2.Dot(new Vector2(-r.Y, r.X), p1 - c)) - Vector2.Dot(absV, h);
                if (separation > 0.0f)
                {
                    continue;
                }

                if (_nodes.IsLeaf(nodeId))
                {
                    RayCastInput subInput;
                    subInput.Point1 = input.Point1;
                    subInput.Point2 = input.Point2;
                    subInput.MaxFraction = maxFraction;

                    float value = callback(ref subInput, nodeId);

                    if (value == 0.0f)
                    {
                        // the client has terminated the raycast.
                        return;
                    }

                    if (value > 0.0f)
                    {
                        // Update segment bounding box.
                        maxFraction = value;
                        Vector2 t = p1 + maxFraction * (p2 - p1);
                        Vector2.Min(ref p1, ref t, out segmentAABB.LowerBound);
                        Vector2.Max(ref p1, ref t, out segmentAABB.UpperBound);
                    }
                }
                else
                {
                    _raycastStack.Push(_nodes.Child1[nodeId]);
                    _raycastStack.Push(_nodes.Child2[nodeId]);
                }
            }
        }

        private int AllocateNode()
        {
            // Expand the node pool as needed.
            if (_freeList == NullNode)
            {
                Debug.Assert(_nodeCount == _nodeCapacity);

                // The free list is empty. Rebuild a bigger pool.
                _nodeCapacity *= 2;
                _nodes.Resize(_nodeCapacity);
                
                // Build a linked list for the free list.
                for (int i = _nodeCount; i < _nodeCapacity - 1; ++i)
                {
                    _nodes.Next[i] = i + 1;
                    _nodes.Height[i] = -1;
                }
                // build last node
                _nodes.Next[_nodeCapacity - 1] = NullNode;
                _nodes.Height[_nodeCapacity - 1] = -1;
                _freeList = _nodeCount;
            }

            // Peel a node off the free list.
            int nodeId = _freeList;
            _freeList = _nodes.Next[nodeId];
            // reinitialize node
            _nodes.Parent[nodeId] = NullNode;
            _nodes.Child1[nodeId] = NullNode;
            _nodes.Child2[nodeId] = NullNode;
            _nodes.Height[nodeId] = 0;
            _nodes.UserData[nodeId] = default(T);
            ++_nodeCount;
            return nodeId;
        }

        private void FreeNode(int nodeId)
        {
            Debug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
            Debug.Assert(0 < _nodeCount);
            _nodes.Next[nodeId] = _freeList;
            _nodes.Height[nodeId] = -1;
            _freeList = nodeId;
            --_nodeCount;
        }

        private void InsertLeaf(int leaf)
        {
            if (_root == NullNode)
            {
                _root = leaf;
                _nodes.Parent[_root] = NullNode;
                return;
            }

            // Find the best sibling for this node
            AABB leafAABB = _nodes.AABB[leaf];

            int index = _root;
            while (_nodes.IsLeaf(index) == false)
            {
                int child1 = _nodes.Child1[index];
                int child2 = _nodes.Child2[index];

                float area = _nodes.AABB[index].Perimeter;


                AABB combinedAABB = new AABB();
                combinedAABB.Combine(ref _nodes.AABB[index], ref leafAABB);
                float combinedArea = combinedAABB.Perimeter;

                // Cost of creating a new parent for this node and the new leaf
                float cost = 2.0f * combinedArea;

                // Minimum cost of pushing the leaf further down the tree
                float inheritanceCost = 2.0f * (combinedArea - area);

                // Cost of descending into child1
                float cost1;
                if (_nodes.IsLeaf(child1))
                {
                    AABB aabb = new AABB();
                    aabb.Combine(ref leafAABB, ref _nodes.AABB[child1]);
                    cost1 = aabb.Perimeter + inheritanceCost;
                }
                else
                {
                    AABB aabb = new AABB();
                    aabb.Combine(ref leafAABB, ref _nodes.AABB[child1]);
                    float oldArea = _nodes.AABB[child1].Perimeter;
                    float newArea = aabb.Perimeter;
                    cost1 = (newArea - oldArea) + inheritanceCost;
                }

                // Cost of descending into child2
                float cost2;
                if (_nodes.IsLeaf(child2))
                {
                    AABB aabb = new AABB();
                    aabb.Combine(ref leafAABB, ref _nodes.AABB[child2]);
                    cost2 = aabb.Perimeter + inheritanceCost;
                }
                else
                {
                    AABB aabb = new AABB();
                    aabb.Combine(ref leafAABB, ref _nodes.AABB[child2]);
                    float oldArea = _nodes.AABB[child2].Perimeter;
                    float newArea = aabb.Perimeter;
                    cost2 = newArea - oldArea + inheritanceCost;
                }

                // Descend according to the minimum cost.
                if (cost < cost1 && cost1 < cost2)
                {
                    break;
                }

                // Descend
                if (cost1 < cost2)
                {
                    index = child1;
                }
                else
                {
                    index = child2;
                }
            }

            int sibling = index;

            // Create a new parent.
            int oldParent = _nodes.Parent[sibling];
            int newParent = AllocateNode();
            _nodes.Parent[newParent] = oldParent;
            _nodes.UserData[newParent] = default(T);
            _nodes.AABB[newParent].Combine(ref leafAABB, ref _nodes.AABB[sibling]);
            _nodes.Height[newParent] = _nodes.Height[sibling] + 1;

            if (oldParent != NullNode)
            {
                // The sibling was not the root.
                if (_nodes.Child1[oldParent] == sibling)
                {
                    _nodes.Child1[oldParent] = newParent;
                }
                else
                {
                    _nodes.Child2[oldParent] = newParent;
                }

                _nodes.Child1[newParent] = sibling;
                _nodes.Child2[newParent] = leaf;
                _nodes.Parent[sibling] = newParent;
                _nodes.Parent[leaf] = newParent;
            }
            else
            {
                // The sibling was the root.
                _nodes.Child1[newParent] = sibling;
                _nodes.Child2[newParent] = leaf;
                _nodes.Parent[sibling] = newParent;
                _nodes.Parent[leaf] = newParent;
                _root = newParent;
            }

            // Walk back up the tree fixing heights and AABBs
            index = _nodes.Parent[leaf];
            while (index != NullNode)
            {
                index = Balance(index);

                int child1 = _nodes.Child1[index];
                int child2 = _nodes.Child2[index];

                Debug.Assert(child1 != NullNode);
                Debug.Assert(child2 != NullNode);

                _nodes.Height[index] = 1 + Math.Max(_nodes.Height[child1], _nodes.Height[child2]);
                _nodes.AABB[index].Combine(ref _nodes.AABB[child1], ref _nodes.AABB[child2]);

                index = _nodes.Parent[index];
            }

            //Validate();
        }

        private void RemoveLeaf(int leaf)
        {
            if (leaf == _root)
            {
                _root = NullNode;
                return;
            }

            int parent = _nodes.Parent[leaf];
            int grandParent = _nodes.Parent[parent];
            int sibling;
            if (_nodes.Child1[parent] == leaf)
            {
                sibling = _nodes.Child2[parent];
            }
            else
            {
                sibling = _nodes.Child1[parent];
            }

            if (grandParent != NullNode)
            {
                // Destroy parent and connect sibling to grandParent.
                if (_nodes.Child1[grandParent] == parent)
                {
                    _nodes.Child1[grandParent] = sibling;
                }
                else
                {
                    _nodes.Child2[grandParent] = sibling;
                }
                _nodes.Parent[sibling] = grandParent;
                FreeNode(parent);

                // Adjust ancestor bounds.
                int index = grandParent;
                while (index != NullNode)
                {
                    index = Balance(index);

                    int child1 = _nodes.Child1[index];
                    int child2 = _nodes.Child2[index];

                    _nodes.AABB[index].Combine(ref _nodes.AABB[child1], ref _nodes.AABB[child2]);
                    _nodes.Height[index] = 1 + Math.Max(_nodes.Height[child1], _nodes.Height[child2]);

                    index = _nodes.Parent[index];
                }
            }
            else
            {
                _root = sibling;
                _nodes.Parent[sibling] = NullNode;
                FreeNode(parent);
            }

            //Validate();
        }

        /// <summary>
        /// Perform a left or right rotation if node N is imbalanced.
        /// </summary>
        /// <param name="iN"></param>
        /// <returns>the new root index.</returns>
        private int Balance(int iN)
        {
            Debug.Assert(iN != NullNode);

            //TreeNode<T>* N = &_nodes[iN];
            if (_nodes.IsLeaf(iN) || _nodes.Height[iN] < 2)
            {
                return iN;
            }

            int iA = _nodes.Child1[iN];
            int iB = _nodes.Child2[iN];
            Debug.Assert(0 <= iA && iA < _nodeCapacity);
            Debug.Assert(0 <= iB && iB < _nodeCapacity);

            //TreeNode<T>* A = &_nodes[iA];
            //TreeNode<T>* B = &_nodes[iB];

            int balance = _nodes.Height[iB] - _nodes.Height[iA];

            // Rotate B up
            if (balance > 1)
            {
                int iP = _nodes.Parent[iN];
                int iBA = _nodes.Child1[iB];
                int iBB = _nodes.Child2[iB];
                //TreeNode<T>* P  = &_nodes[iN->Parent];
                //TreeNode<T>* BA = &_nodes[iBA];
                //TreeNode<T>* BB = &_nodes[iBB];
                Debug.Assert(0 <= iBA && iBA < _nodeCapacity);
                Debug.Assert(0 <= iBB && iBB < _nodeCapacity);

                // Swap N and B
                _nodes.Child1[iB] = iN;
                _nodes.Parent[iB] = _nodes.Parent[iN];
                _nodes.Parent[iN] = iB;

                // N's old parent should point to B
                if (iP != NullNode)
                {
                    if (_nodes.Child1[iP] == iN)
                    {
                        _nodes.Child1[iP] = iB;
                    }
                    else
                    {
                        Debug.Assert(_nodes.Child2[iP] == iN);
                        _nodes.Child2[iP] = iB;
                    }
                }
                else
                {
                    _root = iB;
                }

                // Rotate
                if (_nodes.Height[iBA] > _nodes.Height[iBB])
                {
                    _nodes.Child2[iB] = iBA;
                    _nodes.Child2[iN] = iBB;
                    _nodes.Parent[iBB] = iN;
                    _nodes.AABB[iN].Combine(ref _nodes.AABB[iA], ref _nodes.AABB[iBB]);
                    _nodes.AABB[iB].Combine(ref _nodes.AABB[iN], ref _nodes.AABB[iBA]);

                    _nodes.Height[iN] = 1 + Math.Max(_nodes.Height[iA], _nodes.Height[iBB]);
                    _nodes.Height[iB] = 1 + Math.Max(_nodes.Height[iN], _nodes.Height[iBA]);
                }
                else
                {
                    _nodes.Child2[iB] = iBB;
                    _nodes.Child2[iN] = iBA;
                    _nodes.Parent[iBA] = iN;
                    _nodes.AABB[iN].Combine(ref _nodes.AABB[iA], ref _nodes.AABB[iBA]);
                    _nodes.AABB[iB].Combine(ref _nodes.AABB[iN], ref _nodes.AABB[iBB]);

                    _nodes.Height[iN] = 1 + Math.Max(_nodes.Height[iA], _nodes.Height[iBA]);
                    _nodes.Height[iB] = 1 + Math.Max(_nodes.Height[iN], _nodes.Height[iBB]);
                }

                return iB;
            }

            // Rotate A up
            if (balance < -1)
            {
                int iP = _nodes.Parent[iN];
                int iAA = _nodes.Child1[iA];
                int iAB = _nodes.Child2[iA];
                //TreeNode<T>* P  = &_nodes[iN->Parent];
                //TreeNode<T>* AA = &_nodes[iAA];
                //TreeNode<T>* AB = &_nodes[iAB];
                Debug.Assert(0 <= iAA && iAA < _nodeCapacity);
                Debug.Assert(0 <= iAB && iAB < _nodeCapacity);

                // Swap N and A
                _nodes.Child1[iA] = iN;
                _nodes.Parent[iA] = _nodes.Parent[iN];
                _nodes.Parent[iN] = iA;

                // N's old parent should point to A
                if (iP != NullNode)
                {
                    if (_nodes.Child1[iP] == iN)
                    {
                        _nodes.Child1[iP] = iA;
                    }
                    else
                    {
                        Debug.Assert(_nodes.Child2[iP] == iN);
                        _nodes.Child2[iP] = iA;
                    }
                }
                else
                {
                    _root = iA;
                }

                // Rotate
                if (_nodes.Height[iAA] > _nodes.Height[iAB])
                {
                    _nodes.Child2[iA] = iAA;
                    _nodes.Child1[iN] = iAB;
                    _nodes.Parent[iAB] = iN;
                    _nodes.AABB[iN].Combine(ref _nodes.AABB[iB], ref _nodes.AABB[iAB]);
                    _nodes.AABB[iA].Combine(ref _nodes.AABB[iN], ref _nodes.AABB[iAA]);

                    _nodes.Height[iN] = 1 + Math.Max(_nodes.Height[iB], _nodes.Height[iAB]);
                    _nodes.Height[iA] = 1 + Math.Max(_nodes.Height[iN], _nodes.Height[iAA]);
                }
                else
                {
                    _nodes.Child2[iA] = iAB;
                    _nodes.Child1[iN] = iAA;
                    _nodes.Parent[iAA] = iN;
                    _nodes.AABB[iN].Combine(ref _nodes.AABB[iB], ref _nodes.AABB[iAA]);
                    _nodes.AABB[iA].Combine(ref _nodes.AABB[iN], ref _nodes.AABB[iAB]);

                    _nodes.Height[iN] = 1 + Math.Max(_nodes.Height[iB], _nodes.Height[iAA]);
                    _nodes.Height[iA] = 1 + Math.Max(_nodes.Height[iN], _nodes.Height[iAB]);
                }

                return iA;
            }

            return iN;
        }

        /// <summary>
        /// Compute the height of a sub-tree.
        /// </summary>
        /// <param name="nodeId">The node id to use as parent.</param>
        /// <returns>The height of the tree.</returns>
        public int ComputeHeight(int nodeId)
        {
            Debug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
            //TreeNode<T>* node = &_nodes[nodeId];

            if (_nodes.IsLeaf(nodeId))
            {
                return 0;
            }

            int height1 = ComputeHeight(_nodes.Child1[nodeId]);
            int height2 = ComputeHeight(_nodes.Child2[nodeId]);
            return 1 + Math.Max(height1, height2);
        }

        /// <summary>
        /// Compute the height of the entire tree.
        /// </summary>
        /// <returns>The height of the tree.</returns>
        public int ComputeHeight()
        {
            int height = ComputeHeight(_root);
            return height;
        }

        public void ValidateStructure(int index)
        {
            if (index == NullNode)
            {
                return;
            }

            if (index == _root)
            {
                Debug.Assert(_nodes.Parent[index] == NullNode);
            }

            //TreeNode<T>* node = &_nodes[index];

            int child1 = _nodes.Child1[index];
            int child2 = _nodes.Child2[index];

            if (_nodes.IsLeaf(index))
            {
                Debug.Assert(child1 == NullNode);
                Debug.Assert(child2 == NullNode);
                Debug.Assert(_nodes.Height[index] == 0);
                return;
            }

            Debug.Assert(0 <= child1 && child1 < _nodeCapacity);
            Debug.Assert(0 <= child2 && child2 < _nodeCapacity);

            Debug.Assert(_nodes.Parent[child1] == index);
            Debug.Assert(_nodes.Parent[child2] == index);

            ValidateStructure(child1);
            ValidateStructure(child2);
        }

        public void ValidateMetrics(int index)
        {
            if (index == NullNode)
            {
                return;
            }

            //TreeNode<T>* node = &_nodes[index];

            int child1 = _nodes.Child1[index];
            int child2 = _nodes.Child2[index];

            if (_nodes.IsLeaf(index))
            {
                Debug.Assert(child1 == NullNode);
                Debug.Assert(child2 == NullNode);
                Debug.Assert(_nodes.Height[index] == 0);
                return;
            }

            Debug.Assert(0 <= child1 && child1 < _nodeCapacity);
            Debug.Assert(0 <= child2 && child2 < _nodeCapacity);

            int height1 = _nodes.Height[child1];
            int height2 = _nodes.Height[child2];
            int height = 1 + Math.Max(height1, height2);
            Debug.Assert(_nodes.Height[index] == height);

            AABB AABB = new AABB();
            AABB.Combine(ref _nodes.AABB[child1], ref _nodes.AABB[child2]);

            Debug.Assert(AABB.LowerBound == _nodes.AABB[index].LowerBound);
            Debug.Assert(AABB.UpperBound == _nodes.AABB[index].UpperBound);

            ValidateMetrics(child1);
            ValidateMetrics(child2);
        }

        /// <summary>
        /// Validate this tree. For testing.
        /// </summary>
        public void Validate()
        {
            ValidateStructure(_root);
            ValidateMetrics(_root);

            int freeCount = 0;
            int freeIndex = _freeList;
            while (freeIndex != NullNode)
            {
                Debug.Assert(0 <= freeIndex && freeIndex < _nodeCapacity);
                freeIndex = _nodes.Next[freeIndex];
                ++freeCount;
            }

            Debug.Assert(Height == ComputeHeight());

            Debug.Assert(_nodeCount + freeCount == _nodeCapacity);
        }

        /// <summary>
        /// Build an optimal tree. Very expensive. For testing.
        /// </summary>
        public void RebuildBottomUp()
        {
            int[] nodes = new int[_nodeCount];
            int count = 0;

            // Build array of leaves. Free the rest.
            for (int i = 0; i < _nodeCapacity; ++i)
            {
                if (_nodes.Height[i] < 0)
                {
                    // free node in pool
                    continue;
                }

                if (_nodes.IsLeaf(i))
                {
                    _nodes.Parent[i] = NullNode;
                    nodes[count] = i;
                    ++count;
                }
                else
                {
                    FreeNode(i);
                }
            }

            while (count > 1)
            {
                float minCost = Settings.MaxFloat;
                int iMin = -1, jMin = -1;
                for (int i = 0; i < count; ++i)
                {
                    AABB AABBi = _nodes.AABB[nodes[i]];

                    for (int j = i + 1; j < count; ++j)
                    {
                        AABB AABBj = _nodes.AABB[nodes[j]];
                        AABB b = new AABB();
                        b.Combine(ref AABBi, ref AABBj);
                        float cost = b.Perimeter;
                        if (cost < minCost)
                        {
                            iMin = i;
                            jMin = j;
                            minCost = cost;
                        }
                    }
                }

                int index1 = nodes[iMin];
                int index2 = nodes[jMin];
                //TreeNode<T>* child1 = &_nodes[index1];
                //TreeNode<T>* child2 = &_nodes[index2];

                int parentIndex = AllocateNode();
                //TreeNode<T>* parent = &_nodes[parentIndex];
                _nodes.Child1[parentIndex] = index1;
                _nodes.Child2[parentIndex] = index2;
                _nodes.Height[parentIndex] = 1 + Math.Max(_nodes.Height[index1], _nodes.Height[index2]);
                _nodes.AABB[parentIndex].Combine(ref _nodes.AABB[index1], ref _nodes.AABB[index2]);
                _nodes.Parent[parentIndex] = NullNode;

                _nodes.Parent[index1] = parentIndex;
                _nodes.Parent[index2] = parentIndex;

                nodes[jMin] = nodes[count - 1];
                nodes[iMin] = parentIndex;
                --count;
            }

            _root = nodes[0];

            Validate();
        }

        /// <summary>
        /// Shift the origin of the nodes
        /// </summary>
        /// <param name="newOrigin">The displacement to use.</param>
        public void ShiftOrigin(Vector2 newOrigin)
        {
            // Build array of leaves. Free the rest.
            for (int i = 0; i < _nodeCapacity; ++i)
            {
                _nodes.AABB[i].LowerBound -= newOrigin;
                _nodes.AABB[i].UpperBound -= newOrigin;
            }
        }
    }
}