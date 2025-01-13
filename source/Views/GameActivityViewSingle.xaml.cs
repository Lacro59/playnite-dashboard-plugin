﻿using CommonPluginsControls.Controls;
using CommonPlayniteShared.Converters;
using CommonPluginsShared;
using CommonPluginsShared.Converters;
using GameActivity.Controls;
using GameActivity.Models;
using GameActivity.Services;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Controls.Primitives;

namespace GameActivity.Views
{
    /// <summary>
    /// Logique d'interaction pour GameActivityViewSingle.xaml
    /// </summary>
    public partial class GameActivityViewSingle : UserControl
    {
        private static ILogger Logger => LogManager.GetLogger();

        private ActivityDatabase PluginDatabase => GameActivity.PluginDatabase;

        private PluginChartTime PART_ChartTime { get; set; }
        private PluginChartLog PART_ChartLog { get; set; }

        private GameActivities gameActivities { get; set; }
        private Game GameContext { get; set; }
        private GameActivity Plugin { get; set; }


        public GameActivityViewSingle(GameActivity plugin, Game game)
        {
            GameContext = game;
            Plugin = plugin;

            InitializeComponent();

            ButtonShowConfig.IsChecked = false;


            // Cover
            if (!game.CoverImage.IsNullOrEmpty())
            {
                string coverImage = API.Instance.Database.GetFullFilePath(game.CoverImage);
                PART_ImageCover.Source = BitmapExtensions.BitmapFromFile(coverImage);
            }

            // Game sessions infos
            gameActivities = PluginDatabase.Get(game);

            PlayTimeToStringConverter longToTimePlayedConverter = new PlayTimeToStringConverter();
            PART_TimeAvg.Text = (string)longToTimePlayedConverter.Convert(gameActivities.AvgPlayTime(), null, null, CultureInfo.CurrentCulture);


            PART_RecentActivity.Text = gameActivities.GetRecentActivity();


            LocalDateConverter localDateConverter = new LocalDateConverter();
            PlayTimeToStringConverter converter = new PlayTimeToStringConverter();

            PART_FirstSession.Text = (string)localDateConverter.Convert(gameActivities.GetFirstSession(), null, null, CultureInfo.CurrentCulture);
            PART_FirstSessionElapsedTime.Text = (string)converter.Convert(gameActivities.GetFirstSessionactivity().ElapsedSeconds, null, null, CultureInfo.CurrentCulture);

            PART_LastSession.Text = (string)localDateConverter.Convert(gameActivities.GetLastSession(), null, null, CultureInfo.CurrentCulture);
            PART_LastSessionElapsedTime.Text = (string)converter.Convert(gameActivities.GetLastSessionActivity().ElapsedSeconds, null, null, CultureInfo.CurrentCulture);


            // Game session time line
            PART_ChartTime = (PluginChartTime)PART_ChartTimeContener.Children[0];
            PART_ChartTime.GameContext = game;
            PART_ChartTime.Truncate = PluginDatabase.PluginSettings.Settings.ChartTimeTruncate;
            PART_Truncate.IsChecked = PluginDatabase.PluginSettings.Settings.ChartTimeTruncate;


            lvSessions.SaveColumn = PluginDatabase.PluginSettings.Settings.SaveColumnOrder;
            lvSessions.SaveColumnFilePath = Path.Combine(PluginDatabase.Paths.PluginUserDataPath, "lvSessions.json");

            GridView lvView = (GridView)lvSessions.View;

            // Game logs
            // Add column if log details enable.
            if (!PluginDatabase.PluginSettings.Settings.EnableLogging)
            {
                lvAvgGpuP.Width = 0;
                lvAvgGpuPHeader.IsHitTestVisible = false;
                lvAvgCpuP.Width = 0;
                lvAvgCpuPHeader.IsHitTestVisible = false;
                lvAvgGpuT.Width = 0;
                lvAvgGpuTHeader.IsHitTestVisible = false;
                lvAvgCpuT.Width = 0;
                lvAvgCpuTHeader.IsHitTestVisible = false;
                lvAvgFps.Width = 0;
                lvAvgFpsHeader.IsHitTestVisible = false;
                lvAvgRam.Width = 0;
                lvAvgRamHeader.IsHitTestVisible = false;
                lvAvgGpu.Width = 0;
                lvAvgGpuHeader.IsHitTestVisible = false;
                lvAvgCpu.Width = 0;
                lvAvgCpuHeader.IsHitTestVisible = false;

                PART_BtLogContener.Visibility = Visibility.Collapsed;
                PART_ChartLogContener.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (!PluginDatabase.PluginSettings.Settings.lvAvgGpuP)
                {
                    lvAvgGpuP.Width = 0;
                    lvAvgGpuPHeader.IsHitTestVisible = false;
                }
                if (!PluginDatabase.PluginSettings.Settings.lvAvgCpuP)
                {
                    lvAvgCpuP.Width = 0;
                    lvAvgCpuPHeader.IsHitTestVisible = false;
                }
                if (!PluginDatabase.PluginSettings.Settings.lvAvgGpuT)
                {
                    lvAvgGpuT.Width = 0;
                    lvAvgGpuTHeader.IsHitTestVisible = false;
                }
                if (!PluginDatabase.PluginSettings.Settings.lvAvgCpuT)
                {
                    lvAvgCpuT.Width = 0;
                    lvAvgCpuTHeader.IsHitTestVisible = false;
                }
                if (!PluginDatabase.PluginSettings.Settings.lvAvgFps)
                {
                    lvAvgFps.Width = 0;
                    lvAvgFpsHeader.IsHitTestVisible = false;
                }
                if (!PluginDatabase.PluginSettings.Settings.lvAvgRam)
                {
                    lvAvgRam.Width = 0;
                    lvAvgRamHeader.IsHitTestVisible = false;
                }
                if (!PluginDatabase.PluginSettings.Settings.lvAvgGpu)
                {
                    lvAvgGpu.Width = 0;
                    lvAvgGpuHeader.IsHitTestVisible = false;
                }
                if (!PluginDatabase.PluginSettings.Settings.lvAvgCpu)
                {
                    lvAvgCpu.Width = 0;
                    lvAvgCpuHeader.IsHitTestVisible = false;
                }
            }

            if (!PluginDatabase.PluginSettings.Settings.lvGamesPcName)
            {
                lvGamesPcName.Width = 0;
                lvGamesPcNameHeader.IsHitTestVisible = false;
            }
            if (!PluginDatabase.PluginSettings.Settings.lvGamesSource)
            {
                lvGamesSource.Width = 0;
                lvGamesSourceHeader.IsHitTestVisible = false;
            }
            if (!PluginDatabase.PluginSettings.Settings.lvGamesPlayAction)
            {
                lvGamesPlayAction.Width = 0;
                lvGamesPlayActionHeader.IsHitTestVisible = false;
            }

            getActivityByListGame(gameActivities);

            PART_ChartLog = (PluginChartLog)PART_ChartLogContener.Children[0];
            PART_ChartLog.GameContext = game;

            DataContext = (
                GameDisplayName: game.Name,
                Settings: PluginDatabase.PluginSettings
            );
        }
        

        #region Time navigation 
        private void Bt_PrevTime(object sender, RoutedEventArgs e)
        {
            PART_ChartTime.Prev();
        }

        private void Bt_NextTime(object sender, RoutedEventArgs e)
        {
            PART_ChartTime.Next();
        }

        private void Bt_PrevTimePlus(object sender, RoutedEventArgs e)
        {
            PART_ChartTime.Prev(PluginDatabase.PluginSettings.Settings.VariatorTime);
        }

        private void Bt_NextTimePlus(object sender, RoutedEventArgs e)
        {
            PART_ChartTime.Next(PluginDatabase.PluginSettings.Settings.VariatorTime);
        }

        private void Bt_Truncate(object sender, RoutedEventArgs e)
        {
            PART_ChartTime.Truncate = (bool)((ToggleButton)sender).IsChecked;
            PART_ChartTime.AxisVariator = 0;
        }
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = sender as ToggleButton;
            PART_ChartTime.ShowByWeeks = (bool)tb.IsChecked;
            PART_ChartTime.AxisVariator = 0;
        }
        #endregion


        #region Log navigation
        private void LvSessions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvSessions.SelectedItem == null)
            {
                return;
            }

            string titleChart = "1";
            DateTime dateSelected = ((ListActivities)lvSessions.SelectedItem).GameLastActivity;
            
            PART_ChartLog.DateSelected = dateSelected;
            PART_ChartLog.TitleChart = titleChart;
            PART_ChartLog.AxisVariator = 0;


            int index = ((ListActivities)lvSessions.SelectedItem).PCConfigurationId;
            if (index != -1 && index < PluginDatabase.LocalSystem.GetConfigurations().Count)
            {
                var Configuration = PluginDatabase.LocalSystem.GetConfigurations()[index];

                PART_PcName.Content = Configuration.Name;
                PART_Os.Content = Configuration.Os;
                PART_CpuName.Content = Configuration.Cpu;
                PART_GpuName.Content = Configuration.GpuName;
                PART_Ram.Content = Configuration.RamUsage;
            }
            else
            {
                PART_PcName.Content = string.Empty;
                PART_Os.Content = string.Empty;
                PART_CpuName.Content = string.Empty;
                PART_GpuName.Content = string.Empty;
                PART_Ram.Content = string.Empty;
            }
        }

        private void Bt_PrevLog(object sender, RoutedEventArgs e)
        {
            PART_ChartLog.Prev();
        }

        private void Bt_NextLog(object sender, RoutedEventArgs e)
        {
            PART_ChartLog.Next();
        }

        private void Bt_PrevLogPlus(object sender, RoutedEventArgs e)
        {
            PART_ChartLog.Prev(PluginDatabase.PluginSettings.Settings.VariatorLog);
        }

        private void Bt_NextLogPlus(object sender, RoutedEventArgs e)
        {
            PART_ChartLog.Next(PluginDatabase.PluginSettings.Settings.VariatorLog);
        }
        #endregion


        public void getActivityByListGame(GameActivities gameActivities)
        {
            _ = Task.Run(() => 
            { 
                ObservableCollection<ListActivities> activityListByGame = new ObservableCollection<ListActivities>();

                for (int iItem = 0; iItem < gameActivities.FilterItems.Count; iItem++)
                {
                    try
                    {
                        ulong elapsedSeconds = gameActivities.FilterItems[iItem].ElapsedSeconds;
                        DateTime dateSession = Convert.ToDateTime(gameActivities.FilterItems[iItem].DateSession).ToLocalTime();
                        string sourceName = gameActivities.FilterItems[iItem].SourceName;
                        TextBlockWithIconMode ModeSimple = (PluginDatabase.PluginSettings.Settings.ModeStoreIcon == 1) ? TextBlockWithIconMode.IconTextFirstOnly : TextBlockWithIconMode.IconFirstOnly;

                        activityListByGame.Add(new ListActivities()
                        {
                            GameLastActivity = dateSession,
                            GameElapsedSeconds = elapsedSeconds,
                            AvgCPU = gameActivities.AvgCPU(dateSession.ToUniversalTime()) + "%",
                            AvgGPU = gameActivities.AvgGPU(dateSession.ToUniversalTime()) + "%",
                            AvgRAM = gameActivities.AvgRAM(dateSession.ToUniversalTime()) + "%",
                            AvgFPS = gameActivities.AvgFPS(dateSession.ToUniversalTime()) + "",
                            AvgCPUT = gameActivities.AvgCPUT(dateSession.ToUniversalTime()) + "°",
                            AvgGPUT = gameActivities.AvgGPUT(dateSession.ToUniversalTime()) + "°",
                            AvgCPUP = gameActivities.AvgCPUP(dateSession.ToUniversalTime()) + "W",
                            AvgGPUP = gameActivities.AvgGPUP(dateSession.ToUniversalTime()) + "W",

                            GameSourceName = sourceName,
                            TypeStoreIcon = ModeSimple,
                            SourceIcon = PlayniteTools.GetPlatformIcon(sourceName),
                            SourceIconText = TransformIcon.Get(sourceName),

                            EnableWarm = PluginDatabase.PluginSettings.Settings.EnableWarning,
                            MaxCPUT = PluginDatabase.PluginSettings.Settings.MaxCpuTemp.ToString(),
                            MaxGPUT = PluginDatabase.PluginSettings.Settings.MaxGpuTemp.ToString(),
                            MinFPS = PluginDatabase.PluginSettings.Settings.MinFps.ToString(),
                            MaxCPU = PluginDatabase.PluginSettings.Settings.MaxCpuUsage.ToString(),
                            MaxGPU = PluginDatabase.PluginSettings.Settings.MaxGpuUsage.ToString(),
                            MaxRAM = PluginDatabase.PluginSettings.Settings.MaxRamUsage.ToString(),

                            PCConfigurationId = gameActivities.FilterItems[iItem].IdConfiguration,
                            PCName = gameActivities.FilterItems[iItem].Configuration.Name,

                            GameActionName = gameActivities.FilterItems[iItem].GameActionName
                        });
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, $"Failed to load GameActivities for {gameActivities.Name}", true, PluginDatabase.PluginName);
                    }
                }

                this.Dispatcher.BeginInvoke((Action)delegate
                {
                    lvSessions.ItemsSource = activityListByGame;
                    lvSessions.Sorting();
                
                    if (((ObservableCollection<ListActivities>)lvSessions.ItemsSource).Count > 0)
                    {
                        lvSessions.SelectedItem = ((ObservableCollection<ListActivities>)lvSessions.ItemsSource).OrderByDescending(x => x.DateActivity).LastOrDefault();
                    }
                });
            });
        }


        #region Data actions
        private void PART_Delete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = API.Instance.Dialogs.ShowMessage(ResourceProvider.GetString("LOCConfirumationAskGeneric"), PluginDatabase.PluginName, MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    object GameLastActivity = ((FrameworkElement)sender).Tag;
                    ListActivities activity = ((ObservableCollection<ListActivities>)lvSessions.ItemsSource).FirstOrDefault(x => x.GameLastActivity == (DateTime)GameLastActivity);

                    // Delete playtime
                    if (activity.GameElapsedSeconds != 0)
                    {
                        if ((long)(GameContext.Playtime - activity.GameElapsedSeconds) >= 0)
                        {
                            GameContext.Playtime -= activity.GameElapsedSeconds;
                            if (GameContext.PlayCount != 0)
                            {
                                GameContext.PlayCount--;
                            }
                            else
                            {
                                Logger.Warn($"Play count is already at 0 for {GameContext.Name}");
                            }
                        }
                        else
                        {
                            Logger.Warn($"Impossible to remove GameElapsedSeconds ({activity.GameElapsedSeconds}) in Playtime ({GameContext.Playtime}) of {GameContext.Name}");
                        }
                    }

                    gameActivities.DeleteActivity(activity.GameLastActivity);

                    // Set last played date
                    GameContext.LastActivity = (DateTime)gameActivities.Items.Max(x => x.DateSession);

                    API.Instance.Database.Games.Update(GameContext);
                    PluginDatabase.Update(gameActivities);

                    lvSessions.SelectedIndex = -1;
                    _ = ((ObservableCollection<ListActivities>)lvSessions.ItemsSource).Remove(activity);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, PluginDatabase.PluginName);
                }
            }
        }

        private void PART_BtAdd_Click(object sender, RoutedEventArgs e)
        {
            WindowOptions windowOptions = new WindowOptions
            {
                ShowMinimizeButton = false,
                ShowMaximizeButton = false,
                ShowCloseButton = true
            };

            try
            {
                GameActivityAddTime ViewExtension = new GameActivityAddTime(Plugin, GameContext, null);
                Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCGaAddNewGameSession"), ViewExtension, windowOptions);
                _ = windowExtension.ShowDialog();

                if (ViewExtension.Activity != null)
                {
                    gameActivities.Items.Add(ViewExtension.Activity);
                    getActivityByListGame(gameActivities);

                    if (ViewExtension.Activity.ElapsedSeconds >= 0)
                    {
                        GameContext.Playtime += ViewExtension.Activity.ElapsedSeconds;
                        GameContext.PlayCount++;
                    }

                    // Set last played date
                    GameContext.LastActivity = (DateTime)gameActivities.Items.Max(x => x.DateSession);

                    API.Instance.Database.Games.Update(GameContext);
                    PluginDatabase.Update(gameActivities);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
            }
        }

        private void PART_BtEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                object GameLastActivity = ((FrameworkElement)sender).Tag;
                int index = gameActivities.Items.FindIndex(x => x.DateSession == ((DateTime)GameLastActivity).ToUniversalTime());
                Activity activity = gameActivities.Items[index];
                ulong ElapsedSeconds = activity.ElapsedSeconds;

                WindowOptions windowOptions = new WindowOptions
                {
                    ShowMinimizeButton = false,
                    ShowMaximizeButton = false,
                    ShowCloseButton = true
                };

                GameActivityAddTime ViewExtension = new GameActivityAddTime(Plugin, GameContext, activity);
                Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCGaAddNewGameSession"), ViewExtension, windowOptions);
                _ = windowExtension.ShowDialog();

                if (ViewExtension.Activity != null)
                {
                    gameActivities.Items[index] = ViewExtension.Activity;
                    getActivityByListGame(gameActivities);

                    if (ViewExtension.Activity.ElapsedSeconds >= 0)
                    {
                        GameContext.Playtime += ViewExtension.Activity.ElapsedSeconds - ElapsedSeconds;
                    }

                    // Set last played date
                    GameContext.LastActivity = (DateTime)gameActivities.Items.Max(x => x.DateSession);

                    API.Instance.Database.Games.Update(GameContext);
                    PluginDatabase.Update(gameActivities);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
            }
        }

        private void PART_BtMerged_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WindowOptions windowOptions = new WindowOptions
                {
                    ShowMinimizeButton = false,
                    ShowMaximizeButton = false,
                    ShowCloseButton = true
                };

                GameActivityMergeTime ViewExtension = new GameActivityMergeTime(GameContext);
                Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCGaMergeSession"), ViewExtension, windowOptions);
                _ = windowExtension.ShowDialog();
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
            }
        }
        #endregion
    }
}
