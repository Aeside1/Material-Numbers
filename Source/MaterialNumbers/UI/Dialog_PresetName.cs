using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace MaterialNumbers.UI
{
    internal sealed class Dialog_PresetName : Window
    {
        private readonly string title;
        private readonly Action<string> accepted;
        private string name;

        public Dialog_PresetName(string title, string initialName, Action<string> accepted)
        {
            this.title = title;
            name = initialName ?? string.Empty;
            this.accepted = accepted;
            doCloseX = true;
            absorbInputAroundWindow = true;
            forcePause = true;
        }

        public override Vector2 InitialSize => new Vector2(480f, 190f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), title);
            Text.Font = GameFont.Small;

            GUI.SetNextControlName("MaterialNumbersPresetName");
            name = Widgets.TextField(new Rect(0f, 48f, inRect.width, 32f), name);
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                Accept();
                Event.current.Use();
            }

            float buttonWidth = 130f;
            float buttonY = inRect.height - 40f;
            if (Widgets.ButtonText(new Rect(inRect.width - buttonWidth * 2f - 12f, buttonY, buttonWidth, 36f), "CancelButton".Translate()))
            {
                Close();
            }

            if (Widgets.ButtonText(new Rect(inRect.width - buttonWidth, buttonY, buttonWidth, 36f), "MaterialNumbers.Action.Save".Translate()))
            {
                Accept();
            }
        }

        public override void PostOpen()
        {
            base.PostOpen();
            GUI.FocusControl("MaterialNumbersPresetName");
        }

        private void Accept()
        {
            string trimmed = name.Trim();
            if (trimmed.Length == 0)
            {
                Messages.Message("MaterialNumbers.Message.NameRequired".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }

            accepted?.Invoke(trimmed);
            Close();
        }
    }
}
