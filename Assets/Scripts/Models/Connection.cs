using System;
using UnityEngine;

namespace Models
{
    public class Connection
    {
        private Vector2 _pointA;
        private Vector2 _pointB;

        public Connection(Vector2 pointA, Vector2 pointB)
        {
            _pointA = pointA;
            _pointB = pointB;
        }

        public Vector2 PointA => _pointA;
        public Vector2 PointB => _pointB;

        public bool CheckIntersection(Connection anotherConn, out Vector2 point)
        {
            point = new Vector2();
            
            var multiply1 = Vector3.Cross( PointB - PointA, anotherConn.PointA - PointA);
            var multiply2 = Vector3.Cross(PointB - PointA, anotherConn.PointB - PointA);

            if (multiply1.z == 0 || multiply2.z == 0 || Math.Sign(multiply1.z) == Math.Sign(multiply2.z))
            {
                return false;
            }

            multiply1 = Vector3.Cross(anotherConn.PointB - anotherConn.PointA, PointA - anotherConn.PointA);
            multiply2 = Vector3.Cross(anotherConn.PointB - anotherConn.PointA, PointB - anotherConn.PointA);
            
            if (multiply1.z == 0 || multiply2.z == 0 || Math.Sign(multiply1.z) == Math.Sign(multiply2.z))
            {
                return false;
            }

            point.x = PointA.x + (PointB.x - PointA.x) * Math.Abs(multiply1.z) / Math.Abs(multiply2.z - multiply1.z);
            point.y = PointA.y + (PointB.y - PointA.y) * Math.Abs(multiply1.z) / Math.Abs(multiply2.z - multiply1.z);
            
            return true;
        }
    }
}