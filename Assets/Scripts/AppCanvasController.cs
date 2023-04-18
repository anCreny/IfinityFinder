using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class AppCanvasController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private GameManager _gameManager;
    
    private void Start()
    {
        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _gameManager.OnBeginLineDrawing();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _gameManager.OnEndLineDrawing();
    }
}
