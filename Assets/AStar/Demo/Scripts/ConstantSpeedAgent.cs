using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AStar;

namespace AStar
{
    public class ConstantSpeedAgent : MoveAgent
    {

        [Header("Constant speed agent")]
        public float constSpeed;

        [SerializeField] private TextMesh m_nameTag = null;


        public void ActiveNameTag(bool active)
        {
            m_nameTag.text = gameObject.name;
            m_nameTag.gameObject.SetActive(active);
        }

        protected override void Move(List<AStarTile> way_points)
        {
            m_moveTrans.LookAt(way_points[1].transform.position);
            m_moveTrans.position = Vector3.MoveTowards(m_moveTrans.position, way_points[1].transform.position, constSpeed * Time.deltaTime);
        }
    }
}