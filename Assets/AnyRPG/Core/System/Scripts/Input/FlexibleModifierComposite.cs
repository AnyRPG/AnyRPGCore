using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AnyRPG {
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class FlexibleModifierComposite : InputBindingComposite<float> {
        [InputControlAttribute(layout = "Button")] public int modifier1;
        [InputControlAttribute(layout = "Button")] public int modifier2;
        [InputControlAttribute(layout = "Button")] public int button;

        static FlexibleModifierComposite() {
            InputSystem.RegisterBindingComposite<FlexibleModifierComposite>("FlexibleModifier");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() { }

        public override float ReadValue(ref InputBindingCompositeContext context) {
            var keyboard = Keyboard.current;

            // 1. Check if Modifier 1 is assigned and pressed
            bool mod1IsBound = SlotHasControlMapped(ref context, modifier1);
            if (mod1IsBound && !context.ReadValueAsButton(modifier1)) return 0f;

            // 2. Check if Modifier 2 is assigned and pressed
            bool mod2IsBound = SlotHasControlMapped(ref context, modifier2);
            if (mod2IsBound && !context.ReadValueAsButton(modifier2)) return 0f;

            // 3. THE FIX: Conflict Resolution Logic
            // If this specific slot has NO modifiers assigned, but the player is actively
            // holding down Ctrl or Shift on their keyboard, this is a single key trying to steal
            // a macro input. We force it to return 0f so the macro action can fire instead.
            if (!mod1IsBound && !mod2IsBound && keyboard != null) {
                if (keyboard.ctrlKey.isPressed || keyboard.shiftKey.isPressed || keyboard.altKey.isPressed) {
                    return 0f;
                }
            }

            // 4. If all checks pass, read the primary action button
            return context.ReadValueAsButton(button) ? 1f : 0f;
        }

        private bool SlotHasControlMapped(ref InputBindingCompositeContext context, int partNumber) {
            var boundControls = context.controls;
            foreach (var partBinding in boundControls) {
                if (partBinding.part == partNumber) {
                    return true;
                }
            }
            return false;
        }

        public override float EvaluateMagnitude(ref InputBindingCompositeContext context) {
            return ReadValue(ref context);
        }
    }
}
