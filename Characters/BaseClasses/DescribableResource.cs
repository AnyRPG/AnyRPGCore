using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Describable Resource", menuName = "Describable Resource")]
public abstract class DescribableResource : ScriptableObject, IDescribable {

    [SerializeField]
    protected string resourceName;

    [SerializeField]
    protected Sprite icon;

    [SerializeField]
    [TextArea(10, 20)]
    protected string description;

    public Sprite MyIcon { get => icon; set => icon = value; }
    public string MyName { get => resourceName; set => resourceName = value; }
    public string MyDescription { get => description; set => description = value; }

    public virtual string GetDescription() {
        return string.Format("<color=yellow>{0}</color>\n{1}", MyName, GetSummary());
    }

    public virtual string GetSummary() {
        return string.Format("{0}", description);
    }

}
