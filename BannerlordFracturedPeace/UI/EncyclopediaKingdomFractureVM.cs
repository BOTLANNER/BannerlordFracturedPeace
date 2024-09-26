using System;

using BannerlordFracturedPeace.Behaviours;

using SandBox.GauntletUI.Encyclopedia;

using StoryMode;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BannerlordFracturedPeace.UI
{

    public class EncyclopediaKingdomFractureVM : ViewModel
    {
        private Kingdom current;
        private readonly GauntletMapEncyclopediaView encyclopediaManager;
        private bool _isSplitEnabled = true;

        private HintViewModel? _disableReasonHint;

        public EncyclopediaKingdomFractureVM(Kingdom kingdom, GauntletMapEncyclopediaView encyclopediaManager)
        {
            this.current = kingdom;
            this.encyclopediaManager = encyclopediaManager;
            CalculateEnabled();
        }


        [DataSourceProperty]
        public bool IsFractureAllowed
        {
            get
            {
                return this._isSplitEnabled;
            }
            set
            {
                if (value != this._isSplitEnabled)
                {
                    this._isSplitEnabled = value;
                    this?.OnPropertyChangedWithValue(value, "IsFractureAllowed");
                }
            }
        }

        [DataSourceProperty]
        public string FractureKingdomText
        {
            get
            {
                return new TextObject("{=fractured_peace_n_01}Split Kingdom").ToString();
            }
        }

        [DataSourceProperty]
        public HintViewModel? DisableHint
        {
            get
            {
                return this._disableReasonHint;
            }
            set
            {
                if (value != this._disableReasonHint)
                {
                    this._disableReasonHint = value;
                    this?.OnPropertyChangedWithValue(value, "DisableHint");
                }
            }
        }


        public override void RefreshValues()
        {
            base.RefreshValues();

            CalculateEnabled();
        }

        private void CalculateEnabled()
        {
            TextObject? disableReason = null;

            if (FracturedPeaceBehaviour.Instance?.KingdomToSplit?.Contains(this.current) ?? false)
            {
                disableReason ??= new TextObject("{=fractured_peace_n_02}Already queued to split this kingdom.");
            }

            if (Main.Settings != null && Main.Settings.AddSplitKingdom)
            {
                if (Game.Current == null || Campaign.Current == null || Hero.MainHero == null)
                {
                    disableReason ??= new TextObject("{=fractured_peace_n_04}Not in an active game!");
                }
            }
            else
            {
                disableReason = new TextObject("{=fractured_peace_n_03}Splitting kingdoms from Encyclopedia not enabled!");
            }

            if (disableReason == null)
            {
                disableReason = TextObject.Empty;
                IsFractureAllowed = true;
            }
            else
            {
                IsFractureAllowed = false;
            }


            if (!IsFractureAllowed)
            {
                this.DisableHint = new HintViewModel(disableReason, null);
            }
            else
            {
                this.DisableHint = null;
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
        }


        public void ExecuteFractureKindom()
        {
            if (this.current == null)
            {
                return;
            }

            var title = new TextObject("{=fractured_peace_n_01}Split Kingdom");
            var confirm = new TextObject("{=fractured_peace_n_05}Are you sure you want to split {KINGDOM}?");
            confirm.SetTextVariable("KINGDOM", current.Name);
            InformationManager.ShowInquiry(new InquiryData(title.ToString(), confirm.ToString(), true, true, GameTexts.FindText("str_ok", null).ToString(), GameTexts.FindText("str_cancel", null).ToString(),
                () =>
                {
                    if (FracturedPeaceBehaviour.Instance != null)
                    {
                        FracturedPeaceBehaviour.Instance.QueueSplit(this.current);

                        IsFractureAllowed = false;
                        RefreshValues();
                        if (encyclopediaManager != null)
                        {
                            try
                            {
                                encyclopediaManager.CloseEncyclopedia();
                            }
                            catch (Exception err)
                            {
                                // Ignore
                            }
                        }

                        while (!(Game.Current.GameStateManager.ActiveState is MapState))
                        {
                            Game.Current.GameStateManager.PopState();
                        }
                        Campaign.Current.TimeControlMode = CampaignTimeControlMode.UnstoppablePlay;
                    }
                },
                () =>
                {
                    // Cancelled. Do nothing.
                    InformationManager.HideInquiry();
                }), true, false);
        }
    }
}
