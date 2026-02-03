using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// [0,1]
// [2,3]

public class QuadNode
{
    const int MaxGameObjectCount = 10;
    public QuadNode[] quads = null;
    public GameObject[] gameObjects = new GameObject[MaxGameObjectCount];
    public int count = 0;
    public QuadBounds quadBounds = new QuadBounds();

    public QuadNode(Vector2 _center, float _width, float _height)
    {
        quadBounds.center = _center;
        quadBounds.width = _width;
        quadBounds.height = _height;
    }

    public void Add(GameObject _gameObject)
    {
        if (count < MaxGameObjectCount && quads == null)
        {
            gameObjects[count] = _gameObject;
            count++;
            return;
        }

        if (quads == null)
        {
            quads = new QuadNode[4];

            float halfWidth = quadBounds.width / 2.0f;
            float halfHeight = quadBounds.height / 2.0f;
            float quarterWidth = halfWidth / 2.0f;
            float quarterHeight = halfHeight / 2.0f;

            quads[0] = new QuadNode(
                quadBounds.center + new Vector2(-quarterWidth, quarterHeight),
                halfWidth,
                halfHeight
            );

            quads[1] = new QuadNode(
                quadBounds.center + new Vector2(quarterWidth, quarterHeight),
                halfWidth,
                halfHeight
            );

            quads[2] = new QuadNode(
                quadBounds.center + new Vector2(-quarterWidth, -quarterHeight),
                halfWidth,
                halfHeight
            );

            quads[3] = new QuadNode(
                quadBounds.center + new Vector2(quarterWidth, -quarterHeight),
                halfWidth,
                halfHeight
            );


            for (int i = 0; i < count; i++)
            {
                Add(gameObjects[i]);
            }

            count = 0;
        }

        if (_gameObject.transform.position.x < quadBounds.center.x)
        {
            if (_gameObject.transform.position.y >= quadBounds.center.y)
                quads[0].Add(_gameObject); // Top-Left
            else
                quads[2].Add(_gameObject); // Bottom-Left
        }
        else
        {
            if (_gameObject.transform.position.y >= quadBounds.center.y)
                quads[1].Add(_gameObject); // Top-Right
            else
                quads[3].Add(_gameObject); // Bottom-Right
        }
    }

    public void Find(List<GameObject> _out, Vector2 _center, float _radius)
    {
        if (quads == null)
        {
            for(int i = 0; i < count; i++)
                if (Vector2.Distance(_center, (Vector2)gameObjects[i].transform.position) <= _radius)
                    _out.Add(gameObjects[i]);
            return;
        }

        if (OverLapQuad(quads[0].quadBounds, _center, _radius))
            quads[0].Find(_out, _center, _radius);
        if (OverLapQuad(quads[1].quadBounds, _center, _radius))
            quads[1].Find(_out, _center, _radius);
        if (OverLapQuad(quads[2].quadBounds, _center, _radius))
            quads[2].Find(_out, _center, _radius);
        if (OverLapQuad(quads[3].quadBounds, _center, _radius))
            quads[3].Find(_out, _center, _radius);
    }

    // needs second pass
    public bool OverLapQuad(QuadBounds quadBounds, Vector2 circleCenter, float radius)
    {
        float halfWidth = quadBounds.width / 2.0f;
        float halfHeight = quadBounds.height / 2.0f;

        float closestX = Mathf.Clamp(circleCenter.x, quadBounds.center.x - halfWidth, quadBounds.center.x + halfWidth);
        float closestY = Mathf.Clamp(circleCenter.y, quadBounds.center.y - halfHeight, quadBounds.center.y + halfHeight);

        float distanceX = circleCenter.x - closestX;
        float distanceY = circleCenter.y - closestY;

        float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);
        return distanceSquared <= (radius * radius);
    }

    public bool Remove(GameObject _gameObject)
    {
        if (quads == null)
        {
            int i = 0;
            bool found = false;
            for(; i < count; i++)
            {
                if (gameObjects[i] == _gameObject)
                {
                    found = true;
                    i++;
                    break;
                }
            }
            for(; i < count;i++)
            {
                gameObjects[i-1] = gameObjects[i];
            }
            if (found)
            {
                count--;
            }

            return found;
        }

        if (_gameObject.transform.position.x <= quadBounds.center.x &&
            _gameObject.transform.position.y >= quadBounds.center.y)
            if (quads[0].Remove(_gameObject))
                return true;
        if (_gameObject.transform.position.x > quadBounds.center.x &&
            _gameObject.transform.position.y >= quadBounds.center.y)
            if (quads[1].Remove(_gameObject))
                return true;
        if (_gameObject.transform.position.x <= quadBounds.center.x &&
            _gameObject.transform.position.y < quadBounds.center.y)
            if (quads[2].Remove(_gameObject))
                return true;
        if (_gameObject.transform.position.x > quadBounds.center.x &&
            _gameObject.transform.position.y < quadBounds.center.y)
            if (quads[3].Remove(_gameObject))
                return true;
        
        return false;
    }
}

public class QuadBounds
{
    public Vector2 center = new Vector2();
    public float width = 0;
    public float height = 0;
}