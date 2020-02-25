﻿/* Original source Farseer Physics Engine:
 * Copyright (c) 2014 Ian Qvist, http://farseerphysics.codeplex.com
 * Microsoft Permissive License (Ms-PL) v1.1
 */

using System;
using tainicom.Aether.Physics2D.Common;
using tainicom.Aether.Physics2D.Dynamics;
#if XNAAPI
using Vector2 = Microsoft.Xna.Framework.Vector2;
#endif

namespace tainicom.Aether.Physics2D.Collision
{
    public interface IBroadPhase
    {
        int ProxyCount { get; }
        void UpdatePairs(BroadphaseDelegate callback);

        bool TestOverlap(int proxyIdA, int proxyIdB);

        int AddProxy(ref AABB aabb);

        void RemoveProxy(int proxyId);

        void MoveProxy(int proxyId, ref AABB aabb, Vector2 displacement);

        void SetProxy(int proxyId, ref FixtureProxy proxy);

        FixtureProxy GetProxy(int proxyId);

        void TouchProxy(int proxyId);

        void GetFatAABB(int proxyId, out AABB aabb);

        void Query(BroadPhaseQueryCallback callback, ref AABB aabb);

        void RayCast(BroadPhaseRayCastCallback callback, ref RayCastInput input);

        void ShiftOrigin(Vector2 newOrigin);
    }

    public delegate bool BroadPhaseQueryCallback(int proxyId);
    public delegate float BroadPhaseRayCastCallback(ref RayCastInput input, int proxyId);
}