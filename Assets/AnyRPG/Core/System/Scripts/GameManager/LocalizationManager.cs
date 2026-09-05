using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace AnyRPG
{
    /// <summary>
    /// Central localization manager that acts as a thin facade for Unity Localization.
    /// Persistence and startup selection are handled natively by Unity Localization.
    /// </summary>
    public class LocalizationManager : ConfiguredClass
    {
        public event Action OnLocaleChanged = delegate { };

        /// <summary>
        /// The currently selected Locale.
        /// </summary>
        public Locale CurrentLocale
        {
            get => LocalizationSettings.SelectedLocale;
            set => LocalizationSettings.SelectedLocale = value;
        }

        public override void Configure(SystemGameManager systemGameManager)
        {
            base.Configure(systemGameManager);

            LocalizationSettings.SelectedLocaleChanged -= HandleSelectedLocaleChanged;
            LocalizationSettings.SelectedLocaleChanged += HandleSelectedLocaleChanged;
        }

        private void HandleSelectedLocaleChanged(Locale newLocale)
        {
            OnLocaleChanged();
        }
    }
}
