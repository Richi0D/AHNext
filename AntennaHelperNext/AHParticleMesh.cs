using System;
using System.Collections.Generic;
using UnityEngine;


namespace AntennaHelperNext
{
	
	// This class is used to store predefined particle meshes for the map circles. reuse them.
	public static class DefinedParticleMeshes
	{
		public static ParticleMesh SmallCloud;
		public static ParticleMesh MediumCloud;
		public static ParticleMesh LargeCloud;

		private static bool initialized = false;
        
		public static void Init()
		{
			if (initialized) return;
			initialized = true;

			SmallCloud = new ParticleMesh(GenerateSpherePoints(2000));
			MediumCloud = new ParticleMesh(GenerateSpherePoints(20000));
			LargeCloud = new ParticleMesh(GenerateSpherePoints(50000));
		}
        
		private static List<Vector3> GenerateSpherePoints(int count)
		{
			var pts = new List<Vector3>(count);
			for (int i = 0; i < count; i++)
			{
				Vector3 p = UnityEngine.Random.onUnitSphere; // Unit sphere, we scale later
				pts.Add(p);
			}
			return pts;
		}
	}    
	
	
	// store an arbitrary number of static particles in a set of meshes
	public sealed class ParticleMesh
	{
		// create a particle mesh from a set of points
		public ParticleMesh(List<Vector3> points)
		{
			this.points = points;
		}
		
		void Compile()
		{
			// max number of particles that can be stored in a unity mesh
			const int max_particles = 64000;

			// create the set of meshes
			meshes = new List<Mesh>(points.Count / max_particles + 1);
			Mesh m;
			List<Vector3> t_points = new List<Vector3>(max_particles);
			List<int> t_indexes = new List<int>(max_particles);
			for (var i = 0; i < points.Count; ++i)
			{
				t_points.Add(points[i]);
				t_indexes.Add(t_indexes.Count);
				if (t_indexes.Count >= max_particles || i == points.Count - 1)
				{
					m = new Mesh();
					m.SetVertices(t_points);
					m.SetIndices(t_indexes.ToArray(), MeshTopology.Points, 0);
					meshes.Add(m);
					t_points.Clear();
					t_indexes.Clear();
				}
			}
			points = null;
		}
		
		// render all the meshes
		public void Render(Matrix4x4 m)
		{
			if (meshes == null)
			{
				Compile();
			}

			for (var i = 0; i < meshes.Count; ++i)
			{
				Graphics.DrawMeshNow(meshes[i], m);
			}
		}

		List<Vector3> points;     // set of points
		List<Mesh> meshes;        // set of meshes
	}
}