﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;

public class CompanionAttackState : BaseState
{
    public Companion _companion;
    private Vector3 _companionPosition;
    private Vector3 targetPosition;
    private Vector3 playerPosition;
    private Vector3 lastPosition;

    private float _attackReadyTimer = 1000f;
    private bool checkAttackTurn;




    private float _attackEndTimer = 0f;

    private bool flagEndAttack;
    private bool flagStartAttack;
    private bool playerTarget;

    private Vector3 _destination;
    private Vector3 _relativePos;
    private Vector3 _directionArrival;
    private Vector3 _spacePosition;
    private float _distanceRecorded = 0;

    private Quaternion startingAngle = Quaternion.AngleAxis(-135, Vector3.up);
    private Quaternion stepAngle = Quaternion.AngleAxis(5, Vector3.up);

    public CompanionAttackState(Companion companion) : base(companion.gameObject)
    {
        _companion = companion;
        

    }

    public override Type Tick()
    {
        //Debug.Log(this);
        if (_attackReadyTimer == 1000f)
        {
            _attackReadyTimer = _companion.AttackReadyTimer - 5f;
        }
        if (_companion.Target == null)
        {
            //Debug.Log("Go to WalkState, target is null");
            return typeof(WalkState);
        }
        _attackReadyTimer -= Time.deltaTime;

        if (_companion.AttackNumber <= 0 && _attackReadyTimer <= 0f)
        {
            //Debug.Log("Go to WaitState, AttackNumber <= 0");
            Enemy enemyCompanion = _companion.Target.GetComponent<Enemy>();
            if (enemyCompanion != null && enemyCompanion.Target == _companion.transform)
            {
                enemyCompanion.SetTarget(null);
            }
            if (enemyCompanion != null && enemyCompanion.Chaser == _companion.transform)
            {
                enemyCompanion.SetChaser(null);
            }
            _companion.SetTarget(null);
            return typeof(WaitState);
        }
        Vector3 test =  _companionPosition - targetPosition;


        _directionArrival = targetPosition - _companionPosition;
        
        _spacePosition = targetPosition - _companionPosition;
        _relativePos = Vector3.Normalize(targetPosition + _companionPosition);
        if (targetPosition.x < _companionPosition.x)
        {
            _relativePos = Vector3.Normalize(targetPosition - _companionPosition);
        }
        _destination = targetPosition;
        

        // Assignation des positions
        _companionPosition = _companion.transform.position;
        targetPosition = _companion.Target.transform.position;
        playerPosition = _companion.Player.position;
        //lastPosition = _companionPosition;

        _companion.SetSpeed(_companion.CompanionAttackSpeed);

        var distance = Vector3.Distance(_companionPosition, targetPosition);
        var distanceToPlayer = Vector3.Distance(_companionPosition, playerPosition);
        

        if (flagStartAttack && distance > 1.5f)
        {
            _companion.StopAttack();
            _companion.LookAtDirection(_relativePos, GameSettings.SpeedAttackWalking);
            _companion.Move(_destination, GameSettings.SpeedAttackWalking); //_companionPosition + 2 * spacePosition
        }

        //if (distance <= GameSettings.CompanionAttackRange && flagStartAttack == false)
        if (!flagStartAttack && _companion.NavAgent.remainingDistance < 1.5f)
        {
            //Debug.Log("Stop Moving");
            _companion.LookAtDirection(_directionArrival, GameSettings.SpeedAttackWalking);
            _companion.StopMoving();

        }

        checkAttackTurn = false;
        if (_attackReadyTimer <= 0f ) // _companion.NavAgent.remainingDistance <= 5f
        {
            
            CheckNewEnemy();
            checkAttackTurn = CheckForAttack();
            //Debug.Log("Name checkAttackTurn : " + _companion.Name + " " + checkAttackTurn);
        }


        if (checkAttackTurn && _attackReadyTimer <= 0f) 
        {
            _companion.StopMoving();
            _companion.flagAttack = true;
            //Debug.Log("start attack for : " + _companion.Name);
            lastPosition = _companionPosition;

            targetPosition = _companion.Target.position;
            _companionPosition = _companion.transform.position;
            _companion.LookAtDirection(_directionArrival, 10f);

            // Se déplacer jusqu'à l'ennemi :

            _companion.Move(_destination, GameSettings.SpeedAttackWalking); //_companionPosition + 2 * spacePosition

            flagStartAttack = true;
            _attackReadyTimer = _companion.AttackReadyTimer;



        }

        if (flagStartAttack && _companion.NavAgent.remainingDistance < 1.5f)
        {
            _companion.LookAtDirection(_directionArrival, 10f);
            if (_attackReadyTimer <= _companion.AttackReadyTimer - 5f || _distanceRecorded - distance > 0.5f)
            {
                //Debug.Log("stop attack for : " + _companion.Name);
                _companion.flagAttack = false;
                _companion.StopAttack();
                _distanceRecorded = 0;

                flagStartAttack = false;
                _companion.DecreaseAttackNumber();
            }
            else if (distance <= GameSettings.CompanionAttackRange)
            {
                //Debug.Log("LaunchAttack");
                // Set enemy targeted : déjà fait dans le prepare to attack
                _companion.LaunchAttack(10f);
                _distanceRecorded = distance;
                //_attackReadyTimer = _companion.AttackReadyTimer;
                //return null;
            }

        }

        // Si le joueur sort de la zone d'attaque
        // Retour à l'état Chase
        if (!_companion.IsAttacking() && distanceToPlayer > GameSettings.PlayerLeavingRange)
        {

            _companion.Target.GetComponent<Enemy>().ResetTargets();
            _companion.SetTarget(null);
            //Debug.Log("Go to walkstate, player is leaving");
            return typeof(WalkState);
        }

        // Suivre l'ennemi et continuer à attaquer
        if (distance >= 3f ) // To Replace GameSettings.FollowInAttackStateDistance) //2f
        {
            _companion.StopMoving();
            _companion.LookAtDirection(_relativePos, 10f);

            _companion.Move(_destination, GameSettings.SpeedAttackWalking); //3f

        }

        return null; 
    }

    private void CheckNewEnemy()
    {
        //Debug.Log("CheckNewEnemy");
        RaycastHit hit;
        var angle = transform.rotation * startingAngle;
        var direction = angle * Vector3.forward;
        Transform targetTemp = null;
        //Transform targetTempPlay = null;
        Vector3 raySource = _companionPosition + Vector3.up * 0.5f;
        playerTarget = false;

        for (var i = 0; i < 90; i++)
        {

            if (Physics.Raycast(raySource, direction, out hit, GameSettings.AggroRadius))
            {
                Transform target = hit.transform;
                Enemy enemyCast = target.GetComponent<Enemy>();

                if (target != null && null != enemyCast)
                {
                    Debug.DrawRay(raySource, direction * hit.distance, Color.red);
                    

                    if (targetTemp != null && targetTemp != enemyCast.transform)
                    {
                        // il y a 2 ennemis
                        //
                        _companion.Target.GetComponent<Enemy>().SetTarget(transform);
                        _companion.Target.GetComponent<Enemy>().SetChaser(transform);

                        //Debug.Log("CheckNewEnemy Yes : tragetTemp & enemy.transform : " + targetTemp + " " + enemyCast.transform);
                        return;
                    }
                    targetTemp = enemyCast.transform;

                }
            }

            direction = stepAngle * direction;

        }

    }

    private bool CheckForAttack()
    {
        return true;
        Enemy enemy = _companion.Target.GetComponent<Enemy>();
        Transform chaser = enemy.Chaser;
        


        if (chaser == null)
        {
            //enemy.SetChaser(transform);
            //enemy.SetTarget(transform);
            //Debug.Log("No Chase punching return true");
            return true;
        }

        // Est-ce que l'IA Compagnon peut attaquer le même ennemi que le joueur => true
        if (chaser.CompareTag("Player"))
        {
            //Animator playerAnim = chaser.GetComponent<Animator>();
            //if (playerAnim != null && !playerAnim.IsInTransition(0) 
            //    && playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Punching"))
            //{
            //    Debug.Log("Player punching return false");
            //    return false;
            //}
            //Debug.Log("Player not punching return true");
            return true;
        }
        // Si l'ennemi m'attaque, je n'attaque pas => non
        //if ( !enemy.Anim.IsInTransition(0) && enemy.Anim.GetCurrentAnimatorStateInfo(0).IsName("Puching")
        //    && enemy.Target == transform)
        //{
        //    Debug.Log("Enemy, my target is punching me return false");
        //    return false;
        //}

        Companion otherCompanion = chaser.GetComponent<Companion>();

        // Debug A suupprimer
        if (otherCompanion != null)
        {
            //Debug.Log("Enemy is targeted by : " + otherCompanion.Name);
        }

        if (otherCompanion != null && chaser == transform && !otherCompanion.AnimPlayer.IsInTransition(0)
            && otherCompanion.AnimPlayer.GetCurrentAnimatorStateInfo(0).IsName("Punching"))
        {
            //Debug.Log(otherCompanion.Name + ", me, punching return false");
            return false;
        }

        if (otherCompanion != null && chaser == transform)
        {
            //Debug.Log(otherCompanion.Name + " chase and not punching return true");
            //enemy.SetChaser(transform);
            enemy.SetTarget(transform);
            return true;
        }

        // impossible
        //if (otherCompanion != null && !otherCompanion.AnimPlayer.IsInTransition(0) 
        //    && otherCompanion.AnimPlayer.GetCurrentAnimatorStateInfo(0).IsName("Punching"))
        //{
        //    Debug.Log(otherCompanion.Name + ", not me,  punching return false");
        //    return false;
        //}
      
        

        //Debug.Log(otherCompanion.Name + " and me, not punching return true");

        //if (otherCompanion.flagAttack == true)
        //{
        //    return false;
        //}

        //enemy.SetChaser(transform);
        enemy.SetTarget(transform);
        return true;

    }
}
