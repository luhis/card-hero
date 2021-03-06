﻿import { GameApiClient, GameCreateModel, GameJoinModel, IGameModel } from "../clients/clients";

interface IGameSearchFilter {
    name?: string;
    activeOnly?: boolean;
    page?: number;
    pageSize?: number;
    gameId?: number;
}

export class GameService {
    static async getGameById(id: number): Promise<IGameModel | null> {
        const client = new GameApiClient();
        const model = await client.getById(id);

        return model;
    }

    static async getGames(filter?: IGameSearchFilter): Promise<IGameModel[] | null> {
        const client = new GameApiClient();

        if (!filter) {
            filter = {};
        }

        const model = await client.get(
            filter.gameId,
            filter.name,
            undefined,
            undefined,
            undefined,
            filter.activeOnly,
            undefined,
            filter.page,
            filter.pageSize,
            undefined
        );

        return model;
    }

    static async createGame(model: GameCreateModel): Promise<IGameModel> {
        const client = new GameApiClient();

        const newModel = await client.post(model);

        return newModel;
    }

    static async join(id: number, deckId: number): Promise<void> {
        const client = new GameApiClient();

        const postModel = new GameJoinModel({
            deckId: deckId
        });
        await client.join(id, postModel);
    }
}
