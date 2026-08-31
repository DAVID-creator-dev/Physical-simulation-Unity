using System.Collections.Generic;
using UnityEngine;

public class QuilleManager : MonoBehaviour
{
    [Header("Assign the quilles (pins) here")]
    public List<GameObject> quilles;

    private List<Vector3> initialPositions = new List<Vector3>();
    private List<Quaternion> initialRotations = new List<Quaternion>();

    void Start()
    {
        foreach (GameObject quille in quilles)
        {
            initialPositions.Add(quille.transform.position);
            initialRotations.Add(quille.transform.rotation);
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.E))
            ResetAll();
    }

    public void ResetAll()
    {
        for (int i = 0; i < quilles.Count; i++)
        {
            quilles[i].transform.position = initialPositions[i];
            quilles[i].transform.rotation = initialRotations[i];

            Rigidbody rb = quilles[i].GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        Debug.Log("Toutes les quilles ont été réinitialisées !");
    }
}
