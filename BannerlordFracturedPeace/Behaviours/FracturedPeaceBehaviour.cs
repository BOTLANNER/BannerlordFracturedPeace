using System;
using System.Collections.Generic;
using System.Linq;

using BannerlordFracturedPeace.Actions;
using BannerlordFracturedPeace.UI;

using HarmonyLib;

using Helpers;

using SandBox.GauntletUI.Encyclopedia;
using SandBox.View.Map;

using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.BarterSystem.Barterables;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.GauntletUI.Data;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;

namespace BannerlordFracturedPeace.Behaviours
{
    public class FracturedPeaceBehaviour : CampaignBehaviorBase
    {
        public static FracturedPeaceBehaviour? Instance = null;

        public List<Kingdom>? KingdomToSplit = null;

        private EncyclopediaKingdomFractureVM? kingdomFractureVM;
        private EncyclopediaKingdomFractureAllVM? kingdomFractureAllVM;

        private EncyclopediaFactionPageVM? selectedKingdomPage = null;

        private Kingdom? selectedKingdom = null;

        private ScreenBase? gauntletLayerTopScreen;

        private GauntletLayer? gauntletLayer;

        private IGauntletMovie? gauntletMovie;

        Color Error = new(178 * 255, 34 * 255, 34 * 255);
        Color Warn = new(189 * 255, 38 * 255, 0);
        public FracturedPeaceBehaviour() : base()
        {
            Instance = this;
        }

        #region Overrides
        public override void RegisterEvents()
        {
            Game.Current.EventManager.RegisterEvent<EncyclopediaPageChangedEvent>((EncyclopediaPageChangedEvent e) => this.AddPlayAsLayer(e));
            CampaignEvents.TickEvent.AddNonSerializedListener(this, new Action<float>(this.Tick));
        }

        public override void SyncData(IDataStore dataStore)
        {
        }
        #endregion

        #region Event Handlers

        public void Tick(float delta)
        {
            if (Main.Settings != null && Main.Settings.AddSplitKingdom && this.KingdomToSplit != null && this.KingdomToSplit.Count > 0)
            {
                foreach (var currentKingdom in this.KingdomToSplit)
                {
                    try
                    {
                        InformationManager.HideInquiry();
                        SplitKingdom(currentKingdom);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteDebugLineOnScreen(e.ToString());
                    }
                }
                this.KingdomToSplit = null;
            }
        }

        private void SplitKingdom(Kingdom currentKingdom)
        {
            SplitKingdomAction.Apply(currentKingdom);
        }

        private void AddPlayAsLayer(EncyclopediaPageChangedEvent evt)
        {
            try
            {
                if (!(Main.Settings?.AddSplitKingdom ?? false))
                {
                    return;
                }

                EncyclopediaPages newPage = evt.NewPage;
                this.selectedKingdomPage = null;
                this.selectedKingdom = null;
                if (this.gauntletLayerTopScreen != null && this.gauntletLayer != null)
                {
                    this.gauntletLayerTopScreen.RemoveLayer(this.gauntletLayer);
                    if (this.gauntletMovie != null)
                    {
                        this.gauntletLayer.ReleaseMovie(this.gauntletMovie);
                    }
                    this.gauntletLayerTopScreen = null;
                    this.gauntletMovie = null;
                }
                if (newPage == EncyclopediaPages.Kingdom)
                {
                    if (MapScreen.Instance.EncyclopediaScreenManager is GauntletMapEncyclopediaView encyclopediaScreenManager)
                    {
                        if (AccessTools.Field(encyclopediaScreenManager.GetType(), "_encyclopediaData").GetValue(encyclopediaScreenManager) is not EncyclopediaData encyclopediaData)
                        {
                            return;
                        }
                        this.selectedKingdomPage = AccessTools.Field(encyclopediaData.GetType(), "_activeDatasource").GetValue(encyclopediaData) as EncyclopediaFactionPageVM;
                        if (this.selectedKingdomPage != null)
                        {
                            this.selectedKingdom = this.selectedKingdomPage.Obj as Kingdom;
                            if (this.selectedKingdom == null)
                            {
                                return;
                            }
                            this.gauntletLayer = new GauntletLayer(716, "GauntletLayer", false);
                            this.kingdomFractureVM = new EncyclopediaKingdomFractureVM(this.selectedKingdom, encyclopediaScreenManager);

                            this.gauntletMovie = this.gauntletLayer.LoadMovie("EncyclopediaKingdomFracture", this.kingdomFractureVM);
                            this.gauntletLayerTopScreen = ScreenManager.TopScreen;
                            this.gauntletLayerTopScreen.AddLayer(this.gauntletLayer);
                            this.gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.MouseButtons);
                        }
                    }
                }
                else if (newPage == EncyclopediaPages.ListKingdoms)
                {
                    if (MapScreen.Instance.EncyclopediaScreenManager is GauntletMapEncyclopediaView encyclopediaScreenManager)
                    {
                        if (AccessTools.Field(encyclopediaScreenManager.GetType(), "_encyclopediaData").GetValue(encyclopediaScreenManager) is not EncyclopediaData encyclopediaData)
                        {
                            return;
                        }
                        this.gauntletLayer = new GauntletLayer(716, "GauntletLayer", false);
                        this.kingdomFractureAllVM = new EncyclopediaKingdomFractureAllVM(encyclopediaScreenManager);

                        this.gauntletMovie = this.gauntletLayer.LoadMovie("EncyclopediaKingdomFracture", this.kingdomFractureAllVM);
                        this.gauntletLayerTopScreen = ScreenManager.TopScreen;
                        this.gauntletLayerTopScreen.AddLayer(this.gauntletLayer);
                        this.gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.MouseButtons);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.PrintError(e.Message, e.StackTrace);
                Debug.WriteDebugLineOnScreen(e.ToString());
                Debug.SetCrashReportCustomString(e.Message);
                Debug.SetCrashReportCustomStack(e.StackTrace);
                InformationManager.DisplayMessage(new InformationMessage(e.ToString(), Error));
            }
        }

        public void QueueSplit(params Kingdom[] kingdoms)
        {
            if (this.KingdomToSplit == null)
            {
                this.KingdomToSplit = new List<Kingdom>();
            }
            this.KingdomToSplit.AddRange(kingdoms.Where(k => !this.KingdomToSplit.Contains(k)));
        }

        #endregion
    }
}