using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public List<RenderTexture> textures;
    public GameObject phone;
    int index = 0;
    public void Swap()
    {
        index++;
        index %= textures.Count;
        Renderer rend = phone.GetComponent<Renderer>();
        rend.materials[1].SetTexture("_EmissionMap", textures[index]);
    }

}
