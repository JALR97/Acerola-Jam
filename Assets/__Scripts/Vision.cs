using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;
public class Vision : MonoBehaviour
{
//-----------------//Data structures//-----------------//
//enums


//structs
    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dist, float _angle)
        {
            hit = _hit;
            point = _point;
            distance = _dist;
            angle = _angle;
        }
    }

//-----------------//Components//-----------------//
//Internal Components
    public MeshFilter viewMeshFilter;
    private Mesh viewMesh;

//Prefabs


//External References
    [SerializeField] private Transform lookGizmo;
    
//-----------------//Variables//-----------------//
//Process variables - private
    private Vector3 lookDirection;
    

//Balance variables - serialized 
    [Range(0, 360)]public float viewAngle;

    public float viewRange;
    public LayerMask targetmask;
    public LayerMask obstacleMask;

    public List<Transform> visibleTargets = new List<Transform>();
    public float meshResolution;
    
//-----------------//Functions//-----------------//
//Built-in
private void Start()
{
    StartCoroutine(nameof(FindTargetsCycle), 0.2f);
    viewMesh = new Mesh();
    viewMesh.name = "View Mesh";
    viewMeshFilter.mesh = viewMesh;
}

private void Update()
{
    lookDirection = transform.TransformDirection(transform.forward);
}

private void LateUpdate()
{
    DrawFieldOfView();
}

private void OnDrawGizmos()
{
    Gizmos.color = Color.white;
    Gizmos.DrawLine(transform.position, lookGizmo.position);
    
    Gizmos.color = Color.red;
    //Gizmos.DrawSphere(transform.position, viewRange);
}


//Inner process - private
    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 direction = DirFromAngle(globalAngle, true);
        RaycastHit2D hit;
        hit = Physics2D.Raycast((Vector2)transform.position, (Vector2)direction, viewRange, obstacleMask);
        if (hit.collider != null)
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, transform.position + direction * viewRange, viewRange, globalAngle);
        }
    }
    void DrawFieldOfView()
    {
        int stepcount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepcount;
        List<Vector2> viewPoints = new List<Vector2>();
        
        for (int i = 0; i <= stepcount; i++)
        {
            float angle = - transform.eulerAngles.z - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);
            viewPoints.Add(newViewCast.point);
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector2.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] =  transform.InverseTransformPoint(viewPoints[i]);
            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
           
        }
        
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }
    
    private IEnumerator FindTargetsCycle(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, viewRange,targetmask);
        foreach (var target in cols)
        {
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            if (Vector3.Angle(transform.up, dirToTarget) < viewAngle / 2)
            {
                float distToTarget = Vector3.Distance(transform.position, target.transform.position);
                if (!Physics2D.Raycast(transform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    visibleTargets.Add(target.transform);
                }
            }
        }
    }

//External interaction - public
    public Vector3 DirFromAngle(float angleDegrees, bool isGlobal)
    {
        if (!isGlobal)
        {
            angleDegrees -= transform.eulerAngles.z;
        }
        return new Vector3(Mathf.Sin(angleDegrees * Mathf.Deg2Rad), Mathf.Cos(angleDegrees * Mathf.Deg2Rad), 0);
    }
}
