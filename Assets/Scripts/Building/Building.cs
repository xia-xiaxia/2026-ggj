using UnityEngine;

public class Building : MonoBehaviour
{
    public Vector3Int size;
    public Material material;

    private void Start()
    {
        material = GetComponent<MeshRenderer>().material;
    }
}

