using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class ResourceRequirement
{
    public ResourceType type;
    public int amount;
}

[Serializable]
public class ResourceRequirementSet
{
    public List<ResourceRequirement> requirements = new List<ResourceRequirement>();
}

[Serializable]
public class Order
{
    public int orderId;
    public List<ResourceRequirementSet> fulRequiredResources = new List<ResourceRequirementSet>();
    public int turnsToComplete;
    public Order(int id, Dictionary<ResourceType, int> resources, int turns)
    {
        orderId = id;
        turnsToComplete = turns;
    }

    // 检查当前资源是否满足订单需求
    public bool IsFulfilled(Dictionary<ResourceType, int> currentResources)
    {
        foreach (var resSet in fulRequiredResources)
        {
            bool allResourcesMet = true;
            if (resSet != null && resSet.requirements != null)
            {
                foreach (var req in resSet.requirements)
                {
                    if (!currentResources.ContainsKey(req.type) || currentResources[req.type] < req.amount)
                    {
                        allResourcesMet = false;
                        break;
                    }
                }
            }
            if (allResourcesMet)
            {
                return true;
            }
        }
        return false;
    }
}