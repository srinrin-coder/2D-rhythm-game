using UnityEngine;
using UnityEngine.Playables;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    Vector3 prePlayerPos;
    void Update()
    {
        if(player.transform.position != prePlayerPos)
        {
            transform.position = new Vector3(player.transform.position.x +7, 2, -10);
            prePlayerPos = player.transform.position;            
        }
    }
}
