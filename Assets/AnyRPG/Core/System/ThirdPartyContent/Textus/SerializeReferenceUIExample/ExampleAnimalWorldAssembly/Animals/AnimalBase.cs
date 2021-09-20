using System;
using UnityEngine;

[Serializable]
public class AnimalBase : IAnimal
{
    [SerializeField] protected float age;
    public GameObject food;
    public virtual void Feed()
    {
        Debug.Log("Thanks");
    } 
}
  
[Serializable]
public class AnimalChild : AnimalBase {} 

[Serializable]
public abstract class AnimalGrandChildAbstract : AnimalBase
{
    public string someString;
}

[Serializable]
public class AbstractAnimalGrandChild : AnimalGrandChildAbstract {}


[Serializable]
public abstract class AbstractAnimal : IAnimal
{
    [SerializeField] protected float age;
    public GameObject food;
    public virtual void Feed()
    {
        Debug.Log("Thanks");
    } 
}

[Serializable]
public class AbstractAnimalChild : AbstractAnimal {}


