using System;
using System.Collections.Generic;
using UnityEngine;
 
public class Test : MonoBehaviour
{
    [Header("Restricted to DogBase and IApe")]   
    [SerializeReference]    
    [SerializeReferenceButton]
    [SerializeReferenceUIRestrictionIncludeTypes(typeof(DogBase), typeof(IApe))]
    public IAnimal restrictedAnimals = default;  
    
    [Header("Interface")]   
    [SerializeReference]    
    [SerializeReferenceButton]
    public IAnimal animalInterface = default;  
  
    [Header("Abstract")]    
    [SerializeReference]  
    [SerializeReferenceButton]
    public AbstractAnimal animalAbstract = default;  
    
    [Header("Base Class")] 
    [SerializeReference] 
    [SerializeReferenceButton]
    public AnimalBase animalBaseClass = default;
 
    
    [Header("List of interfaces")]
    [SerializeReference]
    [SerializeReferenceButton] 
    public List<IAnimal> animalsWithInterfaces = new List<IAnimal>();
    
    
    [Header("List of Animals via MMB menu")] 
    [SerializeReference] 
    [SerializeReferenceMenu]
    public List<AnimalBase> animalsWithInterfacesMenu = new List<AnimalBase>();

    
    [Header("Class with serialized reference field")] 
    public List<ClassWithSerializedReferenceChild> classWithChildReferences = default;
}
  
 
[Serializable]
public class ClassWithSerializedReferenceChild
{
    public int integerValue = default;
    
    [SerializeReference] 
    [SerializeReferenceButton]
    public AnimalBase animals = default; 
    
    [SerializeReference] 
    [SerializeReferenceButton]
    public IAnimal animalInterfaces = default;
}