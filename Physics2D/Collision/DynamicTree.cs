// Copyright (c) 2018 Kastellanos Nikolaos

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
        internal AABB AABB;

        internal int Child1;
        internal int Child2;

        // leaf = 0, free node = -1
        internal int Height;
        internal int Parent;

        // to reduce struct size we use Parent for the Free linked-list
        /// <summary>
        /// Next free node
        /// </summary>
        internal int Next
        {
            get { return Parent; }
            set { Parent = value; }
        }

        internal T UserData;


        internal bool IsLeaf()
        {
            return Child1 == DynamicTree<T>.NullNode;
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
        private int _nodeCount;
        private TreeNode<T>[] _nodes;
        private int _root;
        internal const int NullNode = -1;

        /// <summary>
        /// Constructing the tree initializes the node pool.
        /// </summary>
        public DynamicTree()
        {
            _root = NullNode;

            _nodeCapacity = 16;
            _nodeCount = 0;
            _nodes = new TreeNode<T>[_nodeCapacity];

            // Build a linked list for the free list.
            for (int i = 0; i < _nodeCapacity - 1; ++i)
            {
                _nodes[i].Next = i + 1;
                _nodes[i].Height = -1;
            }
            // build last node
            _nodes[_nodeCapacity - 1].Next = NullNode;
            _nodes[_nodeCapacity - 1].Height = -1;
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

                return _nodes[_root].Height;
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
                float rootArea = _nodes[_root].AABB.Perimeter;

                float totalArea = 0.0f;
                for (int i = 0; i < _nodeCapacity; ++i)
                {
                    //TreeNode<T>* node = &_nodes[i];
                    if (_nodes[i].Height < 0)
                    {
                        // Free node in pool
                        continue;
                    }

                    totalArea += _nodes[i].AABB.Perimeter;
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
                    if (_nodes[i].Height <= 1)
                    {
                        continue;
                    }

                    Debug.Assert(_nodes[i].IsLeaf() == false);

                    int child1 = _nodes[i].Child1;
                    int child2 = _nodes[i].Child2;
                    int balance = Math.Abs(_nodes[child2].Height - _nodes[child1].Height);
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
            _nodes[proxyId].AABB.LowerBound = aabb.LowerBound - r;
            _nodes[proxyId].AABB.UpperBound = aabb.UpperBound + r;
            _nodes[proxyId].Height = 0;

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
            Debug.Assert(_nodes[proxyId].IsLeaf());

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

            Debug.Assert(_nodes[proxyId].IsLeaf());

            if (_nodes[proxyId].AABB.Contains(ref aabb))
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

            _nodes[proxyId].AABB = b;

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
            _nodes[proxyId].UserData = userData;
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
            return _nodes[proxyId].UserData;
        }

        /// <summary>
        /// Get the fat AABB for a proxy.
        /// </summary>
        /// <param name="proxyId">The proxy id.</param>
        /// <param name="fatAABB">The fat AABB.</param>
        public void GetFatAABB(int proxyId, out AABB fatAABB)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            fatAABB = _nodes[proxyId].AABB;
        }

        /// <summary>
        /// Get the fat AABB for a proxy.
        /// </summary>
        /// <param name="proxyId">The proxy id.</param>
        /// <returns>The fat AABB.</returns>
        public AABB GetFatAABB(int proxyId)
        {
            Debug.Assert(0 <= proxyId && proxyId < _nodeCapacity);
            return _nodes[proxyId].AABB;
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
            return AABB.TestOverlap(ref _nodes[proxyIdA].AABB, ref _nodes[proxyIdB].AABB);
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

                if (AABB.TestOverlap(ref _nodes[nodeId].AABB, ref aabb))
                {
                    if (_nodes[nodeId].IsLeaf())
                    {
                        bool proceed = callback(nodeId);
                        if (proceed == false)
                        {
                            return;
                        }
                    }
                    else
                    {
                        _queryStack.Push(_nodes[nodeId].Child1);
                        _queryStack.Push(_nodes[nodeId].Child2);
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

                if (AABB.TestOverlap(ref _nodes[nodeId].AABB, ref segmentAABB) == false)
                {
                    continue;
                }

                // Separating axis for segment (Gino, p80).
                // |dot(v, p1 - c)| > dot(|v|, h)
                Vector2 c = _nodes[nodeId].AABB.Center;
                Vector2 h = _nodes[nodeId].AABB.Extents;
                float separation = Math.Abs(Vector2.Dot(new Vector2(-r.Y, r.X), p1 - c)) - Vector2.Dot(absV, h);
                if (separation > 0.0f)
                {
                    continue;
                }

                if (_nodes[nodeId].IsLeaf())
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
                    _raycastStack.Push(_nodes[nodeId].Child1);
                    _raycastStack.Push(_nodes[nodeId].Child2);
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
                TreeNode<T>[] oldNodes = _nodes;
                _nodeCapacity *= 2;
                _nodes = new TreeNode<T>[_nodeCapacity];
                Array.Copy(oldNodes, _nodes, _nodeCount);

                // Build a linked list for the free list.
                for (int i = _nodeCount; i < _nodeCapacity - 1; ++i)
                {
                    _nodes[i].Next = i + 1;
                    _nodes[i].Height = -1;
                }
                // build last node
                _nodes[_nodeCapacity - 1].Next = NullNode;
                _nodes[_nodeCapacity - 1].Height = -1;
                _freeList = _nodeCount;
            }

            // Peel a node off the free list.
            int nodeId = _freeList;
            _freeList = _nodes[nodeId].Next;
            // reinitialize node
            _nodes[nodeId].Parent = NullNode;
            _nodes[nodeId].Child1 = NullNode;
            _nodes[nodeId].Child2 = NullNode;
            _nodes[nodeId].Height = 0;
            _nodes[nodeId].UserData = default(T);
            ++_nodeCount;
            return nodeId;
        }

        private void FreeNode(int nodeId)
        {
            Debug.Assert(0 <= nodeId && nodeId < _nodeCapacity);
            Debug.Assert(0 < _nodeCount);
            _nodes[nodeId].Next = _freeList;
            _nodes[nodeId].Height = -1;
            _freeList = nodeId;
            --_nodeCount;
        }

        private void InsertLeaf(int leaf)
        {
            if (_root == NullNode)
            {
                _root = leaf;
                _nodes[_root].Parent = NullNode;
                return;
            }

            // Find the best sibling for this node
            AABB leafAABB = _nodes[leaf].AABB;
            int index = _root;
            while (_nodes[index].IsLeaf() == false)
            {
                int child1 = _nodes[index].Child1;
                int child2 = _nodes[index].Child2;

                float area = _nodes[index].AABB.Perimeter;

                AABB combinedAABB = new AABB();
                combinedAABB.Combine(ref _nodes[index].AABB, ref leafAABB);
                float combinedArea = combinedAABB.Perimeter;

                // Cost of creating a new parent for this node and the new leaf
                float cost = 2.0f * combinedArea;

                // Minimum cost of pushing the leaf further down the tree
                float inheritanceCost = 2.0f * (combinedArea - area);

                // Cost of descending into child1
                float cost1;
                if (_nodes[child1].IsLeaf())
                {
                    AABB aabb = new AABB();
                    aabb.Combine(ref leafAABB, ref _nodes[child1].AABB);
                    cost1 = aabb.Perimeter + inheritanceCost;
                }
                else
                {
                    AABB aabb = new AABB();
                    aabb.Combine(ref leafAABB, ref _nodes[child1].AABB);
                    float oldArea = _nodes[child1].AABB.Perimeter;
                    float newArea = aabb.Perimeter;
                    cost1 = (newArea - oldArea) + inheritanceCost;
                }

                // Cost of descending into child2
                float cost2;
                if (_nodes[child2].IsLeaf())
                {
                    AABB aabb = new AABB();
                    aabb.Combine(ref leafAABB, ref _nodes[child2].AABB);
                    cost2 = aabb.Perimeter + inheritanceCost;
                }
                else
                {
                    AABB aabb = new AABB();
                    aabb.Combine(ref leafAABB, ref _nodes[child2].AABB);
                    float oldArea = _nodes[child2].AABB.Perimeter;
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
            int oldParent = _nodes[sibling].Parent;
            int newParent = AllocateNode();
            _nodes[newParent].Parent = oldParent;
            _nodes[newParent].UserData = default(T);
            _nodes[newParent].AABB.Combine(ref leafAABB, ref _nodes[sibling].AABB);
            _nodes[newParent].Height = _nodes[sibling].Height + 1;

            if (oldParent != NullNode)
            {
                // The sibling was not the root.
                if (_nodes[oldParent].Child1 == sibling)
                {
                    _nodes[oldParent].Child1 = newParent;
                }
                else
                {
                    _nodes[oldParent].Child2 = newParent;
                }

                _nodes[newParent].Child1 = sibling;
                _nodes[newParent].Child2 = leaf;
                _nodes[sibling].Parent = newParent;
                _nodes[leaf].Parent = newParent;
            }
            else
            {
                // The sibling was the root.
                _nodes[newParent].Child1 = sibling;
                _nodes[newParent].Child2 = leaf;
                _nodes[sibling].Parent = newParent;
                _nodes[leaf].Parent = newParent;
                _root = newParent;
            }

            // Walk back up the tree fixing heights and AABBs
            index = _nodes[leaf].Parent;
            while (index != NullNode)
            {
                index = Balance(index);

                int child1 = _nodes[index].Child1;
                int child2 = _nodes[index].Child2;

                Debug.Assert(child1 != NullNode);
                Debug.Assert(child2 != NullNode);

                _nodes[index].Height = 1 + Math.Max(_nodes[child1].Height, _nodes[child2].Height);
                _nodes[index].AABB.Combine(ref _nodes[child1].AABB, ref _nodes[child2].AABB);

                index = _nodes[index].Parent;
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

            int parent = _nodes[leaf].Parent;
            int grandParent = _nodes[parent].Parent;
            int sibling;
            if (_nodes[parent].Child1 == leaf)
            {
                sibling = _nodes[parent].Child2;
            }
            else
            {
                sibling = _nodes[parent].Child1;
            }

            if (grandParent != NullNode)
            {
                // Destroy parent and connect sibling to grandParent.
                if (_nodes[grandParent].Child1 == parent)
                {
                    _nodes[grandParent].Child1 = sibling;
                }
                else
                {
                    _nodes[grandParent].Child2 = sibling;
                }
                _nodes[sibling].Parent = grandParent;
                FreeNode(parent);

                // Adjust ancestor bounds.
                int index = grandParent;
                while (index != NullNode)
                {
                    index = Balance(index);

                    int child1 = _nodes[index].Child1;
                    int child2 = _nodes[index].Child2;

                    _nodes[index].AABB.Combine(ref _nodes[child1].AABB, ref _nodes[child2].AABB);
                    _nodes[index].Height = 1 + Math.Max(_nodes[child1].Height, _nodes[child2].Height);

                    index = _nodes[index].Parent;
                }
            }
            else
            {
                _root = sibling;
                _nodes[sibling].Parent = NullNode;
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
            if (_nodes[iN].IsLeaf() || _nodes[iN].Height < 2)
            {
                return iN;
            }

            int iA = _nodes[iN].Child1;
            int iB = _nodes[iN].Child2;
            Debug.Assert(0 <= iA && iA < _nodeCapacity);
            Debug.Assert(0 <= iB && iB < _nodeCapacity);

            //TreeNode<T>* A = &_nodes[iA];
            //TreeNode<T>* B = &_nodes[iB];

            int balance = _nodes[iB].Height - _nodes[iA].Height;

            // Rotate B up
            if (balance > 1)
            {
                int iP = _nodes[iN].Parent;
                int iBA = _nodes[iB].Child1;
                int iBB = _nodes[iB].Child2;
                //TreeNode<T>* P  = &_nodes[iN->Parent];
                //TreeNode<T>* BA = &_nodes[iBA];
                //TreeNode<T>* BB = &_nodes[iBB];
                Debug.Assert(0 <= iBA && iBA < _nodeCapacity);
                Debug.Assert(0 <= iBB && iBB < _nodeCapacity);

                // Swap N and B
                _nodes[iB].Child1 = iN;
                _nodes[iB].Parent = _nodes[iN].Parent;
                _nodes[iN].Parent = iB;

                // N's old parent should point to B
                if (iP != NullNode)
                {
                    if (_nodes[iP].Child1 == iN)
                    {
                        _nodes[iP].Child1 = iB;
                    }
                    else
                    {
                        Debug.Assert(_nodes[iP].Child2 == iN);
                        _nodes[iP].Child2 = iB;
                    }
                }
                else
                {
                    _root = iB;
                }

                // Rotate
                if (_nodes[iBA].Height > _nodes[iBB].Height)
                {
                    _nodes[iB].Child2 = iBA;
                    _nodes[iN].Child2 = iBB;
                    _nodes[iBB].Parent = iN;
                    _nodes[iN].AABB.Combine(ref _nodes[iA].AABB, ref _nodes[iBB].AABB);
                    _nodes[iB].AABB.Combine(ref _nodes[iN].AABB, ref _nodes[iBA].AABB);

                    _nodes[iN].Height = 1 + Math.Max(_nodes[iA].Height, _nodes[iBB].Height);
                    _nodes[iB].Height = 1 + Math.Max(_nodes[iN].Height, _nodes[iBA].Height);
                }
                else
                {
                    _nodes[iB].Child2 = iBB;
                    _nodes[iN].Child2 = iBA;
                    _nodes[iBA].Parent = iN;
                    _nodes[iN].AABB.Combine(ref _nodes[iA].AABB, ref _nodes[iBA].AABB);
                    _nodes[iB].AABB.Combine(ref _nodes[iN].AABB, ref _nodes[iBB].AABB);

                    _nodes[iN].Height = 1 + Math.Max(_nodes[iA].Height, _nodes[iBA].Height);
                    _nodes[iB].Height = 1 + Math.Max(_nodes[iN].Height, _nodes[iBB].Height);
                }

                return iB;
            }

            // Rotate A up
            if (balance < -1)
            {
                int iP = _nodes[iN].Parent;
                int iAA = _nodes[iA].Child1;
                int iAB = _nodes[iA].Child2;
                //TreeNode<T>* P  = &_nodes[iN->Parent];
                //TreeNode<T>* AA = &_nodes[iAA];
                //TreeNode<T>* AB = &_nodes[iAB];
                Debug.Assert(0 <= iAA && iAA < _nodeCapacity);
                Debug.Assert(0 <= iAB && iAB < _nodeCapacity);

                // Swap N and A
                _nodes[iA].Child1 = iN;
                _nodes[iA].Parent = _nodes[iN].Parent;
                _nodes[iN].Parent = iA;

                // N's old parent should point to A
                if (iP != NullNode)
                {
                    if (_nodes[iP].Child1 == iN)
                    {
                        _nodes[iP].Child1 = iA;
                    }
                    else
                    {
                        Debug.Assert(_nodes[iP].Child2 == iN);
                        _nodes[iP].Child2 = iA;
                    }
                }
                else
                {
                    _root = iA;
                }

                // Rotate
                if (_nodes[iAA].Height > _nodes[iAB].Height)
                {
                    _nodes[iA].Child2 = iAA;
                    _nodes[iN].Child1 = iAB;
                    _nodes[iAB].Parent = iN;
                    _nodes[iN].AABB.Combine(ref _nodes[iB].AABB, ref  _nodes[iAB].AABB);
                    _nodes[iA].AABB.Combine(ref _nodes[iN].AABB, ref _nodes[iAA].AABB);

                    _nodes[iN].Height = 1 + Math.Max(_nodes[iB].Height, _nodes[iAB].Height);
                    _nodes[iA].Height = 1 + Math.Max(_nodes[iN].Height, _nodes[iAA].Height);
                }
                else
                {
                    _nodes[iA].Child2 = iAB;
                    _nodes[iN].Child1 = iAA;
                    _nodes[iAA].Parent = iN;
                    _nodes[iN].AABB.Combine(ref _nodes[iB].AABB, ref _nodes[iAA].AABB);
                    _nodes[iA].AABB.Combine(ref _nodes[iN].AABB, ref _nodes[iAB].AABB);

                    _nodes[iN].Height = 1 + Math.Max(_nodes[iB].Height, _nodes[iAA].Height);
                    _nodes[iA].Height = 1 + Math.Max(_nodes[iN].Height, _nodes[iAB].Height);
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

            if (_nodes[nodeId].IsLeaf())
            {
                return 0;
            }

            int height1 = ComputeHeight(_nodes[nodeId].Child1);
            int height2 = ComputeHeight(_nodes[nodeId].Child2);
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
                Debug.Assert(_nodes[index].Parent == NullNode);
            }

            //TreeNode<T>* node = &_nodes[index];

            int child1 = _nodes[index].Child1;
            int child2 = _nodes[index].Child2;

            if (_nodes[index].IsLeaf())
            {
                Debug.Assert(child1 == NullNode);
                Debug.Assert(child2 == NullNode);
                Debug.Assert(_nodes[index].Height == 0);
                return;
            }

            Debug.Assert(0 <= child1 && child1 < _nodeCapacity);
            Debug.Assert(0 <= child2 && child2 < _nodeCapacity);

            Debug.Assert(_nodes[child1].Parent == index);
            Debug.Assert(_nodes[child2].Parent == index);

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

            int child1 = _nodes[index].Child1;
            int child2 = _nodes[index].Child2;

            if (_nodes[index].IsLeaf())
            {
                Debug.Assert(child1 == NullNode);
                Debug.Assert(child2 == NullNode);
                Debug.Assert(_nodes[index].Height == 0);
                return;
            }

            Debug.Assert(0 <= child1 && child1 < _nodeCapacity);
            Debug.Assert(0 <= child2 && child2 < _nodeCapacity);

            int height1 = _nodes[child1].Height;
            int height2 = _nodes[child2].Height;
            int height = 1 + Math.Max(height1, height2);
            Debug.Assert(_nodes[index].Height == height);

            AABB AABB = new AABB();
            AABB.Combine(ref _nodes[child1].AABB, ref _nodes[child2].AABB);

            Debug.Assert(AABB.LowerBound == _nodes[index].AABB.LowerBound);
            Debug.Assert(AABB.UpperBound == _nodes[index].AABB.UpperBound);

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
                freeIndex = _nodes[freeIndex].Next;
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
                if (_nodes[i].Height < 0)
                {
                    // free node in pool
                    continue;
                }

                if (_nodes[i].IsLeaf())
                {
                    _nodes[i].Parent = NullNode;
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
                    AABB AABBi = _nodes[nodes[i]].AABB;

                    for (int j = i + 1; j < count; ++j)
                    {
                        AABB AABBj = _nodes[nodes[j]].AABB;
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
                _nodes[parentIndex].Child1 = index1;
                _nodes[parentIndex].Child2 = index2;
                _nodes[parentIndex].Height = 1 + Math.Max(_nodes[index1].Height, _nodes[index2].Height);
                _nodes[parentIndex].AABB.Combine(ref _nodes[index1].AABB, ref _nodes[index2].AABB);
                _nodes[parentIndex].Parent = NullNode;

                _nodes[index1].Parent = parentIndex;
                _nodes[index2].Parent = parentIndex;

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
                _nodes[i].AABB.LowerBound -= newOrigin;
                _nodes[i].AABB.UpperBound -= newOrigin;
            }
        }
    }
}