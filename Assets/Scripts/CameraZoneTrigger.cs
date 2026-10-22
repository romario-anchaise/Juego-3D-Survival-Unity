using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider))]
public class CameraZoneTrigger : MonoBehaviour
{
    public CinemachineCamera zoneCam;
    public int activePriority=20;
    public int idlePriority=5;
    readonly HashSet<Collider> players=new HashSet<Collider>();
    void Reset() {GetComponent<Collider>().isTrigger=true;}
    void OnEnable() {if(zoneCam!=null)zoneCam.Priority=idlePriority;}
    void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player"))return;
        players.Add(other); if(zoneCam!=null)zoneCam.Priority=activePriority;
    }
    void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("Player"))return;
        players.Remove(other); if(players.Count==0 && zoneCam!=null)zoneCam.Priority=idlePriority;
    }
    void OnDisable() {players.Clear();if(zoneCam!=null)zoneCam.Priority=idlePriority;}
}
