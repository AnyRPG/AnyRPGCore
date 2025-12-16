using System;
using System.Collections.Generic;

namespace AnyRPG {
    
    [Serializable]
    public class MailMessageListResponse {
        public List<MailMessage> MailMessages = new List<MailMessage>();
        public ItemInstanceListSaveData ItemInstanceListSaveData = new ItemInstanceListSaveData();

        public MailMessageListResponse() { }

        public void BundleItems(SystemItemManager systemItemManager) {
            foreach (MailMessage mailMessage in MailMessages) {
                foreach (MailAttachmentSlot mailAttachmentSlot in mailMessage.AttachmentSlots) {
                    foreach (int itemInstanceId in mailAttachmentSlot.ItemIds) {
                        InstantiatedItem instantiatedItem = systemItemManager.GetExistingInstantiatedItem(itemInstanceId);
                        ItemInstanceListSaveData.ItemInstances.Add(instantiatedItem.GetItemSaveData());
                    }
                }
            }
        }

    }
}