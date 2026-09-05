using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

namespace AnyRPG
{
    /// <summary>
    /// Language selection sub panel shown inside the settings menu.
    ///
    /// It lists the supported languages as buttons. The active language is highlighted.
    /// Choosing a language persists the selection through LocalizationManager and raises
    /// an event so all open windows can refresh their localized text immediately.
    ///
    /// To add a new language:
    ///  1. Add an entry to languageButtons below and assign its Locale asset.
    ///  2. Wire the corresponding HighlightButton in the prefab to this panel.
    /// </summary>
    public class SystemLanguagePanel : WindowPanel
    {
        [Header("Language Panel")]
        [SerializeField]
        private List<LanguageButton> languageButtons = new List<LanguageButton>();

        // game manager references
        private LocalizationManager localizationManager = null;

        public override void SetGameManagerReferences()
        {
            base.SetGameManagerReferences();

            localizationManager = systemGameManager.LocalizationManager;
        }

        public override void ProcessOpenWindowNotification()
        {
            base.ProcessOpenWindowNotification();

            RefreshLanguageButtons();
        }

        /// <summary>
        /// Called by a language button's OnClick() or programmatic selection.
        /// </summary>
        public void SelectLanguage(Locale locale)
        {
            if (localizationManager == null)
            {
                return;
            }

            localizationManager.CurrentLocale = locale;
            RefreshLanguageButtons();
        }

        /// <summary>
        /// Re-syncs the highlight state of all language buttons with the current locale.
        /// Selected language is highlighted; others are not.
        /// </summary>
        public void RefreshLanguageButtons()
        {
            if (languageButtons == null)
            {
                return;
            }

            Locale currentLocale = localizationManager?.CurrentLocale;

            foreach (LanguageButton langButton in languageButtons)
            {
                langButton?.UpdateHighlight(currentLocale);
            }
        }

        [Serializable]
        public class LanguageButton
        {
            [Tooltip("The Locale asset representing this language option.")]
            public Locale locale;

            [Tooltip("The HighlightButton in the prefab that represents this language.")]
            public HighlightButton button;

            public void UpdateHighlight(Locale activeLocale)
            {
                if (button == null)
                {
                    return;
                }

                bool isActive = (locale != null && locale == activeLocale);
                if (isActive)
                {
                    button.HighlightBackground();
                }
                else
                {
                    button.UnHighlightBackground();
                }
            }
        }
    }
}