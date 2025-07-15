using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Set Dynamically")]
    public Rigidbody rigid;

    private Neighborhood neighborhood;

    //用这个初始化
    void Awake(){
        neighborhood = GetComponent<Neighborhood>();
        rigid = GetComponent<Rigidbody>();

        //设置一个随机初始位置
        pos = Random.insideUnitSphere * Spawner.S.spawnRadius;

        //设置一个随机初始速度
        Vector3 vel = Random.onUnitSphere * Spawner.S.velocity;
        rigid.velocity = vel;

        LookAhead();

        //给Boid一个随机的颜色，并且保证不黯淡
        Color randColor = Color.black;
        while (randColor.r + randColor.g + randColor.b < 1.0f){
            randColor = new Color(Random.value, Random.value, Random.value);
        }
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        foreach(Renderer r in rends){
            r.material.color = randColor;
        }
        TrailRenderer tRend = GetComponent<TrailRenderer>();
        tRend.material.SetColor("_TintColor", randColor);
    }

    void LookAhead(){
        transform.LookAt(pos + rigid.velocity);
    }
    public Vector3 pos{
        get{return transform.position;}
        set{transform.position = value;}
    }

    //每次物理更新时调用FxiedUpdate

void FixedUpdate(){
    Vector3 vel = rigid.velocity;
    Spawner spn = Spawner.S;

    //避免碰撞——避免boid之间离得太近
    Vector3 velAvoid = Vector3.zero;
    Vector3 tooClosePos = neighborhood.avgClosePos;
    //如果返回Vector3.zero，不执行任何操作
    if(tooClosePos != Vector3.zero){
        velAvoid = pos - tooClosePos;
        velAvoid.Normalize();
        velAvoid *=spn.velocity;
    }

    //速度匹配——与周围邻居的速度保持一致
    Vector3 velAlign = neighborhood.avgVel;
    //只在velAlign不为Vector3.zero时起效
    if (velAlign != Vector3.zero){
        //我们很在意方向，所以规范化速度
        velAlign.Normalize();
        //然后设定我们想要的速度
        velAlign *=spn.velocity;
    }

    //中心聚集——朝本地邻居的中心移动
    Vector3 velCenter = neighborhood.avgPos;
    if (velCenter != Vector3.zero){
        velCenter -= transform.position;
        velCenter.Normalize();
        velCenter *= spn.velocity;
    }

    //向着Attractor移动
    Vector3 delta = Attractor.POS - pos;
    //检查是向着还是躲着attractor移动
    bool attracted = (delta.magnitude > spn.attractPushDist);
    Vector3 velAttract = delta.normalized * spn.velocity;

    //应用所有的速度
    float fdt = Time.fixedDeltaTime;

    if(attracted){
        vel = Vector3.Lerp(vel, velAttract, spn.attractPull*fdt);
    }else{
        vel = Vector3.Lerp(vel,-velAttract, spn.attractPush*fdt);
    }

    //设置vel为spawner单例中设置的速度
    vel = vel.normalized * spn.velocity;
    //最后把它赋予给刚体
    rigid.velocity = vel;
    //朝向新速度的方向
    LookAhead();

}
}
