using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neighborhood : MonoBehaviour
{
    [Header("Set Dynamically")]
    public List<Boid> neighbors;
    private SphereCollider coll;
    // Start is called before the first frame update
    void Start()
    {
        neighbors = new List<Boid>();
        coll = GetComponent<SphereCollider>();
        coll.radius = Spawner.S.neighborDist/2;
    }

    void FixedUpdate(){
        if(coll.radius != Spawner.S.neighborDist/2){
            coll.radius = Spawner.S.neighborDist/2;
        }
    }

    void OnTriggerEnter(Collider other){
        Boid b = other.GetComponent<Boid>();
        if(b != null){
            if(neighbors.IndexOf(b) == -1){
                neighbors.Add(b);
            }
        }
    }

    void OnTriggerExit(Collider other){
        Boid b = other.GetComponent<Boid>();
        if(b != null){
            if(neighbors.IndexOf(b) != -1){
                neighbors.Remove(b);
            }
        }
    }

    public Vector3 avgPos{
        get{
            Vector3 avg = Vector3.zero;
            if (neighbors.Count > 0) return avg;
            for (int i = 0; i<neighbors.Count; i++){
                avg += neighbors[i].pos;
            }
            avg /= neighbors.Count;
            return avg;
        }
    }
    public Vector3 avgVel{
        get{
            Vector3 avg = Vector3.zero;
            if(neighbors.Count == 0) return avg;

            for (int i = 0; i<neighbors.Count; i++){
                avg += neighbors[i].rigid.velocity;
            }
            avg /= neighbors.Count;
            return avg;
        }
    }
    public Vector3 avgClosePos{
        get{
            Vector3 avg = Vector3.zero;
            Vector3 delta;
            int nearCount = 0;
            for(int i = 0; i<neighbors.Count; i++){
                delta = neighbors[i].pos - transform.position;
                if(delta.magnitude <= Spawner.S.collDist){
                    avg += neighbors[i].pos;
                    nearCount++;
                }
            }
            //如果附近什么都没有，返回Vector为0
            if (nearCount == 0) return avg;

            //否则，取他们的平均位置
            avg /= nearCount;
            return avg;
        }
    }
}
