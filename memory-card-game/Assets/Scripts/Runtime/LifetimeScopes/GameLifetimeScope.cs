using UnityEngine;
using VContainer;
using VContainer.Unity;
using MessagePipe;
using CardMatch.Logic.Models;
using CardMatch.Logic.Systems;
using CardMatch.Logic.Messages;
using CardMatch.Runtime.Views;
using CardMatch.Runtime.ScriptableObjects;
using CardMatch.Runtime.EntryPoints;
using CardMatch.Runtime.Services;
using CardMatch.Runtime.Coordinators;

namespace CardMatch.Runtime
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private CardDefinitions _cardDefinitions;

        protected override void Configure(IContainerBuilder builder)
        {
            var rootScope = LifetimeScope.Find<RootLifetimeScope>();
            builder.RegisterInstance(rootScope.Container.Resolve<SaveSystem>());
            builder.RegisterInstance(rootScope.Container.Resolve<AudioSystem>());
            builder.RegisterInstance(rootScope.Container.Resolve<AudioSettingsModel>());

            builder.RegisterInstance(_gameConfig);
            builder.RegisterInstance(_cardDefinitions);

            var cards = new CardModel[_gameConfig.CardCount];
            for (int cardIndex = 0; cardIndex < cards.Length; cardIndex++)
            {
                cards[cardIndex] = new CardModel { GridIndex = cardIndex };
            }
            builder.RegisterInstance(cards);

            builder.Register<GameStateModel>(Lifetime.Singleton);
            builder.Register<GridModel>(Lifetime.Singleton);

            builder.Register<CardSystem>(Lifetime.Singleton);
            builder.Register<GridSystem>(Lifetime.Singleton);
            builder.Register<MatchSystem>(Lifetime.Singleton);
            builder.Register<GameFlowSystem>(Lifetime.Singleton);

            var messagePipeOptions = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<CardFlippedMessage>(messagePipeOptions);
            builder.RegisterMessageBroker<MatchResultMessage>(messagePipeOptions);
            builder.RegisterMessageBroker<PenaltyAppliedMessage>(messagePipeOptions);
            builder.RegisterMessageBroker<GamePhaseChangedMessage>(messagePipeOptions);
            builder.RegisterMessageBroker<GameWonMessage>(messagePipeOptions);
            builder.RegisterMessageBroker<SettingsChangedMessage>(messagePipeOptions);
            builder.RegisterMessageBroker<ResetRequestedMessage>(messagePipeOptions);
            builder.RegisterMessageBroker<OpenSettingsRequestedMessage>(messagePipeOptions);
            builder.RegisterMessageBroker<CloseSettingsRequestedMessage>(messagePipeOptions);
            builder.RegisterMessageBroker<OpenResetConfirmRequestedMessage>(messagePipeOptions);
            builder.RegisterMessageBroker<ResetConfirmedMessage>(messagePipeOptions);
            builder.RegisterMessageBroker<NewGameRequestedMessage>(messagePipeOptions);

            builder.RegisterComponentInHierarchy<HUDView>();
            builder.RegisterComponentInHierarchy<GridView>();
            builder.RegisterComponentInHierarchy<DeckView>();
            builder.RegisterComponentInHierarchy<InputView>();
            builder.RegisterComponentInHierarchy<SettingsPanelView>();
            builder.RegisterComponentInHierarchy<WinPanelView>();
            builder.RegisterComponentInHierarchy<ResetConfirmPopupView>();

            builder.RegisterEntryPoint<GameEntryPoint>();
            builder.RegisterEntryPoint<GameCoordinator>();
            builder.RegisterEntryPoint<AudioBridge>();
        }
    }
}
