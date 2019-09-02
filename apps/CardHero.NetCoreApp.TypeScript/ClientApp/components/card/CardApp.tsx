﻿import React, { Component } from "react";
import Constants from "../../constants/constants";
import CardModel from "../../models/CardModel";
import Layout from "../shared/Layout";
import CardList from "./CardList";
import CardSearch from "./CardSearch";

interface ICardAppState {
    cards: CardModel[];
}

export default class CardApp extends Component<any, ICardAppState> {
    constructor(props: any) {
        super(props);

        this.state = { cards: [] };
    }

    onCardsPopulated(cards: CardModel[]) {
        if (Constants.Debug) {
            if (cards != null) {
                cards.forEach(card => {
                    console.log(card);
                });
            }
        }

        this.setState({
            cards: cards
        })
    }

    render() {
        return (
            <Layout
                sideContent={<CardSearch
                    onCardsPopulated={(x) => this.onCardsPopulated(x)} />
                }
            >
                <CardList cards={this.state.cards} />
            </Layout>
        );
    }
}