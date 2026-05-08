using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using CardMatch.Logic.Messages;
using CardMatch.Logic.Models;
using CardMatch.Logic.Systems;
using CardMatch.Runtime.Views;

namespace CardMatch.Runtime.LifetimeScopes
{
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private LifetimeScope _parentScope;

        protected override LifetimeScope Parent => _parentScope;

        protected override void Configure(IContainerBuilder builder)
        {
            RegisterMessageBrokers(builder);
            RegisterModels(builder);
            RegisterSystems(builder);
            RegisterViews(builder);
        }

        private static void RegisterMessageBrokers(IContainerBuilder builder)
        {
            var options = builder.RegisterMessagePipe();
            builder.RegisterMessageBroker<CardFlippedMessage>(options);
            builder.RegisterMessageBroker<MatchResultMessage>(options);
            builder.RegisterMessageBroker<PenaltyAppliedMessage>(options);
            builder.RegisterMessageBroker<GamePhaseChangedMessage>(options);
            builder.RegisterMessageBroker<GameWonMessage>(options);
            builder.RegisterMessageBroker<SettingsChangedMessage>(options);
            builder.RegisterMessageBroker<ResetRequestedMessage>(options);
        }

        private static void RegisterModels(IContainerBuilder builder)
        {
            builder.Register<GameStateModel>(Lifetime.Singleton);
            builder.Register<GridModel>(Lifetime.Singleton);

            builder.Register<CardModel[]>(resolver =>
            {
                var cards = new CardModel[16];
                for (int cardIndex = 0; cardIndex < 16; cardIndex++)
                {
                    cards[cardIndex] = new CardModel { GridIndex = cardIndex };
                }
                return cards;
            }, Lifetime.Singleton);
        }

        private static void RegisterSystems(IContainerBuilder builder)
        {
            builder.Register<CardSystem>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<GridSystem>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<MatchSystem>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<GameFlowSystem>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        }

        private void RegisterViews(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<GridView>();
            builder.RegisterComponentInHierarchy<DeckView>();
            builder.RegisterComponentInHierarchy<HUDView>();
            builder.RegisterComponentInHierarchy<SettingsPanelView>();
            builder.RegisterComponentInHierarchy<WinPanelView>();
            builder.RegisterComponentInHierarchy<ResetConfirmPopupView>();

            builder.RegisterBuildCallback(resolver =>
            {
                var gridView = resolver.Resolve<GridView>();
                var cardViews = gridView.GetCardViews();
                for (int cardIndex = 0; cardIndex < cardViews.Length; cardIndex++)
                {
                    resolver.InjectGameObject(cardViews[cardIndex].gameObject);
                }
            });
        }
    }
}
