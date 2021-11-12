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

using tainicom.Aether.Physics2D.Samples.Testbed.Framework;

namespace tainicom.Aether.Physics2D.Samples.Testbed.Tests
{
    public static class TestEntries
    {
        public static TestEntry[] TestList =
        {
            //Original tests
            new TestEntry("Continuous Test", typeof(ContinuousTest)),
            new TestEntry("Time of Impact", typeof(TimeOfImpactTest)),
            new TestEntry("Motor joint", typeof(MotorJointTest)),
            new TestEntry("One-Sided Platform", typeof(OneSidedPlatformTest)),
            //new TestEntry("Dump Shell", typeof(DumpShellTest)),
            new TestEntry("Mobile", typeof(MobileTest)),
            new TestEntry("MobileBalanced", typeof(MobileBalancedTest)),
            new TestEntry("Ray-Cast", typeof(RayCastTest)),
            new TestEntry("Conveyor Belt", typeof(ConveyorBeltTest)),
            new TestEntry("Gears", typeof(GearsTest)),
            new TestEntry("ConvexHull", typeof(ConvexHullTest)),
            new TestEntry("Varying Restitution", typeof(VaryingRestitutionTest)),
            new TestEntry("Tumbler", typeof(TumblerTest)),
            new TestEntry("Tiles", typeof(TilesTest)),
            new TestEntry("Cantilever", typeof(CantileverTest)),
            new TestEntry("Character collision", typeof(CharacterCollisionTest)),
            new TestEntry("Edge Test", typeof(EdgeTest)),
            new TestEntry("Body Types", typeof(BodyTypesTest)),
            new TestEntry("Shape Editing", typeof(ShapeEditingTest)),
            new TestEntry("Car", typeof(CarTest)),
            new TestEntry("Apply Force", typeof(ApplyForceTest)),
            new TestEntry("Prismatic", typeof(PrismaticTest)),
            new TestEntry("Vertical Stack", typeof(VerticalStackTest)),
            new TestEntry("SphereStack", typeof(SphereStackTest)),
            new TestEntry("Revolute", typeof(RevoluteTest)),
            new TestEntry("Pulleys", typeof(PulleysTest)),
            new TestEntry("Polygon Shapes", typeof(PolyShapesTest)),
            new TestEntry("Web", typeof(WebTest)),
            new TestEntry("RopeJoint", typeof(RopeTest)),
            new TestEntry("Pinball", typeof(PinballTest)),
            new TestEntry("Bullet Test", typeof(BulletTest)),
            new TestEntry("Confined", typeof(ConfinedTest)),
            new TestEntry("Pyramid", typeof(PyramidTest)),
            new TestEntry("Theo Jansen's Walker", typeof(TheoJansenTest)),
            new TestEntry("Edge Shapes", typeof(EdgeShapesTest)),
            new TestEntry("PolyCollision", typeof(PolyCollisionTest)),
            new TestEntry("Bridge", typeof(BridgeTest)),
            new TestEntry("Breakable", typeof(BreakableTest)),
            new TestEntry("Chain", typeof(ChainTest)),
            new TestEntry("Collision Filtering", typeof(CollisionFilteringTest)),
            new TestEntry("Collision Processing", typeof(CollisionProcessingTest)),
            new TestEntry("Compound Shapes", typeof(CompoundShapesTest)),
            new TestEntry("Distance Test", typeof(DistanceTest)),
            new TestEntry("Dominos", typeof(DominosTest)),
            new TestEntry("Dynamic Tree", typeof(DynamicTreeTest)),
            new TestEntry("Sensor Test", typeof(SensorTest)),
            new TestEntry("Slider Crank", typeof(SliderCrankTest)),
            new TestEntry("Varying Friction", typeof(VaryingFrictionTest)),
#if WINDOWS
            //new TestEntry("Add Pair Stress Test", typeof(AddPairTest)),
#endif

            // AetherPhysics2D tests
            new TestEntry("Multithread Solvers() Test 1", typeof(Multithread1Test)),            
            new TestEntry("Multithread Solvers() Test 2", typeof(Multithread2Test)),
            new TestEntry("Multithread Worlds Test", typeof(MultithreadWorldsTest)),
            new TestEntry("Body Pool", typeof(BodyPoolTest)),
            new TestEntry("Collision Test", typeof(CollisionTest)),
            new TestEntry("The Leaning Tower of Lire Test", typeof(TheLeaningTowerofLireTest)),
            new TestEntry("Sparse bodies Test", typeof(SparseBodiesTest)),
            new TestEntry("Sparse bodies with many fixtures Test", typeof(SparseBodiesWithManyFixturesTest)),

            //FPE tests
            new TestEntry("YuPeng Polygon", typeof(YuPengPolygonTest)),
            new TestEntry("Path Test", typeof(PathTest)),
            new TestEntry("Cutting of polygons", typeof(CuttingTest)),
            new TestEntry("Gravity Controller Test", typeof(GravityControllerTest)),
            new TestEntry("Texture to Vertices", typeof(TextureVerticesTest)),
            new TestEntry("BreakableBody test", typeof(BreakableBodyTest)),
            new TestEntry("Rounded rectangle", typeof(RoundedRectangle)),
            new TestEntry("Angle Joint", typeof(AngleJointTest)),
            new TestEntry("Explosion", typeof(ExplosionTest)),
            new TestEntry("Sphere benchmark", typeof(CircleBenchmarkTest)),
            new TestEntry("Edgeshape benchmark", typeof(EdgeShapeBenchmarkTest)),
            new TestEntry("Circle penetration", typeof(CirclePenetrationTest)),
            new TestEntry("Clone Test", typeof(CloneTest)),
            new TestEntry("Serialization Test", typeof(SerializationTest)),
            new TestEntry("Deletion test", typeof(DeletionTest)),
            new TestEntry("Buoyancy test", typeof(BuoyancyTest)),
            new TestEntry("Convex hull test", typeof(ConvexHullTest2)),
            new TestEntry("Simple Wind Force Test", typeof(SimpleWindForceTest)),
            new TestEntry("Quad Tree BroadPhase test", typeof(QuadTreeTest)),
            new TestEntry("Simplification", typeof(SimplificationTest)),
#if WINDOWS
            //new TestEntry("Triangulation", typeof(TriangulationTest)), // Test fails
            new TestEntry("Destructible Terrain Test", typeof(DestructibleTerrainTest)),
#endif
            new TestEntry("Check polygon", typeof(CheckPolygonTest)),
            new TestEntry("Fluids", typeof(FluidsTest)),
        };
    }
}