using UnityEngine;

public class PillarSpawner : MonoBehaviour
{
    [SerializeField] GameObject pillarPrefab;
    [SerializeField] Transform pillarParent;
    [SerializeField] Vector3 pillarScale = Vector3.one;
    [SerializeField] Transform goal;
    [SerializeField] int a = 10;
    [SerializeField] int b = 10;
    [SerializeField] float pillarSpacing = 1.8f;

    private void Awake()
    {
        float x, z;
        GameObject pillar;

        for(int i = -a; i < a; i++)
        {
            for (int j = -b; j < b; j++)
            {
                x = i * pillarSpacing * 2 + Random.Range(-pillarSpacing, pillarSpacing);
                z = j * pillarSpacing * 2 + Random.Range(-pillarSpacing, pillarSpacing);

                if(!((x < 3 && x > -3 ) && (z < 3 && z > -3))) // no pillars around start
                {
                    if(!((x < goal.position.x + 3 && x > goal.position.x - 3) && (z < goal.position.z + 3 && z > goal.position.z - 3))) // no pillars around finish
                    {
                        pillar = Instantiate(pillarPrefab, new Vector3(x, 0, z), Quaternion.identity);
                        pillar.transform.localScale = pillarScale;
                        pillar.transform.SetParent(pillarParent);
                    }
                }                    
            }
        }
    }
}
