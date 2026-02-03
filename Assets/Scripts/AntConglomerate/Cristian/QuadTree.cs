using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuadTree
{
    private QuadNode head = null;
    public int count = 0;

    public QuadTree(Vector2 _center, float _width, float _height)
    {
        head = new QuadNode(_center, _width, _height);
    }
    
    public void Add(GameObject _gameObject)
    {
        head.Add(_gameObject);
        count++;
    }

    public int Count()
    {
        return count;
    }

    public List<GameObject> Find(Vector2 _center, float _radius)
    {
        List<GameObject> gameObjects = new List<GameObject>();

        head.Find(gameObjects, _center, _radius);

        return gameObjects;
    }

    public void Remove(GameObject _gameObject)
    {
        if (head.Remove(_gameObject))
            count--;
    }

    public void Clear()
    {
        if (head == null)
            return;
        
        Vector2 center = head.quadBounds.center;
        float width = head.quadBounds.width;
        float height = head.quadBounds.height;
        head = new QuadNode(center, width, height);
    }
}
