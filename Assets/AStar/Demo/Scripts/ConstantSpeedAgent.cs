using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AStar;


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

    protected override void Move()
    {
        m_moveTrans.LookAt(NextTile.transform.position);
        m_moveTrans.position = Vector3.MoveTowards(m_moveTrans.position, NextTile.transform.position, constSpeed * Time.deltaTime);
    }
}
