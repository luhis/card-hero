﻿import React, { Fragment, useContext } from "react";
import { Link } from "react-router-dom";
import { AccountContext } from "../../contexts/AccountContext";
import AccountModel from "../../models/AccountModel";
import CardModel from "../../models/CardModel";
import Icon from "../../styles/index";

interface ICardListProps {
    cards: CardModel[];
    user?: AccountModel;
}

export default function CardList(props: ICardListProps) {
    const context = useContext(AccountContext);
    const user = context.user;

    return (
        <div className="row">
            <table className="table table-striped">
                <thead className="thead-inverse">
                    <tr>
                        <th>Name</th>
                        <th>Health</th>
                        <th>Attack</th>
                        <th>Defence</th>
                        <th>Up Attack</th>
                        <th>Down Attack</th>
                        <th>Left Attack</th>
                        <th>Right Attack</th>
                        <th>Total Stats</th>
                    </tr>
                </thead>

                <tbody>
                    {props.cards.map(c =>
                        <tr key={c.id}>
                            <th scope="row">
                                {user &&
                                    <Fragment>
                                        <Icon icon="star" className={'card-favourite' + (c.isFavourited ? ' enabled' : '')} data-card-id={c.id} />
                                        {' '}
                                    </Fragment>
                                }
                                <Link to={'/' + c.id}>{c.name}</Link>
                            </th>
                            <td>{c.health}</td>
                            <td>{c.attack}</td>
                            <td>{c.defence}</td>
                            <td>{c.upAttack}</td>
                            <td>{c.downAttack}</td>
                            <td>{c.leftAttack}</td>
                            <td>{c.rightAttack}</td>
                            <td>{c.totalStats}</td>
                        </tr>
                    )}
                </tbody>
            </table>
        </div>
    );
}