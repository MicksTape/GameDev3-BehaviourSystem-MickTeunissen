using System.Collections.Generic;
using UnityEngine;


public class Blackboard : MonoBehaviour
{
    [SerializeReference] public List<BaseScriptableObject> baseSharedVariables = new List<BaseScriptableObject>();

    private Dictionary<string, object> dictionary = new Dictionary<string, object>();

    public T GetVariable<T>(string name) where T : BaseScriptableObject
    {
        if (dictionary.ContainsKey(name))
        {
            return dictionary[name] as T;
        }
        return null;
    }

    public void AddVariable(string name, BaseScriptableObject variable)
    {
        dictionary.Add(name, variable);
    }

    [ContextMenu("Add FloatVariable")]
    public void AddFloatVariable()
    {
        baseSharedVariables.Add(new VariableFloat());
    }

    [ContextMenu("Add GameObjectVariable")]
    public void AddGameObjectVariable()
    {
        baseSharedVariables.Add(new VariableGameObject());
    }
}
