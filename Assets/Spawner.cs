using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    //这是BoidSpawner的单例模式，只允许存在BoidSpawner的一个实例
    //所以我们把它存放在静态变量S中

    static public Spawner S;
    static public List<Boid> boids;

    //以下字段可以调整全体boid对象的生产行为
    [Header("Set in Inspector: Spawning")]
    public GameObject boidPrefab;
    public Transform boidAnchor;
    public int numBoids = 30;
    public float spawnRadius = 2f;
    public float spawnDelay = 0.1f;

    //以下字段可以调整全体boid对象的行为
    [Header("Set in Inspector: Boids")]
    public float velocity = 5f;
    public float neighborDist = 2f;
    public float collDist = 1f;
    public float velMatching = 0.25f;
    public float flockCentering = 0.2f;
    public float collAvoid = 1f;
    public float attractPull = 1f;
    public float attractPush = 1f;
    public float attractPushDist = 1f;

    // 新增：敲击斥散强度
    public float clickRepulsionStrength = 100f; // 默认值，可在 Inspector 中调整
    public float clickRepulsionDuration = 0.15f; // 默认值，可在 Inspector 中调整，效果持续时间

    // 新增：敲击聚集强度和持续时间
    public float clickGatherPullStrength = 15f; // 默认值，瞬间聚集的拉力强度
    public float clickGatherDuration = 0.1f;    // 默认值，聚集效果持续时间

    // 新增：点击特效期间 Boids 的速度
    public float clickEffectVelocity = 13f; // 默认比普通速度快一倍，可以调整
    void Awake(){

        //设置单例变量s为boidspawner的当前实例
        S = this;
        //初始化Boids
        boids = new List<Boid>();
        InstantiateBoid();
    }

    public void InstantiateBoid(){
        GameObject go = Instantiate(boidPrefab);
        Boid b = go.GetComponent<Boid>();
        b.transform.SetParent(boidAnchor);
        boids.Add(b);
        if (boids.Count < numBoids){
            Invoke("InstantiateBoid", spawnDelay);
        }
    }

}
