using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveMaker : MonoBehaviour
{

    class Line
    {
        public Vector3[] points = new Vector3[2];

        public Line(Vector3 p1, Vector3 p2)
        {
            points[0] = p1;
            points[1] = p2;
        }
    }



    List<Vector3> allPoints = new List<Vector3>();
    List<Line> allLines = new List<Line>();
    Vector3 point;
    List<Vector3> curvePoints = new List<Vector3>();
    List<GameObject> movePoints = new List<GameObject>();
    List<GameObject> lineRenders = new List<GameObject>();



    [Range(1, 100), Header("Curve Settings"), Tooltip("if the precision is smaller the drawing of the curve will be slower but more accurate (recommended: 20)")]
    public float precision = 20;

    public Color32 lineColor = Color.red;

    public Color32 startCurveColor = Color.white;
    public Color32 endCurveColor = new Color32(35, 178, 222, 255);
    



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    bool trigger;
    void Update()
    {
        Debug.DrawRay(MousePos() - Vector3.forward, Vector3.forward * 10, Color.red);
        MovePoints();
        if(Input.GetKeyDown(KeyCode.Mouse0) && !trigger)
        {
            if(allPoints.Count <= 3){
                createPoint(MousePos());
            }
            if(allPoints.Count > 1)
            {
                createLine(allPoints[allPoints.Count - 2], allPoints[allPoints.Count - 1]);
                ShowLine(allLines[allLines.Count - 1]);
                
            }
        }

        if(Input.GetKeyDown(KeyCode.KeypadEnter) || allPoints.Count >= 4){
            CreatePointsToDrag();
            trigger = true;
            point = Vector3.zero;
            //curvePoints = allPoints;
        }
        if(trigger){
            CreateCurve();
        }

    }

    Vector2 createPoint(Vector3 mousePos){
        allPoints.Add(mousePos);
        return new Vector2(mousePos.x, mousePos.y);
    }

    Line createLine(Vector3 p1, Vector3 p2){
        Line line = new Line(p1, p2);
        allLines.Add(line);
        return line;
    }

    void ShowLine(Line line){
        lineRenders.Add(new GameObject());
        lineRenders[lineRenders.Count - 1].transform.position = line.points[0];
        lineRenders[lineRenders.Count - 1].AddComponent<LineRenderer>();
        LineRenderer lineRenderer = lineRenders[lineRenders.Count - 1].GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, line.points[0]);
        lineRenderer.SetPosition(1, line.points[1]);
    }

    void UpdateLine(Line line, Vector3 p1, Vector3 p2){
        line.points[0] = p1;
        line.points[1] = p2;
    }

    bool stopDrawing;
    float colorTimer;
    GameObject curveObject;

    void ShowCurve(){
        if (colorTimer <= 1)
        {
            colorTimer += Time.deltaTime * precision;
            //timer = timer % 1;
        }


        curveObject = new GameObject();
        curveObject.transform.position = Vector3.zero;
        curveObject.AddComponent<LineRenderer>();
        LineRenderer lineRenderer = curveObject.GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
        lineRenderer.startColor = startCurveColor;
        lineRenderer.endColor = endCurveColor;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        for (int i = 0; i < curvePoints.Count; i++)
        {
            lineRenderer.positionCount = i + 1;
            lineRenderer.SetPosition(i, curvePoints[i]);
        }
        stopDrawing = true;
        // lineRenderer.SetPosition(0, lineRenderer.GetPosition(5));
        // lineRenderer.SetPosition(1, lineRenderer.GetPosition(5));
        // lineRenderer.SetPosition(2, lineRenderer.GetPosition(5));
        // lineRenderer.SetPosition(3, lineRenderer.GetPosition(5));
        
        //lineRenderer.SetPosition(curvePoints.Count, curvePoints[curvePoints.Count - 1]);



    }
    
    float timer;
    bool addLast;

    void CreateCurve(){
        if(timer <= 1){
            timer += Time.deltaTime * precision;
            //timer = timer % 1;
        }
        else if(timer >= 1 && !stopDrawing){
            ShowCurve();
        }
        if(stopDrawing == true)
            return;
        
        print(curvePoints.Count);
        //optimize this
        
        
        point = Mathf.Pow((1 - timer), 3) * allPoints[0] + 3 * Mathf.Pow((1 - timer), 2) * timer * allPoints[1] + 3 * (1 - timer) * Mathf.Pow(timer, 2) * allPoints[2] + Mathf.Pow(timer, 3) * allPoints[3];
        curvePoints.Add(point);
        
        // for (int i = 0; i < allPoints.Count; i++)
        // {
        //     print(i);
        //     curvePoints[i] = Vector3.Lerp(allPoints[allPoints.Count - (allPoints.Count + i)], allPoints[allPoints.Count - (allPoints.Count + i + 1)], timer);
        // }
    }

    void RecreateCurve(){
        stopDrawing = false;
        Destroy(curveObject);
        curvePoints.Clear();
        timer = 0;
        CreateCurve();
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(point, 0.1f);
    }
    bool createOnce;
    void CreatePointsToDrag(){
        if(createOnce){
            return;
        }
        foreach (Vector3 point in allPoints)
        {
            movePoints.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere));
            movePoints[movePoints.Count - 1].GetComponent<MeshRenderer>().material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));
            movePoints[movePoints.Count - 1].transform.position = point;
            movePoints[movePoints.Count - 1].AddComponent<SphereCollider>();
            movePoints[movePoints.Count - 1].GetComponent<SphereCollider>().radius = 0.1f;
            


        }
        createOnce = true;
    }

    void MovePoints(){
        if(movePoints.Count < 4) {
            return;

        }
        for (int i = 0; i < allPoints.Count; i++)
        {
            allPoints[i] = movePoints[i].transform.position;
        }
        if(Input.GetKey(KeyCode.Mouse0) && Physics.Raycast(MousePos() - Vector3.forward, Vector3.forward, out RaycastHit hit, 10)){
            hit.collider.gameObject.transform.position = MousePos();

            for (int i = 0; i < lineRenders.Count; i++)
            {
                lineRenders[i].GetComponent<LineRenderer>().SetPosition(0, allPoints[i]);
                lineRenders[i].GetComponent<LineRenderer>().SetPosition(1, allPoints[i + 1]);
                
            }
            RecreateCurve();

            
        }      
            
    }

    Vector3 MousePos(){
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }

}

