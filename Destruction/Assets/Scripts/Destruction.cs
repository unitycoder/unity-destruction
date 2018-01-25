﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destruction : MonoBehaviour {

    [Space(7)]
    [Header("State")]
    [Space(2)]
    [Tooltip("Whether the object is unbroken")]
    public bool together = true;
    [Tooltip("Whether the object starts broken")]
    public bool startBroken = false;

    [Space(7)]
    [Header("Breaking on Collision")]
    [Space(2)]
    [Tooltip("Whether the object breaks when it collides with something")]
    public bool breakOnCollision = true;
    [Tooltip("The minimum relative velocity to break the object")]
    public float velocityToBreak = 1; // Velocity required to break object

    [Space(7)]
    [Header("Breaking when nothing underneath")]
    [Space(2)]
    [Tooltip("Whether the object breaks when there's nothing underneath supporting it")]
    public bool breakOnNoSupports = false;
    [Tooltip("The length of the raycast used to check for supports underneath")]
    public float raycastLength = 1f;

    [Space(7)]
    [Header("Sound On Break")]
    [Space(2)]
    [Tooltip("Whether the object makes a sound when it breaks")]
    public bool soundOnBreak = false;
    [Tooltip("An array of sounds for the object to make when it breaks (A random one will be selected)")]
    public AudioClip[] clips;

    [Space(7)]
    [Header("Particles On Break")]
    [Space(2)]
    [Tooltip("Whether the object makes particles when it breaks")]
    public bool particlesOnBreak = false;


    public float breakageMultiplier = 0.3f;
    //Private vars
    private AudioSource src;
    private ParticleSystem psys;

    private Rigidbody rigidbody;
    private Collider coll;
    private Rigidbody[] rigids;

    void Start () {
        //Get the rigidbodies
        rigids = gameObject.GetComponentsInChildren<Rigidbody>();
        coll = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();

        together = !startBroken;
        SetPiecesKinematic(together);

        if (soundOnBreak)
            SetupSound();
        if (particlesOnBreak)
            SetupParticles();
    }
	
    void SetupSound() {
        //Get the audio source or create one
        src = GetComponent<AudioSource>();
        if (src == null)
            src = gameObject.AddComponent<AudioSource>();

        //Add a random audio clip to it
        src.clip = clips[Random.Range(0, clips.Length-1)];
    }

    void SetupParticles() {
        // Get the particle system or create one
        psys = GetComponent<ParticleSystem>();
        if (psys == null)
            psys = gameObject.AddComponent<ParticleSystem>();

        //This doesn't seem to do anything b/c the gameobject is not active
        psys.Stop();
    }

    void Update () {
        /* Broken object should follow unbroken one to prevent them from
         * being in the wrong place when they switch */
        //brokenObj.transform.position = togetherObj.transform.position;

        //Make sure the right object is active
        //togetherObj.SetActive(together);
        //brokenObj.SetActive(!together);
        if (!together)
            Break();

        if (breakOnNoSupports)
            CheckForSupports();
    }

    void CheckForSupports () {
        //Check downwards for supports
        if (!Physics.Raycast(transform.position, Vector3.down, raycastLength))
            Break();
    }

    void OnCollisionEnter(Collision collision) {
        if (!breakOnCollision)
            return;
        //Only break if relative velocity is high enough
        if (collision.relativeVelocity.magnitude > velocityToBreak)
            Break();
        else if (collision.relativeVelocity.magnitude / velocityToBreak > breakageMultiplier) {
            print (collision.relativeVelocity.magnitude / velocityToBreak);
            Collider[] pieces = Physics.OverlapSphere(collision.contacts[0].point, collision.relativeVelocity.magnitude / velocityToBreak * breakageMultiplier);
            print("Amount:" + pieces.Length);
        }
    }

    public void Break () {
        SetPiecesKinematic(false);

        //Play the sound
        if (soundOnBreak)
            src.Play();
        //Show particles
        if (particlesOnBreak)
            psys.Play();
    }

    void SetPiecesKinematic (bool valueIn) {
        foreach (Rigidbody rigid in rigids) {
            rigid.isKinematic = valueIn;
            rigid.GetComponent<Collider>().enabled = !valueIn;
        }
        coll.enabled = valueIn;
        rigidbody.isKinematic = !valueIn;
    }

    public void BreakWithExplosiveForce(float force, float radius = 3) {
        Break();

        //Add the explosive force to each rigidbody
        foreach (Rigidbody rigid in rigids)
            rigid.AddExplosionForce(force, transform.position, radius);
    }

}
