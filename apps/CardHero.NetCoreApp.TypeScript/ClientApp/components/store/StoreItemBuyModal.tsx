﻿import React, { Component } from "react";
import { Modal } from "react-bootstrap";
import StoreItemModel from "../../models/StoreItemModel";

interface IStoreItemBuyModelProps {
    show: boolean;
    onHide?: () => void;
    storeItem?: StoreItemModel;
    onPurchase?: (item: StoreItemModel) => void;
}

export default class StoreItemBuyModal extends Component<IStoreItemBuyModelProps, any> {
    constructor(props: IStoreItemBuyModelProps) {
        super(props);
    }

    onHide() {
        if (this.props.onHide) {
            this.props.onHide();
        }
    }

    onPurchase() {
        if (this.props.onPurchase) {
            this.props.onPurchase(this.props.storeItem);
        }
    }

    render() {
        if (!this.props.storeItem) return null;

        return (
            <Modal
                {...this.props}
                centered
            >
                <Modal.Header closeButton>
                    <Modal.Title>
                        Purchase item
                    </Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <div className="row">
                        <div className="container">
                            Buy <strong>{this.props.storeItem.name}</strong>?
                        </div>
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <button type="button" className="btn btn-secondary" onClick={() => this.onHide()}>Cancel</button>
                    <button type="button" className="btn btn-success" onClick={() => this.onPurchase()}>Purchase</button>
                </Modal.Footer>
            </Modal>
        );
    }
}