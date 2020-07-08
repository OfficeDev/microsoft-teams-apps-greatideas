// <copyright file="no-post-added-page.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { Text, Button } from "@fluentui/react-northstar";
import { EyeIcon, CanvasAddPageIcon } from "@fluentui/react-icons-northstar";
import { WithTranslation, withTranslation } from "react-i18next";
import { TFunction } from "i18next";

import "../../styles/no-post-added-page.css";

interface INoIdeaAddedProps extends WithTranslation {
    showAddButton: boolean;
    onNewIdeaAdded: () => void;
    botId: any;
}

class TeamsConfigPage extends React.Component<INoIdeaAddedProps> {
    localize: TFunction;
    constructor(props: any) {
        super(props);
        this.localize = this.props.t;
    }

    /**
    *Navigate to submit idea task module.
    */
    handleAddClick = () => {
        let appBaseUrl = window.location.origin;
        microsoftTeams.tasks.startTask({
            completionBotId: this.props.botId,
            title: this.localize('submitIdeaTaskModuleHeaderText'),
            height: 720,
            width: 700,
            url: `${appBaseUrl}/submit-idea?telemetry=`,
            fallbackUrl: `${appBaseUrl}/submit-idea?telemetry=`,
        }, this.submitHandler);
    }

    /**
    * Submit idea task module handler.
    */
    submitHandler = async (err, result) => {
        this.props.onNewIdeaAdded();
        window.location.href = "/ideas";
    };

    /**
    * Renders the component
    */
    public render(): JSX.Element {
        return (
            <div className="no-post-added-container">
                <div className="app-logo">
                    <EyeIcon size="largest" />
                </div>
                <div className="add-new-post">
                    <Text content={this.localize("addNewPostNote")} />
                </div>
                {this.props.showAddButton && <div className="add-new-post-btn">
                    <Button icon={<CanvasAddPageIcon />} primary content={this.localize("addButtonText")} onClick={this.handleAddClick} />
                </div>}
            </div>
        )
    }
}

export default withTranslation()(TeamsConfigPage)