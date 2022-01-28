***
<br/>

# Reliquia-The-Immortality-Stone-V2


<img src="https://media.githubusercontent.com/media/SandoKabe/LFS/main/1586176391968-Pourquoi-Reliquia-The-Immortality-Stone.png"  /> 

[*https://www.facebook.com/ProjetReliquia*](https://www.facebook.com/ProjetReliquia)


<br/>

## Contexte
     
- **Qui** The Project;
- **Objectif**  Réaliser un jeu d'aventure et d'énigme en 3D et faire découvrir le monde de l'archéologie;
- **Pour qui**  Le jeu s'adresse aux 12-25 ans, et comprend une vingtaine d'heure de jeu;
- **Quand**  1er trimestre 2022;  

<br/>

***

<br/>
<!--
<img src="https://media.githubusercontent.com/media/SandoKabe/LFS/main/1587046550854-Inventaire-Map.png"  /> 
-->

## La démarche

- Quelle était ma mission  

Développer le système d'*IA*;   
<img src="https://media.githubusercontent.com/media/SandoKabe/LFS/main/Reliquia3.png" width=620 />

Développer les interactions avec *l'environnement*;  
<img src="https://media.githubusercontent.com/media/SandoKabe/LFS/main/Reliquia4.png" width=620 />

<br/>

> <br/>
> Concevoir un code modulable qui puisse être ajouter à n'importe quelle scène.
> <br/>
> <br/>




- Mes responsabilités
- Comment allions-nous nous organisez pour répondre à cette misssion
- Quelles étaient nos contraintes, nos moyens de communication 
- En quoi cela participe à la réflexion sur les NUI

<br/>

***
<br/>
<br/>

## La solution

### Conception

#### Implémentation d'une state machine  
<img src="https://media.githubusercontent.com/media/SandoKabe/LFS/main/external-content.duckduckgo.com.jpg"  width="400"/>  

```C#
// Active l'état en cours en appelant la fonction Tick.
Type nextState = CurrentState?.Tick();
if (nextState != null && nextState != CurrentState?.GetType())
{
SwitchToNewState(nextState);
}
``` 
[*StateMachine.cs*](https://github.com/SandoKabe/LeProjetReliquia/blob/main/Assets/Script/Sandrine_Script/IA/StateMachine.cs)

```C#

private void SwitchToNewState(Type nextState)
{
CurrentState = _availableStates[nextState];
OnStateChanged?.Invoke(CurrentState);

} 
```  
[*StateMachine.cs*](https://github.com/SandoKabe/LeProjetReliquia/blob/main/Assets/Script/Sandrine_Script/IA/StateMachine.cs)  

#### L'ennemi
```C#

private void InitializeStateMachine()
{
var states = new Dictionary<Type, BaseState>()
{
{typeof(WanderState), new WanderState(enemy: this) },
{typeof(ReturnState), new ReturnState(enemy: this) },
{typeof(ChaseState), new ChaseState(enemy: this) },
{typeof(ChasePlayerState), new ChasePlayerState(enemy: this) },
{typeof(AttackState), new AttackState(enemy: this) }
};

GetComponent<StateMachine>().SetStates(states);
} 
```
[*Enemy.cs*](https://github.com/SandoKabe/LeProjetReliquia/blob/main/Assets/Script/Sandrine_Script/IA/Enemy/Enemy.cs)  

#### Les états
```C#
public BaseState(GameObject gameObject)
    {
        this.gameObject = gameObject;
        this.transform = gameObject.transform;
    }

    public abstract Type Tick();
```
[*BaseState.cs*](https://github.com/SandoKabe/LeProjetReliquia/blob/main/Assets/Script/Sandrine_Script/IA/Enemy/BaseState.cs)  
[*ChaseState.cs*](https://github.com/SandoKabe/LeProjetReliquia/blob/main/Assets/Script/Sandrine_Script/IA/Enemy/ChaseState.cs)  

#### La navigation des agents  
##### Le Nav Mesh

<img src="https://media.githubusercontent.com/media/SandoKabe/LFS/main/Reliquia2.png"  width="620"/>  

Déplacements des agents  

```C#
    private void FindRandomDestination()
    {
        Vector3 testPosition = (_enemyPosition + (transform.forward * 4f))
                + new Vector3(x: UnityEngine.Random.Range(-4.5f, 4.5f), y: 0f, z: UnityEngine.Random.Range(-4.5f, 4.5f));

        _destination = new Vector3(testPosition.x, y: 1f, testPosition.z);

        _direction = Vector3.Normalize(_destination - _enemyPosition);
        _direction = new Vector3(_direction.x, y: 0f, _direction.z);

    }
```
Recherche de la cible 
```C#

        for (var i = 0; i < nbRay; i++)
        {
            if (Physics.Raycast(rayOrigine, direction, out hit, GameSettings.AggroRadius))
            {
                var target = hit.transform;
                if (target != null && target == playerTarget) 
                {
                    return playerTarget.transform;
                }
            }
            direction = stepAngle * direction;
        }


```
#### Les interactions avec l'environement
Il y a 2 types d'interaction  
```C#

private void OnTriggerEnter(Collider other)
{
if (other.CompareTag("Item"))
{
physicaltemInventaire = other.gameObject.GetComponent<PhysicaltemInventaire>();
item = physicaltemInventaire.thisItem;

gameManager.AfficherMessageInteraction("");
}
if (other.CompareTag("Interactable")) //&& interactableItem == null
{

interactableObject = other.gameObject.GetComponent<Interactable>();
interactableObject.ApplyOutline(true);

```
[*William_Script.cs*](https://github.com/SandoKabe/LeProjetReliquia/blob/main/Assets/Script/Maxence_Script/William_Script.cs)


```C#

if (Physics.Raycast(transform.position, rayDirection, out hit, rayDistance))
{

var target = hit.transform;
if (target != null && target.CompareTag("ItemInteractable"))
{
// Set Inventaire
physicaltemInventaire = target.gameObject.GetComponent<PhysicaltemInventaire>();
item = physicaltemInventaire.thisItem;

// Add Contour Blanc
interactableItem = target.gameObject.GetComponent<Interactable>();
interactableItem.ApplyOutline(true); 

```
[*William_Script.cs*](https://github.com/SandoKabe/LeProjetReliquia/blob/main/Assets/Script/Maxence_Script/William_Script.cs)

Et pour interagir
```C#

if (Input.GetKeyDown(raccourciClavier.toucheClavier["Action"]) && interactableObject != null)
{
if (interactableObject.type == Interactable.eInteractableType.Brasier)
{
gameManager.AfficherMessageInteraction("Use Light");
return;
}
// Open Door and Chest
INTERACT_ACTIONS?.Invoke();
} 
```
[*William_Script.cs*](https://github.com/SandoKabe/LeProjetReliquia/blob/main/Assets/Script/Maxence_Script/William_Script.cs)

Action définie

```C#

public class InteractionChest : MonoBehaviour
{
public GameObject axisObject;
public Axis axis;
[Header("Pour l'axe Y uniquement")]
public rotationDirection direction;

private bool rotate;
private bool rotateDirection;

private Quaternion delta;
private Quaternion deltaInit;

private Interactable interactable;
private William_Script wilScript;

private void OnEnable()
{
William_Script.INTERACT_ACTIONS += OpenDoor;
}

private void OnDisable()
{
William_Script.INTERACT_ACTIONS -= OpenDoor;
}
```
[InteractionChest](https://github.com/SandoKabe/LeProjetReliquia/blob/main/Assets/Script/Sandrine_Script/Interactions/InteractionChest.cs)



### Démo

### Les bonnes pratiques


### Le workflow

<br/>
<br/>
<br/>

***
<br/>
<br/>

<img src="https://media.githubusercontent.com/media/SandoKabe/LFS/main/1600597636002-Bonus---Tresors.png"  width="620"/> 

## REX

> 
>
> - En quoi ça répond à la problématique;
> - Quelles ont été les difficultés rencontrées et les solutions;
> - Ce qui m'a plu et moins plu;
> - Ce que ça m'a apporté;
>
>  *Conclusion* 

<br/>
<br/>

Ce projet a été réalisé avec [Unity](https://unity.com/).  
Remerciements à 
[Paul](https://www.linkedin.com/in/paul-lamouret-89ba94174/),
[Alexis](https://www.linkedin.com/in/alexischapuisgd/), 
[Maxence](https://www.linkedin.com/in/maxence-colas/), 
et à toute l'équipe de [The Project](https://www.asso-theproject.com/index.html)  


***
