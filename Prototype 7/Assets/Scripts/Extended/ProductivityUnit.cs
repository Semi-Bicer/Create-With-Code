using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProductivityUnit : Unit
{
    public float ProductivityMultiplier = 2;
    
    private ResourcePile m_CurrentPile = null;

    public override void GoTo(Building target)
    {
        ResetProductivity();
        base.GoTo(target);
    }

    public override void GoTo(Vector3 position)
    {
        ResetProductivity();
        base.GoTo(position);
    }

    void ResetProductivity()
    {
        if (m_CurrentPile != null)
        {
            m_CurrentPile.ProductionSpeed /= ProductivityMultiplier;
            m_CurrentPile = null;
        }
    }

    protected override void BuildingInRange()
    {
        //if we don't have a current pile yet
        //As BuildingInRange is called each update we are in range of our target, we want to double production only once
        if (m_CurrentPile == null)
        {
            ResourcePile pile = m_Target as ResourcePile;
            //if target is not a resource pile the cast above will return null
            if (pile != null)
            {
                m_CurrentPile = pile;
                m_CurrentPile.ProductionSpeed *= ProductivityMultiplier;
            }
        }
    }
}
