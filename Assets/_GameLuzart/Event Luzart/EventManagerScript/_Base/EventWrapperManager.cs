namespace Luzart
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public partial class EventManager
    {
        #region Event
        private BattlePassManager _battlePassManager = null;
        public BattlePassManager battlePassManager
        {
            get
            {
                if (_battlePassManager == null)
                {
                    _battlePassManager = dictBaseEventManager[EEventName.BattlePass] as BattlePassManager;
                }
                return _battlePassManager;
            }
        }

        private TicketTallyManager _ticketTallyManager = null;
        public TicketTallyManager ticketTallyManager
        {
            get
            {
                if (_ticketTallyManager == null)
                {
                    _ticketTallyManager = dictBaseEventManager[EEventName.TicketTally] as TicketTallyManager;
                }
                return _ticketTallyManager;
            }
        }

        private JourneyToSuccessManager _journeyToSuccessManager = null;
        public JourneyToSuccessManager journeyToSuccessManager
        {
            get
            {
                if (_journeyToSuccessManager == null)
                {
                    _journeyToSuccessManager = dictBaseEventManager[EEventName.JourneyToSuccess] as JourneyToSuccessManager;
                }
                return _journeyToSuccessManager;
            }
        }

        private FlightEnduranceManager _flightEnduranceManager = null;
        public FlightEnduranceManager flightEnduranceManager
        {
            get
            {
                if (_flightEnduranceManager == null)
                {
                    _flightEnduranceManager = dictBaseEventManager[EEventName.FlightEndurance] as FlightEnduranceManager;
                }
                return _flightEnduranceManager;
            }
        }

        private RiseOfKittensManager _riseOfKittensManager = null;
        public RiseOfKittensManager riseOfKittensManager
        {
            get
            {
                if (_riseOfKittensManager == null)
                {
                    _riseOfKittensManager = dictBaseEventManager[EEventName.RiseOfKittens] as RiseOfKittensManager;
                }
                return _riseOfKittensManager;
            }
        }

        private CupManager _cupManager = null;
        public CupManager cupManager
        {
            get
            {
                if (_cupManager == null)
                {
                    _cupManager = dictBaseEventManager[EEventName.Cup] as CupManager;
                }
                return _cupManager;
            }
        }

        private DailyLoginManager _dailyLoginManager = null;
        public DailyLoginManager dailyLoginManager
        {
            get
            {
                if (_dailyLoginManager == null)
                {
                    _dailyLoginManager = dictBaseEventManager[EEventName.DailyLogin] as DailyLoginManager;
                }
                return _dailyLoginManager;
            }
        }

        private LuckySpinManager _luckySpinManager = null;
        public LuckySpinManager luckySpinManager
        {
            get
            {
                if (_luckySpinManager == null)
                {
                    _luckySpinManager = dictBaseEventManager[EEventName.LuckySpin] as LuckySpinManager;
                }
                return _luckySpinManager;
            }
        }

        private RacingManager _racingManager = null;
        public RacingManager racingManager
        {
            get
            {
                if(_racingManager == null)
                {
                    _racingManager = dictBaseEventManager[EEventName.Racing] as RacingManager;
                }
                return _racingManager;
            }
        }
        #endregion
        #region Funcion In Game
        public void OnStartLevel(int level)
        {
            int lengthBaseEvent = baseEventManagers.Count;
            for (int i = 0; i < lengthBaseEvent; i++)
            {
                var baseEvent = baseEventManagers[i];
                if (IsHasEvent(baseEvent.eEvent) && IsUnlockLevel(baseEvent.eEvent))
                {
                    baseEvent.OnStartGame(level);
                }
            }
        }

        public void CompleteLevelData(int level)
        {
            int lengthBaseEvent = baseEventManagers.Count;
            for (int i = 0; i < lengthBaseEvent; i++)
            {
                var baseEvent = baseEventManagers[i];
                if (IsHasEvent(baseEvent.eEvent) && IsUnlockLevel(baseEvent.eEvent))
                {
                    baseEvent.OnCompleteLevelData(level);
                }

            }
        }
        public void CompleteLevelVisual(int level)
        {
            int lengthBaseEvent = baseEventManagers.Count;
            for (int i = 0; i < lengthBaseEvent; i++)
            {
                var baseEvent = baseEventManagers[i];
                baseEvent.OnCompleteLevelToUnlock(level);
                if (IsHasEvent(baseEvent.eEvent) && IsUnlockLevel(baseEvent.eEvent))
                    baseEvent.OnCompleteLevelVisual(level);
            }
        }
        public void LoseLevelData(int level)
        {
            int lengthBaseEvent = baseEventManagers.Count;
            for (int i = 0; i < lengthBaseEvent; i++)
            {
                var baseEvent = baseEventManagers[i];
                baseEvent.OnLoseLevelData(level);
            }
        }
        public void LoseLevelVisual(int level)
        {
            int lengthBaseEvent = baseEventManagers.Count;
            for (int i = 0; i < lengthBaseEvent; i++)
            {
                var baseEvent = baseEventManagers[i];
                baseEvent.OnLoseLevelVisual(level);
            }
        }
        //public void OnSecondChance(Action onExcute, Action onQuit)
        //{
        //    var list = GetEEventCurrent();
        //    if (list.Count >= 4)
        //    {
        //        if (list.Contains(EEventName.FlightEndurance))
        //        {
        //            var flightEndurance = EventManager.Instance.flightEnduranceManager;
        //            if (!flightEndurance.dataFlightEndurance.isShowStart || (flightEndurance.dataFlightEndurance.isWin || flightEndurance.IsLoss))
        //            {
        //                list.Remove(EEventName.FlightEndurance);
        //            }
        //        }
        //        if (list.Contains(EEventName.JourneyToSuccess))
        //        {
        //            list.Remove(EEventName.JourneyToSuccess);
        //        }
        //    }
        //    var ui = UIManager.Instance.ShowUI<UIAreYouSureTimeOut>(UIName.AreYouSureTimeOut);
        //    ui.InitActionOnExcuteAndActionOnHide(onExcute, onQuit);
        //}
        //public void OnGiveUp(Action onExcute, Action onQuit)
        //{
        //    var list = GetEEventCurrent();
        //    if (list.Count >= 4)
        //    {
        //        if (list.Contains(EEventName.FlightEndurance))
        //        {
        //            var flightEndurance = EventManager.Instance.flightEnduranceManager;
        //            if (!flightEndurance.dataFlightEndurance.isShowStart || (flightEndurance.dataFlightEndurance.isWin || flightEndurance.IsLoss))
        //            {
        //                list.Remove(EEventName.FlightEndurance);
        //            }
        //        }
        //        if (list.Contains(EEventName.JourneyToSuccess))
        //        {
        //            list.Remove(EEventName.JourneyToSuccess);
        //        }
        //    }
        //    var ui = UIManager.Instance.ShowUI<UIAreYouSureGiveUp>(UIName.AreYouSureGiveUp);
        //    ui.InitActionOnExcuteAndActionOnHide(onExcute, onQuit);

        //}
        #endregion
        //
        //#region Wrapper

        private void StartFuncionInGame()
        {
            //EventDispatcher.Register<LevelStartEvent>(OnStartLevel);
            //EventDispatcher.Register<LevelCompleteEvent>(OnWinLevel);
            //EventDispatcher.Register<LevelLoseEvent>(OnLoseLevel);
            //EventDispatcher.Register<OutOfSpaceEvent>(OnOutOfSpace);
            //EventDispatcher.Register<LevelReplayEvent>(OnReplayLevel);
        }
        private void DestroyFuncionInGame()
        {
            //EventDispatcher.UnRegister<LevelStartEvent>(OnStartLevel);
            //EventDispatcher.UnRegister<LevelCompleteEvent>(OnWinLevel);
            //EventDispatcher.UnRegister<LevelLoseEvent>(OnLoseLevel);
            //EventDispatcher.UnRegister<OutOfSpaceEvent>(OnOutOfSpace);
            //EventDispatcher.UnRegister<LevelReplayEvent>(OnReplayLevel);

        }
        //private void OnStartLevel(LevelStartEvent data)
        //{
        //    OnStartLevel(data.CurrentLevel);
        //}
        //private void OnWinLevel(LevelCompleteEvent data)
        //{
        //    CompleteLevelData(data.CurrentLevel);
        //    CompleteLevelVisual(data.CurrentLevel);
        //}
        //private void OnLoseLevel(LevelLoseEvent data)
        //{
        //    LoseLevelData(data.CurrentLevel);
        //    LoseLevelVisual(data.CurrentLevel);
        //}
        //private void OnOutOfSpace(OutOfSpaceEvent data)
        //{

        //}
        //private void OnReplayLevel(LevelReplayEvent data)
        //{
        //    LoseLevelData(data.CurrentLevel);
        //    LoseLevelVisual(data.CurrentLevel);
        //}
        //#endregion

    }

}