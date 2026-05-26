namespace AnyRPG {
    public class DescribableCraftingOutputIcon : DescribableCraftingIcon {

        public override void UpdateVisual() {
            //Debug.Log("DescribableCraftingOutputIcon.UpdateVisual()");
            base.UpdateVisual();

            if (count > 1) {
                stackSize.text = count.ToString();
            } else {
                stackSize.text = "";
            }

        }

    }

}