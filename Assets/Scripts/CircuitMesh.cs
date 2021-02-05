using System.Collections.Generic;
using PathCreation;
using PathCreation.Examples;
using PathCreation.Utility;
using UnityEngine;

public class CircuitMesh : PathSceneTool {
    [Header("Road settings")]
    public float roadWidth = .4f;
    [Range(0, .5f)]
    public float roadThickness = .15f;

    [Header("wall settings")]
    public float WallsHeigth = 1f;
    [Range(0, .5f)]
    public float WallsThickness = .15f;

    public bool flattenSurface;

    [Header("Material settings")]
    public Material roadMaterial;
    public Material undersideMaterial;
    public float textureTiling = 1;
    public Material wallsMaterial;

    private GameObject roadMeshHolder;
    MeshFilter roadMeshFilter;
    MeshRenderer roadMeshRenderer;
    Mesh roadMesh;
    MeshCollider roadMeshCollider;

    private GameObject wallsMeshHolder;
    MeshFilter wallsMeshFilter;
    MeshRenderer wallsMeshRenderer;
    Mesh wallsMesh;
    MeshCollider wallsMeshCollider;

    protected override void PathUpdated() {
        if (pathCreator != null) {
            AssignMeshComponents();
            AssignMaterials();
            CreateRoadMesh();
            CreateWallsMeshs();
        }
    }

    void CreateWallsMeshs() {
        Vector3[] verts = new Vector3[path.NumPoints * 16];
        Vector2[] uvs = new Vector2[verts.Length];
        Vector3[] normals = new Vector3[verts.Length];

        int numTris = 4 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 4 : 0);
        int[] TopBottomTriangles = new int[numTris * 2 *3];
        int[] sideTriangles = new int[numTris * 2 * 3];

        // Vertices are layed out loke that for one points (side view):
        // 4 5   6 7
        // | | . | | (+ 8 for duplicated flat shading sides) 
        // 0 1   2 3  
           
        int[] topMap = { 4,20,5, 5,20,21 , 6,22,7, 7,22,23 };
        int[] sideLeftMap = { 0,16,4, 4,16,20, 1,5,17, 17,5,21 };

        // add 8 to each (duplicated flat shading)
        for (int i = 0; i < sideLeftMap.Length; ++i) {
            sideLeftMap[i] = sideLeftMap[i] + 8;
        }

        bool usePathNormals = !(path.space == PathSpace.xyz && flattenSurface);

        for (int i = 0; i < path.NumPoints; ++i) {

            int vertIndex = i*16;

            Vector3 localUp = (usePathNormals) ? Vector3.Cross(path.GetTangent(i), path.GetNormal(i)) : path.up;
            Vector3 localRight = (usePathNormals) ? path.GetNormal(i) : Vector3.Cross(localUp, path.GetTangent(i));

            // Find position to left and right of current path vertex
            Vector3 leftInPt = path.GetPoint(i) - localRight * Mathf.Abs(roadWidth); // left interior
            Vector3 rightPt = path.GetPoint(i) + localRight * Mathf.Abs(roadWidth); // right interior
            Vector3 leftExtPt = path.GetPoint(i) - localRight * (Mathf.Abs(roadWidth) + Mathf.Abs(WallsThickness)); // left exterior
            Vector3 rightExtPt = path.GetPoint(i) + localRight * (Mathf.Abs(roadWidth) + Mathf.Abs(WallsThickness)); // right exterior

            // Bottom vertices
            verts[vertIndex + 0] = leftExtPt;
            verts[vertIndex + 1] = leftInPt;
            verts[vertIndex + 2] = rightPt;
            verts[vertIndex + 3] = rightExtPt;

            // Top vertices
            for (int j = 0; j < 4; ++j) {
                verts[vertIndex + 4 + j] = verts[vertIndex + 0 + j] + localUp * WallsHeigth;
            }

            // Duplicate vertices to get flat shading for sides of road
            for (int j = 0; j < 8; ++j) {
                verts[vertIndex + 8 + j] = verts[vertIndex + 0 + j];
            }

            // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
            uvs[vertIndex + 0] = new Vector2(0, path.times[i]);
            uvs[vertIndex + 1] = new Vector2(1, path.times[i]);

            // Down normals
            for (int j = 0; j < 4; ++j) {
                normals[vertIndex + j] = -localUp;
            }
            // Up normals
            for (int j = 0; j < 4; ++j) {
                normals[vertIndex + 4 + j] = localUp;
            }
            // Left and right normals
            for (int j = 0; j < 8; ++j) {
                normals[vertIndex + 8 + j] = j%2 == 0 ? -localRight : localRight;
            }

            // Set triangle indices
            if (i < path.NumPoints - 1 || path.isClosedLoop) {
                for (int j = 0; j < topMap.Length; j++) {
                    TopBottomTriangles[i * topMap.Length*2 + j] = (vertIndex + topMap[j]) % verts.Length; // top
                    // reverse triangle map for bottom so that triangles wind the other way
                    TopBottomTriangles[i * topMap.Length*2 + topMap.Length + j] = (vertIndex + topMap[topMap.Length - 1 - j] - 4) % verts.Length; // bottom
                }
                
                for (int j = 0; j < sideLeftMap.Length; j++) {
                    sideTriangles[i * sideLeftMap.Length*2 + j] = (vertIndex + sideLeftMap[j]) % verts.Length; // left wall
                    sideTriangles[i * sideLeftMap.Length*2 + sideLeftMap.Length + j] = (vertIndex + sideLeftMap[j] + 2) % verts.Length; // right wall
                }
            }
        }

        wallsMesh.Clear();
        wallsMesh.vertices = verts;
        wallsMesh.uv = uvs;
        wallsMesh.normals = normals;
        wallsMesh.subMeshCount = 2;
        wallsMesh.SetTriangles(TopBottomTriangles, 0);
        wallsMesh.SetTriangles(sideTriangles, 1);
        wallsMesh.RecalculateBounds();
    }

    void CreateRoadMesh() {
        Vector3[] verts = new Vector3[path.NumPoints * 8];
        Vector2[] uvs = new Vector2[verts.Length];
        Vector3[] normals = new Vector3[verts.Length];

        int numTris = 2 * (path.NumPoints - 1) + ((path.isClosedLoop) ? 2 : 0);
        int[] roadTriangles = new int[numTris * 3];
        int[] underRoadTriangles = new int[numTris * 3];
        int[] sideOfRoadTriangles = new int[numTris * 2 * 3];

        int vertIndex = 0;
        int triIndex = 0;

        // Vertices for the top of the road are layed out:
        // 0  1
        // 8  9
        // and so on... So the triangle map 0,8,1 for example, defines a triangle from top left to bottom left to bottom right.
        int[] triangleMap = { 0, 8, 1, 1, 8, 9 };
        int[] sidesTriangleMap = { 4, 6, 14, 12, 4, 14, 5, 15, 7, 13, 15, 5 };

        bool usePathNormals = !(path.space == PathSpace.xyz && flattenSurface);

        for (int i = 0; i < path.NumPoints; i++) {
            Vector3 localUp = (usePathNormals) ? Vector3.Cross(path.GetTangent(i), path.GetNormal(i)) : path.up;
            Vector3 localRight = (usePathNormals) ? path.GetNormal(i) : Vector3.Cross(localUp, path.GetTangent(i));

            // Find position to left and right of current path vertex
            Vector3 vertSideA = path.GetPoint(i) - localRight * Mathf.Abs(roadWidth);
            Vector3 vertSideB = path.GetPoint(i) + localRight * Mathf.Abs(roadWidth);

            // Add top of road vertices
            verts[vertIndex + 0] = vertSideA;
            verts[vertIndex + 1] = vertSideB;

            // Add bottom of road vertices
            verts[vertIndex + 2] = vertSideA - localUp * roadThickness;
            verts[vertIndex + 3] = vertSideB - localUp * roadThickness;

            // Duplicate vertices to get flat shading for sides of road
            verts[vertIndex + 4] = verts[vertIndex + 0];
            verts[vertIndex + 5] = verts[vertIndex + 1];
            verts[vertIndex + 6] = verts[vertIndex + 2];
            verts[vertIndex + 7] = verts[vertIndex + 3];

            // Set uv on y axis to path time (0 at start of path, up to 1 at end of path)
            uvs[vertIndex + 0] = new Vector2(0, path.times[i]);
            uvs[vertIndex + 1] = new Vector2(1, path.times[i]);

            // Top of road normals
            normals[vertIndex + 0] = localUp;
            normals[vertIndex + 1] = localUp;
            // Bottom of road normals
            normals[vertIndex + 2] = -localUp;
            normals[vertIndex + 3] = -localUp;
            // Sides of road normals
            normals[vertIndex + 4] = -localRight;
            normals[vertIndex + 5] = localRight;
            normals[vertIndex + 6] = -localRight;
            normals[vertIndex + 7] = localRight;

            // Set triangle indices
            if (i < path.NumPoints - 1 || path.isClosedLoop) {
                for (int j = 0; j < triangleMap.Length; j++) {
                    roadTriangles[triIndex + j] = (vertIndex + triangleMap[j]) % verts.Length;
                    // reverse triangle map for under road so that triangles wind the other way and are visible from underneath
                    underRoadTriangles[triIndex + j] = (vertIndex + triangleMap[triangleMap.Length - 1 - j] + 2) % verts.Length;
                }
                for (int j = 0; j < sidesTriangleMap.Length; j++) {
                    sideOfRoadTriangles[triIndex * 2 + j] = (vertIndex + sidesTriangleMap[j]) % verts.Length;
                }
            }

            vertIndex += 8;
            triIndex += 6;
        }

        roadMesh.Clear();
        roadMesh.vertices = verts;
        roadMesh.uv = uvs;
        roadMesh.normals = normals;
        roadMesh.subMeshCount = 3;
        roadMesh.SetTriangles(roadTriangles, 0);
        roadMesh.SetTriangles(underRoadTriangles, 1);
        roadMesh.SetTriangles(sideOfRoadTriangles, 2);
        roadMesh.RecalculateBounds();
    }

    void AssignMeshComponents() {
        AssignRoadMeshComponents();
        AssignWallsMeshComponents();
    }

    // Add MeshRenderer and MeshFilter components to this gameobject if not already attached
    void AssignRoadMeshComponents() {

        if (roadMeshHolder == null) {
            roadMeshHolder = new GameObject("Road Mesh Holder");
            roadMeshHolder.transform.parent = this.GetComponent<Transform>();
        }

        roadMeshHolder.transform.rotation = Quaternion.identity;
        roadMeshHolder.transform.position = Vector3.zero;
        roadMeshHolder.transform.localScale = Vector3.one;

        // Ensure mesh renderer and filter components are assigned
        if (!roadMeshHolder.gameObject.GetComponent<MeshFilter>()) {
            roadMeshHolder.gameObject.AddComponent<MeshFilter>();
        }

        if (!roadMeshHolder.GetComponent<MeshRenderer>()) {
            roadMeshHolder.gameObject.AddComponent<MeshRenderer>();
        }

        if (!roadMeshHolder.GetComponent<MeshCollider>()) {
            roadMeshHolder.gameObject.AddComponent<MeshCollider>();
        }

        roadMeshRenderer = roadMeshHolder.GetComponent<MeshRenderer>();
        roadMeshFilter = roadMeshHolder.GetComponent<MeshFilter>();
        roadMeshCollider = roadMeshHolder.GetComponent<MeshCollider>();
        if (roadMesh == null) {
            roadMesh = new Mesh();
        }
        roadMeshFilter.sharedMesh = roadMesh;
        roadMeshCollider.sharedMesh = roadMesh;
    }

    void AssignWallsMeshComponents() {

        if (wallsMeshHolder == null) {
            wallsMeshHolder = new GameObject("walls Mesh Holder");
            wallsMeshHolder.transform.parent = this.GetComponent<Transform>();
        }

        wallsMeshHolder.transform.rotation = Quaternion.identity;
        wallsMeshHolder.transform.position = Vector3.zero;
        wallsMeshHolder.transform.localScale = Vector3.one;

        // Ensure mesh renderer and filter components are assigned
        if (!wallsMeshHolder.gameObject.GetComponent<MeshFilter>()) {
            wallsMeshHolder.gameObject.AddComponent<MeshFilter>();
        }

        if (!wallsMeshHolder.GetComponent<MeshRenderer>()) {
            wallsMeshHolder.gameObject.AddComponent<MeshRenderer>();
        }

        if (!wallsMeshHolder.GetComponent<MeshCollider>()) {
            wallsMeshHolder.gameObject.AddComponent<MeshCollider>();
        }

        wallsMeshRenderer = wallsMeshHolder.GetComponent<MeshRenderer>();
        wallsMeshFilter = wallsMeshHolder.GetComponent<MeshFilter>();
        wallsMeshCollider = wallsMeshHolder.GetComponent<MeshCollider>();
        if (wallsMesh == null) {
            wallsMesh = new Mesh();
        }
        wallsMeshFilter.sharedMesh = wallsMesh;
        wallsMeshCollider.sharedMesh = wallsMesh;
    }

    void AssignMaterials() {
        if (roadMaterial != null && undersideMaterial != null) {
            roadMeshRenderer.sharedMaterials = new Material[] { roadMaterial, undersideMaterial, undersideMaterial };
            roadMeshRenderer.sharedMaterials[0].mainTextureScale = new Vector3(1, textureTiling);
        }

        if (wallsMaterial != null) {
            wallsMeshRenderer.sharedMaterials = new Material[] { wallsMaterial, wallsMaterial };
        }
    }

}
