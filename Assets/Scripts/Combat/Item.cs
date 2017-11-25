﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {

    [Header("Item:")]
    public Character CurrentOwner;

    [Header("Item Skills:")]
    public ItemSkill[] ItemSkills = new ItemSkill[1];

    [Header("Item Hit Box:")]
    public bool IgnoreCurrentOwnerForCollisionChecks = true;
    public List<Character> CurrentlyCollidingCharacters = new List<Character>();

    public bool CanHitCharactersOnlyOnce = false;
    public SkillType SkillCurrentlyUsingItemHitBox;
    public ItemSkill ItemSkillCurrentlyUsingItemHitBox;
    public List<Character> AlreadyHitCharacters = new List<Character>();
    public Character.TeamAlignment CurrentItemHitBoxAlignment = Character.TeamAlignment.NONE;

    public virtual void EquipItem(Character CharacterToEquipTo, int SlotID)
    {

    }

    public virtual void UnEquipItem()
    {

    }

    public ItemSkill[] GetItemSkills()
    {
        return ItemSkills;
    }

    public List<Character> GetAllCurrentlyCollidingCharacters()
    {
        return CurrentlyCollidingCharacters;
    }

    public void StartSkillCurrentlyUsingItemHitBox(ItemSkill SourceItemSkill, SkillType SourceSkill, bool HitEachCharacterOnce)
    {
        if (SkillCurrentlyUsingItemHitBox)
        {
            Debug.Log("WARNING: " + SourceSkill + " overwrites existing " + SkillCurrentlyUsingItemHitBox + " Skill for use of Item Hit Box!");
        }

        CanHitCharactersOnlyOnce = HitEachCharacterOnce;
        SkillCurrentlyUsingItemHitBox = SourceSkill;
        ItemSkillCurrentlyUsingItemHitBox = SourceItemSkill;
        AlreadyHitCharacters = new List<Character>();

        // Calculate which Team(s) the Item can hit:
        int counter = 0;

        if (SourceSkill.GetAllowTargetFriendly())
        {
            counter += (int)(CurrentOwner.GetAlignment());
        }

        if (SourceSkill.GetAllowTargetEnemy())
        {
            counter += ((int)(CurrentOwner.GetAlignment()) % 2) + 1;
        }

        CurrentItemHitBoxAlignment = (Character.TeamAlignment)(counter);

        // Try to hit all Characters already in the hit box:

        for (int i = 0; i < CurrentlyCollidingCharacters.Count; i++)
        {
            if (CheckIfEnterCharacterLegit(CurrentlyCollidingCharacters[i]))
            {
                ApplyCurrentSkillEffectsToCharacter(CurrentlyCollidingCharacters[i]);
            }
        }
    }

    public void EndSkillCurrentlyUsingItemHitBox()
    {
        SkillCurrentlyUsingItemHitBox = null;
        ItemSkillCurrentlyUsingItemHitBox = null;
        AlreadyHitCharacters.Clear();
    }

    public bool CheckIfSkillIsUsingHitBox(ItemSkill SkillToCheck)
    {
        if (ItemSkillCurrentlyUsingItemHitBox && ItemSkillCurrentlyUsingItemHitBox == SkillToCheck)
        {
            return true;
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(this + " COLLIDED WITH : " + other.gameObject);
        if (other.gameObject.tag == "Character")
        {
            Character OtherCharacter = other.gameObject.GetComponent<Character>();

            if (!CheckIfEnterCharacterLegit(OtherCharacter))
            {
                return;
            }

            CurrentlyCollidingCharacters.Add(OtherCharacter);

            if (SkillCurrentlyUsingItemHitBox)
            {
                ApplyCurrentSkillEffectsToCharacter(OtherCharacter);
            }
        }
    }

    private void ApplyCurrentSkillEffectsToCharacter(Character HitCharacter)
    {
        if (CurrentItemHitBoxAlignment == Character.TeamAlignment.ALL
                    || CurrentItemHitBoxAlignment == HitCharacter.GetAlignment())
        {
            SkillCurrentlyUsingItemHitBox.ApplyEffects(CurrentOwner, ItemSkillCurrentlyUsingItemHitBox, HitCharacter);
        }
    }

    private bool CheckIfEnterCharacterLegit(Character CharacterToCheck)
    {
        if (IgnoreCurrentOwnerForCollisionChecks && CharacterToCheck == CurrentOwner)
        {
            return false;
        }

        if (CanHitCharactersOnlyOnce && AlreadyHitCharacters.Contains(CharacterToCheck))
        {
            return false;
        }

        // If this point is reached, the Character is legit:

        if (CanHitCharactersOnlyOnce)
        {
            AlreadyHitCharacters.Add(CharacterToCheck);
        }

        return true;
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log(this + " COLLIDES NO LONGER WITH : " + other.gameObject);
        if (other.gameObject.tag == "Character")
        {
            Character OtherCharacter = other.gameObject.GetComponent<Character>();

            CurrentlyCollidingCharacters.Remove(OtherCharacter);
        }
    }


    /*   private void OnTriggerEnter(Collider other)
       {
           Debug.Log(this + " COLLIDED WITH : " + other.gameObject);
           if (other.gameObject.tag == "Character")
           {
               Character OtherCharacter = other.gameObject.GetComponent<Character>();

               if (!CheckIfEnterCharacterLegit(OtherCharacter))
               {
                   return;
               }

               CurrentlyCollidingCharacters.Add(OtherCharacter);

               if (SkillCurrentlyUsingItemHitBox)
               {

               }
           }
       }

       private bool CheckIfEnterCharacterLegit(Character CharacterToCheck)
       {
           if (IgnoreCurrentOwnerForCollisionChecks && CharacterToCheck == CurrentOwner)
           {
               return false;
           }

           return true;
       }

       private void OnTriggerExit(Collider other)
       {
           Debug.Log(this + " COLLIDES NO LONGER WITH : " + other.gameObject);
           if (other.gameObject.tag == "Character")
           {
               Character OtherCharacter = other.gameObject.GetComponent<Character>();

               if (!CheckIfExitCharacterLegit(OtherCharacter))
               {
                   return;
               }

               CurrentlyCollidingCharacters.Remove(OtherCharacter);
           }
       }

       private bool CheckIfExitCharacterLegit(Character CharacterToCheck)
       {
           if (IgnoreCurrentOwnerForCollisionChecks && CharacterToCheck == CurrentOwner)
           {
               return false;
           }

           return true;
       }*/
}
