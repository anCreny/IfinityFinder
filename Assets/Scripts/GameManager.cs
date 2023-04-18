using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Models;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private List<LineRenderer> _draws = new();
    
    private List<GameObject> _visibleDraws = new();
    private List<Figure> _figures = new();
    private List<Figure> _isolatedFigures = new();
    private List<NearlyCircle> _convexFigures = new();
    
    [CanBeNull] private LineRenderer _currentDraw;
    
    [SerializeField] private Material _lineMaterial;
    [SerializeField] private TMP_Text _result;
    

    private bool _isDrawing;

    public void OnBeginLineDrawing()
    {
        var startPoint = GetMousePos();
        
        var emptyObject = new GameObject();
        _currentDraw = emptyObject.AddComponent<LineRenderer>();

        ConfigLine(startPoint, ref _currentDraw);
        
        emptyObject.transform.position = startPoint;

        _isDrawing = true;
    }

    private void OnLineDrawing()
    {
        var point = GetMousePos();
        var index = _currentDraw.positionCount;
        
        _currentDraw.SetVertexCount(index+1);
        _currentDraw.SetPosition(index, point);
    }

    public void OnEndLineDrawing()
    {
        var obj = Instantiate(_currentDraw.gameObject, _currentDraw.transform.position,
            _currentDraw.transform.rotation);
        var line = obj.GetComponent<LineRenderer>();
        
        line.enabled = false;
        line.Simplify(0.1f);
        
        ConvertPointsToConnections(line);
        
        _draws.Add(line);
        _visibleDraws.Add(_currentDraw.gameObject);
        
        _isDrawing = false;
    }

    private void Update()
    {
        var mouseStatus = Input.GetMouseButton(0);

        if (mouseStatus)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit))
            {
                Debug.Log(hit.transform.name);
            }

            if (_isDrawing)
            {
                OnLineDrawing();
            }
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            Clear();
        }
        
        if (Input.GetKeyUp(KeyCode.C))
        {
            Check();
        }
    }

    private Vector3 GetMousePos()
    {
        var mousePosition = Input.mousePosition;

        var position = Camera.main.ScreenToWorldPoint(mousePosition);
        position.z = 0;
        
        return position;
    }

    private void ConfigLine(Vector3 startPoint, ref LineRenderer line)
    {
        line.SetVertexCount(1);
        line.SetPosition(0, startPoint);
        
        line.startColor = Color.black;
        line.endColor = Color.black;
        line.material = _lineMaterial;
        
        line.SetWidth(0.1f, 0.1f);
    }

    private void ConvertPointsToConnections(LineRenderer draw)
    {
        List<Connection> connections = new();

        Vector3[] positions = new Vector3[draw.positionCount];
        
        var count = draw.GetPositions(positions);

        for (var i = 0; i < count - 1; i++)
        {
            var connection = new Connection(positions[i], positions[i+1]);
            connections.Add(connection);
        }

        var figure = new Figure(connections);
        _figures.Add(figure);
        
        CheckIsolations(figure);
    }

    private void CheckIsolations(Figure figure)
    {
        List<Connection> toCheck = new();
        foreach (var connection in figure.Connections)
        {
            for (var i = 0; i < toCheck.Count - 1; i++)
            {
                if (connection.CheckIntersection(toCheck[i], out var point))
                {
                    var newFigureConnections = new List<Connection>();

                    var firstConnection = new Connection(point, toCheck[i].PointB);
                    newFigureConnections.Add(firstConnection);

                    for (var j = 1; j < toCheck.Count - 1; j++)
                    {
                        newFigureConnections.Add(toCheck[j]);
                    }

                    var lastConnection = new Connection(connection.PointA, point);
                    newFigureConnections.Add(lastConnection);

                    var newFigure = new Figure(newFigureConnections);
                    
                    _isolatedFigures.Add(newFigure);

                    var indexPreviousConn = figure.Connections.IndexOf(toCheck[i]);
                    var indexConn = figure.Connections.IndexOf(connection);
                    
                    var blazedFigureConnections = new List<Connection>();

                    for (var leftIndex = 0; leftIndex < indexPreviousConn; leftIndex++)
                    {
                        blazedFigureConnections.Add(figure.Connections[leftIndex]);
                    }

                    var leftPatchConnection = new Connection(toCheck[i].PointA, point);
                    var rightPatchConnection = new Connection(point, connection.PointB);
                    
                    blazedFigureConnections.Add(leftPatchConnection);
                    blazedFigureConnections.Add(rightPatchConnection);

                    for (var rightIndex = indexConn + 1; rightIndex < figure.Connections.Count; rightIndex++)
                    {
                        blazedFigureConnections.Add(figure.Connections[rightIndex]);
                    }

                    var blazedFigure = new Figure(blazedFigureConnections);

                    CheckIsolations(blazedFigure);
                    return;
                }
            }

            toCheck.Add(connection);
        }
    }

    private void CheckConvexity(Figure figure)
    {
        var firstConnection = figure.Connections[0];
        var secondConnection = figure.Connections[1];

        var multiply = Vector3.Cross(firstConnection.PointB - firstConnection.PointA,
            secondConnection.PointB - secondConnection.PointA);


        var sign = Math.Sign(multiply.z);

        for (var i = 1; i < figure.Connections.Count - 1; i++)
        {
            var currentConnection = figure.Connections[i];
            var nextConnection = figure.Connections[i + 1];

            var vectorsMultiply = Vector3.Cross(currentConnection.PointB - currentConnection.PointA,
                nextConnection.PointB - nextConnection.PointA);

            var localSign = Math.Sign(vectorsMultiply.z);

            if (localSign != sign)
            {
                return;
            }
            
            sign = localSign;
            
        }
        
        _convexFigures.Add(new NearlyCircle(figure));
    }

    private bool CheckRelations()
    {
        for (var i = 0; i < _convexFigures.Count; i++)
        {
            var currentFigure = _convexFigures[i];

            for (var j = i; j < _convexFigures.Count; j++)
            {
                var comparedFigure = _convexFigures[j];
                if (comparedFigure.VerticalCentre <= currentFigure.VerticalCentre + 1 &&
                    comparedFigure.VerticalCentre >= currentFigure.VerticalCentre - 1)
                {
                    if (FindOneSamePoint(currentFigure, comparedFigure))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool FindOneSamePoint(NearlyCircle figureA, NearlyCircle figureB)
    {
        var intersectionsCounter = 0;
        
        foreach (var connectionA in figureA.Connections)
        {
            foreach (var connectionB in figureB.Connections)
            {
                if (connectionA.PointA == connectionB.PointA)
                {
                    intersectionsCounter++;
                }

                if (connectionA.CheckIntersection(connectionB, out var point))
                {
                    intersectionsCounter++;
                }
            }
        }

        if (intersectionsCounter == 1)
        {
            return true;
        }

        return false;
    }

    public void Clear()
    {
        _result.text = String.Empty;

        var drawsCount = _draws.Count;
        for (var i = 0; i < drawsCount; i++)
        {
            Destroy(_draws[i]);
        }

        _draws = new();

        var visibleDrawsCount = _visibleDraws.Count;
        for (var i = 0; i < visibleDrawsCount; i++)
        {
            Destroy(_visibleDraws[i]);
        }

        _visibleDraws = new();

        _figures = new();
        _isolatedFigures = new();
        _convexFigures = new();
        _currentDraw = null;
    }

    public void Check()
    {
        foreach (var isolatedFigure in _isolatedFigures)
        {
            CheckConvexity(isolatedFigure);
        }

        var result = CheckRelations();

        if (result)
        {
            _result.color = Color.green;
            _result.text = "TRUE";
        }
        else
        {
            _result.color = Color.red;
            _result.text = "FALSE";
        }
        
    }
    
}
