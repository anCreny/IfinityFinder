using System.Collections.Generic;

namespace Models
{
    public class NearlyCircle
    {
        private List<Connection> _connections;
        private float _verticalCentre;

        public NearlyCircle(Figure figure)
        {
            _connections = figure.Connections;
            CalculateVerticalCentre();
        }

        public List<Connection> Connections => _connections;
        public float VerticalCentre => _verticalCentre;

        private void CalculateVerticalCentre()
        {
            var highestPoint = float.MinValue;

            foreach (var connection in _connections)
            {
                if (connection.PointA.y > highestPoint)
                {
                    highestPoint = connection.PointA.y;
                }
            }

            var minPoint = float.MaxValue;

            foreach (var connection in _connections)
            {
                if (connection.PointA.y < minPoint)
                {
                    minPoint = connection.PointA.y;
                }
            }

            _verticalCentre = (highestPoint + minPoint) / 2;
        }
    }
}