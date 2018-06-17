﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "skill_effect_create_hit_objects", menuName = "Combat/SkillEffects/CreateHitObject", order = 40)]
public class SkillEffectCreateHitObject : SkillEffect {

    [Header("Skill Create Hit Object:")]
    public SkillHitObjectForEffect HitObjectPrefab;
    public bool UseSkillLevelAtActivationMoment = true;

    [Header("Threat: 0: Active, 1: Close Range, 2: Long Range")]
    public float[] Threat = new float[3]; // 0: Threat Active, 1: Threat Active Close Range, 2: Threat Long Range

    [Header("Targets: 1: Friendly  2: Enemy")]
    public bool[] AllowTarget = new bool[2];

    [Header("Skill Effects applied by Hit Object:")]
    public SkillEffect[] SkillEffects = new SkillEffect[0];

    [Header("Sound:")]
    public AudioClip SpawnSound;
    public bool LoopSound;

    public override void ApplyEffect(Character Owner, ItemSkill SourceItemSkill, Character Target)
    {
        CreateHitObject(Owner, SourceItemSkill);
    }

    public void CreateHitObject(Character Owner, ItemSkill SourceItemSkill)
    {
        // Spawn and Initialize Projectile:
        SkillHitObjectForEffect SpawnedHitObject = Instantiate(HitObjectPrefab, SourceItemSkill.transform.position, SourceItemSkill.GetCurrentOwner().transform.rotation);

        if (SpawnSound)
        {
            var audioSource = SpawnedHitObject.gameObject.GetComponent<AudioSource>();
            if (!audioSource)
                audioSource = SpawnedHitObject.gameObject.AddComponent<AudioSource>();
            audioSource.clip = SpawnSound;
            if (LoopSound)
            {
                audioSource.loop = LoopSound;
                SpawnedHitObject.FadeSound = true;
                SpawnedHitObject.StartCoroutine(FadeAudioIn(audioSource, .5f));
            }
            else
            {
                audioSource.Play();
            }
        }

        SpawnedHitObject.InitializeHitObject(SourceItemSkill.GetCurrentOwner(), SourceItemSkill, SkillEffects, AllowTarget, Threat, UseSkillLevelAtActivationMoment);
    }

    private IEnumerator FadeAudioIn(AudioSource source, float time)
    {
        source.volume = 0;
        source.Play();
        var step = 1 / time;
        while (source.volume < 1)
        {
            source.volume += step * Time.deltaTime;
            yield return null;
        }
        source.volume = 1;
    }
}