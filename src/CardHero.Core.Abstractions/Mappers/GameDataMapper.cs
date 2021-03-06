﻿using CardHero.Core.Models;
using CardHero.Data.Abstractions;

namespace CardHero.Core.Abstractions
{
    public class GameDataMapper : IDataMapper<GameData, GameModel>
    {
        GameModel IDataMapper<GameData, GameModel>.Map(GameData from)
        {
            return new GameModel
            {
                Columns = from.Columns,
                CurrentUserId = from.CurrentUserId,
                EndTime = from.EndTime,
                Id = from.Id,
                Rows = from.Rows,
                StartTime = from.StartTime,
                Type = (Models.GameType)(int)from.Type,
                WinnerUserId = from.WinnerUserId,
            };
        }

        GameData IDataMapper<GameData, GameModel>.Map(GameModel from)
        {
            return new GameData
            {
                Columns = from.Columns,
                EndTime = from.EndTime,
                Id = from.Id,
                Rows = from.Rows,
                StartTime = from.StartTime,
                Type = (Data.Abstractions.GameType)(int)from.Type,
                WinnerUserId = from.WinnerUserId,
            };
        }
    }
}
