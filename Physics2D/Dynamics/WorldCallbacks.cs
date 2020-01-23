﻿// Copyright (c) 2017 Kastellanos Nikolaos

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

using Microsoft.Xna.Framework;
using tainicom.Aether.Physics2D.Collision;
using tainicom.Aether.Physics2D.Controllers;
using tainicom.Aether.Physics2D.Dynamics.Contacts;
using tainicom.Aether.Physics2D.Dynamics.Joints;

namespace tainicom.Aether.Physics2D.Dynamics
{
    /// <summary>
    /// Called for each fixture found in the query.
    /// <returns>true: Continues the query, false: Terminate the query</returns>
    /// </summary>
    public delegate bool QueryCallback(Fixture fixture);

    /// <summary>
    /// Called for each fixture found in the query. You control how the ray cast
    /// proceeds by returning a float:
    /// return -1: ignore this fixture and continue
    /// return 0: terminate the ray cast
    /// return fraction: clip the ray to this point
    /// return 1: don't clip the ray and continue
    /// @param fixture the fixture hit by the ray
    /// @param point the point of initial intersection
    /// @param normal the normal vector at the point of intersection
    /// @return 0 to terminate, fraction to clip the ray for closest hit, 1 to continue
    /// </summary>
    public delegate float RayCastCallback(Fixture fixture, Vector2 point, Vector2 normal, float fraction);

    /// <summary>
    /// This delegate is called when a contact is deleted
    /// </summary>
    public delegate void EndContactDelegate(Contact contact);

    /// <summary>
    /// This delegate is called when a contact is created
    /// </summary>
    public delegate bool BeginContactDelegate(Contact contact);

    public delegate void PreSolveDelegate(Contact contact, ref Manifold oldManifold);

    public delegate void PostSolveDelegate(Contact contact, ContactVelocityConstraint impulse);

    public delegate void FixtureDelegate(World sender, Body body, Fixture fixture);

    public delegate void JointDelegate(World sender, Joint joint);

    public delegate void BodyDelegate(World sender, Body body);

    public delegate void ControllerDelegate(World sender, Controller controller);

    public delegate bool CollisionFilterDelegate(Fixture fixtureA, Fixture fixtureB);

    public delegate void BroadphaseDelegate(int proxyIdA, int proxyIdB);

    public delegate bool BeforeCollisionEventHandler(Fixture sender, Fixture other);

    public delegate bool OnCollisionEventHandler(Fixture sender, Fixture other, Contact contact);

    public delegate void AfterCollisionEventHandler(Fixture sender, Fixture other, Contact contact, ContactVelocityConstraint impulse);

    public delegate void OnSeparationEventHandler(Fixture sender, Fixture other, Contact contact);
}