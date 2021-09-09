using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class SystemEventManager {

        private static Dictionary<string, Action<string, EventParamProperties>> singleEventDictionary = new Dictionary<string, Action<string, EventParamProperties>>();


        public event System.Action<BaseAbility> OnAbilityUsed = delegate { };
        public event System.Action<BaseAbility> OnAbilityListChanged = delegate { };
        public event System.Action<Skill> OnSkillListChanged = delegate { };
        public event System.Action<int> OnLevelChanged = delegate { };
        public event System.Action<CharacterClass, CharacterClass> OnClassChange = delegate { };

        public event System.Action<string> OnInteractionStarted = delegate { };
        public event System.Action<InteractableOptionComponent> OnInteractionWithOptionStarted = delegate { };
        public event System.Action<Interactable> OnInteractionCompleted = delegate { };
        public event System.Action<InteractableOptionComponent> OnInteractionWithOptionCompleted = delegate { };
        public event System.Action<Item> OnItemCountChanged = delegate { };
        public event System.Action<Dialog> OnDialogCompleted = delegate { };
        public event System.Action<IAbilityCaster, CharacterUnit, int, string> OnTakeDamage = delegate { };

        // equipment manager
        public System.Action<Equipment, Equipment> OnEquipmentChanged = delegate { };

        public static void StartListening(string eventName, Action<string, EventParamProperties> listener) {
            Action<string, EventParamProperties> thisEvent;
            if (singleEventDictionary.TryGetValue(eventName, out thisEvent)) {

                //Add more event to the existing one
                thisEvent += listener;

                //Update the Dictionary
                singleEventDictionary[eventName] = thisEvent;
            } else {
                //Add event to the Dictionary for the first time
                thisEvent += listener;

                singleEventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StopListening(string eventName, Action<string, EventParamProperties> listener) {

            Action<string, EventParamProperties> thisEvent;
            if (singleEventDictionary.TryGetValue(eventName, out thisEvent)) {

                //Remove event from the existing one
                thisEvent -= listener;

                //Update the Dictionary
                singleEventDictionary[eventName] = thisEvent;
            }
        }

        public static void TriggerEvent(string eventName, EventParamProperties eventParam) {
            Action<string, EventParamProperties> thisEvent = null;
            if (singleEventDictionary.TryGetValue(eventName, out thisEvent)) {
                if (thisEvent != null) {
                    thisEvent.Invoke(eventName, eventParam);
                }
                // OR USE  instance.eventDictionary[eventName](eventParam);
            }
        }

        public void NotifyOnEquipmentChanged(Equipment newEquipment, Equipment oldEquipment) {
            OnEquipmentChanged(newEquipment, oldEquipment);
        }

        public void NotifyOnClassChange(CharacterClass newCharacterClass, CharacterClass oldCharacterClass) {
            OnClassChange(newCharacterClass, oldCharacterClass);
        }

        public void NotifyOnTakeDamage(IAbilityCaster source, CharacterUnit target, int damage, string abilityName) {
            OnTakeDamage(source, target, damage, abilityName);
        }

        public void NotifyOnDialogCompleted(Dialog dialog) {
            OnDialogCompleted(dialog);
            //OnPrerequisiteUpdated();

        }

        public void NotifyOnInteractionStarted(string interactableName) {
            //Debug.Log("SystemEventManager.NotifyOnInteractionStarted(" + interactableName + ")");
            OnInteractionStarted(interactableName);
        }

        public void NotifyOnInteractionWithOptionStarted(InteractableOptionComponent interactableOption) {
            //Debug.Log("SystemEventManager.NotifyOnInteractionWithOptionStarted(" + interactableOption.DisplayName + ")");
            OnInteractionWithOptionStarted(interactableOption);
        }

        public void NotifyOnInteractionCompleted(Interactable interactable) {
            OnInteractionCompleted(interactable);
        }

        public void NotifyOnInteractionWithOptionCompleted(InteractableOptionComponent interactableOption) {
            OnInteractionWithOptionCompleted(interactableOption);
        }

        public void NotifyOnLevelChanged(int newLevel) {
            OnLevelChanged(newLevel);
            //OnPrerequisiteUpdated();
        }

        public void NotifyOnAbilityListChanged(BaseAbility newAbility) {
            //Debug.Log("SystemEventManager.NotifyOnAbilityListChanged(" + abilityName + ")");
            OnAbilityListChanged(newAbility);
            //OnPrerequisiteUpdated();
        }
        

        public void NotifyOnAbilityUsed(BaseAbility ability) {
            //Debug.Log("SystemEventManager.NotifyAbilityused(" + ability.MyName + ")");
            OnAbilityUsed(ability);
        }

        public void NotifyOnSkillListChanged(Skill skill) {
            OnSkillListChanged(skill);
            //OnPrerequisiteUpdated();
        }

        public void NotifyOnItemCountChanged(Item item) {
            OnItemCountChanged(item);
        }

    }

    [System.Serializable]
    public class CustomParam {
        public EventParam eventParams = new EventParam();
        public ObjectConfigurationNode objectParam = new ObjectConfigurationNode();
    }

    [System.Serializable]
    public class EventParam {
        public string StringParam = string.Empty;
        public int IntParam = 0;
        public float FloatParam = 0f;
        public bool BoolParam = false;
    }

    [System.Serializable]
    public class EventParamProperties {
        public EventParam simpleParams = new EventParam();
        public ObjectConfigurationNode objectParam = new ObjectConfigurationNode();
    }

}