using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class Vision : MonoBehaviour
{
//-----------------//Data structures//-----------------//
//enums


//structs
    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
    
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
    [SerializeField] private Transform PositionParent;
    
//-----------------//Variables//-----------------//
    private Vector3 lookDirection;
    
    [Range(0, 360)]public float viewAngle;
    public float viewRange;
    
    [SerializeField] private int findEdgeIterations = 1;
    [SerializeField] private float edgeDistanceThreshold = 1;
    
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
        ViewCastInfo previousViewcast = new ViewCastInfo();
        
        for (int i = 0; i <= stepcount; i++)
        {
            float angle = - transform.eulerAngles.z - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0) {
                bool edgeDistanceExceeded = Mathf.Abs(previousViewcast.distance - newViewCast.distance) > edgeDistanceThreshold;
                if (previousViewcast.hit != newViewCast.hit || (previousViewcast.hit && newViewCast.hit && edgeDistanceExceeded)) {
                    EdgeInfo edge = FindEdge(previousViewcast, newViewCast);
                    if (edge.pointA != Vector3.zero) {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero) {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }
            
            viewPoints.Add(newViewCast.point);
            previousViewcast = newViewCast;
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

    private EdgeInfo FindEdge(ViewCastInfo minVC, ViewCastInfo maxVC) {
        float minAngle = minVC.angle;
        float maxAngle = maxVC.angle;
        Vector3 minPoint = minVC.point;
        Vector3 maxPoint = maxVC.point;

        for (int i = 0; i < findEdgeIterations; i++) {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewcast = ViewCast(angle);
            
            bool edgeDistanceExceeded = Mathf.Abs(minVC.distance - newViewcast.distance) > edgeDistanceThreshold;
            
            if (newViewcast.hit == minVC.hit && !edgeDistanceExceeded) {
                minAngle = angle;
                minPoint = newViewcast.point;
            } else {
                maxAngle = angle;
                maxPoint = newViewcast.point;
            }
        }
        return new EdgeInfo(minPoint, maxPoint);
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
                    target.GetComponent<SnarkBehavior>().Seen();
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
