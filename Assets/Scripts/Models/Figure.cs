using System.Collections.Generic;

namespace Models
{
    public class Figure
    {
        private List<Connection> _connections;

        public Figure(List<Connection> connections)
        {
            _connections = connections;
        }

        public List<Connection> Connections => _connections;
    }
}