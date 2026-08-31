using UnityEngine;

public class Bullet : MonoBehaviour
{
    public PlayerMove player;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Destroy(gameObject, 10);
    }

    private void OnDestroy()
    {
        player.collisionManager.UnregisterObject(GetComponent<Rigidbody>());
    }
}
