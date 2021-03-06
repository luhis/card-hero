﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CardHero.Core.Abstractions;
using CardHero.Core.Models;
using CardHero.Data.Abstractions;

namespace CardHero.Core.SqlServer.Services
{
    public class StoreItemService : BaseService, IStoreItemService
    {
        private readonly ICardRepository _cardRepository;
        private readonly IStoreItemRepository _storeItemRepository;
        private readonly IUserRepository _userRepository;

        private readonly IDataMapper<CardData, CardModel> _cardDataMapper;
        private readonly IDataMapper<StoreItemData, StoreItemModel> _storeItemDataMapper;

        public StoreItemService(
            ICardRepository cardRepository,
            IStoreItemRepository storeItemRepository,
            IUserRepository userRepository,
            IDataMapper<CardData, CardModel> cardDataMapper,
            IDataMapper<StoreItemData, StoreItemModel> storeItemDataMapper
        )
        {
            _cardRepository = cardRepository;
            _storeItemRepository = storeItemRepository;
            _userRepository = userRepository;

            _cardDataMapper = cardDataMapper;
            _storeItemDataMapper = storeItemDataMapper;
        }

        async Task<Abstractions.SearchResult<StoreItemModel>> IStoreItemService.GetStoreItemsAsync(StoreItemSearchFilter filter, CancellationToken cancellationToken)
        {
            var result = await _storeItemRepository.FindStoreItemsAsync(cancellationToken: cancellationToken);

            var results = new Abstractions.SearchResult<StoreItemModel>
            {
                Count = result.Count,
                Results = result.Select(_storeItemDataMapper.Map).ToArray(),
            };

            return results;
        }

        async Task<IEnumerable<CardModel>> IStoreItemService.BuyStoreItemAsync(int id, int userId, CancellationToken cancellationToken)
        {
            var bundle = await _storeItemRepository.GetStoreItemById(id, cancellationToken: cancellationToken);

            if (bundle == null)
            {
                throw new InvalidStoreItemException($"Store item { id } not found in store.");
            }

            if (bundle.Expiry.HasValue && bundle.Expiry > DateTime.UtcNow)
            {
                throw new InvalidStoreItemException($"Store item { bundle.Name } has expired.");
            }

            var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken: cancellationToken);

            if (user == null)
            {
                throw new InvalidPlayerException($"Player { userId } does not exist.");
            }

            if (user.Coins < bundle.Cost)
            {
                throw new InvalidPlayerException($"Player { userId } does not have enough coins.");
            }

            var userUpdate = new UserUpdateData
            {
                Coins = user.Coins - bundle.Cost,
            };

            await _userRepository.UpdateUserAsync(userId, userUpdate, cancellationToken: cancellationToken);

            var cardResults = await _cardRepository.FindCardsAsync(
                new Data.Abstractions.CardSearchFilter
                {
                    CardPackId = bundle.CardPackId,
                    PageSize = int.MaxValue,
                },
                cancellationToken: cancellationToken
            );

            var allCards = cardResults
                .Results
                .SelectMany(x => Enumerable.Repeat(x, x.Rarity.Frequency)
                .Select(x => _cardDataMapper.Map(x)))
                .ToArray()
            ;

            var acl = allCards.Length;

            var ic = bundle.ItemCount;
            var cards = new CardModel[ic];
            var random = new Random();

            for (int i = 0; i < ic; i++)
            {
                cards[i] = allCards[random.Next(acl)];
            }

            return cards;
        }
    }
}
