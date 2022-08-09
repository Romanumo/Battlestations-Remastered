using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private List<EnemyWave> waveInfo;
    [SerializeField] private GameObject winScreen;
    PerkChoose perkManager;
    List<GameObject> roundEnemies;
    bool isAwaiting = false;
    int round = 0;
    bool isWon = false;

    void Start()
    {
        perkManager = GameObject.FindObjectOfType<PerkChoose>();
        roundEnemies = new List<GameObject>();
        SpawnNewEnemies();
    }

    void Update()
    {
        if(roundEnemies.Count == 0 && !isAwaiting)
        {
            isAwaiting = true;
            GeneralFunctions.AddTimer(delegate () { SpawnNewEnemies(); }, 2f); //was 20
            GeneralFunctions.KeyTipShow("Round is cleared!", 5f);
        }
    }

    public void UnsubscribeEnemy(GameObject obj)
    {
        if (roundEnemies.Contains(obj))
            roundEnemies.Remove(obj);
    }

    public void AddEnemy(GameObject enemy)
    {
        roundEnemies.Add(enemy);
    }

    void SpawnNewEnemies()
    {
        if(round > waveInfo.Count)
        {
            winScreen.SetActive(true);
            Time.timeScale = 1;
            round--;
            isWon = true;
            for(int i = 0;i<30;i++)
                GameObject.FindObjectOfType<PerkChoose>().AddPerkPoint();
        }

        isAwaiting = false;
        waveInfo[0].units[0].Spawn();

        StartCoroutine(SpawnWave(waveInfo[round]));

        if(!isWon)
            round++;
        perkManager.AddPerkPoint();
    }

    IEnumerator SpawnWave(EnemyWave wave)
    {
        foreach(EnemyWaveUnit unit in wave.units)
        {
            for (int i = 0; i < unit.amount; i++)
            {
                yield return new WaitForSeconds(0.1f);
                unit.Spawn();
            }
        }
        yield return null;
    }
}

[System.Serializable]
public class EnemyWave
{
    [SerializeField] private string name;
    public EnemyWaveUnit[] units;
}

[System.Serializable]
public class EnemyWaveUnit
{
    public GameObject enemy;
    public int amount;

    public void Spawn()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        Vector3 pos = new Vector3(Random.Range(0, 100), 7f, Random.Range(0, 100));
        GameObject obj = GameObject.Instantiate(enemy, pos, new Quaternion());
        obj.transform.parent = GlobalLibrary.roundManager.gameObject.transform;
        GlobalLibrary.roundManager.AddEnemy(obj);
    }
}