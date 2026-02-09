using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    
    [Serializable]
    public class MailMessageListBundle {
        public List<MailMessage> MailMessages = new List<MailMessage>();
        public ItemInstanceListSaveData ItemInstanceListSaveData = new ItemInstanceListSaveData();

        public MailMessageListBundle() { }

        public void BundleItems(SystemItemManager systemItemManager) {
            foreach (MailMessage mailMessage in MailMessages) {
                foreach (MailAttachmentSlot mailAttachmentSlot in mailMessage.AttachmentSlots) {
                    foreach (long itemInstanceId in mailAttachmentSlot.ItemInstanceIds) {
                        InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                        if (instantiatedItem == null) {
                            Debug.LogWarning($"MailMessageListBundle() Item with instanceId {itemInstanceId} not found!");
                            continue;
                        }
                        ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
                    }
                }
            }
        }

    }
}