﻿// <copyright file="error-page.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Text } from "@fluentui/react-northstar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";

import "../styles/site.css";

interface IErrorPageProps extends WithTranslation {
    match: any;
}

class ErrorPage extends React.Component<IErrorPageProps, {}> {
    localize: TFunction;
    constructor(props: any) {
        super(props);

        this.localize = this.props.t;
    }

    /**
    * Renders the component
    */
    public render(): JSX.Element {

        const params = this.props.match.params;
        let message = this.localize("generalErrorMessage");

        if ("id" in params) {
            const id = params["id"];
            if (id === "401") {
                message = this.localize("unauthorizedErrorMessage");
            } else if (id === "403") {
                message = this.localize("forbiddenErrorMessage");
            }
            else {
                message = this.localize("generalErrorMessage");
            }
        }

        return (
            <div className="container-div">
                <div className="container-subdiv">
                    <div className="error-message">
                        <Text content={message} error size="medium" />
                    </div>
                </div>
            </div>
        );
    }
}

export default withTranslation()(ErrorPage)