using UnityEngine;

public class PillarSpawner : MonoBehaviour
{
    [SerializeField] GameObject cylinder;
    [SerializeField] int a = 10;
    [SerializeField] int b = 10;
    [SerializeField] float distance = 1.8f;

    private void Awake()
    {
        float x, z;

        for(int i = -a; i < a; i++)
        {
            for (int j = -b; j < b; j++)
            {
                x = i * distance * 2 + Random.Range(-distance, distance);
                z = j * distance * 2 + Random.Range(-distance, distance);
                if(!((x < 1 && x > -1 ) && (z < 1 && z > -1)))
                    Instantiate(cylinder, new Vector3(x, 0, z), Quaternion.identity);
            }
        }
    }
}
