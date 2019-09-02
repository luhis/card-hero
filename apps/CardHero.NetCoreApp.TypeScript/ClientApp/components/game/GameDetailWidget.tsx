﻿import React, { Component } from "react";
import { Link } from "react-router-dom";
import GameModel from "../../models/GameModel";
import GameType from "../../models/GameType";
import DateFormat from "../shared/DateFormat";

interface IGameDetailWidgetProps {
    game: GameModel;
}

export default class GameDetailWidget extends Component<IGameDetailWidgetProps, any> {
    constructor(props) {
        super(props);
    }

    render() {
        const game = this.props.game;

        return (
            <div className="card">
                <h4 className="card-header">
                    {game ?
                        (<Link to={'/' + game.id}>{game.name}</Link>)
                        :
                        (<span>Unknown</span>)
                    }
                </h4>
                <div className="card-body">
                    <div className="card-text">
                        <div>
                            <strong>Type:</strong>
                            {' '}
                            {game ?
                                (<span>{GameType[game.type]}</span>)
                                :
                                (<span>Unknown</span>)
                            }
                        </div>
                        <div>
                            <strong>Start Time:</strong>
                            {' '}
                            {game ?
                                (<DateFormat date={game.startTime} />)
                                :
                                (<span>Unknown</span>)
                            }
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}