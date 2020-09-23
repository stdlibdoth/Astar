using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AStar
{
    [System.Serializable]
    public struct AStarLayer
    {
        public string layerID;
        [SerializeField]private sbyte m_order;

        public AStarLayer(string id, sbyte order)
        {
            layerID = id;
            m_order = order;
        }

        public static bool operator ==(AStarLayer a, AStarLayer b)
        {
            return a.m_order == b.m_order;
        }

        public static bool operator !=(AStarLayer a, AStarLayer b)
        {
            return a.m_order != b.m_order;
        }

        public override bool Equals(object obj)
        {
            return m_order.Equals((AStarLayer)obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
