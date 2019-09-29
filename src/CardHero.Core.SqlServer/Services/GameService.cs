﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CardHero.Core.Abstractions;
using CardHero.Core.Models;
using CardHero.Core.SqlServer.EntityFramework;
using CardHero.Data.Abstractions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CardHero.Core.SqlServer.Services
{
    public class GameService : BaseService, IGameService
    {
        private readonly IGameValidator _gameValidator;
        private readonly IDeckRepository _deckRepository;
        private readonly IGameDeckRepository _gameDeckRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IGameUserRepository _gameUserRepository;
        private readonly IDataMapper<GameData, GameModel> _gameMapper;
        private readonly IDataMapper<GameCreateData, GameCreateModel> _gameCreateMapper;
        private readonly IDataMapper<GameUserData, GameUserModel> _gameUserMapper;

        public GameService(
            IDesignTimeDbContextFactory<CardHeroDbContext> contextFactory,
            IGameValidator gameValidator,
            IDeckRepository deckRepository,
            IGameDeckRepository gameDeckRepository,
            IGameRepository gameRepository,
            IGameUserRepository gameUserRepository,
            IDataMapper<GameData, GameModel> gameMapper,
            IDataMapper<GameCreateData, GameCreateModel> gameCreateMapper,
            IDataMapper<GameUserData, GameUserModel> gameUserMapper
        )
            : base(contextFactory)
        {
            _gameValidator = gameValidator;
            _deckRepository = deckRepository;
            _gameDeckRepository = gameDeckRepository;
            _gameRepository = gameRepository;
            _gameUserRepository = gameUserRepository;
            _gameMapper = gameMapper;
            _gameCreateMapper = gameCreateMapper;
            _gameUserMapper = gameUserMapper;
        }

        private async Task<GameUserModel> AddUserToGameInternalAsync(int id, int userId, int deckId, CancellationToken cancellationToken = default)
        {
            var game = await _gameRepository.GetGameByIdAsync(id);

            if (game == null)
            {
                throw new InvalidGameException($"Game { id } does not exist.");
            }

            var gameUsers = await _gameRepository.GetGameUsersAsync(id, cancellationToken: cancellationToken);

            if (gameUsers.Any(x => x.UserId == userId))
            {
                throw new InvalidPlayerException($"User { userId } is already in game { id }.");
            }

            var gul = gameUsers.Length;
            if (gul >= game.MaxPlayers)
            {
                throw new InvalidPlayerException($"Game { id } is already filled.");
            }

            var deck = await _deckRepository.GetDeckByIdAsync(deckId, cancellationToken: cancellationToken);

            if (deck == null)
            {
                throw new InvalidDeckException($"Deck { deckId } does not exist.");
            }

            if (deck.UserId != userId)
            {
                throw new InvalidDeckException($"Deck { deckId } does not belong to user { userId }.");
            }

            var newGameUser = await _gameUserRepository.AddGameUserAsync(id, userId, cancellationToken: cancellationToken);

            var dc = deck.Cards.Select(x => x.CardId).ToArray();
            var dcc = dc.Count();
            if (dcc < deck.MaxCards)
            {
                throw new InvalidDeckException($"Deck { deckId } needs { deck.MaxCards } cards. Currently only has { dcc }.");
            }

            var newGameDeck = await _gameDeckRepository.AddGameDeckAsync(newGameUser.Id, deck.Name, deck.Description, dc, cancellationToken: cancellationToken);

            if (gul + 1 == game.MaxPlayers)
            {
                var allUsers = gameUsers.Select(x => x.Id).Concat(new int[] { newGameUser.Id }).ToArray();
                var currentUserId = new Random().Next(0, allUsers.Length);
                var updateGame = new GameUpdateData
                {
                    CurrentGameUserId = allUsers[currentUserId],
                };

                await _gameRepository.UpdateGameAsync(id, updateGame, cancellationToken: cancellationToken);
            }

            return _gameUserMapper.Map(newGameUser);
        }

        private async Task<Abstractions.SearchResult<GameModel>> GetGamesInternalAsync(Abstractions.GameSearchFilter filter, int? userId = null, CancellationToken cancellationToken = default)
        {
            var result = await _gameRepository.FindGamesAsync(
                new Data.Abstractions.GameSearchFilter
                {
                    GameId = filter.GameId,
                    Type = (Data.Abstractions.GameType?)(int?)filter.Type,
                },
                cancellationToken: cancellationToken
            );

            var results = new Abstractions.SearchResult<GameModel>
            {
                Count = result.TotalCount,
                Results = result.Results.Select(_gameMapper.Map).ToArray(),
            };

            if (userId.HasValue)
            {
                var uid = userId.Value;

                foreach (var game in results.Results)
                {
                    //TODO: Fix loop to no make multiple calls
                    await PopulateGameUsersAsync(game, uid, cancellationToken: cancellationToken);
                }
            }

            return results;
        }

        private async Task PopulateGameUsersAsync(GameModel game, int userId, CancellationToken cancellationToken = default)
        {
            var users = await _gameRepository.GetGameUsersAsync(game.Id, cancellationToken: cancellationToken);
            var userIds = users.Select(x => x.UserId).ToArray();
            game.CanJoin = !game.EndTime.HasValue && userIds.Count() < game.MaxUsers && !userIds.Contains(userId);
            game.CanPlay = !game.EndTime.HasValue && userIds.Contains(userId) && game.CurrentUser.Id == userId;

            game.Users = users.Select(x => _gameUserMapper.Map(x)).ToArray();
        }

        Task<GameUserModel> IGameService.AddUserToGameAsync(int id, int userId, int deckId, CancellationToken cancellationToken)
        {
            return AddUserToGameInternalAsync(id, userId, deckId, cancellationToken: cancellationToken);
        }

        async Task<GameModel> IGameService.CreateGameAsync(GameCreateModel game, CancellationToken cancellationToken)
        {
            await _gameValidator.ValidateNewGameAsync(game, cancellationToken: cancellationToken);

            var newGameCreate = _gameCreateMapper.Map(game);

            var newGame = await _gameRepository.AddGameAsync(newGameCreate, cancellationToken: cancellationToken);

            if (game.Users != null)
            {
                foreach (var user in game.Users)
                {
                    await AddUserToGameInternalAsync(newGame.Id, user.Id, game.DeckId, cancellationToken: cancellationToken);
                }
            }

            return _gameMapper.Map(newGame);
        }

        async Task<GameModel> IGameService.GetGameByIdAsync(int id, int? userId, CancellationToken cancellationToken)
        {
            var filter = new Abstractions.GameSearchFilter
            {
                GameId = id,
            };
            var game = (await GetGamesInternalAsync(filter, userId: userId, cancellationToken: cancellationToken)).Results.SingleOrDefault();

            if (game == null)
            {
                throw new InvalidGameException($"Game { id } does not exist.");
            }

            if (userId.HasValue)
            {
                await PopulateGameUsersAsync(game, userId.Value, cancellationToken: cancellationToken);

                if (game.Users.Any(x => x.UserId == userId.Value))
                {
                    //var deck = await _gameDeckRepository.GetGameDeckCardCollectionAsync(0, cancellationToken: cancellationToken);
                    game.Deck = new DeckModel
                    {
                    };
                }
            }

            return game;
        }

        Task<Abstractions.SearchResult<GameModel>> IGameService.GetGamesAsync(Abstractions.GameSearchFilter filter)
        {
            var result = new Abstractions.SearchResult<GameModel>();

            var context = GetContext();

            var query = context.Game
                .Include(x => x.CurrentUserFkNavigation)
                .Include(x => x.DeckFkNavigation)
                    .ThenInclude(x => x.DeckCardCollection)
                        .ThenInclude(x => x.CardCollectionFkNavigation)
                            .ThenInclude(x => x.CardFkNavigation)
                .Include(x => x.GameUser)
                    .ThenInclude(x => x.GameFkNavigation)
                .Include(x => x.GameUser)
                    .ThenInclude(x => x.UserFkNavigation)
                .Include(x => x.WinnerFkNavigation)
                .AsQueryable();

            if (filter.GameId.HasValue)
            {
                query = query.Where(x => x.GamePk == filter.GameId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(x => x.Name.Contains(filter.Name));
            }

            if (filter.StartTime.HasValue)
            {
                query = query.Where(x => x.StartTime >= filter.StartTime.Value);
            }

            if (filter.EndTime.HasValue)
            {
                query = query.Where(x => x.EndTime <= filter.EndTime.Value);
            }

            if (filter.ActiveOnly)
            {
                query = query.Where(x => !x.EndTime.HasValue);
            }

            if (filter.Type.HasValue)
            {
                query = query.Where(x => x.GameTypeFk == (int)filter.Type.Value);
            }

            return PaginateAndSortAsync(query, filter, x => x.ToCore());
        }

        Task<Abstractions.SearchResult<GameModel>> IGameService.NewGetGamesAsync(Abstractions.GameSearchFilter filter, int? userId, CancellationToken cancellationToken)
        {
            return GetGamesInternalAsync(filter, userId: userId, cancellationToken: cancellationToken);
        }
    }
}
