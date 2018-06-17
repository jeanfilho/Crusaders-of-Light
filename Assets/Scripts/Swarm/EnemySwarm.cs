﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySwarm : MonoBehaviour {

    public enum SwarmType
    {
        STANDARD = 1,
        SPIKE = 2,
        RANGED = 3
    }

    [Header("Enemy Swarm:")]
    public NavMeshAgent NMAgent;
    public SwarmType SType = SwarmType.STANDARD;

    [Header("Core Rules:")]
    // Note: Seperation, Alignment, Cohesion, and Danger Factors are currently pulled from EnemyTestSwarm for at runtime testing.
    // Thus, the Factors here for these 4 rules are currently unused, but should be used later!
    [Header("Seperation:")]
    public float SeperationDistance = 3;
    public float SeperationFactor = 1;
    private bool NoSeperationThisUpdate = false;

    [Header("Alignment:")]
    public float AlignmentDistance = 3;
    public float AlignmentFactor = 1;

    [Header("Cohesion:")]
    public float CohesionDistance = 3;
    public float CohesionFactor = 1;

    [Header("Advanced Rules:")]
    [Header("Danger Avoidance:")]
    public float DangerDistance = 3;
    public float DangerFactor = 1;

    [Header("Attraction:")]
    public float AttractionDistance = 4;
    public float AttractionFactor = 1;

    [Header("Go To Border:")]
    public bool BorderOn = false;
    //public float OutsideAcceleration = 1;
    public float BorderDistance = 3;
    public float BorderFactor = 1;

    [Header("Movement:")]
    public Vector3 Velocity = Vector3.zero;
    public Vector3 Acceleration = Vector3.forward;

    public float Friction = 0.1f;

    public float GoalFactor;

    [Header("Speed::")]
    public float DesiredBaseSpeed = 6;
    public float DesiredRunSpeed = 12;

    [Header("Optimization:")]
    public float UpdateTimer = 0.5f;
    public float UpdateCounter = 0;

    [Header("Lists:")]
    public SwarmAttention SAttention;
    public CharacterAttention CAttention;
    public List<EnemySwarm> EnemiesInRange = new List<EnemySwarm>();
    public List<GameObject> DangerInRange = new List<GameObject>();
    public List<Character> PlayersInRange = new List<Character>();

    [Header("FOR TESTING:")]
    public bool PlayerDanger = true;

    [Header("New Variables:")]
    public Transform SwarmlingTransform;
    public int NeighbourRadiusBase = 7;
    public float NeighbourRadiusCurrent = 7;
    public float NeighbourRadiusMin = 2f;
    public float NeighbourRadiusMax = 7f;
    public float NeighbourRadiusStep = 1;
    public Collider[] NeighbourColliders = new Collider[6];
    public int NeighbourCount = 0;
    public int NeighbourLayerMask = 0;

    public EnemySwarm CurrentSwarmling;

    public Vector3 CohesionVec = Vector3.zero;
    public Vector3 SeperationVec = Vector3.zero;
    public Vector3 AlignmentVec = Vector3.zero;

    public int CohesionNumber = 0;
    public int SeperationNumber = 0;
    public int AlignmentNumber = 0;

    public Vector3 DistanceVec = Vector3.zero;
    public float DistanceVecMag = 0;

    public float NewNeighbourTimer = 0f;
    public float NewNeighbourCounter = 1f;

    // ================================================================================================================

    public void SwarmlingRulesCalculation()
    {
        // NeighbourColliders = Physics.OverlapSphere(SwarmlingTransform.position, NeighbourRadius, NeighbourLayerMask);
        // Stop if not enough Neighbours:
        if (NeighbourCount < 2) return;

        CohesionVec = Vector3.zero;
        SeperationVec = Vector3.zero;
        AlignmentVec = Vector3.zero;

        CohesionNumber = 0;
        SeperationNumber = 0;
        AlignmentNumber = 0;

        DistanceVec = Vector3.zero;
        DistanceVecMag = 0;

        for (int i = 0; i < NeighbourCount; i++)
        {
            CurrentSwarmling = NeighbourColliders[i].GetComponent<EnemySwarm>();

            DistanceVec = SwarmlingTransform.position - CurrentSwarmling.SwarmlingTransform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= 0) continue;

            // Cohesion:
            if (DistanceVecMag <= Mathf.Pow(CohesionDistance, 2)) // Could be optimized by storing the pow2 distance!
            {
                CohesionVec += CurrentSwarmling.SwarmlingTransform.position;
                CohesionNumber++;
            }

            // Seperation:
            if (DistanceVecMag <= Mathf.Pow(SeperationDistance, 2)) // Could be optimized by storing the pow2 distance!
            {
                SeperationVec += DistanceVec / DistanceVecMag;
                
                SeperationNumber++;
            }

            // Alignment:
            if (DistanceVecMag <= Mathf.Pow(AlignmentDistance, 2)) // Could be optimized by storing the pow2 distance!
            {
                AlignmentVec += CurrentSwarmling.Velocity;
                AlignmentNumber++;
            }
        }
        // Cohesion:
        if (CohesionNumber > 0)
        {
            //CohesionVec = Vector3.ClampMagnitude(((CohesionVec / CohesionNumber) - SwarmlingTransform.position), DesiredBaseSpeed);
            //Debug.Log("Cohesion: " + CohesionVec);
            CohesionVec = CohesionVec / CohesionNumber;
            CohesionVec = CohesionVec - SwarmlingTransform.position;
            CohesionVec = CohesionVec.normalized * DesiredBaseSpeed;

            CohesionVec = Steer(CohesionVec);
            
            Acceleration += CohesionVec * CohesionFactor;
            GoalFactor += CohesionFactor;
        }

        // Seperation:
        if (SeperationNumber > 0)
        {
            if (NoSeperationThisUpdate)
            {
                NoSeperationThisUpdate = false;
            }
            else
            {
                //SeperationVec = Vector3.ClampMagnitude((SeperationVec / SeperationNumber), DesiredBaseSpeed);
                //Debug.Log("SeperationVec: " + SeperationVec);
                SeperationVec = SeperationVec.normalized * DesiredBaseSpeed;

                SeperationVec = Steer(SeperationVec);
                Acceleration += SeperationVec * SeperationFactor;
                
                GoalFactor += SeperationFactor;
            }    
        }

        // Alignment:
        if (AlignmentNumber > 0)
        {
            //AlignmentVec = Vector3.ClampMagnitude((AlignmentVec/AlignmentNumber), DesiredBaseSpeed);
            //Debug.Log("AlignmentVec: " + AlignmentVec);

            AlignmentVec = AlignmentVec.normalized * DesiredBaseSpeed;

            //AlignmentVec = AlignmentVec / AlignmentNumber;

            AlignmentVec = Steer(AlignmentVec);
            
            Acceleration += AlignmentVec * AlignmentFactor;
            GoalFactor += AlignmentFactor;
        }

    }

    // ================================================================================================================

    private void Start()
    {
        UpdateCounter = Random.Range(0, UpdateTimer);
        NewNeighbourCounter = Random.Range(0, NewNeighbourTimer);
        NeighbourLayerMask = 1 << NeighbourLayerMask;
    }

    public void SwarmlingUpdate()
    {
        GoalFactor = 0;

        // Get List of Neighbours:
        if (NewNeighbourCounter <= 0)
        {
            //NeighbourRadiusCurrent = NeighbourRadiusBase + NeighbourCount * -0.3f;

            if (NeighbourCount == NeighbourColliders.Length)
            {
                NeighbourRadiusCurrent = Mathf.Max(NeighbourRadiusCurrent - NeighbourRadiusStep, NeighbourRadiusMin);
            }
            else
            {
                NeighbourRadiusCurrent = Mathf.Min(NeighbourRadiusCurrent + NeighbourRadiusStep, NeighbourRadiusMax);
            }

            NeighbourCount = Physics.OverlapSphereNonAlloc(SwarmlingTransform.position, NeighbourRadiusCurrent, NeighbourColliders, NeighbourLayerMask);

            for (int i = NeighbourColliders.Length - 1; i > NeighbourCount - 1; i--)
            {
                NeighbourColliders[i] = null;
            }

            NewNeighbourCounter = Random.Range(0, NewNeighbourTimer) + Mathf.Pow(NeighbourCount * 0.3f, 2);
        }
        else
        {
            NewNeighbourCounter -= Time.deltaTime;
        }


        UpdateCounter += Time.deltaTime;

        if (UpdateCounter >= UpdateTimer)
        {
            UpdateCounter -= UpdateTimer;

            // Reset Acceleration:
            Acceleration = Vector3.zero;

            SwarmlingRulesCalculation();

            /*if (BorderOn)
            {
                RuleGoToBorder();
            }*/

            //RuleAttraction();
           // RuleCohesion();
           // RuleSeperation();
            //RuleAlignment();

            /*if (PlayerDanger)
            {
                RuleDangerAvoidanceEnhanced();
            }
            else
            {
                RuleDangerAvoidance();
            }*/
            
        }

        if (GoalFactor > 0)
        {
            Acceleration = Acceleration / GoalFactor;
        }
        
        // Update Velocity:
        Velocity += Acceleration * Time.deltaTime * 10;
        Velocity *= (1 - Friction * Time.deltaTime);

        // Move:
        NMAgent.Move(Velocity * Time.deltaTime);

        // Rotate towards Velocity Direction:
        if (Velocity.sqrMagnitude > 0)
        {
            transform.rotation = Quaternion.LookRotation(Velocity);
        }
    }

    public Vector3 Steer(Vector3 VelDesired)
    {
        return VelDesired - Velocity;
    }

   

    // ===================================================== RULES =====================================================

    // ================================================ RULE: COHESION ================================================
    // Enemies should steer to the center of all nearby enemies.

    private void RuleCohesion()
    {
        int NumberOfOthers = EnemiesInRange.Count;

        Vector3 CohesionVec = Vector3.zero;
        int CohesionNumber = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;

        for (int i = 0; i < NumberOfOthers; i++)
        {
            DistanceVec = transform.position - EnemiesInRange[i].transform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            // Cohesion:
            if (DistanceVecMag <= CohesionDistance * CohesionDistance)
            {
                CohesionVec += EnemiesInRange[i].transform.position;
                CohesionNumber++;
            }
        }
        // Cohesion:
        if (CohesionNumber >= 1)
        {
            CohesionVec = CohesionVec / CohesionNumber;
            CohesionVec = CohesionVec - this.transform.position;
            CohesionVec = CohesionVec.normalized * GetDesiredSpeed();
          
            CohesionVec = Steer(CohesionVec);
            Acceleration += CohesionVec * EnemyTestSwarm.Instance.CohesionFactor;
            GoalFactor += EnemyTestSwarm.Instance.CohesionFactor;
        }


    }

    // ===============================================/ RULE: COHESION /===============================================

    // =============================================== RULE: SEPERATION ===============================================
    // Enemies should steer away from other very close enemies.

    private void RuleSeperation()
    {
        if (NoSeperationThisUpdate)
        {
            NoSeperationThisUpdate = false;
            return;
        }

        int NumberOfOthers = EnemiesInRange.Count;

        Vector3 SeperationVec = Vector3.zero;
        int SeperationNumber = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;

        for (int i = 0; i < NumberOfOthers; i++)
        {
            DistanceVec = transform.position - EnemiesInRange[i].transform.position;
            DistanceVecMag = DistanceVec.magnitude;

            // Seperation:
            if (DistanceVecMag <= SeperationDistance)
            {
                SeperationVec += DistanceVec.normalized / DistanceVecMag;
                SeperationNumber++;
            }
        }

        // Seperation:
        if (SeperationNumber >= 1)
        {
            SeperationVec = SeperationVec.normalized * GetDesiredSpeed();

            SeperationVec = Steer(SeperationVec);
            Acceleration += SeperationVec * EnemyTestSwarm.Instance.SeperationFactor;
            GoalFactor += EnemyTestSwarm.Instance.SeperationFactor;
        }
    }

    // ==============================================/ RULE: SEPERATION /==============================================

    // ================================================ RULE: ALIGNMENT ================================================
    // Enemies should steer towards the average direction of nearby enemies.

    private void RuleAlignment()
    {
        int NumberOfOthers = EnemiesInRange.Count;

        Vector3 AlignmentVec = Vector3.zero;
        int AlignmentNumber = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;

        for (int i = 0; i < NumberOfOthers; i++)
        {
            DistanceVec = transform.position - EnemiesInRange[i].transform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            // Alignment:
            if (DistanceVecMag <= AlignmentDistance * AlignmentDistance)
            {
                AlignmentVec += EnemiesInRange[i].GetCurrenVelocity();
                AlignmentNumber++;
            }
        }

        // Alignment:
        if (AlignmentNumber >= 1)
        {
            AlignmentVec = AlignmentVec.normalized * GetDesiredSpeed();
            //AlignmentVec = AlignmentVec / AlignmentNumber;

            AlignmentVec = Steer(AlignmentVec);
            Acceleration += AlignmentVec * EnemyTestSwarm.Instance.AlignmentFactor;
            GoalFactor += EnemyTestSwarm.Instance.AlignmentFactor;
        }

    }

    // ===============================================/ RULE: ALIGNMENT /===============================================

    // ============================================ RULE: DANGER AVOIDANCE =============================================
    // Enemies should steer away from nearby dangers.

    private void RuleDangerAvoidance()
    {
        // Dangers in Range:
        int NumberOfDangers = DangerInRange.Count;

        Vector3 DangerVec = Vector3.zero;
        int DangerVecNumber = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;
        
        for (int i = 0; i < NumberOfDangers; i++)
        {
            DistanceVec = transform.position - DangerInRange[i].transform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= DangerDistance * DangerDistance)
            {
                DangerVec += DistanceVec.normalized / Mathf.Sqrt(DistanceVecMag);
                DangerVecNumber++;
            }
        }

        if (DangerVecNumber >= 1)
        {
            DangerVec = DangerVec.normalized * GetDesiredRunSpeed();

            DangerVec = Steer(DangerVec);
            Acceleration += DangerVec * EnemyTestSwarm.Instance.DangerFactor;
            GoalFactor += EnemyTestSwarm.Instance.DangerFactor;
        }
    }

    private void RuleDangerAvoidanceEnhanced()
    {
        // Dangers in Range:
        int NumberOfDangers = DangerInRange.Count;

        Vector3 DangerVec = Vector3.zero;
        int DangerVecNumber = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;

        for (int i = 0; i < NumberOfDangers; i++)
        {
            DistanceVec = transform.position - DangerInRange[i].transform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= DangerDistance * DangerDistance)
            {
                DangerVec += DistanceVec.normalized / Mathf.Sqrt(DistanceVecMag);
                DangerVecNumber++;
            }
        }

        // Player Hit Objects In Range:
        List<SkillHitObject> PlayerHitObjects = CAttention.GetPlayerHitObjectsInAttentionRange();
        NumberOfDangers = PlayerHitObjects.Count;

        for (int i = 0; i < NumberOfDangers; i++)
        {
            if (PlayerHitObjects[i])
            {
                DistanceVec = transform.position - PlayerHitObjects[i].transform.position;
                DistanceVecMag = DistanceVec.sqrMagnitude;

                if (DistanceVecMag <= DangerDistance * DangerDistance)
                {
                    DangerVec += DistanceVec.normalized / Mathf.Sqrt(DistanceVecMag);
                    DangerVecNumber++;
                }
            }
        }

        // Players in Range:
        NumberOfDangers = PlayersInRange.Count;

        for (int i = 0; i < NumberOfDangers; i++)
        {
            DistanceVec = transform.position - PlayersInRange[i].transform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= (DangerDistance * 0.6f) * (DangerDistance * 0.6f)
                && PlayersInRange[i].GetCurrentThreatLevel(true, false) >= 2
                && (Vector3.Dot(PlayersInRange[i].transform.forward, (PlayersInRange[i].transform.position - transform.position).normalized) < 0.3f))
            {
                DangerVec += DistanceVec.normalized / Mathf.Sqrt(DistanceVecMag);
                DangerVecNumber++;
            }
        }

        if (DangerVecNumber >= 1)
        {
            DangerVec = DangerVec.normalized * GetDesiredRunSpeed();

            DangerVec = Steer(DangerVec);
            Acceleration += DangerVec * EnemyTestSwarm.Instance.DangerFactor;
            GoalFactor += EnemyTestSwarm.Instance.DangerFactor;
        }
    }

    // ===========================================/ RULE: DANGER AVOIDANCE /============================================

    // =============================================== RULE: ATTRACTION ================================================
    // Enemies steer towards the nearest attraction (in this case Player Characters, so far).

    private void RuleAttraction()
    {
        int NumberOfOthers = PlayersInRange.Count;

        Vector3 AttractionVec = Vector3.zero;
        float AttractionVecMag = AttractionDistance * AttractionDistance + 1;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;
        
        for (int i = 0; i < NumberOfOthers; i++)
        {
            DistanceVec = PlayersInRange[i].transform.position - transform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= AttractionDistance * AttractionDistance
                && DistanceVecMag < AttractionVecMag)
            {
                AttractionVec = DistanceVec;
                AttractionVecMag = DistanceVecMag;
            }
        }

        if (AttractionVecMag < AttractionDistance * AttractionDistance + 1)
        {
            AttractionVec = AttractionVec.normalized * GetDesiredSpeed();

            AttractionVec = Steer(AttractionVec);
            Acceleration += AttractionVec * AttractionFactor;
            GoalFactor += AttractionFactor;
        }
    }

    // ==============================================/ RULE: ATTRACTION /===============================================

    // ============================================== RULE: GO TO BORDER ===============================================
    // Tank Enemies should steer towards the outside of the swarm.

    private void RuleGoToBorder()
    {
        int NumberOfOthers = EnemiesInRange.Count;

        Vector3 BorderVec = Vector3.zero;
        int BorderNumber = 0;

        Vector3 DistanceVec = Vector3.zero;
        float DistanceVecMag = 0;

        for (int i = 0; i < NumberOfOthers; i++)
        {
            DistanceVec = transform.position - EnemiesInRange[i].transform.position;
            DistanceVecMag = DistanceVec.sqrMagnitude;

            if (DistanceVecMag <= BorderDistance * BorderDistance && EnemiesInRange[i].GetSwarmType() != SType)
            {
                BorderVec += EnemiesInRange[i].transform.position;
                BorderNumber++;
            }
        }

        if (BorderNumber >= 2)
        {
            BorderVec = BorderVec / BorderNumber;
            BorderVec = this.transform.position - BorderVec;
           
            float NewFactor = Mathf.Max(BorderDistance - BorderVec.magnitude, 0);
            BorderVec = BorderVec.normalized * GetDesiredRunSpeed() * NewFactor;

            BorderVec = Steer(BorderVec);
            Acceleration += BorderVec * BorderFactor;
            GoalFactor += BorderFactor;

            NoSeperationThisUpdate = true;
        }
    }

    // =============================================/ RULE: GO TO BORDER /==============================================

    // ====================================================/ RULES /====================================================


    // ================================================== NEARBY LISTS ==================================================

    public void AddToEnemiesInRange(EnemySwarm AddEnemySwarm)
    {
        EnemiesInRange.Add(AddEnemySwarm);
    }

    public void RemoveFromEnemiesInRange(EnemySwarm RemoveEnemySwarm)
    {
        EnemiesInRange.Remove(RemoveEnemySwarm);
    }

    public void AddToDangersInRange(GameObject AddDanger)
    {
        DangerInRange.Add(AddDanger);
    }

    public void RemoveFromDangersInRange(GameObject RemoveDanger)
    {
        DangerInRange.Remove(RemoveDanger);
    }

    public void AddToPlayersInRange(GameObject AddPlayer)
    {
        PlayersInRange.Add(AddPlayer.GetComponent<CharacterAttention>().GetOwner());
    }

    public void RemoveFromPlayersInRanger(GameObject RemovePlayer)
    {
        PlayersInRange.Remove(RemovePlayer.GetComponent<CharacterAttention>().GetOwner());
    }

    public void OnDestroy()
    {
        for (int i = 0; i < EnemiesInRange.Count; i++)
        {
            EnemiesInRange[i].RemoveFromEnemiesInRange(this);
        }
    }

    // =================================================/ NEARBY LISTS /=================================================


    // ================================================ GETTERS/SETTERS =================================================

    public float GetDesiredSpeed()
    {
        return DesiredBaseSpeed;
    }

    public float GetDesiredRunSpeed()
    {
        return DesiredRunSpeed;
    }

    public Vector3 GetCurrenVelocity()
    {
        return Velocity;
    }

    public SwarmType GetSwarmType()
    {
        return SType;
    }

    // ===============================================/ GETTERS/SETTERS /================================================
}
