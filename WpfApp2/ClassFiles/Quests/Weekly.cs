using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using L2RBot.Common;
using L2RBot.Common.Enum;

namespace L2RBot
{
    public class Weekly : Quest
    {
        //globals
        private Pixel[] _weeklyComplete;
        private bool _isWeeklyStarted;

        public Pixel[] BtnWeeklyComplete => _weeklyComplete ?? (_weeklyComplete = new Pixel[2]);
        public Pixel BtnGo { get; private set; }
        public Pixel BtnDone { get; private set; }

        //constructors
        public Weekly(Process app, L2RDevice adbApp) : base(app, adbApp)
        {
            Helper = new QuestHelper(app, adbApp)
            {
                Quest = QuestType.Weekly,
                Deathcount = Deathcount,
                Respawn = Respawn,
                CloseTVPopup = CloseTVPopup
            };
            Timer.Start();
            IdleTimeInMs = 60000;
            DefinePixel();

            _isWeeklyStarted = false;
        }

        /// <summary>
        /// Builds the collection of quest complete pixels.
        /// </summary>
        private void DefinePixel()
        {
            BtnGo = new Pixel
            {
                Color = Color.White,
                Point = new Point(675, 303) // white text on 'Go' button.
            };
            BtnDone = new Pixel
            {
                Color = Color.White,
                Point = new Point(752, 312) // white text on 'Go' button.
            };

            //All Weekly Quests complete
            BtnWeeklyComplete[0] = new Pixel
            {
                Color = Color.FromArgb(255, 47, 41, 37),

                Point = new Point(625, 310) // Background color of 'Quest Complete' button.
            };
            BtnWeeklyComplete[1] = new Pixel
            {
                Color = Color.FromArgb(255, 19, 22, 29),

                Point = new Point(752, 310) //Text color of 'Quest Complete' button.
            };
        }

        //logic
        public void Start()
        {
            UpdateScreen();

            if (BringToFront)
            {
                BringWindowToFront();
            }

            Sleep(); //Sleep before to prevent actions from occuring to early.

            if (_isWeeklyStarted == false)
            {
                OpenQuestPage();
            }

            IdleCheck();

            CheckCompletion();

            Helper.Start();

            IsHelperComplete();

            Sleep(); //Sleep after to prevent actions from occuring on the next active window.
        }

        private void OpenQuestPage()
        {
            if (OnCombatScreen() == false) return;

            MainWindow.main.UpdateLog = "Open Weekly Quests pane.";

            Thread.Sleep(TimeSpan.FromSeconds(1));
            Click(Nav.QuestMenu);

            Thread.Sleep(TimeSpan.FromSeconds(2));
            Click(Nav.BtnWeekly);

            if (IsWeeklyComplete()) return;
            Thread.Sleep(TimeSpan.FromSeconds(1));
            Click(Nav.BtnWeeklyAction);

            if (IsQuestDoneButtonExist())
            {
                MainWindow.main.UpdateLog = "Current quest finished.";
                if (IsWeeklyComplete())
                {
                    MainWindow.main.UpdateLog = "Weekly quests finished.";
                    CheckCompletion();
                }
                else
                {
                    MainWindow.main.UpdateLog = "Continuing with next weekly quest.";
                    Thread.Sleep(TimeSpan.FromSeconds(3));
                    Click(Nav.BtnWeeklyAction);
                }
            }
            else if (IsQuestAccepted() == false)
            {
                MainWindow.main.UpdateLog = "Weekly quest is not started yet, starting...";
                Thread.Sleep(TimeSpan.FromSeconds(1));
                Click(Nav.BtnWeeklyAction);
            }

            Thread.Sleep(TimeSpan.FromSeconds(2));
            Click(Nav.BtnWalk);

            Thread.Sleep(TimeSpan.FromSeconds(1));

            _isWeeklyStarted = true;
        }

        private bool IsQuestAccepted() => BtnGo.IsPresent(Screen, 4);
        private bool IsQuestDoneButtonExist() => BtnDone.IsPresent(Screen, 4);

        private void IdleCheck()
        {
            MainWindow.main.UpdateLog = "Start idle check.";
            MainWindow.main.UpdateLog = "Time elapsed: " + Timer.ElapsedMilliseconds.ToString();
            if (Timer.ElapsedMilliseconds > IdleTimeInMs && OnCombatScreen())
            {
                ResetTimer();
                StartTimer();

                while (OnCombatScreen() == false)
                {
                    Helper.Start();

                    if (Timer.ElapsedMilliseconds <= 300000) continue;
                    MainWindow.main.UpdateLog =
                        BotName + " has ended 'Weekly Quest' due to an unknown pop-up being detected.";
                    Complete = true;
                    break;
                }

                if (OnCombatScreen() == false) return;
                if (IsQuestDone())
                {
                    MainWindow.main.UpdateLog = "Current quest is done.";
                    OpenQuestPage();
                    /*
                    Click(Nav.AutoCombat);

                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    Click(Nav.AutoCombat);

                    Thread.Sleep(50);
                    Click(_weeklySearch.Point);
                    */
                }
                else
                {
                    _isWeeklyStarted = false;
                }
            }
            else
            {
                MainWindow.main.UpdateLog = "Sleeping for 1 minutes.";
                Thread.Sleep(TimeSpan.FromMinutes(1));
            }
        }

        private bool IsQuestDone()
        {
            Pixel temp = L2RBot.Screen.SearchPixelVerticalStride(Screen, new Point(8, 170), 140, Colors.WeeklyQuest,
                out bool found, 2);
            return found;
        }

        private bool IsWeeklyComplete()
        {
            MainWindow.main.UpdateLog = "Is Quest Complete?";
            return BtnWeeklyComplete[0].IsPresent(Screen, 2) &&
                   BtnWeeklyComplete[1].IsPresent(Screen, 2);
        }

        private void CheckCompletion()
        {
            MainWindow.main.UpdateLog = "Checking whether Weekly Quests is complete.";
            if (!IsWeeklyComplete()) return;
            Complete = true;
            MainWindow.main.UpdateLog = BotName + " has completed the 'Weekly Quests'";
        }
    }
}