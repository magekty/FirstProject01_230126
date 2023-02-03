using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class TriggerScript : MonoBehaviour
{
    [SerializeField] GameObject garbagesGo = null;
    private ParticleSystem ps;
    private ParticleSystem.TriggerModule tm;
    private List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
    private List<ParticleSystem.Particle> exit = new List<ParticleSystem.Particle>();
    private Collider[] garbageColList = null;


    private void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();
        tm = ps.trigger;
        garbageColList = garbagesGo.GetComponentsInChildren<Collider>();
        foreach (Collider triggerCollider in garbageColList)
            tm.AddCollider(triggerCollider);
    }


    private void OnParticleTrigger()
    {

        int numEnter = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        int numExit = ps.GetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);

        for (int i = 0; i < numEnter; i++)
        {
            ParticleSystem.Particle p = enter[i];
            p.startColor = new Color32(255, 0, 0, 255);
            enter[i] = p;
        }

        for (int i = 0; i < numExit; i++)
        {
            ParticleSystem.Particle p = exit[i];
            p.startColor = new Color32(0, 255, 0, 255);
            exit[i] = p;
        }

        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        ps.SetTriggerParticles(ParticleSystemTriggerEventType.Exit, exit);
    }


}
