// <copyright file="view-idea.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { WithTranslation, withTranslation } from "react-i18next";
import * as microsoftTeams from "@microsoft/teams-js";
import { Text, Flex, Provider, Label, Loader, Button } from "@fluentui/react-northstar";
import { TFunction } from "i18next";
import { IdeaEntity } from "../models/idea";
import UserAvatar from "../curator-team/user-avatar";
import { generateColor } from "../../helpers/helper";
import { getIdea, addUserVote } from "../../api/idea-api";
import { getApplicationInsightsInstance } from "../../helpers/app-insights";
import { createBrowserHistory } from "history";
import { UpvoteEntity } from "../models/idea";
import { SeverityLevel } from "@microsoft/applicationinsights-web";

interface IState {
    idea: IdeaEntity | undefined,
    loading: boolean,
    theme: string;
    submitLoading: boolean
}

const browserHistory = createBrowserHistory({ basename: "" });

class ViewIdea extends React.Component<WithTranslation, IState> {
    localize: TFunction;
    userObjectId: string = "";
    appInsights: any;
    telemetry: string | undefined = "";
    ideaId: string | undefined = "";
    createdById: string | undefined = "";

    constructor(props) {
        super(props);
        this.localize = this.props.t;
        this.state = {
            loading: true,
            idea: undefined,
            theme: "",
            submitLoading: false
        }

        let params = new URLSearchParams(window.location.search);
        this.telemetry = params.get("telemetry")!;
        this.ideaId = params.get("id")!;
        this.createdById = params.get("createdById")!;
    }



    public componentDidMount() {
        microsoftTeams.initialize();
        microsoftTeams.getContext((context) => {
            this.userObjectId = context.userObjectId!;
            this.setState({ theme: context.theme! });

            // Initialize application insights for logging events and errors.
            this.appInsights = getApplicationInsightsInstance(this.telemetry, browserHistory);
            this.getIdea();
        });
    };

    /**
    *Get idea details from API
    */
    async getIdea() {
        this.appInsights.trackTrace({ message: `'getIdea' - Initiated request`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        let idea = await getIdea(this.createdById!, this.ideaId!);
        if (idea.status === 200 && idea.data) {
            this.appInsights.trackTrace({ message: `'getIdea' - Request success`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
            this.setState(
                {
                    idea: idea.data,
                });
        }
        else {
            this.appInsights.trackTrace({ message: `'getIdea' - Request failed`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        }
        this.setState({
            loading: false
        });
    }

    /**
     * Submit vote.
     */
    handleConfirm = async () => {
        this.setState({ submitLoading: true });
        let vote: UpvoteEntity = {
            postId: this.ideaId,
            userId: this.userObjectId
        }
        let result = await addUserVote(vote);
        if (result.status === 200 && result.data) {
            this.appInsights.trackTrace({ message: `'addUserVote' - Request success`, properties: { User: this.userObjectId }, severityLevel: SeverityLevel.Information });
        }
        this.setState({ submitLoading: false });
    }

    /**
     * Renders the component.
     */
    public render(): JSX.Element {
        if (!this.state.loading) {
            return (
                <Provider>
                    <div className="module-container">
                        {this.state.idea && <Flex column gap="gap.small">
                            <Text size="largest" weight="bold" content={this.state.idea.title} />
                             <div className="upvote-count">
                                <Text size="largest" weight="bold" content={this.state.idea.totalVotes} /><br />
                                <Text content={this.localize("UpvotesText")} />
                            </div>
                            <Flex className="margin-subcontent" vAlign="center"><UserAvatar avatarColor={generateColor()}
                                showFullName={true} postType={this.state.idea.category!}
                                content={this.state.idea.createdByName!} title={this.state.idea.title!} />
                                &nbsp;<Text className="author-name" content={this.localize("ideaPostedOnText") + this.state.idea.createdDate} />
                             </Flex>
                            <Text content={this.localize("SynopsisText")} weight="bold" />
                            <Text content={this.state.idea.description} />
                            <Text content={this.localize("supportingDocumentsTitle")} weight="bold" />
                            <div className="documents-area">
                                {this.state.idea.documentLinks && JSON.parse(this.state.idea.documentLinks).map((document) => <Text className="title-text" content={document} onClick={() => window.open(document, "_blank")} />)}
                            </div>                     
                            <Flex>
                                <Flex.Item size="size.half">
                                    <Flex column gap="gap.small">
                                        <Text content="Tags: " weight="bold" />
                                        <div>
                                            {this.state.idea.tags ?.split(";") ?.map((tag, index) => <Label circular
                                                key={index} content={tag} />)}
                                        </div>
                                    </Flex>
                                </Flex.Item>
                                <Flex.Item size="size.half">
                                    <Flex column gap="gap.small">
                                        <Text weight="bold" content={this.localize("category")} />
                                        <Text content={this.state.idea.category} />
                                    </Flex>
                                </Flex.Item>
                            </Flex>

                        </Flex>}
                    </div>
                    <div className="tab-footer">
                        <Flex hAlign="end" ><Button primary content={this.localize("UpvoteButtonText")} loading={this.state.submitLoading} onClick={this.handleConfirm}/></Flex>
                    </div>
                </Provider>)
        }
        else {
            return <Loader />
        }
    }
}

export default withTranslation()(ViewIdea)