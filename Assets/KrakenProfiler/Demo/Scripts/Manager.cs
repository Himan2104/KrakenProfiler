using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{

    [Range(10, 100)] public float multiplier = 10f;
    [SerializeField] private GameObject Prefab;
    [SerializeField] [Range(0,20)] private float SpawnDelay = 1f;
    private float timer = 0;
    // Start is called before the first frame update
    void Start()
    {   
        
    }

    // Update is called once per frame
    void Update()
    {
        if (timer >= SpawnDelay)
        {
            Vector3 position = new Vector3(Random.Range(-20, 20), 20, Random.Range(-20, 20));
            Instantiate(Prefab, position, Quaternion.identity);
        }
        else timer += Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Meteor") Destroy(other.gameObject);
    }

    public void Exit()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Menu");
    }
}
